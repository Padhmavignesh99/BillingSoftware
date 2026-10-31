import React, { useState, useEffect, useCallback } from 'react';
import { Activity, Search, Filter, Trash2, ChevronDown, ChevronRight, RefreshCw } from 'lucide-react';
import { api } from '../../services/apiClient';

interface AuditLog {
  id: number;
  userId?: number;
  userName?: string;
  action: string;
  entityType: string;
  entityId?: number;
  oldValues?: string;
  newValues?: string;
  ipAddress?: string;
  timestamp: string;
}

const ACTION_COLORS: Record<string, string> = {
  Create: 'bg-emerald-100 text-emerald-700',
  Update: 'bg-blue-100 text-blue-700',
  Delete: 'bg-rose-100 text-rose-700',
  Login: 'bg-violet-100 text-violet-700',
  Logout: 'bg-slate-100 text-slate-600',
};

export const AuditLogPage: React.FC = () => {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [entityType, setEntityType] = useState('');
  const [action, setAction] = useState('');
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [entityTypes, setEntityTypes] = useState<string[]>([]);
  const [expanded, setExpanded] = useState<number | null>(null);
  const [clearing, setClearing] = useState(false);
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
      if (search) params.set('search', search);
      if (entityType) params.set('entityType', entityType);
      if (action) params.set('action', action);
      if (from) params.set('from', from);
      if (to) params.set('to', to);
      const res = await api.request<any>(`/api/audit-logs?${params}`);
      setLogs(Array.isArray(res) ? res : []);
      setTotal(res?.totalCount ?? 0);
    } catch { setLogs([]); }
    setLoading(false);
  }, [page, search, entityType, action, from, to]);

  useEffect(() => { load(); }, [load]);
  useEffect(() => {
    api.request<string[]>('/api/audit-logs/entity-types').then(setEntityTypes).catch(() => {});
  }, []);

  const clearLogs = async (days: number) => {
    setClearing(true);
    try {
      await api.request(`/api/audit-logs/clear?olderThanDays=${days}`, { method: 'DELETE' });
      load();
    } catch { }
    setClearing(false);
  };

  const totalPages = Math.ceil(total / pageSize);

  return (
    <div className="p-4 md:p-6 space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-violet-100 rounded-xl flex items-center justify-center">
            <Activity className="w-5 h-5 text-violet-600" />
          </div>
          <div>
            <h1 className="text-lg font-bold text-slate-800">Activity Log</h1>
            <p className="text-xs text-slate-500">{total} total audit events</p>
          </div>
        </div>
        <div className="flex gap-2">
          <button onClick={load} className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50">
            <RefreshCw className="w-3.5 h-3.5" /> Refresh
          </button>
          <button onClick={() => clearLogs(90)} disabled={clearing} className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-rose-600 border border-rose-200 rounded-lg hover:bg-rose-50 disabled:opacity-50">
            <Trash2 className="w-3.5 h-3.5" /> Clear Old (&gt;90d)
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white border border-green-100 rounded-xl p-3 flex flex-wrap gap-2 items-center">
        <div className="relative flex-1 min-w-[180px]">
          <Search className="w-3.5 h-3.5 absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            value={search} onChange={e => { setSearch(e.target.value); setPage(1); }}
            placeholder="Search actions..."
            className="pl-8 pr-3 py-1.5 text-xs border border-slate-200 rounded-lg w-full focus:ring-1 focus:ring-emerald-400 outline-none"
          />
        </div>
        <select value={entityType} onChange={e => { setEntityType(e.target.value); setPage(1); }} className="px-2 py-1.5 text-xs border border-slate-200 rounded-lg text-slate-700 focus:ring-1 focus:ring-emerald-400 outline-none">
          <option value="">All Types</option>
          {entityTypes.map(t => <option key={t} value={t}>{t}</option>)}
        </select>
        <select value={action} onChange={e => { setAction(e.target.value); setPage(1); }} className="px-2 py-1.5 text-xs border border-slate-200 rounded-lg text-slate-700 focus:ring-1 focus:ring-emerald-400 outline-none">
          <option value="">All Actions</option>
          {['Create', 'Update', 'Delete', 'Login', 'Logout'].map(a => <option key={a} value={a}>{a}</option>)}
        </select>
        <input type="date" value={from} onChange={e => { setFrom(e.target.value); setPage(1); }} className="px-2 py-1.5 text-xs border border-slate-200 rounded-lg text-slate-700 focus:ring-1 focus:ring-emerald-400 outline-none" />
        <input type="date" value={to} onChange={e => { setTo(e.target.value); setPage(1); }} className="px-2 py-1.5 text-xs border border-slate-200 rounded-lg text-slate-700 focus:ring-1 focus:ring-emerald-400 outline-none" />
      </div>

      {/* Table */}
      <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
        {loading ? (
          <div className="p-10 text-center text-sm text-slate-400">Loading...</div>
        ) : logs.length === 0 ? (
          <div className="p-10 text-center text-sm text-slate-400">No audit logs found</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="bg-slate-50 border-b border-slate-100">
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600 w-6"></th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Timestamp</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Action</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Entity</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">ID</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">User</th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">IP</th>
                </tr>
              </thead>
              <tbody>
                {logs.map(log => (
                  <React.Fragment key={log.id}>
                    <tr
                      className="border-b border-slate-50 hover:bg-slate-50 cursor-pointer"
                      onClick={() => setExpanded(expanded === log.id ? null : log.id)}
                    >
                      <td className="px-3 py-2 text-slate-400">
                        {expanded === log.id ? <ChevronDown className="w-3.5 h-3.5" /> : <ChevronRight className="w-3.5 h-3.5" />}
                      </td>
                      <td className="px-3 py-2 text-slate-500 whitespace-nowrap">
                        {new Date(log.timestamp).toLocaleString('en-IN', { dateStyle: 'short', timeStyle: 'short' })}
                      </td>
                      <td className="px-3 py-2">
                        <span className={`px-2 py-0.5 rounded-md font-medium text-[11px] ${ACTION_COLORS[log.action] || 'bg-slate-100 text-slate-600'}`}>
                          {log.action}
                        </span>
                      </td>
                      <td className="px-3 py-2 font-medium text-slate-700">{log.entityType}</td>
                      <td className="px-3 py-2 text-slate-500">{log.entityId ?? '-'}</td>
                      <td className="px-3 py-2 text-slate-500">{log.userName || log.userId || '-'}</td>
                      <td className="px-3 py-2 text-slate-400">{log.ipAddress || '-'}</td>
                    </tr>
                    {expanded === log.id && (log.oldValues || log.newValues) && (
                      <tr className="bg-slate-50 border-b border-slate-100">
                        <td colSpan={7} className="px-6 py-3">
                          <div className="grid grid-cols-2 gap-4">
                            {log.oldValues && (
                              <div>
                                <p className="text-[10px] font-semibold text-slate-500 uppercase mb-1">Before</p>
                                <pre className="text-[10px] text-rose-600 bg-rose-50 border border-rose-100 rounded-lg p-2 overflow-auto max-h-32 whitespace-pre-wrap">
                                  {log.oldValues}
                                </pre>
                              </div>
                            )}
                            {log.newValues && (
                              <div>
                                <p className="text-[10px] font-semibold text-slate-500 uppercase mb-1">After</p>
                                <pre className="text-[10px] text-emerald-700 bg-emerald-50 border border-emerald-100 rounded-lg p-2 overflow-auto max-h-32 whitespace-pre-wrap">
                                  {log.newValues}
                                </pre>
                              </div>
                            )}
                          </div>
                        </td>
                      </tr>
                    )}
                  </React.Fragment>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between text-xs text-slate-500">
          <span>Page {page} of {totalPages} ({total} records)</span>
          <div className="flex gap-1">
            <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1.5 border border-slate-200 rounded-lg disabled:opacity-40 hover:bg-slate-50">Prev</button>
            <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="px-3 py-1.5 border border-slate-200 rounded-lg disabled:opacity-40 hover:bg-slate-50">Next</button>
          </div>
        </div>
      )}
    </div>
  );
};

