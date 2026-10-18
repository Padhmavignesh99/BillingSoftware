import type {
  Product,
  Customer,
  Supplier,
  Invoice,
  Purchase,
  Expense,
  NotificationItem,
  BusinessProfile,
  UserStaff,
} from '../types';
import type {
  AuthUser,
  LoginResponse,
  RegisterRequest,
  ProductCategory,
  Brand,
  Unit,
  TaxRate,
  DiscountScheme,
  SalesReturn,
  PurchaseReturn,
  CreditBill,
  CreditBalanceSummary,
  SettleCreditBillDTO,
  ProductMiniStatement,
  ExpiredStockItem,
  Warehouse,
  Branch,
  ExpenseCategory,
  Role,
} from '../types/api';

const API_BASE_URL = (import.meta as any).env?.VITE_API_BASE_URL || '';

// ─── ApiResponse wrapper shape returned by the backend ───────────────────────
interface ApiResponseWrapper<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
  totalCount?: number;
  page?: number;
  pageSize?: number;
}

class ApiService {
  private baseUrl: string = API_BASE_URL.replace(/\/$/, '');
  private token: string | null = null;
  private isOnline: boolean | null = null;

  constructor() {
    this.token = localStorage.getItem('billflow_jwt_token');
  }

  public setBaseUrl(url: string) {
    this.baseUrl = url.replace(/\/$/, '');
  }

  public getBaseUrl(): string {
    return this.baseUrl;
  }

  public setToken(token: string | null) {
    this.token = token;
    if (token) {
      localStorage.setItem('billflow_jwt_token', token);
    } else {
      localStorage.removeItem('billflow_jwt_token');
    }
  }

  public getToken(): string | null {
    return this.token || localStorage.getItem('billflow_jwt_token');
  }

  public hasRole(role: string): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const claims = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      const value = claims.role ?? claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      return Array.isArray(value) ? value.includes(role) : value === role;
    } catch {
      return false;
    }
  }

  // ─── Health check via public /health endpoint ─────────────────────────────
  public async checkHealth(): Promise<boolean> {
    try {
      const res = await fetch(`${this.baseUrl}/health`, {
        method: 'GET',
        signal: AbortSignal.timeout(3000),
      });
      this.isOnline = res.ok;
      return this.isOnline;
    } catch {
      this.isOnline = false;
      return false;
    }
  }

  public getOnlineStatus(): boolean | null {
    return this.isOnline;
  }

  private getHeaders(): HeadersInit {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };
    const token = this.getToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    return headers;
  }

  /**
   * Core fetch method.
   * The backend wraps every response as ApiResponse<T> = { success, message, data, … }.
   * This method unwraps the `.data` field automatically so callers receive T directly.
   * If the backend returns a non-wrapped response (e.g. the public /health endpoint),
   * the raw body is returned as-is.
   */
  public async request<T = any>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const url = `${this.baseUrl}${endpoint.startsWith('/') ? endpoint : '/' + endpoint}`;
    const headers = { ...this.getHeaders(), ...(options.headers as Record<string, string>) };

    try {
      const response = await fetch(url, {
        ...options,
        headers,
      });

      // Handle empty responses (e.g. 204 No Content)
      const text = await response.text();
      let json: any;
      try {
        json = text ? JSON.parse(text) : {};
      } catch {
        json = {};
      }

      if (!response.ok) {
        if (response.status === 401 && !endpoint.startsWith('/api/auth/')) {
          this.setToken(null);
        }
        // Extract message from ApiResponse wrapper or fallback
        const validationError = json?.errors && !Array.isArray(json.errors)
          ? Object.values(json.errors).flat().join(' ')
          : undefined;
        const msg =
          json?.message ||
          (json?.errors && json.errors[0]) ||
          validationError ||
          json?.title ||
          json?.error ||
          `HTTP error ${response.status}`;
        throw new Error(msg);
      }

      // Unwrap ApiResponse<T> wrapper if present
      // The backend returns: { success: bool, message: string, data: T, ... }
      if (json && typeof json === 'object' && 'success' in json) {
        const wrapped = json as ApiResponseWrapper<T>;
        if (!wrapped.success) {
          throw new Error(wrapped.message || 'API request failed');
        }
        return wrapped.data as T;
      }

      // Non-wrapped response (e.g. health endpoint returns plain object)
      return json as T;
    } catch (err: any) {
      console.warn(`[API Client] ${options.method || 'GET'} ${endpoint} failed:`, err.message);
      throw err;
    }
  }

  private async requestAll<T>(endpoint: string): Promise<T[]> {
    const pageSize = 100;
    const results: T[] = [];
    for (let page = 1; ; page++) {
      const separator = endpoint.includes('?') ? '&' : '?';
      const batch = await this.request<T[]>(`${endpoint}${separator}page=${page}&pageSize=${pageSize}`);
      results.push(...batch);
      if (batch.length < pageSize) return results;
    }
  }

  // ================= 1. AUTHENTICATION API =================
  public auth = {
    register: (data: RegisterRequest) =>
      this.request<LoginResponse>('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    login: (credentials: { email: string; password: string }) =>
      this.request<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(credentials),
      }),

    getProfile: () => this.request<AuthUser>('/api/auth/me'),

    refreshToken: (token: string, refreshToken: string) =>
      this.request<LoginResponse>('/api/auth/refresh-token', {
        method: 'POST',
        body: JSON.stringify({ token, refreshToken }),
      }),

    forgotPassword: (email: string) =>
      this.request<{ message: string }>('/api/auth/forgot-password', {
        method: 'POST',
        body: JSON.stringify({ email }),
      }),
  };

  // ================= 2. PRODUCTS & CATALOG API =================
  public products = {
    list: (params?: { category?: string; search?: string }) => {
      const query = new URLSearchParams();
      if (params?.category) query.append('category', params.category);
      if (params?.search) query.append('searchQuery', params.search);
      const qs = query.toString() ? `?${query.toString()}` : '';
      return this.requestAll<Product>(`/api/products${qs}`);
    },

    getById: (id: string) => this.request<Product>(`/api/products/${id}`),

    search: (query: string) =>
      this.request<Product[]>(`/api/products/search?query=${encodeURIComponent(query)}`),

    create: (product: Partial<Product> & { ExpiryDate?: string }) =>
      this.request<Product>('/api/products', {
        method: 'POST',
        body: JSON.stringify(product),
      }),

    update: (id: string, updates: Partial<Product> & { ExpiryDate?: string }) =>
      this.request<Product>(`/api/products/${id}`, {
        method: 'PUT',
        body: JSON.stringify(updates),
      }),

    delete: (id: string) =>
      this.request<{ success: boolean; message?: string }>(`/api/products/${id}`, {
        method: 'DELETE',
      }),

    getCategories: () => this.request<ProductCategory[]>('/api/product-categories'),

    createCategory: (category: Partial<ProductCategory>) =>
      this.request<ProductCategory>('/api/product-categories', {
        method: 'POST',
        body: JSON.stringify(category),
      }),

    getBrands: () => this.request<Brand[]>('/api/brands'),

    createBrand: (name: string) => this.request<Brand>('/api/brands', {
      method: 'POST', body: JSON.stringify({ name }),
    }),

    getUnits: () => this.request<Unit[]>('/api/units'),

    createUnit: (name: string) => this.request<Unit>('/api/units', {
      method: 'POST', body: JSON.stringify({ name, shortName: name }),
    }),

    getTaxRates: () => this.request<TaxRate[]>('/api/tax-rates'),

    createTaxRate: (percentage: number) => this.request<TaxRate>('/api/tax-rates', {
      method: 'POST', body: JSON.stringify({ name: `GST ${percentage}%`, percentage,
        cGSTPercentage: percentage / 2, sGSTPercentage: percentage / 2, iGSTPercentage: percentage }),
    }),

    getDiscounts: () => this.request<DiscountScheme[]>('/api/discounts'),
  };

  // ================= 3. CUSTOMERS API =================
  public customers = {
    list: () => this.requestAll<Customer>('/api/customers'),

    getById: (id: string) => this.request<Customer>(`/api/customers/${id}`),

    create: (customer: Partial<Customer>) =>
      this.request<Customer>('/api/customers', {
        method: 'POST',
        body: JSON.stringify(customer),
      }),

    update: (id: string, customer: Partial<Customer>) =>
      this.request<Customer>(`/api/customers/${id}`, {
        method: 'PUT',
        body: JSON.stringify(customer),
      }),

    recordPayment: (customerId: string, amount: number, notes?: string) =>
      this.request<any>('/api/customer-payments', {
        method: 'POST',
        body: JSON.stringify({ customerId: Number(customerId), amount, paymentMethod: 1, notes }),
      }),
  };

  // ================= 4. SALES / INVOICES API =================
  public invoices = {
    list: () => this.requestAll<Invoice>('/api/invoices'),

    getById: (id: string) => this.request<Invoice>(`/api/invoices/${id}`),

    create: (invoice: Partial<Invoice>) =>
      this.request<Invoice>('/api/invoices', {
        method: 'POST',
        body: JSON.stringify(invoice),
      }),
  };

  // ================= 5. SALES RETURNS API =================
  public salesReturns = {
    list: () => this.request<SalesReturn[]>('/api/sales-returns'),

    create: (returnData: Partial<SalesReturn>) =>
      this.request<SalesReturn>('/api/sales-returns', {
        method: 'POST',
        body: JSON.stringify(returnData),
      }),
  };

  // ================= 6. SUPPLIERS API =================
  public suppliers = {
    list: () => this.requestAll<Supplier>('/api/suppliers'),

    create: (supplier: Partial<Supplier>) =>
      this.request<Supplier>('/api/suppliers', {
        method: 'POST',
        body: JSON.stringify(supplier),
      }),

    update: (id: string, updates: Partial<Supplier>) =>
      this.request<Supplier>(`/api/suppliers/${id}`, {
        method: 'PUT',
        body: JSON.stringify(updates),
      }),
  };

  // ================= 7. PURCHASES API =================
  public purchases = {
    list: () => this.requestAll<Purchase>('/api/purchases'),

    getById: (id: string) => this.request<Purchase>(`/api/purchases/${id}`),

    create: (purchase: Partial<Purchase>) =>
      this.request<Purchase>('/api/purchases', {
        method: 'POST',
        body: JSON.stringify(purchase),
      }),
  };

  // ================= 8. PURCHASE RETURNS API =================
  public purchaseReturns = {
    list: () => this.request<PurchaseReturn[]>('/api/purchase-returns'),

    create: (returnData: Partial<PurchaseReturn>) =>
      this.request<PurchaseReturn>('/api/purchase-returns', {
        method: 'POST',
        body: JSON.stringify(returnData),
      }),
  };

  // ================= 9. CREDIT BILL API (USER-WISE & CUSTOMER) =================
  public creditBills = {
    create: (bill: Partial<CreditBill>) =>
      this.request<CreditBill>('/api/credit-bills', {
        method: 'POST',
        body: JSON.stringify({
          customerId: Number(bill.customerId),
          invoiceId: bill.invoiceId ? Number(bill.invoiceId) : null,
          userId: bill.userId ? Number(bill.userId) : null,
          amount: bill.amount,
          dueDate: bill.dueDate,
          notes: bill.notes,
        }),
      }),

    getByCustomer: (customerId: string) =>
      this.request<CreditBill[]>(`/api/credit-bills/customer/${customerId}`),

    getByUser: (userId: string | number) =>
      this.request<CreditBill[]>(`/api/credit-bills/user/${userId}`),

    getMy: () => this.request<CreditBill[]>('/api/credit-bills/my'),

    getUserBalance: (userId: string | number) =>
      this.request<CreditBalanceSummary>(`/api/credit-bills/user/${userId}/balance`),

    getMyBalance: () => this.request<CreditBalanceSummary>('/api/credit-bills/my/balance'),

    settle: (settleData: SettleCreditBillDTO) =>
      this.request<CreditBill>(
        '/api/credit-bills/settle',
        {
          method: 'POST',
          body: JSON.stringify({
            creditId: Number(settleData.creditBillId),
            paymentAmount: settleData.settleAmount,
            paymentMethod: ({ cash: 1, card: 2, upi: 3, bank_transfer: 4 } as Record<string, number>)[settleData.paymentMethod],
            notes: settleData.notes,
          }),
        }
      ),
  };

  // ================= 10. REPORTS API (14 ENDPOINTS) =================
  public reports = {
    getSales: (params?: { fromDate?: string; toDate?: string }) =>
      this.request<any>('/api/reports/sales'),

    getSalesReturn: () => this.request<any>('/api/reports/sales-return'),

    getPurchase: () => this.request<any>('/api/reports/purchase'),

    getPurchaseReturn: () => this.request<any>('/api/reports/purchase-return'),

    getLowStock: () => this.request<Product[]>('/api/reports/low-stock'),

    getCreditBillAll: () => this.request<CreditBill[]>('/api/reports/credit-bill'),

    getCreditBillByUser: (userId: string | number) =>
      this.request<CreditBill[]>(`/api/reports/credit-bill?userId=${userId}`),

    getMyCreditBill: () => this.request<CreditBill[]>('/api/reports/credit-bill/my'),

    getProductMiniStatement: (productId: string) =>
      this.request<ProductMiniStatement>(`/api/reports/product-mini-statement/${productId}`),

    getProductStock: () => this.request<any>('/api/reports/product-stock'),

    getExpiredStock: () => this.request<ExpiredStockItem[]>('/api/reports/expired-stock'),

    getProfitLoss: () => this.request<any>('/api/reports/profit-loss'),

    getTax: () => this.request<any>('/api/reports/tax'),

    exportPdf: (reportData: { reportType: string; title: string; filters?: any }) =>
      this.request<{ downloadUrl?: string; pdfBase64?: string; success: boolean }>(
        '/api/reports/export',
        {
          method: 'POST',
          body: JSON.stringify(reportData),
        }
      ),
  };

  // ================= 11. BUSINESS & SETTINGS API =================
  public business = {
    getInfo: () => this.request<BusinessProfile>('/api/business'),

    updateInfo: (id: string | number, updates: any) =>
      this.request<BusinessProfile>(`/api/business/${id}`, {
        method: 'PUT',
        body: JSON.stringify(updates),
      }),

    getBranches: () => this.request<Branch[]>('/api/branch'),

    getSettings: () => this.request<any>('/api/business-settings'),

    updateSettings: (settings: any) =>
      this.request<any>('/api/business-settings', {
        method: 'PUT',
        body: JSON.stringify(settings),
      }),

    getInvoiceSettings: () => this.request<any>('/api/invoice-settings'),

    updateInvoiceSettings: (settings: any) =>
      this.request<any>('/api/invoice-settings', {
        method: 'PUT',
        body: JSON.stringify(settings),
      }),
  };

  // ================= 12. DASHBOARD API =================
  public dashboard = {
    getSummary: () => this.request<any>('/api/dashboard/summary'),
    getChart: () => this.request<any>('/api/dashboard/chart'),
    getSales: () => this.request<any>('/api/dashboard/sales'),
  };

  // ================= 13. USERS & ROLES API =================
  public users = {
    list: () => this.request<UserStaff[]>('/api/user'),

    getById: (id: string | number) => this.request<UserStaff>(`/api/user/${id}`),

    create: (data: Partial<UserStaff>) =>
      this.request<UserStaff>('/api/user', {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    update: (id: string | number, data: Partial<UserStaff>) =>
      this.request<UserStaff>(`/api/user/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }),

    delete: (id: string | number) =>
      this.request<boolean>(`/api/user/${id}`, { method: 'DELETE' }),

    getRoles: () => this.request<Role[]>('/api/roles'),
  };

  // ================= 14. INVENTORY API =================
  public inventory = {
    getAll: () => this.request<any[]>('/api/inventory'),
    getStock: () => this.request<any>('/api/inventory/stock'),
    getLowStock: () => this.request<any[]>('/api/inventory/low-stock'),
    getStockHistory: (productId: string | number) =>
      this.request<any[]>(`/api/inventory/stock-history/${productId}`),
    adjustStock: (productId: string | number, data: { quantity: number; reason: string }) =>
      this.request<any>(`/api/inventory/${productId}/adjust`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }),
    getWarehouses: () => this.request<Warehouse[]>('/api/warehouses'),
  };

  // ================= 15. EXPENSES API =================
  public expenses = {
    list: () => this.requestAll<Expense>('/api/expenses'),

    getById: (id: string | number) => this.request<Expense>(`/api/expenses/${id}`),

    create: (expense: Partial<Expense>) =>
      this.request<Expense>('/api/expenses', {
        method: 'POST',
        body: JSON.stringify(expense),
      }),

    update: (id: string | number, expense: Partial<Expense>) =>
      this.request<Expense>(`/api/expenses/${id}`, {
        method: 'PUT',
        body: JSON.stringify(expense),
      }),

    delete: (id: string | number) =>
      this.request<boolean>(`/api/expenses/${id}`, { method: 'DELETE' }),

    getCategories: () => this.request<ExpenseCategory[]>('/api/expense-categories'),
  };

  // ================= 16. NOTIFICATIONS API =================
  public notifications = {
    list: () => this.request<NotificationItem[]>('/api/notifications'),

    markRead: (id: string | number) =>
      this.request<boolean>(`/api/notifications/${id}/read`, { method: 'PATCH' }),

    markAllRead: () =>
      this.request<boolean>('/api/notifications/read-all', { method: 'PATCH' }),
  };
}

export const api = new ApiService();
export default api;
