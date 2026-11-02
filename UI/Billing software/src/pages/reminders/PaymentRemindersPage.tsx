import React, { useState, useEffect, useCallback } from 'react';
import { Bell, Plus, Send, Trash2, Edit2, Users, Clock, CheckCircle, XCircle, AlertCircle } from 'lucide-react';
import { api } from '../../services/apiClient';

interface ReminderRule { id: number; name: string; daysOverdue: number; sendWhatsApp: boolean; sendEmail: boolean; sendSms: boolean; messageTemplate: string; isEnabled: boolean; sortOrder: number; }
interface OverdueCustomer { customerId: number; customerName: string; phone?: string; email?: string; totalOverdue: number; overdueInvoiceCount: number; maxDaysOverdue: number; }
interface ReminderLog { id: number; customerId: number; customerName?: string; invoiceId?: number; channel?: string; status?: string; message?: string; sentAt: string; }

type Tab = 'overdue' | 'rules' | 'log';

export const PaymentRemindersPage: React.FC = () => {
  const [tab, setTab] = useState<Tab>('overdue');
  const [overdue, setOverdue] = useState<OverdueCustomer[]>([]);
  const [rules, setRules] = useState<ReminderRule[]>([]);
  const [logs, setLogs] = useState<ReminderLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<Set<number>>(new Set());
  const [sending, setSending] = useState(false);
  const [showRuleModal, setShowRuleModal] = useState(false);
  const [editRule, setEditRule] = useState<ReminderRule | null>(null);
  const [ruleForm, setRuleForm] = useState({ name: '', daysOverdue: 0, sendWhatsApp: true, sendEmail: false, sendSms: false, messageTemplate: 'Dear {CustomerName}, your invoice {InvoiceNumber} of ₹{Amount} is overdue by {Days} days. Please pay at the earliest. Thank you.', isEnabled: true, sortOrder: 0 });

  const loadOverdue = useCallback(async () => {
    try { setOverdue(await api.request<OverdueCustomer[]>('/api/reminders/overdue-customers')); } catch { setOverdue([]); }
  }, []);

  const loadRules = useCallback(async () => {
    try { setRules(await api.request<ReminderRule[]>('/api/reminders/rules')); } catch { setRules([]); }
  }, []);

  const loadLogs = useCallback(async () => {
    try {
      const res = await api.request<ReminderLog[]>('/api/reminders/logs?page=1&pageSize=50');
      setLogs(Array.isArray(res) ? res : []);
    } catch { setLogs([]); }
  }, []);

  useEffect(() => {
    setLoading(true);
    Promise.all([loadOverdue(), loadRules()]).finally(() => setLoading(false));
  }, [loadOverdue, loadRules]);

  useEffect(() => { if (tab === 'log') loadLogs(); }, [tab, loadLogs]);

  const sendBulkReminders = async () => {
    setSending(true);
    let count = 0;
    for (const customerId of selected) {
      try {
        await api.request('/api/reminders/send-manual', { method: 'POST', body: JSON.stringify({ customerId, sendWhatsApp: true }) });
        count++;
      } catch { }
    }
    setSending(false);
    setSelected(new Set());
    alert(`Sent reminders to ${count} customers`);
    loadLogs();
  };

  const openRuleModal = (rule?: ReminderRule) => {
    if (rule) { setEditRule(rule); setRuleForm({ name: rule.name, daysOverdue: rule.daysOverdue, sendWhatsApp: rule.sendWhatsApp, sendEmail: rule.sendEmail, sendSms: rule.sendSms, messageTemplate: rule.messageTemplate, isEnabled: rule.isEnabled, sortOrder: rule.sortOrder }); }
    else { setEditRule(null); setRuleForm({ name: '', daysOverdue: 0, sendWhatsApp: true, sendEmail: false, sendSms: false, messageTemplate: 'Dear {CustomerName}, your invoice {InvoiceNumber} of ₹{Amount} is overdue. Please pay at the earliest.', isEnabled: true, sortOrder: rules.length }); }
    setShowRuleModal(true);
  };

  const saveRule = async () => {
    try {
      if (editRule) await api.request(`/api/reminders/rules/${editRule.id}`, { method: 'PUT', body: JSON.stringify(ruleForm) });
      else await api.request('/api/reminders/rules', { method: 'POST', body: JSON.stringify(ruleForm) });
      setShowRuleModal(false);
      loadRules();
    } catch { }
  };

  const deleteRule = async (id: number) => {
    if (!confirm('Delete this rule?')) return;
    await api.request(`/api/reminders/rules/${id}`, { method: 'DELETE' });
    loadRules();
  };

  return (
    <div className="p-4 md:p-6 space-y-4">
      {/* Header */}
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 bg-rose-100 rounded-xl flex items-center justify-center"><Bell className="w-5 h-5 text-rose-500" /></div>
        <div><h1 className="text-lg font-bold text-slate-800">Payment Reminders</h1><p className="text-xs text-slate-500">Automate overdue payment follow-ups</p></div>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 p-1 bg-slate-100 rounded-xl w-fit">
        {([['overdue', `Overdue (${overdue.length})`], ['rules', 'Reminder Rules'], ['log', 'Activity Log']] as [Tab, string][]).map(([t, label]) => (
          <button key={t} onClick={() => setTab(t)} className={`px-4 py-1.5 text-xs font-semibold rounded-lg transition-all ${tab === t ? 'bg-white text-emerald-700 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}>{label}</button>
        ))}
      </div>

      {/* Overdue Customers */}
      {tab === 'overdue' && (
        <div className="space-y-3">
          {selected.size > 0 && (
            <div className="flex items-center justify-between bg-amber-50 border border-amber-100 rounded-xl px-4 py-2">
              <span className="text-xs text-amber-700 font-medium">{selected.size} customers selected</span>
              <button onClick={sendBulkReminders} disabled={sending} className="flex items-center gap-1.5 px-3 py-1.5 bg-amber-600 text-white text-xs font-semibold rounded-lg hover:bg-amber-700 disabled:opacity-50">
                <Send className="w-3.5 h-3.5" /> {sending ? 'Sending...' : 'Send WhatsApp Reminder'}
              </button>
            </div>
          )}
          {loading ? <div className="p-8 text-center text-sm text-slate-400">Loading...</div> : overdue.length === 0 ? (
            <div className="bg-white border border-green-100 rounded-xl p-10 text-center">
              <CheckCircle className="w-10 h-10 text-emerald-300 mx-auto mb-3" />
              <p className="text-sm font-semibold text-slate-600">No overdue payments!</p>
              <p className="text-xs text-slate-400 mt-1">All customers are up to date.</p>
            </div>
          ) : (
            <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
              <table className="w-full text-xs">
                <thead><tr className="bg-slate-50 border-b border-slate-100">
                  <th className="px-3 py-2.5 w-8"><input type="checkbox" checked={selected.size === overdue.length} onChange={() => selected.size === overdue.length ? setSelected(new Set()) : setSelected(new Set(overdue.map(o => o.customerId)))} className="rounded border-slate-300 text-emerald-600" /></th>
                  <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Customer</th>
                  <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Invoices</th>
                  <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Total Overdue</th>
                  <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Max Days</th>
                  <th className="px-3 py-2.5 font-semibold text-slate-600">Action</th>
                </tr></thead>
                <tbody>
                  {overdue.map(c => (
                    <tr key={c.customerId} className={`border-b border-slate-50 hover:bg-slate-50 ${c.maxDaysOverdue > 30 ? 'bg-rose-50/30' : ''}`}>
                      <td className="px-3 py-2.5"><input type="checkbox" checked={selected.has(c.customerId)} onChange={() => { setSelected(s => { const n = new Set(s); n.has(c.customerId) ? n.delete(c.customerId) : n.add(c.customerId); return n; }); }} className="rounded border-slate-300 text-emerald-600" /></td>
                      <td className="px-3 py-2.5">
                        <p className="font-semibold text-slate-800">{c.customerName}</p>
                        <p className="text-slate-400">{c.phone || c.email || '-'}</p>
                      </td>
                      <td className="px-3 py-2.5 text-right text-slate-600">{c.overdueInvoiceCount}</td>
                      <td className="px-3 py-2.5 text-right font-bold text-rose-600">₹{c.totalOverdue.toLocaleString('en-IN')}</td>
                      <td className={`px-3 py-2.5 text-right font-semibold ${c.maxDaysOverdue > 30 ? 'text-rose-600' : c.maxDaysOverdue > 7 ? 'text-amber-600' : 'text-slate-600'}`}>{c.maxDaysOverdue}d</td>
                      <td className="px-3 py-2.5">
                        <button onClick={() => { setSelected(new Set([c.customerId])); sendBulkReminders(); }} className="flex items-center gap-1 px-2 py-1 text-[11px] font-medium text-emerald-700 border border-emerald-200 rounded-lg hover:bg-emerald-50">
                          <Send className="w-3 h-3" /> Remind
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* Reminder Rules */}
      {tab === 'rules' && (
        <div className="space-y-3">
          <div className="flex justify-end">
            <button onClick={() => openRuleModal()} className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 text-white text-xs font-semibold rounded-lg hover:bg-emerald-700">
              <Plus className="w-3.5 h-3.5" /> Add Rule
            </button>
          </div>
          {rules.length === 0 ? (
            <div className="bg-white border border-green-100 rounded-xl p-8 text-center text-sm text-slate-400">No reminder rules configured</div>
          ) : rules.map(rule => (
            <div key={rule.id} className="bg-white border border-green-100 rounded-xl p-4">
              <div className="flex items-start justify-between">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <p className="text-sm font-semibold text-slate-800">{rule.name}</p>
                    <span className={`text-[10px] px-1.5 py-0.5 rounded font-semibold ${rule.isEnabled ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-500'}`}>{rule.isEnabled ? 'Active' : 'Inactive'}</span>
                  </div>
                  <p className="text-xs text-slate-500 mb-2">Trigger: {rule.daysOverdue === 0 ? 'On due date' : `${rule.daysOverdue} days overdue`}</p>
                  <div className="flex gap-2 mb-2">
                    {rule.sendWhatsApp && <span className="px-2 py-0.5 text-[10px] font-medium bg-green-100 text-green-700 rounded">WhatsApp</span>}
                    {rule.sendEmail && <span className="px-2 py-0.5 text-[10px] font-medium bg-blue-100 text-blue-700 rounded">Email</span>}
                    {rule.sendSms && <span className="px-2 py-0.5 text-[10px] font-medium bg-violet-100 text-violet-700 rounded">SMS</span>}
                  </div>
                  <p className="text-xs text-slate-400 italic">"{rule.messageTemplate.slice(0, 80)}..."</p>
                </div>
                <div className="flex gap-1.5">
                  <button onClick={() => openRuleModal(rule)} className="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg"><Edit2 className="w-3.5 h-3.5" /></button>
                  <button onClick={() => deleteRule(rule.id)} className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg"><Trash2 className="w-3.5 h-3.5" /></button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Activity Log */}
      {tab === 'log' && (
        <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
          <table className="w-full text-xs">
            <thead><tr className="bg-slate-50 border-b border-slate-100">
              <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Time</th>
              <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Customer</th>
              <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Channel</th>
              <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Status</th>
            </tr></thead>
            <tbody>
              {logs.length === 0 ? <tr><td colSpan={4} className="px-3 py-8 text-center text-slate-400">No reminder logs yet</td></tr> : logs.map(log => (
                <tr key={log.id} className="border-b border-slate-50 hover:bg-slate-50">
                  <td className="px-3 py-2.5 text-slate-500">{new Date(log.sentAt).toLocaleString('en-IN', { dateStyle: 'short', timeStyle: 'short' })}</td>
                  <td className="px-3 py-2.5 font-medium text-slate-700">{log.customerName || log.customerId}</td>
                  <td className="px-3 py-2.5"><span className="px-1.5 py-0.5 text-[10px] font-medium bg-green-100 text-green-700 rounded">{log.channel}</span></td>
                  <td className="px-3 py-2.5">
                    {log.status === 'Sent' ? <CheckCircle className="w-3.5 h-3.5 text-emerald-500" /> : <XCircle className="w-3.5 h-3.5 text-rose-400" />}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Rule Modal */}
      {showRuleModal && (
        <div className="fixed inset-0 z-50 bg-black/30 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md max-h-[90vh] overflow-y-auto">
            <div className="p-5 border-b border-green-100"><h2 className="text-base font-bold text-slate-800">{editRule ? 'Edit Rule' : 'Add Reminder Rule'}</h2></div>
            <div className="p-5 space-y-3">
              <div><label className="text-xs font-medium text-slate-600 mb-1 block">Rule Name</label><input value={ruleForm.name} onChange={e => setRuleForm(f => ({ ...f, name: e.target.value }))} className="w-full text-xs border border-slate-200 rounded-lg px-3 py-2 focus:ring-1 focus:ring-emerald-400 outline-none" /></div>
              <div><label className="text-xs font-medium text-slate-600 mb-1 block">Days Overdue (0 = on due date)</label><input type="number" value={ruleForm.daysOverdue} onChange={e => setRuleForm(f => ({ ...f, daysOverdue: Number(e.target.value) }))} min={0} className="w-full text-xs border border-slate-200 rounded-lg px-3 py-2 focus:ring-1 focus:ring-emerald-400 outline-none" /></div>
              <div>
                <label className="text-xs font-medium text-slate-600 mb-2 block">Send Via</label>
                <div className="flex gap-3">
                  {[['sendWhatsApp', 'WhatsApp'], ['sendEmail', 'Email'], ['sendSms', 'SMS']].map(([key, label]) => (
                    <label key={key} className="flex items-center gap-1.5 cursor-pointer">
                      <input type="checkbox" checked={(ruleForm as any)[key]} onChange={e => setRuleForm(f => ({ ...f, [key]: e.target.checked }))} className="rounded border-slate-300 text-emerald-600" />
                      <span className="text-xs text-slate-600">{label}</span>
                    </label>
                  ))}
                </div>
              </div>
              <div>
                <label className="text-xs font-medium text-slate-600 mb-1 block">Message Template</label>
                <textarea value={ruleForm.messageTemplate} onChange={e => setRuleForm(f => ({ ...f, messageTemplate: e.target.value }))} rows={4} className="w-full text-xs border border-slate-200 rounded-lg px-3 py-2 focus:ring-1 focus:ring-emerald-400 outline-none resize-none" />
                <p className="text-[10px] text-slate-400 mt-1">Variables: {'{CustomerName}'}, {'{InvoiceNumber}'}, {'{Amount}'}, {'{Days}'}</p>
              </div>
              <label className="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" checked={ruleForm.isEnabled} onChange={e => setRuleForm(f => ({ ...f, isEnabled: e.target.checked }))} className="rounded border-slate-300 text-emerald-600" />
                <span className="text-xs font-medium text-slate-600">Rule is active</span>
              </label>
            </div>
            <div className="p-4 border-t border-green-100 flex gap-2 justify-end">
              <button onClick={() => setShowRuleModal(false)} className="px-4 py-2 text-xs font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50">Cancel</button>
              <button onClick={saveRule} disabled={!ruleForm.name} className="px-4 py-2 text-xs font-semibold bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 disabled:opacity-50">Save Rule</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

