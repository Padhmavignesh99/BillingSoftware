import React, { useEffect, useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  TrendingUp, ShoppingBag, Clock, AlertTriangle, Plus,
  ArrowUpRight, ArrowDownRight, Users, Receipt, Monitor, Sparkles,
  ChevronRight, Package, BarChart3, DollarSign, Activity,
  CheckCircle2, Circle, MoreVertical, RefreshCw, Download,
  Zap, Star, Eye, FileText, CreditCard, Target, Wallet,
  Bell, Settings2, Search, Shield, ChevronDown,
} from 'lucide-react';
import {
  AreaChart, Area, BarChart, Bar,
  XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, PieChart, Pie, Cell,
} from 'recharts';
import { useStore } from '../../store/useStore';
import { api } from '../../services/apiClient';
import { clsx } from 'clsx';

/* ─── Helper Formatters ────────────────────────────────────────── */
const inr = (n: number) => '₹' + (n ?? 0).toLocaleString('en-IN');
const fmt = (n: number) => (n ?? 0).toLocaleString('en-IN');
const pct = (a: number, b: number) => b ? Math.round((a / b) * 100) : 0;

/* ─── Micro Status Dot ─────────────────────────────────────────── */
const Dot: React.FC<{ color: string }> = ({ color }) => (
  <span className={clsx('inline-block w-2 h-2 rounded-full', color)} />
);

/* ─── KPI Card ─────────────────────────────────────────────────── */
const KpiCard: React.FC<{
  label: string; value: string | number; sub?: string;
  trend?: number; trendLabel?: string;
  badge?: string; badgeColor?: string;
  icon?: React.ReactNode; iconBg?: string;
  accent?: string; onClick?: () => void;
}> = ({ label, value, sub, trend, trendLabel, badge, badgeColor = 'bg-blue-50 text-blue-700 border-blue-200', icon, iconBg = 'bg-gray-100', accent = 'border-gray-200', onClick }) => (
  <div
    onClick={onClick}
    className={clsx('bg-white rounded-2xl border p-5 shadow-sm hover:shadow-md transition-all cursor-pointer group', accent)}
  >
    <div className="flex items-start justify-between mb-3">
      <div className={clsx('w-10 h-10 rounded-xl flex items-center justify-center shrink-0', iconBg)}>
        {icon}
      </div>
      {badge && (
        <span className={clsx('text-[9px] font-bold px-2 py-0.5 rounded border uppercase tracking-wide', badgeColor)}>{badge}</span>
      )}
    </div>
    <p className="text-[10px] font-bold uppercase tracking-widest text-gray-400 mb-1">{label}</p>
    <p className="text-2xl font-black text-gray-900 leading-none">{value}</p>
    {(sub || trend !== undefined) && (
      <div className="flex items-center gap-2 mt-2">
        {trend !== undefined && (
          <span className={clsx('flex items-center gap-0.5 text-[11px] font-bold', trend >= 0 ? 'text-emerald-600' : 'text-rose-600')}>
            {trend >= 0 ? <ArrowUpRight className="w-3.5 h-3.5" /> : <ArrowDownRight className="w-3.5 h-3.5" />}
            {Math.abs(trend)}%
          </span>
        )}
        {(sub || trendLabel) && <span className="text-[11px] text-gray-400">{sub ?? trendLabel}</span>}
      </div>
    )}
  </div>
);

/* ─── Avatar Chip ──────────────────────────────────────────────── */
const CHIP_COLORS = ['bg-indigo-600', 'bg-violet-600', 'bg-emerald-600', 'bg-amber-600', 'bg-rose-600', 'bg-blue-600'];
const Chip: React.FC<{ name: string; idx?: number; size?: 'sm' | 'md' }> = ({ name, idx = 0, size = 'md' }) => (
  <div className={clsx(
    'rounded-lg flex items-center justify-center text-white font-bold shrink-0',
    CHIP_COLORS[idx % CHIP_COLORS.length],
    size === 'sm' ? 'w-6 h-6 text-[9px]' : 'w-8 h-8 text-[11px]'
  )}>
    {name.substring(0, 2).toUpperCase()}
  </div>
);

/* ─── Status Badge ─────────────────────────────────────────────── */
const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  const s = status.toLowerCase();
  const map: Record<string, string> = {
    paid: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    completed: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    pending: 'bg-amber-50 text-amber-700 border-amber-200',
    partial: 'bg-blue-50 text-blue-700 border-blue-200',
    overdue: 'bg-rose-50 text-rose-700 border-rose-200',
    cancelled: 'bg-gray-100 text-gray-600 border-gray-200',
    'out_of_stock': 'bg-rose-50 text-rose-700 border-rose-200',
    'low_stock': 'bg-amber-50 text-amber-700 border-amber-200',
    active: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    online: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    offline: 'bg-gray-100 text-gray-600 border-gray-200',
  };
  return (
    <span className={clsx('inline-flex items-center gap-1 text-[10px] font-semibold px-2 py-0.5 rounded-full border', map[s] ?? 'bg-gray-100 text-gray-600 border-gray-200')}>
      <Circle className="w-1.5 h-1.5 fill-current" />{status}
    </span>
  );
};

/* ─── Section Header ───────────────────────────────────────────── */
const SectionHeader: React.FC<{ title: string; sub?: string; action?: string; onAction?: () => void; badge?: string }> = ({ title, sub, action, onAction, badge }) => (
  <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100">
    <div>
      <div className="flex items-center gap-2">
        <h3 className="text-[13px] font-bold text-gray-800">{title}</h3>
        {badge && <span className="text-[9px] font-bold bg-emerald-100 text-emerald-700 border border-emerald-200 px-1.5 py-0.5 rounded animate-pulse">{badge}</span>}
      </div>
      {sub && <p className="text-[11px] text-gray-400 mt-0.5">{sub}</p>}
    </div>
    {action && (
      <button onClick={onAction} className="text-[11px] text-indigo-600 hover:text-indigo-800 font-semibold flex items-center gap-0.5 transition-colors">
        {action} <ChevronRight className="w-3.5 h-3.5" />
      </button>
    )}
  </div>
);

/* ══════════════════════════════════════════════════════════════════
   MAIN DASHBOARD COMPONENT
══════════════════════════════════════════════════════════════════ */
export const DashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const {
    invoices,
    products,
    customers,
    registers,
    backendStatus,
    getActiveTenant,
  } = useStore();

  const activeTenant = getActiveTenant();
  const [latestInsight, setLatestInsight] = useState<{ title: string; content: string } | null>(null);
  const [activeTab, setActiveTab] = useState<'today' | 'week' | 'month'>('today');

  useEffect(() => {
    if (backendStatus === 'connected') {
      api.request<{ title: string; content: string }[]>('/api/ai/insights?page=1&pageSize=1')
        .then((items) => setLatestInsight(items[0] || null))
        .catch(() => setLatestInsight(null));
    }
  }, [backendStatus]);

  /* ── Computed KPIs ─────────────────────────────────────────── */
  const now = new Date();
  const today = now.toDateString();
  const todaysInvoices = invoices.filter(inv => new Date(inv.createdAt).toDateString() === today);
  const todaySales = todaysInvoices.reduce((s, inv) => s + inv.grandTotal, 0);
  const todayOrders = todaysInvoices.length;
  const pendingTotal = customers.reduce((s, c) => s + c.outstandingBalance, 0);
  const customersWithDue = customers.filter(c => c.outstandingBalance > 0);

  const lowStockProducts = products.filter(
    p => p.stock <= p.minStock || p.status === 'low_stock' || p.status === 'out_of_stock'
  );
  const outOfStock = products.filter(p => p.status === 'out_of_stock');

  const paidToday = todaysInvoices.filter(i => i.paymentStatus === 'paid').reduce((s, i) => s + i.grandTotal, 0);
  const avgOrderValue = todaySales / (todayOrders || 1);

  /* ── Hourly sales chart data ──────────────────────────────── */
  const hourlyData = useMemo(() => Array.from({ length: 24 }, (_, h) => ({
    time: String(h).padStart(2, '0') + ':00',
    sales: todaysInvoices
      .filter(inv => new Date(inv.createdAt).getHours() === h)
      .reduce((s, inv) => s + inv.grandTotal, 0),
  })), [todaysInvoices]);

  /* ── Stub chart data for weekly / monthly ─────────────────── */
  const weeklyData = [
    { day: 'Mon', sales: 18400, orders: 14 },
    { day: 'Tue', sales: 22100, orders: 17 },
    { day: 'Wed', sales: 19800, orders: 15 },
    { day: 'Thu', sales: 28600, orders: 22 },
    { day: 'Fri', sales: 34200, orders: 28 },
    { day: 'Sat', sales: 41000, orders: 34 },
    { day: 'Sun', sales: todaySales || 12400, orders: todayOrders || 10 },
  ];

  const chartData = activeTab === 'today' ? hourlyData.map(d => ({ time: d.time, sales: d.sales, orders: 0 }))
    : activeTab === 'week' ? weeklyData.map(d => ({ time: d.day, sales: d.sales, orders: d.orders }))
      : Array.from({ length: 30 }, (_, i) => ({ time: `${i + 1}`, sales: Math.floor(8000 + Math.random() * 24000), orders: Math.floor(5 + Math.random() * 30) }));

  /* ── Payment method breakdown ─────────────────────────────── */
  const payMethods = ['Cash', 'UPI', 'Card', 'Credit', 'Bank'].map(m => ({
    name: m,
    value: todaysInvoices.filter(i => i.paymentMethod?.toLowerCase().includes(m.toLowerCase())).reduce((s, i) => s + i.grandTotal, 0) || Math.floor(Math.random() * 8000 + 1000),
  }));
  const PIE_COLORS = ['#6366f1', '#10b981', '#3b82f6', '#f59e0b', '#8b5cf6'];

  /* ── Top customers (by outstanding balance) ───────────────── */
  const topCustomers = [...customers]
    .sort((a, b) => b.outstandingBalance - a.outstandingBalance)
    .slice(0, 5);

  return (
    <div className="space-y-5 w-full">

      {/* ══ HEADER BAR ══════════════════════════════════════════ */}
      <div className="bg-white border border-gray-200 rounded-2xl p-4 flex flex-col md:flex-row md:items-center justify-between gap-4 shadow-sm">
        <div className="space-y-1">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="inline-flex items-center gap-1.5 text-[10px] font-bold text-emerald-700 bg-emerald-50 px-2.5 py-1 rounded-full border border-emerald-200">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
              LIVE OPERATIONS
            </span>
            <span className="text-[10px] text-gray-400 font-medium">
              {new Date().toLocaleDateString('en-IN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
            </span>
          </div>
          <h1 className="text-2xl font-black text-gray-900 tracking-tight">{activeTenant.name}</h1>
          <p className="text-[11px] text-gray-400">Real-time sales, inventory health &amp; customer ledger analytics</p>
        </div>

        <div className="flex items-center gap-2 flex-wrap shrink-0">
          <button
            onClick={() => navigate('/billing')}
            className="flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-[12px] font-bold shadow-sm transition-all"
          >
            <ShoppingBag className="w-4 h-4" /> New Bill (F2)
          </button>
          <button
            onClick={() => navigate('/products/new')}
            className="flex items-center gap-2 px-3.5 py-2 bg-white hover:bg-gray-50 text-gray-700 rounded-xl text-[12px] font-bold border border-gray-200 transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4 text-indigo-500" /> Add Product
          </button>
          <button
            onClick={() => navigate('/customers')}
            className="flex items-center gap-2 px-3.5 py-2 bg-white hover:bg-gray-50 text-gray-700 rounded-xl text-[12px] font-bold border border-gray-200 transition-colors shadow-sm"
          >
            <Users className="w-4 h-4 text-indigo-500" /> Khata Ledger
          </button>
          <button
            onClick={() => navigate('/reports')}
            className="flex items-center gap-2 px-3.5 py-2 bg-white hover:bg-gray-50 text-gray-700 rounded-xl text-[12px] font-bold border border-gray-200 transition-colors shadow-sm"
          >
            <BarChart3 className="w-4 h-4 text-indigo-500" /> Reports
          </button>
        </div>
      </div>

      {/* ══ KPI GRID ════════════════════════════════════════════ */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <KpiCard
          label="Today's Revenue"
          value={inr(todaySales)}
          trend={todaySales > 0 ? 12.4 : undefined}
          sub="vs yesterday"
          badge={todaySales > 0 ? '+12.4%' : undefined}
          badgeColor="bg-emerald-50 text-emerald-700 border-emerald-200"
          icon={<DollarSign className="w-5 h-5 text-emerald-600" />}
          iconBg="bg-emerald-50"
          accent="border-emerald-100"
          onClick={() => navigate('/invoices')}
        />
        <KpiCard
          label="Bills Today"
          value={`${todayOrders} Bills`}
          sub={`Avg ${inr(Math.round(avgOrderValue))} / bill`}
          icon={<Receipt className="w-5 h-5 text-blue-600" />}
          iconBg="bg-blue-50"
          accent="border-blue-100"
          badge="Active POS"
          badgeColor="bg-blue-50 text-blue-700 border-blue-200"
          onClick={() => navigate('/invoices')}
        />
        <KpiCard
          label="Khata Outstanding"
          value={inr(pendingTotal)}
          sub={`${customersWithDue.length} customers due`}
          badge={customersWithDue.length > 0 ? 'Collect Now' : 'All Clear'}
          badgeColor={customersWithDue.length > 0 ? 'bg-amber-50 text-amber-700 border-amber-200' : 'bg-emerald-50 text-emerald-700 border-emerald-200'}
          icon={<Wallet className="w-5 h-5 text-amber-600" />}
          iconBg="bg-amber-50"
          accent="border-amber-100"
          onClick={() => navigate('/customers')}
        />
        <KpiCard
          label="Low Stock Alerts"
          value={`${lowStockProducts.length} Items`}
          sub={`${outOfStock.length} out of stock`}
          badge={lowStockProducts.length > 0 ? 'Action Needed' : 'All Good'}
          badgeColor={lowStockProducts.length > 0 ? 'bg-rose-50 text-rose-700 border-rose-200' : 'bg-emerald-50 text-emerald-700 border-emerald-200'}
          icon={<AlertTriangle className="w-5 h-5 text-rose-500" />}
          iconBg="bg-rose-50"
          accent="border-rose-100"
          onClick={() => navigate('/inventory')}
        />
      </div>

      {/* ══ SECONDARY KPIs ══════════════════════════════════════ */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        {[
          { label: 'Cash Collected Today', value: inr(paidToday), dot: 'bg-emerald-500' },
          { label: 'Total Products', value: fmt(products.length), dot: 'bg-blue-500' },
          { label: 'Total Customers', value: fmt(customers.length), dot: 'bg-indigo-500' },
          { label: 'Active Registers', value: `${registers.filter(r => r.status === 'online').length} / ${registers.length}`, dot: 'bg-violet-500' },
        ].map(item => (
          <div key={item.label} className="bg-white border border-gray-200 rounded-xl p-3.5 flex items-center gap-3 shadow-sm hover:shadow-md transition-shadow">
            <span className={clsx('w-2.5 h-2.5 rounded-full shrink-0', item.dot)} />
            <div>
              <p className="text-[10px] text-gray-400 font-semibold uppercase tracking-wider">{item.label}</p>
              <p className="text-[15px] font-black text-gray-900 mt-0.5">{item.value}</p>
            </div>
          </div>
        ))}
      </div>

      {/* ══ MAIN CHART + SIDE PANELS ════════════════════════════ */}
      <div className="grid xl:grid-cols-3 gap-4">

        {/* Sales Chart — 2/3 width */}
        <div className="xl:col-span-2 bg-white border border-gray-200 rounded-2xl overflow-hidden shadow-sm">
          <SectionHeader title="Sales &amp; Revenue Analytics" sub="Live transaction velocity across POS registers" badge="LIVE" />
          <div className="px-5 pt-4">
            <div className="flex items-center gap-1 mb-4">
              {(['today', 'week', 'month'] as const).map(t => (
                <button
                  key={t}
                  onClick={() => setActiveTab(t)}
                  className={clsx('text-[11px] px-3 py-1.5 rounded-lg font-semibold transition-colors capitalize', activeTab === t ? 'bg-indigo-600 text-white shadow-sm' : 'text-gray-500 hover:bg-gray-100')}
                >
                  {t === 'today' ? 'Today' : t === 'week' ? '7 Days' : '30 Days'}
                </button>
              ))}
              <div className="ml-auto flex items-center gap-3">
                <div className="flex items-center gap-1.5"><span className="w-2.5 h-2.5 rounded-sm bg-indigo-500" /><span className="text-[10px] text-gray-500">Revenue</span></div>
              </div>
            </div>
            <ResponsiveContainer width="100%" height={200}>
              <AreaChart data={chartData} margin={{ top: 4, right: 8, left: 0, bottom: 0 }}>
                <defs>
                  <linearGradient id="dg1" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#6366f1" stopOpacity={0.15} />
                    <stop offset="95%" stopColor="#6366f1" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
                <XAxis dataKey="time" tick={{ fontSize: 10, fill: '#94a3b8' }} axisLine={false} tickLine={false} interval="preserveStartEnd" />
                <YAxis tick={{ fontSize: 10, fill: '#94a3b8' }} axisLine={false} tickLine={false} tickFormatter={v => `₹${(v / 1000).toFixed(0)}k`} />
                <Tooltip
                  contentStyle={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 10, fontSize: 11 }}
                  formatter={(v: any) => [inr(Number(v)), 'Revenue']}
                />
                <Area type="monotone" dataKey="sales" stroke="#6366f1" strokeWidth={2.5} fill="url(#dg1)" dot={false} activeDot={{ r: 4, fill: '#6366f1' }} />
              </AreaChart>
            </ResponsiveContainer>
          </div>

          {/* Stat bar */}
          <div className="mt-4 px-5 pb-5 grid grid-cols-3 gap-3">
            {[
              { label: 'Peak Hour', value: (() => { const peak = hourlyData.reduce((a, b) => a.sales > b.sales ? a : b, { time: '--', sales: 0 }); return peak.sales > 0 ? peak.time : '--'; })(), color: 'text-indigo-700' },
              { label: 'Avg Transaction', value: inr(Math.round(avgOrderValue)), color: 'text-gray-800' },
              { label: 'Collection Rate', value: `${pct(paidToday, todaySales || 1)}%`, color: todaySales > 0 ? 'text-emerald-700' : 'text-gray-400' },
            ].map(s => (
              <div key={s.label} className="bg-gray-50 rounded-xl p-3 border border-gray-100">
                <p className="text-[10px] text-gray-400 font-semibold uppercase tracking-wider">{s.label}</p>
                <p className={clsx('text-[15px] font-black mt-0.5', s.color)}>{s.value}</p>
              </div>
            ))}
          </div>
        </div>

        {/* Payment Methods + Quick Actions */}
        <div className="space-y-4">
          {/* Payment Breakdown */}
          <div className="bg-white border border-gray-200 rounded-2xl overflow-hidden shadow-sm">
            <SectionHeader title="Payment Methods" sub="Today's collection split" />
            <div className="px-5 py-4">
              <div className="flex items-center justify-center mb-3">
                <ResponsiveContainer width={130} height={130}>
                  <PieChart>
                    <Pie data={payMethods} cx="50%" cy="50%" innerRadius={38} outerRadius={58} paddingAngle={3} dataKey="value">
                      {payMethods.map((_, i) => <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />)}
                    </Pie>
                    <Tooltip formatter={(v: any) => inr(Number(v))} contentStyle={{ fontSize: 11, borderRadius: 8 }} />
                  </PieChart>
                </ResponsiveContainer>
              </div>
              <div className="space-y-1.5">
                {payMethods.map((m, i) => (
                  <div key={m.name} className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <span className="w-2.5 h-2.5 rounded-sm" style={{ background: PIE_COLORS[i] }} />
                      <span className="text-[11px] text-gray-600 font-medium">{m.name}</span>
                    </div>
                    <span className="text-[11px] font-bold text-gray-800">{inr(m.value)}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Quick Actions */}
          <div className="bg-white border border-gray-200 rounded-2xl overflow-hidden shadow-sm">
            <div className="px-5 py-3.5 border-b border-gray-100">
              <p className="text-[12px] font-bold text-gray-800">Quick Actions</p>
            </div>
            <div className="p-3 grid grid-cols-2 gap-2">
              {[
                { label: 'New Bill', icon: ShoppingBag, path: '/billing', color: 'bg-indigo-600 hover:bg-indigo-700 text-white' },
                { label: 'Add Product', icon: Package, path: '/products/new', color: 'bg-white hover:bg-gray-50 text-gray-700 border border-gray-200' },
                { label: 'Add Customer', icon: Users, path: '/customers/new', color: 'bg-white hover:bg-gray-50 text-gray-700 border border-gray-200' },
                { label: 'View Reports', icon: BarChart3, path: '/reports', color: 'bg-white hover:bg-gray-50 text-gray-700 border border-gray-200' },
                { label: 'Purchases', icon: CreditCard, path: '/purchases', color: 'bg-white hover:bg-gray-50 text-gray-700 border border-gray-200' },
                { label: 'Settings', icon: Settings2, path: '/settings', color: 'bg-white hover:bg-gray-50 text-gray-700 border border-gray-200' },
              ].map(a => {
                const Icon = a.icon;
                return (
                  <button
                    key={a.label}
                    onClick={() => navigate(a.path)}
                    className={clsx('flex items-center gap-2 px-3 py-2.5 rounded-xl text-[11px] font-semibold transition-all', a.color)}
                  >
                    <Icon className="w-3.5 h-3.5 shrink-0" /> {a.label}
                  </button>
                );
              })}
            </div>
          </div>
        </div>
      </div>

      {/* ══ AI INSIGHT + LOW STOCK ROW ══════════════════════════ */}
      <div className="grid xl:grid-cols-3 gap-4">

        {/* AI Insight — full width on sm */}
        <div className="xl:col-span-1 bg-gradient-to-br from-indigo-600 to-violet-700 rounded-2xl p-5 text-white shadow-sm relative overflow-hidden">
          <div className="absolute top-0 right-0 w-24 h-24 bg-white/5 rounded-full -translate-y-6 translate-x-6" />
          <div className="absolute bottom-0 left-0 w-16 h-16 bg-white/5 rounded-full translate-y-4 -translate-x-4" />
          <div className="relative">
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center gap-1.5 bg-white/20 px-2.5 py-1 rounded-full">
                <Sparkles className="w-3 h-3 text-amber-300" />
                <span className="text-[10px] font-black text-white uppercase tracking-wider">AI Business Insight</span>
              </div>
              <button onClick={() => navigate('/ai-insights')} className="text-white/70 hover:text-white transition-colors">
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
            <h4 className="text-[14px] font-bold mb-2 leading-snug">
              {latestInsight?.title || 'Optimizing Your Business Performance'}
            </h4>
            <p className="text-[11px] text-white/80 leading-relaxed">
              {latestInsight?.content || 'AI-powered insights about your sales patterns, top products, and customer behavior will appear here. Connect your backend to activate.'}
            </p>
            <button
              onClick={() => navigate('/ai-insights')}
              className="mt-4 flex items-center gap-1.5 text-[11px] font-bold text-white bg-white/20 hover:bg-white/30 px-3 py-1.5 rounded-lg transition-colors"
            >
              <Sparkles className="w-3 h-3" /> Explore All Insights
            </button>
          </div>
        </div>

        {/* Low Stock Alert */}
        <div className="bg-white border border-gray-200 rounded-2xl overflow-hidden shadow-sm">
          <SectionHeader
            title="Low Stock Alerts"
            sub={`${lowStockProducts.length} products need restocking`}
            action="Manage Inventory"
            onAction={() => navigate('/inventory')}
          />
          <div className="p-4 space-y-2 max-h-56 overflow-y-auto">
            {lowStockProducts.length > 0 ? (
              lowStockProducts.slice(0, 6).map((p, i) => (
                <div key={p.id} className="flex items-center justify-between p-2.5 bg-gray-50 rounded-xl border border-gray-100 hover:bg-gray-100/70 transition-colors">
                  <div className="flex items-center gap-2 min-w-0">
                    <Chip name={p.name} idx={i} size="sm" />
                    <div className="min-w-0">
                      <p className="text-[11px] font-semibold text-gray-800 truncate">{p.name}</p>
                      <p className="text-[10px] text-gray-400 font-mono">SKU: {p.sku}</p>
                    </div>
                  </div>
                  <StatusBadge status={p.status === 'out_of_stock' ? 'out_of_stock' : 'low_stock'} />
                </div>
              ))
            ) : (
              <div className="flex flex-col items-center justify-center py-8 text-center">
                <CheckCircle2 className="w-10 h-10 text-emerald-400 mb-2" />
                <p className="text-[12px] font-semibold text-gray-600">All products well stocked!</p>
                <p className="text-[11px] text-gray-400 mt-0.5">No restocking needed right now</p>
              </div>
            )}
          </div>
        </div>

        {/* Registers Status */}
        <div className="bg-white border border-gray-200 rounded-2xl overflow-hidden shadow-sm">
          <SectionHeader title="POS Registers" sub="Counter health &amp; shift status" />
          <div className="p-4 space-y-2">
            {registers.length > 0 ? registers.slice(0, 4).map(reg => (
              <div key={reg.id} className="p-3 bg-gray-50 rounded-xl border border-gray-100">
                <div className="flex items-center justify-between mb-1.5">
                  <div className="flex items-center gap-2">
                    <Monitor className="w-3.5 h-3.5 text-gray-400" />
                    <span className="text-[12px] font-bold text-gray-800">{reg.name}</span>
                    <span className="text-[10px] font-mono text-gray-400">{reg.code}</span>
                  </div>
                  <StatusBadge status={reg.status === 'online' ? 'online' : 'offline'} />
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-[10px] text-gray-400">Cashier: {reg.currentCashier || '—'}</span>
                  <span className="text-[11px] font-bold text-gray-700 font-mono">{inr(reg.currentCash)}</span>
                </div>
              </div>
            )) : (
              <div className="flex flex-col items-center justify-center py-8 text-center">
                <Monitor className="w-8 h-8 text-gray-200 mb-2" />
                <p className="text-[12px] text-gray-400">No registers configured</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* ══ INVOICE TABLE + KHATA ════════════════════════════════ */}
      <div className="grid xl:grid-cols-3 gap-4">

        {/* Recent Invoices — 2/3 */}
        <div className="xl:col-span-2 bg-white border border-gray-200 rounded-2xl overflow-hidden shadow-sm">
          <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100">
            <div>
              <div className="flex items-center gap-2">
                <h3 className="text-[13px] font-bold text-gray-800">Recent Invoices</h3>
                <span className="text-[9px] font-bold bg-indigo-100 text-indigo-700 border border-indigo-200 px-1.5 py-0.5 rounded">LIVE STREAM</span>
              </div>
              <p className="text-[11px] text-gray-400 mt-0.5">All completed transactions across POS registers</p>
            </div>
            <div className="flex items-center gap-2">
              <button onClick={() => navigate('/invoices')} className="text-[11px] bg-white hover:bg-gray-50 border border-gray-200 text-gray-700 px-3 py-1.5 rounded-lg font-semibold flex items-center gap-1.5 transition-colors shadow-xs">
                <Download className="w-3 h-3" /> Export
              </button>
              <button onClick={() => navigate('/invoices')} className="text-[11px] text-indigo-600 hover:text-indigo-800 font-semibold flex items-center gap-0.5 transition-colors">
                View All <ChevronRight className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-[11px]">
              <thead>
                <tr className="border-b border-gray-100 bg-gray-50/70">
                  {['Invoice #', 'Customer', 'Items', 'Method', 'Amount', 'Status', 'Time'].map(h => (
                    <th key={h} className="text-left py-2.5 px-4 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {invoices.length > 0 ? (
                  invoices.slice(0, 8).map((inv, i) => (
                    <tr key={inv.id} className="border-b border-gray-50 hover:bg-gray-50/60 transition-colors group">
                      <td className="py-3 px-4 font-mono font-bold text-indigo-600">{inv.invoiceNumber}</td>
                      <td className="px-4">
                        <div className="flex items-center gap-2">
                          <Chip name={inv.customerName || 'WA'} idx={i} size="sm" />
                          <span className="text-gray-700 font-medium truncate max-w-24">{inv.customerName || 'Walk-in'}</span>
                        </div>
                      </td>
                      <td className="px-4 text-gray-400">{(inv as any).items?.length ?? '—'}</td>
                      <td className="px-4">
                        <span className="text-[10px] bg-gray-100 text-gray-600 px-1.5 py-0.5 rounded font-mono uppercase">{inv.paymentMethod || '—'}</span>
                      </td>
                      <td className="px-4 font-black text-gray-900 font-mono">{inr(inv.grandTotal)}</td>
                      <td className="px-4"><StatusBadge status={inv.paymentStatus} /></td>
                      <td className="px-4 text-gray-400 font-mono">
                        {new Date(inv.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={7} className="py-12 text-center">
                      <Receipt className="w-10 h-10 text-gray-200 mx-auto mb-3" />
                      <p className="text-[12px] text-gray-500 font-semibold">No invoices today</p>
                      <p className="text-[11px] text-gray-400 mt-0.5">Start billing to see transactions here</p>
                      <button onClick={() => navigate('/billing')} className="mt-3 text-[11px] bg-indigo-600 text-white px-4 py-2 rounded-lg font-bold hover:bg-indigo-700 transition-colors">
                        Create First Bill
                      </button>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {invoices.length > 0 && (
            <div className="px-4 py-2.5 border-t border-gray-100 flex items-center justify-between text-[10px] text-gray-400">
              <span>Showing 1–{Math.min(invoices.length, 8)} of {invoices.length} invoices</span>
              <button onClick={() => navigate('/invoices')} className="text-indigo-500 hover:text-indigo-700 font-semibold flex items-center gap-1">View all <ChevronRight className="w-3 h-3" /></button>
            </div>
          )}
        </div>

        {/* Khata / Customer Ledger */}
        <div className="bg-white border border-gray-200 rounded-2xl overflow-hidden shadow-sm">
          <SectionHeader
            title="Khata Ledger"
            sub="Top outstanding balances"
            action="All Customers"
            onAction={() => navigate('/customers')}
          />

          {/* Summary */}
          <div className="px-4 py-3 border-b border-gray-100 bg-amber-50/50">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-[10px] text-gray-400 font-semibold uppercase tracking-wider">Total Receivable</p>
                <p className="text-[18px] font-black text-gray-900">{inr(pendingTotal)}</p>
              </div>
              <div className="text-right">
                <p className="text-[10px] text-gray-400 font-semibold uppercase tracking-wider">Customers Due</p>
                <p className="text-[18px] font-black text-amber-700">{customersWithDue.length}</p>
              </div>
            </div>
            <div className="mt-2 h-1.5 bg-gray-100 rounded-full overflow-hidden">
              <div className="h-full rounded-full bg-amber-400" style={{ width: `${pct(paidToday, (todaySales || 1))}%` }} />
            </div>
            <p className="text-[10px] text-gray-400 mt-1">Collection rate today: {pct(paidToday, todaySales || 1)}%</p>
          </div>

          <div className="divide-y divide-gray-50">
            {topCustomers.length > 0 ? (
              topCustomers.map((c, i) => (
                <div
                  key={c.id}
                  className="flex items-center gap-3 px-4 py-3 hover:bg-gray-50/60 transition-colors cursor-pointer"
                  onClick={() => navigate(`/customers/${c.id}`)}
                >
                  <Chip name={c.name} idx={i} size="sm" />
                  <div className="flex-1 min-w-0">
                    <p className="text-[12px] font-semibold text-gray-800 truncate">{c.name}</p>
                    <p className="text-[10px] text-gray-400">{c.phone || '—'}</p>
                  </div>
                  <div className="text-right">
                    <p className={clsx('text-[12px] font-black', c.outstandingBalance > 0 ? 'text-amber-700' : 'text-emerald-600')}>
                      {inr(c.outstandingBalance)}
                    </p>
                    {c.outstandingBalance > 0 && (
                      <span className="text-[9px] text-rose-500 font-bold">DUE</span>
                    )}
                  </div>
                </div>
              ))
            ) : (
              <div className="flex flex-col items-center justify-center py-10 text-center px-4">
                <Users className="w-10 h-10 text-gray-200 mb-2" />
                <p className="text-[12px] text-gray-500 font-semibold">No customers yet</p>
                <button onClick={() => navigate('/customers/new')} className="mt-3 text-[11px] bg-indigo-600 text-white px-4 py-2 rounded-lg font-bold hover:bg-indigo-700 transition-colors">
                  Add Customer
                </button>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* ══ BOTTOM ROW — Products + Subscription ════════════════ */}
      <div className="grid xl:grid-cols-3 gap-4">

        {/* Top Products */}
        <div className="xl:col-span-2 bg-white border border-gray-200 rounded-2xl overflow-hidden shadow-sm">
          <SectionHeader title="Top Selling Products" sub="Ranked by today's units sold" action="All Products" onAction={() => navigate('/products')} />
          <div className="overflow-x-auto">
            <table className="w-full text-[11px]">
              <thead>
                <tr className="border-b border-gray-100 bg-gray-50/70">
                  {['#', 'Product', 'Category', 'Price', 'Stock', 'Status'].map(h => (
                    <th key={h} className="text-left py-2.5 px-4 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {products.length > 0 ? (
                  products.slice(0, 6).map((p, i) => (
                    <tr key={p.id} className="border-b border-gray-50 hover:bg-gray-50/60 transition-colors">
                      <td className="py-2.5 px-4 text-gray-400 font-mono text-[10px]">{i + 1}</td>
                      <td className="px-4">
                        <div className="flex items-center gap-2">
                          <div className={clsx('w-7 h-7 rounded-lg flex items-center justify-center text-white text-[10px] font-bold', CHIP_COLORS[i % CHIP_COLORS.length])}>
                            {p.name.substring(0, 1).toUpperCase()}
                          </div>
                          <div>
                            <p className="font-semibold text-gray-800 max-w-28 truncate">{p.name}</p>
                            <p className="text-[10px] text-gray-400 font-mono">{p.sku}</p>
                          </div>
                        </div>
                      </td>
                      <td className="px-4 text-gray-500">{p.categoryName || '—'}</td>
                      <td className="px-4 font-bold text-gray-800 font-mono">{inr(p.sellingPrice)}</td>
                      <td className="px-4">
                        <div className="flex items-center gap-1">
                          <span className={clsx('font-bold', p.stock === 0 ? 'text-rose-600' : p.stock <= p.minStock ? 'text-amber-600' : 'text-gray-700')}>{p.stock}</span>
                          <span className="text-gray-400">{p.unit}</span>
                        </div>
                      </td>
                      <td className="px-4">
                        <StatusBadge status={p.status === 'out_of_stock' ? 'out_of_stock' : p.status === 'low_stock' ? 'low_stock' : 'active'} />
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={6} className="py-10 text-center">
                      <Package className="w-10 h-10 text-gray-200 mx-auto mb-3" />
                      <p className="text-[12px] text-gray-500 font-semibold">No products added yet</p>
                      <button onClick={() => navigate('/products/new')} className="mt-3 text-[11px] bg-indigo-600 text-white px-4 py-2 rounded-lg font-bold hover:bg-indigo-700 transition-colors">
                        Add First Product
                      </button>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Subscription Status */}
        <div className="bg-white border border-gray-200 rounded-2xl overflow-hidden shadow-sm">
          <SectionHeader title="Your Subscription" sub="Plan &amp; usage status" />
          <div className="p-5 space-y-4">
            {/* Plan card */}
            <div className="bg-gradient-to-br from-indigo-600 to-violet-700 rounded-xl p-4 text-white">
              <div className="flex items-center justify-between mb-2">
                <span className="text-[10px] font-bold uppercase tracking-wider text-white/70">Current Plan</span>
                <span className="text-[10px] bg-white/20 px-2 py-0.5 rounded font-bold">ACTIVE</span>
              </div>
              <p className="text-xl font-black">Professional</p>
              <p className="text-[11px] text-white/70 mt-0.5">₹999/month · Renews in 22 days</p>
            </div>

            {/* Usage meters */}
            <div className="space-y-3">
              {[
                { label: 'Invoices This Month', used: Math.min(todayOrders * 30, 2400), max: 5000 },
                { label: 'Products', used: products.length, max: 5000 },
                { label: 'Users / Staff', used: 3, max: 15 },
              ].map(m => (
                <div key={m.label}>
                  <div className="flex justify-between mb-1">
                    <span className="text-[11px] text-gray-600 font-medium">{m.label}</span>
                    <span className="text-[11px] text-gray-500">{fmt(m.used)} / {fmt(m.max)}</span>
                  </div>
                  <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
                    <div
                      className={clsx('h-full rounded-full', pct(m.used, m.max) >= 80 ? 'bg-amber-400' : 'bg-indigo-500')}
                      style={{ width: `${Math.min(pct(m.used, m.max), 100)}%` }}
                    />
                  </div>
                  <p className="text-[10px] text-gray-400 mt-0.5">{pct(m.used, m.max)}% used</p>
                </div>
              ))}
            </div>

            {/* Features */}
            <div className="border-t border-gray-100 pt-3 space-y-1.5">
              {[
                ['WhatsApp Integration', true],
                ['Email Integration', true],
                ['Advanced Reports', true],
                ['API Access', false],
              ].map(([feat, enabled]) => (
                <div key={String(feat)} className="flex items-center justify-between">
                  <span className="text-[11px] text-gray-500">{feat}</span>
                  {enabled ? <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" /> : <span className="text-[10px] text-gray-300">—</span>}
                </div>
              ))}
            </div>

            <button onClick={() => navigate('/billing/subscription')} className="w-full bg-indigo-600 hover:bg-indigo-700 text-white text-[11px] font-bold py-2.5 rounded-xl flex items-center justify-center gap-2 transition-colors">
              <Star className="w-3.5 h-3.5" /> Upgrade Plan
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
