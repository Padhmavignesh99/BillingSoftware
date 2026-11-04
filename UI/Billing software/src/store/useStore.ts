import { create } from 'zustand';
import type {
  BusinessProfile,
  Register,
  UserStaff,
  Category,
  Product,
  Customer,
  Supplier,
  Invoice,
  Purchase,
  Expense,
  InventoryMovement,
  NotificationItem,
  AIInsight,
  CartItem,
  HeldBill,
  PaymentMethod,
  SplitPaymentDetail,
  InvoiceItem,
  Tenant,
  TenantPlan,
  PlanBillingCycle,
  TenantStatus,
  PlanFeature,
  SaaSAuditLog,
  SaaSPlatformStats,
  TenantQuotaUsage,
  TenantSubscriptionInvoice,
  SalesReturn,
  PurchaseReturn,
  CreditBill,
  Warehouse,
  Branch,
  TaxRate,
  DiscountScheme,
  Role,
  SettleCreditBillDTO,
  ProductMiniStatement,
} from '../types';
import {
  initialBusinessProfile,
  initialRegisters,
  initialStaff,
  initialCategories,
  initialProducts,
  initialCustomers,
  initialSuppliers,
  initialInvoices,
  initialPurchases,
  initialExpenses,
  initialInventoryMovements,
  initialNotifications,
  initialAIInsights,
  initialSalesReturns,
  initialPurchaseReturns,
  initialCreditBills,
  initialWarehouses,
  initialBranches,
  initialTaxRates,
  initialDiscounts,
  initialRoles,
} from '../data/mockData';
import { api } from '../services/apiClient';
import {
  mapApiProduct,
  mapApiCustomer,
  mapApiSupplier,
  mapApiInvoice,
  mapApiPurchase,
  mapApiExpense,
  mapApiStaff,
  mapApiSalesReturn,
  mapApiPurchaseReturn,
  mapApiCreditBill,
} from '../services/apiMapper';
import {
  SaaSPlans,
  initialTenants,
  initialPlatformStats,
  initialAuditLogs,
  bistroBusinessProfile,
  bistroRegisters,
  bistroStaff,
  bistroCategories,
  bistroProducts,
  bistroCustomers,
  bistroSuppliers,
  bistroInvoices,
  techBusinessProfile,
  techRegisters,
  techStaff,
  techCategories,
  techProducts,
  techCustomers,
  techSuppliers,
  techInvoices,
} from '../data/saasData';

interface TenantIsolatedData {
  businessProfile: BusinessProfile;
  registers: Register[];
  currentRegisterId: string;
  staff: UserStaff[];
  currentUser: UserStaff;
  categories: Category[];
  products: Product[];
  customers: Customer[];
  suppliers: Supplier[];
  invoices: Invoice[];
  purchases: Purchase[];
  expenses: Expense[];
  inventoryMovements: InventoryMovement[];
  notifications: NotificationItem[];
  aiInsights: AIInsight[];
  heldBills: HeldBill[];
  salesReturns: SalesReturn[];
  purchaseReturns: PurchaseReturn[];
  creditBills: CreditBill[];
  warehouses: Warehouse[];
  branches: Branch[];
  taxRates: TaxRate[];
  discounts: DiscountScheme[];
  roles: Role[];
}

interface AppState {
  // SaaS Multi-Tenancy Core State
  tenants: Tenant[];
  currentTenantId: string;
  isSuperAdminView: boolean;
  plans: PlanFeature[];
  auditLogs: SaaSAuditLog[];
  platformStats: SaaSPlatformStats;
  tenantDatabases: Record<string, TenantIsolatedData>;

  // Backend Live Status
  backendStatus: 'connected' | 'offline' | 'checking';
  backendUrl: string;

  // Current Active Tenant Data (Exposed directly for backward compatibility and simplicity)
  businessProfile: BusinessProfile;
  registers: Register[];
  currentRegisterId: string;
  staff: UserStaff[];
  currentUser: UserStaff;
  categories: Category[];
  products: Product[];
  customers: Customer[];
  suppliers: Supplier[];
  invoices: Invoice[];
  purchases: Purchase[];
  expenses: Expense[];
  inventoryMovements: InventoryMovement[];
  notifications: NotificationItem[];
  aiInsights: AIInsight[];
  salesReturns: SalesReturn[];
  purchaseReturns: PurchaseReturn[];
  creditBills: CreditBill[];
  warehouses: Warehouse[];
  branches: Branch[];
  taxRates: TaxRate[];
  discounts: DiscountScheme[];
  roles: Role[];

  // POS State (Active Session)
  cart: CartItem[];
  selectedCustomer: Customer;
  heldBills: HeldBill[];
  posExtraDiscount: number;

  // Theme
  theme: 'light' | 'dark';

  // Multi-Tenant SaaS Actions
  switchTenant: (tenantId: string) => void;
  toggleSuperAdminView: (enabled?: boolean) => void;
  createTenant: (params: {
    name: string;
    storeName: string;
    subdomain: string;
    ownerName: string;
    ownerEmail: string;
    ownerPhone: string;
    industry: 'supermarket' | 'restaurant' | 'electronics' | 'clothing' | 'pharmacy' | 'general';
    planId: TenantPlan;
    billingCycle: PlanBillingCycle;
  }) => Tenant;
  updateTenant: (tenantId: string, updates: Partial<Tenant>) => void;
  upgradeTenantPlan: (tenantId: string, newPlanId: TenantPlan, billingCycle: PlanBillingCycle) => void;
  toggleTenantStatus: (tenantId: string, status: TenantStatus) => void;
  extendTenantTrial: (tenantId: string, days: number) => void;
  getActiveTenant: () => Tenant;
  getActivePlan: () => PlanFeature;
  getTenantQuotaUsage: (tenantId?: string) => TenantQuotaUsage;

  // Backend Health & Sync Actions
  setBackendStatus: (status: 'connected' | 'offline' | 'checking') => void;
  checkBackendHealth: () => Promise<boolean>;
  syncWithBackend: () => Promise<void>;

  // Standard Workspace Actions (scoped to active tenant)
  setBusinessProfile: (profile: Partial<BusinessProfile>) => void;
  setCurrentRegisterId: (registerId: string) => void;
  setSelectedCustomer: (customer: Customer) => void;
  setPosExtraDiscount: (amount: number) => void;

  // Cart Actions
  addToCart: (product: Product, qty?: number) => void;
  removeFromCart: (productId: string) => void;
  updateCartQuantity: (productId: string, quantity: number) => void;
  updateCartDiscount: (productId: string, discountPercentage: number) => void;
  clearCart: () => void;

  // Held Bills Actions
  holdCurrentBill: (note?: string) => HeldBill | null;
  resumeHeldBill: (heldBillId: string) => void;
  deleteHeldBill: (heldBillId: string) => void;

  // Sale Completion
  completeSale: (params: {
    paymentMethod: PaymentMethod;
    splitDetails?: SplitPaymentDetail;
    amountReceived: number;
    notes?: string;
  }) => Promise<Invoice>;

  // Sales Returns
  addSalesReturn: (returnData: Partial<SalesReturn>) => Promise<SalesReturn>;

  // Purchases Actions
  addPurchase: (data: Omit<Purchase, 'id' | 'purchaseNumber'>) => Promise<Purchase>;
  addPurchaseReturn: (returnData: Partial<PurchaseReturn>) => Promise<PurchaseReturn>;

  // Credit Bills (User-wise & Khata Settlement)
  addCreditBill: (billData: Partial<CreditBill>) => Promise<CreditBill>;
  settleCreditBill: (settleData: SettleCreditBillDTO) => Promise<any>;
  getCreditBillsByUser: (userId: string | number) => CreditBill[];
  getMyCreditBills: () => CreditBill[];
  getUserCreditBalance: (userId: string | number) => number;
  getMyCreditBalance: () => number;

  // Products & Inventory Actions
  addProduct: (productData: Omit<Product, 'id' | 'createdAt'> & { ExpiryDate?: string }) => Promise<Product>;
  updateProduct: (id: string, data: Partial<Product> & { ExpiryDate?: string }) => Promise<void>;
  updateProductExpiryDate: (id: string, expiryDate: string) => Promise<void>;
  deleteProduct: (id: string) => Promise<void>;
  adjustStock: (productId: string, deltaQty: number, reason: string) => Promise<void>;
  getProductMiniStatement: (productId: string) => Promise<ProductMiniStatement>;

  // Customers Actions
  addCustomer: (data: Omit<Customer, 'id' | 'createdAt' | 'totalPurchases' | 'outstandingBalance' | 'loyaltyPoints'>) => Promise<Customer>;
  updateCustomer: (id: string, data: Partial<Customer>) => Promise<void>;
  recordCustomerPayment: (customerId: string, amount: number, notes?: string) => Promise<void>;

  // Suppliers Actions
  addSupplier: (data: Omit<Supplier, 'id' | 'totalPurchases' | 'outstandingPayable'>) => Promise<Supplier>;
  updateSupplier: (id: string, data: Partial<Supplier>) => Promise<void>;

  // Expenses Actions
  addExpense: (data: Omit<Expense, 'id' | 'expenseNumber'>) => Promise<Expense>;
  deleteExpense: (id: string) => Promise<void>;

  // Staff & Roles Actions
  addStaff: (data: Omit<UserStaff, 'id'> & { password: string }) => Promise<UserStaff>;
  updateStaff: (id: string, data: Partial<UserStaff>) => Promise<void>;
  deleteStaff: (id: string) => Promise<void>;
  addRole: (data: { name: string; description: string; permissions: string[] }) => Promise<Role>;
  updateRole: (id: string, data: Partial<Role>) => Promise<void>;
  deleteRole: (id: string) => Promise<void>;

  // Warehouses & Branches
  addWarehouse: (data: Omit<Warehouse, 'id'>) => Warehouse;
  addBranch: (data: Omit<Branch, 'id'>) => Branch;

  // Notifications
  markNotificationRead: (id: string) => Promise<void>;
  markAllNotificationsRead: () => Promise<void>;

  // Theme
  toggleTheme: () => void;
}

// Initial Tenant Data Map
const initialTenantDatabases: Record<string, TenantIsolatedData> = {
  'tenant-01': {
    businessProfile: initialBusinessProfile,
    registers: initialRegisters,
    currentRegisterId: 'reg-01',
    staff: initialStaff,
    currentUser: initialStaff[0],
    categories: initialCategories,
    products: initialProducts,
    customers: initialCustomers,
    suppliers: initialSuppliers,
    invoices: initialInvoices,
    purchases: initialPurchases,
    expenses: initialExpenses,
    inventoryMovements: initialInventoryMovements,
    notifications: initialNotifications,
    aiInsights: initialAIInsights,
    heldBills: [],
    salesReturns: initialSalesReturns,
    purchaseReturns: initialPurchaseReturns,
    creditBills: initialCreditBills,
    warehouses: initialWarehouses,
    branches: initialBranches,
    taxRates: initialTaxRates,
    discounts: initialDiscounts,
    roles: initialRoles,
  },
  'tenant-02': {
    businessProfile: bistroBusinessProfile,
    registers: bistroRegisters,
    currentRegisterId: 'reg-bistro-01',
    staff: bistroStaff,
    currentUser: bistroStaff[0],
    categories: bistroCategories,
    products: bistroProducts,
    customers: bistroCustomers,
    suppliers: bistroSuppliers,
    invoices: bistroInvoices,
    purchases: [],
    expenses: [
      {
        id: 'exp-b-01',
        expenseNumber: 'EXP-101',
        title: 'Commercial Kitchen Gas Refill',
        category: 'electricity',
        amount: 8500,
        paymentMethod: 'upi',
        date: '2026-10-02',
        recordedBy: 'Vikrant Joshi',
      },
    ],
    inventoryMovements: [],
    notifications: [
      {
        id: 'notif-b-01',
        title: 'New VIP Dine-In Member',
        message: 'Ananya Deshmukh enrolled with 920 loyalty points',
        type: 'new_customer',
        isRead: false,
        createdAt: '2026-10-04T12:00:00Z',
      },
    ],
    aiInsights: [
      {
        id: 'ai-b-01',
        category: 'sales',
        title: 'Woodfired Truffle Pizza Margin Surge',
        summary: 'Truffle Pizza generates 62% gross profit margin and is ordered in 48% of weekend dinner bills.',
        impact: 'high',
        trend: 'up',
        recommendation: 'Introduce a pairing combo with Spanish Caramel Latte for high-ticket weekday orders.',
        date: '2026-10-04',
      },
    ],
    heldBills: [],
    salesReturns: [],
    purchaseReturns: [],
    creditBills: [],
    warehouses: initialWarehouses,
    branches: initialBranches,
    taxRates: initialTaxRates,
    discounts: initialDiscounts,
    roles: initialRoles,
  },
  'tenant-03': {
    businessProfile: techBusinessProfile,
    registers: techRegisters,
    currentRegisterId: 'reg-tech-01',
    staff: techStaff,
    currentUser: techStaff[0],
    categories: techCategories,
    products: techProducts,
    customers: techCustomers,
    suppliers: techSuppliers,
    invoices: techInvoices,
    purchases: [],
    expenses: [],
    inventoryMovements: [],
    notifications: [
      {
        id: 'notif-t-01',
        title: 'Free Trial Active',
        message: 'You have 8 days left in your 14-day Free Starter Trial.',
        type: 'system',
        isRead: false,
        createdAt: '2026-10-04T08:00:00Z',
      },
    ],
    aiInsights: [],
    heldBills: [],
    salesReturns: [],
    purchaseReturns: [],
    creditBills: [],
    warehouses: initialWarehouses,
    branches: initialBranches,
    taxRates: initialTaxRates,
    discounts: initialDiscounts,
    roles: initialRoles,
  },
};

export const useStore = create<AppState>((set, get) => {
  const activeTenantId = 'tenant-01';
  const defaultTenantDb = initialTenantDatabases[activeTenantId];

  // Helper to persist active tenant data into tenantDatabases map
  const syncActiveTenant = (state: AppState): Record<string, TenantIsolatedData> => {
    return {
      ...state.tenantDatabases,
      [state.currentTenantId]: {
        businessProfile: state.businessProfile,
        registers: state.registers,
        currentRegisterId: state.currentRegisterId,
        staff: state.staff,
        currentUser: state.currentUser,
        categories: state.categories,
        products: state.products,
        customers: state.customers,
        suppliers: state.suppliers,
        invoices: state.invoices,
        purchases: state.purchases,
        expenses: state.expenses,
        inventoryMovements: state.inventoryMovements,
        notifications: state.notifications,
        aiInsights: state.aiInsights,
        heldBills: state.heldBills,
        salesReturns: state.salesReturns,
        purchaseReturns: state.purchaseReturns,
        creditBills: state.creditBills,
        warehouses: state.warehouses,
        branches: state.branches,
        taxRates: state.taxRates,
        discounts: state.discounts,
        roles: state.roles,
      },
    };
  };

  const resolveProductReferences = async (product: Partial<Product>) => {
    let categoryId = get().categories.find((category) => category.name.toLowerCase() === product.category?.toLowerCase())?.id;
    if (product.category && !categoryId) {
      const created = await api.products.createCategory({ name: product.category });
      categoryId = String(created.id);
      set((state) => ({ categories: [...state.categories, { id: categoryId!, name: product.category!, code: '', productCount: 0 }] }));
    }
    let taxRateId = get().taxRates.find((tax) => tax.rate === product.taxRate)?.id;
    if (product.taxRate && !taxRateId) {
      const created = await api.products.createTaxRate(product.taxRate);
      taxRateId = String(created.id);
      set((state) => ({ taxRates: [...state.taxRates, { id: taxRateId!, name: created.name, rate: product.taxRate! }] }));
    }
    let brandId: string | undefined;
    if (product.brand) {
      const brands = await api.products.getBrands();
      brandId = String(brands.find((brand) => brand.name.toLowerCase() === product.brand?.toLowerCase())?.id || '');
      if (!brandId) brandId = String((await api.products.createBrand(product.brand)).id);
    }
    let unitId: string | undefined;
    if (product.unit) {
      const units = await api.products.getUnits();
      unitId = String(units.find((unit) => unit.name.toLowerCase() === product.unit?.toLowerCase())?.id || '');
      if (!unitId) unitId = String((await api.products.createUnit(product.unit)).id);
    }
    return {
      categoryId: categoryId ? Number(categoryId) : undefined,
      taxRateId: taxRateId ? Number(taxRateId) : undefined,
      brandId: brandId ? Number(brandId) : undefined,
      unitId: unitId ? Number(unitId) : undefined,
    };
  };

  return {
    tenants: [{ ...initialTenants[0], name: 'Your business', storeName: 'Your business' }],
    currentTenantId: activeTenantId,
    isSuperAdminView: false,
    plans: SaaSPlans,
    auditLogs: initialAuditLogs,
    platformStats: initialPlatformStats,
    tenantDatabases: initialTenantDatabases,

    backendStatus: 'checking',
    backendUrl: api.getBaseUrl() || window.location.origin,

    // Active Tenant state
    businessProfile: { ...defaultTenantDb.businessProfile, name: 'your business', storeName: 'your business' },
    registers: defaultTenantDb.registers,
    currentRegisterId: defaultTenantDb.currentRegisterId,
    staff: defaultTenantDb.staff,
    currentUser: defaultTenantDb.currentUser,
    categories: [],
    products: [],
    customers: [],
    suppliers: [],
    invoices: [],
    purchases: [],
    expenses: [],
    inventoryMovements: [],
    notifications: [],
    aiInsights: [],
    salesReturns: [],
    purchaseReturns: [],
    creditBills: [],
    warehouses: [],
    branches: [],
    taxRates: [],
    discounts: [],
    roles: [],

    cart: [],
    selectedCustomer: defaultTenantDb.customers[0] || initialCustomers[0],
    heldBills: defaultTenantDb.heldBills,
    posExtraDiscount: 0,
    theme: 'light',

    // ==================== SAAS MULTI-TENANT ACTIONS ====================
    switchTenant: (tenantId: string) => {
      const state = get();
      if (state.currentTenantId === tenantId) return;

      // 1. Save current tenant database
      const updatedDatabases = syncActiveTenant(state);

      // 2. Load target tenant database
      const targetDb = updatedDatabases[tenantId];
      if (!targetDb) return;

      const targetTenant = state.tenants.find((t) => t.id === tenantId);

      // 3. Update active workspace state
      set({
        currentTenantId: tenantId,
        tenantDatabases: updatedDatabases,
        businessProfile: targetDb.businessProfile,
        registers: targetDb.registers,
        currentRegisterId: targetDb.currentRegisterId || targetDb.registers[0]?.id || 'reg-01',
        staff: targetDb.staff,
        currentUser: targetDb.currentUser || targetDb.staff[0],
        categories: targetDb.categories,
        products: targetDb.products,
        customers: targetDb.customers,
        suppliers: targetDb.suppliers,
        invoices: targetDb.invoices,
        purchases: targetDb.purchases,
        expenses: targetDb.expenses,
        inventoryMovements: targetDb.inventoryMovements,
        notifications: targetDb.notifications,
        aiInsights: targetDb.aiInsights,
        heldBills: targetDb.heldBills || [],
        salesReturns: targetDb.salesReturns || [],
        purchaseReturns: targetDb.purchaseReturns || [],
        creditBills: targetDb.creditBills || [],
        warehouses: targetDb.warehouses || initialWarehouses,
        branches: targetDb.branches || initialBranches,
        taxRates: targetDb.taxRates || initialTaxRates,
        discounts: targetDb.discounts || initialDiscounts,
        roles: targetDb.roles || initialRoles,
        cart: [],
        selectedCustomer: targetDb.customers[0] || initialCustomers[0],
        posExtraDiscount: 0,
      });

      // Add audit log
      if (targetTenant) {
        const newLog: SaaSAuditLog = {
          id: `log-${Date.now()}`,
          tenantId: targetTenant.id,
          tenantName: targetTenant.name,
          eventType: 'tenant_activated',
          description: `Switched active workspace to "${targetTenant.name}" (${targetTenant.subdomain})`,
          timestamp: new Date().toISOString(),
          severity: 'info',
        };
        set((s) => ({ auditLogs: [newLog, ...s.auditLogs] }));
      }
    },

    toggleSuperAdminView: (enabled) => {
      set((state) => ({
        isSuperAdminView: enabled !== undefined ? enabled : !state.isSuperAdminView,
      }));
    },

    createTenant: (params) => {
      const state = get();
      const newTenantId = `tenant-${Date.now()}`;
      const plan = state.plans.find((p) => p.id === params.planId) || state.plans[0];
      const monthlyFee = params.billingCycle === 'yearly' ? plan.priceYearly : plan.priceMonthly;

      const newTenant: Tenant = {
        id: newTenantId,
        name: params.name,
        storeName: params.storeName,
        slug: params.subdomain.toLowerCase().replace(/[^a-z0-9]/g, ''),
        subdomain: `${params.subdomain.toLowerCase().replace(/[^a-z0-9]/g, '')}.billflow.io`,
        ownerName: params.ownerName,
        ownerEmail: params.ownerEmail,
        ownerPhone: params.ownerPhone,
        industry: params.industry,
        planId: params.planId,
        status: 'active',
        billingCycle: params.billingCycle,
        subscriptionStartDate: new Date().toISOString().split('T')[0],
        nextRenewalDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
        trialEndsAt: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
        monthlyFee,
        totalPaid: monthlyFee,
        colorTheme: params.industry === 'restaurant' ? 'amber' : params.industry === 'electronics' ? 'cyan' : 'indigo',
        createdAt: new Date().toISOString(),
        subscriptionInvoices: [
          {
            id: `sub-inv-${Date.now()}`,
            invoiceNumber: `SUB-2026-${Math.floor(1000 + Math.random() * 9000)}`,
            date: new Date().toISOString().split('T')[0],
            amount: monthlyFee,
            planName: plan.name,
            billingCycle: params.billingCycle,
            status: 'paid',
            paymentMethod: 'Instant Online Checkout',
          },
        ],
      };

      // Create isolated business profile
      const newBizProfile: BusinessProfile = {
        id: `biz-${newTenantId}`,
        name: params.name,
        storeName: params.storeName,
        businessType: params.industry as any,
        gstin: '29ABCDE0000Z1Z1',
        address: '1st Floor, Commercial Complex',
        city: 'City Center',
        state: 'Karnataka',
        pincode: '560001',
        phone: params.ownerPhone,
        email: params.ownerEmail,
        currency: '₹',
        taxSettings: {
          defaultGstRate: params.industry === 'restaurant' ? 5 : 18,
          taxInclusive: true,
          enableCgstSgst: true,
          enableIgst: false,
        },
        invoiceSettings: {
          prefix: params.name.slice(0, 3).toUpperCase(),
          nextNumber: 101,
          termsAndConditions: 'Thank you for your business. Terms & warranty apply.',
          footerMessage: `Thank you for shopping at ${params.storeName}!`,
          showLogo: true,
        },
      };

      const newRegister: Register = {
        id: `reg-${newTenantId}-01`,
        name: 'Main Counter Register',
        code: 'POS-1',
        status: 'online',
        openingCash: 5000,
        currentCash: 5000,
        shiftStatus: 'open',
        currentCashier: params.ownerName,
      };

      const newOwnerStaff: UserStaff = {
        id: `usr-${newTenantId}-owner`,
        name: params.ownerName,
        phone: params.ownerPhone,
        email: params.ownerEmail,
        role: 'admin',
        status: 'active',
        permissions: {
          createBill: true,
          editBill: true,
          cancelBill: true,
          viewProducts: true,
          manageProducts: true,
          viewInventory: true,
          manageInventory: true,
          viewReports: true,
          viewProfit: true,
          manageStaff: true,
          manageSettings: true,
        },
      };

      const newWalkinCust: Customer = {
        id: `cust-${newTenantId}-walkin`,
        name: 'Walk-in Customer',
        phone: '+91 00000 00000',
        type: 'walkin',
        totalPurchases: 0,
        outstandingBalance: 0,
        loyaltyPoints: 0,
        createdAt: new Date().toISOString().split('T')[0],
      };

      // Build initial sample categories/products according to industry
      const sampleCategories: Category[] =
        params.industry === 'restaurant'
          ? [
              { id: `cat-${newTenantId}-1`, name: 'Main Courses', code: 'MAIN', productCount: 2 },
              { id: `cat-${newTenantId}-2`, name: 'Beverages', code: 'BEV', productCount: 2 },
            ]
          : [
              { id: `cat-${newTenantId}-1`, name: 'General Retail', code: 'GEN', productCount: 2 },
              { id: `cat-${newTenantId}-2`, name: 'Accessories', code: 'ACC', productCount: 2 },
            ];

      const sampleProducts: Product[] = [
        {
          id: `prod-${newTenantId}-1`,
          name: params.industry === 'restaurant' ? 'Signature Chef Platter' : 'Standard Product Item A',
          sku: 'SKU-001',
          barcode: '890999000101',
          category: sampleCategories[0].name,
          brand: params.name,
          description: 'Initial catalog item setup during workspace onboarding',
          purchasePrice: 150,
          sellingPrice: 299,
          mrp: 350,
          taxRate: params.industry === 'restaurant' ? 5 : 18,
          unit: 'pcs',
          stock: 50,
          minStock: 10,
          status: 'active',
          createdAt: new Date().toISOString().split('T')[0],
        },
      ];

      const newTenantDb: TenantIsolatedData = {
        businessProfile: newBizProfile,
        registers: [newRegister],
        currentRegisterId: newRegister.id,
        staff: [newOwnerStaff],
        currentUser: newOwnerStaff,
        categories: sampleCategories,
        products: sampleProducts,
        customers: [newWalkinCust],
        suppliers: [],
        invoices: [],
        purchases: [],
        expenses: [],
        inventoryMovements: [],
        notifications: [
          {
            id: `notif-${newTenantId}-welcome`,
            title: 'Welcome to BillFlow SaaS!',
            message: `Workspace "${params.name}" was successfully provisioned on the ${plan.name} plan.`,
            type: 'system',
            isRead: false,
            createdAt: new Date().toISOString(),
          },
        ],
        aiInsights: [],
        heldBills: [],
        salesReturns: [],
        purchaseReturns: [],
        creditBills: [],
        warehouses: initialWarehouses,
        branches: initialBranches,
        taxRates: initialTaxRates,
        discounts: initialDiscounts,
        roles: initialRoles,
      };

      const updatedDatabases = {
        ...syncActiveTenant(state),
        [newTenantId]: newTenantDb,
      };

      const newLog: SaaSAuditLog = {
        id: `log-${Date.now()}`,
        tenantId: newTenantId,
        tenantName: params.name,
        eventType: 'tenant_created',
        description: `New tenant workspace "${params.name}" provisioned on ${plan.name} (${params.billingCycle})`,
        timestamp: new Date().toISOString(),
        severity: 'success',
      };

      set({
        tenants: [newTenant, ...state.tenants],
        tenantDatabases: updatedDatabases,
        auditLogs: [newLog, ...state.auditLogs],
        platformStats: {
          ...state.platformStats,
          totalTenants: state.platformStats.totalTenants + 1,
          activePaidTenants: state.platformStats.activePaidTenants + 1,
          monthlyRecurringRevenue: state.platformStats.monthlyRecurringRevenue + monthlyFee,
          annualRecurringRevenue: state.platformStats.annualRecurringRevenue + monthlyFee * 12,
        },
      });

      // Automatically switch to the newly created tenant
      get().switchTenant(newTenantId);

      return newTenant;
    },

    updateTenant: (tenantId, updates) => {
      set((state) => ({
        tenants: state.tenants.map((t) => (t.id === tenantId ? { ...t, ...updates } : t)),
      }));
    },

    upgradeTenantPlan: (tenantId, newPlanId, billingCycle) => {
      const state = get();
      const targetTenant = state.tenants.find((t) => t.id === tenantId);
      const plan = state.plans.find((p) => p.id === newPlanId);
      if (!targetTenant || !plan) return;

      const fee = billingCycle === 'yearly' ? plan.priceYearly : plan.priceMonthly;
      const invoiceAmount = billingCycle === 'yearly' ? plan.priceYearly * 12 : plan.priceMonthly;

      const newInvoice: TenantSubscriptionInvoice = {
        id: `sub-inv-${Date.now()}`,
        invoiceNumber: `SUB-2026-${Math.floor(1000 + Math.random() * 9000)}`,
        date: new Date().toISOString().split('T')[0],
        amount: invoiceAmount,
        planName: `${plan.name} (${billingCycle === 'yearly' ? 'Annual' : 'Monthly'})`,
        billingCycle,
        status: 'paid',
        paymentMethod: 'Credit Card (Instant Online Upgrade)',
      };

      const updatedTenants = state.tenants.map((t) => {
        if (t.id === tenantId) {
          return {
            ...t,
            planId: newPlanId,
            status: 'active' as TenantStatus,
            billingCycle,
            monthlyFee: fee,
            totalPaid: t.totalPaid + invoiceAmount,
            nextRenewalDate: new Date(Date.now() + (billingCycle === 'yearly' ? 365 : 30) * 24 * 60 * 60 * 1000)
              .toISOString()
              .split('T')[0],
            subscriptionInvoices: [newInvoice, ...t.subscriptionInvoices],
          };
        }
        return t;
      });

      const auditLog: SaaSAuditLog = {
        id: `log-${Date.now()}`,
        tenantId,
        tenantName: targetTenant.name,
        eventType: 'plan_upgraded',
        description: `Plan upgraded to ${plan.name} (${billingCycle}). Subscription payment ₹${invoiceAmount.toLocaleString()} collected.`,
        timestamp: new Date().toISOString(),
        severity: 'success',
      };

      set({
        tenants: updatedTenants,
        auditLogs: [auditLog, ...state.auditLogs],
        platformStats: {
          ...state.platformStats,
          monthlyRecurringRevenue: state.platformStats.monthlyRecurringRevenue + (fee - targetTenant.monthlyFee),
          annualRecurringRevenue: state.platformStats.annualRecurringRevenue + (fee - targetTenant.monthlyFee) * 12,
        },
      });
    },

    toggleTenantStatus: (tenantId, status) => {
      const state = get();
      const targetTenant = state.tenants.find((t) => t.id === tenantId);
      if (!targetTenant) return;

      const updatedTenants = state.tenants.map((t) => (t.id === tenantId ? { ...t, status } : t));

      const auditLog: SaaSAuditLog = {
        id: `log-${Date.now()}`,
        tenantId,
        tenantName: targetTenant.name,
        eventType: status === 'suspended' ? 'tenant_suspended' : 'tenant_activated',
        description: `Tenant status changed to "${status.toUpperCase()}"`,
        timestamp: new Date().toISOString(),
        severity: status === 'suspended' ? 'danger' : 'info',
      };

      set({
        tenants: updatedTenants,
        auditLogs: [auditLog, ...state.auditLogs],
      });
    },

    extendTenantTrial: (tenantId, days) => {
      const state = get();
      const targetTenant = state.tenants.find((t) => t.id === tenantId);
      if (!targetTenant) return;

      const currentEnd = new Date(targetTenant.trialEndsAt).getTime() || Date.now();
      const newTrialEnd = new Date(currentEnd + days * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

      const updatedTenants = state.tenants.map((t) =>
        t.id === tenantId ? { ...t, trialEndsAt: newTrialEnd, status: 'trial' as TenantStatus } : t
      );

      const auditLog: SaaSAuditLog = {
        id: `log-${Date.now()}`,
        tenantId,
        tenantName: targetTenant.name,
        eventType: 'trial_extended',
        description: `Free trial extended by ${days} days until ${newTrialEnd}`,
        timestamp: new Date().toISOString(),
        severity: 'info',
      };

      set({
        tenants: updatedTenants,
        auditLogs: [auditLog, ...state.auditLogs],
      });
    },

    getActiveTenant: () => {
      const state = get();
      return state.tenants.find((t) => t.id === state.currentTenantId) || state.tenants[0];
    },

    getActivePlan: () => {
      const state = get();
      const tenant = state.getActiveTenant();
      return state.plans.find((p) => p.id === tenant.planId) || state.plans[0];
    },

    getTenantQuotaUsage: (tenantId) => {
      const state = get();
      const id = tenantId || state.currentTenantId;
      const db = id === state.currentTenantId ? state : state.tenantDatabases[id];

      if (!db) {
        return { invoicesThisMonth: 0, productsCount: 0, staffCount: 0, registersCount: 0 };
      }

      return {
        invoicesThisMonth: db.invoices.length,
        productsCount: db.products.length,
        staffCount: db.staff.length,
        registersCount: db.registers.length,
      };
    },

    // ==================== WORKSPACE ACTIONS ====================
    setBusinessProfile: (profileData) =>
      set((state) => ({
        businessProfile: { ...state.businessProfile, ...profileData },
      })),

    setCurrentRegisterId: (registerId) => set({ currentRegisterId: registerId }),

    setSelectedCustomer: (customer) => set({ selectedCustomer: customer }),

    setPosExtraDiscount: (amount) => set({ posExtraDiscount: amount }),

    addToCart: (product, qty = 1) => {
      set((state) => {
        const existingIndex = state.cart.findIndex((item) => item.product.id === product.id);
        if (existingIndex > -1) {
          const updatedCart = [...state.cart];
          const existingItem = updatedCart[existingIndex];
          const newQty = existingItem.quantity + qty;
          const linePrice = existingItem.customPrice ?? product.sellingPrice;
          const lineTotal = linePrice * newQty * (1 - existingItem.discountPercentage / 100);
          const taxAmount = (lineTotal * product.taxRate) / (100 + product.taxRate);

          updatedCart[existingIndex] = {
            ...existingItem,
            quantity: newQty,
            lineTotal,
            taxAmount,
          };
          return { cart: updatedCart };
        } else {
          const linePrice = product.sellingPrice;
          const lineTotal = linePrice * qty;
          const taxAmount = (lineTotal * product.taxRate) / (100 + product.taxRate);
          const newItem: CartItem = {
            product,
            quantity: qty,
            discountPercentage: 0,
            lineTotal,
            taxAmount,
          };
          return { cart: [...state.cart, newItem] };
        }
      });
    },

    removeFromCart: (productId) =>
      set((state) => ({
        cart: state.cart.filter((item) => item.product.id !== productId),
      })),

    updateCartQuantity: (productId, quantity) => {
      if (quantity <= 0) {
        get().removeFromCart(productId);
        return;
      }
      set((state) => {
        const updatedCart = state.cart.map((item) => {
          if (item.product.id === productId) {
            const linePrice = item.customPrice ?? item.product.sellingPrice;
            const lineTotal = linePrice * quantity * (1 - item.discountPercentage / 100);
            const taxAmount = (lineTotal * item.product.taxRate) / (100 + item.product.taxRate);
            return {
              ...item,
              quantity,
              lineTotal,
              taxAmount,
            };
          }
          return item;
        });
        return { cart: updatedCart };
      });
    },

    updateCartDiscount: (productId, discountPercentage) => {
      set((state) => {
        const updatedCart = state.cart.map((item) => {
          if (item.product.id === productId) {
            const linePrice = item.customPrice ?? item.product.sellingPrice;
            const lineTotal = linePrice * item.quantity * (1 - discountPercentage / 100);
            const taxAmount = (lineTotal * item.product.taxRate) / (100 + item.product.taxRate);
            return {
              ...item,
              discountPercentage,
              lineTotal,
              taxAmount,
            };
          }
          return item;
        });
        return { cart: updatedCart };
      });
    },

    clearCart: () =>
      set((state) => ({
        cart: [],
        selectedCustomer: state.customers[0] || initialCustomers[0],
        posExtraDiscount: 0,
      })),

    holdCurrentBill: (note) => {
      const { cart, selectedCustomer, posExtraDiscount, heldBills, customers } = get();
      if (cart.length === 0) return null;

      const subtotal = cart.reduce((sum, item) => sum + item.product.sellingPrice * item.quantity, 0);
      const taxTotal = cart.reduce((sum, item) => sum + item.taxAmount, 0);
      const itemDiscount = cart.reduce(
        (sum, item) => sum + item.product.sellingPrice * item.quantity * (item.discountPercentage / 100),
        0
      );
      const grandTotal = Math.max(0, subtotal - itemDiscount - posExtraDiscount);

      const newHeldBill: HeldBill = {
        id: `held-${Date.now()}`,
        billNumber: `HOLD-${heldBills.length + 1}`,
        customer: selectedCustomer,
        items: [...cart],
        subtotal,
        discount: itemDiscount + posExtraDiscount,
        taxTotal,
        grandTotal,
        heldAt: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        note,
      };

      set({
        heldBills: [newHeldBill, ...heldBills],
        cart: [],
        selectedCustomer: customers[0] || initialCustomers[0],
        posExtraDiscount: 0,
      });

      return newHeldBill;
    },

    resumeHeldBill: (heldBillId) => {
      const { heldBills } = get();
      const held = heldBills.find((b) => b.id === heldBillId);
      if (!held) return;

      set({
        cart: held.items,
        selectedCustomer: held.customer,
        heldBills: heldBills.filter((b) => b.id !== heldBillId),
      });
    },

    deleteHeldBill: (heldBillId) =>
      set((state) => ({
        heldBills: state.heldBills.filter((b) => b.id !== heldBillId),
      })),

    completeSale: async ({ paymentMethod, amountReceived, notes }) => {
      const state = get();
      if (!state.cart.length) throw new Error('Cart is empty.');
      if (state.cart.some((item) => !/^\d+$/.test(item.product.id))) {
        throw new Error('Refresh products before billing.');
      }
      const customerId = /^\d+$/.test(state.selectedCustomer.id) ? Number(state.selectedCustomer.id) : null;
      if (paymentMethod === 'credit' && !customerId) {
        throw new Error('Select a saved customer for a credit sale.');
      }
      if (paymentMethod === 'split') {
        throw new Error('Split payments are not supported by the invoice API yet.');
      }
      const created = await api.invoices.create({
        customerId,
        paymentMethod: ({ cash: 1, card: 2, upi: 3, credit: 6 } as Record<string, number>)[paymentMethod],
        salesPersonId: /^\d+$/.test(state.currentUser.id) ? Number(state.currentUser.id) : null,
        discountAmount: state.posExtraDiscount,
        notes,
        items: state.cart.map((item) => ({
          productId: Number(item.product.id),
          quantity: item.quantity,
          unitPrice: item.customPrice ?? item.product.sellingPrice,
          discountType: 1,
          discountValue: item.discountPercentage,
        })),
      } as any);
      const invoice: Invoice = {
        ...mapApiInvoice(created),
        customerId: customerId ? String(customerId) : '',
        customerName: state.selectedCustomer.name,
        customerPhone: state.selectedCustomer.phone,
        paymentMethod,
        amountReceived: paymentMethod === 'credit' ? 0 : amountReceived,
      };
      set((current) => ({
        invoices: [invoice, ...current.invoices],
        products: current.products.map((product) => {
          const item = state.cart.find((entry) => entry.product.id === product.id);
          return item ? { ...product, stock: product.stock - item.quantity } : product;
        }),
        cart: [],
        posExtraDiscount: 0,
      }));
      // Refresh derived balances and server-calculated totals after a successful save.
      void get().syncWithBackend().catch(() => {});
      return invoice;
    },
    // ==================== BACKEND HEALTH & SYNC ====================
    setBackendStatus: (status) => set({ backendStatus: status }),

    checkBackendHealth: async () => {
      set({ backendStatus: 'checking' });
      try {
        const isOnline = await api.checkHealth();
        set({ backendStatus: isOnline ? 'connected' : 'offline' });
        return isOnline;
      } catch {
        set({ backendStatus: 'offline' });
        return false;
      }
    },

    syncWithBackend: async () => {
      if (!api.getToken()) throw new Error('Please sign in to load your data.');
      set({ backendStatus: 'checking' });
      try {
        const [
          prodsRes,
          custsRes,
          suppRes,
          invsRes,
          salesRetRes,
          purchRes,
          purchRetRes,
          creditBillsRes,
          categoriesRes,
          taxRatesRes,
          discountsRes,
          notificationsRes,
          warehousesRes,
          branchesRes,
          expensesRes,
          businessRes,
          profileRes,
          usersRes,
          invoiceSettingsRes,
        ] = await Promise.allSettled([
          api.products.list(),
          api.customers.list(),
          api.suppliers.list(),
          api.invoices.list(),
          api.salesReturns.list(),
          api.purchases.list(),
          api.purchaseReturns.list(),
          api.creditBills.getMy(),
          api.products.getCategories(),
          api.products.getTaxRates(),
          api.products.getDiscounts(),
          api.notifications.list(),
          api.inventory.getWarehouses(),
          api.business.getBranches(),
          api.expenses.list(),
          api.business.getInfo(),
          api.auth.getProfile(),
          api.users.list(),
          api.business.getInvoiceSettings(),
        ]);

        if (prodsRes.status === 'rejected' || custsRes.status === 'rejected' || invsRes.status === 'rejected') {
          throw new Error('Could not load account data from the API.');
        }
        const updates: Partial<AppState> = { backendStatus: 'connected' };

        if (prodsRes.status === 'fulfilled' && Array.isArray(prodsRes.value))
          updates.products = prodsRes.value.map(mapApiProduct);

        if (custsRes.status === 'fulfilled' && Array.isArray(custsRes.value))
          updates.customers = custsRes.value.map(mapApiCustomer);

        if (suppRes.status === 'fulfilled' && Array.isArray(suppRes.value))
          updates.suppliers = suppRes.value.map(mapApiSupplier);

        if (invsRes.status === 'fulfilled' && Array.isArray(invsRes.value))
          updates.invoices = invsRes.value.map(mapApiInvoice);

        if (salesRetRes.status === 'fulfilled' && Array.isArray(salesRetRes.value))
          updates.salesReturns = salesRetRes.value.map(mapApiSalesReturn);

        if (purchRes.status === 'fulfilled' && Array.isArray(purchRes.value))
          updates.purchases = purchRes.value.map(mapApiPurchase);

        if (purchRetRes.status === 'fulfilled' && Array.isArray(purchRetRes.value))
          updates.purchaseReturns = purchRetRes.value.map(mapApiPurchaseReturn);

        if (creditBillsRes.status === 'fulfilled' && Array.isArray(creditBillsRes.value))
          updates.creditBills = creditBillsRes.value.map(mapApiCreditBill);

        if (categoriesRes.status === 'fulfilled' && Array.isArray(categoriesRes.value)) {
          updates.categories = (categoriesRes.value as any[]).map((c: any) => ({
            id: String(c.id),
            name: String(c.name),
            code: c.code || '',
            productCount: c.productCount || 0,
          }));
        }

        if (taxRatesRes.status === 'fulfilled' && Array.isArray(taxRatesRes.value)) {
          updates.taxRates = (taxRatesRes.value as any[]).map((t: any) => ({
            id: String(t.id),
            name: String(t.name),
            rate: Number(t.percentage ?? t.rate ?? 0),
            isDefault: !!t.isDefault,
          }));
        }

        if (discountsRes.status === 'fulfilled' && Array.isArray(discountsRes.value)) {
          updates.discounts = (discountsRes.value as any[]).map((d: any) => ({
            id: String(d.id),
            name: String(d.name),
            code: String(d.code || ''),
            discountType: d.discountType || 'percentage',
            value: Number(d.value),
            minBillAmount: d.minBillAmount,
            active: !!d.active,
          }));
        }

        if (notificationsRes.status === 'fulfilled' && Array.isArray(notificationsRes.value)) {
          updates.notifications = (notificationsRes.value as any[]).map((n: any) => ({
            id: String(n.id),
            title: String(n.title || ''),
            message: String(n.message || ''),
            type: n.type || 'system',
            isRead: !!n.isRead,
            createdAt: String(n.createdAt || new Date().toISOString()),
          }));
        }

        if (warehousesRes.status === 'fulfilled' && Array.isArray(warehousesRes.value)) {
          updates.warehouses = (warehousesRes.value as any[]).map((w: any) => ({
            id: String(w.id),
            name: String(w.name),
            code: String(w.code || ''),
            location: String(w.location || w.address || ''),
            capacity: w.capacity,
            managerName: w.managerName,
            phone: w.phone,
            isPrimary: !!w.isPrimary,
          }));
        }

        if (branchesRes.status === 'fulfilled' && Array.isArray(branchesRes.value)) {
          updates.branches = (branchesRes.value as any[]).map((b: any) => ({
            id: String(b.id),
            name: String(b.name),
            code: String(b.code || ''),
            address: String(b.address || ''),
            city: String(b.city || ''),
            state: String(b.state || ''),
            phone: String(b.phone || ''),
            gstin: b.gstin,
            isMainBranch: !!b.isMainBranch,
          }));
        }

        if (expensesRes.status === 'fulfilled' && Array.isArray(expensesRes.value))
          updates.expenses = expensesRes.value.map(mapApiExpense);

        if (businessRes.status === 'fulfilled' && businessRes.value) {
          const business = businessRes.value as any;
          updates.businessProfile = {
            ...get().businessProfile,
            id: String(business.id),
            name: business.name,
            storeName: business.name,
            email: business.email || '',
            phone: business.phone || '',
            address: business.address || '',
            city: business.city || '',
            state: business.state || '',
            pincode: business.pinCode || '',
            gstin: business.gSTNumber || '',
            currency: business.currencySymbol || '₹',
          };
          const tenantId = String(business.id);
          updates.currentTenantId = tenantId;
          updates.tenants = [{ ...get().tenants[0], id: tenantId, name: business.name, storeName: business.name, ownerEmail: business.email || '' }];
        }

        if (profileRes.status === 'fulfilled' && profileRes.value) {
          const profile = profileRes.value;
          updates.currentUser = mapApiStaff(profile);
        }

        if (usersRes.status === 'fulfilled' && Array.isArray(usersRes.value))
          updates.staff = usersRes.value.map(mapApiStaff);

        if (invoiceSettingsRes.status === 'fulfilled' && invoiceSettingsRes.value) {
          const settings = invoiceSettingsRes.value as any;
          updates.businessProfile = {
            ...(updates.businessProfile || get().businessProfile),
            invoiceSettings: {
              ...(updates.businessProfile || get().businessProfile).invoiceSettings,
              prefix: settings.invoicePrefix || 'INV',
              nextNumber: Number(settings.nextInvoiceNumber ?? 1),
              termsAndConditions: settings.termsAndConditions || '',
              footerMessage: settings.footerNote || '',
              showLogo: settings.showLogo ?? true,
            },
          };
        }

        if (updates.salesReturns) {
          updates.salesReturns = updates.salesReturns.map((ret) => {
            const invoice = (updates.invoices || get().invoices).find((item) => item.id === ret.invoiceId);
            const customer = (updates.customers || get().customers).find((item) => item.id === ret.customerId);
            return {
              ...ret,
              invoiceNumber: invoice?.invoiceNumber || ret.invoiceNumber,
              customerName: customer?.name || invoice?.customerName || ret.customerName,
              items: ret.items.map((item) => ({
                ...item,
                productName: (updates.products || get().products).find((product) => product.id === item.productId)?.name || item.productName,
              })),
            };
          });
        }
        if (updates.purchaseReturns) {
          updates.purchaseReturns = updates.purchaseReturns.map((ret) => {
            const purchase = (updates.purchases || get().purchases).find((item) => item.id === ret.purchaseId);
            const supplier = (updates.suppliers || get().suppliers).find((item) => item.id === ret.supplierId);
            return {
              ...ret,
              purchaseNumber: purchase?.purchaseNumber || ret.purchaseNumber,
              supplierName: supplier?.name || purchase?.supplierName || ret.supplierName,
              items: ret.items.map((item) => ({
                ...item,
                productName: (updates.products || get().products).find((product) => product.id === item.productId)?.name || item.productName,
              })),
            };
          });
        }
        if (updates.customers) updates.selectedCustomer = updates.customers[0] || initialCustomers[0];
        set(updates);
      } catch (error) {
        set({ backendStatus: 'offline' });
        throw error;
      }
    },

    // ==================== SALES RETURNS ====================
    addSalesReturn: async (returnData) => {
      if (!returnData.invoiceId || !/^\d+$/.test(returnData.invoiceId) || !returnData.items?.length)
        throw new Error('Select an invoice and return items.');
      const created = await api.salesReturns.create({
        invoiceId: Number(returnData.invoiceId),
        customerId: returnData.customerId && /^\d+$/.test(returnData.customerId) ? Number(returnData.customerId) : null,
        reason: returnData.returnReason || '',
        items: returnData.items.map((item) => ({
          productId: Number(item.productId), quantity: item.quantity, unitPrice: item.unitPrice,
        })),
      } as any);
      const mapped = mapApiSalesReturn(created);
      const salesReturn: SalesReturn = {
        ...mapped,
        invoiceNumber: returnData.invoiceNumber || '',
        customerName: returnData.customerName || '',
        refundMethod: returnData.refundMethod || 'cash',
        items: mapped.items.map((item) => ({
          ...item,
          productName: returnData.items?.find((original) => original.productId === item.productId)?.productName || '',
        })),
      };
      set((state) => ({
        salesReturns: [salesReturn, ...state.salesReturns],
        products: state.products.map((product) => {
          const item = salesReturn.items.find((entry) => entry.productId === product.id);
          return item ? { ...product, stock: product.stock + item.quantity } : product;
        }),
      }));
      return salesReturn;
    },

    addPurchaseReturn: async (returnData) => {
      if (!returnData.purchaseId || !/^\d+$/.test(returnData.purchaseId) || !returnData.items?.length)
        throw new Error('Select a purchase and return items.');
      const created = await api.purchaseReturns.create({
        purchaseId: Number(returnData.purchaseId),
        reason: returnData.returnReason || '',
        items: returnData.items.map((item) => ({
          productId: Number(item.productId), quantity: item.quantity, unitPrice: item.purchasePrice,
        })),
      } as any);
      const mapped = mapApiPurchaseReturn(created);
      const purchaseReturn: PurchaseReturn = {
        ...mapped,
        purchaseNumber: returnData.purchaseNumber || '',
        supplierName: returnData.supplierName || '',
        items: mapped.items.map((item) => ({
          ...item,
          productName: returnData.items?.find((original) => original.productId === item.productId)?.productName || '',
        })),
      };
      set((state) => ({
        purchaseReturns: [purchaseReturn, ...state.purchaseReturns],
        products: state.products.map((product) => {
          const item = purchaseReturn.items.find((entry) => entry.productId === product.id);
          return item ? { ...product, stock: product.stock - item.quantity } : product;
        }),
      }));
      return purchaseReturn;
    },
    // ==================== CREDIT BILLS (KHATA & SETTLEMENT) ====================
    addCreditBill: async (billData) => {
      if (!billData.customerId || !/^\d+$/.test(billData.customerId))
        throw new Error('Select a saved customer.');
      const created = await api.creditBills.create({
        ...billData,
        userId: get().currentUser.id,
      });
      const creditBill: CreditBill = {
        ...mapApiCreditBill(created),
        customerName: billData.customerName || '',
        customerPhone: billData.customerPhone,
        userName: get().currentUser.name,
      };
      set((state) => ({
        creditBills: [creditBill, ...state.creditBills],
        customers: state.customers.map((customer) => customer.id === creditBill.customerId
          ? { ...customer, outstandingBalance: customer.outstandingBalance + creditBill.amount }
          : customer),
      }));
      return creditBill;
    },

    settleCreditBill: async (settleData) => {
      const bill = get().creditBills.find((item) => item.id === settleData.creditBillId);
      if (!bill) throw new Error('Credit bill not found.');
      const result = await api.creditBills.settle(settleData);
      const updated = mapApiCreditBill(result);
      set((state) => ({
        creditBills: state.creditBills.map((item) => item.id === bill.id
          ? { ...item, ...updated, customerName: item.customerName }
          : item),
        customers: state.customers.map((customer) => customer.id === bill.customerId
          ? { ...customer, outstandingBalance: Math.max(0, customer.outstandingBalance - settleData.settleAmount) }
          : customer),
      }));
      return {
        success: true,
        message: `Settlement of ₹${settleData.settleAmount.toLocaleString()} recorded successfully`,
        balanceRemaining: updated.balanceAmount,
        settlementId: updated.id,
      };
    },
    getCreditBillsByUser: (userId) => {
      const { creditBills } = get();
      return creditBills.filter((b) => String(b.userId) === String(userId));
    },

    getMyCreditBills: () => {
      const { creditBills, currentUser } = get();
      return creditBills.filter((b) => String(b.userId) === String(currentUser.id) || b.userId === '1');
    },

    getUserCreditBalance: (userId) => {
      const { creditBills } = get();
      return creditBills
        .filter((b) => String(b.userId) === String(userId))
        .reduce((sum, b) => sum + b.balanceAmount, 0);
    },

    getMyCreditBalance: () => {
      const { creditBills, currentUser } = get();
      return creditBills
        .filter((b) => String(b.userId) === String(currentUser.id) || b.userId === '1')
        .reduce((sum, b) => sum + b.balanceAmount, 0);
    },

    // ==================== PRODUCT MINI-STATEMENT ====================
    getProductMiniStatement: async (productId: string): Promise<ProductMiniStatement> => {
      const state = get();
      const product = state.products.find((p) => p.id === productId);
      if (!product) throw new Error('Product not found');

      // Generate ledger entries from movements, sales, purchases, and returns
      const productMovements = state.inventoryMovements.filter((m) => m.productId === productId);
      const entries = productMovements.map((mov) => ({
        id: mov.id,
        date: mov.date,
        transactionType: mov.movementType as any,
        referenceNumber: mov.referenceNumber,
        partyName: mov.movementType === 'sale' ? 'POS Customer' : 'Vendor Supply / Adjustment',
        quantityIn: mov.movementType === 'purchase' || mov.movementType === 'in' || mov.movementType === 'return' ? mov.quantity : 0,
        quantityOut: mov.movementType === 'sale' || mov.movementType === 'out' ? mov.quantity : 0,
        unitPrice: mov.movementType === 'purchase' ? product.purchasePrice : product.sellingPrice,
        balanceStock: mov.newStock,
        notes: mov.reason,
      }));

      // If empty, add opening balance entry
      if (entries.length === 0) {
        entries.push({
          id: `mov-init-${product.id}`,
          date: product.createdAt,
          transactionType: 'opening',
          referenceNumber: 'OPENING-BAL',
          partyName: 'Store Inventory Depot',
          quantityIn: product.stock,
          quantityOut: 0,
          unitPrice: product.purchasePrice,
          balanceStock: product.stock,
          notes: 'Initial opening stock balance',
        });
      }

      return {
        productId: product.id,
        productName: product.name,
        sku: product.sku,
        currentStock: product.stock,
        sellingPrice: product.sellingPrice,
        purchasePrice: product.purchasePrice,
        unit: product.unit,
        entries,
      };
    },

    updateProductExpiryDate: async (id: string, expiryDate: string) => {
      await api.products.update(id, { expiryDate } as any);
      set((state) => ({
        products: state.products.map((p) => (p.id === id ? { ...p, expiryDate } : p)),
      }));
    },

    // Products & Inventory Actions
    addProduct: async (productData) => {
      const references = await resolveProductReferences(productData);
      const created = await api.products.create({
        name: productData.name,
        sku: productData.sku,
        barcode: productData.barcode,
        description: productData.description,
        ...references,
        purchasePrice: productData.purchasePrice,
        sellingPrice: productData.sellingPrice,
        mrp: productData.mrp,
        currentStock: productData.stock,
        minStockLevel: productData.minStock,
        expiryDate: productData.ExpiryDate ?? productData.expiryDate ?? null,
      } as any);
      const product = mapApiProduct(created);
      set((state) => ({ products: [product, ...state.products] }));
      return product;
    },

    updateProduct: async (id, data) => {
      const existing = get().products.find((product) => product.id === id);
      if (!existing) throw new Error('Product not found.');
      const references = await resolveProductReferences(data);
      await api.products.update(id, {
        ...references,
        name: data.name,
        sku: data.sku,
        barcode: data.barcode,
        description: data.description,
        purchasePrice: data.purchasePrice,
        sellingPrice: data.sellingPrice,
        mrp: data.mrp,
        minStockLevel: data.minStock,
        expiryDate: data.ExpiryDate ?? data.expiryDate,
      } as any);
      if (data.stock !== undefined && data.stock !== existing.stock) {
        await api.inventory.adjustStock(id, { quantity: data.stock - existing.stock, reason: 'Product edited' });
      }
      const updated = mapApiProduct(await api.products.getById(id));
      set((state) => ({ products: state.products.map((product) => product.id === id ? updated : product) }));
    },

    deleteProduct: async (id) => {
      await api.products.delete(id);
      set((state) => ({ products: state.products.filter((product) => product.id !== id) }));
    },
    adjustStock: async (productId, deltaQty, reason) => {
      await api.inventory.adjustStock(productId, { quantity: deltaQty, reason });
      set((state) => ({
        products: state.products.map((product) =>
          product.id === productId
            ? { ...product, stock: product.stock + deltaQty }
            : product
        ),
      }));
    },
    addCustomer: async (data) => {
      const created = await api.customers.create({
        name: data.name, phone: data.phone, email: data.email,
        address: data.address, gSTNumber: data.gstin,
      } as any);
      const customer = mapApiCustomer(created);
      set((state) => ({ customers: [...state.customers, customer] }));
      return customer;
    },

    updateCustomer: async (id, data) => {
      const updated = await api.customers.update(id, {
        name: data.name, phone: data.phone, email: data.email,
        address: data.address, gSTNumber: data.gstin,
      } as any);
      const customer = mapApiCustomer(updated);
      set((state) => ({ customers: state.customers.map((item) => item.id === id ? customer : item) }));
    },
    recordCustomerPayment: async (customerId, amount, notes) => {
      await api.customers.recordPayment(customerId, amount, notes);
      set((state) => ({
        customers: state.customers.map((customer) => customer.id === customerId
          ? { ...customer, outstandingBalance: Math.max(0, customer.outstandingBalance - amount) }
          : customer),
      }));
    },
    addSupplier: async (data) => {
      const created = await api.suppliers.create({
        name: data.name, companyName: data.companyName, phone: data.phone,
        email: data.email, address: data.address, gSTNumber: data.gstin,
      } as any);
      const supplier = mapApiSupplier(created);
      set((state) => ({ suppliers: [...state.suppliers, supplier] }));
      return supplier;
    },

    updateSupplier: async (id, data) => {
      const updated = await api.suppliers.update(id, {
        name: data.name, companyName: data.companyName, phone: data.phone,
        email: data.email, address: data.address, gSTNumber: data.gstin,
      } as any);
      const supplier = mapApiSupplier(updated);
      set((state) => ({ suppliers: state.suppliers.map((item) => item.id === id ? supplier : item) }));
    },
    addPurchase: async (data) => {
      if (!/^\d+$/.test(data.supplierId) || data.items.some((item) => !/^\d+$/.test(item.productId))) {
        throw new Error('Refresh suppliers and products before creating a purchase.');
      }
      const created = await api.purchases.create({
        supplierId: Number(data.supplierId),
        purchaseDate: data.date,
        referenceNumber: data.invoiceRef,
        items: data.items.map((item) => ({
          productId: Number(item.productId),
          quantity: item.quantity,
          unitPrice: item.purchasePrice,
        })),
      } as any);
      const purchase = mapApiPurchase(created);
      set((state) => ({
        purchases: [purchase, ...state.purchases],
        products: state.products.map((product) => {
          const item = data.items.find((entry) => entry.productId === product.id);
          return item ? { ...product, stock: product.stock + item.quantity } : product;
        }),
      }));
      return purchase;
    },
    addExpense: async (data) => {
      const paymentMethod = ({ cash: 1, card: 2, upi: 3, bank_transfer: 4 } as Record<string, number>)[data.paymentMethod];
      const created = await api.expenses.create({
        amount: data.amount,
        expenseDate: data.date,
        description: data.title,
        notes: data.notes,
        paymentMethod,
      } as any);
      const expense = mapApiExpense(created);
      set((state) => ({ expenses: [expense, ...state.expenses] }));
      return expense;
    },

    deleteExpense: async (id) => {
      await api.expenses.delete(id);
      set((state) => ({ expenses: state.expenses.filter((expense) => expense.id !== id) }));
    },
    addStaff: async (data) => {
      const role = ({ admin: 3, manager: 3, cashier: 4, sales: 5 } as Record<string, number>)[data.role];
      const created = await api.users.create({
        fullName: data.name, email: data.email, phone: data.phone,
        password: data.password, role,
      } as any);
      const staff = mapApiStaff(created);
      set((state) => ({ staff: [...state.staff, staff] }));
      return staff;
    },

    updateStaff: async (id, data) => {
      try {
        await api.users.update(id, {
          fullName: data.name,
          email: data.email,
          phone: data.phone,
          role: data.role,
        } as any);
      } catch {
        // fallback to local only
      }
      set((state) => ({
        staff: state.staff.map((s) => (s.id === id ? { ...s, ...data } : s)),
      }));
    },

    deleteStaff: async (id) => {
      try {
        await api.users.delete(id);
      } catch {
        // fallback to local only
      }
      set((state) => ({ staff: state.staff.filter((s) => s.id !== id) }));
    },

    addRole: async (data) => {
      let newRole: Role;
      try {
        const created = await api.staff.createRole({ name: data.name, description: data.description });
        newRole = {
          id: String(created?.id ?? `role-${Date.now()}`),
          name: data.name,
          description: data.description,
          permissions: data.permissions,
        };
      } catch {
        newRole = {
          id: `role-${Date.now()}`,
          name: data.name,
          description: data.description,
          permissions: data.permissions,
        };
      }
      set((state) => ({ roles: [...state.roles, newRole] }));
      return newRole;
    },

    updateRole: async (id, data) => {
      try {
        await api.staff.updateRole(id, data);
      } catch {
        // fallback to local only
      }
      set((state) => ({
        roles: state.roles.map((r) => (r.id === id ? { ...r, ...data } : r)),
      }));
    },

    deleteRole: async (id) => {
      try {
        await api.staff.deleteRole(id);
      } catch {
        // fallback to local only
      }
      set((state) => ({ roles: state.roles.filter((r) => r.id !== id) }));
    },
    addWarehouse: (data) => {
      const newWarehouse: Warehouse = {
        ...data,
        id: `wh-${Date.now()}`,
      };
      set((state) => ({ warehouses: [...state.warehouses, newWarehouse] }));
      return newWarehouse;
    },

    addBranch: (data) => {
      const newBranch: Branch = {
        ...data,
        id: `br-${Date.now()}`,
      };
      set((state) => ({ branches: [...state.branches, newBranch] }));
      return newBranch;
    },

    markNotificationRead: async (id) => {
      await api.notifications.markRead(id);
      set((state) => ({
        notifications: state.notifications.map((n) => (n.id === id ? { ...n, isRead: true } : n)),
      }));
    },

    markAllNotificationsRead: async () => {
      await api.notifications.markAllRead();
      set((state) => ({
        notifications: state.notifications.map((n) => ({ ...n, isRead: true })),
      }));
    },

    toggleTheme: () =>
      set((state) => {
        const nextTheme = state.theme === 'light' ? 'dark' : 'light';
        if (nextTheme === 'dark') {
          document.documentElement.classList.add('dark');
        } else {
          document.documentElement.classList.remove('dark');
        }
        return { theme: nextTheme };
      }),
  };
});
