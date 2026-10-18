import React, { useEffect, useState } from 'react';
import { api } from '../../services/apiClient';
import { useToast } from '../../components/common/Toast';

type Insight = { id: number; insightType: string; title: string; content: string; recommendation?: unknown; confidenceScore?: number; isRead: boolean; generatedAt: string };
const formatDetail = (value: unknown) => typeof value === 'string' ? value : value == null ? '' : JSON.stringify(value);

export const AIInsightsPage: React.FC = () => {
  const { showToast } = useToast();
  const [insights, setInsights] = useState<Insight[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = async () => {
    setLoading(true);
    setError('');
    try {
      setInsights(await api.request<Insight[]>('/api/ai/insights?page=1&pageSize=100'));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not load insights.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void load(); }, []);

  const markRead = async (id: number) => {
    try {
      await api.request('/api/ai/insights/' + id + '/read', { method: 'PATCH', body: JSON.stringify({ isRead: true }) });
      setInsights(current => current.map(insight => insight.id === id ? { ...insight, isRead: true } : insight));
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not mark insight as read.', 'error');
    }
  };

  return <div className="max-w-5xl mx-auto space-y-6">
    <header><h1 className="text-2xl font-bold">Business Insights</h1><p className="text-sm text-slate-500">Insights generated for your business by the API.</p></header>
    {loading && <p className="text-slate-500">Loading insights…</p>}
    {error && <div className="space-y-2"><p className="text-rose-600">{error}</p><button className="text-indigo-600 font-semibold" onClick={() => void load()}>Retry</button></div>}
    {!loading && !error && insights.length === 0 && <p className="rounded-xl bg-white border border-slate-200 p-6 text-slate-500">No insights have been generated yet.</p>}
    <div className="grid md:grid-cols-2 gap-4">{insights.map(insight => <article key={insight.id} className="rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5 space-y-3">
      <div className="flex justify-between text-xs text-slate-500"><span>{insight.insightType}</span><span>{new Date(insight.generatedAt).toLocaleDateString('en-IN')}</span></div>
      <h2 className="font-bold text-lg">{insight.title}</h2><p className="text-sm text-slate-700 dark:text-slate-300">{insight.content}</p>
      {insight.recommendation != null && <div className="rounded-lg bg-indigo-50 dark:bg-indigo-950 p-3 text-sm"><strong>Recommendation</strong><p>{formatDetail(insight.recommendation)}</p></div>}
      {!insight.isRead && <button onClick={() => void markRead(insight.id)} className="text-sm text-indigo-600 font-semibold">Mark as read</button>}
    </article>)}</div>
  </div>;
};
