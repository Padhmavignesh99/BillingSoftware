import React, { useState } from 'react';
import {
  Search,
  Plus,
  FileSpreadsheet,
  CreditCard,
  Banknote,
  CheckCircle2,
  Clock,
  AlertTriangle,
  IndianRupee,
  QrCode,
  Wallet,
  ArrowDownRight,
  ArrowUpRight,
  User,
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/common/Toast';
import { exportToCSV } from '../../utils/exportUtils';

export const CreditBillsPage: React.FC = () => {
  const {
    creditBills,
    customers,
    currentUser,
    invoices,
    addCreditBill,
    settleCreditBill,
    getMyCreditBalance,
  } = useStore();
  const { showToast } = useToast();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'pending' | 'partially_settled' | 'settled'>('all');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showSettleModal, setShowSettleModal] = useState(false);
  const [selectedBillId, setSelectedBillId] = useState('');

  // Create form
  const [newCustomerId, setNewCustomerId] = useState('');
  const [newAmount, setNewAmount] = useState(0);
  const [newDueDate, setNewDueDate] = useState(new Date(Date.now() + 15 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]);
  const [newNotes, setNewNotes] = useState('');

  // Settle form
  const [settleAmount, setSettleAmount] = useState(0);
  const [settleMethod, setSettleMethod] = useState<'cash' | 'upi' | 'card' | 'bank_transfer'>('cash');
  const [settleNotes, setSettleNotes] = useState('');

  const filteredBills = creditBills.filter((b) => {
    const matchesStatus = statusFilter === 'all' || b.status === statusFilter;
    const q = searchQuery.toLowerCase().trim();
    const matchesQuery =
      !q ||
      b.creditBillNumber.toLowerCase().includes(q) ||
      b.customerName.toLowerCase().includes(q) ||
      (b.invoiceNumber || '').toLowerCase().includes(q);
    return matchesStatus && matchesQuery;
  });

  const totalOutstanding = creditBills.reduce((sum, b) => sum + b.balanceAmount, 0);
  const totalSettled = creditBills.reduce((sum, b) => sum + b.settledAmount, 0);
  const pendingCount = creditBills.filter((b) => b.status !== 'settled').length;
  const myBalance = getMyCreditBalance();

  const handleExportCSV = () => {
    exportToCSV('BillFlow_Credit_Bills_Khata', filteredBills, [
      { key: 'creditBillNumber', label: 'Credit Bill #' },
      { key: 'customerName', label: 'Customer' },
      { key: 'invoiceNumber', label: 'Invoice #' },
      { key: 'amount', label: 'Bill Amount (₹)' },
      { key: 'settledAmount', label: 'Settled (₹)' },
      { key: 'balanceAmount', label: 'Balance (₹)' },
      { key: 'dueDate', label: 'Due Date' },
      { key: 'status', label: 'Status' },
      { key: 'userName', label: 'Created By' },
    ]);
    showToast('Credit bills exported as CSV', 'success');
  };

  const handleCreateBill = async (e: React.FormEvent) => {
    e.preventDefault();
    const customer = customers.find((c) => c.id === newCustomerId);
    if (!customer || newAmount <= 0) {
      showToast('Please select customer and enter valid amount', 'error');
      return;
    }
    try {
      const created = await addCreditBill({
        customerId: customer.id,
        customerName: customer.name,
        customerPhone: customer.phone,
        amount: newAmount,
        dueDate: newDueDate,
        notes: newNotes,
      });
      showToast(`Credit Bill ${created.creditBillNumber} created for ${customer.name} — ₹${newAmount.toLocaleString()}`, 'success');
      setShowCreateModal(false);
      setNewAmount(0);
      setNewNotes('');
    } catch (err: any) {
      showToast(err.message || 'Failed to create credit bill', 'error');
    }
  };

  const handleSettle = async (e: React.FormEvent) => {
    e.preventDefault();
    if (settleAmount <= 0) {
      showToast('Enter a valid settlement amount', 'error');
      return;
    }
    try {
      const result = await settleCreditBill({
        creditBillId: selectedBillId,
        settleAmount,
        paymentMethod: settleMethod,
        notes: settleNotes,
      });
      showToast(result.message, 'success');
      setShowSettleModal(false);
      setSettleAmount(0);
      setSettleNotes('');
    } catch (err: any) {
      showToast(err.message || 'Settlement failed', 'error');
    }
  };

  const openSettleModal = (billId: string) => {
    const bill = creditBills.find((b) => b.id === billId);
    setSelectedBillId(billId);
    setSettleAmount(bill?.balanceAmount || 0);
    setShowSettleModal(true);
  };

  const statusColor = (status: string): 'success' | 'warning' | 'danger' | 'neutral' => {
    switch (status) {
      case 'settled': return 'success';
      case 'partially_settled': return 'warning';
      case 'pending': return 'danger';
      default: return 'neutral';
    }
  };

  const isOverdue = (dueDate: string) => new Date(dueDate) < new Date();

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Credit Bills & Khata Ledger</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            User-wise credit bill management, customer outstanding tracking & settlement
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={handleExportCSV} className="flex items-center gap-1.5 px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-xs font-semibold transition-colors">
            <FileSpreadsheet className="w-4 h-4 text-emerald-600" />
            <span>Export CSV</span>
          </button>
          <button onClick={() => setShowCreateModal(true)} className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-md shadow-indigo-600/30 transition-all">
            <Plus className="w-4 h-4" />
            <span>New Credit Bill</span>
          </button>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-rose-50 flex items-center justify-center"><IndianRupee className="w-5 h-5 text-rose-600" /></div>
            <div><p className="text-[11px] text-slate-500 font-medium">Total Outstanding</p><p className="text-lg font-bold text-rose-600">₹{totalOutstanding.toLocaleString()}</p></div>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-emerald-50 flex items-center justify-center"><CheckCircle2 className="w-5 h-5 text-emerald-600" /></div>
            <div><p className="text-[11px] text-slate-500 font-medium">Total Settled</p><p className="text-lg font-bold text-emerald-600">₹{totalSettled.toLocaleString()}</p></div>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-amber-50 flex items-center justify-center"><Clock className="w-5 h-5 text-amber-600" /></div>
            <div><p className="text-[11px] text-slate-500 font-medium">Pending Bills</p><p className="text-lg font-bold text-slate-900">{pendingCount}</p></div>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-indigo-50 flex items-center justify-center"><User className="w-5 h-5 text-indigo-600" /></div>
            <div><p className="text-[11px] text-slate-500 font-medium">My Balance (User-wise)</p><p className="text-lg font-bold text-indigo-600">₹{myBalance.toLocaleString()}</p></div>
          </div>
        </div>
      </div>

      {/* Search & Filters */}
      <div className="bg-white p-4 rounded-2xl border border-slate-200/80 shadow-2xs flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input type="text" value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} placeholder="Search by credit bill #, customer, or invoice..."
            className="w-full pl-9 pr-3 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-600/30 focus:border-indigo-600" />
        </div>
        <div className="flex items-center gap-2">
          {(['all', 'pending', 'partially_settled', 'settled'] as const).map((s) => (
            <button key={s} onClick={() => setStatusFilter(s)}
              className={`px-3 py-2 rounded-xl text-xs font-bold transition-all ${statusFilter === s ? 'bg-indigo-600 text-white shadow-sm' : 'text-slate-600 bg-slate-100 hover:bg-slate-200'}`}>
              {s === 'all' ? 'All' : s === 'partially_settled' ? 'Partial' : s.charAt(0).toUpperCase() + s.slice(1)}
            </button>
          ))}
        </div>
      </div>

      {/* Credit Bills Table */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 text-slate-500 text-[11px] uppercase tracking-wider">
              <tr>
                <th className="px-4 py-3 font-semibold">Credit Bill #</th>
                <th className="px-4 py-3 font-semibold">Customer</th>
                <th className="px-4 py-3 font-semibold">Invoice #</th>
                <th className="px-4 py-3 font-semibold text-right">Amount (₹)</th>
                <th className="px-4 py-3 font-semibold text-right">Settled (₹)</th>
                <th className="px-4 py-3 font-semibold text-right">Balance (₹)</th>
                <th className="px-4 py-3 font-semibold">Due Date</th>
                <th className="px-4 py-3 font-semibold">Status</th>
                <th className="px-4 py-3 font-semibold">Created By</th>
                <th className="px-4 py-3 font-semibold text-center">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {filteredBills.length === 0 ? (
                <tr><td colSpan={10} className="px-6 py-12 text-center text-sm text-slate-400">
                  <CreditCard className="w-8 h-8 mx-auto mb-2 text-slate-300" />No credit bills found
                </td></tr>
              ) : (
                filteredBills.map((bill) => (
                  <tr key={bill.id} className="hover:bg-slate-50/50 transition-colors">
                    <td className="px-4 py-3 font-bold text-indigo-600">{bill.creditBillNumber}</td>
                    <td className="px-4 py-3">
                      <div className="text-slate-900 font-medium">{bill.customerName}</div>
                      {bill.customerPhone && <div className="text-[10px] text-slate-400">{bill.customerPhone}</div>}
                    </td>
                    <td className="px-4 py-3 text-slate-600">{bill.invoiceNumber || '—'}</td>
                    <td className="px-4 py-3 text-right font-bold text-slate-900">₹{bill.amount.toLocaleString()}</td>
                    <td className="px-4 py-3 text-right font-semibold text-emerald-600">₹{bill.settledAmount.toLocaleString()}</td>
                    <td className="px-4 py-3 text-right font-bold text-rose-600">
                      {bill.balanceAmount > 0 ? `₹${bill.balanceAmount.toLocaleString()}` : '₹0'}
                    </td>
                    <td className="px-4 py-3 whitespace-nowrap">
                      <span className={`text-xs font-medium ${isOverdue(bill.dueDate) && bill.status !== 'settled' ? 'text-rose-600' : 'text-slate-600'}`}>
                        {bill.dueDate}
                        {isOverdue(bill.dueDate) && bill.status !== 'settled' && (
                          <AlertTriangle className="w-3 h-3 inline ml-1 text-rose-500" />
                        )}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <Badge variant={statusColor(bill.status)}>
                        {bill.status === 'partially_settled' ? 'Partial' : bill.status}
                      </Badge>
                    </td>
                    <td className="px-4 py-3 text-slate-600 text-[11px]">{bill.userName || '—'}</td>
                    <td className="px-4 py-3 text-center">
                      {bill.status !== 'settled' ? (
                        <button
                          onClick={() => openSettleModal(bill.id)}
                          className="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-[11px] font-bold shadow-sm transition-all"
                        >
                          Settle
                        </button>
                      ) : (
                        <span className="text-emerald-600 text-[11px] font-bold flex items-center justify-center gap-1"><CheckCircle2 className="w-3 h-3" /> Paid</span>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Create Credit Bill Modal */}
      <Modal isOpen={showCreateModal} title="Create New Credit Bill" onClose={() => setShowCreateModal(false)}>
          <form onSubmit={handleCreateBill} className="space-y-4 p-1">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Customer</label>
              <select value={newCustomerId} onChange={(e) => setNewCustomerId(e.target.value)} required
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30">
                <option value="">Select Customer...</option>
                {customers.map((c) => (
                  <option key={c.id} value={c.id}>{c.name} ({c.phone})</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Credit Amount (₹)</label>
              <input type="number" min={1} value={newAmount} onChange={(e) => setNewAmount(Number(e.target.value))} required
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30" />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Due Date</label>
              <input type="date" value={newDueDate} onChange={(e) => setNewDueDate(e.target.value)} required
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30" />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Notes</label>
              <textarea value={newNotes} onChange={(e) => setNewNotes(e.target.value)} rows={2}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30 resize-none"
                placeholder="e.g. Monthly bulk grocery Khata credit" />
            </div>
            <button type="submit" className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-sm font-bold shadow-md shadow-indigo-600/30 transition-all flex items-center justify-center gap-2">
              <CreditCard className="w-4 h-4" /> Create Credit Bill
            </button>
          </form>
      </Modal>

      {/* Settle Credit Bill Modal */}
      {(() => {
        const bill = creditBills.find((b) => b.id === selectedBillId);
        return (
          <Modal isOpen={showSettleModal} title={`Settle — ${bill?.creditBillNumber || ''}`} onClose={() => setShowSettleModal(false)}>
            <form onSubmit={handleSettle} className="space-y-4 p-1">
              <div className="p-3 bg-slate-50 border border-slate-200 rounded-xl space-y-1">
                <div className="flex items-center justify-between text-xs">
                  <span className="text-slate-500">Customer</span>
                  <span className="font-semibold text-slate-900">{bill?.customerName}</span>
                </div>
                <div className="flex items-center justify-between text-xs">
                  <span className="text-slate-500">Total Amount</span>
                  <span className="font-bold text-slate-900">₹{bill?.amount.toLocaleString()}</span>
                </div>
                <div className="flex items-center justify-between text-xs">
                  <span className="text-slate-500">Already Settled</span>
                  <span className="font-semibold text-emerald-600">₹{bill?.settledAmount.toLocaleString()}</span>
                </div>
                <div className="flex items-center justify-between text-xs pt-1 border-t border-slate-200">
                  <span className="text-slate-500 font-bold">Outstanding Balance</span>
                  <span className="text-lg font-black text-rose-600">₹{bill?.balanceAmount.toLocaleString()}</span>
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Settlement Amount (₹)</label>
                <input type="number" min={1} max={bill?.balanceAmount || 99999} value={settleAmount} onChange={(e) => setSettleAmount(Number(e.target.value))} required
                  className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30" />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Payment Method</label>
                <div className="grid grid-cols-2 gap-2">
                  {([
                    { id: 'cash', label: 'Cash', icon: Banknote },
                    { id: 'upi', label: 'UPI', icon: QrCode },
                    { id: 'card', label: 'Card', icon: CreditCard },
                    { id: 'bank_transfer', label: 'Bank Transfer', icon: Wallet },
                  ] as const).map((m) => {
                    const Icon = m.icon;
                    const active = settleMethod === m.id;
                    return (
                      <button key={m.id} type="button" onClick={() => setSettleMethod(m.id)}
                        className={`flex items-center gap-2 px-3 py-2 rounded-lg text-xs font-semibold border transition-all ${
                          active ? 'bg-indigo-600 text-white border-indigo-600 shadow-md' : 'bg-slate-50 text-slate-600 border-slate-200 hover:bg-slate-100'
                        }`}>
                        <Icon className="w-4 h-4" />{m.label}
                      </button>
                    );
                  })}
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Notes / Receipt #</label>
                <input type="text" value={settleNotes} onChange={(e) => setSettleNotes(e.target.value)}
                  placeholder="Receipt number or notes..."
                  className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-600/30" />
              </div>

              <button type="submit" className="w-full py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-sm font-bold shadow-md shadow-emerald-600/30 transition-all flex items-center justify-center gap-2">
                <CheckCircle2 className="w-4 h-4" /> Record Settlement of ₹{settleAmount.toLocaleString()}
              </button>
            </form>
          </Modal>
        );
      })()}
    </div>
  );
};
