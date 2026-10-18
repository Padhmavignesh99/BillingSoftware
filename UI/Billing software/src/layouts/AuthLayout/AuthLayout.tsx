import React from 'react';
import { Outlet } from 'react-router-dom';
import { ToastProvider } from '../../components/common/Toast';

export const AuthLayout: React.FC = () => {
  return (
    <ToastProvider><div className="min-h-screen bg-slate-900 text-slate-100 flex items-center justify-center p-4 relative overflow-hidden">
      {/* Background Subtle Gradient Blobs */}
      <div className="absolute -top-40 -left-40 w-96 h-96 bg-indigo-600/20 rounded-full blur-3xl" />
      <div className="absolute -bottom-40 -right-40 w-96 h-96 bg-purple-600/20 rounded-full blur-3xl" />

      <div className="w-full max-w-md relative z-10">
        <Outlet />
      </div>
    </div></ToastProvider>
  );
};
