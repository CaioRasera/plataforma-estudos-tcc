import { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate, Link } from 'react-router-dom';
import { Spinner } from '../components/ui/Spinner';
import api from '../services/api';

export const LogoIcon = () => (
  <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <rect x="3" y="7" width="18" height="13" rx="2" ry="2" strokeWidth="2.5" />`n    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M18 7V5a2 2 0 0 0-2-2H8a2 2 0 0 0-2 2v2" />
  </svg>
);

export default function RegisterPage() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const res = await api.post('/auth/register', { name, email, password });
      login(res.data.token, res.data.user);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Não foi possível criar a conta. Tente novamente.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex bg-white font-sans text-neutral-900">
      {/* Left Side - Form */}
      <div className="w-full lg:w-1/2 flex flex-col justify-center px-8 sm:px-16 md:px-24 xl:px-32 relative">
        
        {/* Top Logo */}
        <div className="absolute top-10 left-8 sm:left-16 md:left-24 xl:left-32 flex items-center gap-2.5">
          <div className="w-8 h-8 rounded bg-brand-600 flex items-center justify-center shadow-sm">
            <LogoIcon />
          </div>
          <span className="font-bold text-lg tracking-tight">RevisIA</span>
        </div>

        <div className="max-w-sm w-full mx-auto animate-fade-in mt-12">
          <h1 className="text-3xl font-bold tracking-tight">Criar conta</h1>
          <p className="text-neutral-500 mt-2 mb-8 text-sm">
            Junte-se a nós para otimizar seus estudos e aprender de forma mais inteligente.
          </p>

          {error && (
            <div className="mb-6 px-4 py-3 bg-red-50 border border-red-200 text-sm text-red-700 rounded-md flex items-center gap-2">
              <svg className="w-4 h-4 text-red-600 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path></svg>
              <span>{error}</span>
            </div>
          )}

          <form onSubmit={handleRegister} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1.5">Nome completo</label>
              <input
                type="text"
                className="w-full px-3 py-2.5 rounded-lg border border-neutral-300 focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-colors bg-white text-neutral-900 outline-none sm:text-sm"
                placeholder="Seu nome"
                value={name}
                onChange={e => setName(e.target.value)}
                required
              />
            </div>
            
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1.5">E-mail</label>
              <input
                type="email"
                className="w-full px-3 py-2.5 rounded-lg border border-neutral-300 focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-colors bg-white text-neutral-900 outline-none sm:text-sm"
                placeholder="seu@email.com"
                value={email}
                onChange={e => setEmail(e.target.value)}
                required
              />
            </div>
            
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1.5">Senha</label>
              <input
                type="password"
                className="w-full px-3 py-2.5 rounded-lg border border-neutral-300 focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-colors bg-white text-neutral-900 outline-none sm:text-sm"
                placeholder="Mínimo de 6 caracteres"
                value={password}
                onChange={e => setPassword(e.target.value)}
                required
                minLength={6}
              />
            </div>

            <div className="pt-2">
              <button type="submit" className="w-full bg-neutral-900 hover:bg-neutral-800 text-white font-medium py-2.5 px-4 rounded-lg transition-colors flex justify-center items-center gap-2 sm:text-sm" disabled={loading}>
                {loading ? <Spinner size="sm" /> : null}
                {loading ? 'Criando conta...' : 'Cadastrar-se'}
              </button>
            </div>
          </form>

          <div className="mt-8 text-center text-sm text-neutral-500">
            Já tem uma conta?{' '}
            <Link to="/login" className="text-brand-600 font-medium hover:text-brand-800 hover:underline transition-colors">
              Fazer login
            </Link>
          </div>
        </div>
      </div>

      {/* Right Side - Soulful & Human */}
      <div className="hidden lg:flex lg:w-1/2 bg-neutral-50 relative items-center justify-center p-16 lg:p-24 border-l border-neutral-200">
        <div className="max-w-md w-full animate-slide-up">
          <div className="space-y-6">
            <svg className="w-10 h-10 text-brand-500" fill="currentColor" viewBox="0 0 24 24">
              <path d="M14.017 21v-7.391c0-5.704 3.731-9.57 8.983-10.609l.995 2.151c-2.432.917-3.995 3.638-3.995 5.849h4v10h-9.983zm-14.017 0v-7.391c0-5.704 3.748-9.57 9-10.609l.996 2.151c-2.433.917-3.996 3.638-3.996 5.849h3.983v10h-9.983z" />
            </svg>
            
            <blockquote className="text-3xl font-semibold text-neutral-800 leading-snug">
              "Ensinar não é transferir conhecimento, mas criar as possibilidades para a sua própria produção ou a sua construção."
            </blockquote>
            
            <div className="pt-2">
              <p className="text-neutral-900 font-medium">— Paulo Freire</p>
              <p className="text-neutral-500 text-sm mt-1">Sua jornada começa aqui.</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
