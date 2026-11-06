import React, { useState } from 'react';
import { Plus, Search, FileSpreadsheet } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/common/Toast';
import { exportToCSV } from '../../utils/exportUtils';

export const PurchasesPage: React.FC = () => {
  const { purchases, suppliers, products, addPurchase } = useStore();
  const { showToast } = useToast();

  const [searchQuery, setSearchQuery] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);

  // New Purchase Form State
  const [supplierId, setSupplierId] = useState(suppliers[0]?.id || '');
  const [productId, setProductId] = useState(products[0]?.id || '');
  const [qty, setQty] = useState(50);
  const [purchaseCost, setPurchaseCost] = useState(50);
  const [invoiceRef, setInvoiceRef] = useState(`BILL-SUP-${Math.floor(1000 + Math.random() * 9000)}`);

  const filteredPurchases = purchases.filter((p) => {
    const q = searchQuery.toLowerCase().trim();
    return (
      !q ||
      p.purchaseNumber.toLowerCase().includes(q) ||
      p.supplierName.toLowerCase().includes(q)
    );
  });

  const handleExportCSV = () => {
    exportToCSV('BillFlow_Purchases_Ledger', filteredPurchases, [
      { key: 'purchaseNumber', label: 'Purchase #' },
      { key: 'supplierName', label: 'Supplier Company' },
      { key: 'date', label: 'Order Date' },
      { key: 'grandTotal', label: 'Total Amount (₹)' },
      { key: 'paymentStatus', label: 'Payment Status' },
      { key: 'status', label: 'Stock Status' },
    ]);
    showToast('Purchases exported as CSV', 'success');
  };

  const handleSavePurchase = async (e: React.FormEvent) => {
    e.preventDefault();
    const sup = suppliers.find((s) => s.id === supplierId);
    const prod = products.find((p) => p.id === productId);
    if (!sup || !prod) return;

    const lineTotal = qty * purchaseCost;
    const taxTotal = (lineTotal * prod.taxRate) / 100;
    const grandTotal = lineTotal + taxTotal;

    try {
    const created = await addPurchase({
      supplierId: sup.id,
      supplierName: sup.companyName,
      items: [
        {
          productId: prod.id,
          productName: prod.name,
          sku: prod.sku,
          quantity: qty,
          purchasePrice: purchaseCost,
          taxRate: prod.taxRate,
          total: lineTotal,
        },
      ],
      subtotal: lineTotal,
      taxTotal,
      grandTotal,
      paymentStatus: 'paid',
      status: 'received',
      date: new Date().toISOString().split('T')[0],
      invoiceRef,
    });

    setShowCreateModal(false);
    showToast(`Purchase ${created.purchaseNumber} created & inventory stock updated!`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not create purchase', 'error');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Stock Purchase Orders</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            Record supplier inventory purchases & stock replenishment
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
            <span>New Purchase Order</span>
          </button>
        </div>
      </div>

      {/* Search */}
      <div className="bg-white p-4 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div className="relative">
          <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search by purchase # or supplier name..."
            className="w-full pl-10 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-900"
          />
        </div>
      </div>

      {/* Purchases Table */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50/80 text-slate-500 font-semibold border-b border-slate-100">
              <tr>
                <th className="py-3.5 px-4">Purchase #</th>
                <th className="py-3.5 px-4">Supplier Firm</th>
                <th className="py-3.5 px-4">Order Date</th>
                <th className="py-3.5 px-4">Items Count</th>
                <th className="py-3.5 px-4">Grand Total</th>
                <th className="py-3.5 px-4">Stock Status</th>
                <th className="py-3.5 px-4">Payment</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 font-medium">
              {filteredPurchases.map((p) => (
                <tr key={p.id} className="hover:bg-slate-50/60 transition-colors">
                  <td className="py-3.5 px-4 font-mono font-bold text-slate-900">{p.purchaseNumber}</td>
                  <td className="py-3.5 px-4 font-semibold text-slate-800">{p.supplierName}</td>
                  <td className="py-3.5 px-4 text-slate-500 font-mono">{p.date}</td>
                  <td className="py-3.5 px-4 text-slate-700">{p.items.length} items</td>
                  <td className="py-3.5 px-4 font-bold text-slate-900">₹{p.grandTotal.toLocaleString()}</td>
                  <td className="py-3.5 px-4">
                    <Badge variant={p.status === 'received' ? 'success' : 'warning'} size="sm">
                      {p.status.toUpperCase()}
                    </Badge>
                  </td>
                  <td className="py-3.5 px-4">
                    <Badge variant={p.paymentStatus === 'paid' ? 'indigo' : 'amber'} size="sm">
                      {p.paymentStatus.toUpperCase()}
                    </Badge>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* CREATE PURCHASE MODAL */}
      <Modal isOpen={showCreateModal} onClose={() => setShowCreateModal(false)} title="New Stock Purchase Entry" maxWidth="md">
        <form onSubmit={handleSavePurchase} className="space-y-4">
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Select Supplier</label>
            <select
              value={supplierId}
              onChange={(e) => setSupplierId(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
            >
              {suppliers.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.companyName} ({s.name})
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Select Product Item</label>
            <select
              value={productId}
              onChange={(e) => setProductId(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
            >
              {products.map((prod) => (
                <option key={prod.id} value={prod.id}>
                  {prod.name} (Curr Stock: {prod.stock})
                </option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Quantity Purchased</label>
              <input
                type="number"
                min="1"
                value={qty}
                onChange={(e) => setQty(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-bold"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Unit Cost Price (₹)</label>
              <input
                type="number"
                min="1"
                value={purchaseCost}
                onChange={(e) => setPurchaseCost(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-bold"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Supplier Invoice Bill Reference</label>
            <input
              type="text"
              value={invoiceRef}
              onChange={(e) => setInvoiceRef(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono"
            />
          </div>

          <button
            type="submit"
            className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-bold text-xs shadow-md"
          >
            Receive Stock & Add Purchase
          </button>
        </form>
      </Modal>
    </div>
  );
};
