import { useState, useEffect } from 'react';
import { Layout } from '../components/ui/Layout';
import { Spinner } from '../components/ui/Spinner';
import { Link } from 'react-router-dom';
import api from '../services/api';
import type { ReviewItemDto, DocumentDto } from '../types';

const QUALITY_LABELS = [
  { q: 0, label: 'Bloqueio total', color: 'bg-red-100 text-red-700 hover:bg-red-200' },
  { q: 1, label: 'Muito difícil',  color: 'bg-orange-100 text-orange-700 hover:bg-orange-200' },
  { q: 2, label: 'Difícil',       color: 'bg-amber-100 text-amber-700 hover:bg-amber-200' },
  { q: 3, label: 'Razoável',      color: 'bg-yellow-100 text-yellow-700 hover:bg-yellow-200' },
  { q: 4, label: 'Fácil',         color: 'bg-lime-100 text-lime-700 hover:bg-lime-200' },
  { q: 5, label: 'Muito fácil',   color: 'bg-green-100 text-green-700 hover:bg-green-200' },
];

export default function ReviewPage() {
  const [items, setItems] = useState<ReviewItemDto[]>([]);
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [selectedDocumentId, setSelectedDocumentId] = useState<string>('');
  
  const [index, setIndex] = useState(0);
  const [showAnswer, setShowAnswer] = useState(false);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    api.get<DocumentDto[]>('/documents')
      .then(res => setDocuments(res.data))
      .catch(() => {});
  }, []);

  useEffect(() => {
    setLoading(true);
    setIndex(0);
    setShowAnswer(false);
    
    const params = selectedDocumentId ? { documentId: selectedDocumentId } : {};
    
    api.get<ReviewItemDto[]>('/reviews/due', { params })
      .then(res => setItems(res.data))
      .finally(() => setLoading(false));
  }, [selectedDocumentId]);

  const handleRating = async (quality: number) => {
    const item = items[index];
    setSubmitting(true);
    try {
      await api.post('/reviews/submit', { progressId: item.progressId, quality });
    } catch { /* continue anyway */ }
    setShowAnswer(false);
    setIndex(i => i + 1);
    setSubmitting(false);
  };

  const progress = items.length > 0 ? Math.round((index / items.length) * 100) : 0;

  if (loading) return (
    <Layout>
      <div className="flex justify-center py-20"><Spinner size="lg" /></div>
    </Layout>
  );

  if (items.length === 0 && index === 0) return (
    <Layout>
      <div className="p-8 max-w-xl mx-auto animate-fade-in">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold text-neutral-900">Revisão</h1>
          {documents.length > 0 && (
            <select
              value={selectedDocumentId}
              onChange={(e) => setSelectedDocumentId(e.target.value)}
              className="text-sm rounded-lg border-neutral-300 focus:border-brand-500 focus:ring-brand-500"
            >
              <option value="">Todos os documentos</option>
              {documents.map(d => (
                <option key={d.id} value={d.id}>{d.title}</option>
              ))}
            </select>
          )}
        </div>
        <div className="card text-center p-8">
          <p className="text-5xl mb-4">🎉</p>
          <h2 className="text-xl font-bold text-neutral-900">Nenhum item pendente!</h2>
          <p className="text-neutral-500 mt-2">Você não tem itens para revisar agora{selectedDocumentId ? ' neste documento' : ''}.</p>
          <Link to="/documents" className="btn-primary mt-6 inline-flex">Ir para Documentos</Link>
        </div>
      </div>
    </Layout>
  );

  if (index >= items.length) return (
    <Layout>
      <div className="p-8 max-w-xl mx-auto animate-fade-in">
        <div className="card text-center p-8">
          <p className="text-5xl mb-4">🏆</p>
          <h2 className="text-xl font-bold text-neutral-900">Sessão concluída!</h2>
          <p className="text-neutral-500 mt-2">Você revisou {items.length} itens. Ótimo trabalho!</p>
          <Link to="/" className="btn-primary mt-6 inline-flex">Ver Dashboard</Link>
        </div>
      </div>
    </Layout>
  );

  const current = items[index];

  return (
    <Layout>
      <div className="p-8 max-w-xl mx-auto animate-fade-in">
        
        {/* Header & Filter */}
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold text-neutral-900">Revisão</h1>
          {documents.length > 0 && (
            <select
              value={selectedDocumentId}
              onChange={(e) => setSelectedDocumentId(e.target.value)}
              className="text-sm rounded-lg border-neutral-300 focus:border-brand-500 focus:ring-brand-500 max-w-[200px] truncate"
            >
              <option value="">Todos os documentos</option>
              {documents.map(d => (
                <option key={d.id} value={d.id}>{d.title}</option>
              ))}
            </select>
          )}
        </div>

        {/* Progress bar */}
        <div className="mb-6">
          <div className="flex justify-between text-sm text-neutral-500 mb-2">
            <span>Item {index + 1} de {items.length}</span>
            <span>{progress}% concluído</span>
          </div>
          <div className="w-full bg-neutral-100 rounded-full h-1.5">
            <div className="bg-brand-500 h-1.5 rounded-full transition-all duration-300" style={{ width: `${progress}%` }} />
          </div>
        </div>

        {/* Card */}
        <div className="card mb-4 min-h-48 relative">
          <div className="flex justify-between items-start mb-3">
            <span className="inline-flex items-center gap-1.5 py-1 px-2.5 rounded-md bg-brand-50 text-brand-700 text-xs font-medium border border-brand-100">
              {current.type}
              {current.topic && <span className="text-brand-500 font-normal"> {current.topic}</span>}
            </span>
            {current.documentTitle && (
              <span className="text-xs text-neutral-400 font-medium truncate max-w-[150px]" title={current.documentTitle}>
                📄 {current.documentTitle}
              </span>
            )}
          </div>
          <p className="text-lg text-neutral-800 leading-relaxed mt-2">{current.question}</p>
        </div>

        {!showAnswer ? (
          <button className="btn-primary w-full" onClick={() => setShowAnswer(true)}>
            Ver resposta
          </button>
        ) : (
          <div className="space-y-4 animate-slide-up">
            <div className="card border-brand-200 bg-brand-50">
              <p className="text-xs font-semibold uppercase tracking-widest text-brand-500 mb-2">Resposta</p>
              <p className="text-neutral-800 leading-relaxed">{current.answer}</p>
            </div>

            <div>
              <p className="text-sm font-medium text-neutral-700 mb-2 text-center">
                Como foi para você?
              </p>
              <div className="grid grid-cols-3 gap-2">
                {QUALITY_LABELS.map(({ q, label, color }) => (
                  <button
                    key={q}
                    className={`py-2 px-3 text-xs font-medium rounded-lg transition-all active:scale-95 ${color}`}
                    onClick={() => handleRating(q)}
                    disabled={submitting}
                  >
                    {label}
                  </button>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}