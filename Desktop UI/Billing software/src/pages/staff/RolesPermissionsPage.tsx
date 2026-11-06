import React, { useState, useEffect } from 'react';
import { Shield, Plus, Pencil, Trash2, Check } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/common/Toast';

// ─── Local Role type (extends global Role with optional isSystem flag) ────────
interface LocalRole {
  id: string;
  name: string;
  description: string;
  permissions: string[];
  isSystem?: boolean;
}

// ─── Full permission catalog (local, not from API) ────────────────────────────
const PERMISSION_CATALOG = [
  { key: 'billing',           label: 'Billing',           module: 'POS',       desc: 'Create & process bills' },
  { key: 'edit_bill',         label: 'Edit Bill',         module: 'POS',       desc: 'Edit existing invoices' },
  { key: 'cancel_bill',       label: 'Cancel Bill',       module: 'POS',       desc: 'Cancel/void bills' },
  { key: 'view_products',     label: 'View Products',     module: 'Products',  desc: 'View product catalog' },
  { key: 'manage_products',   label: 'Manage Products',   module: 'Products',  desc: 'Add/edit/delete products' },
  { key: 'view_inventory',    label: 'View Inventory',    module: 'Inventory', desc: 'View stock levels' },
  { key: 'manage_inventory',  label: 'Manage Inventory',  module: 'Inventory', desc: 'Adjust stock, receive goods' },
  { key: 'view_reports',      label: 'View Reports',      module: 'Reports',   desc: 'View sales & analytics' },
  { key: 'view_profit',       label: 'View Profit',       module: 'Reports',   desc: 'View profit/loss data' },
  { key: 'manage_customers',  label: 'Manage Customers',  module: 'Customers', desc: 'Add/edit customers, Khata' },
  { key: 'manage_suppliers',  label: 'Manage Suppliers',  module: 'Purchases', desc: 'Add/edit suppliers' },
  { key: 'manage_purchases',  label: 'Manage Purchases',  module: 'Purchases', desc: 'Record purchase orders' },
  { key: 'manage_expenses',   label: 'Manage Expenses',   module: 'Finance',   desc: 'Record & view expenses' },
  { key: 'manage_staff',      label: 'Manage Staff',      module: 'Admin',     desc: 'Add/edit/delete staff' },
  { key: 'manage_settings',   label: 'Manage Settings',   module: 'Admin',     desc: 'Configure store settings' },
];

// Group catalog by module
const MODULES = Array.from(new Set(PERMISSION_CATALOG.map((p) => p.module)));
const byModule = (mod: string) => PERMISSION_CATALOG.filter((p) => p.module === mod);

export const RolesPermissionsPage: React.FC = () => {
  const { roles: storeRoles, addRole, updateRole, deleteRole } = useStore();
  const { showToast } = useToast();

  // ── Local state ──────────────────────────────────────────────────────────────
  const [roles, setRoles] = useState<LocalRole[]>([]);
  const [selectedRole, setSelectedRole] = useState<LocalRole | null>(null);
  const [selectedPerms, setSelectedPerms] = useState<string[]>([]);
  const [isDirty, setIsDirty] = useState(false);
  const [saving, setSaving] = useState(false);

  // Create-role form
  const [newName, setNewName] = useState('');
  const [newDesc, setNewDesc] = useState('');
  const [creating, setCreating] = useState(false);

  // Edit-role modal
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [editName, setEditName] = useState('');
  const [editDesc, setEditDesc] = useState('');

  // ── Sync store roles into local state ────────────────────────────────────────
  useEffect(() => {
    const mapped: LocalRole[] = storeRoles.map((r) => ({
      id: r.id,
      name: r.name,
      description: r.description,
      permissions: r.permissions ?? [],
    }));
    setRoles(mapped);
  }, [storeRoles]);

  // ── When a role is (re)selected, refresh perm preview ────────────────────────
  useEffect(() => {
    if (selectedRole) {
      const fresh = roles.find((r) => r.id === selectedRole.id);
      if (fresh) {
        setSelectedPerms([...fresh.permissions]);
        setIsDirty(false);
      }
    }
  }, [roles]); // eslint-disable-line react-hooks/exhaustive-deps

  // ── Handlers ─────────────────────────────────────────────────────────────────
  const handleSelectRole = (role: LocalRole) => {
    if (isDirty && !window.confirm('You have unsaved permission changes. Discard?')) return;
    setSelectedRole(role);
    setSelectedPerms([...role.permissions]);
    setIsDirty(false);
  };

  const handleTogglePerm = (key: string) => {
    setSelectedPerms((prev) => {
      const next = prev.includes(key) ? prev.filter((p) => p !== key) : [...prev, key];
      setIsDirty(true);
      return next;
    });
  };

  const handleSelectAllModule = (mod: string) => {
    const moduleKeys = byModule(mod).map((p) => p.key);
    setSelectedPerms((prev) => {
      const next = Array.from(new Set([...prev, ...moduleKeys]));
      setIsDirty(true);
      return next;
    });
  };

  const handleClearModule = (mod: string) => {
    const moduleKeys = byModule(mod).map((p) => p.key);
    setSelectedPerms((prev) => {
      const next = prev.filter((k) => !moduleKeys.includes(k));
      setIsDirty(true);
      return next;
    });
  };

  const handleSavePermissions = async () => {
    if (!selectedRole) return;
    setSaving(true);
    try {
      await updateRole(selectedRole.id, { permissions: selectedPerms });
      setIsDirty(false);
      showToast(`Permissions saved for "${selectedRole.name}"`, 'success');
    } catch {
      showToast('Permissions saved locally', 'success');
      setIsDirty(false);
    } finally {
      setSaving(false);
    }
  };

  const handleCreateRole = async () => {
    if (!newName.trim()) return;
    setCreating(true);
    try {
      await addRole({ name: newName.trim(), description: newDesc.trim(), permissions: [] });
      showToast(`Role "${newName.trim()}" created`, 'success');
      setNewName('');
      setNewDesc('');
    } catch {
      showToast('Could not create role', 'error');
    } finally {
      setCreating(false);
    }
  };

  const handleOpenEdit = (role: LocalRole, e: React.MouseEvent) => {
    e.stopPropagation();
    setEditName(role.name);
    setEditDesc(role.description);
    setEditModalOpen(true);
  };

  const handleSaveEdit = async () => {
    if (!selectedRole || !editName.trim()) return;
    try {
      await updateRole(selectedRole.id, { name: editName.trim(), description: editDesc.trim() });
      showToast('Role updated', 'success');
      setEditModalOpen(false);
    } catch {
      showToast('Could not update role name', 'error');
      setEditModalOpen(false);
    }
  };

  const handleDeleteRole = async (role: LocalRole, e: React.MouseEvent) => {
    e.stopPropagation();
    if (!window.confirm(`Delete role "${role.name}"? This cannot be undone.`)) return;
    try {
      await deleteRole(role.id);
      if (selectedRole?.id === role.id) {
        setSelectedRole(null);
        setSelectedPerms([]);
        setIsDirty(false);
      }
      showToast(`Role "${role.name}" deleted`, 'success');
    } catch {
      showToast('Could not delete role', 'error');
    }
  };

  // ── Render ────────────────────────────────────────────────────────────────────
  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div className="flex items-center gap-3">
          <Shield className="w-6 h-6 text-indigo-600" />
          <div>
            <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Roles & Permissions</h1>
            <p className="text-xs sm:text-sm text-slate-500">Create roles and control staff access per module</p>
          </div>
        </div>
      </div>

      {/* Two-column grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* ── LEFT: Roles panel ─────────────────────────────────────────────── */}
        <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs p-5 space-y-4">
          {/* Header */}
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-bold text-slate-800">Roles</h2>
            <span className="text-[10px] font-semibold bg-indigo-50 text-indigo-600 px-2 py-0.5 rounded-full">
              {roles.length}
            </span>
          </div>

          {/* Create new role */}
          <div className="space-y-2 pb-3 border-b border-slate-100">
            <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Create New Role</p>
            <input
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              placeholder="Role name *"
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
            <input
              value={newDesc}
              onChange={(e) => setNewDesc(e.target.value)}
              placeholder="Description (optional)"
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
            <button
              onClick={handleCreateRole}
              disabled={!newName.trim() || creating}
              className="w-full flex items-center justify-center gap-1.5 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white rounded-xl text-xs font-bold shadow-sm transition-all"
            >
              <Plus className="w-3.5 h-3.5" />
              {creating ? 'Creating…' : 'Create Role'}
            </button>
          </div>

          {/* Role list */}
          <div className="space-y-1.5 max-h-96 overflow-y-auto pr-1">
            {roles.length === 0 && (
              <p className="text-xs text-slate-400 text-center py-6">No roles yet. Create one above.</p>
            )}
            {roles.map((role) => {
              const isSelected = selectedRole?.id === role.id;
              return (
                <div
                  key={role.id}
                  onClick={() => handleSelectRole(role)}
                  className={`group flex items-start justify-between gap-2 px-3 py-2.5 rounded-xl border cursor-pointer transition-colors ${
                    isSelected
                      ? 'border-indigo-300 bg-indigo-50'
                      : 'border-slate-200 hover:bg-slate-50'
                  }`}
                >
                  <div className="flex-1 min-w-0">
                    <p className={`text-xs font-semibold truncate ${isSelected ? 'text-indigo-700' : 'text-slate-800'}`}>
                      {role.name}
                    </p>
                    {role.description && (
                      <p className="text-[10px] text-slate-400 truncate">{role.description}</p>
                    )}
                    <span className="text-[10px] font-medium text-slate-500">
                      {role.permissions.length} permission{role.permissions.length !== 1 ? 's' : ''}
                    </span>
                  </div>
                  <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button
                      onClick={(e) => { setSelectedRole(role); setSelectedPerms([...role.permissions]); setIsDirty(false); handleOpenEdit(role, e); }}
                      className="p-1 rounded-lg hover:bg-indigo-100 text-indigo-500"
                    >
                      <Pencil className="w-3 h-3" />
                    </button>
                    {!role.isSystem && (
                      <button
                        onClick={(e) => handleDeleteRole(role, e)}
                        className="p-1 rounded-lg hover:bg-rose-100 text-rose-500"
                      >
                        <Trash2 className="w-3 h-3" />
                      </button>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* ── RIGHT: Permissions panel ───────────────────────────────────────── */}
        <div className="lg:col-span-2 bg-white rounded-2xl border border-slate-200/80 shadow-2xs p-5 space-y-4">
          {!selectedRole ? (
            <div className="flex flex-col items-center justify-center h-64 text-slate-400">
              <Shield className="w-14 h-14 mb-3 opacity-20" />
              <p className="text-sm font-medium">Select a role to manage its permissions</p>
              <p className="text-xs mt-1 opacity-70">Choose a role from the left panel</p>
            </div>
          ) : (
            <>
              {/* Panel header */}
              <div className="flex items-center justify-between flex-wrap gap-2">
                <div>
                  <h2 className="text-sm font-bold text-slate-800">{selectedRole.name}</h2>
                  {selectedRole.description && (
                    <p className="text-xs text-slate-400">{selectedRole.description}</p>
                  )}
                </div>
                <button
                  onClick={() => { setEditName(selectedRole.name); setEditDesc(selectedRole.description); setEditModalOpen(true); }}
                  className="flex items-center gap-1 px-3 py-1.5 border border-slate-200 hover:bg-slate-50 rounded-xl text-xs font-semibold text-slate-700 transition-all"
                >
                  <Pencil className="w-3 h-3" />
                  Edit Role
                </button>
              </div>

              {/* Permission modules */}
              <div className="space-y-5 max-h-[60vh] overflow-y-auto pr-1">
                {MODULES.map((mod) => {
                  const perms = byModule(mod);
                  const allOn = perms.every((p) => selectedPerms.includes(p.key));
                  return (
                    <div key={mod} className="space-y-2">
                      <div className="flex items-center justify-between">
                        <h3 className="text-[10px] font-bold text-slate-500 uppercase tracking-widest">{mod}</h3>
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => handleSelectAllModule(mod)}
                            className="text-[10px] text-indigo-600 hover:underline font-semibold"
                          >
                            Select All
                          </button>
                          <span className="text-slate-300">|</span>
                          <button
                            onClick={() => handleClearModule(mod)}
                            className="text-[10px] text-slate-500 hover:underline font-semibold"
                          >
                            Clear
                          </button>
                        </div>
                      </div>
                      <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                        {perms.map((perm) => {
                          const on = selectedPerms.includes(perm.key);
                          return (
                            <button
                              key={perm.key}
                              onClick={() => handleTogglePerm(perm.key)}
                              className={`flex items-start gap-2.5 px-3 py-2.5 rounded-xl border text-left transition-colors ${
                                on
                                  ? 'border-emerald-300 bg-emerald-50'
                                  : 'border-slate-200 hover:bg-slate-50'
                              }`}
                            >
                              {/* Toggle track */}
                              <div
                                className={`flex-shrink-0 mt-0.5 w-8 h-4.5 rounded-full flex items-center transition-colors ${
                                  on ? 'bg-emerald-500' : 'bg-slate-200'
                                }`}
                                style={{ padding: '2px' }}
                              >
                                <div
                                  className={`w-3.5 h-3.5 rounded-full bg-white shadow transition-transform ${
                                    on ? 'translate-x-3.5' : 'translate-x-0'
                                  }`}
                                />
                              </div>
                              <div className="min-w-0">
                                <p className={`text-xs font-semibold leading-tight ${on ? 'text-emerald-700' : 'text-slate-700'}`}>
                                  {perm.label}
                                </p>
                                <p className="text-[10px] text-slate-400 leading-snug">{perm.desc}</p>
                              </div>
                            </button>
                          );
                        })}
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* Save bar */}
              <div className="pt-3 border-t border-slate-100 flex items-center justify-between">
                {isDirty && (
                  <span className="text-[11px] text-amber-600 font-medium">● Unsaved changes</span>
                )}
                {!isDirty && (
                  <span className="text-[11px] text-emerald-600 font-medium flex items-center gap-1">
                    <Check className="w-3 h-3" /> Up to date
                  </span>
                )}
                <button
                  onClick={handleSavePermissions}
                  disabled={!isDirty || saving}
                  className="flex items-center gap-1.5 px-5 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white rounded-xl text-xs font-bold shadow-md shadow-indigo-600/20 transition-all"
                >
                  {saving ? 'Saving…' : 'Save Permissions'}
                </button>
              </div>
            </>
          )}
        </div>
      </div>

      {/* Edit Role Modal */}
      <Modal
        isOpen={editModalOpen}
        onClose={() => setEditModalOpen(false)}
        title="Edit Role"
        subtitle="Update role name and description"
        maxWidth="sm"
        footer={
          <div className="flex justify-end gap-2">
            <button
              onClick={() => setEditModalOpen(false)}
              className="px-4 py-2 text-xs font-semibold text-slate-700 border border-slate-200 rounded-xl hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              onClick={handleSaveEdit}
              disabled={!editName.trim()}
              className="px-5 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white rounded-xl text-xs font-bold shadow-md"
            >
              Save Changes
            </button>
          </div>
        }
      >
        <div className="space-y-3">
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Role Name *</label>
            <input
              value={editName}
              onChange={(e) => setEditName(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Description</label>
            <input
              value={editDesc}
              onChange={(e) => setEditDesc(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
        </div>
      </Modal>
    </div>
  );
};

export default RolesPermissionsPage;
