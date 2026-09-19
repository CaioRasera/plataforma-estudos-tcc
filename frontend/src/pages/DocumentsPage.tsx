import { useState, useEffect, useRef } from 'react';
import { Layout } from '../components/ui/Layout';
import { Spinner } from '../components/ui/Spinner';
import { StatusBadge } from '../components/ui/StatusBadge';
import api from '../services/api';
import type { DocumentDto } from '../types';

export default function DocumentsPage() {
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  
  // States para o modal de exclusão
  const [documentToDelete, setDocumentToDelete] = useState<DocumentDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // States para o Toast
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

  const fileRef = useRef<HTMLInputElement>(null);
  const prevDocumentsRef = useRef<DocumentDto[]>([]);

  const showToast = (message: string, type: 'success' | 'error') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 4000);
  };

  const loadDocs = async (isPolling = false) => {
    try {
      const res = await api.get<DocumentDto[]>('/documents');
      const currentDocs = res.data;
      
      // Checa se algum doc mudou de Processing para Failed
      if (isPolling) {
        currentDocs.forEach(newDoc => {
          const oldDoc = prevDocumentsRef.current.find(d => d.id === newDoc.id);
          if (oldDoc && (oldDoc.status === 'Processing' || oldDoc.status === 'Pending')) {
            if (newDoc.status === 'Failed') {
              showToast(`Falha ao processar "${newDoc.title}". O Google Gemini pode estar instável.`, 'error');
            } else if (newDoc.status === 'TokensInsufficient') {
              showToast(`Tokens esgotados durante "${newDoc.title}".`, 'error');
            } else if (newDoc.status === 'Processed') {
              showToast(`"${newDoc.title}" processado com sucesso!`, 'success');
            }
          }
        });
      }

      setDocuments(currentDocs);
      prevDocumentsRef.current = currentDocs;
    } catch {
      if (!isPolling) showToast('Não foi possível carregar os documentos.', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadDocs(); }, []);

  useEffect(() => {
    const isProcessing = documents.some(d => d.status === 'Pending' || d.status === 'Processing');
    if (!isProcessing) return;

    const interval = setInterval(() => loadDocs(true), 3000);
    return () => clearInterval(interval);
  }, [documents]);

  const handleUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('title', file.name.replace('.pdf', ''));
      await api.post('/documents/upload', formData);
      loadDocs();
      showToast('Documento enviado com sucesso!', 'success');
    } catch {
      showToast('Falha no upload. Verifique se é um PDF válido.', 'error');
    } finally {
      setUploading(false);
      if (fileRef.current) fileRef.current.value = '';
    }
  };

  const confirmDelete = async () => {
    if (!documentToDelete) return;
    setIsDeleting(true);
    try {
      await api.delete(`/documents/${documentToDelete.id}`);
      setDocuments(docs => docs.filter(d => d.id !== documentToDelete.id));
      prevDocumentsRef.current = prevDocumentsRef.current.filter(d => d.id !== documentToDelete.id);
      showToast('Documento excluído com sucesso!', 'success');
    } catch {
      showToast('Erro ao excluir o documento.', 'error');
    } finally {
      setIsDeleting(false);
      setDocumentToDelete(null);
    }
  };

  return (
    <Layout>
      <div className="p-8 max-w-3xl mx-auto animate-fade-in relative">
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-2xl font-bold text-neutral-900">Documentos</h1>
            <p className="text-neutral-500 mt-1">Faça upload de PDFs para gerar flashcards com IA</p>
          </div>
          <div>
            <input ref={fileRef} type="file" accept=".pdf" className="hidden" onChange={handleUpload} />
            <button className="btn-primary" onClick={() => fileRef.current?.click()} disabled={uploading}>
              {uploading ? <Spinner size="sm" /> : '📎'}
              {uploading ? 'Enviando...' : 'Upload de PDF'}
            </button>
          </div>
        </div>

        {loading && documents.length === 0 ? (
          <div className="flex justify-center py-20"><Spinner size="lg" /></div>
        ) : documents.length === 0 ? (
          <div className="card text-center py-16">
            <p className="text-4xl mb-4">📂</p>
            <p className="font-medium text-neutral-700">Nenhum documento ainda</p>
            <p className="text-sm text-neutral-400 mt-1">Faça upload de um PDF para começar</p>
          </div>
        ) : (
          <div className="space-y-3">
            {documents.map(d => (
              <div key={d.id} className="card-hover flex items-center gap-4">
                <div className="w-10 h-10 rounded-lg bg-brand-50 flex items-center justify-center text-lg flex-shrink-0">
                  📄
                </div>
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-neutral-800 truncate">{d.title}</p>
                  <p className="text-xs text-neutral-400 mt-0.5">
                    {new Date(d.createdAt).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' })}
                  </p>
                </div>
                <StatusBadge status={d.status} />
                <button 
                  onClick={() => setDocumentToDelete(d)}
                  className="w-8 h-8 flex items-center justify-center text-neutral-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors ml-2"
                  title="Excluir documento"
                >
                  🗑️
                </button>
              </div>
            ))}
          </div>
        )}

        {/* Custom Delete Confirmation Modal */}
        {documentToDelete && (
          <div className="fixed inset-0 bg-neutral-900/40 flex items-center justify-center z-50 p-4 animate-fade-in">
            <div className="bg-white rounded-xl shadow-xl w-full max-w-sm overflow-hidden animate-slide-up">
              <div className="p-6">
                <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center text-red-500 text-xl mb-4">
                  🗑️
                </div>
                <h3 className="text-lg font-bold text-neutral-900">Excluir documento?</h3>
                <p className="text-neutral-500 text-sm mt-2">
                  Tem certeza que deseja excluir <strong>{documentToDelete.title}</strong>? Esta ação é irreversível e apagará todos os flashcards e recortes gerados.
                </p>
              </div>
              <div className="bg-neutral-50 px-6 py-4 flex justify-end gap-3 border-t border-neutral-100">
                <button 
                  onClick={() => setDocumentToDelete(null)}
                  disabled={isDeleting}
                  className="px-4 py-2 text-sm font-medium text-neutral-700 bg-white border border-neutral-300 rounded-lg hover:bg-neutral-50 transition-colors disabled:opacity-50"
                >
                  Cancelar
                </button>
                <button 
                  onClick={confirmDelete}
                  disabled={isDeleting}
                  className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors flex items-center gap-2 disabled:opacity-50"
                >
                  {isDeleting ? <Spinner size="sm" /> : null}
                  Sim, excluir
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Toast Notification */}
        {toast && (
          <div className="fixed bottom-6 right-6 z-50 animate-slide-up">
            <div className={`flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg border ${
              toast.type === 'success' ? 'bg-green-50 border-green-200 text-green-800' : 'bg-red-50 border-red-200 text-red-800'
            }`}>
              <span className="text-lg">{toast.type === 'success' ? '✅' : '⚠️'}</span>
              <p className="text-sm font-medium">{toast.message}</p>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}