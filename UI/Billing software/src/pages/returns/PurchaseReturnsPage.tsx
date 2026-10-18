import React, { useState } from 'react';
import {
  RotateCcw,
  Search,
  Plus,
  FileSpreadsheet,
  Truck,
  Package,
  Banknote,
  CheckCircle2,
  AlertTriangle,
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/common/Toast';
import { exportToCSV } from '../../utils/exportUtils';

export const PurchaseReturnsPage: React.FC = () => {
  const {
    purchaseReturns,
    purchases,
    products,
    suppliers,
    addPurchaseReturn,
  } = useStore();
  const { showToast } = useToast();

  const [searchQuery, setSearchQuery] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);

  // Form state
  const [selectedPurchaseId, setSelectedPurchaseId] = useState('');
  const [selectedProductId, setSelectedProductId] = useState('');
  const [returnQty, setReturnQty] = useState(5);
  const [returnReason, setReturnReason] = useState('Damaged / expired goods in batch');

  const filteredReturns = purchaseReturns.filter((r) => {
    const q = searchQuery.toLowerCase().trim();
    return (
      !q ||
      r.returnNumber.toLowerCase().includes(q) ||
      r.supplierName.toLowerCase().includes(q) ||
      r.purchaseNumber.toLowerCase().includes(q)
    );
  });

  const totalRefundValue = purchaseReturns.reduce((sum, r) => sum + r.totalRefundAmount, 0);

  const handleExportCSV = () => {
    exportToCSV('BillFlow_Purchase_Returns', filteredReturns, [
      { key: 'returnNumber', label: 'Return #' },
      { key: 'purchaseNumber', label: 'Purchase #' },
      { key: 'supplierName', label: 'Supplier' },
      { key: 'totalRefundAmount', label: 'Return Value (₹)' },
      { key: 'returnReason', label: 'Reason' },
      { key: 'status', label: 'Status' },
      { key: 'date', label: 'Date' },
    ]);
    showToast('Purchase returns exported as CSV', 'success');
  };

  const handleCreateReturn = async (e: React.FormEvent) => {
    e.preventDefault();
    const purchase = purchases.find((p) => p.id === selectedPurchaseId);
    const product = products.find((p) => p.id === selectedProductId);
    const purchasedItem = purchase?.items.find((item) => item.productId === selectedProductId);
    if (!purchase || !product || !purchasedItem || returnQty <= 0 || returnQty > purchasedItem.quantity) {
      showToast('Please select valid purchase and product', 'error');
      return;
    }
    const refundAmount = purchasedItem.purchasePrice * returnQty;

    try {
      const created = await addPurchaseReturn({
        purchaseId: purchase.id,
        purchaseNumber: purchase.purchaseNumber,
        supplierId: purchase.supplierId,
        supplierName: purchase.supplierName,
        items: [
          {
            productId: product.id,
            productName: product.name,
            quantity: returnQty,
            purchasePrice: purchasedItem.purchasePrice,
            refundAmount,
            reason: returnReason,
          },
        ],
        totalRefundAmount: refundAmount,
        returnReason,
      });
      showToast(`Purchase Return ${created.returnNumber} recorded. Stock was reduced.`, 'success');
      setShowCreateModal(false);
    } catch (err: any) {
      showToast(err.message || 'Failed to create purchase return', 'error');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Purchase Returns to Suppliers</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            Return defective, expired, or excess goods back to vendors and auto-deduct inventory
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={handleExportCSV} className="flex items-center gap-1.5 px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-xs font-semibold transition-colors">
            <FileSpreadsheet className="w-4 h-4 text-emerald-600" />
            <span>Export CSV</span>
          </button>
          <button onClick={() => setShowCreateModal(true)} className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-md shadow-indigo-600/30 transition-all">
            <Plus className="w-4 h-4" />
            <span>New Purchase Return</span>
          </button>
        </div>
      </div>

      {/* Summary */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-orange-50 flex items-center justify-center"><Truck className="w-5 h-5 text-orange-600" /></div>
            <div><p className="text-[11px] text-slate-500 font-medium">Total Returns</p><p className="text-lg font-bold text-slate-900">{purchaseReturns.length}</p></div>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-rose-50 flex items-center justify-center"><Banknote className="w-5 h-5 text-rose-600" /></div>
            <div><p className="text-[11px] text-slate-500 font-medium">Total Return Value</p><p className="text-lg font-bold text-slate-900">₹{totalRefundValue.toLocaleString()}</p></div>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-emerald-50 flex items-center justify-center"><CheckCircle2 className="w-5 h-5 text-emerald-600" /></div>
            <div><p className="text-[11px] text-slate-500 font-medium">Completed</p><p className="text-lg font-bold text-slate-900">{purchaseReturns.filter((r) => r.status === 'completed').length}</p></div>
          </div>
        </div>
      </div>

      {/* Search */}
      <div className="bg-white p-4 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div className="relative">
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input type="text" value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} placeholder="Search by return #, purchase #, or supplier..."
            className="w-full pl-9 pr-3 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-600/30 focus:border-indigo-600" />
        </div>
      </div>

      {/* Table */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 text-slate-500 text-[11px] uppercase tracking-wider">
              <tr>
                <th className="px-4 py-3 font-semibold">Return #</th>
                <th className="px-4 py-3 font-semibold">Purchase #</th>
                <th className="px-4 py-3 font-semibold">Supplier</th>
                <th className="px-4 py-3 font-semibold">Items</th>
                <th className="px-4 py-3 font-semibold text-right">Value (₹)</th>
                <th className="px-4 py-3 font-semibold">Reason</th>
                <th className="px-4 py-3 font-semibold">Status</th>
                <th className="px-4 py-3 font-semibold">Date</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {filteredReturns.length === 0 ? (
                <tr><td colSpan={8} className="px-6 py-12 text-center text-sm text-slate-400"><RotateCcw className="w-8 h-8 mx-auto mb-2 text-slate-300" />No purchase returns found</td></tr>
              ) : (
                filteredReturns.map((ret) => (
                  <tr key={ret.id} className="hover:bg-slate-50/50 transition-colors">
                    <td className="px-4 py-3 font-bold text-indigo-600">{ret.returnNumber}</td>
                    <td className="px-4 py-3 font-medium text-slate-700">{ret.purchaseNumber}</td>
                    <td className="px-4 py-3 text-slate-700">{ret.supplierName}</td>
                    <td className="px-4 py-3">
                      {ret.items.map((item, idx) => (
                        <div key={idx} className="text-[11px] text-slate-600">{item.productName} × {item.quantity}</div>
                      ))}
                    </td>
                    <td className="px-4 py-3 text-right font-bold text-rose-600">₹{ret.totalRefundAmount.toLocaleString()}</td>
                    <td className="px-4 py-3 text-slate-600 max-w-[180px] truncate">{ret.returnReason}</td>
                    <td className="px-4 py-3">
                      <Badge variant={ret.status === 'completed' ? 'success' : ret.status === 'approved' ? 'info' : 'warning'}>{ret.status}</Badge>
                    </td>
                    <td className="px-4 py-3 text-slate-500 whitespace-nowrap">{ret.date}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Create Modal */}
      <Modal isOpen={showCreateModal} title="Create Purchase Return" onClose={() => setShowCreateModal(false)}>
          <form onSubmit={handleCreateReturn} className="space-y-4 p-1">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Original Purchase Order</label>
              <select value={selectedPurchaseId} onChange={(e) => setSelectedPurchaseId(e.target.value)} required
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30">
                <option value="">Select Purchase...</option>
                {purchases.map((p) => (
                  <option key={p.id} value={p.id}>{p.purchaseNumber} — {p.supplierName} (₹{p.grandTotal.toLocaleString()})</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Product to Return</label>
              <select value={selectedProductId} onChange={(e) => setSelectedProductId(e.target.value)} required
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30">
                <option value="">Select Product...</option>
                {products.filter((p) => purchases.find((purchase) => purchase.id === selectedPurchaseId)?.items.some((item) => item.productId === p.id)).map((p) => (<option key={p.id} value={p.id}>{p.name} — ₹{p.purchasePrice} (Stock: {p.stock})</option>))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Return Qty</label>
              <input type="number" min={1} value={returnQty} onChange={(e) => setReturnQty(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30" />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Return Reason</label>
              <textarea value={returnReason} onChange={(e) => setReturnReason(e.target.value)} rows={2}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30 resize-none" />
            </div>
            {selectedProductId && (
              <div className="p-3 bg-orange-50 border border-orange-200 rounded-xl flex items-center justify-between">
                <span className="text-xs font-medium text-orange-700">Return Value</span>
                <span className="text-lg font-black text-orange-700">₹{((products.find((p) => p.id === selectedProductId)?.purchasePrice || 0) * returnQty).toLocaleString()}</span>
              </div>
            )}
            <button type="submit" className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-sm font-bold shadow-md shadow-indigo-600/30 transition-all flex items-center justify-center gap-2">
              <Truck className="w-4 h-4" /> Process Return to Supplier
            </button>
          </form>
      </Modal>
    </div>
  );
};
