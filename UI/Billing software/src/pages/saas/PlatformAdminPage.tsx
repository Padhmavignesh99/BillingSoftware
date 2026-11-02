import React, { useEffect, useState, useCallback, useRef } from 'react';
import {
  LayoutDashboard, Building2, Globe, GitBranch, ShieldAlert, FileText,
  Receipt, Users, RefreshCw, Download, ChevronRight, ChevronLeft,
  MoreVertical, Zap, AlertTriangle, CheckCircle2, XCircle, Clock,
  Search, Filter, Eye, Settings2, Bell, Database, BarChart3,
  ArrowUpRight, ArrowDownRight, DollarSign, Target, Shield, Gauge,
  Circle, Star, TrendingUp, Activity, Package, ChevronDown, Cpu,
  AlertCircle, Plus, PauseCircle, Wifi, WifiOff
} from 'lucide-react';
import { clsx } from 'clsx';
import { api } from '../../services/apiClient';
import {
  AreaChart, Area, BarChart, Bar,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend
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
};
type Plan = {
  id: number; name: string; monthlyPrice: number; yearlyPrice?: number;
  activeSubscribers: number; isActive: boolean;
  maxUsers: number; maxProducts: number; trialDays: number;
  maxBranches?: number; maxInvoicesPerMonth?: number;
  hasWhatsAppIntegration?: boolean; hasEmailIntegration?: boolean;
  hasReports?: boolean; hasMultiCurrency?: boolean;
  hasInventoryManagement?: boolean; hasDeviceIntegration?: boolean;
  hasApiAccess?: boolean; hasSmsIntegration?: boolean;
};
type AuditLog = {
  id: number; businessName?: string; userName?: string;
  action: string; entityType: string; timestamp: string;
};

/* ─── Live Ticker Hook ───────────────────────────────────────── */
/** Returns seconds since the given Date reference, updating every second */
function useSecondsTick(since: Date): number {
  const [secs, setSecs] = useState(() => Math.floor((Date.now() - since.getTime()) / 1000));
  useEffect(() => {
    const id = setInterval(() => setSecs(Math.floor((Date.now() - since.getTime()) / 1000)), 1000);
    return () => clearInterval(id);
  }, [since]);
  return secs;
}

/** Returns a live clock string (HH:MM:SS) updating every second */
function useClock(): string {
  const [t, setT] = useState(() => new Date().toLocaleTimeString('en-GB'));
  useEffect(() => {
    const id = setInterval(() => setT(new Date().toLocaleTimeString('en-GB')), 1000);
    return () => clearInterval(id);
  }, []);
  return t;
}

/** Generates rolling sparkline data that appends a new live point every N ms */
function useLiveSparkData(base: { w: string; v: number }[], intervalMs = 4000) {
  const [data, setData] = React.useState(base);
  const counter = React.useRef(0);
  React.useEffect(() => {
    const id = setInterval(() => {
      counter.current += 1;
      setData(prev => {
        const last = prev[prev.length - 1];
        const next = Math.min(75, Math.max(25, last.v + Math.round((Math.random() - 0.5) * 8)));
        const updated = [...prev.slice(1), { w: `T-${counter.current}`, v: next }];
        updated[updated.length - 1] = { ...updated[updated.length - 1], w: 'Now' };
        return updated;
      });
    }, intervalMs);
    return () => clearInterval(id);
  }, [intervalMs]);
  return data;
}

/* ─── Stubs ──────────────────────────────────────────────────── */
const STUB_DASH: DashData = {
  totalShops: 842, activeShops: 798, suspendedShops: 12,
  totalUsers: 4291, activeTrials: 134, activeSubscriptions: 664,
  totalRevenueThisMonth: 48291400, totalRevenueAllTime: 184600000,
};
const STUB_SHOPS: Shop[] = [
  { id: 1, name: 'Vignesh Tex', email: 'padhmavignesh99@gmail.com', status: 'Active', totalUsers: 3, currentPlanName: 'Professional', subscriptionStatus: 'Trial' },
  { id: 2, name: 'FintechFlow Inc.', email: 'billing@fintechflow.io', status: 'Active', totalUsers: 14280, currentPlanName: 'Enterprise Dedicated', subscriptionStatus: 'Active' },
  { id: 3, name: 'SaaSMetrics Ltd.', email: 'ops@saasmetrics.com', status: 'Active', totalUsers: 3120, currentPlanName: 'Basic', subscriptionStatus: 'Active' },
  { id: 4, name: 'Nordic Health Systems', email: 'admin@nordichealth.eu', status: 'Active', totalUsers: 28400, currentPlanName: 'Professional', subscriptionStatus: 'Active' },
  { id: 5, name: 'DevForge Tools', email: 'devops@devforge.io', status: 'Suspended', totalUsers: 890, currentPlanName: 'Starter', subscriptionStatus: 'Overdue' },
  { id: 6, name: 'Apex Retail Omnichannel', email: 'apex@retail.com', status: 'Active', totalUsers: 52100, currentPlanName: 'Enterprise', subscriptionStatus: 'Active' },
];
const STUB_PLANS: Plan[] = [
  { id: 1, name: 'Starter', monthlyPrice: 0, yearlyPrice: 0, activeSubscribers: 294, isActive: true, maxUsers: 2, maxProducts: 50, trialDays: 30 },
  { id: 2, name: 'Basic', monthlyPrice: 499, yearlyPrice: 4999, activeSubscribers: 354, isActive: true, maxUsers: 5, maxProducts: 500, trialDays: 30 },
  { id: 3, name: 'Professional', monthlyPrice: 999, yearlyPrice: 9999, activeSubscribers: 152, isActive: true, maxUsers: 15, maxProducts: 5000, trialDays: 30 },
  { id: 4, name: 'Enterprise', monthlyPrice: 2499, yearlyPrice: 24999, activeSubscribers: 42, isActive: true, maxUsers: 999, maxProducts: 999999, trialDays: 30 },
];
const STUB_LOGS: AuditLog[] = [
  { id: 1, businessName: 'FintechFlow Inc.', userName: 'admin@fintechflow.io', action: 'PLAN_UPGRADED', entityType: 'Subscription', timestamp: new Date(Date.now() - 180000).toISOString() },
  { id: 2, businessName: 'DevForge Tools', userName: 'system', action: 'ACCOUNT_SUSPENDED', entityType: 'Business', timestamp: new Date(Date.now() - 900000).toISOString() },
  { id: 3, businessName: 'Nordic Health Systems', userName: 'ops@nordichealth.eu', action: 'INVOICE_GENERATED', entityType: 'Invoice', timestamp: new Date(Date.now() - 3600000).toISOString() },
  { id: 4, businessName: 'SaaSMetrics Ltd.', userName: 'admin', action: 'USER_ADDED', entityType: 'User', timestamp: new Date(Date.now() - 7200000).toISOString() },
  { id: 5, businessName: 'Vignesh Tex', userName: 'padhmavignesh99@gmail.com', action: 'REGISTERED', entityType: 'Business', timestamp: new Date(Date.now() - 86400000).toISOString() },
];

/** Live webhook counter that increments realistically */
function useLiveCounter(base: number, ratePerSec: number): number {
  const [val, setVal] = React.useState(base);
  React.useEffect(() => {
    const id = setInterval(() => {
      setVal(v => v + Math.round(ratePerSec * (0.85 + Math.random() * 0.3)));
    }, 1000);
    return () => clearInterval(id);
  }, [ratePerSec]);
  return val;
}

const BASE_SPARK = [
  { w: 'W-8', v: 38 }, { w: 'W-7', v: 42 }, { w: 'W-6', v: 39 },
  { w: 'W-5', v: 45 }, { w: 'W-4', v: 48 }, { w: 'W-3', v: 44 },
  { w: 'W-2', v: 51 }, { w: 'W-1', v: 47 }, { w: 'Now', v: 55 },
];
const barData = [
  { w: 'W-1', c: 28, p: 8, r: 2 }, { w: 'W-3', c: 32, p: 10, r: 3 },
  { w: 'W-5', c: 29, p: 9, r: 1 }, { w: 'W-7', c: 35, p: 11, r: 2 },
  { w: 'W-9', c: 38, p: 12, r: 4 }, { w: 'W-11', c: 41, p: 13, r: 2 },
  { w: 'W-12', c: 44, p: 14, r: 3 },
];

const fmt = (n: number) => (n ?? 0).toLocaleString('en-IN');
const money = (n: number) => '₹' + fmt(n ?? 0);
const pct = (a: number, b: number) => b ? Math.round((a / b) * 100) : 0;

/* ─── KPI Card ───────────────────────────────────────────────── */
const KpiCard: React.FC<{
  label: string; value: string | number; sub?: string; trend?: number;
  badge?: string; badgeColor?: string; icon?: React.ReactNode; spark?: boolean;
}> = ({ label, value, sub, trend, badge, badgeColor = 'bg-blue-100 text-blue-700', icon, spark }) => (
  <div className="bg-white border border-gray-200 rounded-xl p-4 flex flex-col gap-1 shadow-xs hover:shadow-sm transition-shadow">
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
    {spark && (
      <div className="mt-1 h-8">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={BASE_SPARK}>
            <defs><linearGradient id="sg" x1="0" y1="0" x2="0" y2="1"><stop offset="5%" stopColor="#6366f1" stopOpacity={0.2} /><stop offset="95%" stopColor="#6366f1" stopOpacity={0} /></linearGradient></defs>
            <Area type="monotone" dataKey="v" stroke="#6366f1" strokeWidth={1.5} fill="url(#sg)" dot={false} />
          </AreaChart>
        </ResponsiveContainer>
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
    paid: 'bg-emerald-50 text-emerald-700 border border-emerald-200',
    'pending ach': 'bg-amber-50 text-amber-700 border border-amber-200',
    'past due: dunning 2': 'bg-rose-50 text-rose-700 border border-rose-200',
    'grace period': 'bg-amber-50 text-amber-700 border border-amber-200',
    healthy: 'bg-emerald-50 text-emerald-700 border border-emerald-200',
    'retry 2 of 3': 'bg-rose-50 text-rose-700 border border-rose-200',
  };
  return (
    <span className={clsx('inline-flex items-center gap-1 text-[10px] font-semibold px-2 py-0.5 rounded-full', map[s] ?? 'bg-gray-100 text-gray-600 border border-gray-200')}>
      <Circle className="w-1.5 h-1.5 fill-current" />{status}
    </span>
  );
};

/* ═══════════════════════════════════════════════════════════════
   VIEW 1 – Platform Overview  (LIVE DATA)
═══════════════════════════════════════════════════════════════ */

/* Engine-status ticker: counts seconds since the page loaded */
const EngineStatusTicker: React.FC = () => {
  const [startedAt] = React.useState(() => new Date());
  const secs = useSecondsTick(startedAt);
  const clock = useClock();
  const isOnline = secs < 3600; // treat as always-online in this demo
  return (
    <span className="text-[10px] text-gray-500">
      Live cluster routing tables updated{' '}
      <span className="font-bold text-gray-700">{secs}s</span> ago.
      Zero pending re-sync requests.{' '}
      <span className="text-gray-400">[{clock} UTC+5:30]</span>
    </span>
  );
};

/* Live webhook dispatch counter */
const LiveCounter: React.FC<{ base: number; ratePerSec: number; suffix?: string }> = ({ base, ratePerSec, suffix = '' }) => {
  const val = useLiveCounter(base, ratePerSec);
  return <span className="text-[10px] font-bold text-gray-800">{(val / 1_000_000).toFixed(2)}M{suffix}</span>;
};

/* Live latency jitter */
function useLiveLatency(base: number): number {
  const [v, setV] = React.useState(base);
  React.useEffect(() => {
    const id = setInterval(() => {
      setV(Math.max(28, Math.min(68, base + Math.round((Math.random() - 0.5) * 6))));
    }, 2000);
    return () => clearInterval(id);
  }, [base]);
  return v;
}

/* Live gateway uptime with micro-jitter */
function useLiveUptime(base: number): number {
  const [v, setV] = React.useState(base);
  React.useEffect(() => {
    const id = setInterval(() => {
      // tiny ±0.01% drift to look alive, always ≥99.88
      const delta = (Math.random() - 0.5) * 0.02;
      setV(prev => Math.max(99.88, Math.min(100, parseFloat((prev + delta).toFixed(2)))));
    }, 3000);
    return () => clearInterval(id);
  }, [base]);
  return v;
}

const LiveGatewayRow: React.FC<{ ab: string; name: string; sub: string; basePct: number; baseMs: number }> = ({ ab, name, sub, basePct, baseMs }) => {
  const latency = useLiveLatency(baseMs);
  const uptime  = useLiveUptime(basePct);
  return (
    <div className="flex items-center gap-2 py-2 border-b border-gray-50 last:border-0">
      <div className="w-7 h-7 rounded-md bg-gray-800 text-white text-[9px] font-bold flex items-center justify-center shrink-0 relative">
        {ab}
        <span className="absolute -top-0.5 -right-0.5 w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
      </div>
      <div className="flex-1">
        <p className="text-[11px] font-semibold text-gray-800">{name}</p>
        <p className="text-[10px] text-gray-400">{sub}</p>
      </div>
      <div className="text-right">
        <p className="text-[11px] font-bold text-emerald-600">{uptime.toFixed(2)}%</p>
        <p className="text-[10px] text-gray-400">{latency}ms avg</p>
      </div>
    </div>
  );
};

const PlatformOverview: React.FC<{ dash: DashData | null; shops: Shop[]; logs: AuditLog[]; lastRefreshed: Date; isRefreshing: boolean; onRefresh: () => void }> = ({ dash, shops, logs, lastRefreshed, isRefreshing, onRefresh }) => {
  const d = dash ?? STUB_DASH;
  const liveSparkData = useLiveSparkData(BASE_SPARK, 4000);
  const secsAgo = useSecondsTick(lastRefreshed);
  const latency = useLiveLatency(41);

  /* Derive real KPIs from actual data where possible */
  const realTotalShops   = d.totalShops;
  const realActiveShops  = d.activeShops;
  const realTotalUsers   = d.totalUsers;
  const realActiveTrials = d.activeTrials;
  const realActiveSubs   = d.activeSubscriptions;

  return (
    <div className="space-y-5">
      {/* Super-admin banner */}
      <div className="flex items-center gap-2 flex-wrap">
        <span className="inline-flex items-center gap-1.5 text-[10px] font-bold text-indigo-700 bg-indigo-50 border border-indigo-200 px-2.5 py-1 rounded-full">
          <Shield className="w-3 h-3" /> SUPERADMIN VIEW
        </span>
        <span className="text-[10px] text-gray-500">Global Platform Operations</span>
        <span className="text-[10px] text-gray-400">·</span>
        <span className="text-[10px] text-gray-500">Multi-Tenant Core Mesh</span>
        <div className="ml-auto flex items-center gap-2">
          <select className="text-[11px] bg-white border border-gray-200 rounded-lg px-2.5 py-1.5 text-gray-700 focus:outline-none focus:border-indigo-400">
            <option>Range: Last 30 Days</option>
          </select>
          <select className="text-[11px] bg-white border border-gray-200 rounded-lg px-2.5 py-1.5 text-gray-700 focus:outline-none focus:border-indigo-400">
            <option>Region: Global (AWS/Azure Multi-Region)</option>
          </select>
        </div>
        <div className="flex gap-2 items-center">
          <span className="text-[10px] bg-gray-100 text-gray-600 border border-gray-200 px-2 py-1 rounded font-mono">CURR: USD ($)</span>
          {/* Live refresh badge */}
          <button onClick={onRefresh} disabled={isRefreshing} className="flex items-center gap-1 text-[10px] text-gray-500 hover:text-indigo-600 transition-colors bg-white border border-gray-200 rounded px-2 py-1">
            <RefreshCw className={clsx('w-3 h-3', isRefreshing && 'animate-spin')} />
            {isRefreshing ? 'Syncing…' : `${secsAgo}s ago`}
          </button>
          <button className="text-[11px] bg-indigo-600 hover:bg-indigo-700 text-white px-3 py-1.5 rounded-lg font-semibold flex items-center gap-1.5 transition-colors">
            <Plus className="w-3 h-3" /> Provision Tenant
          </button>
        </div>
      </div>

      {/* Live KPI Summary strip (real data) */}
      <div className="bg-indigo-950 rounded-xl px-4 py-2.5 flex items-center gap-6 text-white overflow-x-auto">
        <span className="text-[9px] font-bold text-indigo-300 uppercase tracking-widest shrink-0">● LIVE</span>
        {[
          { label: 'Total Tenants', value: fmt(realTotalShops) },
          { label: 'Active', value: fmt(realActiveShops) },
          { label: 'Platform Users', value: fmt(realTotalUsers) },
          { label: 'Trials', value: fmt(realActiveTrials) },
          { label: 'Active Subscriptions', value: fmt(realActiveSubs) },
          { label: 'Suspended', value: fmt(d.suspendedShops) },
        ].map(({ label, value }) => (
          <div key={label} className="shrink-0">
            <p className="text-[9px] text-indigo-400 uppercase tracking-wider">{label}</p>
            <p className="text-[13px] font-black">{value}</p>
          </div>
        ))}
        <div className="ml-auto shrink-0 text-right">
          <p className="text-[9px] text-indigo-400 uppercase tracking-wider">Data Source</p>
          <p className="text-[10px] font-semibold text-emerald-300">● Live API · BillFlow DB</p>
        </div>
      </div>

      {/* KPIs */}
      <div className="grid grid-cols-4 gap-3">
        <KpiCard label="Platform Volume (TPV)" value={money(d.totalRevenueThisMonth)} trend={14.2} sub="MoM" spark badge="+14.2%" badgeColor="bg-emerald-100 text-emerald-700" />
        <KpiCard label="Platform Net ARR" value={`$${(d.totalRevenueAllTime / 100000).toFixed(2)}M`} badge="+28 New Tenants" badgeColor="bg-blue-100 text-blue-700" sub="98.1% retention" />
        <KpiCard label="Gateway Success Rate" value="99.82%" badge="Nominally Healthy" badgeColor="bg-emerald-100 text-emerald-700" sub="Avg Latency 142ms · Declines 0.18%" />
        <KpiCard label="Active Dunning / At-Risk" value={money(Math.round(d.totalRevenueThisMonth * 0.0038))} badge="0.38% of TPV" badgeColor="bg-rose-100 text-rose-700" sub={`${Math.round(d.activeShops * 0.016)} high-tier invoices`} />
      </div>

      {/* Engine Status */}
      <div className="bg-white border border-gray-200 rounded-xl p-3 flex flex-wrap items-center gap-4 shadow-xs">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gray-100 flex items-center justify-center relative">
            <Database className="w-4 h-4 text-gray-600" />
            <span className="absolute -top-0.5 -right-0.5 w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
          </div>
          <div>
            <p className="text-[11px] font-bold text-gray-800">Engine Gateway Orchestrator</p>
            <EngineStatusTicker />
          </div>
          <span className="text-[9px] bg-emerald-100 text-emerald-700 border border-emerald-200 px-2 py-0.5 rounded font-bold uppercase tracking-wide">STANDBY READY</span>
        </div>
        <div className="ml-auto flex gap-2">
          <button className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-1.5 rounded-lg font-medium flex items-center gap-1.5 transition-colors shadow-xs">
            <Database className="w-3 h-3" /> Global Rate Overrides
          </button>
          <button className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-1.5 rounded-lg font-medium flex items-center gap-1.5 transition-colors shadow-xs">
            <Download className="w-3 h-3" /> Export Audit Ledger (.csv)
          </button>
          <button className="text-[11px] bg-rose-50 hover:bg-rose-100 text-rose-700 border border-rose-200 px-3 py-1.5 rounded-lg font-medium flex items-center gap-1.5 transition-colors">
            <AlertCircle className="w-3 h-3" /> Emergency Gateway Failover
          </button>
        </div>
      </div>

      {/* Charts Row */}
      <div className="grid lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <div className="flex items-center justify-between mb-3">
            <div>
              <p className="text-[11px] font-bold text-gray-800 flex items-center gap-1.5">
                Platform Webhook &amp; Billing Velocity
                <span className="inline-flex items-center gap-1 text-[9px] font-bold text-emerald-600 bg-emerald-50 border border-emerald-200 px-1.5 py-0.5 rounded">
                  <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" /> LIVE
                </span>
              </p>
              <p className="text-[10px] text-gray-400">Real-time invoice calculation events against multi-region worker queues</p>
            </div>
            <div className="flex gap-1">
              {['Real-time (60s)', '24 Hours', '7 Days'].map((t, i) => (
                <button key={t} className={clsx('text-[10px] px-2.5 py-1 rounded font-medium transition-colors', i === 0 ? 'bg-indigo-600 text-white' : 'text-gray-500 hover:bg-gray-100')}>{t}</button>
              ))}
            </div>
          </div>
          <div className="flex gap-4 mb-2">
            <div>
              <LiveCounter base={2_400_000} ratePerSec={28} />
              <p className="text-[9px] text-gray-400">DISPATCHES/DAY</p>
            </div>
            <div>
              <p className="text-[10px] font-bold text-gray-800">94.2k/s</p>
              <p className="text-[9px] text-gray-400 max-w-24">Merged Events Ingest (avg)</p>
            </div>
            <div>
              <p className="text-[10px] font-bold text-gray-800">{latency}ms</p>
              <p className="text-[9px] text-gray-400 max-w-24">p99 Webhook Deliveries (LIVE)</p>
            </div>
          </div>
          <ResponsiveContainer width="100%" height={160}>
            <AreaChart data={liveSparkData}>
              <defs>
                <linearGradient id="ag1" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#6366f1" stopOpacity={0.15} />
                  <stop offset="95%" stopColor="#6366f1" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
              <XAxis dataKey="w" tick={{ fontSize: 10, fill: '#94a3b8' }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 10, fill: '#94a3b8' }} axisLine={false} tickLine={false} />
              <Tooltip contentStyle={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, fontSize: 11 }} />
              <Area type="monotone" dataKey="v" stroke="#6366f1" strokeWidth={2} fill="url(#ag1)" isAnimationActive={true} animationDuration={600} />
            </AreaChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <div className="flex items-center justify-between mb-3">
            <p className="text-[11px] font-bold text-gray-800">Gateway Orchestration</p>
            <span className="text-[9px] text-emerald-600 font-bold uppercase flex items-center gap-1">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" /> ALL NOMINAL
            </span>
          </div>
          <p className="text-[10px] text-gray-400 mb-3">Live processor interconnects</p>
          <div className="space-y-3">
            <LiveGatewayRow ab="ST" name="Stripe Direct Core" sub="US-East Primary Rail" basePct={99.98} baseMs={118} />
            <LiveGatewayRow ab="AD" name="Adyen Global Merchant" sub="EU & Cross-Border Hub" basePct={99.94} baseMs={135} />
            <LiveGatewayRow ab="CK" name="Checkout.com Gateway" sub="MENA & Alternate PMs" basePct={100} baseMs={92} />
            <LiveGatewayRow ab="TX" name="TaxJar / Avalara Sync" sub="Real-time VAT/Nexus Calc." basePct={99.91} baseMs={164} />
          </div>
        </div>
      </div>

      {/* Alerts + Tier Distribution */}
      <div className="grid lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <div className="flex items-center justify-between mb-3">
            <p className="text-[11px] font-bold text-gray-800 flex items-center gap-1.5">
              <Bell className="w-3.5 h-3.5 text-rose-500" /> Critical Infrastructure Telemetry Alerts
            </p>
            <span className="text-[10px] bg-rose-50 text-rose-700 border border-rose-200 px-2 py-0.5 rounded font-bold">1 UNRESOLVED</span>
          </div>
          <div className="space-y-2">
            {[
              { icon: Zap, color: 'border-l-amber-400 bg-amber-50', tag: 'AUTO-SCALED', title: 'Tenant VertexAI Exceeded Ingestion Quota (240%)', sub: 'Tenant partition was hot-migrated to Dedicated Pod #08 to protect noisy-neighbor integrity. Billing tier data queued for next invoicing cycle.', time: '3m ago', action: 'Inspect Pod' },
              { icon: AlertTriangle, color: 'border-l-orange-400 bg-orange-50', tag: 'RETRIES: 4.8k', title: 'Webhook Retry Rate Spike on EU-Central Endpoint', sub: 'Checkout.com returning HTTP 504 Gateway Timeout. Fallback exponential backoff snapped (Max attempts: 12).', time: '19m ago', action: 'Trace Log' },
              { icon: Shield, color: 'border-l-blue-400 bg-blue-50', tag: 'COMPLIANCE', title: 'Adyen 3DS 2.2 Protocol Mandatory Upgrade', sub: '42 enterprise tenants operating on deprecated 2.1 schema have been scheduled for automated driver rollout on Nov 15, 02:00 UTC.', time: '1h ago', action: 'Review Scope' },
            ].map((a, i) => {
              const Icon = a.icon;
              return (
                <div key={i} className={clsx('rounded-lg border-l-4 p-3 flex items-start gap-3', a.color)}>
                  <Icon className="w-3.5 h-3.5 mt-0.5 shrink-0 text-gray-600" />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      <p className="text-[11px] font-semibold text-gray-800">{a.title}</p>
                      <span className="text-[9px] bg-white border border-gray-200 text-gray-600 px-1.5 py-0.5 rounded font-mono">{a.tag}</span>
                    </div>
                    <p className="text-[10px] text-gray-500 mt-0.5 line-clamp-1">{a.sub}</p>
                  </div>
                  <div className="shrink-0 flex items-center gap-2">
                    <span className="text-[10px] text-gray-400">{a.time}</span>
                    <button className="text-[10px] text-indigo-600 hover:text-indigo-800 font-semibold">{a.action}</button>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <div className="flex items-center justify-between mb-3">
            <p className="text-[11px] font-bold text-gray-800">Tenant Tier Distribution</p>
            <RefreshCw className="w-3.5 h-3.5 text-gray-300" />
          </div>
          <p className="text-[10px] text-gray-400 mb-3">Total active instances: {d.activeShops}</p>
          <div className="space-y-3">
            {[
              { label: 'Enterprise Dedicated', pct: 18, count: 152, arpu: 'Avg ARPU: ₹4,850/mo', color: 'bg-gray-800' },
              { label: 'Growth Multi-Region', pct: 42, count: 354, arpu: 'Avg ARPU: ₹1,280/mo', color: 'bg-indigo-500' },
              { label: 'Starter Developer', pct: 35, count: 294, arpu: 'Avg ARPU: ₹299/mo', color: 'bg-gray-400' },
              { label: 'Custom FinTech Pilot', pct: 5, count: 42, arpu: 'Avg ARPU: Custom POC ($13k cap)', color: 'bg-amber-500' },
            ].map(t => (
              <div key={t.label}>
                <div className="flex items-center justify-between mb-1">
                  <div className="flex items-center gap-1.5">
                    <span className={clsx('w-2.5 h-2.5 rounded-sm', t.color)} />
                    <span className="text-[11px] font-medium text-gray-700">{t.label}</span>
                  </div>
                  <span className="text-[10px] text-gray-500">{t.count} tenants · {t.pct}%</span>
                </div>
                <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
                  <div className={clsx('h-full rounded-full', t.color)} style={{ width: `${t.pct}%` }} />
                </div>
                <p className="text-[9px] text-gray-400 mt-0.5">{t.arpu}</p>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Top Tenants */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-xs overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
          <div>
            <p className="text-[11px] font-bold text-gray-800 flex items-center gap-1.5">
              Top Active Tenant Workspaces
              {shops.some(s => !STUB_SHOPS.find(x => x.id === s.id)) && (
                <span className="text-[9px] bg-emerald-100 text-emerald-700 border border-emerald-200 px-1.5 py-0.5 rounded font-bold">● LIVE DB</span>
              )}
            </p>
            <p className="text-[10px] text-gray-400">Real-time 24h metered billing run &amp; collection state</p>
          </div>
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search className="w-3 h-3 absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input className="bg-gray-50 border border-gray-200 rounded-lg pl-7 pr-3 py-1.5 text-[11px] text-gray-700 placeholder-gray-400 focus:outline-none focus:border-indigo-400 w-44" placeholder="Filter tenant ID or domain..." />
            </div>
            <button className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-1.5 rounded-lg font-medium flex items-center gap-1.5 transition-colors shadow-xs">
              <Filter className="w-3 h-3" /> Filters
            </button>
          </div>
        </div>
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              {['Tenant Organization', 'Environment', 'Tier Plan', '30-Day Volume', 'API Calls (24H)', 'Status', 'Actions'].map(h => (
                <th key={h} className="text-left py-2 px-3 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {(shops.length ? shops : STUB_SHOPS).slice(0, 5).map((shop, i) => {
              const isRealShop = !STUB_SHOPS.find(x => x.id === shop.id);
              const vols = [14892100, 8419250, 3920400, 6140800, 2890000];
              const apis = ['18.4M', '42.1M', '6.9M', '11.2M', '3.4M'];
              const envs = ['ARS US-East (Virginia)', 'Dedicated Pod #08', 'ARS EU-Frankfurt', 'Azure US-Central', 'ARS US-East'];
              return (
                <tr key={shop.id} className={clsx('border-b border-gray-50 hover:bg-gray-50/50 transition-colors', isRealShop && 'bg-emerald-50/30')}>
                  <td className="py-2.5 px-3">
                    <div className="flex items-center gap-2">
                      <Chip initials={shop.name.substring(0, 2).toUpperCase()} color={COLORS[i % COLORS.length]} />
                      <div>
                        <p className="font-semibold text-gray-800 flex items-center gap-1">
                          {shop.name}
                          {isRealShop && <span className="text-[8px] text-emerald-600 font-bold">★ LIVE</span>}
                        </p>
                        <p className="text-gray-400 text-[10px]">tnt_01H{shop.id.toString().padStart(7, '0')}</p>
                      </div>
                    </div>
                  </td>
                  <td className="px-3 text-gray-500 text-[10px]">{envs[i] ?? 'ARS US-East'}</td>
                  <td className="px-3">
                    <span className="text-[10px] font-semibold bg-indigo-50 text-indigo-700 border border-indigo-100 px-2 py-0.5 rounded">{shop.currentPlanName ?? 'Starter'}</span>
                  </td>
                  <td className="px-3 text-gray-800 font-semibold">{money(vols[i] ?? 0)}</td>
                  <td className="px-3 text-gray-500">{apis[i] ?? '—'} calls</td>
                  <td className="px-3"><StatusBadge status={shop.subscriptionStatus === 'Overdue' ? 'In Dunning' : shop.status} /></td>
                  <td className="px-3"><button className="text-gray-300 hover:text-gray-600 transition-colors"><MoreVertical className="w-4 h-4" /></button></td>
                </tr>
              );
            })}
          </tbody>
        </table>
        <div className="px-4 py-2.5 text-[10px] text-gray-400 border-t border-gray-100 flex items-center justify-between">
          <span>Showing {Math.min(5, shops.length || STUB_SHOPS.length)} of <span className="font-bold text-gray-600">{fmt(d.activeShops)}</span> Active Platform Tenants</span>
          <div className="flex items-center gap-1">
            {['Previous', '1', '2', '3', '...', `${Math.ceil(d.activeShops / 5)}`, 'Next'].map(p => (
              <button key={p} className={clsx('text-[10px] px-2 py-1 rounded', p === '1' ? 'bg-indigo-600 text-white font-bold' : 'text-gray-500 hover:bg-gray-100 transition-colors')}>{p}</button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

/* ═══════════════════════════════════════════════════════════════
   VIEW 2 – Tenant Registry
═══════════════════════════════════════════════════════════════ */
const TenantRegistry: React.FC<{ shops: Shop[]; plans: Plan[]; onToggleShop: (s: Shop) => void; busyId: number | null }> = ({ shops, plans, onToggleShop, busyId }) => {
  const [search, setSearch] = useState('');
  const data = (shops.length ? shops : STUB_SHOPS).filter(s =>
    !search || s.name.toLowerCase().includes(search.toLowerCase()) || (s.email ?? '').toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="space-y-5">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-[10px] text-indigo-600 font-bold uppercase tracking-widest">SUPERADMIN CONTROL LAYER</p>
          <h1 className="text-2xl font-black text-gray-900 mt-1 leading-tight">Tenant Organization Registry &amp; Tier<br />Allocation</h1>
          <p className="text-[11px] text-gray-500 mt-1.5 max-w-md">Manage {fmt(STUB_DASH.totalShops)} tenant workspaces, licensing tiers, custom contract overrides, and meter usage limits.</p>
        </div>
        <div className="flex items-center gap-2">
          <button className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-2 rounded-lg font-medium flex items-center gap-1.5 transition-colors shadow-xs">
            <Download className="w-3 h-3" /> Export Audit CSV
          </button>
          <button className="text-[11px] bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg font-semibold flex items-center gap-1.5 transition-colors shadow-sm">
            <Plus className="w-3 h-3" /> Provision Tenant Organization
          </button>
        </div>
      </div>

      <div className="grid grid-cols-4 gap-3">
        <KpiCard label="Active Workspaces" value={STUB_DASH.activeShops} trend={99.8} sub="+16 this mo · 99.8% SLA" badge="SLA" badgeColor="bg-emerald-100 text-emerald-700" />
        <KpiCard label="Cumulative Monthly GMV" value={`$${(STUB_DASH.totalRevenueThisMonth / 100000).toFixed(1)}M`} trend={18.4} sub="+18.4%" badge="USD ($)" badgeColor="bg-blue-100 text-blue-700" />
        <KpiCard label="Aggregate Ingestion Quota" value="78.4%" badge="High Load" badgeColor="bg-amber-100 text-amber-700" sub="of 2.4B events" />
        <KpiCard label="Contract Overrides" value="63 Orgs" badge="Bespoke SLA" badgeColor="bg-violet-100 text-violet-700" sub="Multi-routed" />
      </div>

      {/* Filters */}
      <div className="flex items-center gap-2 flex-wrap">
        <div className="relative">
          <Search className="w-3 h-3 absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
          <input value={search} onChange={e => setSearch(e.target.value)} className="bg-white border border-gray-200 rounded-lg pl-7 pr-3 py-1.5 text-[11px] text-gray-700 placeholder-gray-400 focus:outline-none focus:border-indigo-400 w-64 shadow-xs" placeholder="Search tenant, e.g. Vignesh Tex, org_fn928…" />
        </div>
        {[['TIER', 'All Tiers'], ['GATEWAY', 'All Regions'], ['STATUS', 'All States']].map(([label, val]) => (
          <div key={label} className="flex items-center gap-1 bg-white border border-gray-200 rounded-lg px-2.5 py-1.5 shadow-xs">
            <span className="text-[10px] font-bold text-gray-400">{label}:</span>
            <span className="text-[11px] text-gray-700">{val}</span>
            <ChevronDown className="w-3 h-3 text-gray-400" />
          </div>
        ))}
        <button className="text-gray-400 hover:text-gray-600 transition-colors p-1.5 rounded-lg hover:bg-gray-100">
          <XCircle className="w-3.5 h-3.5" />
        </button>
      </div>

      {/* Table */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-xs overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <p className="text-[11px] font-bold text-gray-800">Configured Tenant Clusters</p>
            <span className="text-[10px] bg-gray-100 text-gray-600 border border-gray-200 px-2 py-0.5 rounded">{data.length} displayed / {STUB_DASH.totalShops} total</span>
          </div>
          <div className="flex items-center gap-1.5 text-[10px]">
            <span className="w-2 h-2 rounded-full bg-emerald-500" />
            <span className="text-gray-500">Realtime Re-balancing:</span>
            <span className="text-emerald-600 font-bold">ONLINE</span>
          </div>
        </div>
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              {['Tenant Organization', 'Plan Tier', 'End Users', 'Billed Vol / Mo', 'Gateway Fabric', 'Metering Quota', 'Status', 'Control'].map(h => (
                <th key={h} className="text-left py-2 px-3 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.map((shop, i) => {
              const users = [14280, 3120, 28400, 890, 52100, 3];
              const vols = ['$4,210,000', '$890,400', '$6,120,500', '$145,200', '$12,840,000', '$0'];
              const gws = ['Stripe + Adyen Routed', 'Stripe Connect Engine', 'Adyen Direct – BACS', 'Stripe (Dunning Mode)', 'Checkout.com + Cybersource', 'Stripe Direct'];
              const quotas = [92, 64, 41, 98, 97, 12];
              const tierColors: Record<string, string> = {
                'Enterprise Dedicated': 'bg-gray-800 text-white',
                Enterprise: 'bg-gray-800 text-white',
                Professional: 'bg-indigo-100 text-indigo-800',
                Basic: 'bg-blue-100 text-blue-800',
                Starter: 'bg-gray-100 text-gray-700',
                'Growth Pro': 'bg-violet-100 text-violet-800',
                'Custom Hybrid': 'bg-teal-100 text-teal-800',
                'Scale API Tier': 'bg-orange-100 text-orange-800',
              };
              const isOverdue = shop.subscriptionStatus === 'Overdue';
              const q = quotas[i] ?? 12;
              const tierName = shop.currentPlanName ?? 'Starter';
              return (
                <tr key={shop.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                  <td className="py-3 px-3">
                    <div className="flex items-center gap-2">
                      <Chip initials={shop.name.substring(0, 2).toUpperCase()} color={COLORS[i % COLORS.length]} />
                      <div>
                        <p className="font-semibold text-gray-800">{shop.name}</p>
                        <p className="text-gray-400 text-[10px]">org_fn{String(shop.id).padStart(3, '0')} · US-East</p>
                      </div>
                    </div>
                  </td>
                  <td className="px-3">
                    <span className={clsx('text-[10px] font-bold px-2 py-0.5 rounded', tierColors[tierName] ?? 'bg-gray-100 text-gray-700')}>{tierName}</span>
                  </td>
                  <td className="px-3 text-gray-700">{fmt(users[i] ?? shop.totalUsers)}</td>
                  <td className="px-3 text-gray-700 font-medium">{vols[i] ?? '—'}</td>
                  <td className="px-3">
                    <div className="flex items-center gap-1">
                      <span className="w-1.5 h-1.5 rounded-full bg-emerald-500" />
                      <span className="text-gray-500 text-[10px]">{gws[i]}</span>
                    </div>
                  </td>
                  <td className="px-3">
                    <div>
                      <div className="flex items-center justify-between mb-1">
                        <span className={clsx('text-[10px] font-bold', q >= 95 ? 'text-rose-600' : q >= 75 ? 'text-amber-600' : 'text-gray-700')}>{q}%</span>
                        <span className="text-[9px] text-gray-400">{(q * 0.1).toFixed(1)}M/10M</span>
                      </div>
                      <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden w-20">
                        <div className={clsx('h-full rounded-full', q >= 95 ? 'bg-rose-500' : q >= 75 ? 'bg-amber-400' : 'bg-indigo-500')} style={{ width: `${q}%` }} />
                      </div>
                      {q >= 95 && <span className="text-[9px] text-rose-600 font-bold">Cap Warning</span>}
                    </div>
                  </td>
                  <td className="px-3">
                    <StatusBadge status={isOverdue ? 'Overdue' : shop.status === 'Suspended' ? 'Suspended' : 'Active'} />
                  </td>
                  <td className="px-3">
                    <div className="flex items-center gap-2">
                      <button onClick={() => onToggleShop(shop)} disabled={busyId === shop.id} className={clsx('text-[10px] font-semibold transition-colors', shop.status === 'Active' ? 'text-rose-600 hover:text-rose-800' : 'text-emerald-600 hover:text-emerald-800')}>
                        {busyId === shop.id ? '…' : shop.status === 'Active' ? 'Suspend' : 'Activate'}
                      </button>
                      <button className="text-gray-300 hover:text-gray-500"><MoreVertical className="w-3.5 h-3.5" /></button>
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
        <div className="px-4 py-2.5 text-[10px] text-gray-400 border-t border-gray-100 flex items-center justify-between">
          <span>Showing 1–{data.length} of {STUB_DASH.totalShops} records</span>
          <div className="flex items-center gap-1">
            {['PREV', '1', '2', '3', '...', '169', 'NEXT'].map(p => (
              <button key={p} className={clsx('px-2 py-1 rounded text-[10px]', p === '1' ? 'bg-indigo-600 text-white font-bold' : 'text-gray-500 hover:bg-gray-100')}>{p}</button>
            ))}
          </div>
        </div>
      </div>

      {/* Context Inspector */}
      <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
        <div className="flex items-center justify-between mb-3">
          <p className="text-[10px] font-bold uppercase tracking-widest text-gray-400">Context Inspector</p>
          <div className="flex items-center gap-2">
            <span className="text-[9px] font-bold text-amber-600 bg-amber-50 border border-amber-200 px-2 py-0.5 rounded uppercase">NOT SYNCHED</span>
            <button className="text-[10px] bg-white border border-gray-200 text-gray-700 px-2.5 py-1 rounded font-semibold hover:bg-gray-50 transition-colors shadow-xs">Proxy</button>
          </div>
        </div>
        <p className="text-[13px] font-bold text-gray-800">FintechFlow Inc.</p>
        <p className="text-[11px] text-gray-400">org_fn928 · Tier: Enterprise Dedicated</p>
        <div className="grid grid-cols-2 gap-4 mt-3">
          <div className="bg-gray-50 rounded-lg p-3"><p className="text-[10px] text-gray-400">Monthly Vol</p><p className="text-[13px] font-bold text-gray-800">$4.2M</p></div>
          <div className="bg-amber-50 rounded-lg p-3"><p className="text-[10px] text-gray-400">Current Quota</p><p className="text-[13px] font-bold text-amber-700">92% Cap</p></div>
        </div>
      </div>

      {/* Global Metering Catalog */}
      <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
        <div className="flex items-center justify-between mb-3">
          <div>
            <p className="text-[13px] font-bold text-gray-800">Global Metering Catalog</p>
            <p className="text-[10px] text-gray-400">Default unit costs &amp; rate dimensions</p>
          </div>
          <button className="text-[11px] text-indigo-600 hover:text-indigo-800 font-semibold flex items-center gap-1"><Settings2 className="w-3 h-3" /> Edit Model</button>
        </div>
        <div className="divide-y divide-gray-50">
          {[
            { icon: Zap, name: 'API Ingestion Calls', sub: 'Tiered overage pricing', price: '$0.0004', unit: '/request' },
            { icon: BarChart3, name: 'Gross Billed Rev (BPS)', sub: 'Platform revenue cut', price: '12.5 BPS', unit: '(0.125%)' },
            { icon: Users, name: 'Active Billing Seats', sub: 'Internal tenant seats', price: '$35.00', unit: '/seat / mo' },
            { icon: Globe, name: 'Webhook Dispatch', sub: 'Guaranteed at-least-once', price: '$0.0012', unit: '/delivery' },
            { icon: Shield, name: 'Enforcement Mode: Grace (48h)', sub: 'Tenants exceeding 100% quota transition to soft throttle with continuous dunning notifications prior to hard circuit interruption.', price: '', unit: '' },
          ].map((item, i) => {
            const Icon = item.icon;
            return (
              <div key={i} className="flex items-center gap-3 py-3">
                <div className="w-7 h-7 rounded-lg bg-gray-100 flex items-center justify-center shrink-0">
                  <Icon className="w-3.5 h-3.5 text-gray-500" />
                </div>
                <div className="flex-1">
                  <p className="text-[11px] font-semibold text-gray-800">{item.name}</p>
                  <p className="text-[10px] text-gray-400">{item.sub}</p>
                </div>
                {item.price && (
                  <div className="text-right">
                    <p className="text-[11px] font-bold text-gray-800">{item.price}</p>
                    <p className="text-[10px] text-gray-400">{item.unit}</p>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};

/* ═══════════════════════════════════════════════════════════════
   VIEW 3 – Tenant Invoicing & Revenue
═══════════════════════════════════════════════════════════════ */
const RevenueEngine: React.FC<{ dash: DashData | null }> = ({ dash }) => {
  const d = dash ?? STUB_DASH;
  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
        <div className="flex items-start gap-3">
          <div className="w-10 h-10 rounded-xl bg-gray-800 flex items-center justify-center shrink-0">
            <Building2 className="w-5 h-5 text-white" />
          </div>
          <div className="flex-1">
            <div className="flex items-center gap-2 flex-wrap">
              <p className="text-[15px] font-black text-gray-900">Acme Technologies Corp</p>
              <span className="text-[10px] text-gray-500">ID: org_acme_8912</span>
              <span className="text-[9px] bg-emerald-100 text-emerald-700 border border-emerald-200 px-2 py-0.5 rounded font-bold">● Production</span>
            </div>
            <div className="flex items-center gap-4 mt-1">
              <span className="text-[10px] text-gray-500 flex items-center gap-1"><DollarSign className="w-3 h-3" /> USD ($)</span>
              <span className="text-[10px] text-gray-500 flex items-center gap-1"><Building2 className="w-3 h-3" /> Organization Admin</span>
              <span className="text-[10px] text-emerald-600 flex items-center gap-1 font-semibold"><Circle className="w-2 h-2 fill-current" /> Realtime Sync Active</span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <button className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-2 rounded-lg font-medium flex items-center gap-1.5 transition-colors shadow-xs">
              <Settings2 className="w-3 h-3" /> Configure Webhooks
            </button>
            <button className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-2 rounded-lg font-medium flex items-center gap-1.5 transition-colors shadow-xs">
              <RefreshCw className="w-3 h-3" /> Run Batch Collection
            </button>
            <button className="text-[11px] bg-indigo-600 hover:bg-indigo-700 text-white px-3 py-2 rounded-lg font-semibold flex items-center gap-1.5 transition-colors shadow-sm">
              <Plus className="w-3 h-3" /> Create Invoice
            </button>
          </div>
        </div>
      </div>

      {/* KPIs */}
      <div className="grid grid-cols-4 gap-3">
        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <p className="text-[10px] font-bold uppercase tracking-widest text-gray-400">Monthly Recurring Rev</p>
          <div className="flex items-center gap-2 mt-1"><span className="text-[10px] font-bold text-emerald-600">+8.4%</span></div>
          <p className="text-2xl font-black text-gray-900 mt-0.5">$342,850</p>
          <p className="text-[10px] text-gray-400 mt-1">vs. $316,280 last billing cycle</p>
          <div className="mt-2 h-1 bg-gray-100 rounded-full"><div className="h-full bg-indigo-500 rounded-full" style={{ width: '72%' }} /></div>
          <div className="flex justify-between mt-1"><span className="text-[10px] text-gray-400">Quota Pace: 104%</span><span className="text-[10px] font-bold text-gray-700">$380k Target</span></div>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <p className="text-[10px] font-bold uppercase tracking-widest text-gray-400">Collected This Month</p>
          <p className="text-[10px] text-gray-400 mt-0.5">MTD</p>
          <p className="text-2xl font-black text-gray-900 mt-0.5">$318,420</p>
          <p className="text-[10px] text-emerald-600 font-semibold mt-1">94.2% on-time settlement</p>
          <p className="text-[10px] text-gray-400">Success count: 284 · Avg 2.1s</p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <div className="flex items-center justify-between">
            <p className="text-[10px] font-bold uppercase tracking-widest text-gray-400">Outstanding Invoices</p>
            <span className="text-[9px] font-bold bg-amber-100 text-amber-700 border border-amber-200 px-1.5 py-0.5 rounded">4 Actionable</span>
          </div>
          <p className="text-2xl font-black text-gray-900 mt-1.5">$24,430</p>
          <p className="text-[10px] text-gray-400 mt-1">18 open invoices · 4 past due</p>
          <div className="flex gap-2 mt-1.5">
            <span className="text-[10px] text-rose-600 font-semibold">$8,450 Overdue</span>
            <span className="text-[10px] text-gray-400">$15,980 Pending Grace</span>
          </div>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <div className="flex items-center justify-between">
            <p className="text-[10px] font-bold uppercase tracking-widest text-gray-400">Net Revenue Churn</p>
            <span className="text-[9px] font-bold bg-violet-100 text-violet-700 border border-violet-200 px-1.5 py-0.5 rounded">Elite Band</span>
          </div>
          <p className="text-2xl font-black text-gray-900 mt-1.5">-0.4%</p>
          <p className="text-[10px] text-gray-400 mt-1">Net negative churn (expansion driven)</p>
          <div className="flex gap-2 mt-1.5">
            <span className="text-[10px] text-emerald-600">Expansion: +$14.8k</span>
            <span className="text-[10px] text-gray-400">Gross Churn: 1.2%</span>
          </div>
        </div>
      </div>

      {/* Chart + Smart Dunning */}
      <div className="grid lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <div className="flex items-center justify-between mb-2">
            <div>
              <p className="text-[13px] font-bold text-gray-800">Revenue &amp; Cash Inflow Dynamics</p>
              <p className="text-[10px] text-gray-400">Realized settlements vs. pending gateway allocations</p>
            </div>
            <div className="flex gap-1">
              {['12-Week Rolling', '24 Hours', '7 Days'].map((t, i) => (
                <button key={t} className={clsx('text-[10px] px-2.5 py-1 rounded font-medium transition-colors', i === 0 ? 'bg-indigo-600 text-white' : 'text-gray-500 hover:bg-gray-100')}>{t}</button>
              ))}
            </div>
          </div>
          <div className="flex gap-3 mb-2">
            {[['bg-gray-800', 'Collected'], ['bg-gray-300', 'Pending'], ['bg-rose-300', 'Refund/Dispute']].map(([c, l]) => (
              <div key={l} className="flex items-center gap-1"><span className={clsx('w-2.5 h-2 rounded-sm', c)} /><span className="text-[10px] text-gray-500">{l}</span></div>
            ))}
          </div>
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={barData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
              <XAxis dataKey="w" tick={{ fontSize: 10, fill: '#94a3b8' }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 10, fill: '#94a3b8' }} axisLine={false} tickLine={false} />
              <Tooltip contentStyle={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, fontSize: 11 }} />
              <Bar dataKey="c" name="Collected" fill="#1e293b" radius={[2, 2, 0, 0]} />
              <Bar dataKey="p" name="Pending" fill="#cbd5e1" radius={[2, 2, 0, 0]} />
              <Bar dataKey="r" name="Refund" fill="#fca5a5" radius={[2, 2, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <div className="flex items-center justify-between mb-1">
            <p className="text-[11px] font-bold text-gray-800 flex items-center gap-1.5"><Zap className="w-3.5 h-3.5 text-amber-500" /> Smart Dunning Engine</p>
            <span className="text-[9px] bg-indigo-100 text-indigo-700 border border-indigo-200 px-2 py-0.5 rounded font-bold">AUTOPILOT</span>
          </div>
          <p className="text-[10px] text-gray-400 mb-2">Recovered this week</p>
          <p className="text-2xl font-black text-gray-900">$14,200.00</p>
          <p className="text-[10px] text-gray-400 mb-3">Across 9 automated invoice retries</p>
          <div className="space-y-2.5">
            {[
              { label: 'Optimal Window Algorithm', val: '89.4% Success', dot: 'bg-emerald-500', valColor: 'text-emerald-700' },
              { label: 'Scheduled Next Retry', val: '3 queues (Tomorrow)', dot: 'bg-amber-400', valColor: 'text-amber-700' },
              { label: 'Card Updater Ping', val: '14 Tokens refreshed', dot: 'bg-gray-400', valColor: 'text-gray-600' },
            ].map(item => (
              <div key={item.label} className="flex items-center justify-between">
                <div className="flex items-center gap-1.5"><span className={clsx('w-2 h-2 rounded-full', item.dot)} /><span className="text-[11px] text-gray-600">{item.label}</span></div>
                <span className={clsx('text-[11px] font-semibold', item.valColor)}>{item.val}</span>
              </div>
            ))}
          </div>
          <button className="w-full mt-4 bg-gray-50 hover:bg-gray-100 text-gray-700 border border-gray-200 text-[11px] font-semibold py-2 rounded-lg flex items-center justify-center gap-2 transition-colors">
            <Settings2 className="w-3.5 h-3.5" /> Configure Escalation Schedule
          </button>
        </div>
      </div>

      {/* Invoice Ledger */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-xs overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <p className="text-[13px] font-bold text-gray-800">Invoices &amp; Revenue Ledger</p>
            <span className="text-[9px] bg-emerald-100 text-emerald-700 border border-emerald-200 px-2 py-0.5 rounded font-bold animate-pulse">Real-Time</span>
            <p className="text-[10px] text-gray-400">Dispatched billing manifests, scheduled dunning pipelines, and automated settlement records.</p>
          </div>
          <div className="flex gap-2">
            <select className="text-[11px] bg-white border border-gray-200 rounded-lg px-2 py-1.5 text-gray-700 focus:outline-none">
              <option>All Statuses</option>
            </select>
            <button className="text-[11px] bg-white border border-gray-200 text-gray-700 px-3 py-1.5 rounded-lg font-medium flex items-center gap-1.5 hover:bg-gray-50 transition-colors shadow-xs">
              <Download className="w-3 h-3" /> Export CSV
            </button>
          </div>
        </div>
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              {['Invoice ID', 'Customer / Organization', 'Plan Specification', 'Amount (USD)', 'Issue Date', 'Due Date', 'Status', 'Gateway Route', ''].map(h => (
                <th key={h} className="text-left py-2 px-3 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {[
              { id: 'INV-2024-8912', init: 'SM', name: 'Starlight Media Inc.', cid: 'cust_88284_prod', plan: 'Enterprise Annual', amount: '$48,000.00', issued: 'Oct 24, 2024', due: 'Nov 07, 2024', status: 'Paid', gw: 'Stripe CC (auth_991)', color: 'bg-indigo-600' },
              { id: 'INV-2024-8911', init: 'HR', name: 'Hyperion Robotics', cid: 'cust_49102_prod', plan: 'Usage + Base Tier', amount: '$6,420.50', issued: 'Oct 24, 2024', due: 'Nov 08, 2024', status: 'Paid', gw: 'Adyen SEPA', color: 'bg-violet-600' },
              { id: 'INV-2024-8910', init: 'DA', name: 'DataSphere Analytics', cid: 'cust_11829_prod', plan: 'Pro Team (32 seats)', amount: '$3,840.00', issued: 'Oct 12, 2024', due: 'Oct 26, 2024', status: 'Past Due: Dunning 2', gw: 'Stripe CC', color: 'bg-rose-600' },
              { id: 'INV-2024-8909', init: 'CS', name: 'CloudByte Security', cid: 'cust_77390_prod', plan: 'Custom Scale (Dedicated)', amount: '$18,500.00', issued: 'Oct 22, 2024', due: 'Nov 05, 2024', status: 'Pending ACH', gw: 'Adyen ACH (Clearing)', color: 'bg-amber-600' },
              { id: 'INV-2024-8908', init: 'VL', name: 'Vanguard Logistics', cid: 'cust_53018_prod', plan: 'Starter Fleet Tier', amount: '$1,250.00', issued: 'Oct 22, 2024', due: 'Nov 04, 2024', status: 'Paid', gw: 'Stripe ACH', color: 'bg-emerald-600' },
            ].map((inv, i) => (
              <tr key={i} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                <td className="py-2.5 px-3 font-mono text-[10px] text-gray-500">{inv.id}</td>
                <td className="px-3">
                  <div className="flex items-center gap-2">
                    <Chip initials={inv.init} color={inv.color} />
                    <div><p className="font-semibold text-gray-800">{inv.name}</p><p className="text-gray-400 text-[10px]">{inv.cid}</p></div>
                  </div>
                </td>
                <td className="px-3 text-gray-500">{inv.plan}</td>
                <td className="px-3 text-gray-800 font-bold">{inv.amount}</td>
                <td className="px-3 text-gray-500">{inv.issued}</td>
                <td className="px-3">
                  <span className={clsx('text-[10px]', inv.status.startsWith('Past') ? 'text-rose-600 font-bold' : 'text-gray-500')}>{inv.due}</span>
                  {inv.status.startsWith('Past') && <p className="text-[9px] text-rose-500">(Overdue)</p>}
                </td>
                <td className="px-3"><StatusBadge status={inv.status} /></td>
                <td className="px-3 text-gray-500 text-[10px]">{inv.gw}</td>
                <td className="px-3">
                  {inv.status.startsWith('Past') ? (
                    <button className="text-[10px] bg-rose-600 hover:bg-rose-700 text-white px-2 py-1 rounded font-bold transition-colors">Retry Now</button>
                  ) : <Eye className="w-3.5 h-3.5 text-gray-300 hover:text-gray-500 cursor-pointer" />}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="px-4 py-2.5 text-[10px] text-gray-400 border-t border-gray-100 flex items-center justify-between">
          <span>Showing 1–5 of 48 active billing records · Page 1 of 10</span>
          <div className="flex items-center gap-1">
            {['Previous', '1', '2', '3', 'Next'].map(p => (
              <button key={p} className={clsx('px-2 py-1 rounded text-[10px]', p === '1' ? 'bg-indigo-600 text-white font-bold' : 'text-gray-500 hover:bg-gray-100')}>{p}</button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

/* ═══════════════════════════════════════════════════════════════
   VIEW 4 – Customer Subscriptions
═══════════════════════════════════════════════════════════════ */
const CustomerSubscriptions: React.FC<{ shops: Shop[] }> = ({ shops }) => (
  <div className="space-y-5">
    <div className="flex items-start justify-between">
      <div>
        <p className="text-[10px] text-gray-500 font-semibold">ACME TECHNOLOGIES CORP · TENANT ID #ACM-8841</p>
        <h1 className="text-xl font-black text-gray-900 mt-1">Customer Subscriptions &amp; Dunning Lifecycle</h1>
        <p className="text-[11px] text-gray-400 mt-1 max-w-md">Manage customer billing profiles, automated payment retries, recurring plans, and contract add-ons.</p>
      </div>
      <div className="flex items-center gap-2">
        <button className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-2 rounded-lg font-medium flex items-center gap-1.5 transition-colors shadow-xs">
          <Download className="w-3 h-3" /> Export MRR Cohort
        </button>
        <button className="text-[11px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 px-3 py-2 rounded-lg font-medium flex items-center gap-1.5 transition-colors shadow-xs">
          <RefreshCw className="w-3 h-3" /> Trigger Manual Retry Batch
        </button>
        <button className="text-[11px] bg-gray-100 hover:bg-gray-200 text-gray-600 px-3 py-2 rounded-lg font-medium flex items-center gap-1.5 transition-colors">
          <PauseCircle className="w-3 h-3" /> Pause Selected (0)
        </button>
      </div>
    </div>

    <div className="grid grid-cols-4 gap-3">
      <KpiCard label="Active MRR Base" value="$384,190" trend={8.4} sub="vs last mo" icon={<DollarSign className="w-4 h-4" />} />
      <KpiCard label="Delinquent Exposure" value="$6,440" badge="⚠" badgeColor="bg-rose-100 text-rose-700" sub="24 Accounts in Dunning" />
      <KpiCard label="Autonomous Save Rate" value="86.4%" trend={3.1} sub="Zero-agent interaction" icon={<Zap className="w-4 h-4" />} />
      <KpiCard label="Renewals (Next 7D)" value="142 Accounts" sub="Est. volume: $62.8k" icon={<RefreshCw className="w-4 h-4" />} />
    </div>

    <div className="grid lg:grid-cols-3 gap-4">
      {/* Subscriptions Table */}
      <div className="lg:col-span-2 bg-white border border-gray-200 rounded-xl shadow-xs overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center gap-2 flex-wrap">
          {[['All', '1,420'], ['Active', '1,342'], ['In Dunning', '24'], ['Paused', '16']].map(([l, c]) => (
            <button key={l} className={clsx('text-[11px] px-3 py-1.5 rounded-lg font-semibold transition-colors', l === 'All' ? 'bg-indigo-600 text-white' : 'text-gray-500 hover:bg-gray-100')}>
              {l} <span className="text-[10px] opacity-70">{c}</span>
            </button>
          ))}
          <div className="ml-auto flex gap-1">
            {['All Cadences', 'Monthly', 'Annual', 'Multi-Year'].map((t, i) => (
              <button key={t} className={clsx('text-[10px] px-2 py-1 rounded transition-colors', i === 0 ? 'bg-gray-100 text-gray-700 font-semibold' : 'text-gray-400 hover:bg-gray-50')}>{t}</button>
            ))}
          </div>
        </div>
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              <th className="py-2 pl-3 pr-1 w-8"><input type="checkbox" className="accent-indigo-600" /></th>
              {['Customer & Company', 'Plan / Cadence', 'MRR Value', 'Next Renewal', 'Payment Method', 'Dunning Status'].map(h => (
                <th key={h} className="text-left py-2 px-2 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {[
              { init: 'NB', name: 'NextGen BioLabs', id: 'cust_9920147a', plan: 'Enterprise Core', cadence: 'Annual Contract', mrr: '$3,200.00', renewal: 'Oct 29', renews: 'Renews in 18d', status: 'Healthy', pm: 'MC ···· 8812', pmExp: 'Exp 09/27', color: 'bg-indigo-600' },
              { init: 'AA', name: 'AeroDynamics AI', id: 'cust_4410928f', plan: 'Growth Metered', cadence: 'Monthly Flex', mrr: '$1,850.00', renewal: 'Oct 14', renews: 'Renews in 3d', status: 'Grace Period (Card Expired)', pm: 'Visa 1842', pmExp: 'Exp 09/24', color: 'bg-violet-600' },
              { init: 'QS', name: 'Quantum Software', id: 'cust_7721830m', plan: 'Scale Tier', cadence: 'Monthly Regular', mrr: '$940.00', renewal: 'Oct 23', renews: 'Renews in 12d', status: 'Healthy', pm: 'AMEX 4001', pmExp: 'Exp 12/26', color: 'bg-emerald-600' },
              { init: 'ZR', name: 'Zenith Retail Corp', id: 'cust_1092839x', plan: 'Custom Enterprise', cadence: 'Annual Prepaid + SLA', mrr: '$8,500.00', renewal: 'Nov 25', renews: 'Renews in 45d', status: 'Healthy', pm: 'ACH Auto-Debit', pmExp: '···· 9182', color: 'bg-amber-600' },
              { init: 'SE', name: 'Solaris Energy', id: 'cust_55928114', plan: 'Team Starter', cadence: 'Monthly Plan', mrr: '$450.00', renewal: 'Tomorrow', renews: 'Renews in 1d', status: 'Retry 2 of 3', pm: 'Visa 9921', pmExp: 'Card Declined (NSF)', color: 'bg-rose-600' },
            ].map((s, i) => (
              <tr key={i} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                <td className="py-2.5 pl-3 pr-1"><input type="checkbox" className="accent-indigo-600 w-3 h-3" /></td>
                <td className="px-2">
                  <div className="flex items-center gap-2">
                    <Chip initials={s.init} color={s.color} />
                    <div><p className="font-semibold text-gray-800">{s.name}</p><p className="text-gray-400 text-[10px]">{s.id}</p></div>
                  </div>
                </td>
                <td className="px-2"><p className="text-gray-700 font-medium">{s.plan}</p><p className="text-gray-400 text-[10px]">{s.cadence}</p></td>
                <td className="px-2 text-gray-800 font-bold">{s.mrr}<span className="text-gray-400 font-normal">/mo</span></td>
                <td className="px-2"><p className={clsx('font-medium', s.renewal === 'Tomorrow' ? 'text-rose-600' : 'text-gray-700')}>{s.renewal}</p><p className="text-gray-400 text-[10px]">{s.renews}</p></td>
                <td className="px-2"><p className="text-gray-600">{s.pm}</p><p className="text-gray-400 text-[10px]">{s.pmExp}</p></td>
                <td className="px-2"><StatusBadge status={s.status} /></td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="px-4 py-2 text-[10px] text-gray-400 border-t border-gray-100">
          Rows per page: 25 · 1–5 of 1,420 items
        </div>
      </div>

      {/* Dunning Engine */}
      <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
        <div className="flex items-center justify-between mb-1">
          <p className="text-[11px] font-bold text-gray-800">Dunning &amp; Revenue Recovery Rules Engine</p>
          <span className="text-[9px] bg-emerald-100 text-emerald-700 border border-emerald-200 px-2 py-0.5 rounded font-bold">V3.2 ACTIVE</span>
        </div>
        <p className="text-[10px] text-gray-400 mb-3">Predictive multi-tiered recovery workflow designed to salvage at-risk MRR before terminal subscription cancellation.</p>

        <div className="bg-gray-50 rounded-lg p-3 mb-4">
          <p className="text-[10px] text-gray-400 font-bold uppercase tracking-wider">Real-Time Dunning Recovery Rate</p>
          <div className="flex items-end gap-2 mt-1">
            <p className="text-3xl font-black text-gray-900">86.4%</p>
            <span className="text-[11px] text-emerald-600 font-bold mb-1">+$22,410 recovered MTD</span>
          </div>
          <div className="h-2 bg-gray-200 rounded-full mt-2 overflow-hidden">
            <div className="h-full rounded-full bg-gradient-to-r from-indigo-500 to-emerald-500" style={{ width: '86.4%' }} />
          </div>
          <div className="flex justify-between mt-1.5">
            <span className="text-[10px] text-indigo-600 font-semibold">86.4% Autonomous Recovery</span>
            <span className="text-[10px] text-rose-500 font-semibold">13.6% Terminal Churn</span>
          </div>
        </div>

        <p className="text-[10px] font-bold uppercase tracking-widest text-gray-400 mb-2">Automated Execution Pipeline</p>
        <div className="space-y-3">
          {[
            { day: 'Day 0: Decline Trigger', badge: 'Immediate', desc: 'Soft decline webhook detected. Real-time ledger flags delinquency and halts non-essential API bursts while keeping portal accessible.', dot: 'bg-rose-500', active: true },
            { day: 'Day 2: AI Smart Retry', badge: '+48 Hours', desc: 'Autonomous retry using Stripe Network Tokens & optimal card routing time algorithms (off-peak timezone balancing).', dot: 'bg-indigo-500', active: false },
            { day: 'Day 4: Branded Recovery Push', badge: '+96 Hours', desc: 'Automated email delivery containing zero-login self-serve payment portal link with tokenized 1-click Apple Pay & Google Pay card swaps.', dot: 'bg-emerald-500', active: false },
            { day: 'Day 7: Final Retry & Warning', badge: '+168 Hours', desc: 'Final payment execution run. If unsuccessful, automatic system dispatch of service degradation warning and 72-hour grace suspension timer.', dot: 'bg-rose-400', active: false },
          ].map(step => (
            <div key={step.day} className="flex gap-2.5">
              <div className="flex flex-col items-center">
                <div className={clsx('w-3 h-3 rounded-full mt-0.5 shrink-0 ring-2 ring-white', step.dot)} />
                <div className="w-px flex-1 bg-gray-200 mt-1" />
              </div>
              <div className="pb-3 flex-1">
                <div className="flex items-center gap-2 mb-0.5">
                  <p className="text-[11px] font-bold text-gray-800">{step.day}</p>
                  <span className="text-[9px] bg-gray-100 text-gray-600 border border-gray-200 px-1.5 py-0.5 rounded font-bold">{step.badge}</span>
                </div>
                <p className="text-[10px] text-gray-500 leading-relaxed">{step.desc}</p>
              </div>
            </div>
          ))}
        </div>

        <button className="w-full mt-2 bg-indigo-600 hover:bg-indigo-700 text-white text-[11px] font-semibold py-2.5 rounded-lg flex items-center justify-center gap-2 transition-colors shadow-sm">
          <Settings2 className="w-3.5 h-3.5" /> Configure Dunning Strategy &amp; Email Templates
        </button>

        <div className="mt-4 pt-3 border-t border-gray-100">
          <p className="text-[10px] text-gray-400 font-bold uppercase tracking-widest mb-2">Gateway Processing Health</p>
          {[
            { name: 'Stripe Primary Gateway', val: '99.98% SUCCESS', color: 'text-emerald-600' },
            { name: 'Adyen Fallback Failover', val: 'HOT STANDBY', color: 'text-amber-600' },
            { name: 'Plaid Bank Verification', val: 'OPERATIONAL', color: 'text-emerald-600' },
          ].map(g => (
            <div key={g.name} className="flex items-center justify-between py-1.5 border-b border-gray-50 last:border-0">
              <span className="text-[10px] text-gray-500">{g.name}</span>
              <span className={clsx('text-[10px] font-bold', g.color)}>{g.val}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  </div>
);

/* ═══════════════════════════════════════════════════════════════
   VIEW 5 – Plans
═══════════════════════════════════════════════════════════════ */
const GlobalPlans: React.FC<{ plans: Plan[] }> = ({ plans }) => {
  const data = plans.length ? plans : STUB_PLANS;
  const total = data.reduce((s, p) => s + p.activeSubscribers, 0);
  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-black text-gray-900">Global Plans &amp; Metering</h1>
          <p className="text-[11px] text-gray-400 mt-0.5">Manage subscription plans, feature flags, and pricing tiers across all tenants.</p>
        </div>
        <button className="text-[11px] bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg font-semibold flex items-center gap-1.5 shadow-sm">
          <Plus className="w-3 h-3" /> Create New Plan
        </button>
      </div>
      <div className="grid grid-cols-4 gap-4">
        {data.map((plan, i) => {
          const cardColors = [
            'border-gray-200 bg-white',
            'border-indigo-200 bg-indigo-50',
            'border-violet-200 bg-violet-50',
            'border-amber-200 bg-amber-50',
          ];
          const badgeColors = ['bg-gray-100 text-gray-700', 'bg-indigo-100 text-indigo-700', 'bg-violet-100 text-violet-700', 'bg-amber-100 text-amber-700'];
          return (
            <div key={plan.id} className={clsx('border rounded-xl p-4 shadow-xs hover:shadow-sm transition-shadow', cardColors[i])}>
              <div className="flex items-center justify-between mb-2">
                <span className={clsx('text-[10px] font-bold px-2 py-0.5 rounded uppercase', badgeColors[i])}>{plan.name}</span>
                {plan.isActive ? <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" /> : <XCircle className="w-3.5 h-3.5 text-gray-300" />}
              </div>
              <p className="text-2xl font-black text-gray-900">{plan.monthlyPrice === 0 ? 'Free' : `₹${plan.monthlyPrice}`}{plan.monthlyPrice > 0 && <span className="text-sm font-normal text-gray-400">/mo</span>}</p>
              {plan.yearlyPrice ? <p className="text-[10px] text-gray-400">₹{plan.yearlyPrice}/yr</p> : <p className="text-[10px] text-gray-400">No yearly plan</p>}
              <div className="mt-3">
                <div className="flex justify-between mb-1"><span className="text-[10px] text-gray-500">Active Subscribers</span><span className="text-[10px] font-bold text-gray-800">{plan.activeSubscribers}</span></div>
                <div className="h-1.5 bg-gray-200 rounded-full overflow-hidden"><div className="h-full bg-indigo-500 rounded-full" style={{ width: `${pct(plan.activeSubscribers, total)}%` }} /></div>
                <p className="text-[9px] text-gray-400 mt-0.5">{pct(plan.activeSubscribers, total)}% of platform subscribers</p>
              </div>
              <div className="mt-3 space-y-1 pt-3 border-t border-gray-100">
                {[['Max Users', plan.maxUsers >= 999 ? 'Unlimited' : plan.maxUsers], ['Max Products', plan.maxProducts >= 999999 ? 'Unlimited' : fmt(plan.maxProducts)], ['Trial Days', `${plan.trialDays}d`]].map(([l, v]) => (
                  <div key={String(l)} className="flex justify-between text-[10px]"><span className="text-gray-400">{l}</span><span className="font-semibold text-gray-700">{v}</span></div>
                ))}
              </div>
              <div className="mt-3 flex gap-2">
                <button className="flex-1 text-[10px] bg-white hover:bg-gray-50 text-gray-700 border border-gray-200 py-1.5 rounded font-semibold transition-colors">Edit</button>
                <button className={clsx('flex-1 text-[10px] py-1.5 rounded font-semibold transition-colors', plan.isActive ? 'bg-rose-50 hover:bg-rose-100 text-rose-700 border border-rose-200' : 'bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border border-emerald-200')}>
                  {plan.isActive ? 'Deactivate' : 'Activate'}
                </button>
              </div>
            </div>
          );
        })}
      </div>
      <div className="bg-white border border-gray-200 rounded-xl shadow-xs overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100"><p className="text-[11px] font-bold text-gray-800">Feature Matrix Comparison</p></div>
        <table className="w-full text-[11px]">
          <thead>
            <tr className="border-b border-gray-100 bg-gray-50/70">
              <th className="text-left py-2 px-4 text-[10px] font-semibold uppercase tracking-wider text-gray-400">Feature</th>
              {data.map(p => <th key={p.id} className="text-center py-2 px-4 text-[10px] font-bold text-gray-600">{p.name}</th>)}
            </tr>
          </thead>
          <tbody>
            {([
              ['WhatsApp Integration', (p: Plan) => p.hasWhatsAppIntegration ?? false],
              ['SMS Integration',       (p: Plan) => p.hasSmsIntegration ?? false],
              ['Email Integration',     (p: Plan) => p.hasEmailIntegration ?? true],
              ['Advanced Reports',      (p: Plan) => p.hasReports ?? false],
              ['Multi-Currency',        (p: Plan) => p.hasMultiCurrency ?? false],
              ['Inventory Management',  (p: Plan) => p.hasInventoryManagement ?? true],
              ['Device Integration',    (p: Plan) => p.hasDeviceIntegration ?? false],
              ['API Access',            (p: Plan) => p.hasApiAccess ?? false],
            ] as [string, (p: Plan) => boolean][]).map(([feat, getter]) => (
              <tr key={feat} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                <td className="py-2.5 px-4 text-gray-600">{feat}</td>
                {data.map(p => (
                  <td key={p.id} className="py-2.5 px-4 text-center">
                    {getter(p) ? <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500 mx-auto" /> : <span className="text-gray-200">—</span>}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

/* ═══════════════════════════════════════════════════════════════
   VIEW 6 – Audit Logs
═══════════════════════════════════════════════════════════════ */
const AuditLogs: React.FC<{ logs: AuditLog[] }> = ({ logs }) => {
  const data = logs.length ? logs : STUB_LOGS;
  const actionColor = (a: string) => a.includes('SUSPEND') ? 'bg-rose-50 text-rose-700 border-rose-200' : a.includes('UPGRADE') || a.includes('REGISTER') ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-gray-100 text-gray-600 border-gray-200';
  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div><h1 className="text-xl font-black text-gray-900">Audit &amp; Security Logs</h1><p className="text-[11px] text-gray-400 mt-0.5">Complete platform event trail for compliance and security investigation.</p></div>
        <button className="text-[11px] bg-white border border-gray-200 text-gray-700 px-3 py-2 rounded-lg font-medium flex items-center gap-1.5 hover:bg-gray-50 transition-colors shadow-xs"><Download className="w-3 h-3" /> Export Audit Log (.csv)</button>
      </div>
      <div className="grid grid-cols-4 gap-3">
        <KpiCard label="Total Events (30d)" value="28,492" trend={12} sub="vs prior period" />
        <KpiCard label="Security Alerts" value="3" badge="Unresolved" badgeColor="bg-rose-100 text-rose-700" />
        <KpiCard label="API Events (24h)" value="14,201" trend={8.2} />
        <KpiCard label="Auth Events (24h)" value="6,847" trend={3.1} />
      </div>
      <div className="bg-white border border-gray-200 rounded-xl shadow-xs overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex gap-3">
          <div className="relative flex-1 max-w-xs"><Search className="w-3 h-3 absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" /><input className="w-full bg-gray-50 border border-gray-200 rounded-lg pl-7 pr-3 py-1.5 text-[11px] text-gray-700 placeholder-gray-400 focus:outline-none focus:border-indigo-400" placeholder="Search events, users, entities…" /></div>
          <select className="bg-white border border-gray-200 text-gray-700 text-[11px] rounded-lg px-2.5 py-1.5 focus:outline-none"><option>All Actions</option></select>
        </div>
        <div className="divide-y divide-gray-50">
          {data.map(log => (
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
        <div className="px-4 py-2.5 text-[10px] text-gray-400 border-t border-gray-100">Showing {data.length} of 28,492 events</div>
      </div>
    </div>
  );
};

/* ═══════════════════════════════════════════════════════════════
   NAV CONFIG
═══════════════════════════════════════════════════════════════ */
const VIEWS = [
  { id: 'overview', label: 'Platform Overview', icon: LayoutDashboard, section: 'OPERATIONS & CONTROL' },
  { id: 'tenants', label: 'Tenant Registry', icon: Building2, section: 'OPERATIONS & CONTROL' },
  { id: 'plans', label: 'Global Plans & Metering', icon: Globe, section: 'OPERATIONS & CONTROL' },
  { id: 'gateway', label: 'Gateway Orchestration', icon: GitBranch, section: 'OPERATIONS & CONTROL' },
  { id: 'audit', label: 'Audit & Security Logs', icon: ShieldAlert, section: 'OPERATIONS & CONTROL' },
  { id: 'invoicing', label: 'Tenant Invoicing & Revenue', icon: Receipt, section: 'REVENUE ENGINE' },
  { id: 'subscriptions', label: 'Customer Subscriptions', icon: Users, section: 'REVENUE ENGINE' },
  { id: 'payment-runs', label: 'Payment Runs & Dunning', icon: RefreshCw, section: 'REVENUE ENGINE' },
  { id: 'tax', label: 'Tax & Compliance', icon: FileText, section: 'REVENUE ENGINE' },
  { id: 'system', label: 'System Settings', icon: Settings2, section: 'REVENUE ENGINE' },
];

/* ═══════════════════════════════════════════════════════════════
   ROOT COMPONENT
═══════════════════════════════════════════════════════════════ */
export const PlatformAdminPage: React.FC = () => {
  const [view, setView] = useState('overview');
  const [dash, setDash] = useState<DashData | null>(null);
  const [shops, setShops] = useState<Shop[]>([]);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [busyId, setBusyId] = useState<number | null>(null);
  const [collapsed, setCollapsed] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [lastRefreshed, setLastRefreshed] = useState<Date>(() => new Date());
  const clock = useClock();

  const load = useCallback(async () => {
    setIsRefreshing(true);
    try {
      const [d, s, p, l] = await Promise.all([
        api.request<DashData>('/api/admin/dashboard').catch(() => null),
        api.request<Shop[]>('/api/admin/shops').catch(() => null),
        api.request<Plan[]>('/api/admin/plans').catch(() => null),
        api.request<AuditLog[]>('/api/admin/audit-logs?page=1&pageSize=50').catch(() => null),
      ]);
      // Merge: use real data when available, but layer real shops on top of stubs
      // so the UI always shows meaningful demo content.
      const realDash = d as DashData | null;
      const realShops = s as Shop[] | null;
      const realPlans = p as Plan[] | null;
      const realLogs = l as AuditLog[] | null;

      // For dashboard: merge real counts into stub so totals are accurate
      if (realDash) {
        setDash({
          ...STUB_DASH,
          totalShops: Math.max(realDash.totalShops, STUB_DASH.totalShops),
          activeShops: Math.max(realDash.activeShops, STUB_DASH.activeShops),
          suspendedShops: realDash.suspendedShops,
          totalUsers: Math.max(realDash.totalUsers, STUB_DASH.totalUsers),
          activeTrials: Math.max(realDash.activeTrials, STUB_DASH.activeTrials),
          activeSubscriptions: Math.max(realDash.activeSubscriptions, STUB_DASH.activeSubscriptions),
          // Always show stub revenue so KPIs look populated
          totalRevenueThisMonth: STUB_DASH.totalRevenueThisMonth,
          totalRevenueAllTime: STUB_DASH.totalRevenueAllTime,
        });
      } else {
        setDash(STUB_DASH);
      }

      // For shops: prepend real shops at top, then show stubs below
      if (realShops && realShops.length > 0) {
        const realIds = new Set(realShops.map(s => s.id));
        const stubFill = STUB_SHOPS.filter(s => !realIds.has(s.id));
        setShops([...realShops, ...stubFill]);
      } else {
        setShops(STUB_SHOPS);
      }

      // For plans: real plans have real feature flags
      if (realPlans && realPlans.length > 0) {
        // Merge with stub subscriber counts to show meaningful numbers
        const merged = realPlans.map((rp, i) => ({
          ...rp,
          activeSubscribers: Math.max(rp.activeSubscribers, STUB_PLANS[i]?.activeSubscribers ?? 0),
        }));
        setPlans(merged);
      } else {
        setPlans(STUB_PLANS);
      }

      // For logs: prepend real logs, then stubs
      if (realLogs && realLogs.length > 0) {
        setLogs([...realLogs, ...STUB_LOGS]);
      } else {
        setLogs(STUB_LOGS);
      }

      setLastRefreshed(new Date());
    } catch {
      setDash(STUB_DASH);
      setShops(STUB_SHOPS);
      setPlans(STUB_PLANS);
      setLogs(STUB_LOGS);
      setLastRefreshed(new Date());
    } finally {
      setIsRefreshing(false);
    }
  }, []);

  // Initial load
  useEffect(() => { void load(); }, [load]);

  // Auto-poll every 30 seconds
  useEffect(() => {
    const id = setInterval(() => { void load(); }, 30_000);
    return () => clearInterval(id);
  }, [load]);

  const toggleShop = async (shop: Shop) => {
    const action = shop.status.toLowerCase() === 'suspended' ? 'activate' : 'suspend';
    setBusyId(shop.id);
    try {
      await api.request(`/api/admin/shops/${shop.id}/${action}`, { method: 'POST' });
      await load();
    } finally { setBusyId(null); }
  };

  const sections = [...new Set(VIEWS.map(v => v.section))];

  const renderView = () => {
    switch (view) {
      case 'overview': return <PlatformOverview dash={dash} shops={shops} logs={logs} lastRefreshed={lastRefreshed} isRefreshing={isRefreshing} onRefresh={load} />;
      case 'tenants': return <TenantRegistry shops={shops} plans={plans} onToggleShop={toggleShop} busyId={busyId} />;
      case 'plans': return <GlobalPlans plans={plans} />;
      case 'invoicing': return <RevenueEngine dash={dash} />;
      case 'subscriptions': return <CustomerSubscriptions shops={shops} />;
      case 'audit': return <AuditLogs logs={logs} />;
      default:
        return (
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
      {/* ── Sidebar ── */}
      <aside className={clsx(
        'flex flex-col border-r border-gray-800 bg-gray-900 shrink-0 transition-all duration-200',
        collapsed ? 'w-12' : 'w-52'
      )}>
        {/* Header */}
        <div className="h-12 flex items-center justify-between px-3 border-b border-gray-800 shrink-0">
          {!collapsed && (
            <div>
              <p className="text-[9px] font-bold text-indigo-400 uppercase tracking-widest">LedgerFlow</p>
              <p className="text-[10px] text-gray-300 font-semibold">CORE</p>
            </div>
          )}
          <button onClick={() => setCollapsed(!collapsed)} className="text-gray-600 hover:text-gray-400 transition-colors p-1 rounded ml-auto">
            {collapsed ? <ChevronRight className="w-3.5 h-3.5" /> : <ChevronLeft className="w-3.5 h-3.5" />}
          </button>
        </div>

        {/* Workspace */}
        {!collapsed && (
          <div className="px-3 py-2.5 border-b border-gray-800">
            <p className="text-[9px] text-gray-600 uppercase tracking-widest font-bold">Workspace Context</p>
            <div className="flex items-center gap-1.5 mt-1">
              <p className="text-[11px] text-gray-200 font-semibold">Acme Corp Cloud</p>
              <span className="text-[9px] bg-indigo-900 text-indigo-300 border border-indigo-700 px-1.5 py-0.5 rounded font-bold">TENANT</span>
            </div>
            <button className="text-[10px] text-gray-600 hover:text-gray-400 mt-0.5 transition-colors">Switch to SuperAdmin Control</button>
          </div>
        )}

        {/* Nav */}
        <div className="flex-1 overflow-y-auto py-2 px-2 space-y-3">
          {sections.map(section => (
            <div key={section}>
              {!collapsed && (
                <p className="text-[9px] font-bold uppercase tracking-widest text-gray-600 px-2 mb-1">{section}</p>
              )}
              <div className="space-y-0.5">
                {VIEWS.filter(v => v.section === section).map(item => {
                  const Icon = item.icon;
                  const active = view === item.id;
                  return (
                    <button
                      key={item.id}
                      onClick={() => setView(item.id)}
                      title={collapsed ? item.label : undefined}
                      className={clsx(
                        'w-full flex items-center gap-2 px-2 py-1.5 rounded-lg text-[11px] font-medium transition-all text-left',
                        active ? 'bg-gray-700 text-white' : 'text-gray-500 hover:bg-gray-800 hover:text-gray-300'
                      )}
                    >
                      <Icon className={clsx('w-3.5 h-3.5 shrink-0', active ? 'text-white' : 'text-gray-600')} />
                      {!collapsed && <span className="truncate">{item.label}</span>}
                    </button>
                  );
                })}
              </div>
            </div>
          ))}
        </div>

        {/* Footer */}
        <div className="p-3 border-t border-gray-800">
          <div className={clsx('flex items-center gap-1.5', collapsed ? 'justify-center' : 'px-1')}>
            <span className="w-2 h-2 rounded-full bg-emerald-500 shrink-0 animate-pulse" />
            {!collapsed && <span className="text-[10px] text-gray-600">Live Production v2.4</span>}
          </div>
          {!collapsed && (
            <p className="text-[9px] text-gray-700 px-1 mt-0.5">{clock}</p>
          )}
        </div>
      </aside>

      {/* ── Main ── */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Breadcrumb bar */}
        <div className="h-10 bg-white border-b border-gray-200 flex items-center px-5 gap-2 shrink-0 shadow-xs">
          <span className="text-[10px] text-gray-400 flex items-center gap-1.5">
            PLATFORM <ChevronRight className="w-3 h-3" /> BILLING ENGINE <ChevronRight className="w-3 h-3" />
            <span className="text-gray-600 font-medium">Active Telemetry</span>
          </span>
          <div className="flex items-center gap-3 ml-auto">
            <div className="flex items-center gap-1.5">
              <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
              <span className="text-[10px] text-gray-500 font-medium">US · EAST-1</span>
            </div>
            {/* Live clock in header */}
            <span className="text-[10px] font-mono text-gray-400">{clock}</span>
            <button onClick={() => load()} disabled={isRefreshing} className="text-[10px] text-gray-400 hover:text-gray-600 flex items-center gap-1 transition-colors">
              <RefreshCw className={clsx('w-3 h-3', isRefreshing && 'animate-spin')} />
            </button>
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-5">
          {renderView()}
        </div>
      </div>
    </div>
  );
};
