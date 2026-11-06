import React, { useState, useEffect } from 'react';
import { Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';
import { CommandPalette } from '../../components/common/CommandPalette';
import { ToastProvider } from '../../components/common/Toast';
import { useStore } from '../../store/useStore';
import { api } from '../../services/apiClient';

export const AppLayout: React.FC = () => {
  const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false);
  const [commandPaletteOpen, setCommandPaletteOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { checkBackendHealth, syncWithBackend } = useStore();

  // ─── On mount: check API health, then sync live data if connected ─────────
  useEffect(() => {
    (async () => {
      const isOnline = await checkBackendHealth();
      if (isOnline && api.getToken() && !api.hasRole('SuperAdmin')) {
        syncWithBackend().catch(() => {});
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Global Keyboard Shortcuts Listener (F2, F4, F6, F8, Ctrl+K)
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Ctrl+K for search command palette
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        setCommandPaletteOpen((prev) => !prev);
      }
      // F2 for New Bill POS
      if (e.key === 'F2') {
        e.preventDefault();
        navigate('/billing');
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [navigate]);

  if (!api.getToken()) return <Navigate to="/login" replace />;
  if (api.hasRole('SuperAdmin') && location.pathname !== '/platform-admin')
    return <Navigate to="/platform-admin" replace />;
  if (api.hasRole('SuperAdmin')) return (
    <ToastProvider>
      <div className="min-h-screen bg-slate-50">
        <header className="bg-white border-b border-slate-200 px-6 py-4 flex justify-between items-center">
          <strong>BillFlow Platform Admin</strong>
          <button className="text-sm font-semibold text-rose-600" onClick={() => { api.setToken(null); navigate('/login'); }}>Logout</button>
        </header>
        <main className="p-4 sm:p-6"><Outlet /></main>
      </div>
    </ToastProvider>
  );

  return (
    <ToastProvider>
      <div className="min-h-screen bg-slate-50 flex flex-col font-sans antialiased text-slate-900">
        {/* Left Sidebar */}
        <Sidebar isOpen={mobileSidebarOpen} onCloseMobile={() => setMobileSidebarOpen(false)} />

        {/* Main Content Area offset by Sidebar width on lg screens */}
        <div className="lg:pl-64 flex flex-col flex-1 min-w-0">
          {/* Top Bar */}
          <Topbar
            onOpenMobileSidebar={() => setMobileSidebarOpen(true)}
            onOpenCommandPalette={() => setCommandPaletteOpen(true)}
          />

          {/* Router View Page Container - Full Width Seamless Canvas */}
          <main className="flex-1 p-4 sm:p-6 w-full min-w-0">
            <Outlet />
          </main>
        </div>

        {/* Global Ctrl+K Search Modal */}
        <CommandPalette isOpen={commandPaletteOpen} onClose={() => setCommandPaletteOpen(false)} />
      </div>
    </ToastProvider>
  );
};
