import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Search,
  Plus,
  Bell,
  HelpCircle,
  Store,
  Monitor,
  Menu,
  Moon,
  Sun,
  ChevronDown,
  CheckCircle2,
  Building2,
  ShieldCheck,
  Zap,
  Globe,
  PlusCircle,
  ExternalLink,
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { CreateTenantModal } from '../../components/saas/CreateTenantModal';
import { LanguageSwitcher } from '../../components/ui/LanguageSwitcher';
import { api } from '../../services/apiClient';

interface TopbarProps {
  onOpenMobileSidebar: () => void;
  onOpenCommandPalette: () => void;
}

export const Topbar: React.FC<TopbarProps> = ({ onOpenMobileSidebar, onOpenCommandPalette }) => {
  const navigate = useNavigate();
  const {
    businessProfile,
    registers,
    currentRegisterId,
    setCurrentRegisterId,
    currentUser,
    notifications,
    theme,
    toggleTheme,
    tenants,
    currentTenantId,
    switchTenant,
    getActiveTenant,
    backendStatus,
  } = useStore();

  const [showProfileMenu, setShowProfileMenu] = useState(false);
  const [showRegisterMenu, setShowRegisterMenu] = useState(false);
  const [showTenantMenu, setShowTenantMenu] = useState(false);
  const [isCreateTenantOpen, setIsCreateTenantOpen] = useState(false);

  const activeTenant = getActiveTenant();
  const currentRegister = registers.find((r) => r.id === currentRegisterId) || registers[0] || {
    id: 'reg-01',
    code: 'POS-1',
    name: 'Main Counter',
    currentCash: 0,
  };
  const unreadNotifs = notifications.filter((n) => !n.isRead);

  return (
    <>
      <header className="h-16 bg-white border-b border-green-100 sticky top-0 z-30 px-4 sm:px-6 flex items-center justify-between shadow-sm">
        {/* Left section */}
        <div className="flex items-center gap-3">
          <button
            onClick={onOpenMobileSidebar}
            className="p-2 text-slate-500 hover:text-slate-800 hover:bg-green-100 rounded-lg lg:hidden"
          >
            <Menu className="w-5 h-5" />
          </button>

          {/* Tenant Switcher Dropdown (Primary SaaS Multi-Tenant Control) */}
          <div className="relative">
            <button
              onClick={() => backendStatus !== 'connected' && setShowTenantMenu(!showTenantMenu)}
              className="flex items-center gap-2.5 px-3 py-1.5 bg-green-50 hover:bg-green-100 border border-green-200 rounded-xl text-xs font-semibold text-slate-800 transition-all shadow-2xs group"
            >
              <div className="w-6 h-6 rounded-lg bg-emerald-600 text-white flex items-center justify-center font-bold text-xs shrink-0 shadow-xs">
                {activeTenant.name.charAt(0)}
              </div>
              <div className="text-left hidden sm:block">
                <div className="flex items-center gap-1.5">
                  <span className="font-bold text-slate-900 dark:text-white max-w-[130px] truncate">
                    {activeTenant.name}
                  </span>
                </div>
                {backendStatus !== 'connected' && <p className="text-[10px] text-slate-400 font-mono leading-none">{activeTenant.subdomain}</p>}
              </div>
              {backendStatus !== 'connected' && <ChevronDown className="w-3.5 h-3.5 text-slate-400 group-hover:text-emerald-600" />}
            </button>

            {/* Tenant Selection Menu */}
            {backendStatus !== 'connected' && showTenantMenu && (
              <div className="absolute left-0 mt-2 w-72 bg-white rounded-2xl shadow-2xl border border-green-100 py-1.5 z-50 animate-in fade-in duration-150">
                <div className="px-4 py-2.5 border-b border-slate-100 dark:border-slate-800 bg-slate-50/60 dark:bg-slate-800/40 flex items-center justify-between">
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">
                    Subscribed Organizations ({tenants.length})
                  </p>
                  <span className="text-[9px] bg-emerald-100 text-emerald-700 font-bold px-1.5 py-0.2 rounded">
                    Multi-Tenant
                  </span>
                </div>

                <div className="max-h-60 overflow-y-auto divide-y divide-slate-50 dark:divide-slate-800">
                  {tenants.map((t) => {
                    const isSelected = t.id === currentTenantId;
                    return (
                      <button
                        key={t.id}
                        onClick={() => {
                          switchTenant(t.id);
                          setShowTenantMenu(false);
                        }}
                        className={`w-full px-4 py-2.5 text-left text-xs hover:bg-green-50 flex items-center justify-between transition-colors ${isSelected ? 'bg-emerald-50/70' : ''}`}
                      >
                        <div className="min-w-0 pr-2">
                          <div className="flex items-center gap-1.5">
                            <p className="font-bold text-slate-900 dark:text-white truncate">{t.name}</p>
                            <span className="text-[9px] font-mono text-slate-400 uppercase">({t.planId})</span>
                          </div>
                          <p className="text-[10px] text-slate-400 font-mono truncate">{t.subdomain}</p>
                        </div>
                        {isSelected && <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0" />}
                      </button>
                    );
                  })}
                </div>

                <div className="p-2 border-t border-slate-100 dark:border-slate-800 bg-slate-50/30 dark:bg-slate-800/20 space-y-1">
                  <button
                    onClick={() => {
                      setShowTenantMenu(false);
                      setIsCreateTenantOpen(true);
                    }}
                    className="w-full flex items-center gap-2 px-3 py-2 text-xs font-bold text-emerald-700 hover:bg-emerald-50 rounded-xl transition-colors"
                  >
                    <PlusCircle className="w-4 h-4" />
                    <span>+ Provision New Tenant</span>
                  </button>

                  {api.hasRole('SuperAdmin') && <button
                    onClick={() => {
                      setShowTenantMenu(false);
                      navigate('/platform-admin');
                    }}
                    className="w-full flex items-center justify-between px-3 py-2 text-xs font-bold text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-950/40 rounded-xl transition-colors"
                  >
                    <div className="flex items-center gap-2">
                      <ShieldCheck className="w-4 h-4" />
                      <span>SaaS Platform Admin</span>
                    </div>
                    <span className="text-[9px] bg-amber-100 text-amber-800 px-1.5 py-0.2 rounded font-mono">OWNER</span>
                  </button>}
                </div>
              </div>
            )}
          </div>

          {/* Global Search Trigger */}
          <button
            onClick={onOpenCommandPalette}
            className="hidden md:flex items-center gap-3 bg-green-50 hover:bg-green-100 text-slate-500 px-3.5 py-2 rounded-xl text-xs font-medium border border-green-200/60 w-48 sm:w-64 md:w-80 transition-all group"
          >
            <Search className="w-4 h-4 text-slate-400 group-hover:text-emerald-600 transition-colors" />
            <span className="truncate">Search products, bills, customers...</span>
            <div className="ml-auto hidden sm:flex items-center gap-1 text-[10px] font-semibold text-slate-400 bg-white px-1.5 py-0.5 rounded border border-green-200 shadow-2xs font-mono">
              <span>Ctrl K</span>
            </div>
          </button>
        </div>

        {/* Right section */}
        <div className="flex items-center gap-2 sm:gap-2.5">

          {/* Language Switcher */}
          <div className="hidden sm:block">
            <LanguageSwitcher />
          </div>

          {/* Backend API Status Pill */}
          <div
            title={backendStatus === 'connected' ? 'API connected' : backendStatus === 'offline' ? 'API unavailable' : 'Checking API...'}
            className={`hidden sm:flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg text-xs font-bold border transition-all cursor-default select-none ${
              backendStatus === 'connected'
                ? 'bg-emerald-50 border-emerald-200/60 text-emerald-700'
                : backendStatus === 'offline'
                ? 'bg-amber-50 border-amber-200/60 text-amber-700'
                : 'bg-slate-50 border-slate-200/60 text-slate-500'
            }`}
          >
            <span className={`w-1.5 h-1.5 rounded-full ${
              backendStatus === 'connected' ? 'bg-emerald-500 animate-pulse' :
              backendStatus === 'offline'    ? 'bg-amber-400' :
              'bg-slate-400 animate-pulse'
            }`} />
            <span className="hidden md:inline">
              {backendStatus === 'connected' ? 'API Live' : backendStatus === 'offline' ? 'API Offline' : 'Checking...'}
            </span>
          </div>

          {/* Subscription status entry */}
          <button
            onClick={() => navigate('/subscription')}
            className="hidden sm:flex items-center gap-1.5 px-2.5 py-1.5 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200/60 text-emerald-700 rounded-lg text-xs font-bold transition-all"
          >
            <Zap className="w-3.5 h-3.5 text-emerald-600 fill-current" />
            <span>Subscription</span>
          </button>

          {/* Register Selector Dropdown */}
          {backendStatus !== 'connected' && <div className="relative">
            <button
              onClick={() => setShowRegisterMenu(!showRegisterMenu)}
              className="flex items-center gap-2 px-3 py-1.5 bg-green-50 border border-green-200 rounded-lg text-xs font-medium text-slate-700 hover:bg-green-100 transition-colors"
            >
              <Monitor className="w-4 h-4 text-emerald-600" />
              <span className="hidden sm:inline font-mono">{currentRegister.code}</span>
              <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
              <ChevronDown className="w-3.5 h-3.5 text-slate-400" />
            </button>

            {showRegisterMenu && (
              <div className="absolute right-0 mt-2 w-56 bg-white rounded-xl shadow-xl border border-green-100 py-1 z-50 animate-in fade-in duration-150">
                <div className="px-3 py-2 border-b border-slate-100 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-800/50">
                  <p className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider">POS Registers ({registers.length})</p>
                </div>
                {registers.map((reg) => (
                  <button
                    key={reg.id}
                    onClick={() => {
                      setCurrentRegisterId(reg.id);
                      setShowRegisterMenu(false);
                    }}
                    className="w-full px-3 py-2 text-left text-xs hover:bg-green-50 flex items-center justify-between"
                  >
                    <div>
                      <p className="font-semibold text-slate-800 dark:text-white">{reg.name}</p>
                      <p className="text-[10px] text-slate-500 font-mono">Cash: ₹{reg.currentCash.toLocaleString()}</p>
                    </div>
                    {reg.id === currentRegisterId && <CheckCircle2 className="w-4 h-4 text-emerald-600" />}
                  </button>
                ))}
              </div>
            )}
          </div>}

          {/* New Bill POS Quick Action Button */}
          <button
            onClick={() => navigate('/billing')}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-semibold shadow-sm shadow-emerald-600/30 transition-all"
          >
            <Plus className="w-4 h-4" />
            <span className="hidden sm:inline">New Bill</span>
            <span className="text-[10px] bg-emerald-700 px-1 py-0.2 rounded font-mono font-bold">F2</span>
          </button>

          {/* Theme Toggle */}
          <button
            onClick={toggleTheme}
            title="Toggle Theme"
            className="p-2 text-slate-500 hover:text-slate-800 hover:bg-green-100 rounded-lg transition-colors"
          >
            {theme === 'dark' ? <Sun className="w-4 h-4 text-amber-500" /> : <Moon className="w-4 h-4" />}
          </button>

          {/* Notifications */}
          <button
            onClick={() => navigate('/notifications')}
            className="relative p-2 text-slate-500 hover:text-slate-800 hover:bg-green-100 rounded-lg transition-colors"
          >
            <Bell className="w-4.5 h-4.5" />
            {unreadNotifs.length > 0 && (
              <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-rose-500 rounded-full ring-2 ring-white" />
            )}
          </button>

          {/* User Profile */}
          <div className="relative border-l border-green-100 pl-3">
            <button
              onClick={() => setShowProfileMenu(!showProfileMenu)}
              className="flex items-center gap-2 hover:opacity-85 transition-opacity"
            >
              <img
                src={currentUser?.avatarUrl || 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=120&auto=format&fit=crop&q=80'}
                alt={currentUser?.name || 'User'}
                className="w-8 h-8 rounded-full object-cover border-2 border-emerald-200"
              />
              <div className="text-left hidden lg:block">
                <p className="text-xs font-semibold text-slate-800 leading-none">{currentUser?.name || 'Admin'}</p>
                <p className="text-[10px] text-emerald-600 capitalize mt-0.5">{activeTenant.name}</p>
              </div>
            </button>

            {showProfileMenu && (
              <div className="absolute right-0 mt-2 w-64 bg-white rounded-xl shadow-xl border border-green-100 py-1.5 z-50 animate-in fade-in duration-150">
                <div className="px-4 py-3 border-b border-slate-100 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-800/50">
                  <p className="text-xs font-bold text-slate-900 dark:text-white">{currentUser?.name}</p>
                  <p className="text-[11px] text-slate-500">{currentUser?.email || activeTenant.ownerEmail}</p>
                  <div className="mt-2 flex items-center gap-2">
                    <Badge variant="success" size="sm">
                      {(currentUser?.role || 'admin').toUpperCase()}
                    </Badge>
                    <span className="text-[10px] text-slate-400 font-mono">{activeTenant.subdomain}</span>
                  </div>
                </div>

                <button
                  onClick={() => {
                    setShowProfileMenu(false);
                    navigate('/subscription');
                  }}
                  className="w-full text-left px-4 py-2 text-xs text-slate-700 hover:bg-green-50 flex items-center justify-between"
                >
                  <span>SaaS Subscription & Plan</span>
                  <span className="text-[9px] bg-emerald-100 text-emerald-700 font-bold px-1 rounded">MANAGE</span>
                </button>

                <button
                  onClick={() => {
                    setShowProfileMenu(false);
                    navigate('/settings');
                  }}
                  className="w-full text-left px-4 py-2 text-xs text-slate-700 hover:bg-green-50"
                >
                  Store Settings & GST
                </button>

                {api.hasRole('SuperAdmin') && <button
                  onClick={() => {
                    setShowProfileMenu(false);
                    navigate('/platform-admin');
                  }}
                  className="w-full text-left px-4 py-2 text-xs text-amber-600 dark:text-amber-400 font-semibold hover:bg-amber-50 dark:hover:bg-amber-950/40"
                >
                  👑 Platform Admin Portal
                </button>}

                <div className="border-t border-green-100 my-1" />

                <button
                  onClick={() => {
                    setShowProfileMenu(false);
                    api.setToken(null);
                    navigate('/login');
                  }}
                  className="w-full text-left px-4 py-2 text-xs text-rose-600 hover:bg-rose-50 dark:hover:bg-rose-950/40 font-medium"
                >
                  Logout Session
                </button>
              </div>
            )}
          </div>
        </div>
      </header>

      {/* Modal for adding a new tenant workspace */}
      <CreateTenantModal isOpen={isCreateTenantOpen} onClose={() => setIsCreateTenantOpen(false)} />
    </>
  );
};
