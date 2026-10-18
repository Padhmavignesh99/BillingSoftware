import React, { useEffect, useState } from 'react';
import { api } from '../../services/apiClient';
import { useToast } from '../../components/common/Toast';

type Status = { status: string; message: string; isAllowed: boolean; isTrial: boolean; expiryDate?: string; daysRemaining?: number; currentPlanName?: string };
type Plan = { id: number; name: string; description?: string; monthlyPrice: number; yearlyPrice?: number; maxProducts: number; maxUsers: number; maxBranches: number; maxInvoicesPerMonth: number; features: string[] };
type Payment = { id: number; amount: number; currency?: string; paymentGateway?: string; transactionId?: string; status: string; paymentDate: string };
type Subscription = { id: number; planName?: string; status: string | number; billingCycle: string; startDate: string; endDate: string; amountPaid: number };
type History = { subscriptions: Subscription[]; payments: Payment[]; trial?: { startDate: string; endDate: string; isConverted: boolean } };

const money = (value: number) => '₹' + value.toLocaleString('en-IN');
const date = (value?: string) => value ? new Date(value).toLocaleDateString('en-IN') : '—';

export const SubscriptionPage: React.FC = () => {
  const { showToast } = useToast();
  const [status, setStatus] = useState<Status | null>(null);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [history, setHistory] = useState<History | null>(null);
  const [loading, setLoading] = useState(true);
  const [starting, setStarting] = useState(false);
  const [error, setError] = useState('');

  const load = async () => {
    setLoading(true);
    setError('');
    try {
      const [nextStatus, nextPlans, nextHistory] = await Promise.all([
        api.request<Status>('/api/subscription/status'),
        api.request<Plan[]>('/api/subscription/plans'),
        api.request<History>('/api/subscription/history'),
      ]);
      setStatus(nextStatus);
      setPlans(nextPlans);
      setHistory(nextHistory);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not load subscription details.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void load(); }, []);

  const startTrial = async () => {
    setStarting(true);
    try {
      await api.request('/api/subscription/start-trial', { method: 'POST', body: JSON.stringify({}) });
      await load();
      showToast('Free trial started.', 'success');
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not start the trial.', 'error');
    } finally {
      setStarting(false);
    }
  };

  if (loading) return <div className="p-6 text-slate-500">Loading subscription details…</div>;
  if (error) return <div className="p-6 space-y-3"><p className="text-rose-600">{error}</p><button onClick={() => void load()} className="text-indigo-600 font-semibold">Retry</button></div>;

  return <div className="max-w-6xl mx-auto space-y-6">
    <header><h1 className="text-2xl font-bold text-slate-900 dark:text-white">Subscription & Plan</h1><p className="text-sm text-slate-500">Your current status and available plans.</p></header>
    <section className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 space-y-3">
      <div className="flex flex-wrap items-center gap-3"><h2 className="text-lg font-bold">{status?.currentPlanName || (status?.isTrial ? 'Free trial' : 'No active plan')}</h2><span className="rounded-full bg-indigo-100 text-indigo-700 px-3 py-1 text-xs font-bold">{status?.status || 'Unknown'}</span></div>
      <p className="text-sm text-slate-600 dark:text-slate-300">{status?.message}</p>
      {status?.expiryDate && <p className="text-sm">Ends {date(status.expiryDate)}{status.daysRemaining !== undefined ? ' · ' + status.daysRemaining + ' days remaining' : ''}</p>}
      {status?.status === 'NoSubscription' && !history?.trial && <button disabled={starting} onClick={() => void startTrial()} className="px-4 py-2 rounded-lg bg-indigo-600 text-white font-semibold disabled:opacity-50">{starting ? 'Starting…' : 'Start free trial'}</button>}
    </section>
    <section className="space-y-3"><h2 className="text-lg font-bold">Available plans</h2><div className="grid md:grid-cols-3 gap-4">{plans.map(plan => <div key={plan.id} className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5 space-y-3"><h3 className="font-bold text-lg">{plan.name}</h3><p className="text-sm text-slate-500">{plan.description}</p><p className="font-semibold">{money(plan.monthlyPrice)} / month</p>{plan.yearlyPrice != null && <p className="text-sm">{money(plan.yearlyPrice)} / year</p>}<div className="text-sm text-slate-600 dark:text-slate-300"><p>Products: {plan.maxProducts}</p><p>Users: {plan.maxUsers}</p><p>Branches: {plan.maxBranches}</p><p>Invoices per month: {plan.maxInvoicesPerMonth}</p></div><ul className="text-sm list-disc pl-5">{plan.features.map(feature => <li key={feature}>{feature}</li>)}</ul></div>)}</div>{plans.length === 0 && <p className="text-slate-500">No plans are currently available.</p>}</section>
    <section className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 space-y-3"><h2 className="text-lg font-bold">Subscription history</h2>{history?.subscriptions?.length ? <div className="overflow-x-auto"><table className="w-full text-sm text-left"><thead><tr className="border-b"><th className="py-2">Plan</th><th>Cycle</th><th>Period</th><th>Status</th><th className="text-right">Paid</th></tr></thead><tbody>{history.subscriptions.map(sub => <tr key={sub.id} className="border-b"><td className="py-2">{sub.planName || 'Plan'}</td><td>{sub.billingCycle}</td><td>{date(sub.startDate)} – {date(sub.endDate)}</td><td>{sub.status}</td><td className="text-right">{money(sub.amountPaid)}</td></tr>)}</tbody></table></div> : <p className="text-sm text-slate-500">No subscription history yet.</p>}</section>
    <section className="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 space-y-3"><h2 className="text-lg font-bold">Recorded payments</h2>{history?.payments?.length ? <div className="overflow-x-auto"><table className="w-full text-sm text-left"><thead><tr className="border-b"><th className="py-2">Date</th><th>Reference</th><th>Method</th><th>Status</th><th className="text-right">Amount</th></tr></thead><tbody>{history.payments.map(payment => <tr key={payment.id} className="border-b"><td className="py-2">{date(payment.paymentDate)}</td><td>{payment.transactionId || '#' + payment.id}</td><td>{payment.paymentGateway || 'Manual'}</td><td>{payment.status}</td><td className="text-right">{money(payment.amount)}</td></tr>)}</tbody></table></div> : <p className="text-sm text-slate-500">No payments recorded.</p>}</section>
  </div>;
};
