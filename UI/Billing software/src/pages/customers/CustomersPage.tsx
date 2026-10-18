import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Users,
  Search,
  Plus,
  Receipt,
  Smartphone,
  CreditCard,
  Eye,
  FileSpreadsheet,
  AlertCircle,
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { exportToCSV } from '../../utils/exportUtils';
import { useToast } from '../../components/common/Toast';
import { Modal } from '../../components/ui/Modal';
import { whatsappService } from '../../services/whatsappService';

export const CustomersPage: React.FC = () => {
  const navigate = useNavigate();
  const { customers, addCustomer, recordCustomerPayment } = useStore();
  const { showToast } = useToast();

  const [searchQuery, setSearchQuery] = useState('');
  const [typeFilter, setTypeFilter] = useState('all');

  const [showAddModal, setShowAddModal] = useState(false);
  const [payCustModal, setPayCustModal] = useState<any>(null);
  const [payAmount, setPayAmount] = useState<number>(500);
  const [payNotes, setPayNotes] = useState('Cash Payment towards Khata Ledger');

  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [address, setAddress] = useState('');
  const [gstin, setGstin] = useState('');
  const [custType, setCustType] = useState<'retail' | 'wholesaler' | 'vip'>('retail');

  const filteredCustomers = customers.filter((c) => {
    const matchesType = typeFilter === 'all' || c.type === typeFilter;
    const q = searchQuery.toLowerCase().trim();
    const matchesQ =
      !q ||
      c.name.toLowerCase().includes(q) ||
      c.phone.includes(q) ||
      (c.email && c.email.toLowerCase().includes(q));
    return matchesType && matchesQ;
  });

  const handleExportCSV = () => {
    exportToCSV('BillFlow_Customers_Khata', filteredCustomers, [
      { key: 'name', label: 'Customer Name' },
      { key: 'phone', label: 'Phone Number' },
      { key: 'email', label: 'Email' },
      { key: 'type', label: 'Type' },
      { key: 'totalPurchases', label: 'Total Sales (₹)' },
      { key: 'outstandingBalance', label: 'Khata Due Balance (₹)' },
      { key: 'loyaltyPoints', label: 'Loyalty Points' },
    ]);
    showToast('Customer Khata exported as CSV', 'success');
  };

  const handleCreateCustomer = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name || !phone) return;
    try {
    const created = await addCustomer({
      name,
      phone,
      email,
      address,
      gstin,
      type: custType,
    });
    setShowAddModal(false);
    setName('');
    setPhone('');
    setEmail('');
    setAddress('');
    setGstin('');
    showToast(`Added customer ${created.name}`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not add customer', 'error');
    }
  };

  const handleRecordPayment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!payCustModal || payAmount <= 0) return;
    try {
      await recordCustomerPayment(payCustModal.id, payAmount, payNotes);
      setPayCustModal(null);
      showToast(`Recorded ₹${payAmount} Khata payment for ${payCustModal.name}`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not record payment', 'error');
    }
  };

  const handleSendReminder = async (cust: any) => {
    try {
      await whatsappService.sendInvoice({
        customerName: cust.name,
        phone: cust.phone,
        invoiceNumber: 'KHATA-STATEMENT',
        grandTotal: cust.outstandingBalance,
      });
      showToast(`Simulated WhatsApp payment reminder sent to ${cust.name}`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Failed to send reminder', 'error');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Customer Management & Khata Ledger</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            Track customer purchases, credit accounts (Khata), payments and loyalty points
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
            <span>Add Customer</span>
          </button>
        </div>
      </div>

      {/* Search & Type Filters */}
      <div className="bg-white p-4 rounded-2xl border border-slate-200/80 shadow-2xs flex flex-wrap items-center justify-between gap-3">
        <div className="relative flex-1 min-w-[240px]">
          <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search customer name, phone number, email..."
            className="w-full pl-10 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-600/30"
          />
        </div>

        <select
          value={typeFilter}
          onChange={(e) => setTypeFilter(e.target.value)}
          className="px-3 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-700"
        >
          <option value="all">All Customer Types</option>
          <option value="walkin">Walk-in Customers</option>
          <option value="retail">Regular Retail</option>
          <option value="wholesaler">Wholesale Khata</option>
          <option value="vip">VIP Accounts</option>
        </select>
      </div>

      {/* Customers Data Table */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50/80 text-slate-500 font-semibold border-b border-slate-100">
              <tr>
                <th className="py-3.5 px-4">Customer Name</th>
                <th className="py-3.5 px-4">Phone / Email</th>
                <th className="py-3.5 px-4">Type</th>
                <th className="py-3.5 px-4">Total Purchases</th>
                <th className="py-3.5 px-4">Khata Due Balance</th>
                <th className="py-3.5 px-4 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 font-medium">
              {filteredCustomers.map((c) => (
                <tr key={c.id} className="hover:bg-slate-50/60 transition-colors">
                  <td className="py-3.5 px-4 font-bold text-slate-900">{c.name}</td>
                  <td className="py-3.5 px-4">
                    <p className="font-mono text-slate-800">{c.phone}</p>
                    {c.email && <p className="text-[10px] text-slate-400">{c.email}</p>}
                  </td>
                  <td className="py-3.5 px-4">
                    <Badge variant={c.type === 'vip' ? 'indigo' : c.type === 'wholesaler' ? 'purple' : 'neutral'} size="sm">
                      {c.type.toUpperCase()}
                    </Badge>
                  </td>
                  <td className="py-3.5 px-4 font-bold text-slate-900">₹{c.totalPurchases.toLocaleString()}</td>
                  <td className="py-3.5 px-4 font-bold">
                    {c.outstandingBalance > 0 ? (
                      <span className="text-amber-600 font-mono">₹{c.outstandingBalance.toLocaleString()}</span>
                    ) : (
                      <span className="text-emerald-600">No Due (Clear)</span>
                    )}
                  </td>
                  <td className="py-3.5 px-4 text-right space-x-1">
                    {c.outstandingBalance > 0 && (
                      <button
                        onClick={() => {
                          setPayCustModal(c);
                          setPayAmount(c.outstandingBalance);
                        }}
                        className="px-2.5 py-1 bg-emerald-50 hover:bg-emerald-100 text-emerald-700 rounded text-[11px] font-bold"
                      >
                        Record Pay
                      </button>
                    )}

                    {c.outstandingBalance > 0 && (
                      <button
                        onClick={() => handleSendReminder(c)}
                        className="p-1.5 text-slate-600 hover:text-emerald-600 hover:bg-slate-100 rounded-lg"
                        title="Send WhatsApp Reminder"
                      >
                        <Smartphone className="w-4 h-4" />
                      </button>
                    )}

                    <button
                      onClick={() => navigate(`/customers/${c.id}/ledger`)}
                      className="p-1.5 text-slate-600 hover:text-indigo-600 hover:bg-slate-100 rounded-lg"
                      title="View Ledger Statement"
                    >
                      <Eye className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* ADD CUSTOMER MODAL */}
      <Modal isOpen={showAddModal} onClose={() => setShowAddModal(false)} title="Add New Customer" maxWidth="md">
        <form onSubmit={handleCreateCustomer} className="space-y-3">
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Full Name *</label>
            <input
              type="text"
              required
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Phone Number *</label>
            <input
              type="text"
              required
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Customer Type</label>
            <select
              value={custType}
              onChange={(e) => setCustType(e.target.value as any)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
            >
              <option value="retail">Regular Retail</option>
              <option value="wholesaler">Wholesale Account</option>
              <option value="vip">VIP Preferred Customer</option>
            </select>
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Email Address</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs"
            />
          </div>
          <button
            type="submit"
            className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-bold shadow-md"
          >
            Save Customer Account
          </button>
        </form>
      </Modal>

      {/* RECORD PAYMENT MODAL */}
      {payCustModal && (
        <Modal
          isOpen={!!payCustModal}
          onClose={() => setPayCustModal(null)}
          title={`Record Khata Payment - ${payCustModal.name}`}
          maxWidth="md"
        >
          <form onSubmit={handleRecordPayment} className="space-y-4">
            <div className="p-3 bg-amber-50 rounded-xl border border-amber-200 text-xs text-amber-800">
              Current Outstanding Balance: <strong>₹{payCustModal.outstandingBalance.toLocaleString()}</strong>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Amount Collected (₹)</label>
              <input
                type="number"
                min="1"
                max={payCustModal.outstandingBalance}
                value={payAmount}
                onChange={(e) => setPayAmount(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm font-bold"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Payment Reference / Notes</label>
              <input
                type="text"
                value={payNotes}
                onChange={(e) => setPayNotes(e.target.value)}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs"
              />
            </div>

            <button
              type="submit"
              className="w-full py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-xs shadow-md"
            >
              Confirm Khata Payment
            </button>
          </form>
        </Modal>
      )}
    </div>
  );
};
