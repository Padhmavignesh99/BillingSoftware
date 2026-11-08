import React from 'react';
import { createBrowserRouter, Navigate } from 'react-router-dom';
import { AppLayout } from '../../layouts/AppLayout/AppLayout';
import { AuthLayout } from '../../layouts/AuthLayout/AuthLayout';

import { LoginPage } from '../../pages/auth/LoginPage';
import { RegisterPage } from '../../pages/auth/RegisterPage';
import { OnboardingPage } from '../../pages/onboarding/OnboardingPage';

import { DashboardPage } from '../../pages/dashboard/DashboardPage';
import { BillingPage } from '../../pages/billing/BillingPage';
import { InvoicesPage } from '../../pages/invoices/InvoicesPage';
import { InvoiceDetailPage } from '../../pages/invoices/InvoiceDetailPage';
import { ProductsPage } from '../../pages/products/ProductsPage';
import { AddProductPage } from '../../pages/products/AddProductPage';
import { InventoryPage } from '../../pages/inventory/InventoryPage';
import { CustomersPage } from '../../pages/customers/CustomersPage';
import { CustomerLedgerPage } from '../../pages/customers/CustomerLedgerPage';
import { SuppliersPage } from '../../pages/suppliers/SuppliersPage';
import { PurchasesPage } from '../../pages/purchases/PurchasesPage';
import { ExpensesPage } from '../../pages/expenses/ExpensesPage';
import { PaymentsPage } from '../../pages/payments/PaymentsPage';
import { ReportsPage } from '../../pages/reports/ReportsPage';
import { StaffPage } from '../../pages/staff/StaffPage';
import { NotificationsPage } from '../../pages/notifications/NotificationsPage';
import { AIInsightsPage } from '../../pages/ai/AIInsightsPage';
import { SettingsPage } from '../../pages/settings/SettingsPage';
import { PrinterSettingsPage } from '../../pages/settings/PrinterSettingsPage';
import { SubscriptionPage } from '../../pages/saas/SubscriptionPage';
import { PlatformAdminPage } from '../../pages/saas/PlatformAdminPage';
import PlanPermitManagerPage from '../../pages/saas/PlanPermitManagerPage';
import { SalesReturnsPage } from '../../pages/returns/SalesReturnsPage';
import { PurchaseReturnsPage } from '../../pages/returns/PurchaseReturnsPage';
import { CreditBillsPage } from '../../pages/credit/CreditBillsPage';

// ── New Feature Pages ─────────────────────────────────────────────────────────
import { ImportExportPage } from '../../pages/import-export/ImportExportPage';
import { AuditLogPage } from '../../pages/audit/AuditLogPage';
import { DeliveryChallansPage } from '../../pages/challans/DeliveryChallansPage';
import { RecurringInvoicesPage } from '../../pages/invoices/RecurringInvoicesPage';
import { StockTakePage } from '../../pages/inventory/StockTakePage';
import { ReorderPage } from '../../pages/inventory/ReorderPage';
import { LoyaltySettingsPage } from '../../pages/loyalty/LoyaltySettingsPage';
import { PaymentRemindersPage } from '../../pages/reminders/PaymentRemindersPage';
import { ProfitLossPage } from '../../pages/reports/ProfitLossPage';
import { GSTReportsPage } from '../../pages/reports/GSTReportsPage';

// ── Wave 3 Feature Pages ──────────────────────────────────────────────────────
import ProductVariantsPage from '../../pages/products/ProductVariantsPage';
import RolesPermissionsPage from '../../pages/staff/RolesPermissionsPage';
import DataBackupPage from '../../pages/settings/DataBackupPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/dashboard" replace />,
  },
  {
    element: <AuthLayout />,
    children: [
      { path: '/login', element: <LoginPage /> },
      { path: '/register', element: <RegisterPage /> },
      { path: '/onboarding', element: <OnboardingPage /> },
    ],
  },
  {
    element: <AppLayout />,
    children: [
      { path: '/dashboard', element: <DashboardPage /> },
      { path: '/billing', element: <BillingPage /> },
      { path: '/invoices', element: <InvoicesPage /> },
      { path: '/invoices/:id', element: <InvoiceDetailPage /> },
      { path: '/recurring-invoices', element: <RecurringInvoicesPage /> },
      { path: '/products', element: <ProductsPage /> },
      { path: '/products/new', element: <AddProductPage /> },
      { path: '/products/variants', element: <ProductVariantsPage /> },
      { path: '/inventory', element: <InventoryPage /> },
      { path: '/stock-take', element: <StockTakePage /> },
      { path: '/reorder', element: <ReorderPage /> },
      { path: '/customers', element: <CustomersPage /> },
      { path: '/customers/:id/ledger', element: <CustomerLedgerPage /> },
      { path: '/suppliers', element: <SuppliersPage /> },
      { path: '/purchases', element: <PurchasesPage /> },
      { path: '/challans', element: <DeliveryChallansPage /> },
      { path: '/expenses', element: <ExpensesPage /> },
      { path: '/sales-returns', element: <SalesReturnsPage /> },
      { path: '/purchase-returns', element: <PurchaseReturnsPage /> },
      { path: '/credit-bills', element: <CreditBillsPage /> },
      { path: '/loyalty', element: <LoyaltySettingsPage /> },
      { path: '/reminders', element: <PaymentRemindersPage /> },
      { path: '/import-export', element: <ImportExportPage /> },
      { path: '/payments', element: <PaymentsPage /> },
      { path: '/reports', element: <ReportsPage /> },
      { path: '/reports/profit-loss', element: <ProfitLossPage /> },
      { path: '/reports/gst', element: <GSTReportsPage /> },
      { path: '/staff', element: <StaffPage /> },
      { path: '/staff/roles', element: <RolesPermissionsPage /> },
      { path: '/notifications', element: <NotificationsPage /> },
      { path: '/activity-log', element: <AuditLogPage /> },
      { path: '/ai-insights', element: <AIInsightsPage /> },
      { path: '/settings', element: <SettingsPage /> },
      { path: '/settings/printers', element: <PrinterSettingsPage /> },
      { path: '/settings/backup', element: <DataBackupPage /> },
      { path: '/subscription', element: <SubscriptionPage /> },
      { path: '/platform-admin', element: <PlatformAdminPage /> },
      { path: '/admin/plan-permits', element: <PlanPermitManagerPage /> },
    ],
  },

  {
    path: '*',
    element: <Navigate to="/dashboard" replace />,
  },
]);
