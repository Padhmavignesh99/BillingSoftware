import React, { useState, useEffect, useCallback } from 'react';
import { Zap, ShoppingCart, AlertTriangle, TrendingDown, Eye, Package } from 'lucide-react';
import { api } from '../../services/apiClient';

interface ReorderSuggestion {
  productId: number; productName: string; sku?: string; currentStock: number;
  reorderLevel: number; avgDailySales: number; daysOfStockLeft: number;
  suggestedOrderQty: number; preferredSupplierId?: number; supplierName?: string;
  lastPurchasePrice?: number; estimatedCost: number; urgency: string;
}

const URGENCY_STYLES: Record<string, { badge: string; row: string }> = {
  Critical: { badge: 'bg-rose-100 text-rose-700 border-rose-200', row: 'bg-rose-50/40' },
  Urgent:   { badge: 'bg-amber-100 text-amber-700 border-amber-200', row: 'bg-amber-50/30' },
  Watch:    { badge: 'bg-blue-100 text-blue-700 border-blue-200', row: '' },
  Normal:   { badge: 'bg-slate-100 text-slate-600 border-slate-200', row: '' },
};

export const ReorderPage: React.FC = () => {
  const [suggestions, setSuggestions] = useState<ReorderSuggestion[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<'All' | 'Critical' | 'Urgent' | 'Watch'>('All');
  const [selected, setSelected] = useState<Set<number>>(new Set());
  const [creating, setCreating] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.request<ReorderSuggestion[]>('/api/reorder/suggestions');
      setSuggestions(Array.isArray(res) ? res : []);
    } catch { setSuggestions([]); }
    setLoading(false);
  }, []);

  useEffect(() => { load(); }, [load]);

  const critical = suggestions.filter(s => s.urgency === 'Critical').length;
  const urgent = suggestions.filter(s => s.urgency === 'Urgent').length;
  const watch = suggestions.filter(s => s.urgency === 'Watch').length;

  const filtered = filter === 'All' ? suggestions : suggestions.filter(s => s.urgency === filter);

  const toggleSelect = (id: number) => {
    setSelected(s => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });
  };

  const selectAll = () => setSelected(new Set(filtered.map(s => s.productId)));
  const clearAll = () => setSelected(new Set());

  const createPO = async () => {
    if (selected.size === 0) { alert('Select at least one product'); return; }
    setCreating(true);
    try {
      const result = await api.request<any>('/api/reorder/create-purchase-order', {
        method: 'POST',
        body: JSON.stringify([...selected])
      });
      alert(`Created ${result.count} purchase order(s): ${result.createdPOs?.join(', ')}`);
      setSelected(new Set());
      load();
    } catch (e: any) { alert(e.message); }
    setCreating(false);
  };

  return (
    <div className="p-4 md:p-6 space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-amber-100 rounded-xl flex items-center justify-center"><Zap className="w-5 h-5 text-amber-600" /></div>
          <div><h1 className="text-lg font-bold text-slate-800">Reorder Alerts</h1><p className="text-xs text-slate-500">Smart restocking suggestions</p></div>
        </div>
        {selected.size > 0 && (
          <button onClick={createPO} disabled={creating} className="flex items-center gap-1.5 px-4 py-2 bg-emerald-600 text-white text-xs font-semibold rounded-xl hover:bg-emerald-700 disabled:opacity-50">
            <ShoppingCart className="w-4 h-4" /> {creating ? 'Creating PO...' : `Create PO (${selected.size} products)`}
          </button>
        )}
      </div>

      {/* Stats */}
      <div className="grid grid-cols-3 gap-3">
        {[{ label: 'Critical', count: critical, icon: AlertTriangle, color: 'text-rose-600', bg: 'bg-rose-50 border-rose-100' },
          { label: 'Urgent', count: urgent, icon: TrendingDown, color: 'text-amber-600', bg: 'bg-amber-50 border-amber-100' },
          { label: 'Watch', count: watch, icon: Eye, color: 'text-blue-600', bg: 'bg-blue-50 border-blue-100' }].map(stat => (
          <div key={stat.label} className={`border rounded-xl p-3 flex items-center gap-3 ${stat.bg}`}>
            <stat.icon className={`w-5 h-5 ${stat.color}`} />
            <div><p className={`text-lg font-bold ${stat.color}`}>{stat.count}</p><p className="text-xs text-slate-500">{stat.label}</p></div>
          </div>
        ))}
      </div>

      {/* Filter tabs */}
      <div className="flex gap-1 p-1 bg-slate-100 rounded-xl w-fit">
        {(['All', 'Critical', 'Urgent', 'Watch'] as const).map(f => (
          <button key={f} onClick={() => setFilter(f)} className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-all ${filter === f ? 'bg-white text-emerald-700 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}>{f}</button>
        ))}
      </div>

      {/* Table */}
      <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
        {loading ? <div className="p-8 text-center text-sm text-slate-400">Analyzing inventory...</div> : filtered.length === 0 ? (
          <div className="p-10 text-center">
            <Package className="w-10 h-10 text-slate-200 mx-auto mb-3" />
            <p className="text-sm text-slate-400">No reorder suggestions — stock levels look good!</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="bg-slate-50 border-b border-slate-100">
                  <th className="px-3 py-2.5 w-8">
                    <input type="checkbox" checked={selected.size === filtered.length && filtered.length > 0} onChange={() => selected.size === filtered.length ? clearAll() : selectAll()} className="rounded border-slate-300 text-emerald-600 focus:ring-emerald-400" />
                  </th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Product</th>
                  <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Current</th>
                  <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Daily Avg</th>
                  <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Days Left</th>
                  <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Suggest Qty</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Supplier</th>
                  <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Est. Cost</th>
                  <th className="text-center px-3 py-2.5 font-semibold text-slate-600">Urgency</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map(s => {
                  const style = URGENCY_STYLES[s.urgency] ?? URGENCY_STYLES.Normal;
                  return (
                    <tr key={s.productId} className={`border-b border-slate-50 ${style.row}`}>
                      <td className="px-3 py-2.5">
                        <input type="checkbox" checked={selected.has(s.productId)} onChange={() => toggleSelect(s.productId)} className="rounded border-slate-300 text-emerald-600 focus:ring-emerald-400" />
                      </td>
                      <td className="px-3 py-2.5">
                        <p className="font-semibold text-slate-800">{s.productName}</p>
                        {s.sku && <p className="text-slate-400">{s.sku}</p>}
                      </td>
                      <td className="px-3 py-2.5 text-right font-semibold text-slate-700">{s.currentStock}</td>
                      <td className="px-3 py-2.5 text-right text-slate-500">{s.avgDailySales.toFixed(1)}</td>
                      <td className={`px-3 py-2.5 text-right font-semibold ${s.daysOfStockLeft <= 3 ? 'text-rose-600' : s.daysOfStockLeft <= 7 ? 'text-amber-600' : 'text-slate-700'}`}>{s.daysOfStockLeft >= 999 ? '∞' : s.daysOfStockLeft.toFixed(0)}</td>
                      <td className="px-3 py-2.5 text-right font-semibold text-emerald-700">{s.suggestedOrderQty}</td>
                      <td className="px-3 py-2.5 text-slate-600">{s.supplierName || '-'}</td>
                      <td className="px-3 py-2.5 text-right font-semibold text-slate-700">₹{s.estimatedCost.toLocaleString('en-IN')}</td>
                      <td className="px-3 py-2.5 text-center">
                        <span className={`px-2 py-0.5 text-[10px] font-semibold rounded-md border ${style.badge}`}>{s.urgency}</span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

