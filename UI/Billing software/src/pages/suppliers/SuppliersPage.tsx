import React, { useState } from 'react';
import { Search, Plus, FileSpreadsheet } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { exportToCSV } from '../../utils/exportUtils';
import { useToast } from '../../components/common/Toast';

export const SuppliersPage: React.FC = () => {
  const { suppliers, addSupplier } = useStore();
  const { showToast } = useToast();

  const [searchQuery, setSearchQuery] = useState('');
  const [showAddModal, setShowAddModal] = useState(false);

  const [name, setName] = useState('');
  const [companyName, setCompanyName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [gstin, setGstin] = useState('');
  const [address, setAddress] = useState('');

  const filteredSuppliers = suppliers.filter((s) => {
    const q = searchQuery.toLowerCase().trim();
    return (
      !q ||
      s.companyName.toLowerCase().includes(q) ||
      s.name.toLowerCase().includes(q) ||
      s.phone.includes(q)
    );
  });

  const handleExportCSV = () => {
    exportToCSV('BillFlow_Suppliers_Directory', filteredSuppliers, [
      { key: 'companyName', label: 'Company Name' },
      { key: 'name', label: 'Contact Person' },
      { key: 'phone', label: 'Phone' },
      { key: 'email', label: 'Email' },
      { key: 'gstin', label: 'GSTIN' },
      { key: 'totalPurchases', label: 'Total Purchases (₹)' },
      { key: 'outstandingPayable', label: 'Payable Balance (₹)' },
    ]);
    showToast('Suppliers exported as CSV', 'success');
  };

  const handleCreateSupplier = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!companyName || !phone) return;
    try {
    const created = await addSupplier({
      name: name || companyName,
      companyName,
      phone,
      email,
      gstin,
      address,
      status: 'active',
    });
    setShowAddModal(false);
    setCompanyName('');
    setName('');
    setPhone('');
    setEmail('');
    setGstin('');
    setAddress('');
    showToast(`Added supplier ${created.companyName}`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not add supplier', 'error');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Suppliers & Vendor Directory</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            Manage wholesale distributors, vendor contact info, and payables
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
            <span>Add Supplier</span>
          </button>
        </div>
      </div>

      {/* Search Input */}
      <div className="bg-white p-4 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div className="relative">
          <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search company name, contact person, phone..."
            className="w-full pl-10 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-900"
          />
        </div>
      </div>

      {/* Suppliers Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {filteredSuppliers.map((s) => (
          <div key={s.id} className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs space-y-3">
            <div className="flex items-start justify-between">
              <div>
                <h3 className="text-sm font-bold text-slate-900">{s.companyName}</h3>
                <p className="text-xs text-slate-500">Contact: {s.name}</p>
              </div>
              <Badge variant="indigo" size="sm">
                Active
              </Badge>
            </div>

            <div className="space-y-1 text-xs text-slate-600 font-mono">
              <p>Phone: {s.phone}</p>
              <p>GSTIN: {s.gstin}</p>
              <p className="truncate text-slate-500">{s.address}</p>
            </div>

            <div className="pt-3 border-t border-slate-100 flex items-center justify-between text-xs">
              <div>
                <span className="text-[10px] text-slate-400 uppercase font-semibold">Total Purchases</span>
                <p className="font-bold text-slate-900">₹{s.totalPurchases.toLocaleString()}</p>
              </div>
              <div className="text-right">
                <span className="text-[10px] text-slate-400 uppercase font-semibold">Payable Balance</span>
                <p className="font-bold text-amber-600">₹{s.outstandingPayable.toLocaleString()}</p>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* ADD SUPPLIER MODAL */}
      <Modal isOpen={showAddModal} onClose={() => setShowAddModal(false)} title="Add Supplier Vendor" maxWidth="md">
        <form onSubmit={handleCreateSupplier} className="space-y-3">
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Company / Firm Name *</label>
            <input
              type="text"
              required
              value={companyName}
              onChange={(e) => setCompanyName(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Contact Person Name</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-medium"
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
            <label className="block text-xs font-semibold text-slate-700 mb-1">GSTIN Number</label>
            <input
              type="text"
              value={gstin}
              onChange={(e) => setGstin(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono uppercase"
            />
          </div>
          <button
            type="submit"
            className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-bold shadow-md"
          >
            Save Supplier Profile
          </button>
        </form>
      </Modal>
    </div>
  );
};
