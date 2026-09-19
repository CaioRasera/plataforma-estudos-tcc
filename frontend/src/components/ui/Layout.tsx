import { ReactNode, useEffect, useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import api from '../../services/api';
import type { BalanceDto } from '../../types';

interface LayoutProps {
  children: ReactNode;
}

export const LogoIcon = () => (
  <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <rect x="3" y="7" width="18" height="13" rx="2" ry="2" strokeWidth="2.5" />`n    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M18 7V5a2 2 0 0 0-2-2H8a2 2 0 0 0-2 2v2" />
  </svg>
);

export function Layout({ children }: LayoutProps) {
  const { logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [balance, setBalance] = useState<BalanceDto | null>(null);

  useEffect(() => {
    api.get<BalanceDto>('/payment/balance')
      .then(res => setBalance(res.data))
      .catch(() => {}); // silent fail for MVP
  }, [location.pathname]);

  const navItems = [
    { path: '/', label: 'Dashboard', icon: '📊' },
    { path: '/documents', label: 'Documentos', icon: '📄' },
    { path: '/review', label: 'Revisar', icon: '🧠' },
    { path: '/plans', label: 'Planos', icon: '💎' },
  ];

  return (
    <div className="min-h-screen bg-neutral-50 flex">
      <aside className="w-64 bg-white border-r border-neutral-200 flex flex-col hidden md:flex">
        <div className="p-6">
          <div className="flex items-center gap-3 text-neutral-900 font-bold text-xl tracking-tight">
            <div className="w-8 h-8 rounded bg-brand-600 flex items-center justify-center shadow-sm">
              <LogoIcon />
            </div>
            <span>RevisIA</span>
          </div>
        </div>

        <nav className="flex-1 px-4 space-y-1">
          {navItems.map((item) => {
            const isActive = location.pathname === item.path || (item.path !== '/' && location.pathname.startsWith(item.path));
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                  isActive 
                    ? 'bg-brand-50 text-brand-700' 
                    : 'text-neutral-600 hover:bg-neutral-50 hover:text-neutral-900'
                }`}
              >
                <span className="text-lg grayscale opacity-80">{item.icon}</span>
                {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="p-4 border-t border-neutral-200">
          {balance && (
            <div className="mb-4 p-3 bg-neutral-50 rounded-lg border border-neutral-200">
              <div className="text-xs text-neutral-500 font-bold mb-1 tracking-wide">SALDO DE TOKENS</div>
              <div className="flex items-center justify-between">
                <span className="text-sm font-bold text-neutral-800">{balance.tokenBalance.toLocaleString('pt-BR')} ⚡</span>
                <span className="text-xs px-2 py-0.5 bg-brand-100 text-brand-700 rounded-full font-semibold">
                  {balance.planName || 'Gratuito'}
                </span>
              </div>
            </div>
          )}
          <button
            onClick={() => { logout(); navigate('/login'); }}
            className="flex items-center gap-3 px-3 py-2 w-full rounded-lg text-sm font-medium text-red-600 hover:bg-red-50 transition-colors"
          >
            <span className="text-lg opacity-80">🚪</span>
            Sair
          </button>
        </div>
      </aside>

      <main className="flex-1 overflow-auto">
        {children}
      </main>
    </div>
  );
}
