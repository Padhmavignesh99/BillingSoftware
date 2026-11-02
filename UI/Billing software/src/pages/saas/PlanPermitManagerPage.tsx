import React, { useState, useEffect, useCallback } from 'react';
import {
  Shield, Gift, Clock, Search, CheckCircle, XCircle,
  Trash2, RefreshCw, AlertCircle, Star, User, Info,
  ToggleLeft, ToggleRight, FileText, ChevronDown
} from 'lucide-react';
import { clsx } from 'clsx';
import { api } from '../../services/apiClient';

/* ─── Types ──────────────────────────────────────────────────── */
interface Business {
  id: number;
  name: string;
  email?: string;
  status: string;
  currentPlanName?: string;
  subscriptionStatus: string;
}

interface Permit {
  id: number;
  businessId: number;
  businessName: string;
  businessEmail?: string;
  planName: string;
  planPermit: string;          // "Free" | "Paid"
  permitDuration?: string;     // "OneMonth" | "LifetimeFree"
  status: string;              // "LifetimeFree" | "MonthlyFree" | "Cancelled"
  startDate: string;
  endDate: string;
  grantedAt?: string;
  grantNote?: string;
  isLifetime: boolean;
}

/* ─── Helpers ────────────────────────────────────────────────── */
const COLORS = ['bg-indigo-600', 'bg-violet-600', 'bg-blue-600',
                 'bg-rose-600', 'bg-amber-600', 'bg-emerald-600'];

const Avatar: React.FC<{ name: string; idx: number }> = ({ name, idx }) => (
  <div className={clsx(
    'w-8 h-8 rounded-lg flex items-center justify-center text-white text-[10px] font-bold shrink-0',
    COLORS[idx % COLORS.length]
  )}>
    {name.substring(0, 2).toUpperCase()}
  </div>
);

const Badge: React.FC<{ children: React.ReactNode; variant: 'free' | 'paid' | 'lifetime' | 'month' | 'active' | 'revoked' }> = ({ children, variant }) => {
  const styles = {
    free:     'bg-emerald-100 text-emerald-700 border border-emerald-200',
    paid:     'bg-gray-100 text-gray-600 border border-gray-200',
    lifetime: 'bg-blue-100 text-blue-700 border border-blue-200',
    month:    'bg-orange-100 text-orange-700 border border-orange-200',
    active:   'bg-emerald-100 text-emerald-700 border border-emerald-200',
    revoked:  'bg-rose-100 text-rose-700 border border-rose-200',
  };
  return (
    <span className={clsx('inline-flex items-center gap-1 text-[10px] font-semibold px-2 py-0.5 rounded-full', styles[variant])}>
      {children}
    </span>
  );
};

/* ─── Grant Modal ────────────────────────────────────────────── */
const GrantModal: React.FC<{
  businesses: Business[];
  onClose: () => void;
  onGranted: () => void;
}> = ({ businesses, onClose, onGranted }) => {
  const [search, setSearch]                   = useState('');
  const [selected, setSelected]               = useState<Business | null>(null);
  const [planPermit, setPlanPermit]           = useState<'Free' | 'Paid'>('Free');
  const [permitDuration, setPermitDuration]   = useState<'OneMonth' | 'LifetimeFree'>('OneMonth');
  const [note, setNote]                       = useState('');
  const [granting, setGranting]               = useState(false);
  const [error, setError]                     = useState('');

  const filtered = businesses.filter(b =>
    b.name.toLowerCase().includes(search.toLowerCase()) ||
    (b.email || '').toLowerCase().includes(search.toLowerCase())
  );

  const isLifetime = permitDuration === 'LifetimeFree';

  const handleGrant = async () => {
    if (!selected) return;
    setGranting(true);
    setError('');
    try {
      await api.request(`/api/admin/shops/${selected.id}/grant-permit`, {
        method: 'POST',
        body: JSON.stringify({ permitDuration, note: note || null }),
      });
      onGranted();
      onClose();
    } catch (e: any) {
      setError(e?.message || 'Failed to grant permit.');
    } finally {
      setGranting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-xl flex flex-col max-h-[90vh]">

        {/* Header */}
        <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between shrink-0">
          <div className="flex items-center gap-2.5">
            <div className="w-9 h-9 rounded-xl bg-emerald-600 flex items-center justify-center">
              <Gift className="w-4.5 h-4.5 text-white w-5 h-5" />
            </div>
            <div>
              <p className="text-[13px] font-bold text-gray-900">Grant Plan Permit</p>
              <p className="text-[10px] text-gray-400">SuperAdmin · Free access override</p>
            </div>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 p-1 rounded-lg hover:bg-gray-100">
            <XCircle className="w-4.5 h-4.5 w-5 h-5" />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto px-6 py-5 space-y-5">

          {/* Step 1: Select Business */}
          <div>
            <label className="block text-[11px] font-bold text-gray-500 uppercase tracking-wider mb-2">
              1 · Select Business
            </label>
            <div className="relative mb-2">
              <Search className="w-3.5 h-3.5 absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                value={search}
                onChange={e => setSearch(e.target.value)}
                placeholder="Search by name or email…"
                className="w-full pl-8 pr-3 py-2 border border-gray-200 rounded-lg text-[11px] focus:outline-none focus:border-emerald-400 bg-gray-50"
              />
            </div>
            <div className="border border-gray-200 rounded-xl divide-y divide-gray-50 max-h-44 overflow-y-auto">
              {filtered.slice(0, 30).map((b, i) => (
                <button
                  key={b.id}
                  onClick={() => setSelected(b)}
                  className={clsx(
                    'w-full text-left px-3 py-2.5 flex items-center gap-2.5 hover:bg-emerald-50 transition-colors',
                    selected?.id === b.id && 'bg-emerald-50'
                  )}
                >
                  <Avatar name={b.name} idx={i} />
                  <div className="flex-1 min-w-0">
                    <p className="text-[11px] font-semibold text-gray-800 truncate">{b.name}</p>
                    <p className="text-[10px] text-gray-400 truncate">{b.email || '—'}</p>
                  </div>
                  <div className="flex items-center gap-1.5 shrink-0">
                    <span className="text-[9px] text-gray-400">{b.subscriptionStatus}</span>
                    {selected?.id === b.id && <CheckCircle className="w-4 h-4 text-emerald-500" />}
                  </div>
                </button>
              ))}
              {filtered.length === 0 && (
                <p className="px-4 py-4 text-[11px] text-gray-400 text-center">No businesses found</p>
              )}
            </div>
            {selected && (
              <div className="mt-2 flex items-center gap-2 px-3 py-2 bg-emerald-50 border border-emerald-200 rounded-lg">
                <CheckCircle className="w-3.5 h-3.5 text-emerald-500 shrink-0" />
                <p className="text-[11px] text-emerald-700 font-medium">
                  Selected: <strong>{selected.name}</strong>
                </p>
              </div>
            )}
          </div>

          {/* Step 2: Plan Permit Type */}
          <div>
            <label className="block text-[11px] font-bold text-gray-500 uppercase tracking-wider mb-2">
              2 · Plan Permit
            </label>
            <div className="grid grid-cols-2 gap-3">
              {([
                {
                  value: 'Free',
                  label: 'Free',
                  sub: 'No payment required · SuperAdmin override',
                  icon: Gift,
                  border: 'border-emerald-400 bg-emerald-50',
                  textActive: 'text-emerald-700',
                  textSub: 'text-emerald-600',
                },
                {
                  value: 'Paid',
                  label: 'Paid (Normal)',
                  sub: 'Regular paying subscription',
                  icon: FileText,
                  border: 'border-gray-300 bg-white',
                  textActive: 'text-gray-700',
                  textSub: 'text-gray-400',
                },
              ] as const).map(opt => {
                const isSelected = planPermit === opt.value;
                const Icon = opt.icon;
                return (
                  <button
                    key={opt.value}
                    onClick={() => setPlanPermit(opt.value)}
                    className={clsx(
                      'p-3.5 rounded-xl border-2 text-left transition-all',
                      isSelected ? opt.border : 'border-gray-200 hover:border-gray-300 bg-white'
                    )}
                  >
                    <div className="flex items-center gap-2 mb-1">
                      <Icon className={clsx('w-4 h-4', isSelected ? opt.textActive : 'text-gray-400')} />
                      <span className={clsx('text-[12px] font-bold', isSelected ? opt.textActive : 'text-gray-600')}>{opt.label}</span>
                    </div>
                    <p className={clsx('text-[10px] leading-relaxed', isSelected ? opt.textSub : 'text-gray-400')}>{opt.sub}</p>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Step 3: Duration (only shown when Free) */}
          {planPermit === 'Free' && (
            <div>
              <label className="block text-[11px] font-bold text-gray-500 uppercase tracking-wider mb-2">
                3 · Plan Permit Duration
              </label>
              <div className="grid grid-cols-2 gap-3">
                {([
                  {
                    value: 'OneMonth',
                    label: '1 Month Free',
                    sub: 'Free access for 30 days',
                    emoji: '🕐',
                    border: 'border-orange-400 bg-orange-50',
                    textActive: 'text-orange-700',
                    textSub: 'text-orange-500',
                  },
                  {
                    value: 'LifetimeFree',
                    label: 'Lifetime Free',
                    sub: 'Free access forever — no expiry',
                    emoji: '♾️',
                    border: 'border-blue-400 bg-blue-50',
                    textActive: 'text-blue-700',
                    textSub: 'text-blue-500',
                  },
                ] as const).map(opt => {
                  const isSelected = permitDuration === opt.value;
                  return (
                    <button
                      key={opt.value}
                      onClick={() => setPermitDuration(opt.value)}
                      className={clsx(
                        'p-3.5 rounded-xl border-2 text-left transition-all',
                        isSelected ? opt.border : 'border-gray-200 hover:border-gray-300 bg-white'
                      )}
                    >
                      <span className="text-xl mb-1 block">{opt.emoji}</span>
                      <p className={clsx('text-[12px] font-bold', isSelected ? opt.textActive : 'text-gray-700')}>{opt.label}</p>
                      <p className={clsx('text-[10px] mt-0.5', isSelected ? opt.textSub : 'text-gray-400')}>{opt.sub}</p>
                    </button>
                  );
                })}
              </div>
            </div>
          )}

          {/* Step 4: Internal Note */}
          <div>
            <label className="block text-[11px] font-bold text-gray-500 uppercase tracking-wider mb-2">
              {planPermit === 'Free' ? '4' : '3'} · Internal Note <span className="text-gray-400 font-normal">(optional)</span>
            </label>
            <textarea
              value={note}
              onChange={e => setNote(e.target.value)}
              rows={2}
              placeholder="e.g. Beta tester, Partner deal, Support courtesy, Early adopter reward…"
              className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-[11px] focus:outline-none focus:border-emerald-400 bg-gray-50 resize-none"
            />
          </div>

          {/* Confirmation Summary */}
          {selected && planPermit === 'Free' && (
            <div className={clsx(
              'rounded-xl p-4 border',
              isLifetime ? 'bg-blue-50 border-blue-200' : 'bg-orange-50 border-orange-200'
            )}>
              <p className="text-[11px] font-semibold text-gray-700 mb-1">
                {isLifetime ? '♾️ Lifetime Free Access' : '🕐 1 Month Free Access'}
              </p>
              <p className="text-[11px] text-gray-600 leading-relaxed">
                <strong>{selected.name}</strong> will receive{' '}
                {isLifetime
                  ? 'lifetime free access — no payment will ever be required.'
                  : `1 month free access valid until ${new Date(Date.now() + 30 * 86400000).toLocaleDateString('en-IN')}.`}
                {note && <><br /><span className="text-gray-400">Note: {note}</span></>}
              </p>
            </div>
          )}

          {error && (
            <div className="flex items-center gap-2 px-3 py-2.5 bg-rose-50 border border-rose-200 rounded-lg">
              <AlertCircle className="w-3.5 h-3.5 text-rose-500 shrink-0" />
              <p className="text-[11px] text-rose-600">{error}</p>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 py-4 border-t border-gray-100 flex gap-3 shrink-0">
          <button
            onClick={onClose}
            className="flex-1 py-2.5 border border-gray-200 rounded-xl text-[11px] font-semibold text-gray-600 hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={handleGrant}
            disabled={!selected || granting}
            className={clsx(
              'flex-1 py-2.5 rounded-xl text-[11px] font-bold flex items-center justify-center gap-2 transition-colors',
              planPermit === 'Free'
                ? 'bg-emerald-600 hover:bg-emerald-700 text-white disabled:opacity-50'
                : 'bg-gray-800 hover:bg-gray-900 text-white disabled:opacity-50'
            )}
          >
            {granting
              ? <><RefreshCw className="w-3.5 h-3.5 animate-spin" /> Granting…</>
              : planPermit === 'Free'
                ? <><Gift className="w-3.5 h-3.5" /> Grant Free Access</>
                : <><CheckCircle className="w-3.5 h-3.5" /> Assign Paid Plan</>
            }
          </button>
        </div>
      </div>
    </div>
  );
};

/* ─── Main Plan Permit Manager Component ────────────────────── */
export const PlanPermitManager: React.FC<{ businesses: Business[] }> = ({ businesses }) => {
  const [permits, setPermits]         = useState<Permit[]>([]);
  const [loading, setLoading]         = useState(true);
  const [showModal, setShowModal]     = useState(false);
  const [revoking, setRevoking]       = useState<number | null>(null);
  const [filterDuration, setFilterDuration] = useState<'All' | 'Lifetime' | 'Month'>('All');
  const [search, setSearch]           = useState('');

  const loadPermits = useCallback(async () => {
    setLoading(true);
    try {
      const data = await api.request<Permit[]>('/api/admin/plan-permits').catch(() => null);
      setPermits(data ?? []);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { void loadPermits(); }, [loadPermits]);

  const handleRevoke = async (businessId: number, name: string) => {
    if (!window.confirm(`Revoke free permit for "${name}"?\n\nThey will need to subscribe to continue using the software.`)) return;
    setRevoking(businessId);
    try {
      await api.request(`/api/admin/shops/${businessId}/revoke-permit`, { method: 'POST' });
      await loadPermits();
    } finally {
      setRevoking(null);
    }
  };

  const filtered = permits.filter(p => {
    const matchSearch = p.businessName.toLowerCase().includes(search.toLowerCase()) ||
                        (p.businessEmail || '').toLowerCase().includes(search.toLowerCase());
    const matchDuration = filterDuration === 'All' ||
                          (filterDuration === 'Lifetime' && p.isLifetime) ||
                          (filterDuration === 'Month' && !p.isLifetime);
    return matchSearch && matchDuration;
  });

  const totalLifetime = permits.filter(p => p.isLifetime).length;
  const totalMonth    = permits.filter(p => !p.isLifetime).length;

  return (
    <div className="space-y-5">

      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <p className="text-[10px] text-emerald-600 font-bold uppercase tracking-widest">SUPERADMIN CONTROL LAYER</p>
          <h1 className="text-xl font-black text-gray-900 mt-1">Plan Permit Manager</h1>
          <p className="text-[11px] text-gray-500 mt-1 max-w-lg">
            Grant or revoke free access for any business. Normal users pay their subscription; SuperAdmin can override to
            give <strong>1-month free</strong> or <strong>lifetime free</strong> access at zero cost.
          </p>
        </div>
        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white text-[11px] font-bold px-4 py-2.5 rounded-xl transition-colors shadow-sm"
        >
          <Gift className="w-3.5 h-3.5" /> Grant Free Permit
        </button>
      </div>

      {/* How It Works Banner */}
      <div className="bg-gradient-to-r from-indigo-50 to-emerald-50 border border-indigo-100 rounded-xl p-4">
        <p className="text-[11px] font-bold text-indigo-700 mb-2 flex items-center gap-1.5">
          <Info className="w-3.5 h-3.5" /> How Plan Permits Work
        </p>
        <div className="grid grid-cols-3 gap-4">
          {[
            {
              icon: User,
              color: 'text-gray-600',
              bg: 'bg-gray-100',
              title: 'Normal User (Paid)',
              desc: 'Registers, starts a 30-day trial, then must purchase a plan (Starter / Basic / Professional / Enterprise) to keep using the software.',
            },
            {
              icon: Clock,
              color: 'text-orange-600',
              bg: 'bg-orange-100',
              title: '1 Month Free',
              desc: 'SuperAdmin grants 30 days of completely free access. After 30 days, the business reverts to paid subscription or loses access.',
            },
            {
              icon: Star,
              color: 'text-blue-600',
              bg: 'bg-blue-100',
              title: 'Lifetime Free',
              desc: 'SuperAdmin grants permanent free access. The business never pays — all features unlocked forever at ₹0.',
            },
          ].map(item => {
            const Icon = item.icon;
            return (
              <div key={item.title} className="flex items-start gap-2.5">
                <div className={clsx('w-7 h-7 rounded-lg flex items-center justify-center shrink-0 mt-0.5', item.bg)}>
                  <Icon className={clsx('w-3.5 h-3.5', item.color)} />
                </div>
                <div>
                  <p className="text-[11px] font-bold text-gray-800">{item.title}</p>
                  <p className="text-[10px] text-gray-500 leading-relaxed mt-0.5">{item.desc}</p>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-3 gap-3">
        <div className="bg-white border border-gray-200 rounded-xl p-4 shadow-xs">
          <p className="text-[10px] font-bold uppercase tracking-widest text-gray-400 mb-1">Total Free Permits</p>
          <p className="text-2xl font-black text-gray-900">{permits.length}</p>
          <p className="text-[10px] text-gray-400 mt-0.5">Active superadmin grants</p>
        </div>
        <div className="bg-blue-50 border border-blue-100 rounded-xl p-4 shadow-xs">
          <p className="text-[10px] font-bold uppercase tracking-widest text-blue-400 mb-1">Lifetime Free</p>
          <p className="text-2xl font-black text-blue-700">{totalLifetime}</p>
          <p className="text-[10px] text-blue-400 mt-0.5">♾️ Free forever</p>
        </div>
        <div className="bg-orange-50 border border-orange-100 rounded-xl p-4 shadow-xs">
          <p className="text-[10px] font-bold uppercase tracking-widest text-orange-400 mb-1">1-Month Free</p>
          <p className="text-2xl font-black text-orange-700">{totalMonth}</p>
          <p className="text-[10px] text-orange-400 mt-0.5">🕐 30-day grants</p>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-xs overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center gap-3 flex-wrap">
          <p className="text-[11px] font-bold text-gray-800 flex-1">Active Free Permits</p>

          {/* Search */}
          <div className="relative">
            <Search className="w-3 h-3 absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Search business…"
              className="pl-7 pr-3 py-1.5 text-[11px] bg-gray-50 border border-gray-200 rounded-lg focus:outline-none focus:border-emerald-400 w-44"
            />
          </div>

          {/* Filter */}
          {(['All', 'Lifetime', 'Month'] as const).map(f => (
            <button
              key={f}
              onClick={() => setFilterDuration(f)}
              className={clsx(
                'text-[10px] px-2.5 py-1 rounded-lg font-semibold transition-colors',
                filterDuration === f ? 'bg-emerald-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
              )}
            >
              {f === 'All' ? 'All' : f === 'Lifetime' ? '♾️ Lifetime' : '🕐 1 Month'}
            </button>
          ))}

          <button onClick={loadPermits} className="text-gray-400 hover:text-gray-600 p-1.5 rounded-lg hover:bg-gray-100 transition-colors">
            <RefreshCw className="w-3.5 h-3.5" />
          </button>
        </div>

        {loading ? (
          <div className="py-16 text-center">
            <RefreshCw className="w-6 h-6 text-gray-300 animate-spin mx-auto mb-2" />
            <p className="text-[11px] text-gray-400">Loading permits…</p>
          </div>
        ) : filtered.length === 0 ? (
          <div className="py-16 text-center">
            <Gift className="w-10 h-10 text-gray-200 mx-auto mb-3" />
            <p className="text-[12px] font-semibold text-gray-500">No free permits granted yet</p>
            <p className="text-[11px] text-gray-400 mt-1">Click "Grant Free Permit" to give a business free access.</p>
          </div>
        ) : (
          <table className="w-full text-[11px]">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/70">
                {['Business', 'Plan Permit', 'Plan Permit Duration', 'Status', 'Valid Until', 'Granted At', 'Note', 'Actions'].map(h => (
                  <th key={h} className="text-left py-2.5 px-3 text-[10px] font-semibold uppercase tracking-wider text-gray-400">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {filtered.map((p, i) => (
                <tr key={p.id} className="border-b border-gray-50 hover:bg-emerald-50/30 transition-colors">

                  {/* Business */}
                  <td className="py-3 px-3">
                    <div className="flex items-center gap-2">
                      <Avatar name={p.businessName} idx={i} />
                      <div>
                        <p className="font-semibold text-gray-800">{p.businessName}</p>
                        <p className="text-gray-400 text-[10px]">{p.businessEmail || '—'}</p>
                      </div>
                    </div>
                  </td>

                  {/* Plan Permit */}
                  <td className="px-3">
                    <Badge variant="free">
                      <Gift className="w-3 h-3" /> Free
                    </Badge>
                  </td>

                  {/* Plan Permit Duration */}
                  <td className="px-3">
                    {p.isLifetime ? (
                      <Badge variant="lifetime">♾️ Lifetime Free</Badge>
                    ) : (
                      <Badge variant="month">🕐 1 Month Free</Badge>
                    )}
                  </td>

                  {/* Status */}
                  <td className="px-3">
                    <Badge variant={p.status === 'Cancelled' ? 'revoked' : 'active'}>
                      {p.status === 'LifetimeFree' ? '● Lifetime Free' :
                       p.status === 'MonthlyFree'  ? '● Monthly Free'  :
                       p.status === 'Cancelled'    ? '✕ Revoked'       : p.status}
                    </Badge>
                  </td>

                  {/* Valid Until */}
                  <td className="px-3 text-gray-600">
                    {p.isLifetime
                      ? <span className="text-blue-600 font-semibold">♾️ Lifetime</span>
                      : new Date(p.endDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })}
                  </td>

                  {/* Granted At */}
                  <td className="px-3 text-gray-500 text-[10px]">
                    {p.grantedAt
                      ? new Date(p.grantedAt).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })
                      : '—'}
                  </td>

                  {/* Note */}
                  <td className="px-3 text-gray-500 text-[10px] max-w-[120px] truncate" title={p.grantNote || ''}>
                    {p.grantNote || <span className="text-gray-300">—</span>}
                  </td>

                  {/* Actions */}
                  <td className="px-3">
                    <button
                      onClick={() => handleRevoke(p.businessId, p.businessName)}
                      disabled={revoking === p.businessId || p.status === 'Cancelled'}
                      title="Revoke free permit"
                      className="flex items-center gap-1 text-rose-400 hover:text-rose-600 hover:bg-rose-50 px-2 py-1 rounded-lg transition-colors text-[10px] font-semibold disabled:opacity-40 disabled:cursor-not-allowed"
                    >
                      {revoking === p.businessId
                        ? <RefreshCw className="w-3 h-3 animate-spin" />
                        : <Trash2 className="w-3 h-3" />}
                      Revoke
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {filtered.length > 0 && (
          <div className="px-4 py-2.5 border-t border-gray-100 text-[10px] text-gray-400">
            Showing {filtered.length} of {permits.length} permits
          </div>
        )}
      </div>

      {/* Modal */}
      {showModal && (
        <GrantModal
          businesses={businesses}
          onClose={() => setShowModal(false)}
          onGranted={loadPermits}
        />
      )}
    </div>
  );
};

export default PlanPermitManager;
