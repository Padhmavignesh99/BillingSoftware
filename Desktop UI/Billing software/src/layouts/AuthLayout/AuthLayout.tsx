import React from 'react';
import { Outlet } from 'react-router-dom';
import { ToastProvider } from '../../components/common/Toast';

export const AuthLayout: React.FC = () => {
  return (
    <ToastProvider>
      <Outlet />
    </ToastProvider>
  );
};
