import { useState, useEffect } from 'react';
import { Layout } from '../components/ui/Layout';
import { Spinner } from '../components/ui/Spinner';
import { Link } from 'react-router-dom';
import api from '../services/api';
import type { ReviewItemDto, DocumentDto } from '../types';

const QUALITY_LABELS = [
  { q: 0, label: 'Bloqueio total', color: 'bg-red-100 text-red-700 hover:bg-red-200' },
  { q: 1, label: 'Muito dificil',  color: 'bg-orange-100 text-orange-700 hover:bg-orange-200' },
  { q: 2, label: 'Dificil',        color: 'bg-amber-100 text-amber-700 hover:bg-amber-200' },
  { q: 3, label: 'Razoavel',       color: 'bg-yellow-100 text-yellow-700 hover:bg-yellow-200' },
  { q: 4, label: 'Facil',          color: 'bg-lime-100 text-lime-700 hover:bg-lime-200' },
  { q: 5, label: 'Muito facil',    color: 'bg-green-100 text-green-700 hover:bg-green-200' },
];

function shuffleArray<T>(arr: T[]): T[] {
  const a = [...arr];
  for (let i = a.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [a[i], a[j]] = [a[j], a[i]];
  }
  return a;
}

export default function ReviewPage() {
  const [items, setItems] = useState<ReviewItemDto[]>([]);
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [selectedDocumentId, setSelectedDocumentId] = useState<string>('');
  const [index, setIndex] = useState(0);
  const [showAnswer, setShowAnswer] = useState(false);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [quizState, setQuizState] = useState<any>(null);

  useEffect(() => {
    api.get('/documents')
      .then(res => setDocuments(res.data))
      .catch(() => {});
  }, []);

  useEffect(() => {
    setLoading(true);
    setIndex(0);
    setShowAnswer(false);
    setQuizState(null);
    const params = selectedDocumentId ? { documentId: selectedDocumentId } : {};
    api.get('/reviews/due', { params })
      .then(res => setItems(res.data))
      .finally(() => setLoading(false));
  }, [selectedDocumentId]);

  const current = items[index];

  useEffect(() => {
    if (!current || current.type !== 'Quiz') {
      setQuizState(null);
      return;
    }
    try {
      const wrong = JSON.parse(current.wrongAnswers || '[]');
      const opts = shuffleArray([current.answer, ...wrong]);
      setQuizState({ options: opts, selected: null, correct: current.answer });
    } catch {
      setQuizState(null);
    }
    setShowAnswer(false);
  }, [index, items]);

  const handleRating = async (quality: number) => {
    const item = items[index];
    setSubmitting(true);
    try {
      await api.post('/reviews/submit', { progressId: item.progressId, quality });
    } catch {}
    setShowAnswer(false);
    setQuizState(null);
    setIndex(i => i + 1);
    setSubmitting(false);
  };

  const handleQuizAnswer = (selected: string) => {
    if (!quizState || quizState.selected !== null || submitting) return;
    const isCorrect = selected === quizState.correct;
    setQuizState((s: any) => s ? { ...s, selected } : s);
    setTimeout(async () => {
      await handleRating(isCorrect ? 5 : 2);
    }, 1500);
  };

  const progress = items.length > 0 ? Math.round((index / items.length) * 100) : 0;

  const DocumentFilter = () => documents.length > 0 ? (
    <select
      value={selectedDocumentId}
      onChange={(e) => setSelectedDocumentId(e.target.value)}
      className="text-sm rounded-lg border border-neutral-300 px-2 py-1.5 focus:border-brand-500 focus:ring-1 focus:ring-brand-500 max-w-[200px] truncate"
    >
      <option value="">Todos os documentos</option>
      {documents.map(d => (
        <option key={d.id} value={d.id}>{d.title}</option>
      ))}
    </select>
  ) : null;

  if (loading) return (
    <Layout>
      <div className="flex justify-center py-20"><Spinner size="lg" /></div>
    </Layout>
  );

  if (items.length === 0 && index === 0) return (
    <Layout>
      <div className="p-8 max-w-xl mx-auto animate-fade-in">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold text-neutral-900">Revisao</h1>
          <DocumentFilter />
        </div>
        <div className="card text-center p-8">
          <p className="text-5xl mb-4">🎉</p>
          <h2 className="text-xl font-bold text-neutral-900">Nenhum item pendente!</h2>
          <p className="text-neutral-500 mt-2">Voce nao tem itens para revisar agora{selectedDocumentId ? ' neste documento' : ''}.</p>
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
          <h2 className="text-xl font-bold text-neutral-900">Sessao concluida!</h2>
          <p className="text-neutral-500 mt-2">Voce revisou {items.length} itens. Otimo trabalho!</p>
          <Link to="/" className="btn-primary mt-6 inline-flex">Ver Dashboard</Link>
        </div>
      </div>
    </Layout>
  );

  const isQuiz = current.type === 'Quiz';

  return (
    <Layout>
      <div className="p-8 max-w-xl mx-auto animate-fade-in">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold text-neutral-900">Revisao</h1>
          <DocumentFilter />
        </div>

        <div className="mb-6">
          <div className="flex justify-between text-sm text-neutral-500 mb-2">
            <span>Item {index + 1} de {items.length}</span>
            <span>{progress}% concluido</span>
          </div>
          <div className="w-full bg-neutral-100 rounded-full h-1.5">
            <div className="bg-brand-500 h-1.5 rounded-full transition-all duration-300" style={{ width: `${progress}%` }} />
          </div>
        </div>

        <div className="card mb-4 min-h-48">
          <div className="flex justify-between items-start mb-3">
            <span className={`inline-flex items-center gap-1.5 py-1 px-2.5 rounded-md text-xs font-medium border ${
              isQuiz
                ? 'bg-purple-50 text-purple-700 border-purple-100'
                : 'bg-brand-50 text-brand-700 border-brand-100'
            }`}>
              {isQuiz ? '📝 Quiz' : '🃏 Flashcard'}
              {current.topic && <span className="font-normal opacity-75"> {current.topic}</span>}
            </span>
            {current.documentTitle && (
              <span className="text-xs text-neutral-400 font-medium truncate max-w-[150px]" title={current.documentTitle}>
                📄 {current.documentTitle}
              </span>
            )}
          </div>
          <p className="text-lg text-neutral-800 leading-relaxed mt-2">{current.question}</p>
        </div>

        {isQuiz && quizState ? (
          <div className="space-y-3 animate-slide-up">
            {quizState.options.map((option: string) => {
              const isSelected = quizState.selected === option;
              const isCorrectOption = option === quizState.correct;
              const hasAnswered = quizState.selected !== null;

              let btnClass = 'w-full text-left px-4 py-3 rounded-lg border text-sm font-medium transition-all duration-300 ';
              if (!hasAnswered) {
                btnClass += 'border-neutral-300 bg-white hover:border-brand-400 hover:bg-brand-50 text-neutral-800 cursor-pointer';
              } else if (isCorrectOption) {
                btnClass += 'border-green-500 bg-green-50 text-green-800';
              } else if (isSelected) {
                btnClass += 'border-red-500 bg-red-50 text-red-800';
              } else {
                btnClass += 'border-neutral-200 bg-neutral-50 text-neutral-400 opacity-60';
              }

              return (
                <button
                  key={option}
                  className={btnClass}
                  onClick={() => handleQuizAnswer(option)}
                  disabled={hasAnswered || submitting}
                >
                  <span className="flex items-center gap-2">
                    {hasAnswered && isCorrectOption && <span>✅</span>}
                    {hasAnswered && isSelected && !isCorrectOption && <span>❌</span>}
                    {option}
                  </span>
                </button>
              );
            })}
            {quizState.selected && (
              <p className="text-center text-sm text-neutral-400 pt-1">Avancando automaticamente...</p>
            )}
          </div>
        ) : (
          !showAnswer ? (
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
                <p className="text-sm font-medium text-neutral-700 mb-2 text-center">Como foi para voce?</p>
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
          )
        )}
      </div>
    </Layout>
  );
}



