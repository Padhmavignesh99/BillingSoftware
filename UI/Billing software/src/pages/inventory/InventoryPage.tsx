import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Boxes,
  PackageCheck,
  AlertTriangle,
  PackageX,
  History,
  Plus,
  ArrowUpRight,
  ArrowDownLeft,
  RefreshCw,
  Search,
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/common/Toast';

export const InventoryPage: React.FC = () => {
  const navigate = useNavigate();
  const { products, inventoryMovements, adjustStock } = useStore();
  const { showToast } = useToast();

  const [searchQuery, setSearchQuery] = useState('');
  const [showAdjModal, setShowAdjModal] = useState(false);
  const [selectedProdId, setSelectedProdId] = useState(products[0]?.id || '');
  const [adjType, setAdjType] = useState<'in' | 'out'>('in');
  const [qty, setQty] = useState(10);
  const [reason, setReason] = useState('Stock Audit Count Adjustment');

  const totalProductsCount = products.length;
  const inStockCount = products.filter((p) => p.stock > p.minStock).length;
  const lowStockCount = products.filter((p) => p.stock > 0 && p.stock <= p.minStock).length;
  const outOfStockCount = products.filter((p) => p.stock === 0).length;

  const filteredMovements = inventoryMovements.filter((m) => {
    const q = searchQuery.toLowerCase().trim();
    return (
      !q ||
      m.productName.toLowerCase().includes(q) ||
      m.sku.toLowerCase().includes(q) ||
      m.referenceNumber.toLowerCase().includes(q)
    );
  });

  const handleExecuteAdj = async (e: React.FormEvent) => {
    e.preventDefault();
    const prod = products.find((p) => p.id === selectedProdId);
    if (!prod) return;

    const delta = adjType === 'in' ? Math.abs(qty) : -Math.abs(qty);
    try {
      await adjustStock(prod.id, delta, reason);
      setShowAdjModal(false);
      showToast(`Adjusted ${prod.name} stock by ${delta > 0 ? `+${delta}` : delta}`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not adjust stock', 'error');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Stock & Inventory Management</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            Monitor real-time inventory counts, stock movement audit trail, and adjustments
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => setShowAdjModal(true)}
            className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-md shadow-indigo-600/30 transition-all"
          >
            <Plus className="w-4 h-4" />
            <span>Stock Adjustment</span>
          </button>
        </div>
      </div>

      {/* 4 Stock Metric Cards Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Total SKUs</span>
            <div className="w-8 h-8 rounded-xl bg-indigo-50 text-indigo-600 flex items-center justify-center">
              <Boxes className="w-4 h-4" />
            </div>
          </div>
          <h3 className="text-2xl font-bold text-slate-900">{totalProductsCount}</h3>
          <p className="text-[11px] text-slate-400">Active catalog items</p>
        </div>

        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">In Stock</span>
            <div className="w-8 h-8 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
              <PackageCheck className="w-4 h-4" />
            </div>
          </div>
          <h3 className="text-2xl font-bold text-slate-900">{inStockCount}</h3>
          <p className="text-[11px] text-emerald-600 font-semibold">Healthy inventory levels</p>
        </div>

        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Low Stock Warning</span>
            <div className="w-8 h-8 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
              <AlertTriangle className="w-4 h-4" />
            </div>
          </div>
          <h3 className="text-2xl font-bold text-slate-900">{lowStockCount}</h3>
          <p className="text-[11px] text-amber-600 font-semibold">Reorder recommended</p>
        </div>

        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Out of Stock</span>
            <div className="w-8 h-8 rounded-xl bg-rose-50 text-rose-600 flex items-center justify-center">
              <PackageX className="w-4 h-4" />
            </div>
          </div>
          <h3 className="text-2xl font-bold text-slate-900">{outOfStockCount}</h3>
          <p className="text-[11px] text-rose-600 font-semibold">Zero stock remaining</p>
        </div>
      </div>

      {/* Stock Movement History Table */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden space-y-4 p-5">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-slate-100 pb-3">
          <div>
            <h2 className="text-base font-bold text-slate-900 flex items-center gap-2">
              <History className="w-4 h-4 text-indigo-600" />
              <span>Stock Movement Audit Trail</span>
            </h2>
            <p className="text-xs text-slate-500">Live track of sales deductions, purchase stock arrivals & manual adjustments</p>
          </div>

          <div className="relative min-w-[220px]">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search movement history..."
              className="w-full pl-9 pr-3 py-1.5 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50/80 text-slate-500 font-semibold border-b border-slate-100">
              <tr>
                <th className="py-3 px-3">Date & Time</th>
                <th className="py-3 px-3">Product / SKU</th>
                <th className="py-3 px-3">Type</th>
                <th className="py-3 px-3">Delta Qty</th>
                <th className="py-3 px-3">Stock Shift</th>
                <th className="py-3 px-3">Ref Number</th>
                <th className="py-3 px-3">Reason / User</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 font-medium">
              {filteredMovements.map((mov) => (
                <tr key={mov.id} className="hover:bg-slate-50/60 transition-colors">
                  <td className="py-3 px-3 text-slate-500 text-[11px] whitespace-nowrap">{mov.date}</td>
                  <td className="py-3 px-3">
                    <p className="font-bold text-slate-900">{mov.productName}</p>
                    <p className="text-[10px] text-slate-400 font-mono">{mov.sku}</p>
                  </td>
                  <td className="py-3 px-3">
                    <Badge
                      variant={
                        mov.movementType === 'sale'
                          ? 'neutral'
                          : mov.movementType === 'purchase'
                          ? 'success'
                          : mov.movementType === 'in'
                          ? 'indigo'
                          : 'amber'
                      }
                      size="sm"
                    >
                      {mov.movementType.toUpperCase()}
                    </Badge>
                  </td>
                  <td className="py-3 px-3 font-bold font-mono">
                    <span className={mov.quantity > 0 ? 'text-emerald-600' : 'text-rose-600'}>
                      {mov.quantity > 0 ? `+${mov.quantity}` : mov.quantity}
                    </span>
                  </td>
                  <td className="py-3 px-3 text-slate-600 font-mono text-[11px]">
                    {mov.previousStock} ➔ <strong className="text-slate-900">{mov.newStock}</strong>
                  </td>
                  <td className="py-3 px-3 font-mono text-indigo-900 font-semibold">{mov.referenceNumber}</td>
                  <td className="py-3 px-3 text-slate-600">
                    <p className="truncate max-w-xs">{mov.reason}</p>
                    <p className="text-[10px] text-slate-400">By {mov.performedBy}</p>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* STOCK ADJUSTMENT MODAL */}
      <Modal isOpen={showAdjModal} onClose={() => setShowAdjModal(false)} title="New Stock Adjustment" maxWidth="md">
        <form onSubmit={handleExecuteAction} className="space-y-4">
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Select Product</label>
            <select
              value={selectedProdId}
              onChange={(e) => setSelectedProdId(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
            >
              {products.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name} (Curr: {p.stock} {p.unit})
                </option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Movement Direction</label>
              <select
                value={adjType}
                onChange={(e) => setAdjType(e.target.value as 'in' | 'out')}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
              >
                <option value="in">+ Stock In (Add)</option>
                <option value="out">- Stock Out (Reduce)</option>
              </select>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Quantity</label>
              <input
                type="number"
                min="1"
                value={qty}
                onChange={(e) => setQty(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-bold"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Reason for Stock Movement</label>
            <input
              type="text"
              required
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-medium"
            />
          </div>

          <button
            type="button"
            onClick={handleExecuteAction}
            className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-bold text-xs shadow-md"
          >
            Confirm Stock Shift
          </button>
        </form>
      </Modal>
    </div>
  );

  async function handleExecuteAction(e: any) {
    e.preventDefault();
    const prod = products.find((p) => p.id === selectedProdId);
    if (!prod) return;

    const delta = adjType === 'in' ? Math.abs(qty) : -Math.abs(qty);
    try {
      await adjustStock(prod.id, delta, reason);
      setShowAdjModal(false);
      showToast(`Updated stock for ${prod.name}`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not adjust stock', 'error');
    }
  }
};
