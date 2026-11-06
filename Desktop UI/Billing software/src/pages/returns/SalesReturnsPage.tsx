import React, { useState } from 'react';
import {
  RotateCcw,
  Search,
  Plus,
  Package,
  FileSpreadsheet,
  Banknote,
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/common/Toast';
import { exportToCSV } from '../../utils/exportUtils';

export const SalesReturnsPage: React.FC = () => {
  const {
    salesReturns,
    invoices,
    products,
    addSalesReturn,
  } = useStore();
  const { showToast } = useToast();

  const [searchQuery, setSearchQuery] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);

  // Create Return Form
  const [selectedInvoiceId, setSelectedInvoiceId] = useState('');
  const [selectedProductId, setSelectedProductId] = useState('');
  const [returnQty, setReturnQty] = useState(1);
  const [returnReason, setReturnReason] = useState('Wrong item / duplicate purchase');

  const filteredReturns = salesReturns.filter((r) => {
    const q = searchQuery.toLowerCase().trim();
    return (
      !q ||
      r.returnNumber.toLowerCase().includes(q) ||
      r.customerName.toLowerCase().includes(q) ||
      r.invoiceNumber.toLowerCase().includes(q)
    );
  });

  const totalReturnValue = salesReturns.reduce((sum, r) => sum + r.totalRefundAmount, 0);

  const handleExportCSV = () => {
    exportToCSV('BillFlow_Sales_Returns', filteredReturns, [
      { key: 'returnNumber', label: 'Return #' },
      { key: 'invoiceNumber', label: 'Invoice #' },
      { key: 'customerName', label: 'Customer' },
      { key: 'totalRefundAmount', label: 'Return Value (₹)' },
      { key: 'returnReason', label: 'Reason' },
      { key: 'createdAt', label: 'Date' },
    ]);
    showToast('Sales returns exported as CSV', 'success');
  };

  const handleCreateReturn = async (e: React.FormEvent) => {
    e.preventDefault();
    const invoice = invoices.find((inv) => inv.id === selectedInvoiceId);
    const product = products.find((p) => p.id === selectedProductId);
    if (!invoice || !product) {
      showToast('Please select a valid invoice and product', 'error');
      return;
    }
    const soldItem = invoice.items.find((item) => item.productId === product.id);
    if (!soldItem || returnQty <= 0 || returnQty > soldItem.quantity) {
      showToast('Return quantity exceeds the quantity on this invoice', 'error');
      return;
    }
    const refundAmount = soldItem.price * returnQty;

    try {
      const created = await addSalesReturn({
        invoiceId: invoice.id,
        invoiceNumber: invoice.invoiceNumber,
        customerId: invoice.customerId,
        customerName: invoice.customerName,
        items: [
          {
            productId: product.id,
            productName: product.name,
            quantity: returnQty,
            unitPrice: soldItem.price,
            refundAmount,
            taxRate: product.taxRate,
            reason: returnReason,
          },
        ],
        totalRefundAmount: refundAmount,
        refundMethod: 'cash',
        returnReason,
      });
      showToast(`Sales Return ${created.returnNumber} recorded. Stock was restored.`, 'success');
      setShowCreateModal(false);
      setReturnQty(1);
      setReturnReason('Wrong item / duplicate purchase');
    } catch (err: any) {
      showToast(err.message || 'Failed to create return', 'error');
    }
  };


  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Sales Returns</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            Record customer returns and restore inventory. Settle any customer refund separately.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={handleExportCSV}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-xs font-semibold transition-colors"
          >
            <FileSpreadsheet className="w-4 h-4 text-emerald-600" />
            <span>Export CSV</span>
          </button>
          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-md shadow-indigo-600/30 transition-all"
          >
            <Plus className="w-4 h-4" />
            <span>New Sales Return</span>
          </button>
        </div>
      </div>

      {/* Summary Metrics */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-rose-50 flex items-center justify-center">
              <RotateCcw className="w-5 h-5 text-rose-600" />
            </div>
            <div>
              <p className="text-[11px] text-slate-500 font-medium">Total Returns</p>
              <p className="text-lg font-bold text-slate-900">{salesReturns.length}</p>
            </div>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-amber-50 flex items-center justify-center">
              <Banknote className="w-5 h-5 text-amber-600" />
            </div>
            <div>
              <p className="text-[11px] text-slate-500 font-medium">Total Return Value</p>
              <p className="text-lg font-bold text-slate-900">₹{totalReturnValue.toLocaleString()}</p>
            </div>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-emerald-50 flex items-center justify-center">
              <Package className="w-5 h-5 text-emerald-600" />
            </div>
            <div>
              <p className="text-[11px] text-slate-500 font-medium">Items Restocked</p>
              <p className="text-lg font-bold text-slate-900">
                {salesReturns.reduce((sum, r) => sum + r.items.reduce((s, i) => s + i.quantity, 0), 0)}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Search */}
      <div className="bg-white p-4 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div className="relative">
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search by return #, invoice #, or customer name..."
            className="w-full pl-9 pr-3 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-600/30 focus:border-indigo-600"
          />
        </div>
      </div>

      {/* Returns Table */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 text-slate-500 text-[11px] uppercase tracking-wider">
              <tr>
                <th className="px-4 py-3 font-semibold">Return #</th>
                <th className="px-4 py-3 font-semibold">Invoice #</th>
                <th className="px-4 py-3 font-semibold">Customer</th>
                <th className="px-4 py-3 font-semibold">Items</th>
                <th className="px-4 py-3 font-semibold text-right">Return Value (₹)</th>
                <th className="px-4 py-3 font-semibold">Reason</th>
                <th className="px-4 py-3 font-semibold">Date</th>
                <th className="px-4 py-3 font-semibold">Processed By</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {filteredReturns.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-6 py-12 text-center text-sm text-slate-400">
                    <RotateCcw className="w-8 h-8 mx-auto mb-2 text-slate-300" />
                    No sales returns found
                  </td>
                </tr>
              ) : (
                filteredReturns.map((ret) => (
                  <tr key={ret.id} className="hover:bg-slate-50/50 transition-colors">
                    <td className="px-4 py-3 font-bold text-indigo-600">{ret.returnNumber}</td>
                    <td className="px-4 py-3 font-medium text-slate-700">{ret.invoiceNumber}</td>
                    <td className="px-4 py-3 text-slate-700">{ret.customerName}</td>
                    <td className="px-4 py-3">
                      {ret.items.map((item, idx) => (
                        <div key={idx} className="text-[11px] text-slate-600">
                          {item.productName} × {item.quantity}
                        </div>
                      ))}
                    </td>
                    <td className="px-4 py-3 text-right font-bold text-rose-600">
                      ₹{ret.totalRefundAmount.toLocaleString()}
                    </td>
                    <td className="px-4 py-3 text-slate-600 max-w-[180px] truncate">{ret.returnReason}</td>
                    <td className="px-4 py-3 text-slate-500 whitespace-nowrap">
                      {new Date(ret.createdAt).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })}
                    </td>
                    <td className="px-4 py-3 text-slate-600">{ret.processedBy || '—'}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Create Return Modal */}
      <Modal isOpen={showCreateModal} title="Process New Sales Return" onClose={() => setShowCreateModal(false)}>
          <form onSubmit={handleCreateReturn} className="space-y-4 p-1">
            {/* Select Invoice */}
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Original Invoice</label>
              <select
                value={selectedInvoiceId}
                onChange={(e) => setSelectedInvoiceId(e.target.value)}
                required
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30 focus:border-indigo-600"
              >
                <option value="">Select Invoice...</option>
                {invoices.slice(0, 20).map((inv) => (
                  <option key={inv.id} value={inv.id}>
                    {inv.invoiceNumber} — {inv.customerName} (₹{inv.grandTotal.toLocaleString()})
                  </option>
                ))}
              </select>
            </div>

            {/* Select Product */}
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Product to Return</label>
              <select
                value={selectedProductId}
                onChange={(e) => setSelectedProductId(e.target.value)}
                required
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30 focus:border-indigo-600"
              >
                <option value="">Select Product...</option>
                {products.filter((p) => invoices.find((invoice) => invoice.id === selectedInvoiceId)?.items.some((item) => item.productId === p.id)).map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name} — ₹{p.sellingPrice} (Stock: {p.stock})
                  </option>
                ))}
              </select>
            </div>

            {/* Quantity */}
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Return Quantity</label>
              <input
                type="number"
                min={1}
                max={99}
                value={returnQty}
                onChange={(e) => setReturnQty(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30 focus:border-indigo-600"
              />
            </div>

            {/* Reason */}
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Return Reason</label>
              <textarea
                value={returnReason}
                onChange={(e) => setReturnReason(e.target.value)}
                rows={2}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30 focus:border-indigo-600 resize-none"
              />
            </div>

            {/* Return value preview */}
            {selectedProductId && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-xl flex items-center justify-between">
                <span className="text-xs font-medium text-rose-700">Estimated Return Value</span>
                <span className="text-lg font-black text-rose-700">
                  ₹{((invoices.find((inv) => inv.id === selectedInvoiceId)?.items.find((item) => item.productId === selectedProductId)?.price || 0) * returnQty).toLocaleString()}
                </span>
              </div>
            )}

            <button
              type="submit"
              className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-sm font-bold shadow-md shadow-indigo-600/30 transition-all flex items-center justify-center gap-2"
            >
              <RotateCcw className="w-4 h-4" />
              Process Return & Restock
            </button>
          </form>
      </Modal>
    </div>
  );
};
