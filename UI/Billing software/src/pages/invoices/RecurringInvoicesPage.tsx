import React, { useState, useEffect, useCallback } from 'react';
import { RefreshCw, Plus, Play, Pause, Trash2, Edit, Zap } from 'lucide-react';
import { api } from '../../services/apiClient';

interface RecurringInvoice {
  id: number; name: string; customerId?: number; customerName?: string;
  frequency: string; frequencyValue: number; startDate: string; endDate?: string;
  nextRunDate?: string; lastRunDate?: string; runCount: number; maxRuns?: number;
  status: string; totalAmount: number; createdAt: string;
}
interface Customer { id: number; name: string; }
interface Product { id: number; name: string; sellingPrice: number; }

const STATUS_COLORS: Record<string, string> = {
  Active: 'bg-emerald-100 text-emerald-700',
  Paused: 'bg-amber-100 text-amber-700',
  Completed: 'bg-blue-100 text-blue-700',
  Cancelled: 'bg-rose-100 text-rose-700',
};

const FREQ_OPTIONS = ['Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly'];

export const RecurringInvoicesPage: React.FC = () => {
  const [items, setItems] = useState<RecurringInvoice[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [saving, setSaving] = useState(false);
  const [generating, setGenerating] = useState<number | null>(null);
  const [form, setForm] = useState({
    name: '', customerId: '', frequency: 'Monthly', frequencyValue: 1,
    startDate: new Date().toISOString().slice(0, 10), endDate: '', maxRuns: '',
    notes: '', items: [{ productId: '', quantity: 1, unitPrice: 0, totalPrice: 0 }]
  });

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.request<RecurringInvoice[]>('/api/recurring-invoices?page=1&pageSize=50');
      setItems(Array.isArray(res) ? res : []);
    } catch { setItems([]); }
    setLoading(false);
  }, []);

  useEffect(() => { load(); }, [load]);
  useEffect(() => {
    api.request<Customer[]>('/api/customers?page=1&pageSize=200').then(r => setCustomers(Array.isArray(r) ? r : [])).catch(() => {});
    api.request<Product[]>('/api/products?page=1&pageSize=500').then(r => setProducts(Array.isArray(r) ? r : [])).catch(() => {});
  }, []);

  const submit = async () => {
    setSaving(true);
    try {
      await api.request('/api/recurring-invoices', {
        method: 'POST',
        body: JSON.stringify({
          name: form.name, customerId: form.customerId ? Number(form.customerId) : undefined,
          frequency: form.frequency, frequencyValue: Number(form.frequencyValue),
          startDate: form.startDate, endDate: form.endDate || undefined,
          maxRuns: form.maxRuns ? Number(form.maxRuns) : undefined, notes: form.notes,
          items: form.items.filter(i => i.productId).map(i => {
            const p = products.find(x => x.id === Number(i.productId));
            return { productId: Number(i.productId), productName: p?.name || '', quantity: Number(i.quantity), unitPrice: Number(i.unitPrice), discountAmount: 0, taxAmount: 0, totalPrice: Number(i.unitPrice) * Number(i.quantity) };
          })
        })
      });
      setShowModal(false);
      load();
    } catch { }
    setSaving(false);
  };

  const toggleStatus = async (id: number) => {
    await api.request(`/api/recurring-invoices/${id}/toggle-status`, { method: 'POST' });
    load();
  };

  const generateNow = async (id: number) => {
    setGenerating(id);
    try {
      await api.request(`/api/recurring-invoices/${id}/generate-now`, { method: 'POST' });
      alert('Invoice generated successfully!');
      load();
    } catch (e: any) { alert(e.message); }
    setGenerating(null);
  };

  const deleteItem = async (id: number) => {
    if (!confirm('Delete this recurring invoice template?')) return;
    await api.request(`/api/recurring-invoices/${id}`, { method: 'DELETE' });
    load();
  };

  return (
    <div className="p-4 md:p-6 space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-violet-100 rounded-xl flex items-center justify-center">
            <RefreshCw className="w-5 h-5 text-violet-600" />
          </div>
          <div>
            <h1 className="text-lg font-bold text-slate-800">Recurring Invoices</h1>
            <p className="text-xs text-slate-500">{items.length} templates</p>
          </div>
        </div>
        <button onClick={() => { setForm({ name: '', customerId: '', frequency: 'Monthly', frequencyValue: 1, startDate: new Date().toISOString().slice(0, 10), endDate: '', maxRuns: '', notes: '', items: [{ productId: '', quantity: 1, unitPrice: 0, totalPrice: 0 }] }); setShowModal(true); }} className="flex items-center gap-1.5 px-4 py-2 bg-emerald-600 text-white text-xs font-semibold rounded-xl hover:bg-emerald-700">
          <Plus className="w-4 h-4" /> New Template
        </button>
      </div>

      {/* Cards */}
      {loading ? <div className="p-8 text-center text-sm text-slate-400">Loading...</div> : (
        <div className="grid gap-3">
          {items.length === 0 ? (
            <div className="bg-white border border-green-100 rounded-xl p-10 text-center text-sm text-slate-400">No recurring invoices set up yet</div>
          ) : items.map(item => (
            <div key={item.id} className="bg-white border border-green-100 rounded-xl p-4 flex items-center justify-between flex-wrap gap-3">
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 bg-violet-50 rounded-xl flex items-center justify-center">
                  <RefreshCw className="w-4 h-4 text-violet-600" />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-semibold text-slate-800">{item.name}</p>
                    <span className={`px-1.5 py-0.5 text-[10px] font-semibold rounded ${STATUS_COLORS[item.status]}`}>{item.status}</span>
                  </div>
                  <p className="text-xs text-slate-500">{item.customerName || 'No customer'} · Every {item.frequencyValue} {item.frequency.toLowerCase()}</p>
                  <div className="flex gap-3 mt-0.5">
                    <span className="text-xs text-slate-400">Runs: {item.runCount}{item.maxRuns ? `/${item.maxRuns}` : ''}</span>
                    {item.nextRunDate && <span className="text-xs text-slate-400">Next: {new Date(item.nextRunDate).toLocaleDateString('en-IN')}</span>}
                    <span className="text-xs font-semibold text-emerald-700">₹{item.totalAmount.toLocaleString('en-IN')}</span>
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-1.5">
                <button onClick={() => generateNow(item.id)} disabled={generating === item.id} className="flex items-center gap-1 px-2.5 py-1.5 text-xs font-medium text-emerald-600 border border-emerald-200 rounded-lg hover:bg-emerald-50 disabled:opacity-50">
                  <Zap className="w-3.5 h-3.5" /> {generating === item.id ? 'Generating...' : 'Run Now'}
                </button>
                <button onClick={() => toggleStatus(item.id)} className="p-1.5 text-slate-400 hover:text-amber-600 hover:bg-amber-50 rounded-lg transition-colors" title={item.status === 'Active' ? 'Pause' : 'Resume'}>
                  {item.status === 'Active' ? <Pause className="w-3.5 h-3.5" /> : <Play className="w-3.5 h-3.5" />}
                </button>
                <button onClick={() => deleteItem(item.id)} className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors">
                  <Trash2 className="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 bg-black/30 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-xl max-h-[90vh] overflow-y-auto">
            <div className="p-5 border-b border-green-100"><h2 className="text-base font-bold text-slate-800">New Recurring Invoice</h2></div>
            <div className="p-5 space-y-3">
              <div>
                <label className="text-xs font-medium text-slate-600 mb-1 block">Template Name *</label>
                <input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} placeholder="e.g. Monthly Rent Invoice" className="w-full text-xs border border-slate-200 rounded-lg px-3 py-2 focus:ring-1 focus:ring-emerald-400 outline-none" />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">Customer</label>
                  <select value={form.customerId} onChange={e => setForm(f => ({ ...f, customerId: e.target.value }))} className="w-full text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none">
                    <option value="">Select customer</option>
                    {customers.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                  </select>
                </div>
                <div>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">Frequency</label>
                  <select value={form.frequency} onChange={e => setForm(f => ({ ...f, frequency: e.target.value }))} className="w-full text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none">
                    {FREQ_OPTIONS.map(f => <option key={f} value={f}>{f}</option>)}
                  </select>
                </div>
                <div>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">Start Date</label>
                  <input type="date" value={form.startDate} onChange={e => setForm(f => ({ ...f, startDate: e.target.value }))} className="w-full text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" />
                </div>
                <div>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">Max Runs (optional)</label>
                  <input type="number" value={form.maxRuns} onChange={e => setForm(f => ({ ...f, maxRuns: e.target.value }))} placeholder="Unlimited" className="w-full text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" />
                </div>
              </div>
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-xs font-semibold text-slate-700">Line Items</label>
                  <button onClick={() => setForm(f => ({ ...f, items: [...f.items, { productId: '', quantity: 1, unitPrice: 0, totalPrice: 0 }] }))} className="text-xs text-emerald-600 font-medium">+ Add</button>
                </div>
                {form.items.map((item, i) => (
                  <div key={i} className="flex gap-2 mb-2">
                    <select value={item.productId} onChange={e => { const p = products.find(x => x.id === Number(e.target.value)); setForm(f => ({ ...f, items: f.items.map((it, idx) => idx === i ? { ...it, productId: e.target.value, unitPrice: p?.sellingPrice || 0, totalPrice: (p?.sellingPrice || 0) * it.quantity } : it) })); }} className="flex-1 text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none">
                      <option value="">Product</option>
                      {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                    </select>
                    <input type="number" value={item.quantity} onChange={e => setForm(f => ({ ...f, items: f.items.map((it, idx) => idx === i ? { ...it, quantity: Number(e.target.value), totalPrice: it.unitPrice * Number(e.target.value) } : it) }))} min={1} className="w-16 text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" placeholder="Qty" />
                    <input type="number" value={item.unitPrice} onChange={e => setForm(f => ({ ...f, items: f.items.map((it, idx) => idx === i ? { ...it, unitPrice: Number(e.target.value), totalPrice: Number(e.target.value) * it.quantity } : it) }))} className="w-24 text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" placeholder="Price" />
                    {form.items.length > 1 && <button onClick={() => setForm(f => ({ ...f, items: f.items.filter((_, idx) => idx !== i) }))} className="p-1 text-rose-400"><Trash2 className="w-3.5 h-3.5" /></button>}
                  </div>
                ))}
              </div>
            </div>
            <div className="p-4 border-t border-green-100 flex gap-2 justify-end">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 text-xs font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50">Cancel</button>
              <button onClick={submit} disabled={saving || !form.name} className="px-4 py-2 text-xs font-semibold bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 disabled:opacity-50">{saving ? 'Saving...' : 'Create Template'}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

