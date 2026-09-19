import { useState, useEffect } from 'react';
import { Layout } from '../components/ui/Layout';
import { Spinner } from '../components/ui/Spinner';
import api from '../services/api';
import type { PlanDto, BalanceDto } from '../types';

export default function PlansPage() {
  const [plans, setPlans] = useState<PlanDto[]>([]);
  const [balance, setBalance] = useState<BalanceDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [checkoutPlan, setCheckoutPlan] = useState<PlanDto | null>(null);
  const [processing, setProcessing] = useState(false);
  
  // State para o Toast
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

  const showToast = (message: string, type: 'success' | 'error') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 4000);
  };

  const loadData = async () => {
    try {
      const [plansRes, balanceRes] = await Promise.all([
        api.get<PlanDto[]>('/payment/plans'),
        api.get<BalanceDto>('/payment/balance')
      ]);
      setPlans(plansRes.data);
      setBalance(balanceRes.data);
    } catch {
      // silent
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadData(); }, []);

  const handleCheckout = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!checkoutPlan) return;
    
    setProcessing(true);
    try {
      // Simula tempo de processamento do cartão
      await new Promise(r => setTimeout(r, 1500));
      
      await api.post('/payment/checkout', { planId: checkoutPlan.id });
      await loadData();
      setCheckoutPlan(null);
      showToast('Plano atualizado com sucesso! Seus tokens foram recarregados.', 'success');
    } catch {
      showToast('Erro ao processar pagamento.', 'error');
    } finally {
      setProcessing(false);
    }
  };

  return (
    <Layout>
      <div className="p-8 max-w-5xl mx-auto animate-fade-in relative">
        <div className="text-center mb-12">
          <h1 className="text-3xl font-bold text-neutral-900">Planos e Tokens</h1>
          <p className="text-neutral-500 mt-2 max-w-xl mx-auto">
            Cada flashcard gerado consome 1 token. Escolha o plano ideal para seus estudos.
          </p>
        </div>

        {loading ? (
          <div className="flex justify-center py-20"><Spinner size="lg" /></div>
        ) : (
          <div className="grid md:grid-cols-3 gap-8">
            {plans.map(plan => {
              const isCurrent = balance?.planId === plan.id;
              
              return (
                <div 
                  key={plan.id} 
                  className={`card relative flex flex-col ${isCurrent ? 'ring-2 ring-brand-500 shadow-lg' : ''}`}
                >
                  {isCurrent && (
                    <div className="absolute -top-3 left-1/2 -translate-x-1/2 bg-brand-500 text-white text-xs font-bold px-3 py-1 rounded-full">
                      SEU PLANO ATUAL
                    </div>
                  )}
                  
                  <div className="text-center mb-6">
                    <h3 className="text-xl font-bold text-neutral-900">{plan.name}</h3>
                    <div className="mt-4 flex items-baseline justify-center gap-1">
                      <span className="text-3xl font-bold text-neutral-900">{plan.priceDisplay}</span>
                      {plan.priceDisplay !== 'Grátis' && <span className="text-neutral-500 text-sm">/mês</span>}
                    </div>
                  </div>

                  <div className="flex-1">
                    <ul className="space-y-3 mb-8">
                      <li className="flex items-center gap-2 text-sm text-neutral-600">
                        <span className="text-green-500">✓</span> {plan.monthlyTokens} tokens por mês
                      </li>
                      <li className="flex items-center gap-2 text-sm text-neutral-600">
                        <span className="text-green-500">✓</span> Geração de Flashcards com IA
                      </li>
                      <li className="flex items-center gap-2 text-sm text-neutral-600">
                        <span className="text-green-500">✓</span> Revisão Espaçada Inteligente
                      </li>
                    </ul>
                  </div>

                  <button
                    onClick={() => setCheckoutPlan(plan)}
                    disabled={isCurrent}
                    className={`w-full py-2.5 rounded-lg text-sm font-semibold transition-colors ${
                      isCurrent 
                        ? 'bg-neutral-100 text-neutral-400 cursor-not-allowed' 
                        : plan.priceDisplay === 'Grátis' 
                          ? 'bg-neutral-100 text-neutral-800 hover:bg-neutral-200'
                          : 'bg-brand-600 text-white hover:bg-brand-700 shadow-sm'
                    }`}
                  >
                    {isCurrent ? 'Ativo' : plan.priceDisplay === 'Grátis' ? 'Fazer Downgrade' : 'Assinar ' + plan.name}
                  </button>
                </div>
              );
            })}
          </div>
        )}

        {/* Modal Fake Checkout */}
        {checkoutPlan && (
          <div className="fixed inset-0 bg-neutral-900/40 flex items-center justify-center p-4 z-50 animate-fade-in">
            <div className="bg-white rounded-xl shadow-xl p-6 w-full max-w-md animate-slide-up">
              <h2 className="text-xl font-bold mb-4">Checkout de Demonstração</h2>
              <p className="text-sm text-neutral-500 mb-6">
                Você está simulando a assinatura do plano <strong>{checkoutPlan.name}</strong> por <strong>{checkoutPlan.priceDisplay}</strong>.
                Nenhuma cobrança real será feita.
              </p>

              <form onSubmit={handleCheckout} className="space-y-4">
                <div>
                  <label className="block text-xs font-medium text-neutral-700 mb-1">Nome no Cartão</label>
                  <input type="text" defaultValue="Usuario Teste" required className="input-field" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-neutral-700 mb-1">Número do Cartão</label>
                  <input type="text" defaultValue="0000 0000 0000 0000" required className="input-field" />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-medium text-neutral-700 mb-1">Validade</label>
                    <input type="text" defaultValue="12/30" required className="input-field" />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-neutral-700 mb-1">CVV</label>
                    <input type="text" defaultValue="123" required className="input-field" />
                  </div>
                </div>

                <div className="flex gap-3 pt-4">
                  <button type="button" onClick={() => setCheckoutPlan(null)} className="btn-secondary flex-1" disabled={processing}>
                    Cancelar
                  </button>
                  <button type="submit" className="btn-primary flex-1" disabled={processing}>
                    {processing ? <Spinner size="sm" /> : 'Confirmar Assinatura'}
                  </button>
                </div>
              </form>
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