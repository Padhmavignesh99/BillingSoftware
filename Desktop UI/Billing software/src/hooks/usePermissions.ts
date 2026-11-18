import { useStore } from '../store/useStore';
import type { UserRole } from '../types';

/**
 * usePermissions
 * Returns helper flags for the current user's role and permissions.
 * Used to conditionally show/hide sidebar menu items and UI controls.
 */
export function usePermissions() {
  const { currentUser } = useStore();

  const role     = (currentUser?.role ?? 'cashier') as UserRole;
  const perms    = currentUser?.permissions;

  const isAdmin   = role === 'admin';
  const isManager = role === 'manager';
  const isCashier = role === 'cashier';
  const isSales   = role === 'sales';
  const isAdminOrManager = isAdmin || isManager;

  return {
    role,
    isAdmin,
    isManager,
    isCashier,
    isSales,
    isAdminOrManager,
    hasAnyRole: (...roles: UserRole[]) => roles.includes(role),

    // Granular permission checks (falls back to false if perms not set)
    can: {
      createBill:       isAdmin || isManager || isCashier || !!perms?.createBill,
      editBill:         isAdmin || isManager || !!perms?.editBill,
      cancelBill:       isAdmin || isManager || !!perms?.cancelBill,
      viewProducts:     isAdmin || isManager || !!perms?.viewProducts,
      manageProducts:   isAdmin || isManager || !!perms?.manageProducts,
      viewInventory:    isAdmin || isManager || !!perms?.viewInventory,
      manageInventory:  isAdmin || !!perms?.manageInventory,
      viewReports:      isAdmin || isManager || !!perms?.viewReports,
      viewProfit:       isAdmin || !!perms?.viewProfit,
      manageStaff:      isAdmin || !!perms?.manageStaff,
      manageSettings:   isAdmin || !!perms?.manageSettings,
      manageCustomers:  isAdminOrManager || isCashier,
      manageSuppliers:  isAdminOrManager,
      managePurchases:  isAdminOrManager,
      manageExpenses:   isAdmin || !!perms?.manageInventory,
    },
  };
}
