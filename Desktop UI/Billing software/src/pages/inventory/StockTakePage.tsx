import React, { useState, useEffect, useCallback } from 'react';
import { ClipboardList, Plus, CheckCircle, BarChart2, AlertTriangle, Search } from 'lucide-react';
import { api } from '../../services/apiClient';

interface StockTakeItem { id: number; productId: number; productName: string; sku?: string; barcode?: string; systemStock: number; countedStock?: number; discrepancy?: number; isAdjusted: boolean; notes?: string; }
interface Session { id: number; sessionNumber: string; name: string; status: string; totalProducts: number; countedProducts: number; discrepancyCount: number; createdAt: string; completedAt?: string; items: StockTakeItem[]; }

const STATUS_COLORS: Record<string, string> = {
  InProgress: 'bg-blue-100 text-blue-700',
  Completed: 'bg-emerald-100 text-emerald-700',
  Cancelled: 'bg-rose-100 text-rose-700',
};

export const StockTakePage: React.FC = () => {
  const [sessions, setSessions] = useState<Session[]>([]);
  const [activeSession, setActiveSession] = useState<Session | null>(null);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [searchFilter, setSearchFilter] = useState('');
  const [saving, setSaving] = useState(false);
  const [applying, setApplying] = useState(false);
  const [form, setForm] = useState({ name: '', notes: '' });
  const [counts, setCounts] = useState<Record<number, string>>({});

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.request<Session[]>('/api/stock-take?page=1&pageSize=20');
      setSessions(Array.isArray(res) ? res : []);
    } catch { setSessions([]); }
    setLoading(false);
  }, []);

  useEffect(() => { load(); }, [load]);

  const openSession = async (id: number) => {
    const res = await api.request<Session>(`/api/stock-take/${id}`);
    setActiveSession(res);
    const init: Record<number, string> = {};
    res.items.forEach(i => { if (i.countedStock != null) init[i.id] = String(i.countedStock); });
    setCounts(init);
  };

  const createSession = async () => {
    setSaving(true);
    try {
      const res = await api.request<Session>('/api/stock-take', { method: 'POST', body: JSON.stringify({ name: form.name, notes: form.notes }) });
      setShowCreate(false);
      setActiveSession(res);
      setCounts({});
      load();
    } catch { }
    setSaving(false);
  };

  const saveCount = async (itemId: number) => {
    if (!activeSession || counts[itemId] === undefined) return;
    await api.request(`/api/stock-take/${activeSession.id}/bulk-update`, {
      method: 'PUT',
      body: JSON.stringify([{ itemId, countedStock: Number(counts[itemId]) }])
    });
  };

  const complete = async () => {
    if (!activeSession) return;
    await api.request(`/api/stock-take/${activeSession.id}/complete`, { method: 'POST' });
    openSession(activeSession.id);
    load();
  };

  const applyAdjustments = async () => {
    if (!activeSession) return;
    setApplying(true);
    try {
      const count = await api.request<number>(`/api/stock-take/${activeSession.id}/apply-adjustments`, { method: 'POST' });
      alert(`${count} adjustments applied to inventory`);
      openSession(activeSession.id);
    } catch { }
    setApplying(false);
  };

  const filteredItems = activeSession?.items.filter(i =>
    i.productName.toLowerCase().includes(searchFilter.toLowerCase()) ||
    (i.barcode || '').includes(searchFilter) ||
    (i.sku || '').toLowerCase().includes(searchFilter.toLowerCase())
  ) ?? [];

  if (activeSession) {
    return (
      <div className="p-4 md:p-6 space-y-4">
        <div className="flex items-center justify-between flex-wrap gap-3">
          <div className="flex items-center gap-3">
            <button onClick={() => setActiveSession(null)} className="text-xs text-slate-500 hover:text-slate-700">← Back</button>
            <div>
              <h1 className="text-base font-bold text-slate-800">{activeSession.name}</h1>
              <div className="flex items-center gap-3 mt-0.5">
                <span className={`text-[10px] px-1.5 py-0.5 rounded font-semibold ${STATUS_COLORS[activeSession.status]}`}>{activeSession.status}</span>
                <span className="text-xs text-slate-500">{activeSession.countedProducts}/{activeSession.totalProducts} counted</span>
                {activeSession.discrepancyCount > 0 && <span className="text-xs text-amber-600 flex items-center gap-1"><AlertTriangle className="w-3 h-3" />{activeSession.discrepancyCount} discrepancies</span>}
              </div>
            </div>
          </div>
          <div className="flex gap-2">
            {activeSession.status === 'InProgress' && (
              <button onClick={complete} className="px-3 py-1.5 text-xs font-semibold bg-blue-600 text-white rounded-lg hover:bg-blue-700">Complete Session</button>
            )}
            {activeSession.status === 'Completed' && activeSession.discrepancyCount > 0 && (
              <button onClick={applyAdjustments} disabled={applying} className="px-3 py-1.5 text-xs font-semibold bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 disabled:opacity-50">
                {applying ? 'Applying...' : 'Apply Adjustments'}
              </button>
            )}
          </div>
        </div>
        <div className="relative">
          <Search className="w-3.5 h-3.5 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input value={searchFilter} onChange={e => setSearchFilter(e.target.value)} placeholder="Search by name, barcode or SKU..." className="pl-8 pr-4 py-2 text-xs border border-slate-200 rounded-xl w-full max-w-md focus:ring-1 focus:ring-emerald-400 outline-none" />
        </div>
        <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
          <table className="w-full text-xs">
            <thead>
              <tr className="bg-slate-50 border-b border-slate-100">
                <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Product</th>
                <th className="text-left px-3 py-2.5 font-semibold text-slate-600">SKU/Barcode</th>
                <th className="text-right px-3 py-2.5 font-semibold text-slate-600">System Stock</th>
                <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Counted</th>
                <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Diff</th>
              </tr>
            </thead>
            <tbody>
              {filteredItems.map(item => {
                const counted = counts[item.id];
                const diff = counted !== undefined ? Number(counted) - item.systemStock : item.discrepancy;
                return (
                  <tr key={item.id} className={`border-b border-slate-50 ${diff !== undefined && diff !== null && diff !== 0 ? 'bg-amber-50/50' : ''}`}>
                    <td className="px-3 py-2 font-medium text-slate-700">{item.productName}</td>
                    <td className="px-3 py-2 text-slate-400">{item.sku || item.barcode || '-'}</td>
                    <td className="px-3 py-2 text-right text-slate-600">{item.systemStock}</td>
                    <td className="px-3 py-2 text-right">
                      {activeSession.status === 'InProgress' ? (
                        <input
                          type="number"
                          value={counts[item.id] ?? ''}
                          onChange={e => setCounts(c => ({ ...c, [item.id]: e.target.value }))}
                          onBlur={() => saveCount(item.id)}
                          placeholder="Enter count"
                          className="w-24 text-right border border-slate-200 rounded-lg px-2 py-1 focus:ring-1 focus:ring-emerald-400 outline-none"
                        />
                      ) : (
                        <span className="text-slate-600">{item.countedStock ?? '-'}</span>
                      )}
                    </td>
                    <td className={`px-3 py-2 text-right font-semibold ${diff !== undefined && diff !== null ? diff > 0 ? 'text-emerald-600' : diff < 0 ? 'text-rose-600' : 'text-slate-400' : 'text-slate-300'}`}>
                      {diff !== undefined && diff !== null ? (diff > 0 ? `+${diff}` : diff) : '-'}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    );
  }

  return (
    <div className="p-4 md:p-6 space-y-4">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-emerald-100 rounded-xl flex items-center justify-center"><ClipboardList className="w-5 h-5 text-emerald-600" /></div>
          <div><h1 className="text-lg font-bold text-slate-800">Stock Take</h1><p className="text-xs text-slate-500">Physical count &amp; reconciliation</p></div>
        </div>
        <button onClick={() => setShowCreate(true)} className="flex items-center gap-1.5 px-4 py-2 bg-emerald-600 text-white text-xs font-semibold rounded-xl hover:bg-emerald-700">
          <Plus className="w-4 h-4" /> New Session
        </button>
      </div>
      {loading ? <div className="p-8 text-center text-sm text-slate-400">Loading...</div> : (
        <div className="grid gap-3">
          {sessions.length === 0 ? <div className="bg-white border border-green-100 rounded-xl p-10 text-center text-sm text-slate-400">No stock take sessions yet</div> : sessions.map(s => (
            <button key={s.id} onClick={() => openSession(s.id)} className="bg-white border border-green-100 rounded-xl p-4 flex items-center justify-between hover:shadow-md transition-shadow text-left w-full">
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 bg-emerald-50 rounded-xl flex items-center justify-center"><ClipboardList className="w-4 h-4 text-emerald-600" /></div>
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-semibold text-slate-800">{s.name}</p>
                    <span className={`text-[10px] px-1.5 py-0.5 rounded font-semibold ${STATUS_COLORS[s.status]}`}>{s.status}</span>
                  </div>
                  <p className="text-xs text-slate-500">{s.sessionNumber} · {new Date(s.createdAt).toLocaleDateString('en-IN')}</p>
                </div>
              </div>
              <div className="text-right">
                <p className="text-xs font-semibold text-slate-700">{s.countedProducts}/{s.totalProducts}</p>
                <p className="text-xs text-slate-400">products counted</p>
                {s.discrepancyCount > 0 && <p className="text-xs text-amber-600">{s.discrepancyCount} discrepancies</p>}
              </div>
            </button>
          ))}
        </div>
      )}
      {showCreate && (
        <div className="fixed inset-0 z-50 bg-black/30 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-sm">
            <div className="p-5 border-b border-green-100"><h2 className="text-base font-bold text-slate-800">New Stock Take Session</h2></div>
            <div className="p-5 space-y-3">
              <div>
                <label className="text-xs font-medium text-slate-600 mb-1 block">Session Name *</label>
                <input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} placeholder="e.g. Oct 2024 Monthly Count" className="w-full text-xs border border-slate-200 rounded-lg px-3 py-2 focus:ring-1 focus:ring-emerald-400 outline-none" />
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600 mb-1 block">Notes</label>
                <textarea value={form.notes} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} rows={2} className="w-full text-xs border border-slate-200 rounded-lg px-3 py-2 focus:ring-1 focus:ring-emerald-400 outline-none resize-none" />
              </div>
              <p className="text-xs text-slate-400">All active products will be added to this session for counting.</p>
            </div>
            <div className="p-4 border-t border-green-100 flex gap-2 justify-end">
              <button onClick={() => setShowCreate(false)} className="px-4 py-2 text-xs font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50">Cancel</button>
              <button onClick={createSession} disabled={saving || !form.name} className="px-4 py-2 text-xs font-semibold bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 disabled:opacity-50">{saving ? 'Creating...' : 'Create Session'}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

