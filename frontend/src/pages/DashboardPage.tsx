import { useState, useEffect } from 'react';
import { Layout } from '../components/ui/Layout';
import { StatCard } from '../components/ui/StatCard';
import { Spinner } from '../components/ui/Spinner';
import { Link } from 'react-router-dom';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import api from '../services/api';
import type { DashboardSummaryDto } from '../types';

export default function DashboardPage() {
  const [data, setData] = useState<DashboardSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get('/dashboard')
      .then(res => setData(res.data))
      .catch(() => setData(null))
      .finally(() => setLoading(false));
  }, []);

  return (
    <Layout>
      <div className="p-8 max-w-5xl mx-auto animate-fade-in">
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-neutral-900">Dashboard</h1>
          <p className="text-neutral-500 mt-1">Acompanhe seu progresso de estudos</p>
        </div>

        {loading ? (
          <div className="flex justify-center py-20"><Spinner size="lg" /></div>
        ) : !data ? (
          <div className="card text-center py-12 text-neutral-500">
            Não foi possível carregar os dados. Verifique se a API está rodando.
          </div>
        ) : (
          <div className="space-y-8">
            {/* Stats */}
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              <StatCard label="Flashcards para revisar" value={data.dueToday} icon="⏰" color="bg-amber-50 text-amber-600" />
              <StatCard label="Flashcards revisados" value={data.reviewedItems} icon="✅" color="bg-green-50 text-green-600" />
              <StatCard label="Total de flashcards" value={data.totalItems} icon="📚" />
              <StatCard label="Sequência (dias)" value={data.streakDays} icon="🔥" color="bg-orange-50 text-orange-600" />
            </div>

            {/* Chart */}
            <div className="card">
              <h2 className="text-base font-semibold text-neutral-800 mb-4">Revisões — últimos 7 dias</h2>
              {data.reviewsLast7Days.length > 0 ? (
                <ResponsiveContainer width="100%" height={200}>
                  <BarChart data={data.reviewsLast7Days} barSize={28}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f4f4f5" />
                    <XAxis dataKey="date" tick={{ fontSize: 12, fill: '#71717a' }} axisLine={false} tickLine={false} />
                    <YAxis tick={{ fontSize: 12, fill: '#71717a' }} axisLine={false} tickLine={false} allowDecimals={false} />
                    <Tooltip
                      contentStyle={{ borderRadius: 8, border: '1px solid #e4e4e7', fontSize: 12 }}
                      cursor={{ fill: '#f4f4f5' }}
                    />
                    <Bar dataKey="count" fill="#6366f1" radius={[4, 4, 0, 0]} name="Revisões" />
                  </BarChart>
                </ResponsiveContainer>
              ) : (
                <p className="text-sm text-neutral-400 text-center py-8">Ainda sem dados de revisão.</p>
              )}
            </div>

            {/* Document progress */}
            <div className="card">
              <h2 className="text-base font-semibold text-neutral-800 mb-4">Progresso por documento</h2>
              {data.documentProgress.length === 0 ? (
                <p className="text-sm text-neutral-400">
                  Nenhum documento ainda. <Link to="/documents" className="text-brand-600 hover:underline">Faça upload de um PDF →</Link>
                </p>
              ) : (
                <div className="space-y-3">
                  {data.documentProgress.map((doc, i) => {
                    const pct = doc.totalItems > 0 ? Math.round((doc.reviewedItems / doc.totalItems) * 100) : 0;
                    return (
                      <div key={i}>
                        <div className="flex justify-between text-sm mb-1">
                          <span className="font-medium text-neutral-700 truncate">{doc.documentTitle}</span>
                          <span className="text-neutral-400 ml-4 flex-shrink-0">{doc.reviewedItems}/{doc.totalItems} cards ({pct}%)</span>
                        </div>
                        <div className="w-full bg-neutral-100 rounded-full h-1.5">
                          <div className="bg-brand-500 h-1.5 rounded-full transition-all" style={{ width: `${pct}%` }} />
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>

            {/* CTA */}
            {data.dueToday > 0 && (
              <div className="card bg-brand-50 border-brand-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-semibold text-brand-800">Você tem {data.dueToday} itens para revisar hoje!</p>
                    <p className="text-sm text-brand-600 mt-0.5">Mantenha sua sequência de estudos em dia.</p>
                  </div>
                  <Link to="/review" className="btn-primary flex-shrink-0">Revisar agora</Link>
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </Layout>
  );
}
