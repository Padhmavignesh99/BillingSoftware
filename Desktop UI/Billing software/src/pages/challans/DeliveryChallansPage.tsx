import React, { useState, useEffect, useCallback } from 'react';
import { Truck, Plus, Search, Eye, Edit, Trash2, FileText, CheckCircle } from 'lucide-react';
import { api } from '../../services/apiClient';

interface ChallanItem { id: number; productId: number; productName: string; quantity: number; unit?: string; notes?: string; }
interface Challan {
  id: number; challanNumber: string; challanDate: string; customerId?: number; customerName?: string;
  invoiceId?: number; invoiceNumber?: string; status: string; deliveryAddress?: string;
  transportMode?: string; vehicleNumber?: string; driverName?: string; lrNumber?: string;
  notes?: string; dispatchedAt?: string; deliveredAt?: string; createdAt: string; items: ChallanItem[];
}
interface Customer { id: number; name: string; phone?: string; }
interface Product { id: number; name: string; sku?: string; }

const STATUS_COLORS: Record<string, string> = {
  Draft: 'bg-slate-100 text-slate-600',
  Dispatched: 'bg-blue-100 text-blue-700',
  Delivered: 'bg-emerald-100 text-emerald-700',
  Cancelled: 'bg-rose-100 text-rose-700',
};

export const DeliveryChallansPage: React.FC = () => {
  const [challans, setChallans] = useState<Challan[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [viewChallan, setViewChallan] = useState<Challan | null>(null);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({
    customerId: '', deliveryAddress: '', transportMode: '', vehicleNumber: '',
    driverName: '', lrNumber: '', notes: '', items: [{ productId: '', productName: '', quantity: 1, unit: 'Pcs', notes: '' }]
  });

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.request<Challan[]>('/api/delivery-challans?page=1&pageSize=50');
      setChallans(Array.isArray(res) ? res : []);
    } catch { setChallans([]); }
    setLoading(false);
  }, []);

  useEffect(() => { load(); }, [load]);
  useEffect(() => {
    api.request<Customer[]>('/api/customers?page=1&pageSize=200').then(r => setCustomers(Array.isArray(r) ? r : [])).catch(() => {});
    api.request<Product[]>('/api/products?page=1&pageSize=500').then(r => setProducts(Array.isArray(r) ? r : [])).catch(() => {});
  }, []);

  const addItem = () => setForm(f => ({ ...f, items: [...f.items, { productId: '', productName: '', quantity: 1, unit: 'Pcs', notes: '' }] }));
  const removeItem = (i: number) => setForm(f => ({ ...f, items: f.items.filter((_, idx) => idx !== i) }));

  const submit = async () => {
    setSaving(true);
    try {
      await api.request('/api/delivery-challans', {
        method: 'POST',
        body: JSON.stringify({
          customerId: form.customerId ? Number(form.customerId) : undefined,
          deliveryAddress: form.deliveryAddress,
          transportMode: form.transportMode,
          vehicleNumber: form.vehicleNumber,
          driverName: form.driverName,
          lrNumber: form.lrNumber,
          notes: form.notes,
          items: form.items.filter(i => i.productId).map(i => ({
            productId: Number(i.productId),
            productName: products.find(p => p.id === Number(i.productId))?.name || i.productName,
            quantity: Number(i.quantity),
            unit: i.unit,
            notes: i.notes
          }))
        })
      });
      setShowModal(false);
      load();
    } catch { }
    setSaving(false);
  };

  const updateStatus = async (id: number, status: string) => {
    await api.request(`/api/delivery-challans/${id}/status`, { method: 'PUT', body: JSON.stringify({ status }) });
    load();
  };

  const deleteChallan = async (id: number) => {
    if (!confirm('Delete this challan?')) return;
    await api.request(`/api/delivery-challans/${id}`, { method: 'DELETE' });
    load();
  };

  const convertToInvoice = async (id: number) => {
    try {
      await api.request(`/api/delivery-challans/${id}/convert-to-invoice`, { method: 'POST' });
      alert('Converted to invoice successfully!');
      load();
    } catch (e: any) { alert(e.message); }
  };

  const filtered = challans.filter(c =>
    c.challanNumber.toLowerCase().includes(search.toLowerCase()) ||
    (c.customerName || '').toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="p-4 md:p-6 space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-emerald-100 rounded-xl flex items-center justify-center">
            <Truck className="w-5 h-5 text-emerald-600" />
          </div>
          <div>
            <h1 className="text-lg font-bold text-slate-800">Delivery Challans</h1>
            <p className="text-xs text-slate-500">{challans.length} challans</p>
          </div>
        </div>
        <button onClick={() => { setForm({ customerId: '', deliveryAddress: '', transportMode: '', vehicleNumber: '', driverName: '', lrNumber: '', notes: '', items: [{ productId: '', productName: '', quantity: 1, unit: 'Pcs', notes: '' }] }); setShowModal(true); }} className="flex items-center gap-1.5 px-4 py-2 bg-emerald-600 text-white text-xs font-semibold rounded-xl hover:bg-emerald-700 transition-colors">
          <Plus className="w-4 h-4" /> New Challan
        </button>
      </div>

      {/* Search */}
      <div className="relative">
        <Search className="w-3.5 h-3.5 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Search challan number or customer..." className="pl-8 pr-4 py-2 text-xs border border-slate-200 rounded-xl w-full max-w-md focus:ring-1 focus:ring-emerald-400 outline-none" />
      </div>

      {/* Table */}
      <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
        {loading ? <div className="p-8 text-center text-sm text-slate-400">Loading...</div> : (
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="bg-slate-50 border-b border-slate-100">
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Challan #</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Date</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Customer</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Items</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Status</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.length === 0 ? (
                  <tr><td colSpan={6} className="px-3 py-8 text-center text-slate-400">No challans found</td></tr>
                ) : filtered.map(c => (
                  <tr key={c.id} className="border-b border-slate-50 hover:bg-slate-50">
                    <td className="px-3 py-2.5 font-semibold text-emerald-700">{c.challanNumber}</td>
                    <td className="px-3 py-2.5 text-slate-500">{new Date(c.challanDate).toLocaleDateString('en-IN')}</td>
                    <td className="px-3 py-2.5 text-slate-700">{c.customerName || '-'}</td>
                    <td className="px-3 py-2.5 text-slate-500">{c.items?.length || 0} items</td>
                    <td className="px-3 py-2.5">
                      <span className={`px-2 py-0.5 rounded-md font-medium text-[11px] ${STATUS_COLORS[c.status] || 'bg-slate-100 text-slate-600'}`}>{c.status}</span>
                    </td>
                    <td className="px-3 py-2.5">
                      <div className="flex items-center gap-1.5">
                        <button onClick={() => setViewChallan(c)} className="p-1 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors" title="View"><Eye className="w-3.5 h-3.5" /></button>
                        {c.status === 'Draft' && <button onClick={() => updateStatus(c.id, 'Dispatched')} className="p-1 text-slate-400 hover:text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors" title="Mark Dispatched"><Truck className="w-3.5 h-3.5" /></button>}
                        {c.status === 'Dispatched' && <button onClick={() => updateStatus(c.id, 'Delivered')} className="p-1 text-slate-400 hover:text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors" title="Mark Delivered"><CheckCircle className="w-3.5 h-3.5" /></button>}
                        {c.status !== 'Delivered' && <button onClick={() => convertToInvoice(c.id)} className="p-1 text-slate-400 hover:text-violet-600 hover:bg-violet-50 rounded-lg transition-colors" title="Convert to Invoice"><FileText className="w-3.5 h-3.5" /></button>}
                        <button onClick={() => deleteChallan(c.id)} className="p-1 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors" title="Delete"><Trash2 className="w-3.5 h-3.5" /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Create Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 bg-black/30 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <div className="p-5 border-b border-green-100">
              <h2 className="text-base font-bold text-slate-800">New Delivery Challan</h2>
            </div>
            <div className="p-5 space-y-3">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">Customer</label>
                  <select value={form.customerId} onChange={e => setForm(f => ({ ...f, customerId: e.target.value }))} className="w-full text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none">
                    <option value="">Select customer</option>
                    {customers.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                  </select>
                </div>
                <div>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">Transport Mode</label>
                  <input value={form.transportMode} onChange={e => setForm(f => ({ ...f, transportMode: e.target.value }))} placeholder="Road, Rail..." className="w-full text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" />
                </div>
                <div>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">Vehicle Number</label>
                  <input value={form.vehicleNumber} onChange={e => setForm(f => ({ ...f, vehicleNumber: e.target.value }))} placeholder="TN 01 AB 1234" className="w-full text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" />
                </div>
                <div>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">Driver Name</label>
                  <input value={form.driverName} onChange={e => setForm(f => ({ ...f, driverName: e.target.value }))} className="w-full text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" />
                </div>
                <div>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">LR Number</label>
                  <input value={form.lrNumber} onChange={e => setForm(f => ({ ...f, lrNumber: e.target.value }))} className="w-full text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" />
                </div>
                <div>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">Delivery Address</label>
                  <input value={form.deliveryAddress} onChange={e => setForm(f => ({ ...f, deliveryAddress: e.target.value }))} className="w-full text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" />
                </div>
              </div>
              {/* Items */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-xs font-semibold text-slate-700">Items</label>
                  <button onClick={addItem} className="text-xs text-emerald-600 hover:text-emerald-700 font-medium">+ Add Item</button>
                </div>
                {form.items.map((item, i) => (
                  <div key={i} className="flex gap-2 mb-2 items-center">
                    <select value={item.productId} onChange={e => { const p = products.find(x => x.id === Number(e.target.value)); setForm(f => ({ ...f, items: f.items.map((it, idx) => idx === i ? { ...it, productId: e.target.value, productName: p?.name || '' } : it) })); }} className="flex-1 text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none">
                      <option value="">Select product</option>
                      {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                    </select>
                    <input type="number" value={item.quantity} onChange={e => setForm(f => ({ ...f, items: f.items.map((it, idx) => idx === i ? { ...it, quantity: Number(e.target.value) } : it) }))} min={1} className="w-20 text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" />
                    <input value={item.unit} onChange={e => setForm(f => ({ ...f, items: f.items.map((it, idx) => idx === i ? { ...it, unit: e.target.value } : it) }))} placeholder="Unit" className="w-16 text-xs border border-slate-200 rounded-lg px-2 py-1.5 focus:ring-1 focus:ring-emerald-400 outline-none" />
                    {form.items.length > 1 && <button onClick={() => removeItem(i)} className="p-1 text-rose-400 hover:text-rose-600"><Trash2 className="w-3.5 h-3.5" /></button>}
                  </div>
                ))}
              </div>
            </div>
            <div className="p-4 border-t border-green-100 flex gap-2 justify-end">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 text-xs font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50">Cancel</button>
              <button onClick={submit} disabled={saving} className="px-4 py-2 text-xs font-semibold bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 disabled:opacity-50">{saving ? 'Saving...' : 'Create Challan'}</button>
            </div>
          </div>
        </div>
      )}

      {/* View Modal */}
      {viewChallan && (
        <div className="fixed inset-0 z-50 bg-black/30 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
            <div className="p-5 border-b border-green-100 flex items-center justify-between">
              <h2 className="text-base font-bold text-slate-800">{viewChallan.challanNumber}</h2>
              <span className={`px-2 py-0.5 rounded-md font-medium text-[11px] ${STATUS_COLORS[viewChallan.status]}`}>{viewChallan.status}</span>
            </div>
            <div className="p-5 space-y-3 text-xs">
              {viewChallan.customerName && <div className="flex gap-2"><span className="text-slate-500 w-28">Customer:</span><span className="font-medium text-slate-800">{viewChallan.customerName}</span></div>}
              {viewChallan.vehicleNumber && <div className="flex gap-2"><span className="text-slate-500 w-28">Vehicle:</span><span>{viewChallan.vehicleNumber}</span></div>}
              {viewChallan.driverName && <div className="flex gap-2"><span className="text-slate-500 w-28">Driver:</span><span>{viewChallan.driverName}</span></div>}
              {viewChallan.deliveryAddress && <div className="flex gap-2"><span className="text-slate-500 w-28">Delivery To:</span><span>{viewChallan.deliveryAddress}</span></div>}
              <div className="border-t border-slate-100 pt-3">
                <p className="font-semibold text-slate-700 mb-2">Items ({viewChallan.items.length})</p>
                {viewChallan.items.map(item => (
                  <div key={item.id} className="flex justify-between py-1 border-b border-slate-50">
                    <span className="text-slate-700">{item.productName}</span>
                    <span className="text-slate-500">{item.quantity} {item.unit}</span>
                  </div>
                ))}
              </div>
            </div>
            <div className="p-4 border-t border-green-100 flex justify-end">
              <button onClick={() => setViewChallan(null)} className="px-4 py-2 text-xs font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50">Close</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

