import { useState, useEffect } from 'react';
import { Shield, Plus, Trash2, ChevronRight, Check, X } from 'lucide-react';
import apiClient from '../../services/apiClient';

interface Role {
  id: number;
  name: string;
  description?: string;
  color?: string;
  isSystemRole: boolean;
}

interface Permission {
  id: number;
  name: string;
  module?: string;
  action?: string;
  key: string;
  description?: string;
}

export default function RolesPermissionsPage() {
  const [roles, setRoles] = useState<Role[]>([]);
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [selectedRole, setSelectedRole] = useState<Role | null>(null);
  const [rolePerms, setRolePerms] = useState<number[]>([]);
  const [newRoleName, setNewRoleName] = useState('');
  const [newRoleDesc, setNewRoleDesc] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => { loadRoles(); loadPermissions(); }, []);

  const loadRoles = async () => {
    setLoading(true);
    try {
      const res = await apiClient.get('/staff/roles');
      setRoles(res.data.data || []);
    } catch { /* ignore */ } finally { setLoading(false); }
  };

  const loadPermissions = async () => {
    try {
      const res = await apiClient.get('/staff/permissions');
      setPermissions(res.data.data || []);
    } catch { /* ignore */ }
  };

  const selectRole = async (role: Role) => {
    setSelectedRole(role);
    try {
      const res = await apiClient.get(`/staff/roles/${role.id}/permissions`);
      setRolePerms((res.data.data || []).map((p: any) => p.permissionId || p.id));
    } catch { setRolePerms([]); }
  };

  const createRole = async () => {
    if (!newRoleName.trim()) return;
    await apiClient.post('/staff/roles', { name: newRoleName, description: newRoleDesc });
    setNewRoleName(''); setNewRoleDesc('');
    loadRoles();
  };

  const togglePermission = async (permId: number) => {
    if (!selectedRole) return;
    const has = rolePerms.includes(permId);
    setSaving(true);
    try {
      if (has) {
        await apiClient.delete(`/staff/roles/${selectedRole.id}/permissions/${permId}`);
        setRolePerms(prev => prev.filter(p => p !== permId));
      } else {
        await apiClient.post(`/staff/roles/${selectedRole.id}/permissions`, { permissionId: permId });
        setRolePerms(prev => [...prev, permId]);
      }
    } catch { /* ignore */ } finally { setSaving(false); }
  };

  // Group permissions by module
  const grouped = permissions.reduce((acc, p) => {
    const mod = p.module || 'General';
    if (!acc[mod]) acc[mod] = [];
    acc[mod].push(p);
    return acc;
  }, {} as Record<string, Permission[]>);

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center gap-3">
        <Shield className="w-7 h-7 text-emerald-600" />
        <div>
          <h1 className="text-xl font-bold text-gray-800">Roles & Permissions</h1>
          <p className="text-xs text-gray-500">Create roles and assign permissions to control staff access</p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Roles List */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="font-semibold text-gray-700">Roles</h2>

          <div className="space-y-2">
            <input value={newRoleName} onChange={e => setNewRoleName(e.target.value)}
              placeholder="Role name"
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-emerald-500 focus:outline-none" />
            <input value={newRoleDesc} onChange={e => setNewRoleDesc(e.target.value)}
              placeholder="Description (optional)"
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-emerald-500 focus:outline-none" />
            <button onClick={createRole}
              className="w-full bg-emerald-600 text-white py-2 rounded-lg hover:bg-emerald-700 text-sm flex items-center justify-center gap-1">
              <Plus className="w-4 h-4" /> Create Role
            </button>
          </div>

          <div className="space-y-2 max-h-80 overflow-y-auto">
            {loading ? (
              <p className="text-gray-400 text-sm">Loading...</p>
            ) : (
              roles.map(role => (
                <button key={role.id} onClick={() => selectRole(role)}
                  className={`w-full text-left px-3 py-2.5 rounded-lg border text-sm flex items-center justify-between transition-colors
                    ${selectedRole?.id === role.id ? 'border-emerald-400 bg-emerald-50 text-emerald-700' : 'border-gray-200 hover:bg-gray-50'}`}>
                  <div>
                    <div className="font-medium">{role.name}</div>
                    {role.isSystemRole && <span className="text-xs bg-blue-100 text-blue-600 px-1.5 py-0.5 rounded">System</span>}
                  </div>
                  <ChevronRight className="w-4 h-4 flex-shrink-0" />
                </button>
              ))
            )}
          </div>
        </div>

        {/* Permissions for selected role */}
        <div className="lg:col-span-2 bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          {selectedRole ? (
            <>
              <div className="flex items-center justify-between">
                <h2 className="font-semibold text-gray-700">Permissions — <span className="text-emerald-600">{selectedRole.name}</span></h2>
                {saving && <span className="text-xs text-gray-400 animate-pulse">Saving...</span>}
              </div>

              {Object.entries(grouped).map(([module, perms]) => (
                <div key={module} className="space-y-2">
                  <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wide">{module}</h3>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                    {perms.map(perm => {
                      const granted = rolePerms.includes(perm.id);
                      return (
                        <button key={perm.id} onClick={() => togglePermission(perm.id)}
                          className={`flex items-center gap-2 px-3 py-2 rounded-lg border text-sm transition-colors
                            ${granted ? 'border-emerald-300 bg-emerald-50 text-emerald-700' : 'border-gray-200 text-gray-600 hover:bg-gray-50'}`}>
                          {granted ? <Check className="w-4 h-4 text-emerald-500" /> : <X className="w-4 h-4 text-gray-300" />}
                          <span>{perm.name}</span>
                          {perm.action && <span className="ml-auto text-xs bg-gray-100 text-gray-500 px-1.5 py-0.5 rounded">{perm.action}</span>}
                        </button>
                      );
                    })}
                  </div>
                </div>
              ))}

              {permissions.length === 0 && (
                <p className="text-gray-400 text-sm text-center py-8">No permissions defined yet</p>
              )}
            </>
          ) : (
            <div className="flex flex-col items-center justify-center h-48 text-gray-400">
              <Shield className="w-12 h-12 mb-3 opacity-30" />
              <p className="text-sm">Select a role to manage its permissions</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

