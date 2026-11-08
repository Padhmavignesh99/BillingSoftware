import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ReceiptIndianRupee, Plus, Search, Trash2, FileSpreadsheet, Calendar, DollarSign } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { exportToCSV } from '../../utils/exportUtils';
import { useToast } from '../../components/common/Toast';

export const ExpensesPage: React.FC = () => {
  const navigate = useNavigate();
  const { expenses, addExpense, deleteExpense } = useStore();
  const { showToast } = useToast();

  const [searchQuery, setSearchQuery] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [showAddModal, setShowAddModal] = useState(false);

  const [title, setTitle] = useState('');
  const [category, setCategory] = useState<any>('rent');
  const [amount, setAmount] = useState(1500);
  const [method, setMethod] = useState<any>('upi');
  const [notes, setNotes] = useState('');

  const totalExpensesSum = expenses.reduce((sum, e) => sum + e.amount, 0);

  const filteredExpenses = expenses.filter((e) => {
    const matchesCat = categoryFilter === 'all' || e.category === categoryFilter;
    const q = searchQuery.toLowerCase().trim();
    const matchesQ = !q || e.title.toLowerCase().includes(q) || e.expenseNumber.toLowerCase().includes(q);
    return matchesCat && matchesQ;
  });

  const handleExportCSV = () => {
    exportToCSV('BillFlow_Expenses_Tracker', filteredExpenses, [
      { key: 'expenseNumber', label: 'Expense #' },
      { key: 'title', label: 'Title / Purpose' },
      { key: 'category', label: 'Category' },
      { key: 'amount', label: 'Amount (₹)' },
      { key: 'paymentMethod', label: 'Payment Method' },
      { key: 'date', label: 'Date' },
      { key: 'recordedBy', label: 'Recorded By' },
    ]);
    showToast('Expenses exported as CSV', 'success');
  };

  const handleCreateExpense = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title || amount <= 0) return;

    try {
    const created = await addExpense({
      title,
      category,
      amount,
      paymentMethod: method,
      date: new Date().toISOString().split('T')[0],
      notes,
      recordedBy: 'Staff',
    });

    setShowAddModal(false);
    setTitle('');
    setAmount(1000);
    setNotes('');
    showToast(`Recorded expense ${created.expenseNumber}`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not record expense', 'error');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Store Expense Tracker</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            Log and categorize store operational costs (Rent, Electricity, Salary, Transport, Maintenance)
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
            onClick={() => setShowAddModal(true)}
            className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-md shadow-indigo-600/30 transition-all"
          >
            <Plus className="w-4 h-4" />
            <span>Record Expense</span>
          </button>
        </div>
      </div>

      {/* Metric summary banner */}
      <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs flex items-center justify-between">
        <div>
          <span className="text-xs text-slate-500 uppercase font-semibold">Total Recorded Store Expenses</span>
          <h3 className="text-2xl font-bold text-slate-900 mt-0.5">₹{totalExpensesSum.toLocaleString()}</h3>
        </div>
        <Badge variant="indigo" size="md">
          {expenses.length} Records Total
        </Badge>
      </div>

      {/* Filters Bar */}
      <div className="bg-white p-4 rounded-2xl border border-slate-200/80 shadow-2xs flex flex-wrap items-center justify-between gap-3">
        <div className="relative flex-1 min-w-[240px]">
          <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search expense description or voucher #..."
            className="w-full pl-10 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-900"
          />
        </div>

        <select
          value={categoryFilter}
          onChange={(e) => setCategoryFilter(e.target.value)}
          className="px-3 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-700"
        >
          <option value="all">All Expense Categories</option>
          <option value="rent">Premises Rent</option>
          <option value="electricity">Electricity / Utilities</option>
          <option value="salary">Staff Salaries</option>
          <option value="transport">Freight & Transport</option>
          <option value="maintenance">Maintenance</option>
          <option value="marketing">Marketing</option>
          <option value="other">Other Expenses</option>
        </select>
      </div>

      {/* Expense Table */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50/80 text-slate-500 font-semibold border-b border-slate-100">
              <tr>
                <th className="py-3.5 px-4">Voucher #</th>
                <th className="py-3.5 px-4">Expense Title</th>
                <th className="py-3.5 px-4">Category</th>
                <th className="py-3.5 px-4">Amount</th>
                <th className="py-3.5 px-4">Payment Method</th>
                <th className="py-3.5 px-4">Date</th>
                <th className="py-3.5 px-4 text-right">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 font-medium">
              {filteredExpenses.map((exp) => (
                <tr key={exp.id} className="hover:bg-slate-50/60 transition-colors">
                  <td className="py-3.5 px-4 font-mono font-bold text-slate-900">{exp.expenseNumber}</td>
                  <td className="py-3.5 px-4">
                    <p className="font-semibold text-slate-900">{exp.title}</p>
                    {exp.notes && <p className="text-[10px] text-slate-400">{exp.notes}</p>}
                  </td>
                  <td className="py-3.5 px-4">
                    <Badge variant="purple" size="sm">
                      {exp.category.toUpperCase()}
                    </Badge>
                  </td>
                  <td className="py-3.5 px-4 font-extrabold text-slate-900">₹{exp.amount.toLocaleString()}</td>
                  <td className="py-3.5 px-4 uppercase font-mono text-slate-600">{exp.paymentMethod}</td>
                  <td className="py-3.5 px-4 text-slate-500 font-mono text-[11px]">{exp.date}</td>
                  <td className="py-3.5 px-4 text-right">
                    <button
                      onClick={async () => {
                        try {
                          await deleteExpense(exp.id);
                          showToast(`Deleted expense ${exp.expenseNumber}`, 'info');
                        } catch (err: any) {
                          showToast(err.message || 'Could not delete expense', 'error');
                        }
                      }}
                      className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* ADD EXPENSE MODAL */}
      <Modal isOpen={showAddModal} onClose={() => setShowAddModal(false)} title="Record Store Expense" maxWidth="md">
        <form onSubmit={handleCreateExpense} className="space-y-3">
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Expense Title / Description *</label>
            <input
              type="text"
              required
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="e.g. Oct Shop Premises Rent"
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Category</label>
              <select
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
              >
                <option value="rent">Rent</option>
                <option value="electricity">Electricity / EB</option>
                <option value="salary">Staff Salary</option>
                <option value="transport">Transport / Freight</option>
                <option value="maintenance">Maintenance</option>
                <option value="marketing">Marketing</option>
                <option value="other">Other</option>
              </select>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Amount (₹) *</label>
              <input
                type="number"
                min="1"
                required
                value={amount}
                onChange={(e) => setAmount(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-bold"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Payment Method</label>
            <select
              value={method}
              onChange={(e) => setMethod(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
            >
              <option value="upi">UPI / QR</option>
              <option value="cash">Cash</option>
              <option value="bank_transfer">Bank Transfer / NEFT</option>
              <option value="card">Card</option>
            </select>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Notes / Voucher Reference</label>
            <input
              type="text"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs"
            />
          </div>

          <button
            type="submit"
            className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-bold text-xs shadow-md"
          >
            Save Expense Voucher
          </button>
        </form>
      </Modal>
    </div>
  );
};
