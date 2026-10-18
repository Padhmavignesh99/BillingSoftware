export type BusinessType = 'grocery' | 'supermarket' | 'clothing' | 'electronics' | 'hardware' | 'pharmacy' | 'general' | 'other';

export interface TaxSettings {
  defaultGstRate: number; // e.g. 18
  taxInclusive: boolean;
  enableCgstSgst: boolean;
  enableIgst: boolean;
}

export interface InvoiceSettings {
  prefix: string; // e.g., 'INV'
  nextNumber: number;
  termsAndConditions: string;
  footerMessage: string;
  showLogo: boolean;
}

export interface BusinessProfile {
  id: string;
  name: string;
  storeName: string;
  businessType: BusinessType;
  gstin: string;
  address: string;
  city: string;
  state: string;
  pincode: string;
  phone: string;
  email: string;
  logoUrl?: string;
  currency: string; // '₹'
  taxSettings: TaxSettings;
  invoiceSettings: InvoiceSettings;
}

export interface Register {
  id: string;
  name: string;
  code: string;
  status: 'online' | 'offline' | 'syncing';
  openingCash: number;
  currentCash: number;
  shiftStatus: 'open' | 'closed';
  currentCashier: string;
}

export type UserRole = 'admin' | 'manager' | 'cashier' | 'sales';

export interface UserPermissions {
  createBill: boolean;
  editBill: boolean;
  cancelBill: boolean;
  viewProducts: boolean;
  manageProducts: boolean;
  viewInventory: boolean;
  manageInventory: boolean;
  viewReports: boolean;
  viewProfit: boolean;
  manageStaff: boolean;
  manageSettings: boolean;
}

export interface UserStaff {
  id: string;
  name: string;
  phone: string;
  email: string;
  role: UserRole;
  registerId?: string;
  status: 'active' | 'inactive';
  permissions: UserPermissions;
  avatarUrl?: string;
}

export interface Category {
  id: string;
  name: string;
  code: string;
  productCount: number;
}

export interface Product {
  id: string;
  name: string;
  sku: string;
  barcode: string;
  category: string;
  brand: string;
  description: string;
  imageUrl?: string;
  purchasePrice: number;
  sellingPrice: number;
  mrp: number;
  taxRate: number; // e.g. 5, 12, 18, 28
  unit: 'pcs' | 'kg' | 'pack' | 'box' | 'liter' | 'm';
  stock: number;
  minStock: number;
  supplierId?: string;
  supplierName?: string;
  batch?: string;
  expiryDate?: string;
  status: 'active' | 'inactive' | 'low_stock' | 'out_of_stock';
  createdAt: string;
}

export interface Customer {
  id: string;
  name: string;
  phone: string;
  email?: string;
  address?: string;
  gstin?: string;
  type: 'walkin' | 'retail' | 'wholesaler' | 'vip';
  totalPurchases: number;
  outstandingBalance: number; // Khata balance
  lastPurchaseDate?: string;
  loyaltyPoints: number;
  createdAt: string;
}

export interface Supplier {
  id: string;
  name: string;
  companyName: string;
  phone: string;
  email: string;
  gstin: string;
  address: string;
  totalPurchases: number;
  outstandingPayable: number;
  status: 'active' | 'inactive';
}

export interface CartItem {
  product: Product;
  quantity: number;
  discountPercentage: number;
  customPrice?: number;
  taxAmount: number;
  lineTotal: number;
}

export interface HeldBill {
  id: string;
  billNumber: string;
  customer: Customer;
  items: CartItem[];
  subtotal: number;
  discount: number;
  taxTotal: number;
  grandTotal: number;
  heldAt: string;
  note?: string;
}

export type PaymentMethod = 'cash' | 'upi' | 'card' | 'credit' | 'split';
export type PaymentStatus = 'paid' | 'pending' | 'partially_paid' | 'cancelled' | 'refunded';

export interface SplitPaymentDetail {
  cash: number;
  upi: number;
  card: number;
  credit: number;
}

export interface InvoiceItem {
  productId: string;
  productName: string;
  sku: string;
  unit: string;
  quantity: number;
  price: number;
  mrp: number;
  discount: number;
  taxRate: number;
  taxAmount: number;
  total: number;
}

export interface Invoice {
  id: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  customerPhone: string;
  customerGstin?: string;
  items: InvoiceItem[];
  subtotal: number;
  itemDiscountTotal: number;
  extraDiscount: number;
  cgst: number;
  sgst: number;
  igst: number;
  taxTotal: number;
  roundOff: number;
  grandTotal: number;
  amountReceived: number;
  changeReturned: number;
  paymentMethod: PaymentMethod;
  splitDetails?: SplitPaymentDetail;
  paymentStatus: PaymentStatus;
  registerId: string;
  cashierName: string;
  createdAt: string;
  notes?: string;
}

export interface PurchaseItem {
  productId: string;
  productName: string;
  sku: string;
  quantity: number;
  purchasePrice: number;
  taxRate: number;
  total: number;
}

export interface Purchase {
  id: string;
  purchaseNumber: string;
  supplierId: string;
  supplierName: string;
  items: PurchaseItem[];
  subtotal: number;
  taxTotal: number;
  grandTotal: number;
  paymentStatus: PaymentStatus;
  status: 'received' | 'pending' | 'draft';
  date: string;
  invoiceRef?: string;
}

export interface Expense {
  id: string;
  expenseNumber: string;
  title: string;
  category: 'rent' | 'electricity' | 'salary' | 'transport' | 'maintenance' | 'marketing' | 'other';
  amount: number;
  paymentMethod: 'cash' | 'upi' | 'card' | 'bank_transfer';
  date: string;
  notes?: string;
  recordedBy: string;
}

export interface InventoryMovement {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  movementType: 'in' | 'out' | 'adjustment' | 'return' | 'sale' | 'purchase';
  quantity: number;
  previousStock: number;
  newStock: number;
  referenceNumber: string; // e.g. INV-1046 or PUR-201
  reason: string;
  date: string;
  performedBy: string;
}

export interface NotificationItem {
  id: string;
  title: string;
  message: string;
  type: 'low_stock' | 'out_of_stock' | 'payment_received' | 'payment_overdue' | 'new_customer' | 'invoice_sent' | 'purchase_received' | 'system';
  isRead: boolean;
  createdAt: string;
  link?: string;
}

export interface AIInsight {
  id: string;
  category: 'sales' | 'inventory' | 'customer' | 'financial';
  title: string;
  summary: string;
  impact: 'high' | 'medium' | 'low';
  trend: 'up' | 'down' | 'neutral';
  recommendation: string;
  date: string;
}

export interface DashboardMetrics {
  todaySales: number;
  todaySalesGrowth: number;
  todayOrders: number;
  todayOrdersGrowth: number;
  pendingPayments: number;
  pendingPaymentsCount: number;
  lowStockItemsCount: number;
}

export * from './saas';
export * from './api';
