import React, { useEffect, useState, useCallback } from 'react';
import {
  LayoutDashboard, Building2, Globe, ShieldAlert, FileText,
  Receipt, Users, RefreshCw, Download, ChevronRight, ChevronLeft,
  MoreVertical, CheckCircle2, XCircle, Clock,
  Search, Eye, Settings2, Bell, Database, BarChart3,
  ArrowUpRight, ArrowDownRight, DollarSign, Shield, Gauge,
  Circle, TrendingUp, Activity, ChevronDown, Cpu,
  AlertCircle, Plus, Gift, Trash2, Edit2, X, UserPlus, Key, Package
} from 'lucide-react';
import { clsx } from 'clsx';
import { api } from '../../services/apiClient';
import { PlanPermitManager } from './PlanPermitManagerPage';
import {
  AreaChart, Area, BarChart, Bar,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer
} from 'recharts';

/* ─── Types ──────────────────────────────────────────────────── */
type DashData = {
  totalShops: number; activeShops: number; suspendedShops: number;
  totalUsers: number; activeTrials: number; activeSubscriptions: number;
  totalRevenueThisMonth: number; totalRevenueAllTime: number;
};
type Shop = {
  id: number; name: string; email?: string; status: string;
  totalUsers: number; currentPlanName?: string; subscriptionStatus: string;
  phone?: string; address?: string; city?: string; state?: string;
};
type Plan = {
  id: number; name: string; monthlyPrice: number; yearlyPrice?: number;
  activeSubscribers: number; isActive: boolean;
  maxUsers: number; maxProducts: number; trialDays: number;
  maxBranches?: number; maxInvoicesPerMonth?: number; description?: string;
  hasWhatsAppIntegration?: boolean; hasEmailIntegration?: boolean;
  hasReports?: boolean; hasMultiCurrency?: boolean;
  hasInventoryManagement?: boolean; hasDeviceIntegration?: boolean;
  hasApiAccess?: boolean; hasSmsIntegration?: boolean;
};
type AuditLog = {
  id: number; businessName?: string; userName?: string;
  action: string; entityType: string; timestamp: string;
};
type AdminUser = {
  id: number; fullName: string; email: string; phone?: string;
  role: string; tenantId?: number; businessName?: string;
  isActive: boolean; lastLoginAt?: string; createdAt: string;
};

/* ─── Empty defaults ─────────────────────────────────────────── */
const EMPTY_DASH: DashData = {
  totalShops: 0, activeShops: 0, suspendedShops: 0,
  totalUsers: 0, activeTrials: 0, activeSubscriptions: 0,
  totalRevenueThisMonth: 0, totalRevenueAllTime: 0,
};

/* ─── Live Clock ─────────────────────────────────────────────── */
function useClock(): string {
  const [t, setT] = useState(() => new Date().toLocaleTimeString('en-GB'));
  useEffect(() => {
    const id = setInterval(() => setT(new Date().toLocaleTimeString('en-GB')), 1000);
    return () => clearInterval(id);
  }, []);
  return t;
}

function useSecondsTick(since: Date): number {
  const [secs, setSecs] = useState(() => Math.floor((Date.now() - since.getTime()) / 1000));
  useEffect(() => {
    const id = setInterval(() => setSecs(Math.floor((Date.now() - since.getTime()) / 1000)), 1000);
    return () => clearInterval(id);
  }, [since]);
  return secs;
}

/* ─── Helpers ────────────────────────────────────────────────── */
const fmt = (n: number) => (n ?? 0).toLocaleString('en-IN');
const money = (n: number) => '₹' + fmt(n ?? 0);
const pct = (a: number, b: number) => b ? Math.round((a / b) * 100) : 0;

/* ─── Toast ──────────────────────────────────────────────────── */
type ToastState = { msg: string; ok: boolean } | null;
const Toast: React.FC<{ toast: ToastState }> = ({ toast }) => {
  if (!toast) return null;
  return (
    <div className={clsx(
      'fixed bottom-5 right-5 z-50 px-4 py-3 rounded-xl shadow-xl text-white text-[12px] font-semibold flex items-center gap-2 transition-all',
      toast.ok ? 'bg-emerald-600' : 'bg-rose-600'
    )}>
      {toast.ok ? <CheckCircle2 className="w-4 h-4" /> : <AlertCircle className="w-4 h-4" />}
      {toast.msg}
    </div>
  );
};

/* ─── Confirm Dialog ─────────────────────────────────────────── */
const ConfirmDialog: React.FC<{
  message: string; onConfirm: () => void; onCancel: () => void; busy?: boolean;
}> = ({ message, onConfirm, onCancel, busy }) => (
  <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
    <div className="bg-white rounded-xl p-6 shadow-2xl max-w-sm w-full mx-4">
      <p className="text-[13px] font-semibold text-gray-800 mb-5">{message}</p>
      <div className="flex gap-3 justify-end">
        <button onClick={onCancel} className="text-[12px] px-4 py-2 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50 font-medium">Cancel</button>
        <button onClick={onConfirm} disabled={busy} className="text-[12px] px-4 py-2 rounded-lg bg-rose-600 hover:bg-rose-700 text-white font-semibold disabled:opacity-50">
          {busy ? 'Deleting…' : 'Delete'}
        </button>
      </div>
    </div>
  </div>
);

/* ─── KPI Card ───────────────────────────────────────────────── */
const KpiCard: React.FC<{
  label: string; value: string | number; sub?: string; trend?: number;
  badge?: string; badgeColor?: string; icon?: React.ReactNode;
}> = ({ label, value, sub, trend, badge, badgeColor = 'bg-blue-100 text-blue-700', icon }) => (
  <div className="bg-white border border-gray-200 rounded-xl p-4 flex flex-col gap-1 shadow-sm hover:shadow-md transition-shadow">
    <div className="flex items-center justify-between mb-1">
      <p className="text-[10px] font-bold uppercase tracking-widest text-gray-400">{label}</p>
      {badge && <span className={clsx('text-[9px] font-bold px-1.5 py-0.5 rounded uppercase tracking-wide', badgeColor)}>{badge}</span>}
      {icon && !badge && <span className="text-gray-300">{icon}</span>}
    </div>
    <p className="text-xl font-black text-gray-900 leading-none">{value}</p>
    {(sub || trend !== undefined) && (
      <div className="flex items-center gap-1.5 mt-0.5">
        {trend !== undefined && (
          <span className={clsx('flex items-center text-[10px] font-bold', trend >= 0 ? 'text-emerald-600' : 'text-rose-600')}>
            {trend >= 0 ? <ArrowUpRight className="w-3 h-3" /> : <ArrowDownRight className="w-3 h-3" />}
            {Math.abs(trend)}%
          </span>
        )}
        {sub && <span className="text-[10px] text-gray-400">{sub}</span>}
      </div>
    )}
  </div>
);

/* ─── Avatar Chip ────────────────────────────────────────────── */
const Chip: React.FC<{ initials: string; color: string }> = ({ initials, color }) => (
  <div className={clsx('w-7 h-7 rounded-lg flex items-center justify-center text-white text-[10px] font-bold shrink-0', color)}>{initials}</div>
);
const COLORS = ['bg-indigo-600', 'bg-violet-600', 'bg-blue-600', 'bg-rose-600', 'bg-amber-600', 'bg-emerald-600'];

/* ─── Status Badge ───────────────────────────────────────────── */
const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  const s = status.toLowerCase();
  const map: Record<string, string> = {
    active: 'bg-emerald-50 text-emerald-700 border border-emerald-200',
    trial: 'bg-blue-50 text-blue-700 border border-blue-200',
    overdue: 'bg-rose-50 text-rose-700 border border-rose-200',
    suspended: 'bg-gray-100 text-gray-600 border border-gray-200',
    cancelled: 'bg-gray-100 text-gray-500 border border-gray-200',
    lifetimefree: 'bg-violet-50 text-violet-700 border border-violet-200',
    monthlyfree: 'bg-sky-50 text-sky-700 border border-sky-200',
    expired: 'bg-orange-50 text-orange-700 border border-orange-200',
    none: 'bg-gray-100 text-gray-500 border border-gray-200',
  };
  return (
    <span className={clsx('inline-flex items-center gap-1 text-[10px] font-semibold px-2 py-0.5 rounded-full', map[s] ?? 'bg-gray-100 text-gray-600 border border-gray-200')}>
      <Circle className="w-1.5 h-1.5 fill-current" />{status}
    </span>
  );
};

/* ════════════════════════════════════════════════════════════════
   VIEW 1 – Platform Overview
════════════════════════════════════════════════════════════════ */
const PlatformOverview: React.FC<{
  dash: DashData; shops: Shop[]; logs: AuditLog[];
  lastRefreshed: Date; isRefreshing: boolean; onRefresh: () => void;
}> = ({ dash, shops, logs, lastRefreshed, isRefreshing, onRefresh }) => {
  const secsAgo = useSecondsTick(lastRefreshed);
  const clock = useClock();

  return (
    <div className="space-y-5">
      <div className="flex items-center gap-2 flex-wrap">
        <span className="inline-flex items-center gap-1.5 text-[10px] font-bold text-indigo-700 bg-indigo-50 border border-indigo-200 px-2.5 py-1 rounded-full">
          <Shield className="w-3 h-3" /> SUPERADMIN VIEW
        </span>
        <span className="text-[10px] text-gray-500">Global Platform Operations · Live DB</span>
        <div className="ml-auto flex gap-2 items-center">
          <span className="text-[10px] font-mono text-gray-400">{clock}</span>
          <button onClick={onRefresh} disabled={isRefreshing} className="flex items-center gap-1 text-[10px] text-gray-500 hover:text-indigo-600 transition-colors bg-white border border-gray-200 rounded px-2 py-1">
            <RefreshCw className={clsx('w-3 h-3', isRefreshing && 'animate-spin')} />
            {isRefreshing ? 'Syncing…' : `${secsAgo}s ago`}
          </button>
        </div>
      </div>

      {/* Live KPI bar */}
      <div className="bg-indigo-950 rounded-xl px-4 py-2.5 flex items-center gap-6 text-white overflow-x-auto">
        <span className="text-[9px] font-bold text-indigo-300 uppercase tracking-widest shrink-0">● LIVE</span>
        {[
          { label: 'Total Shops', value: fmt(dash.totalShops) },
          { label: 'Active', value: fmt(dash.activeShops) },
          { label: 'Suspended', value: fmt(dash.suspendedShops) },
          { label: 'Platform Users', value: fmt(dash.totalUsers) },
          { label: 'Trials', value: fmt(dash.activeTrials) },
          { label: 'Active Subscriptions', value: fmt(dash.activeSubscriptions) },
        ].map(({ label, value }) => (
          <div key={label} className="shrink-0">
            <p className="text-[9px] text-indigo-400 uppercase tracking-wider">{label}</p>
            <p className="text-[13px] font-black">{value}</p>
          </div>
        ))}
        <div className="ml-auto shrink-0 text-right">
          <p className="text-[9px] text-indigo-400 uppercase tracking-wider">Source</p>
          <p className="text-[10px] font-semibold text-emerald-300">● Live API</p>
        </div>
      </div>

      {/* KPIs */}
      <div className="grid grid-cols-4 gap-3">
        <KpiCard label="Total Shops" value={fmt(dash.totalShops)} sub="registered" icon={<Building2 className="w-4 h-4" />} />
        <KpiCard label="Active Shops" value={fmt(dash.activeShops)} badge="Online" badgeColor="bg-emerald-100 text-emerald-700" />
        <KpiCard label="Active Subscriptions" value={fmt(dash.activeSubscriptions)} icon={<TrendingUp className="w-4 h-4" />} />
        <KpiCard label="Revenue This Month" value={money(dash.totalRevenueThisMonth)} sub={`All time: ${money(dash.totalRevenueAllTime)}`} icon={<DollarSign className="w-4 h-4" />} />
      </div>

      {/* Top Shops Table */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
          <p className="text-[11px] font-bold text-gray-800">Recent Shops <span className="text-[9px] bg-emerald-100 text-emerald-700 border border-emerald-200 px-1.5 py-0.5 rounded font-bold ml-1">● LIVE DB</span></p>
          <span className="text-[10px] text-gray-400">{shops.length} shops loaded</span>
        </div>
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              {['Shop', 'Plan', 'Users', 'Subscription Status', 'Status'].map(h => (
                <th key={h} className="text-left py-2 px-3 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {shops.slice(0, 8).map((shop, i) => (
              <tr key={shop.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                <td className="py-2.5 px-3">
                  <div className="flex items-center gap-2">
                    <Chip initials={shop.name.substring(0, 2).toUpperCase()} color={COLORS[i % COLORS.length]} />
                    <div>
                      <p className="font-semibold text-gray-800">{shop.name}</p>
                      <p className="text-gray-400 text-[10px]">{shop.email ?? '—'}</p>
                    </div>
                  </div>
                </td>
                <td className="px-3"><span className="text-[10px] font-semibold bg-indigo-50 text-indigo-700 border border-indigo-100 px-2 py-0.5 rounded">{shop.currentPlanName ?? 'None'}</span></td>
                <td className="px-3 text-gray-700">{shop.totalUsers}</td>
                <td className="px-3"><StatusBadge status={shop.subscriptionStatus} /></td>
                <td className="px-3"><StatusBadge status={shop.status} /></td>
              </tr>
            ))}
            {shops.length === 0 && (
              <tr><td colSpan={5} className="py-8 text-center text-[11px] text-gray-400">No shops found in database.</td></tr>
            )}
          </tbody>
        </table>
      </div>

      {/* Recent Audit Logs */}
      {logs.length > 0 && (
        <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
          <div className="px-4 py-3 border-b border-gray-100">
            <p className="text-[11px] font-bold text-gray-800">Recent Audit Events</p>
          </div>
          <div className="divide-y divide-gray-50">
            {logs.slice(0, 5).map(log => (
              <div key={log.id} className="px-4 py-3 hover:bg-gray-50/50 transition-colors flex items-center gap-4">
                <Activity className="w-4 h-4 text-gray-300 shrink-0" />
                <div className="flex-1 min-w-0 flex items-center gap-2 flex-wrap">
                  <span className="text-[10px] font-mono font-bold px-2 py-0.5 rounded border bg-gray-100 text-gray-600 border-gray-200">{log.action}</span>
                  <span className="text-[11px] text-gray-500">{log.entityType}</span>
                  {log.businessName && <span className="text-[11px] font-semibold text-gray-700">· {log.businessName}</span>}
                  {log.userName && <span className="text-[11px] text-gray-400">by {log.userName}</span>}
                </div>
                <span className="text-[10px] text-gray-400 shrink-0">{new Date(log.timestamp).toLocaleString('en-IN', { dateStyle: 'short', timeStyle: 'short' })}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

/* ════════════════════════════════════════════════════════════════
   VIEW 2 – Tenant Registry (Shops)
════════════════════════════════════════════════════════════════ */
const TenantRegistry: React.FC<{
  shops: Shop[];
  onToggleShop: (s: Shop) => void;
  onDeleteShop: (s: Shop) => void;
  busyId: number | null;
  onRefresh: () => void;
}> = ({ shops, onToggleShop, onDeleteShop, busyId, onRefresh }) => {
  const [search, setSearch] = useState('');
  const [confirmShop, setConfirmShop] = useState<Shop | null>(null);
  const [deleting, setDeleting] = useState(false);
  const data = shops.filter(s =>
    !search || s.name.toLowerCase().includes(search.toLowerCase()) || (s.email ?? '').toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="space-y-5">
      {confirmShop && (
        <ConfirmDialog
          message={`Delete shop "${confirmShop.name}"? This action cannot be undone.`}
          busy={deleting}
          onCancel={() => setConfirmShop(null)}
          onConfirm={async () => {
            setDeleting(true);
            await onDeleteShop(confirmShop);
            setDeleting(false);
            setConfirmShop(null);
          }}
        />
      )}

      <div className="flex items-start justify-between">
        <div>
          <p className="text-[10px] text-indigo-600 font-bold uppercase tracking-widest">SUPERADMIN CONTROL LAYER</p>
          <h1 className="text-2xl font-black text-gray-900 mt-1">Tenant / Shop Registry</h1>
          <p className="text-[11px] text-gray-500 mt-1.5">{shops.length} shops registered in database.</p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={onRefresh} className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-2 rounded-lg font-medium flex items-center gap-1.5 shadow-sm">
            <RefreshCw className="w-3 h-3" /> Refresh
          </button>
        </div>
      </div>

      <div className="grid grid-cols-4 gap-3">
        <KpiCard label="Total Shops" value={shops.length} icon={<Building2 className="w-4 h-4" />} />
        <KpiCard label="Active" value={shops.filter(s => s.status === 'Active').length} badge="Live" badgeColor="bg-emerald-100 text-emerald-700" />
        <KpiCard label="Suspended" value={shops.filter(s => s.status === 'Suspended').length} badge="Blocked" badgeColor="bg-rose-100 text-rose-700" />
        <KpiCard label="With Active Plan" value={shops.filter(s => s.subscriptionStatus === 'Active').length} icon={<TrendingUp className="w-4 h-4" />} />
      </div>

      <div className="relative max-w-xs">
        <Search className="w-3 h-3 absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
        <input value={search} onChange={e => setSearch(e.target.value)} className="bg-white border border-gray-200 rounded-lg pl-7 pr-3 py-1.5 text-[11px] text-gray-700 placeholder-gray-400 focus:outline-none focus:border-indigo-400 w-full shadow-sm" placeholder="Search shop name or email…" />
      </div>

      <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              {['ID', 'Shop', 'Plan', 'Users', 'Sub Status', 'Status', 'Actions'].map(h => (
                <th key={h} className="text-left py-2 px-3 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.map((shop, i) => (
              <tr key={shop.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                <td className="py-2.5 px-3 font-mono text-gray-400 text-[10px]">#{shop.id}</td>
                <td className="px-3">
                  <div className="flex items-center gap-2">
                    <Chip initials={shop.name.substring(0, 2).toUpperCase()} color={COLORS[i % COLORS.length]} />
                    <div>
                      <p className="font-semibold text-gray-800">{shop.name}</p>
                      <p className="text-gray-400 text-[10px]">{shop.email ?? '—'}</p>
                    </div>
                  </div>
                </td>
                <td className="px-3"><span className="text-[10px] font-semibold bg-indigo-50 text-indigo-700 border border-indigo-100 px-2 py-0.5 rounded">{shop.currentPlanName ?? 'None'}</span></td>
                <td className="px-3 text-gray-700">{shop.totalUsers}</td>
                <td className="px-3"><StatusBadge status={shop.subscriptionStatus} /></td>
                <td className="px-3"><StatusBadge status={shop.status} /></td>
                <td className="px-3">
                  <div className="flex items-center gap-2">
                    <button onClick={() => onToggleShop(shop)} disabled={busyId === shop.id}
                      className={clsx('text-[10px] font-semibold transition-colors px-2 py-1 rounded', shop.status === 'Active' ? 'bg-rose-50 text-rose-600 hover:bg-rose-100' : 'bg-emerald-50 text-emerald-600 hover:bg-emerald-100')}>
                      {busyId === shop.id ? '…' : shop.status === 'Active' ? 'Suspend' : 'Activate'}
                    </button>
                    <button onClick={() => setConfirmShop(shop)} className="p-1 text-gray-300 hover:text-rose-500 transition-colors rounded">
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {data.length === 0 && (
              <tr><td colSpan={7} className="py-8 text-center text-[11px] text-gray-400">{search ? 'No shops match your search.' : 'No shops registered yet.'}</td></tr>
            )}
          </tbody>
        </table>
        <div className="px-4 py-2.5 text-[10px] text-gray-400 border-t border-gray-100">
          Showing {data.length} of {shops.length} shops
        </div>
      </div>
    </div>
  );
};

/* ════════════════════════════════════════════════════════════════
   VIEW 3 – Global Plans (with Create / Edit / Toggle)
════════════════════════════════════════════════════════════════ */
type PlanFormState = {
  name: string; description: string; monthlyPrice: string; yearlyPrice: string;
  maxUsers: string; maxProducts: string; maxBranches: string; trialDays: string;
  hasWhatsAppIntegration: boolean; hasEmailIntegration: boolean; hasReports: boolean;
  hasMultiCurrency: boolean; hasInventoryManagement: boolean; hasDeviceIntegration: boolean;
  hasApiAccess: boolean; hasSmsIntegration: boolean; isActive: boolean; isPopular: boolean; sortOrder: string;
};
const defaultPlanForm = (): PlanFormState => ({
  name: '', description: '', monthlyPrice: '0', yearlyPrice: '',
  maxUsers: '5', maxProducts: '100', maxBranches: '1', trialDays: '30',
  hasWhatsAppIntegration: false, hasEmailIntegration: true, hasReports: true,
  hasMultiCurrency: false, hasInventoryManagement: true, hasDeviceIntegration: false,
  hasApiAccess: false, hasSmsIntegration: false, isActive: true, isPopular: false, sortOrder: '0',
});

const GlobalPlans: React.FC<{ plans: Plan[]; onRefresh: () => void; setToast: (t: ToastState) => void }> = ({ plans, onRefresh, setToast }) => {
  const [showModal, setShowModal] = useState(false);
  const [editPlan, setEditPlan] = useState<Plan | null>(null);
  const [form, setForm] = useState<PlanFormState>(defaultPlanForm());
  const [saving, setSaving] = useState(false);
  const [confirmPlan, setConfirmPlan] = useState<Plan | null>(null);
  const [deleting, setDeleting] = useState(false);

  const openCreate = () => { setEditPlan(null); setForm(defaultPlanForm()); setShowModal(true); };
  const openEdit = (p: Plan) => {
    setEditPlan(p);
    setForm({
      name: p.name, description: p.description ?? '', monthlyPrice: String(p.monthlyPrice),
      yearlyPrice: p.yearlyPrice ? String(p.yearlyPrice) : '',
      maxUsers: String(p.maxUsers), maxProducts: String(p.maxProducts),
      maxBranches: String(p.maxBranches ?? 1), trialDays: String(p.trialDays),
      hasWhatsAppIntegration: p.hasWhatsAppIntegration ?? false,
      hasEmailIntegration: p.hasEmailIntegration ?? true,
      hasReports: p.hasReports ?? true, hasMultiCurrency: p.hasMultiCurrency ?? false,
      hasInventoryManagement: p.hasInventoryManagement ?? true,
      hasDeviceIntegration: p.hasDeviceIntegration ?? false,
      hasApiAccess: p.hasApiAccess ?? false, hasSmsIntegration: p.hasSmsIntegration ?? false,
      isActive: p.isActive, isPopular: false, sortOrder: '0',
    });
    setShowModal(true);
  };

  const toggleActive = async (p: Plan) => {
    try {
      await api.request(`/api/admin/plans/${p.id}`, {
        method: 'PUT',
        body: JSON.stringify({ ...p, isActive: !p.isActive, name: p.name, monthlyPrice: p.monthlyPrice, maxUsers: p.maxUsers, maxProducts: p.maxProducts, trialDays: p.trialDays }),
      });
      setToast({ msg: `Plan "${p.name}" ${!p.isActive ? 'activated' : 'deactivated'}`, ok: true });
      onRefresh();
    } catch {
      setToast({ msg: 'Failed to update plan status', ok: false });
    }
  };

  const savePlan = async () => {
    setSaving(true);
    try {
      const payload = {
        name: form.name, description: form.description,
        monthlyPrice: parseFloat(form.monthlyPrice) || 0,
        yearlyPrice: form.yearlyPrice ? parseFloat(form.yearlyPrice) : null,
        maxUsers: parseInt(form.maxUsers) || 5,
        maxProducts: parseInt(form.maxProducts) || 100,
        maxBranches: parseInt(form.maxBranches) || 1,
        maxInvoicesPerMonth: 500,
        trialDays: parseInt(form.trialDays) || 30,
        hasWhatsAppIntegration: form.hasWhatsAppIntegration,
        hasEmailIntegration: form.hasEmailIntegration,
        hasReports: form.hasReports, hasMultiCurrency: form.hasMultiCurrency,
        hasInventoryManagement: form.hasInventoryManagement,
        hasDeviceIntegration: form.hasDeviceIntegration,
        hasApiAccess: form.hasApiAccess, hasSmsIntegration: form.hasSmsIntegration,
        isActive: form.isActive, isPopular: form.isPopular,
        sortOrder: parseInt(form.sortOrder) || 0,
      };
      if (editPlan) {
        await api.request(`/api/admin/plans/${editPlan.id}`, { method: 'PUT', body: JSON.stringify(payload) });
        setToast({ msg: `Plan "${form.name}" updated`, ok: true });
      } else {
        await api.request('/api/admin/plans', { method: 'POST', body: JSON.stringify(payload) });
        setToast({ msg: `Plan "${form.name}" created`, ok: true });
      }
      setShowModal(false);
      onRefresh();
    } catch {
      setToast({ msg: 'Failed to save plan', ok: false });
    } finally { setSaving(false); }
  };

  const deletePlan = async (p: Plan) => {
    setDeleting(true);
    try {
      await api.request(`/api/admin/plans/${p.id}`, { method: 'DELETE' });
      setToast({ msg: `Plan "${p.name}" deleted`, ok: true });
      onRefresh();
    } catch {
      setToast({ msg: 'Failed to delete plan', ok: false });
    } finally { setDeleting(false); setConfirmPlan(null); }
  };

  const total = plans.reduce((s, p) => s + p.activeSubscribers, 0);

  return (
    <div className="space-y-5">
      {confirmPlan && (
        <ConfirmDialog
          message={`Delete plan "${confirmPlan.name}"? This will deactivate it.`}
          busy={deleting}
          onCancel={() => setConfirmPlan(null)}
          onConfirm={() => deletePlan(confirmPlan)}
        />
      )}

      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
            <div className="px-5 py-4 border-b border-gray-100 flex items-center justify-between">
              <p className="font-bold text-gray-800 text-[13px]">{editPlan ? `Edit Plan: ${editPlan.name}` : 'Create New Plan'}</p>
              <button onClick={() => setShowModal(false)} className="text-gray-400 hover:text-gray-600"><X className="w-4 h-4" /></button>
            </div>
            <div className="p-5 space-y-3">
              {[
                { label: 'Plan Name *', key: 'name', type: 'text' },
                { label: 'Description', key: 'description', type: 'text' },
                { label: 'Monthly Price (₹)', key: 'monthlyPrice', type: 'number' },
                { label: 'Yearly Price (₹)', key: 'yearlyPrice', type: 'number' },
                { label: 'Max Users', key: 'maxUsers', type: 'number' },
                { label: 'Max Products', key: 'maxProducts', type: 'number' },
                { label: 'Trial Days', key: 'trialDays', type: 'number' },
              ].map(({ label, key, type }) => (
                <div key={key}>
                  <label className="text-[11px] font-semibold text-gray-600 mb-1 block">{label}</label>
                  <input type={type} value={(form as any)[key]} onChange={e => setForm(f => ({ ...f, [key]: e.target.value }))}
                    className="w-full border border-gray-200 rounded-lg px-3 py-2 text-[12px] focus:outline-none focus:border-indigo-400" />
                </div>
              ))}
              <div className="grid grid-cols-2 gap-2 pt-2">
                {[
                  { label: 'WhatsApp Integration', key: 'hasWhatsAppIntegration' },
                  { label: 'Email Integration', key: 'hasEmailIntegration' },
                  { label: 'Reports', key: 'hasReports' },
                  { label: 'Multi-Currency', key: 'hasMultiCurrency' },
                  { label: 'Inventory Management', key: 'hasInventoryManagement' },
                  { label: 'Device Integration', key: 'hasDeviceIntegration' },
                  { label: 'API Access', key: 'hasApiAccess' },
                  { label: 'SMS Integration', key: 'hasSmsIntegration' },
                  { label: 'Active', key: 'isActive' },
                ].map(({ label, key }) => (
                  <label key={key} className="flex items-center gap-2 text-[11px] text-gray-700 cursor-pointer">
                    <input type="checkbox" checked={(form as any)[key]} onChange={e => setForm(f => ({ ...f, [key]: e.target.checked }))} className="accent-indigo-600 w-3.5 h-3.5" />
                    {label}
                  </label>
                ))}
              </div>
            </div>
            <div className="px-5 py-4 border-t border-gray-100 flex justify-end gap-3">
              <button onClick={() => setShowModal(false)} className="text-[12px] px-4 py-2 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50">Cancel</button>
              <button onClick={savePlan} disabled={saving || !form.name}
                className="text-[12px] px-4 py-2 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white font-semibold disabled:opacity-50">
                {saving ? 'Saving…' : editPlan ? 'Update Plan' : 'Create Plan'}
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-black text-gray-900">Global Plans & Metering</h1>
          <p className="text-[11px] text-gray-400 mt-0.5">{plans.length} plans in database</p>
        </div>
        <button onClick={openCreate} className="text-[11px] bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg font-semibold flex items-center gap-1.5 shadow-sm">
          <Plus className="w-3 h-3" /> Create New Plan
        </button>
      </div>

      {plans.length === 0 ? (
        <div className="bg-white border border-gray-200 rounded-xl p-12 text-center">
          <Globe className="w-10 h-10 text-gray-200 mx-auto mb-3" />
          <p className="text-gray-500 font-semibold">No plans found</p>
          <p className="text-gray-400 text-[11px] mt-1">Create your first subscription plan to get started.</p>
        </div>
      ) : (
        <div className="grid grid-cols-4 gap-4">
          {plans.map((plan, i) => {
            const cardColors = ['border-gray-200 bg-white', 'border-indigo-200 bg-indigo-50', 'border-violet-200 bg-violet-50', 'border-amber-200 bg-amber-50'];
            const badgeColors = ['bg-gray-100 text-gray-700', 'bg-indigo-100 text-indigo-700', 'bg-violet-100 text-violet-700', 'bg-amber-100 text-amber-700'];
            return (
              <div key={plan.id} className={clsx('border rounded-xl p-4 shadow-sm hover:shadow-md transition-shadow', cardColors[i % 4])}>
                <div className="flex items-center justify-between mb-2">
                  <span className={clsx('text-[10px] font-bold px-2 py-0.5 rounded uppercase', badgeColors[i % 4])}>{plan.name}</span>
                  {plan.isActive ? <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" /> : <XCircle className="w-3.5 h-3.5 text-gray-300" />}
                </div>
                <p className="text-[9px] font-mono text-gray-400 mb-1">ID: {plan.id}</p>
                <p className="text-2xl font-black text-gray-900">{plan.monthlyPrice === 0 ? 'Free' : `₹${plan.monthlyPrice}`}{plan.monthlyPrice > 0 && <span className="text-sm font-normal text-gray-400">/mo</span>}</p>
                {plan.yearlyPrice ? <p className="text-[10px] text-gray-400">₹{plan.yearlyPrice}/yr</p> : <p className="text-[10px] text-gray-400">No yearly plan</p>}
                <div className="mt-3">
                  <div className="flex justify-between mb-1"><span className="text-[10px] text-gray-500">Active Subscribers</span><span className="text-[10px] font-bold text-gray-800">{plan.activeSubscribers}</span></div>
                  <div className="h-1.5 bg-gray-200 rounded-full overflow-hidden"><div className="h-full bg-indigo-500 rounded-full" style={{ width: `${pct(plan.activeSubscribers, Math.max(total, 1))}%` }} /></div>
                </div>
                <div className="mt-3 space-y-1 pt-3 border-t border-gray-100">
                  {[['Max Users', plan.maxUsers >= 999 ? 'Unlimited' : plan.maxUsers], ['Max Products', plan.maxProducts >= 999999 ? 'Unlimited' : fmt(plan.maxProducts)], ['Trial Days', `${plan.trialDays}d`]].map(([l, v]) => (
                    <div key={String(l)} className="flex justify-between text-[10px]"><span className="text-gray-400">{l}</span><span className="font-semibold text-gray-700">{v}</span></div>
                  ))}
                </div>
                <div className="mt-3 flex gap-2">
                  <button onClick={() => openEdit(plan)} className="flex-1 text-[10px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 py-1.5 rounded font-semibold flex items-center justify-center gap-1 transition-colors">
                    <Edit2 className="w-3 h-3" /> Edit
                  </button>
                  <button onClick={() => toggleActive(plan)} className={clsx('flex-1 text-[10px] py-1.5 rounded font-semibold transition-colors', plan.isActive ? 'bg-rose-50 hover:bg-rose-100 text-rose-700 border border-rose-200' : 'bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border border-emerald-200')}>
                    {plan.isActive ? 'Deactivate' : 'Activate'}
                  </button>
                  <button onClick={() => setConfirmPlan(plan)} className="p-1.5 text-gray-300 hover:text-rose-500 transition-colors rounded border border-gray-200 hover:border-rose-200">
                    <Trash2 className="w-3 h-3" />
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Feature Matrix */}
      {plans.length > 0 && (
        <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
          <div className="px-4 py-3 border-b border-gray-100"><p className="text-[11px] font-bold text-gray-800">Feature Matrix</p></div>
          <table className="w-full text-[11px]">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/70">
                <th className="text-left py-2 px-4 text-[10px] font-semibold uppercase tracking-wider text-gray-400">Feature</th>
                {plans.map(p => <th key={p.id} className="text-center py-2 px-4 text-[10px] font-bold text-gray-600">{p.name}</th>)}
              </tr>
            </thead>
            <tbody>
              {([
                ['WhatsApp Integration', (p: Plan) => p.hasWhatsAppIntegration ?? false],
                ['SMS Integration', (p: Plan) => p.hasSmsIntegration ?? false],
                ['Email Integration', (p: Plan) => p.hasEmailIntegration ?? true],
                ['Advanced Reports', (p: Plan) => p.hasReports ?? false],
                ['Multi-Currency', (p: Plan) => p.hasMultiCurrency ?? false],
                ['Inventory Management', (p: Plan) => p.hasInventoryManagement ?? true],
                ['Device Integration', (p: Plan) => p.hasDeviceIntegration ?? false],
                ['API Access', (p: Plan) => p.hasApiAccess ?? false],
              ] as [string, (p: Plan) => boolean][]).map(([feat, getter]) => (
                <tr key={feat} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                  <td className="py-2.5 px-4 text-gray-600">{feat}</td>
                  {plans.map(p => (
                    <td key={p.id} className="py-2.5 px-4 text-center">
                      {getter(p) ? <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500 mx-auto" /> : <span className="text-gray-200">—</span>}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

/* ════════════════════════════════════════════════════════════════
   VIEW 4 – All Users
════════════════════════════════════════════════════════════════ */
const AllUsers: React.FC<{ onRefresh: () => void; setToast: (t: ToastState) => void }> = ({ onRefresh, setToast }) => {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [busyId, setBusyId] = useState<number | null>(null);
  const [confirmUser, setConfirmUser] = useState<AdminUser | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [resetUser, setResetUser] = useState<AdminUser | null>(null);
  const [newPwd, setNewPwd] = useState('');
  const [resetting, setResetting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const data = await api.request<AdminUser[]>('/api/admin/users');
      setUsers(data ?? []);
    } catch { setUsers([]); } finally { setLoading(false); }
  }, []);

  useEffect(() => { void load(); }, [load]);

  const toggleActive = async (u: AdminUser) => {
    setBusyId(u.id);
    try {
      const action = u.isActive ? 'deactivate' : 'activate';
      await api.request(`/api/admin/users/${u.id}/${action}`, { method: 'POST' });
      setToast({ msg: `User ${u.isActive ? 'deactivated' : 'activated'}`, ok: true });
      await load();
    } catch { setToast({ msg: 'Failed to update user', ok: false }); } finally { setBusyId(null); }
  };

  const deleteUser = async () => {
    if (!confirmUser) return;
    setDeleting(true);
    try {
      await api.request(`/api/admin/users/${confirmUser.id}`, { method: 'DELETE' });
      setToast({ msg: `User "${confirmUser.fullName}" deleted`, ok: true });
      setConfirmUser(null);
      await load();
    } catch { setToast({ msg: 'Failed to delete user', ok: false }); } finally { setDeleting(false); }
  };

  const resetPassword = async () => {
    if (!resetUser || !newPwd) return;
    setResetting(true);
    try {
      await api.request(`/api/admin/users/${resetUser.id}/reset-password?newPassword=${encodeURIComponent(newPwd)}`, { method: 'POST' });
      setToast({ msg: `Password reset for ${resetUser.fullName}`, ok: true });
      setResetUser(null); setNewPwd('');
    } catch { setToast({ msg: 'Failed to reset password', ok: false }); } finally { setResetting(false); }
  };

  const data = users.filter(u => !search || u.fullName.toLowerCase().includes(search.toLowerCase()) || u.email.toLowerCase().includes(search.toLowerCase()));

  return (
    <div className="space-y-5">
      {confirmUser && <ConfirmDialog message={`Delete user "${confirmUser.fullName}"?`} busy={deleting} onCancel={() => setConfirmUser(null)} onConfirm={deleteUser} />}

      {resetUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-xl p-6 shadow-2xl max-w-sm w-full mx-4">
            <p className="font-bold text-gray-800 text-[13px] mb-4">Reset Password for {resetUser.fullName}</p>
            <input type="password" value={newPwd} onChange={e => setNewPwd(e.target.value)} placeholder="New password" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-[12px] focus:outline-none focus:border-indigo-400 mb-4" />
            <div className="flex gap-3 justify-end">
              <button onClick={() => { setResetUser(null); setNewPwd(''); }} className="text-[12px] px-4 py-2 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50">Cancel</button>
              <button onClick={resetPassword} disabled={resetting || !newPwd} className="text-[12px] px-4 py-2 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white font-semibold disabled:opacity-50">
                {resetting ? 'Resetting…' : 'Reset Password'}
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-black text-gray-900">All Users</h1>
          <p className="text-[11px] text-gray-400 mt-0.5">{users.length} users (excluding SuperAdmins)</p>
        </div>
        <button onClick={load} className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-2 rounded-lg font-medium flex items-center gap-1.5 shadow-sm">
          <RefreshCw className="w-3 h-3" /> Refresh
        </button>
      </div>

      <div className="relative max-w-xs">
        <Search className="w-3 h-3 absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
        <input value={search} onChange={e => setSearch(e.target.value)} className="bg-white border border-gray-200 rounded-lg pl-7 pr-3 py-1.5 text-[11px] text-gray-700 placeholder-gray-400 focus:outline-none focus:border-indigo-400 w-full shadow-sm" placeholder="Search name or email…" />
      </div>

      <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              {['ID', 'Name', 'Email', 'Role', 'Business', 'Status', 'Last Login', 'Actions'].map(h => (
                <th key={h} className="text-left py-2 px-3 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={8} className="py-8 text-center text-[11px] text-gray-400">Loading users…</td></tr>
            ) : data.map((u, i) => (
              <tr key={u.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                <td className="py-2.5 px-3 font-mono text-gray-400 text-[10px]">#{u.id}</td>
                <td className="px-3">
                  <div className="flex items-center gap-2">
                    <Chip initials={u.fullName.substring(0, 2).toUpperCase()} color={COLORS[i % COLORS.length]} />
                    <span className="font-semibold text-gray-800">{u.fullName}</span>
                  </div>
                </td>
                <td className="px-3 text-gray-500">{u.email}</td>
                <td className="px-3"><span className="text-[10px] bg-gray-100 text-gray-700 px-2 py-0.5 rounded font-medium">{u.role}</span></td>
                <td className="px-3 text-gray-500">{u.businessName ?? '—'}</td>
                <td className="px-3"><StatusBadge status={u.isActive ? 'Active' : 'Suspended'} /></td>
                <td className="px-3 text-gray-400">{u.lastLoginAt ? new Date(u.lastLoginAt).toLocaleDateString('en-IN') : '—'}</td>
                <td className="px-3">
                  <div className="flex items-center gap-1">
                    <button onClick={() => toggleActive(u)} disabled={busyId === u.id}
                      className={clsx('text-[10px] font-semibold px-2 py-1 rounded transition-colors', u.isActive ? 'bg-rose-50 text-rose-600 hover:bg-rose-100' : 'bg-emerald-50 text-emerald-600 hover:bg-emerald-100')}>
                      {busyId === u.id ? '…' : u.isActive ? 'Deactivate' : 'Activate'}
                    </button>
                    <button onClick={() => setResetUser(u)} className="p-1 text-gray-300 hover:text-indigo-500 transition-colors rounded" title="Reset Password">
                      <Key className="w-3.5 h-3.5" />
                    </button>
                    <button onClick={() => setConfirmUser(u)} className="p-1 text-gray-300 hover:text-rose-500 transition-colors rounded" title="Delete">
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {!loading && data.length === 0 && (
              <tr><td colSpan={8} className="py-8 text-center text-[11px] text-gray-400">{search ? 'No users match your search.' : 'No users found.'}</td></tr>
            )}
          </tbody>
        </table>
        <div className="px-4 py-2.5 text-[10px] text-gray-400 border-t border-gray-100">Showing {data.length} of {users.length} users</div>
      </div>
    </div>
  );
};

/* ════════════════════════════════════════════════════════════════
   VIEW 5 – SuperAdmins Management
════════════════════════════════════════════════════════════════ */
type SAFormState = { fullName: string; email: string; phone: string; password: string; isActive: boolean };
const defaultSAForm = (): SAFormState => ({ fullName: '', email: '', phone: '', password: '', isActive: true });

const SuperAdminsView: React.FC<{ setToast: (t: ToastState) => void }> = ({ setToast }) => {
  const [admins, setAdmins] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editAdmin, setEditAdmin] = useState<AdminUser | null>(null);
  const [form, setForm] = useState<SAFormState>(defaultSAForm());
  const [saving, setSaving] = useState(false);
  const [confirmAdmin, setConfirmAdmin] = useState<AdminUser | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [search, setSearch] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const data = await api.request<AdminUser[]>('/api/admin/superadmins');
      setAdmins(data ?? []);
    } catch { setAdmins([]); } finally { setLoading(false); }
  }, []);

  useEffect(() => { void load(); }, [load]);

  const openCreate = () => { setEditAdmin(null); setForm(defaultSAForm()); setShowModal(true); };
  const openEdit = (a: AdminUser) => {
    setEditAdmin(a);
    setForm({ fullName: a.fullName, email: a.email, phone: a.phone ?? '', password: '', isActive: a.isActive });
    setShowModal(true);
  };

  const save = async () => {
    setSaving(true);
    try {
      if (editAdmin) {
        const payload: any = { fullName: form.fullName, phone: form.phone, isActive: form.isActive };
        if (form.password) payload.password = form.password;
        await api.request(`/api/admin/superadmins/${editAdmin.id}`, { method: 'PUT', body: JSON.stringify(payload) });
        setToast({ msg: `SuperAdmin "${form.fullName}" updated`, ok: true });
      } else {
        await api.request('/api/admin/superadmins', { method: 'POST', body: JSON.stringify({ fullName: form.fullName, email: form.email, phone: form.phone, password: form.password }) });
        setToast({ msg: `SuperAdmin "${form.fullName}" created`, ok: true });
      }
      setShowModal(false);
      await load();
    } catch (e: any) {
      setToast({ msg: e?.message ?? 'Failed to save SuperAdmin', ok: false });
    } finally { setSaving(false); }
  };

  const deleteAdmin = async () => {
    if (!confirmAdmin) return;
    setDeleting(true);
    try {
      await api.request(`/api/admin/superadmins/${confirmAdmin.id}`, { method: 'DELETE' });
      setToast({ msg: `SuperAdmin "${confirmAdmin.fullName}" deleted`, ok: true });
      setConfirmAdmin(null);
      await load();
    } catch { setToast({ msg: 'Failed to delete SuperAdmin', ok: false }); } finally { setDeleting(false); }
  };

  const data = admins.filter(a => !search || a.fullName.toLowerCase().includes(search.toLowerCase()) || a.email.toLowerCase().includes(search.toLowerCase()));

  return (
    <div className="space-y-5">
      {confirmAdmin && <ConfirmDialog message={`Delete SuperAdmin "${confirmAdmin.fullName}"? This cannot be undone.`} busy={deleting} onCancel={() => setConfirmAdmin(null)} onConfirm={deleteAdmin} />}

      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md">
            <div className="px-5 py-4 border-b border-gray-100 flex items-center justify-between">
              <p className="font-bold text-gray-800 text-[13px]">{editAdmin ? `Edit SuperAdmin: ${editAdmin.fullName}` : 'Add New SuperAdmin'}</p>
              <button onClick={() => setShowModal(false)} className="text-gray-400 hover:text-gray-600"><X className="w-4 h-4" /></button>
            </div>
            <div className="p-5 space-y-3">
              <div>
                <label className="text-[11px] font-semibold text-gray-600 mb-1 block">Full Name *</label>
                <input value={form.fullName} onChange={e => setForm(f => ({ ...f, fullName: e.target.value }))} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-[12px] focus:outline-none focus:border-indigo-400" placeholder="Enter full name" />
              </div>
              {!editAdmin && (
                <div>
                  <label className="text-[11px] font-semibold text-gray-600 mb-1 block">Email *</label>
                  <input type="email" value={form.email} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-[12px] focus:outline-none focus:border-indigo-400" placeholder="admin@example.com" />
                </div>
              )}
              <div>
                <label className="text-[11px] font-semibold text-gray-600 mb-1 block">Phone</label>
                <input value={form.phone} onChange={e => setForm(f => ({ ...f, phone: e.target.value }))} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-[12px] focus:outline-none focus:border-indigo-400" placeholder="+91 9876543210" />
              </div>
              <div>
                <label className="text-[11px] font-semibold text-gray-600 mb-1 block">{editAdmin ? 'New Password (leave blank to keep)' : 'Password *'}</label>
                <input type="password" value={form.password} onChange={e => setForm(f => ({ ...f, password: e.target.value }))} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-[12px] focus:outline-none focus:border-indigo-400" placeholder={editAdmin ? '(unchanged)' : 'Min 8 characters'} />
              </div>
              {editAdmin && (
                <label className="flex items-center gap-2 text-[12px] text-gray-700 cursor-pointer">
                  <input type="checkbox" checked={form.isActive} onChange={e => setForm(f => ({ ...f, isActive: e.target.checked }))} className="accent-indigo-600 w-3.5 h-3.5" />
                  Active Account
                </label>
              )}
            </div>
            <div className="px-5 py-4 border-t border-gray-100 flex justify-end gap-3">
              <button onClick={() => setShowModal(false)} className="text-[12px] px-4 py-2 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50">Cancel</button>
              <button onClick={save} disabled={saving || !form.fullName || (!editAdmin && (!form.email || !form.password))}
                className="text-[12px] px-4 py-2 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white font-semibold disabled:opacity-50">
                {saving ? 'Saving…' : editAdmin ? 'Update' : 'Create SuperAdmin'}
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="flex items-start justify-between">
        <div>
          <p className="text-[10px] text-indigo-600 font-bold uppercase tracking-widest">PLATFORM ADMINISTRATION</p>
          <h1 className="text-2xl font-black text-gray-900 mt-1">SuperAdmin Accounts</h1>
          <p className="text-[11px] text-gray-500 mt-1.5">{admins.length} SuperAdmin account{admins.length !== 1 ? 's' : ''} in system.</p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={load} className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-2 rounded-lg font-medium flex items-center gap-1.5 shadow-sm">
            <RefreshCw className="w-3 h-3" /> Refresh
          </button>
          <button onClick={openCreate} className="text-[11px] bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg font-semibold flex items-center gap-1.5 shadow-sm">
            <UserPlus className="w-3 h-3" /> Add SuperAdmin
          </button>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-3">
        <KpiCard label="Total SuperAdmins" value={admins.length} icon={<Shield className="w-4 h-4" />} />
        <KpiCard label="Active" value={admins.filter(a => a.isActive).length} badge="Online" badgeColor="bg-emerald-100 text-emerald-700" />
        <KpiCard label="Inactive" value={admins.filter(a => !a.isActive).length} badge="Disabled" badgeColor="bg-gray-100 text-gray-600" />
      </div>

      <div className="relative max-w-xs">
        <Search className="w-3 h-3 absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
        <input value={search} onChange={e => setSearch(e.target.value)} className="bg-white border border-gray-200 rounded-lg pl-7 pr-3 py-1.5 text-[11px] text-gray-700 placeholder-gray-400 focus:outline-none focus:border-indigo-400 w-full shadow-sm" placeholder="Search name or email…" />
      </div>

      <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              {['ID', 'Name', 'Email', 'Phone', 'Status', 'Last Login', 'Created At', 'Actions'].map(h => (
                <th key={h} className="text-left py-2 px-3 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={8} className="py-8 text-center text-[11px] text-gray-400">Loading SuperAdmins…</td></tr>
            ) : data.map((a, i) => (
              <tr key={a.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                <td className="py-3 px-3">
                  <span className="inline-flex items-center gap-1 font-mono text-[10px] bg-indigo-50 text-indigo-700 border border-indigo-200 px-2 py-0.5 rounded font-bold">
                    <Shield className="w-2.5 h-2.5" />#{a.id}
                  </span>
                </td>
                <td className="px-3">
                  <div className="flex items-center gap-2">
                    <Chip initials={a.fullName.substring(0, 2).toUpperCase()} color={COLORS[i % COLORS.length]} />
                    <span className="font-semibold text-gray-800">{a.fullName}</span>
                  </div>
                </td>
                <td className="px-3 text-gray-500">{a.email}</td>
                <td className="px-3 text-gray-500">{a.phone ?? '—'}</td>
                <td className="px-3"><StatusBadge status={a.isActive ? 'Active' : 'Suspended'} /></td>
                <td className="px-3 text-gray-400">{a.lastLoginAt ? new Date(a.lastLoginAt).toLocaleDateString('en-IN') : 'Never'}</td>
                <td className="px-3 text-gray-400">{new Date(a.createdAt).toLocaleDateString('en-IN')}</td>
                <td className="px-3">
                  <div className="flex items-center gap-2">
                    <button onClick={() => openEdit(a)} className="p-1.5 text-gray-400 hover:text-indigo-600 transition-colors rounded border border-gray-200 hover:border-indigo-200" title="Edit">
                      <Edit2 className="w-3.5 h-3.5" />
                    </button>
                    <button onClick={() => setConfirmAdmin(a)} className="p-1.5 text-gray-400 hover:text-rose-500 transition-colors rounded border border-gray-200 hover:border-rose-200" title="Delete">
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {!loading && data.length === 0 && (
              <tr><td colSpan={8} className="py-10 text-center text-[11px] text-gray-400">
                <Shield className="w-8 h-8 text-gray-200 mx-auto mb-2" />
                {search ? 'No SuperAdmins match your search.' : 'No SuperAdmin accounts found.'}
              </td></tr>
            )}
          </tbody>
        </table>
        <div className="px-4 py-2.5 text-[10px] text-gray-400 border-t border-gray-100">
          Showing {data.length} of {admins.length} SuperAdmin accounts
        </div>
      </div>
    </div>
  );
};

/* ════════════════════════════════════════════════════════════════
   VIEW 6 – Audit Logs
════════════════════════════════════════════════════════════════ */
const AuditLogs: React.FC<{ logs: AuditLog[] }> = ({ logs }) => {
  const actionColor = (a: string) => a.includes('SUSPEND') ? 'bg-rose-50 text-rose-700 border-rose-200' : a.includes('UPGRADE') || a.includes('REGISTER') ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-gray-100 text-gray-600 border-gray-200';
  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div><h1 className="text-xl font-black text-gray-900">Audit & Security Logs</h1><p className="text-[11px] text-gray-400 mt-0.5">Platform event trail from live database.</p></div>
      </div>
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        <div className="divide-y divide-gray-50">
          {logs.length === 0 && <div className="py-10 text-center text-[11px] text-gray-400"><Activity className="w-8 h-8 text-gray-200 mx-auto mb-2" />No audit events recorded yet.</div>}
          {logs.map(log => (
            <div key={log.id} className="px-4 py-3 hover:bg-gray-50/50 transition-colors flex items-center gap-4">
              <Activity className="w-4 h-4 text-gray-300 shrink-0" />
              <div className="flex-1 min-w-0 flex items-center gap-2 flex-wrap">
                <span className={clsx('text-[10px] font-mono font-bold px-2 py-0.5 rounded border', actionColor(log.action))}>{log.action}</span>
                <span className="text-[11px] text-gray-500">{log.entityType}</span>
                {log.businessName && <span className="text-[11px] font-semibold text-gray-700">· {log.businessName}</span>}
                {log.userName && <span className="text-[11px] text-gray-400">by {log.userName}</span>}
              </div>
              <span className="text-[10px] text-gray-400 shrink-0">{new Date(log.timestamp).toLocaleString('en-IN', { dateStyle: 'short', timeStyle: 'short' })}</span>
            </div>
          ))}
        </div>
        {logs.length > 0 && <div className="px-4 py-2.5 text-[10px] text-gray-400 border-t border-gray-100">Showing {logs.length} recent events</div>}
      </div>
    </div>
  );
};

/* ════════════════════════════════════════════════════════════════
   NAV CONFIG
════════════════════════════════════════════════════════════════ */
/* ════════════════════════════════════════════════════════════════
   VIEW – Tenant Users (tenant-wise user breakdown)
════════════════════════════════════════════════════════════════ */
type TenantUserEntry = {
  tenantId: number; tenantName: string; tenantEmail?: string;
  tenantStatus: string; isActive: boolean; createdAt: string;
  userCount: number;
  users: { id: number; fullName: string; email: string; phone?: string; role: string; isActive: boolean; lastLoginAt?: string; createdAt: string }[];
};

const TenantUsersView: React.FC<{ setToast: (t: ToastState) => void }> = ({ setToast }) => {
  const [data, setData] = useState<TenantUserEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [expanded, setExpanded] = useState<number | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.request<TenantUserEntry[]>('/api/admin/tenants/users');
      setData((res as TenantUserEntry[]) ?? []);
    } catch {
      setToast({ msg: 'Failed to load tenant users', ok: false });
    } finally { setLoading(false); }
  }, [setToast]);

  useEffect(() => { void load(); }, [load]);

  const filtered = data.filter(t =>
    t.tenantName.toLowerCase().includes(search.toLowerCase()) ||
    (t.tenantEmail ?? '').toLowerCase().includes(search.toLowerCase())
  );

  if (loading) return <div className="flex items-center justify-center h-48 text-gray-400 text-[12px]">Loading tenant users…</div>;

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3 flex-wrap">
        <h2 className="text-[13px] font-bold text-gray-800">Tenant-wise Users</h2>
        <span className="text-[9px] bg-emerald-100 text-emerald-700 border border-emerald-200 px-1.5 py-0.5 rounded font-bold">● LIVE DB</span>
        <div className="ml-auto relative">
          <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3 h-3 text-gray-400" />
          <input
            value={search} onChange={e => setSearch(e.target.value)} placeholder="Search tenant…"
            className="pl-7 pr-3 py-1.5 text-[11px] border border-gray-200 rounded-lg w-52 focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
          />
        </div>
        <button onClick={load} className="flex items-center gap-1 text-[10px] text-gray-500 hover:text-indigo-600 bg-white border border-gray-200 rounded px-2 py-1 transition-colors">
          <RefreshCw className="w-3 h-3" /> Refresh
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              {['', 'Tenant ID', 'Business Name', 'Email', 'Status', 'User Count', 'Joined'].map(h => (
                <th key={h} className="text-left py-2.5 px-3 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {filtered.map((tenant, i) => (
              <>
                <tr key={tenant.tenantId} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors cursor-pointer"
                  onClick={() => setExpanded(expanded === tenant.tenantId ? null : tenant.tenantId)}>
                  <td className="py-2.5 px-3 w-6">
                    <ChevronDown className={`w-3.5 h-3.5 text-gray-400 transition-transform ${expanded === tenant.tenantId ? 'rotate-180' : ''}`} />
                  </td>
                  <td className="py-2.5 px-3 font-mono text-gray-500">#{tenant.tenantId}</td>
                  <td className="py-2.5 px-3">
                    <div className="flex items-center gap-2">
                      <Chip initials={tenant.tenantName.substring(0, 2).toUpperCase()} color={COLORS[i % COLORS.length]} />
                      <span className="font-semibold text-gray-800">{tenant.tenantName}</span>
                    </div>
                  </td>
                  <td className="px-3 text-gray-500">{tenant.tenantEmail ?? '—'}</td>
                  <td className="px-3"><StatusBadge status={tenant.tenantStatus} /></td>
                  <td className="px-3">
                    <span className="font-bold text-indigo-700 bg-indigo-50 border border-indigo-100 px-2 py-0.5 rounded text-[10px]">
                      {tenant.userCount} user{tenant.userCount !== 1 ? 's' : ''}
                    </span>
                  </td>
                  <td className="px-3 text-gray-400">{new Date(tenant.createdAt).toLocaleDateString('en-IN')}</td>
                </tr>
                {expanded === tenant.tenantId && (
                  <tr key={`${tenant.tenantId}-expanded`} className="bg-indigo-50/40">
                    <td colSpan={7} className="px-6 py-3">
                      {tenant.users.length === 0 ? (
                        <p className="text-[11px] text-gray-400 py-2">No users registered for this tenant yet.</p>
                      ) : (
                        <table className="w-full text-[10px]">
                          <thead>
                            <tr className="border-b border-indigo-100">
                              {['ID', 'Name', 'Email', 'Phone', 'Role', 'Active', 'Last Login'].map(h => (
                                <th key={h} className="text-left py-1.5 px-2 font-semibold uppercase tracking-wider text-indigo-400">{h}</th>
                              ))}
                            </tr>
                          </thead>
                          <tbody>
                            {tenant.users.map(u => (
                              <tr key={u.id} className="border-b border-indigo-50 hover:bg-indigo-50/60">
                                <td className="py-1.5 px-2 font-mono text-gray-500">#{u.id}</td>
                                <td className="px-2 font-semibold text-gray-700">{u.fullName}</td>
                                <td className="px-2 text-gray-500">{u.email}</td>
                                <td className="px-2 text-gray-400">{u.phone ?? '—'}</td>
                                <td className="px-2">
                                  <span className="bg-violet-50 text-violet-700 border border-violet-100 px-1.5 py-0.5 rounded font-semibold">{u.role}</span>
                                </td>
                                <td className="px-2">
                                  {u.isActive
                                    ? <span className="text-emerald-600 font-semibold">✓ Active</span>
                                    : <span className="text-gray-400">Inactive</span>}
                                </td>
                                <td className="px-2 text-gray-400">
                                  {u.lastLoginAt ? new Date(u.lastLoginAt).toLocaleDateString('en-IN') : 'Never'}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      )}
                    </td>
                  </tr>
                )}
              </>
            ))}
            {filtered.length === 0 && (
              <tr><td colSpan={7} className="py-10 text-center text-[11px] text-gray-400">No tenants found.</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

/* ════════════════════════════════════════════════════════════════
   VIEW – Tenant Products (tenant-wise product breakdown)
════════════════════════════════════════════════════════════════ */
type TenantProductEntry = {
  tenantId: number; tenantName: string; tenantStatus: string; productCount: number;
  products: { id: number; name: string; barcode: string; sku: string; salePrice: number; isActive: boolean }[];
};

const TenantProductsView: React.FC<{ setToast: (t: ToastState) => void }> = ({ setToast }) => {
  const [data, setData] = useState<TenantProductEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [expanded, setExpanded] = useState<number | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.request<TenantProductEntry[]>('/api/admin/tenants/products');
      setData((res as TenantProductEntry[]) ?? []);
    } catch {
      setToast({ msg: 'Failed to load tenant products', ok: false });
    } finally { setLoading(false); }
  }, [setToast]);

  useEffect(() => { void load(); }, [load]);

  const filtered = data.filter(t => t.tenantName.toLowerCase().includes(search.toLowerCase()));

  if (loading) return <div className="flex items-center justify-center h-48 text-gray-400 text-[12px]">Loading tenant products…</div>;

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3 flex-wrap">
        <h2 className="text-[13px] font-bold text-gray-800">Tenant-wise Products</h2>
        <span className="text-[9px] bg-emerald-100 text-emerald-700 border border-emerald-200 px-1.5 py-0.5 rounded font-bold">● LIVE DB</span>
        <div className="ml-auto relative">
          <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3 h-3 text-gray-400" />
          <input
            value={search} onChange={e => setSearch(e.target.value)} placeholder="Search tenant…"
            className="pl-7 pr-3 py-1.5 text-[11px] border border-gray-200 rounded-lg w-52 focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
          />
        </div>
        <button onClick={load} className="flex items-center gap-1 text-[10px] text-gray-500 hover:text-indigo-600 bg-white border border-gray-200 rounded px-2 py-1 transition-colors">
          <RefreshCw className="w-3 h-3" /> Refresh
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              {['', 'Tenant ID', 'Business Name', 'Status', 'Product Count'].map(h => (
                <th key={h} className="text-left py-2.5 px-3 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {filtered.map((tenant, i) => (
              <>
                <tr key={tenant.tenantId} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors cursor-pointer"
                  onClick={() => setExpanded(expanded === tenant.tenantId ? null : tenant.tenantId)}>
                  <td className="py-2.5 px-3 w-6">
                    <ChevronDown className={`w-3.5 h-3.5 text-gray-400 transition-transform ${expanded === tenant.tenantId ? 'rotate-180' : ''}`} />
                  </td>
                  <td className="py-2.5 px-3 font-mono text-gray-500">#{tenant.tenantId}</td>
                  <td className="py-2.5 px-3">
                    <div className="flex items-center gap-2">
                      <Chip initials={tenant.tenantName.substring(0, 2).toUpperCase()} color={COLORS[i % COLORS.length]} />
                      <span className="font-semibold text-gray-800">{tenant.tenantName}</span>
                    </div>
                  </td>
                  <td className="px-3"><StatusBadge status={tenant.tenantStatus} /></td>
                  <td className="px-3">
                    <span className="font-bold text-emerald-700 bg-emerald-50 border border-emerald-100 px-2 py-0.5 rounded text-[10px]">
                      {tenant.productCount} product{tenant.productCount !== 1 ? 's' : ''}
                    </span>
                  </td>
                </tr>
                {expanded === tenant.tenantId && (
                  <tr key={`${tenant.tenantId}-products`} className="bg-emerald-50/30">
                    <td colSpan={5} className="px-6 py-3">
                      {tenant.products.length === 0 ? (
                        <p className="text-[11px] text-gray-400 py-2">
                          {tenant.productCount === 0
                            ? 'No products added for this tenant yet.'
                            : `${tenant.productCount} products exist — preview limited to 10.`}
                        </p>
                      ) : (
                        <>
                          {tenant.productCount > 10 && (
                            <p className="text-[9px] text-gray-400 mb-2">Showing first 10 of {tenant.productCount} products</p>
                          )}
                          <table className="w-full text-[10px]">
                            <thead>
                              <tr className="border-b border-emerald-100">
                                {['ID', 'Product Name', 'Barcode', 'SKU', 'Sale Price', 'Active'].map(h => (
                                  <th key={h} className="text-left py-1.5 px-2 font-semibold uppercase tracking-wider text-emerald-400">{h}</th>
                                ))}
                              </tr>
                            </thead>
                            <tbody>
                              {tenant.products.map(p => (
                                <tr key={p.id} className="border-b border-emerald-50 hover:bg-emerald-50/60">
                                  <td className="py-1.5 px-2 font-mono text-gray-500">#{p.id}</td>
                                  <td className="px-2 font-semibold text-gray-700">{p.name}</td>
                                  <td className="px-2 font-mono text-gray-400">{p.barcode || '—'}</td>
                                  <td className="px-2 font-mono text-gray-400">{p.sku || '—'}</td>
                                  <td className="px-2 font-semibold text-gray-700">₹{(p.salePrice ?? 0).toLocaleString('en-IN')}</td>
                                  <td className="px-2">
                                    {p.isActive
                                      ? <span className="text-emerald-600 font-semibold">✓ Active</span>
                                      : <span className="text-gray-400">Inactive</span>}
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </>
                      )}
                    </td>
                  </tr>
                )}
              </>
            ))}
            {filtered.length === 0 && (
              <tr><td colSpan={5} className="py-10 text-center text-[11px] text-gray-400">No tenants found.</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

const VIEWS = [
  { id: 'overview', label: 'Platform Overview', icon: LayoutDashboard, section: 'OPERATIONS' },
  { id: 'tenants', label: 'Tenant Registry', icon: Building2, section: 'OPERATIONS' },
  { id: 'tenant-users', label: 'Tenant Users', icon: Users, section: 'OPERATIONS' },
  { id: 'tenant-products', label: 'Tenant Products', icon: Package, section: 'OPERATIONS' },
  { id: 'plans', label: 'Global Plans', icon: Globe, section: 'OPERATIONS' },
  { id: 'superadmins', label: 'SuperAdmin Accounts', icon: Shield, section: 'OPERATIONS' },
  { id: 'users', label: 'All Users', icon: Users, section: 'OPERATIONS' },
  { id: 'plan-permits', label: 'Plan Permit Manager', icon: Gift, section: 'OPERATIONS' },
  { id: 'audit', label: 'Audit & Security Logs', icon: ShieldAlert, section: 'OPERATIONS' },
];

/* ════════════════════════════════════════════════════════════════
   ROOT COMPONENT
════════════════════════════════════════════════════════════════ */
export const PlatformAdminPage: React.FC = () => {
  const [view, setView] = useState('overview');
  const [dash, setDash] = useState<DashData>(EMPTY_DASH);
  const [shops, setShops] = useState<Shop[]>([]);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [busyId, setBusyId] = useState<number | null>(null);
  const [collapsed, setCollapsed] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [lastRefreshed, setLastRefreshed] = useState<Date>(() => new Date());
  const [toast, setToastState] = useState<ToastState>(null);
  const clock = useClock();

  const setToast = (t: ToastState) => {
    setToastState(t);
    if (t) setTimeout(() => setToastState(null), 3500);
  };

  const load = useCallback(async () => {
    setIsRefreshing(true);
    try {
      const [d, s, p, l] = await Promise.all([
        api.request<DashData>('/api/admin/dashboard').catch(() => null),
        api.request<Shop[]>('/api/admin/shops').catch(() => null),
        api.request<Plan[]>('/api/admin/plans').catch(() => null),
        api.request<AuditLog[]>('/api/admin/audit-logs?page=1&pageSize=50').catch(() => null),
      ]);
      setDash((d as DashData) ?? EMPTY_DASH);
      setShops((s as Shop[]) ?? []);
      setPlans((p as Plan[]) ?? []);
      setLogs((l as AuditLog[]) ?? []);
      setLastRefreshed(new Date());
    } catch {
      setDash(EMPTY_DASH);
      setShops([]); setPlans([]); setLogs([]);
    } finally {
      setIsRefreshing(false);
    }
  }, []);

  useEffect(() => { void load(); }, [load]);
  useEffect(() => {
    const id = setInterval(() => { void load(); }, 30_000);
    return () => clearInterval(id);
  }, [load]);

  const toggleShop = async (shop: Shop) => {
    const action = shop.status.toLowerCase() === 'suspended' ? 'activate' : 'suspend';
    setBusyId(shop.id);
    try {
      await api.request(`/api/admin/shops/${shop.id}/${action}`, { method: 'POST' });
      setToast({ msg: `Shop "${shop.name}" ${action}d`, ok: true });
      await load();
    } catch { setToast({ msg: `Failed to ${action} shop`, ok: false }); }
    finally { setBusyId(null); }
  };

  const deleteShop = async (shop: Shop) => {
    try {
      await api.request(`/api/admin/shops/${shop.id}`, { method: 'DELETE' });
      setToast({ msg: `Shop "${shop.name}" deleted`, ok: true });
      await load();
    } catch { setToast({ msg: 'Failed to delete shop', ok: false }); }
  };

  const sections = [...new Set(VIEWS.map(v => v.section))];

  const renderView = () => {
    switch (view) {
      case 'overview': return <PlatformOverview dash={dash} shops={shops} logs={logs} lastRefreshed={lastRefreshed} isRefreshing={isRefreshing} onRefresh={load} />;
      case 'tenants': return <TenantRegistry shops={shops} onToggleShop={toggleShop} onDeleteShop={deleteShop} busyId={busyId} onRefresh={load} />;
      case 'tenant-users': return <TenantUsersView setToast={setToast} />;
      case 'tenant-products': return <TenantProductsView setToast={setToast} />;
      case 'plans': return <GlobalPlans plans={plans} onRefresh={load} setToast={setToast} />;
      case 'superadmins': return <SuperAdminsView setToast={setToast} />;
      case 'users': return <AllUsers onRefresh={load} setToast={setToast} />;
      case 'plan-permits': return <PlanPermitManager businesses={shops} />;
      case 'audit': return <AuditLogs logs={logs} />;
      default: return (
        <div className="flex items-center justify-center h-64 bg-white rounded-xl border border-gray-200">
          <div className="text-center">
            <Cpu className="w-10 h-10 text-gray-200 mx-auto mb-3" />
            <p className="text-gray-600 font-semibold">{VIEWS.find(v => v.id === view)?.label}</p>
            <p className="text-gray-400 text-[11px] mt-1">Module coming soon</p>
          </div>
        </div>
      );
    }
  };

  return (
    <div className="flex h-full -m-4 lg:-m-6 bg-gray-50 overflow-hidden" style={{ minHeight: 'calc(100vh - 64px)' }}>
      <Toast toast={toast} />

      {/* Sidebar */}
      <aside className={clsx('flex flex-col border-r border-gray-800 bg-gray-900 shrink-0 transition-all duration-200', collapsed ? 'w-12' : 'w-52')}>
        <div className="h-12 flex items-center justify-between px-3 border-b border-gray-800 shrink-0">
          {!collapsed && (
            <div>
              <p className="text-[9px] font-bold text-indigo-400 uppercase tracking-widest">MasterBilling</p>
              <p className="text-[10px] text-gray-300 font-semibold">SUPERADMIN</p>
            </div>
          )}
          <button onClick={() => setCollapsed(!collapsed)} className="text-gray-600 hover:text-gray-400 transition-colors p-1 rounded ml-auto">
            {collapsed ? <ChevronRight className="w-3.5 h-3.5" /> : <ChevronLeft className="w-3.5 h-3.5" />}
          </button>
        </div>

        <div className="flex-1 overflow-y-auto py-2 px-2 space-y-3">
          {sections.map(section => (
            <div key={section}>
              {!collapsed && <p className="text-[9px] font-bold uppercase tracking-widest text-gray-600 px-2 mb-1">{section}</p>}
              <div className="space-y-0.5">
                {VIEWS.filter(v => v.section === section).map(item => {
                  const Icon = item.icon;
                  const active = view === item.id;
                  return (
                    <button key={item.id} onClick={() => setView(item.id)} title={collapsed ? item.label : undefined}
                      className={clsx('w-full flex items-center gap-2 px-2 py-1.5 rounded-lg text-[11px] font-medium transition-all text-left', active ? 'bg-gray-700 text-white' : 'text-gray-500 hover:bg-gray-800 hover:text-gray-300')}>
                      <Icon className={clsx('w-3.5 h-3.5 shrink-0', active ? 'text-white' : 'text-gray-600')} />
                      {!collapsed && <span className="truncate">{item.label}</span>}
                    </button>
                  );
                })}
              </div>
            </div>
          ))}
        </div>

        <div className="p-3 border-t border-gray-800">
          <div className={clsx('flex items-center gap-1.5', collapsed ? 'justify-center' : 'px-1')}>
            <span className="w-2 h-2 rounded-full bg-emerald-500 shrink-0 animate-pulse" />
            {!collapsed && <span className="text-[10px] text-gray-600">Live Production</span>}
          </div>
          {!collapsed && <p className="text-[9px] text-gray-700 px-1 mt-0.5">{clock}</p>}
        </div>
      </aside>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <div className="h-10 bg-white border-b border-gray-200 flex items-center px-5 gap-2 shrink-0 shadow-sm">
          <span className="text-[10px] text-gray-400 flex items-center gap-1.5">
            PLATFORM <ChevronRight className="w-3 h-3" />
            <span className="text-gray-600 font-medium">{VIEWS.find(v => v.id === view)?.label ?? 'Dashboard'}</span>
          </span>
          <div className="flex items-center gap-3 ml-auto">
            <div className="flex items-center gap-1.5">
              <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
              <span className="text-[10px] text-gray-500 font-medium">Live DB</span>
            </div>
            <span className="text-[10px] font-mono text-gray-400">{clock}</span>
            <button onClick={() => load()} disabled={isRefreshing} className="text-[10px] text-gray-400 hover:text-gray-600 flex items-center gap-1 transition-colors">
              <RefreshCw className={clsx('w-3 h-3', isRefreshing && 'animate-spin')} />
            </button>
          </div>
        </div>

        <div className="flex-1 overflow-y-auto p-5">
          {renderView()}
        </div>
      </div>
    </div>
  );
};
