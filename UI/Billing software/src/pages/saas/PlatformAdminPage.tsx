import React, { useEffect, useState } from 'react';
import { api } from '../../services/apiClient';
import { useToast } from '../../components/common/Toast';

type Dashboard = { totalShops: number; activeShops: number; suspendedShops: number; totalUsers: number; activeTrials: number; activeSubscriptions: number; totalRevenueThisMonth: number; totalRevenueAllTime: number };
type Shop = { id: number; name: string; email?: string; status: string; totalUsers: number; currentPlanName?: string; subscriptionStatus: string };
type Plan = { id: number; name: string; monthlyPrice: number; yearlyPrice?: number; activeSubscribers: number; isActive: boolean };
type AuditLog = { id: number; businessName?: string; userName?: string; action: string; entityType: string; timestamp: string };
const money = (value: number) => '₹' + value.toLocaleString('en-IN');

export const PlatformAdminPage: React.FC = () => {
  const { showToast } = useToast();
  const [dashboard, setDashboard] = useState<Dashboard | null>(null);
  const [shops, setShops] = useState<Shop[]>([]);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busyId, setBusyId] = useState<number | null>(null);

  const load = async () => {
    setLoading(true);
    setError('');
    try {
      const [nextDashboard, nextShops, nextPlans, nextLogs] = await Promise.all([
        api.request<Dashboard>('/api/admin/dashboard'),
        api.request<Shop[]>('/api/admin/shops'),
        api.request<Plan[]>('/api/admin/plans'),
        api.request<AuditLog[]>('/api/admin/audit-logs?page=1&pageSize=50'),
      ]);
      setDashboard(nextDashboard);
      setShops(nextShops);
      setPlans(nextPlans);
      setLogs(nextLogs);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not load platform data.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void load(); }, []);

  const changeShopStatus = async (shop: Shop) => {
    const action = shop.status.toLowerCase() === 'suspended' ? 'activate' : 'suspend';
    setBusyId(shop.id);
    try {
      await api.request('/api/admin/shops/' + shop.id + '/' + action, { method: 'POST' });
      await load();
      showToast('Shop status updated.', 'success');
    } catch (err) {
      showToast(err instanceof Error ? err.message : 'Could not update shop.', 'error');
    } finally {
      setBusyId(null);
    }
  };

  if (loading) return <div className="p-6 text-slate-500">Loading platform data…</div>;
  if (error) return <div className="p-6 space-y-3"><p className="text-rose-600">{error}</p><button className="text-indigo-600 font-semibold" onClick={() => void load()}>Retry</button></div>;

  return <div className="max-w-7xl mx-auto space-y-6">
    <header><h1 className="text-2xl font-bold">Platform Admin</h1><p className="text-sm text-slate-500">Live platform data. This page requires a SuperAdmin account.</p></header>
    {dashboard && <section className="grid grid-cols-2 lg:grid-cols-4 gap-4">{[
      ['Total shops', dashboard.totalShops], ['Active shops', dashboard.activeShops],
      ['Users', dashboard.totalUsers], ['Active subscriptions', dashboard.activeSubscriptions],
      ['Active trials', dashboard.activeTrials], ['Suspended shops', dashboard.suspendedShops],
      ['Revenue this month', money(dashboard.totalRevenueThisMonth)], ['Revenue all time', money(dashboard.totalRevenueAllTime)],
    ].map(([label, value]) => <div key={label} className="rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-4"><p className="text-xs text-slate-500">{label}</p><p className="text-xl font-bold mt-1">{value}</p></div>)}</section>}
    <section className="rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5"><h2 className="font-bold mb-3">Shops</h2><div className="overflow-x-auto"><table className="w-full text-sm text-left"><thead><tr className="border-b"><th className="py-2">Shop</th><th>Email</th><th>Plan</th><th>Subscription</th><th>Users</th><th>Status</th><th></th></tr></thead><tbody>{shops.map(shop => <tr key={shop.id} className="border-b"><td className="py-2 font-medium">{shop.name}</td><td>{shop.email || '—'}</td><td>{shop.currentPlanName || '—'}</td><td>{shop.subscriptionStatus}</td><td>{shop.totalUsers}</td><td>{shop.status}</td><td className="text-right"><button disabled={busyId === shop.id} onClick={() => void changeShopStatus(shop)} className="text-indigo-600 font-semibold disabled:opacity-50">{shop.status.toLowerCase() === 'suspended' ? 'Activate' : 'Suspend'}</button></td></tr>)}</tbody></table></div>{shops.length === 0 && <p className="text-sm text-slate-500 mt-3">No shops found.</p>}</section>
    <section className="rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5"><h2 className="font-bold mb-3">Plans</h2><div className="grid md:grid-cols-3 gap-3">{plans.map(plan => <div key={plan.id} className="rounded-lg border border-slate-200 dark:border-slate-700 p-4"><p className="font-bold">{plan.name}</p><p className="text-sm">{money(plan.monthlyPrice)} / month</p><p className="text-xs text-slate-500">{plan.activeSubscribers} active subscribers · {plan.isActive ? 'Active' : 'Inactive'}</p></div>)}</div>{plans.length === 0 && <p className="text-sm text-slate-500">No plans found.</p>}</section>
    <section className="rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5"><h2 className="font-bold mb-3">Recent audit events</h2>{logs.map(log => <div key={log.id} className="border-t border-slate-100 dark:border-slate-800 py-2 text-sm"><span className="font-semibold">{log.action}</span> · {log.entityType} · {log.businessName || 'Platform'} · {log.userName || 'System'}<span className="text-slate-500 ml-2">{new Date(log.timestamp).toLocaleString('en-IN')}</span></div>)}{logs.length === 0 && <p className="text-sm text-slate-500">No audit events found.</p>}</section>
  </div>;
};
