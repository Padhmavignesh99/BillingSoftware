import React, { useState } from 'react';
import { Plus, Pencil, Trash2, User } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/common/Toast';
import type { UserRole, UserPermissions, UserStaff } from '../../types';

// ─── Permission catalog keys ──────────────────────────────────────────────────
const PERMISSION_CATALOG = [
  { key: 'billing',           label: 'Billing',           module: 'POS' },
  { key: 'edit_bill',         label: 'Edit Bill',         module: 'POS' },
  { key: 'cancel_bill',       label: 'Cancel Bill',       module: 'POS' },
  { key: 'view_products',     label: 'View Products',     module: 'Products' },
  { key: 'manage_products',   label: 'Manage Products',   module: 'Products' },
  { key: 'view_inventory',    label: 'View Inventory',    module: 'Inventory' },
  { key: 'manage_inventory',  label: 'Manage Inventory',  module: 'Inventory' },
  { key: 'view_reports',      label: 'View Reports',      module: 'Reports' },
  { key: 'view_profit',       label: 'View Profit',       module: 'Reports' },
  { key: 'manage_customers',  label: 'Manage Customers',  module: 'Customers' },
  { key: 'manage_suppliers',  label: 'Manage Suppliers',  module: 'Purchases' },
  { key: 'manage_purchases',  label: 'Manage Purchases',  module: 'Purchases' },
  { key: 'manage_expenses',   label: 'Manage Expenses',   module: 'Finance' },
  { key: 'manage_staff',      label: 'Manage Staff',      module: 'Admin' },
  { key: 'manage_settings',   label: 'Manage Settings',   module: 'Admin' },
];

// ─── Map catalog key → UserPermissions key ────────────────────────────────────
const permToUserPerm: Record<string, keyof UserPermissions> = {
  billing:           'createBill',
  edit_bill:         'editBill',
  cancel_bill:       'cancelBill',
  view_products:     'viewProducts',
  manage_products:   'manageProducts',
  view_inventory:    'viewInventory',
  manage_inventory:  'manageInventory',
  view_reports:      'viewReports',
  view_profit:       'viewProfit',
  manage_staff:      'manageStaff',
  manage_settings:   'manageSettings',
};

// Build UserPermissions from a list of catalog keys
function buildPermissions(keys: string[]): UserPermissions {
  const base: UserPermissions = {
    createBill: false, editBill: false, cancelBill: false,
    viewProducts: false, manageProducts: false,
    viewInventory: false, manageInventory: false,
    viewReports: false, viewProfit: false,
    manageStaff: false, manageSettings: false,
  };
  keys.forEach((k) => {
    const mapped = permToUserPerm[k];
    if (mapped) base[mapped] = true;
  });
  return base;
}

// Default permissions per built-in role
const defaultPermsForRole = (role: UserRole): UserPermissions => {
  switch (role) {
    case 'admin':
      return buildPermissions(PERMISSION_CATALOG.map((p) => p.key));
    case 'manager':
      return buildPermissions(['billing', 'view_products', 'manage_products', 'view_inventory', 'manage_inventory', 'view_reports', 'manage_customers', 'manage_suppliers', 'manage_purchases', 'manage_expenses']);
    case 'cashier':
      return buildPermissions(['billing', 'edit_bill', 'cancel_bill', 'view_products', 'view_inventory']);
    case 'sales':
      return buildPermissions(['billing', 'view_products', 'view_inventory', 'manage_customers']);
    default:
      return buildPermissions([]);
  }
};

// ─── Role badge variant map ───────────────────────────────────────────────────
const roleBadgeVariant = (role: UserRole): 'indigo' | 'purple' | 'neutral' | 'emerald' | 'rose' | 'amber' => {
  if (role === 'admin') return 'indigo';
  if (role === 'manager') return 'purple';
  if (role === 'sales') return 'emerald';
  return 'neutral';
};

// ─── Staff card ───────────────────────────────────────────────────────────────
interface StaffCardProps {
  member: UserStaff;
  onEdit: () => void;
  onDelete: () => void;
}

const StaffCard: React.FC<StaffCardProps> = ({ member, onEdit, onDelete }) => {
  const initials = member.name.split(' ').map((n) => n[0]).join('').slice(0, 2).toUpperCase();
  const activePerms = PERMISSION_CATALOG.filter((p) => {
    const mapped = permToUserPerm[p.key];
    return mapped ? member.permissions[mapped] : false;
  });

  return (
    <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs p-5 space-y-4 hover:shadow-md transition-shadow">
      {/* Avatar + Name */}
      <div className="flex items-center gap-3">
        {member.avatarUrl ? (
          <img
            src={member.avatarUrl}
            alt={member.name}
            className="w-11 h-11 rounded-full object-cover border-2 border-slate-200"
          />
        ) : (
          <div className="w-11 h-11 rounded-full bg-indigo-100 flex items-center justify-center border-2 border-indigo-200">
            <span className="text-sm font-bold text-indigo-700">{initials}</span>
          </div>
        )}
        <div className="flex-1 min-w-0">
          <h3 className="text-sm font-bold text-slate-900 truncate">{member.name}</h3>
          <p className="text-[10px] text-slate-400 truncate">{member.email}</p>
          {member.phone && (
            <p className="text-[10px] text-slate-400 font-mono">{member.phone}</p>
          )}
        </div>
      </div>

      {/* Role + Status */}
      <div className="flex items-center justify-between">
        <Badge variant={roleBadgeVariant(member.role)} size="sm">
          {member.role.toUpperCase()}
        </Badge>
        <span className={`text-[10px] font-bold uppercase ${member.status === 'active' ? 'text-emerald-600' : 'text-slate-400'}`}>
          ● {member.status}
        </span>
      </div>

      {/* Permission pills */}
      <div className="pt-3 border-t border-slate-100">
        <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-2">Role Access</p>
        {activePerms.length === 0 ? (
          <p className="text-[10px] text-slate-300 italic">No permissions</p>
        ) : (
          <div className="flex flex-wrap gap-1">
            {activePerms.slice(0, 6).map((p) => (
              <span key={p.key} className="text-[9px] font-semibold bg-indigo-50 text-indigo-600 px-1.5 py-0.5 rounded">
                {p.label}
              </span>
            ))}
            {activePerms.length > 6 && (
              <span className="text-[9px] font-semibold bg-slate-100 text-slate-500 px-1.5 py-0.5 rounded">
                +{activePerms.length - 6}
              </span>
            )}
          </div>
        )}
      </div>

      {/* Actions */}
      <div className="flex items-center gap-2 pt-2 border-t border-slate-100">
        <button
          onClick={onEdit}
          className="flex-1 flex items-center justify-center gap-1 py-1.5 border border-slate-200 hover:bg-slate-50 rounded-xl text-[11px] font-semibold text-slate-700 transition-all"
        >
          <Pencil className="w-3 h-3" /> Edit
        </button>
        <button
          onClick={onDelete}
          className="flex-1 flex items-center justify-center gap-1 py-1.5 border border-rose-200 hover:bg-rose-50 rounded-xl text-[11px] font-semibold text-rose-600 transition-all"
        >
          <Trash2 className="w-3 h-3" /> Delete
        </button>
      </div>
    </div>
  );
};

// ─── Blank form state ─────────────────────────────────────────────────────────
interface StaffForm {
  name: string;
  phone: string;
  email: string;
  password: string;
  role: UserRole;
  status: 'active' | 'inactive';
  permissions: UserPermissions;
}

const blankForm = (): StaffForm => ({
  name: '',
  phone: '',
  email: '',
  password: '',
  role: 'cashier',
  status: 'active',
  permissions: defaultPermsForRole('cashier'),
});

// ─── Permissions matrix for form ─────────────────────────────────────────────
const MODULES = Array.from(new Set(PERMISSION_CATALOG.map((p) => p.module)));

// ─── Main StaffPage ───────────────────────────────────────────────────────────
export const StaffPage: React.FC = () => {
  const { staff, roles, addStaff, updateStaff, deleteStaff } = useStore();
  const { showToast } = useToast();

  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<StaffForm>(blankForm());
  const [submitting, setSubmitting] = useState(false);

  // ── Open modal ──────────────────────────────────────────────────────────────
  const openAdd = () => {
    setEditingId(null);
    setForm(blankForm());
    setModalOpen(true);
  };

  const openEdit = (member: UserStaff) => {
    setEditingId(member.id);
    setForm({
      name: member.name,
      phone: member.phone,
      email: member.email,
      password: '',
      role: member.role,
      status: member.status,
      permissions: { ...member.permissions },
    });
    setModalOpen(true);
  };

  const closeModal = () => {
    setModalOpen(false);
    setEditingId(null);
  };

  // ── Form helpers ────────────────────────────────────────────────────────────
  const handleRoleChange = (role: UserRole) => {
    // Find matching custom role in store first
    const customRole = roles.find((r) => r.name.toLowerCase() === role.toLowerCase());
    const perms = customRole
      ? buildPermissions(customRole.permissions)
      : defaultPermsForRole(role);
    setForm((f) => ({ ...f, role, permissions: perms }));
  };

  const handlePermToggle = (key: keyof UserPermissions) => {
    setForm((f) => ({
      ...f,
      permissions: { ...f.permissions, [key]: !f.permissions[key] },
    }));
  };

  // ── Submit ──────────────────────────────────────────────────────────────────
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name.trim() || !form.phone.trim() || !form.email.trim()) return;
    if (!editingId && !form.password) return;

    setSubmitting(true);
    try {
      if (editingId) {
        await updateStaff(editingId, {
          name: form.name,
          phone: form.phone,
          email: form.email,
          role: form.role,
          status: form.status,
          permissions: form.permissions,
        });
        showToast(`Staff "${form.name}" updated`, 'success');
      } else {
        await addStaff({
          name: form.name,
          phone: form.phone,
          email: form.email,
          password: form.password,
          role: form.role,
          status: form.status,
          permissions: form.permissions,
        });
        showToast(`Staff "${form.name}" added`, 'success');
      }
      closeModal();
    } catch (err: any) {
      showToast(err.message || 'Could not save staff member', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  // ── Delete ──────────────────────────────────────────────────────────────────
  const handleDelete = async (member: UserStaff) => {
    if (!window.confirm(`Delete staff member "${member.name}"? This cannot be undone.`)) return;
    try {
      await deleteStaff(member.id);
      showToast(`"${member.name}" removed from staff`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not delete staff member', 'error');
    }
  };

  // ─── Render ─────────────────────────────────────────────────────────────────
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Staff & Roles</h1>
          <p className="text-xs sm:text-sm text-slate-500">Manage cashier and store staff accounts and their access</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-md shadow-indigo-600/30 transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>Add Staff</span>
        </button>
      </div>

      {/* Staff Grid */}
      {staff.length === 0 ? (
        <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs flex flex-col items-center justify-center py-20 text-slate-400 space-y-3">
          <User className="w-14 h-14 opacity-20" />
          <p className="text-sm font-medium">No staff members yet</p>
          <button
            onClick={openAdd}
            className="flex items-center gap-1 px-4 py-2 bg-indigo-600 text-white rounded-xl text-xs font-semibold shadow-sm"
          >
            <Plus className="w-3.5 h-3.5" /> Add First Staff Member
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {staff.map((member) => (
            <StaffCard
              key={member.id}
              member={member}
              onEdit={() => openEdit(member)}
              onDelete={() => handleDelete(member)}
            />
          ))}
        </div>
      )}

      {/* Add / Edit Modal */}
      <Modal
        isOpen={modalOpen}
        onClose={closeModal}
        title={editingId ? 'Edit Staff Member' : 'Add Staff Member'}
        subtitle={editingId ? 'Update staff details and permissions' : 'Create a new staff account'}
        maxWidth="lg"
        footer={
          <div className="flex justify-end gap-2">
            <button
              type="button"
              onClick={closeModal}
              className="px-4 py-2 text-xs font-semibold text-slate-700 border border-slate-200 rounded-xl hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              form="staff-form"
              type="submit"
              disabled={submitting}
              className="px-5 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white rounded-xl text-xs font-bold shadow-md"
            >
              {submitting ? 'Saving…' : editingId ? 'Save Changes' : 'Create Staff Account'}
            </button>
          </div>
        }
      >
        <form id="staff-form" onSubmit={handleSubmit} className="space-y-4">
          {/* Basic Info */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Full Name *</label>
              <input
                type="text"
                required
                value={form.name}
                onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Phone *</label>
              <input
                type="text"
                required
                value={form.phone}
                onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Email *</label>
              <input
                type="email"
                required
                value={form.email}
                onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            {!editingId && (
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Password *</label>
                <input
                  type="password"
                  required
                  minLength={8}
                  value={form.password}
                  onChange={(e) => setForm((f) => ({ ...f, password: e.target.value }))}
                  className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>
            )}
          </div>

          {/* Role + Status */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Role</label>
              <select
                value={form.role}
                onChange={(e) => handleRoleChange(e.target.value as UserRole)}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold focus:outline-none focus:ring-2 focus:ring-indigo-500"
              >
                <option value="admin">Admin / Owner</option>
                <option value="manager">Store Manager</option>
                <option value="cashier">Cashier</option>
                <option value="sales">Sales Staff</option>
                {roles.filter((r) => !['Admin / Owner', 'Store Manager', 'Cashier / POS Operator'].includes(r.name)).map((r) => (
                  <option key={r.id} value={r.name as any}>{r.name}</option>
                ))}
              </select>
            </div>
            {editingId && (
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Status</label>
                <div className="flex gap-2 mt-1">
                  <button
                    type="button"
                    onClick={() => setForm((f) => ({ ...f, status: 'active' }))}
                    className={`flex-1 py-2 rounded-xl text-xs font-semibold border transition-all ${
                      form.status === 'active'
                        ? 'bg-emerald-50 border-emerald-300 text-emerald-700'
                        : 'border-slate-200 text-slate-500 hover:bg-slate-50'
                    }`}
                  >
                    ● Active
                  </button>
                  <button
                    type="button"
                    onClick={() => setForm((f) => ({ ...f, status: 'inactive' }))}
                    className={`flex-1 py-2 rounded-xl text-xs font-semibold border transition-all ${
                      form.status === 'inactive'
                        ? 'bg-slate-100 border-slate-400 text-slate-700'
                        : 'border-slate-200 text-slate-500 hover:bg-slate-50'
                    }`}
                  >
                    ○ Inactive
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* Permissions matrix */}
          <div className="border-t border-slate-100 pt-3 space-y-3">
            <p className="text-[10px] font-bold text-slate-500 uppercase tracking-wider">Permissions (override per module)</p>
            {MODULES.map((mod) => {
              const perms = PERMISSION_CATALOG.filter((p) => p.module === mod);
              return (
                <div key={mod} className="space-y-1">
                  <p className="text-[10px] font-bold text-slate-400 uppercase">{mod}</p>
                  <div className="flex flex-wrap gap-2">
                    {perms.map((perm) => {
                      const userPermKey = permToUserPerm[perm.key];
                      if (!userPermKey) return null;
                      const checked = form.permissions[userPermKey];
                      return (
                        <label
                          key={perm.key}
                          className={`flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg border cursor-pointer text-[10px] font-semibold transition-all ${
                            checked
                              ? 'border-indigo-300 bg-indigo-50 text-indigo-700'
                              : 'border-slate-200 text-slate-500 hover:bg-slate-50'
                          }`}
                        >
                          <input
                            type="checkbox"
                            checked={checked}
                            onChange={() => handlePermToggle(userPermKey)}
                            className="sr-only"
                          />
                          <span
                            className={`w-3.5 h-3.5 rounded flex items-center justify-center border ${
                              checked ? 'bg-indigo-600 border-indigo-600' : 'border-slate-300'
                            }`}
                          >
                            {checked && (
                              <svg className="w-2.5 h-2.5 text-white" viewBox="0 0 12 12" fill="none">
                                <path d="M2 6l3 3 5-5" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                              </svg>
                            )}
                          </span>
                          {perm.label}
                        </label>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default StaffPage;
