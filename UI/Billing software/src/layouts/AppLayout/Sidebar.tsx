import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  ShoppingBag,
  Receipt,
  Package,
  Boxes,
  Users,
  Truck,
  ShoppingCart,
  ReceiptIndianRupee,
  BarChart3,
  UserCheck,
  Sparkles,
  Bell,
  Settings,
  LogOut,
  CreditCard,
  ShieldCheck,
  RotateCcw,
  Wallet,
  Printer,
  FileUp,
  Activity,
  ClipboardList,
  ClipboardCheck,
  Zap,
  RefreshCw,
  Star,
  TrendingUp,
  FileText,
  Warehouse,
  Layers,
  Shield,
  Database,
  Gift,
} from 'lucide-react';
import { clsx } from 'clsx';
import { useStore } from '../../store/useStore';
import { api } from '../../services/apiClient';

interface SidebarProps {
  isOpen: boolean;
  onCloseMobile?: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ isOpen, onCloseMobile }) => {
  const location = useLocation();
  const {
    businessProfile,
    currentUser,
    notifications,
    getActiveTenant,
  } = useStore();

  const activeTenant = getActiveTenant();
  const unreadCount = notifications.filter((n) => !n.isRead).length;

  const coreNav = [
    { name: 'Dashboard', path: '/dashboard', icon: LayoutDashboard },
    { name: 'Billing (POS)', path: '/billing', icon: ShoppingBag, badge: 'F2' },
    { name: 'Invoices', path: '/invoices', icon: Receipt },
    { name: 'Recurring Invoices', path: '/recurring-invoices', icon: RefreshCw },
    { name: 'Products', path: '/products', icon: Package },
    { name: 'Product Variants', path: '/products/variants', icon: Layers },
    { name: 'Inventory', path: '/inventory', icon: Boxes },
    { name: 'Reorder Alerts', path: '/reorder', icon: Zap },
    { name: 'Customers', path: '/customers', icon: Users },
    { name: 'Suppliers', path: '/suppliers', icon: Truck },
    { name: 'Loyalty Program', path: '/loyalty', icon: Star },
  ];

  const operationsNav = [
    { name: 'Purchases', path: '/purchases', icon: ShoppingCart },
    { name: 'Delivery Challans', path: '/challans', icon: Truck },
    { name: 'Expenses', path: '/expenses', icon: ReceiptIndianRupee },
    { name: 'Sales Returns', path: '/sales-returns', icon: RotateCcw },
    { name: 'Purchase Returns', path: '/purchase-returns', icon: Truck },
    { name: 'Credit Bills (Khata)', path: '/credit-bills', icon: Wallet, highlight: true },
    { name: 'Stock Take', path: '/stock-take', icon: ClipboardList },
    { name: 'Payment Reminders', path: '/reminders', icon: Bell },
    { name: 'Import / Export', path: '/import-export', icon: FileUp },
    { name: 'Reports', path: '/reports', icon: BarChart3 },
    { name: 'P&L Statement', path: '/reports/profit-loss', icon: TrendingUp },
    { name: 'GST Reports', path: '/reports/gst', icon: FileText },
    { name: 'Staff & Roles', path: '/staff', icon: UserCheck },
    { name: 'Roles & Permissions', path: '/staff/roles', icon: Shield },
    { name: 'AI Insights', path: '/ai-insights', icon: Sparkles, highlight: true },
    { name: 'Notifications', path: '/notifications', icon: Bell, count: unreadCount },
    { name: 'Activity Log', path: '/activity-log', icon: Activity },
  ];

  const saasNav = [
    { name: 'Subscription & Plan', path: '/subscription', icon: CreditCard, highlight: true },
    ...(api.hasRole('SuperAdmin') ? [{ name: 'Platform Admin', path: '/platform-admin', icon: ShieldCheck, badge: 'SaaS' }] : []),
    ...(api.hasRole('SuperAdmin') ? [{ name: 'Plan Permits', path: '/admin/plan-permits', icon: Gift, badge: 'Admin' }] : []),
    { name: 'Printer Setup', path: '/settings/printers', icon: Printer },
    { name: 'Data Backup', path: '/settings/backup', icon: Database },
    { name: 'Settings', path: '/settings', icon: Settings },
  ];

  const isActive = (path: string) => {
    if (path === '/settings') return location.pathname === '/settings';
    if (path === '/dashboard') return location.pathname === '/dashboard';
    return location.pathname.startsWith(path);
  };

  return (
    <>
      {/* Mobile Backdrop */}
      {isOpen && (
        <div
          onClick={onCloseMobile}
          className="fixed inset-0 z-40 bg-green-950/30 backdrop-blur-xs lg:hidden"
        />
      )}

      {/* Sidebar Drawer Container */}
      <aside
        className={clsx(
          'fixed top-0 left-0 z-40 h-screen w-64 flex flex-col border-r transition-transform duration-300 ease-in-out lg:translate-x-0',
          'bg-white border-green-100',
          isOpen ? 'translate-x-0' : '-translate-x-full'
        )}
      >
        {/* Brand Header */}
        <div className="h-16 flex items-center justify-between px-5 border-b border-green-100 shrink-0 bg-gradient-to-r from-emerald-600 to-green-500">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-xl bg-white/20 flex items-center justify-center text-white shadow-lg font-bold text-lg border border-white/30">
              {activeTenant.name.charAt(0)}
            </div>
            <div className="min-w-0">
              <h1 className="text-sm font-bold tracking-tight text-white flex items-center gap-1.5 truncate">
                BillFlow
              </h1>
              <p className="text-[11px] text-emerald-100 truncate max-w-[130px] font-medium">{activeTenant.name}</p>
            </div>
          </div>
        </div>

        {/* Navigation Scrollable Body */}
        <div className="flex-1 overflow-y-auto px-3 py-4 space-y-5 bg-white">
          {/* CORE SECTION */}
          <div>
            <p className="px-3 text-[11px] font-semibold uppercase tracking-wider text-green-600/70 mb-2">CORE</p>
            <nav className="space-y-0.5">
              {coreNav.map((item) => {
                const active = isActive(item.path);
                const Icon = item.icon;
                return (
                  <NavLink
                    key={item.path}
                    to={item.path}
                    onClick={onCloseMobile}
                    className={clsx(
                      'flex items-center justify-between px-3 py-2 rounded-xl text-xs font-medium transition-all group',
                      active
                        ? 'bg-emerald-600 text-white font-semibold shadow-md shadow-emerald-600/25'
                        : 'text-slate-600 hover:bg-emerald-50 hover:text-emerald-800'
                    )}
                  >
                    <div className="flex items-center gap-2.5">
                      <Icon className={clsx('w-4 h-4 shrink-0', active ? 'text-white' : 'text-slate-400 group-hover:text-emerald-600')} />
                      <span>{item.name}</span>
                    </div>
                    {item.badge && (
                      <span
                        className={clsx(
                          'text-[10px] font-mono px-1.5 py-0.5 rounded font-bold',
                          active ? 'bg-emerald-700 text-white' : 'bg-green-100 text-green-700 group-hover:bg-green-200'
                        )}
                      >
                        {item.badge}
                      </span>
                    )}
                  </NavLink>
                );
              })}
            </nav>
          </div>

          {/* OPERATIONS SECTION */}
          <div>
            <p className="px-3 text-[11px] font-semibold uppercase tracking-wider text-green-600/70 mb-2">OPERATIONS</p>
            <nav className="space-y-0.5">
              {operationsNav.map((item) => {
                const active = isActive(item.path);
                const Icon = item.icon;
                return (
                  <NavLink
                    key={item.path}
                    to={item.path}
                    onClick={onCloseMobile}
                    className={clsx(
                      'flex items-center justify-between px-3 py-2 rounded-xl text-xs font-medium transition-all group',
                      active
                        ? 'bg-emerald-600 text-white font-semibold shadow-md shadow-emerald-600/25'
                        : 'text-slate-600 hover:bg-emerald-50 hover:text-emerald-800'
                    )}
                  >
                    <div className="flex items-center gap-2.5">
                      <Icon
                        className={clsx(
                          'w-4 h-4 shrink-0',
                          item.highlight && !active && 'text-amber-500',
                          active ? 'text-white' : !item.highlight && 'text-slate-400 group-hover:text-emerald-600'
                        )}
                      />
                      <span>{item.name}</span>
                    </div>
                    {item.count && item.count > 0 ? (
                      <span className="w-5 h-5 rounded-full bg-rose-500 text-white text-[10px] font-bold flex items-center justify-center">
                        {item.count}
                      </span>
                    ) : null}
                  </NavLink>
                );
              })}
            </nav>
          </div>

          {/* SAAS & SYSTEM SECTION */}
          <div>
            <p className="px-3 text-[11px] font-semibold uppercase tracking-wider text-green-600/70 mb-2">SAAS & SYSTEM</p>
            <nav className="space-y-0.5">
              {saasNav.map((item) => {
                const active = isActive(item.path);
                const Icon = item.icon;
                return (
                  <NavLink
                    key={item.path}
                    to={item.path}
                    onClick={onCloseMobile}
                    className={clsx(
                      'flex items-center justify-between px-3 py-2 rounded-xl text-xs font-medium transition-all group',
                      active
                        ? 'bg-emerald-600 text-white font-semibold shadow-md shadow-emerald-600/25'
                        : 'text-slate-600 hover:bg-emerald-50 hover:text-emerald-800'
                    )}
                  >
                    <div className="flex items-center gap-2.5">
                      <Icon
                        className={clsx(
                          'w-4 h-4 shrink-0',
                          item.path === '/platform-admin' && !active && 'text-amber-500',
                          item.path === '/subscription' && !active && 'text-emerald-600',
                          active ? 'text-white' : item.path === '/settings' && 'text-slate-400 group-hover:text-emerald-600'
                        )}
                      />
                      <span>{item.name}</span>
                    </div>
                    {item.badge && (
                      <span
                        className={clsx(
                          'text-[9px] font-mono px-1.5 py-0.5 rounded font-bold uppercase',
                          active ? 'bg-emerald-700 text-white' : 'bg-amber-100 text-amber-700 group-hover:bg-amber-200'
                        )}
                      >
                        {item.badge}
                      </span>
                    )}
                  </NavLink>
                );
              })}
            </nav>
          </div>
        </div>

        {/* Bottom Profile & Help Footer */}
        <div className="p-3 border-t border-green-100 space-y-2 bg-green-50/60">
          <div className="flex items-center justify-between p-2 rounded-xl bg-white border border-green-200/80 shadow-xs">
            <div className="flex items-center gap-2.5 min-w-0">
              <img
                src={currentUser?.avatarUrl || 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=120&auto=format&fit=crop&q=80'}
                alt={currentUser?.name || 'User'}
                className="w-8 h-8 rounded-full object-cover shrink-0 border-2 border-emerald-200"
              />
              <div className="min-w-0">
                <p className="text-xs font-semibold text-slate-800 truncate">{currentUser?.name || 'Admin'}</p>
                <p className="text-[10px] text-emerald-600 capitalize truncate">{activeTenant.name}</p>
              </div>
            </div>
            <NavLink to="/login" onClick={() => api.setToken(null)} title="Logout" className="text-slate-400 hover:text-rose-500 p-1 rounded-lg hover:bg-rose-50 transition-colors">
              <LogOut className="w-4 h-4" />
            </NavLink>
          </div>
        </div>
      </aside>
    </>
  );
};
