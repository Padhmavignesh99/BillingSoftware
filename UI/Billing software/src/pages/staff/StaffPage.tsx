import React, { useState } from 'react';
import { Plus } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/common/Toast';
import type { UserRole } from '../../types';

export const StaffPage: React.FC = () => {
  const { staff, addStaff } = useStore();
  const { showToast } = useToast();

  const [showAddModal, setShowAddModal] = useState(false);
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState<UserRole>('cashier');

  const handleCreateStaff = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name || !email || !password) return;

    try {
    const created = await addStaff({
      name,
      phone,
      email,
      password,
      role,
      status: 'active',
      avatarUrl: `https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100&auto=format&fit=crop&q=80`,
      permissions: {
        createBill: true,
        editBill: role === 'admin' || role === 'manager',
        cancelBill: role === 'admin' || role === 'manager',
        viewProducts: true,
        manageProducts: role === 'admin' || role === 'manager',
        viewInventory: true,
        manageInventory: role === 'admin' || role === 'manager',
        viewReports: role === 'admin' || role === 'manager',
        viewProfit: role === 'admin',
        manageStaff: role === 'admin',
        manageSettings: role === 'admin',
      },
    });

    setShowAddModal(false);
    setName('');
    setPhone('');
    setEmail('');
    setPassword('');
    showToast(`Added staff member ${created.name}`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not create staff account', 'error');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Staff & Role Permissions</h1>
          <p className="text-xs sm:text-sm text-slate-500">Manage cashier and store staff accounts</p>
        </div>

        <button
          onClick={() => setShowAddModal(true)}
          className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-md shadow-indigo-600/30 transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>Add Staff Member</span>
        </button>
      </div>

      {/* Staff Members List */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {staff.map((member) => (
          <div key={member.id} className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
            <div className="flex items-center gap-3">
              <img
                src={member.avatarUrl}
                alt={member.name}
                className="w-10 h-10 rounded-full object-cover border border-slate-200"
              />
              <div>
                <h3 className="text-sm font-bold text-slate-900">{member.name}</h3>
                <p className="text-xs text-slate-500 font-mono">{member.phone}</p>
              </div>
            </div>

            <div className="flex items-center justify-between">
              <Badge variant={member.role === 'admin' ? 'indigo' : member.role === 'manager' ? 'purple' : 'neutral'} size="sm">
                {member.role.toUpperCase()}
              </Badge>
              <span className="text-[10px] text-emerald-600 font-bold uppercase">● Active</span>
            </div>

            {/* Permission Checkboxes Toggles Matrix */}
            <div className="pt-3 border-t border-slate-100 space-y-1.5 text-xs">
              <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-2">Role access</p>
              {Object.entries(member.permissions).map(([key, val]) => (
                <label key={key} className="flex items-center justify-between text-slate-700 p-1 rounded">
                  <span className="capitalize text-[11px]">{key.replace(/([A-Z])/g, ' $1')}</span>
                  <input
                    type="checkbox"
                    checked={val}
                    readOnly
                    disabled
                    className="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
                  />
                </label>
              ))}
            </div>
          </div>
        ))}
      </div>

      {/* ADD STAFF MODAL */}
      <Modal isOpen={showAddModal} onClose={() => setShowAddModal(false)} title="Add Staff Member" maxWidth="md">
        <form onSubmit={handleCreateStaff} className="space-y-3">
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
            <label className="block text-xs font-semibold text-slate-700 mb-1">Email *</label>
            <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs" />
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Temporary Password *</label>
            <input type="password" required minLength={8} value={password} onChange={(e) => setPassword(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs" />
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Role</label>
            <select
              value={role}
              onChange={(e) => setRole(e.target.value as any)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
            >
              <option value="cashier">Cashier</option>
              <option value="manager">Store Manager</option>
              <option value="sales">Sales Staff</option>
            </select>
          </div>
          <button
            type="submit"
            className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-bold shadow-md"
          >
            Create Staff Account
          </button>
        </form>
      </Modal>
    </div>
  );
};
