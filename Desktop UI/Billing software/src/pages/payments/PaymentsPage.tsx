import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CreditCard, Users, Truck, BarChart3, History, AlertCircle,
  RefreshCw, RotateCcw, Monitor, Settings2, CheckCircle2,
  Search, ArrowRight, Loader2, IndianRupee,
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import api from '../../services/apiClient';

// ─── Types ───────────────────────────────────────────────────────────────────
type Tab = 'overview' | 'history' | 'due' | 'refunds' | 'terminals' | 'config';

const TABS: { id: Tab; label: string; icon: React.ReactNode }[] = [
  { id: 'overview',   label: 'Overview',      icon: <BarChart3 className="w-4 h-4" /> },
  { id: 'history',    label: 'History',       icon: <History className="w-4 h-4" /> },
  { id: 'due',        label: 'Due Payments',  icon: <AlertCircle className="w-4 h-4" /> },
  { id: 'refunds',    label: 'Refunds',       icon: <RotateCcw className="w-4 h-4" /> },
  { id: 'terminals',  label: 'POS Terminals', icon: <Monitor className="w-4 h-4" /> },
  { id: 'config',     label: 'Configuration', icon: <Settings2 className="w-4 h-4" /> },
];

function fmt(n: number) {
  return '₹' + n.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function StatusBadge({ status }: { status: string }) {
  const s = status?.toLowerCase();
  const cls =
    s === 'completed' || s === 'paid' || s === 'success' || s === 'approved'
      ? 'bg-green-100 text-green-700'
      : s === 'pending' || s === 'initiated'
      ? 'bg-amber-100 text-amber-700'
      : s === 'failed' || s === 'declined' || s === 'cancelled'
      ? 'bg-red-100 text-red-700'
      : 'bg-slate-100 text-slate-600';
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold uppercase ${cls}`}>
      {status}
    </span>
  );
}

// ─── Overview Tab ─────────────────────────────────────────────────────────────
const OverviewTab: React.FC = () => {
  const navigate = useNavigate();
  const { customers, suppliers } = useStore();
  const [summary, setSummary] = useState<any>(null);
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await api.payments.dailySummary(date);
      setSummary(r.data);
    } catch { setSummary(null); }
    finally { setLoading(false); }
  }, [date]);

  useEffect(() => { load(); }, [load]);

  const customerReceivablesTotal = customers.reduce((s, c) => s + c.outstandingBalance, 0);
  const supplierPayablesTotal   = suppliers.reduce((s, sp) => s + sp.outstandingPayable, 0);

  const cards = [
    { label: 'Total Collections', value: summary?.totalAmount ?? 0,       color: 'text-indigo-600', bg: 'bg-indigo-50' },
    { label: 'Cash',              value: summary?.cashTotal ?? 0,          color: 'text-green-700',  bg: 'bg-green-50' },
    { label: 'UPI',               value: summary?.upiTotal ?? 0,           color: 'text-purple-700', bg: 'bg-purple-50' },
    { label: 'Card',              value: summary?.cardTotal ?? 0,          color: 'text-blue-700',   bg: 'bg-blue-50' },
    { label: 'Other',             value: summary?.otherTotal ?? 0,         color: 'text-orange-700', bg: 'bg-orange-50' },
    { label: 'Transactions',      value: summary?.totalTransactions ?? 0,  color: 'text-slate-700',  bg: 'bg-slate-50', isCount: true },
  ];

  return (
    <div className="space-y-6">
      {/* Daily summary */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-bold text-slate-900">Daily Collection Summary</h3>
          <div className="flex items-center gap-2">
            <input
              type="date"
              value={date}
              onChange={e => setDate(e.target.value)}
              className="px-3 py-1.5 text-xs rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-indigo-300"
            />
            <button onClick={load} className="p-1.5 rounded-lg border border-slate-200 hover:bg-slate-50">
              <RefreshCw className={`w-4 h-4 text-slate-500 ${loading ? 'animate-spin' : ''}`} />
            </button>
          </div>
        </div>

        <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
          {cards.map(c => (
            <div key={c.label} className={`${c.bg} rounded-xl p-4`}>
              <p className="text-xs text-slate-500 font-semibold mb-1">{c.label}</p>
              <p className={`text-xl font-extrabold ${c.color}`}>
                {c.isCount ? c.value : fmt(c.value)}
              </p>
            </div>
          ))}
        </div>
      </div>

      {/* Customer Khata & Supplier Payables */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Customer Khata */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
          <div className="flex items-center justify-between border-b border-slate-100 pb-3">
            <div className="flex items-center gap-2">
              <Users className="w-5 h-5 text-amber-600" />
              <h2 className="text-base font-bold text-slate-900">Customer Khata</h2>
            </div>
            <button onClick={() => navigate('/customers')} className="text-xs font-semibold text-indigo-600 hover:underline">
              Manage
            </button>
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Total Due</span>
            <h3 className="text-3xl font-extrabold text-amber-600 mt-1">{fmt(customerReceivablesTotal)}</h3>
          </div>
          <div className="space-y-2 max-h-48 overflow-y-auto">
            {customers.filter(c => c.outstandingBalance > 0).map(c => (
              <div key={c.id} className="p-3 bg-slate-50 rounded-xl border border-slate-100 flex items-center justify-between">
                <div>
                  <p className="text-xs font-bold text-slate-900">{c.name}</p>
                  <p className="text-[10px] text-slate-500 font-mono">{c.phone}</p>
                </div>
                <div className="text-right">
                  <p className="text-xs font-bold text-amber-600 font-mono">{fmt(c.outstandingBalance)}</p>
                  <button onClick={() => navigate(`/customers/${c.id}/ledger`)} className="text-[10px] text-indigo-600 font-semibold hover:underline">
                    View Ledger →
                  </button>
                </div>
              </div>
            ))}
            {customers.filter(c => c.outstandingBalance > 0).length === 0 && (
              <p className="text-xs text-slate-400 text-center py-4">No outstanding balances</p>
            )}
          </div>
        </div>

        {/* Supplier Payables */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
          <div className="flex items-center justify-between border-b border-slate-100 pb-3">
            <div className="flex items-center gap-2">
              <Truck className="w-5 h-5 text-indigo-600" />
              <h2 className="text-base font-bold text-slate-900">Supplier Payables</h2>
            </div>
            <button onClick={() => navigate('/suppliers')} className="text-xs font-semibold text-indigo-600 hover:underline">
              Manage
            </button>
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Total Payable</span>
            <h3 className="text-3xl font-extrabold text-indigo-900 mt-1">{fmt(supplierPayablesTotal)}</h3>
          </div>
          <div className="space-y-2 max-h-48 overflow-y-auto">
            {suppliers.map(s => (
              <div key={s.id} className="p-3 bg-slate-50 rounded-xl border border-slate-100 flex items-center justify-between">
                <div>
                  <p className="text-xs font-bold text-slate-900">{s.companyName}</p>
                  <p className="text-[10px] text-slate-500">{s.name} ({s.phone})</p>
                </div>
                <div className="text-right">
                  <p className="text-xs font-bold text-slate-900 font-mono">{fmt(s.outstandingPayable)}</p>
                  <span className="text-[10px] text-slate-400">Payable</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

// ─── History Tab ──────────────────────────────────────────────────────────────
const HistoryTab: React.FC = () => {
  const [payments, setPayments] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const pageSize = 15;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await api.payments.list(page, pageSize);
      setPayments(r.data?.items ?? r.data ?? []);
      setTotal(r.data?.totalCount ?? 0);
    } catch { setPayments([]); }
    finally { setLoading(false); }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  return (
    <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs">
      <div className="flex items-center justify-between p-6 border-b border-slate-100">
        <h3 className="font-bold text-slate-900">Payment History</h3>
        <button onClick={load} className="p-1.5 rounded-lg border border-slate-200 hover:bg-slate-50">
          <RefreshCw className={`w-4 h-4 text-slate-500 ${loading ? 'animate-spin' : ''}`} />
        </button>
      </div>
      {loading ? (
        <div className="flex justify-center py-16"><Loader2 className="w-6 h-6 animate-spin text-indigo-500" /></div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b border-slate-100 bg-slate-50 text-slate-500 font-semibold uppercase tracking-wide">
                <th className="px-4 py-3 text-left">Invoice</th>
                <th className="px-4 py-3 text-left">Customer</th>
                <th className="px-4 py-3 text-left">Method</th>
                <th className="px-4 py-3 text-right">Total</th>
                <th className="px-4 py-3 text-right">Paid</th>
                <th className="px-4 py-3 text-left">Date</th>
                <th className="px-4 py-3 text-left">Status</th>
              </tr>
            </thead>
            <tbody>
              {payments.map((p: any) => (
                <tr key={p.id} className="border-b border-slate-50 hover:bg-slate-50/50">
                  <td className="px-4 py-3 font-mono text-indigo-700">{p.invoiceNumber || `#${p.id}`}</td>
                  <td className="px-4 py-3 text-slate-700">{p.customerName || '—'}</td>
                  <td className="px-4 py-3">
                    <span className="px-2 py-0.5 bg-slate-100 rounded-full font-semibold text-slate-600">
                      {p.paymentMethod || (p.details?.length > 1 ? 'Split' : p.details?.[0]?.method) || '—'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right font-mono">{fmt(p.totalAmount ?? 0)}</td>
                  <td className="px-4 py-3 text-right font-mono text-green-700">{fmt(p.paidAmount ?? 0)}</td>
                  <td className="px-4 py-3 text-slate-500">{p.paymentDate ? new Date(p.paymentDate).toLocaleDateString('en-IN') : '—'}</td>
                  <td className="px-4 py-3"><StatusBadge status={p.status} /></td>
                </tr>
              ))}
              {payments.length === 0 && (
                <tr><td colSpan={7} className="text-center py-12 text-slate-400">No payment records found</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
      {total > pageSize && (
        <div className="flex items-center justify-between p-4 border-t border-slate-100">
          <span className="text-xs text-slate-500">{total} records</span>
          <div className="flex gap-2">
            <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1}
              className="px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-200 disabled:opacity-40">← Prev</button>
            <button onClick={() => setPage(p => p + 1)} disabled={page * pageSize >= total}
              className="px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-200 disabled:opacity-40">Next →</button>
          </div>
        </div>
      )}
    </div>
  );
};

// ─── Due Payments Tab ─────────────────────────────────────────────────────────
const DueTab: React.FC = () => {
  const navigate = useNavigate();
  const [dues, setDues] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');

  useEffect(() => {
    setLoading(true);
    api.payments.due()
      .then(r => setDues(r.data ?? []))
      .catch(() => setDues([]))
      .finally(() => setLoading(false));
  }, []);

  const filtered = dues.filter(d =>
    !search ||
    d.invoiceNumber?.toLowerCase().includes(search.toLowerCase()) ||
    d.customerName?.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs">
      <div className="p-6 border-b border-slate-100">
        <div className="flex items-center justify-between mb-3">
          <h3 className="font-bold text-slate-900">Outstanding Due Payments</h3>
          <span className="text-xs font-bold bg-red-100 text-red-700 px-2 py-0.5 rounded-full">{dues.length} pending</span>
        </div>
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <input
            type="text"
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Search invoice or customer…"
            className="w-full pl-9 pr-4 py-2 text-xs rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-indigo-300"
          />
        </div>
      </div>
      {loading ? (
        <div className="flex justify-center py-16"><Loader2 className="w-6 h-6 animate-spin text-indigo-500" /></div>
      ) : (
        <div className="divide-y divide-slate-50">
          {filtered.map((d: any) => (
            <div key={d.id} className="flex items-center justify-between p-4 hover:bg-slate-50/50">
              <div>
                <p className="text-xs font-bold text-indigo-700 font-mono">{d.invoiceNumber}</p>
                <p className="text-xs text-slate-700 font-semibold">{d.customerName || 'Walk-in'}</p>
                <p className="text-[10px] text-slate-400">{d.invoiceDate ? new Date(d.invoiceDate).toLocaleDateString('en-IN') : ''}</p>
              </div>
              <div className="flex items-center gap-3">
                <div className="text-right">
                  <p className="text-xs text-slate-500">Due</p>
                  <p className="text-sm font-extrabold text-red-600 font-mono">{fmt(d.dueAmount ?? 0)}</p>
                </div>
                <button
                  onClick={() => navigate(`/invoices/${d.id}`)}
                  className="flex items-center gap-1 px-3 py-1.5 bg-indigo-600 text-white text-xs font-bold rounded-lg hover:bg-indigo-700"
                >
                  Collect <ArrowRight className="w-3 h-3" />
                </button>
              </div>
            </div>
          ))}
          {filtered.length === 0 && (
            <p className="text-center py-12 text-xs text-slate-400">
              {search ? 'No matching records' : '🎉 No outstanding payments!'}
            </p>
          )}
        </div>
      )}
    </div>
  );
};

// ─── Refunds Tab ──────────────────────────────────────────────────────────────
const RefundsTab: React.FC = () => {
  const [refunds, setRefunds] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setLoading(true);
    api.paymentRefunds.list()
      .then(r => setRefunds(r.data ?? []))
      .catch(() => setRefunds([]))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs">
      <div className="flex items-center justify-between p-6 border-b border-slate-100">
        <h3 className="font-bold text-slate-900">Refund History</h3>
      </div>
      {loading ? (
        <div className="flex justify-center py-16"><Loader2 className="w-6 h-6 animate-spin text-indigo-500" /></div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b border-slate-100 bg-slate-50 text-slate-500 font-semibold uppercase tracking-wide">
                <th className="px-4 py-3 text-left">Invoice</th>
                <th className="px-4 py-3 text-left">Customer</th>
                <th className="px-4 py-3 text-left">Reason</th>
                <th className="px-4 py-3 text-right">Amount</th>
                <th className="px-4 py-3 text-left">Method</th>
                <th className="px-4 py-3 text-left">Date</th>
                <th className="px-4 py-3 text-left">Status</th>
              </tr>
            </thead>
            <tbody>
              {refunds.map((r: any) => (
                <tr key={r.id} className="border-b border-slate-50 hover:bg-slate-50/50">
                  <td className="px-4 py-3 font-mono text-indigo-700">{r.invoiceNumber || `#${r.invoiceId}`}</td>
                  <td className="px-4 py-3">{r.customerName || '—'}</td>
                  <td className="px-4 py-3 text-slate-600">{r.reason || '—'}</td>
                  <td className="px-4 py-3 text-right font-mono text-red-600 font-bold">{fmt(r.amount ?? 0)}</td>
                  <td className="px-4 py-3">{r.refundMethod || '—'}</td>
                  <td className="px-4 py-3 text-slate-500">{r.refundDate ? new Date(r.refundDate).toLocaleDateString('en-IN') : '—'}</td>
                  <td className="px-4 py-3"><StatusBadge status={r.status} /></td>
                </tr>
              ))}
              {refunds.length === 0 && (
                <tr><td colSpan={7} className="text-center py-12 text-slate-400">No refund records found</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

// ─── POS Terminals Tab ────────────────────────────────────────────────────────
const TerminalsTab: React.FC = () => {
  const [terminals, setTerminals] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [testing, setTesting] = useState<Record<number, boolean>>({});
  const [testResults, setTestResults] = useState<Record<number, { ok: boolean; msg: string }>>({});

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await api.paymentTerminals.list();
      setTerminals(r.data ?? []);
    } catch { setTerminals([]); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  const testConnection = async (id: number) => {
    setTesting(p => ({ ...p, [id]: true }));
    try {
      const r = await api.paymentTerminals.testConnection(id);
      setTestResults(p => ({ ...p, [id]: { ok: r.data?.isConnected ?? false, msg: r.data?.message ?? '' } }));
    } catch (e: any) {
      setTestResults(p => ({ ...p, [id]: { ok: false, msg: e.message || 'Connection failed' } }));
    } finally {
      setTesting(p => ({ ...p, [id]: false }));
    }
  };

  return (
    <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs">
      <div className="flex items-center justify-between p-6 border-b border-slate-100">
        <h3 className="font-bold text-slate-900">POS Terminals</h3>
        <button onClick={load} className="p-1.5 rounded-lg border border-slate-200 hover:bg-slate-50">
          <RefreshCw className={`w-4 h-4 text-slate-500 ${loading ? 'animate-spin' : ''}`} />
        </button>
      </div>
      {loading ? (
        <div className="flex justify-center py-16"><Loader2 className="w-6 h-6 animate-spin text-indigo-500" /></div>
      ) : (
        <div className="divide-y divide-slate-50">
          {terminals.map((t: any) => (
            <div key={t.id} className="flex items-center justify-between p-4 hover:bg-slate-50/50">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-indigo-50 rounded-xl">
                  <Monitor className="w-5 h-5 text-indigo-600" />
                </div>
                <div>
                  <p className="text-sm font-bold text-slate-900">{t.terminalName}</p>
                  <p className="text-xs text-slate-500">{t.providerName} · {t.terminalId}</p>
                  {t.ipAddress && <p className="text-[10px] text-slate-400 font-mono">{t.ipAddress}:{t.port}</p>}
                </div>
              </div>
              <div className="flex items-center gap-3">
                <StatusBadge status={t.status || 'Unknown'} />
                {testResults[t.id] && (
                  <span className={`text-xs font-semibold ${testResults[t.id].ok ? 'text-green-600' : 'text-red-600'}`}>
                    {testResults[t.id].ok ? '✓ Connected' : '✗ ' + testResults[t.id].msg}
                  </span>
                )}
                <button
                  onClick={() => testConnection(t.id)}
                  disabled={testing[t.id]}
                  className="flex items-center gap-1 px-3 py-1.5 bg-indigo-50 text-indigo-700 text-xs font-bold rounded-lg hover:bg-indigo-100 disabled:opacity-50"
                >
                  {testing[t.id] ? <Loader2 className="w-3 h-3 animate-spin" /> : <CheckCircle2 className="w-3 h-3" />}
                  Test
                </button>
              </div>
            </div>
          ))}
          {terminals.length === 0 && (
            <div className="text-center py-16 text-slate-400">
              <Monitor className="w-10 h-10 mx-auto mb-3 opacity-30" />
              <p className="text-sm font-semibold">No POS terminals registered</p>
              <p className="text-xs mt-1">Contact your payment provider to set up a POS terminal</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

// ─── Config Tab ───────────────────────────────────────────────────────────────
const ConfigTab: React.FC = () => {
  const [methods, setMethods] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    setLoading(true);
    api.paymentConfig.get()
      .then(r => setMethods(r.data ?? []))
      .catch(() => setMethods([]))
      .finally(() => setLoading(false));
  }, []);

  const toggle = (index: number) => {
    setMethods(prev => prev.map((m, i) => i === index ? { ...m, isEnabled: !m.isEnabled } : m));
  };

  const save = async () => {
    setSaving(true);
    try {
      await api.paymentConfig.update(methods);
      setSaved(true);
      setTimeout(() => setSaved(false), 3000);
    } catch {}
    finally { setSaving(false); }
  };

  const METHOD_ICONS: Record<string, string> = {
    Cash: '💵', UPI: '📱', DebitCard: '💳', CreditCard: '💳', NetBanking: '🏦',
    Wallet: '👛', GiftCard: '🎁', StoreCredit: '🏪', CustomerCredit: '📒',
    BankTransfer: '🔄', Cheque: '📃', EMI: '📅', PaymentOnDelivery: '🚚', Online: '🌐',
  };

  return (
    <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs">
      <div className="flex items-center justify-between p-6 border-b border-slate-100">
        <div>
          <h3 className="font-bold text-slate-900">Payment Method Configuration</h3>
          <p className="text-xs text-slate-500 mt-0.5">Enable or disable payment methods for your shop</p>
        </div>
        <button
          onClick={save}
          disabled={saving}
          className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 text-white text-xs font-bold rounded-lg hover:bg-indigo-700 disabled:opacity-50"
        >
          {saving ? <Loader2 className="w-3 h-3 animate-spin" /> : saved ? <CheckCircle2 className="w-3 h-3" /> : null}
          {saved ? 'Saved!' : 'Save Changes'}
        </button>
      </div>
      {loading ? (
        <div className="flex justify-center py-16"><Loader2 className="w-6 h-6 animate-spin text-indigo-500" /></div>
      ) : (
        <div className="p-6 grid grid-cols-1 sm:grid-cols-2 gap-3">
          {methods.map((m: any, i: number) => (
            <div
              key={m.methodCode || m.method || i}
              className={`flex items-center justify-between p-4 rounded-xl border-2 transition-all ${
                m.isEnabled ? 'border-indigo-200 bg-indigo-50' : 'border-slate-200 bg-slate-50'
              }`}
            >
              <div className="flex items-center gap-3">
                <span className="text-xl">{METHOD_ICONS[m.methodCode || m.method] || '💰'}</span>
                <div>
                  <p className="text-sm font-bold text-slate-900">{m.displayName || m.method || m.methodCode}</p>
                  <p className="text-[10px] text-slate-500 font-mono">{m.methodCode || m.method}</p>
                </div>
              </div>
              <button
                onClick={() => toggle(i)}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  m.isEnabled ? 'bg-indigo-600' : 'bg-slate-300'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white shadow-sm transition-transform ${
                    m.isEnabled ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </div>
          ))}
          {methods.length === 0 && (
            <div className="col-span-2 text-center py-10 text-slate-400 text-sm">
              No payment methods configured yet. They will appear here after first setup.
            </div>
          )}
        </div>
      )}
    </div>
  );
};

// ─── Main Page ────────────────────────────────────────────────────────────────
export const PaymentsPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<Tab>('overview');

  return (
    <div className="space-y-5">
      {/* Page header */}
      <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div className="flex items-center gap-3">
          <div className="p-2.5 bg-indigo-50 rounded-xl">
            <IndianRupee className="w-6 h-6 text-indigo-600" />
          </div>
          <div>
            <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Payments</h1>
            <p className="text-xs sm:text-sm text-slate-500">
              Manage collections, dues, refunds, POS terminals &amp; payment configuration
            </p>
          </div>
        </div>
      </div>

      {/* Tab bar */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <div className="flex overflow-x-auto border-b border-slate-200 scrollbar-none">
          {TABS.map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex items-center gap-1.5 px-4 py-3.5 text-xs font-bold whitespace-nowrap border-b-2 transition-colors ${
                activeTab === tab.id
                  ? 'border-indigo-600 text-indigo-600 bg-indigo-50/50'
                  : 'border-transparent text-slate-500 hover:text-slate-700 hover:bg-slate-50'
              }`}
            >
              {tab.icon}
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* Tab content */}
      {activeTab === 'overview'  && <OverviewTab />}
      {activeTab === 'history'   && <HistoryTab />}
      {activeTab === 'due'       && <DueTab />}
      {activeTab === 'refunds'   && <RefundsTab />}
      {activeTab === 'terminals' && <TerminalsTab />}
      {activeTab === 'config'    && <ConfigTab />}
    </div>
  );
};
