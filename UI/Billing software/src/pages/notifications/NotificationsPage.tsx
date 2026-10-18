import React from 'react';
import { Bell, CheckCheck, AlertTriangle, PackageX, CheckCircle2, Clock } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { useToast } from '../../components/common/Toast';

export const NotificationsPage: React.FC = () => {
  const { notifications, markNotificationRead, markAllNotificationsRead } = useStore();
  const { showToast } = useToast();

  return (
    <div className="space-y-6 w-full">
      <div className="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl font-bold text-slate-900 tracking-tight">Notification Center</h1>
          <p className="text-xs text-slate-500">Live store system alerts, stock warnings & payment notices</p>
        </div>

        <button
          onClick={() => { void markAllNotificationsRead().catch((error) => showToast(error.message, 'error')); }}
          className="flex items-center gap-1.5 px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-xs font-semibold"
        >
          <CheckCheck className="w-4 h-4 text-indigo-600" />
          <span>Mark All Read</span>
        </button>
      </div>

      <div className="space-y-3">
        {notifications.map((n) => (
          <div
            key={n.id}
            onClick={() => { if (!n.isRead) void markNotificationRead(n.id).catch((error) => showToast(error.message, 'error')); }}
            className={`p-4 rounded-2xl border transition-all cursor-pointer flex items-start gap-3.5 ${
              n.isRead
                ? 'bg-white border-slate-200/80 opacity-75'
                : 'bg-indigo-50/40 border-indigo-200/90 shadow-2xs'
            }`}
          >
            <div className="shrink-0 mt-0.5">
              {n.type === 'out_of_stock' && <PackageX className="w-5 h-5 text-rose-600" />}
              {n.type === 'low_stock' && <AlertTriangle className="w-5 h-5 text-amber-600" />}
              {n.type === 'payment_received' && <CheckCircle2 className="w-5 h-5 text-emerald-600" />}
              {n.type === 'payment_overdue' && <Clock className="w-5 h-5 text-rose-600" />}
            </div>

            <div className="flex-1 min-w-0">
              <div className="flex items-center justify-between mb-0.5">
                <h4 className="text-xs font-bold text-slate-900">{n.title}</h4>
                <span className="text-[10px] text-slate-400">
                  {new Date(n.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </span>
              </div>
              <p className="text-xs text-slate-600 leading-relaxed">{n.message}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
