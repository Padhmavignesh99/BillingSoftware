export interface AuthUser {
  id: string;
  name: string;
  fullName?: string;
  email: string;
  role: string;
  phone?: string;
  businessName?: string;
  branchId?: string;
  tenantId?: number;
  avatarUrl?: string;
  profileImageUrl?: string;
  isActive?: boolean;
}

export interface LoginResponse {
  token: string;
  Token?: string;                 // Backend may PascalCase
  refreshToken?: string;
  RefreshToken?: string;
  expiration?: string;
  Expiration?: string;
  user: AuthUser;
  User?: AuthUser;
  message?: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  businessName: string;
  phone?: string;
}

export interface ProductCategory {
  id: string;
  name: string;
  code?: string;
  description?: string;
  productCount?: number;
}

export interface Brand {
  id: string;
  name: string;
  code?: string;
}

export interface Unit {
  id: string;
  name: string;
  symbol: string;
}

export interface TaxRate {
  id: string;
  name: string;
  rate: number; // e.g. 0, 5, 12, 18, 28
  isDefault?: boolean;
}

export interface DiscountScheme {
  id: string;
  name: string;
  code: string;
  discountType: 'percentage' | 'flat';
  value: number;
  minBillAmount?: number;
  active: boolean;
}

export interface SalesReturnItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  refundAmount: number;
  taxRate?: number;
  reason?: string;
}

export interface SalesReturn {
  id: string;
  returnNumber: string;
  invoiceId: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  items: SalesReturnItem[];
  totalRefundAmount: number;
  refundMethod: 'cash' | 'upi' | 'credit_note' | 'khata_adjustment';
  returnReason: string;
  createdAt: string;
  processedBy?: string;
}

export interface PurchaseReturnItem {
  productId: string;
  productName: string;
  quantity: number;
  purchasePrice: number;
  refundAmount: number;
  reason?: string;
}

export interface PurchaseReturn {
  id: string;
  returnNumber: string;
  purchaseId: string;
  purchaseNumber: string;
  supplierId: string;
  supplierName: string;
  items: PurchaseReturnItem[];
  totalRefundAmount: number;
  returnReason: string;
  date: string;
  status: 'pending' | 'approved' | 'completed';
}

export interface CreditBill {
  id: string;
  creditBillNumber: string;
  invoiceId?: string;
  invoiceNumber?: string;
  userId: string | number;
  userName?: string;
  customerId: string;
  customerName: string;
  customerPhone?: string;
  amount: number;
  settledAmount: number;
  balanceAmount: number;
  dueDate: string;
  status: 'pending' | 'partially_settled' | 'settled';
  notes?: string;
  createdAt: string;
}

export interface CreditBalanceSummary {
  userId?: string | number;
  totalCreditIssued: number;
  totalSettled: number;
  outstandingBalance: number;
  pendingBillsCount: number;
}

export interface SettleCreditBillDTO {
  creditBillId: string;
  customerId?: string;
  settleAmount: number;
  paymentMethod: 'cash' | 'upi' | 'card' | 'bank_transfer';
  notes?: string;
  receiptNumber?: string;
}

export interface ProductMiniStatementEntry {
  id: string;
  date: string;
  transactionType: 'sale' | 'purchase' | 'sales_return' | 'purchase_return' | 'adjustment' | 'opening';
  referenceNumber: string;
  partyName?: string;
  quantityIn: number;
  quantityOut: number;
  unitPrice: number;
  balanceStock: number;
  notes?: string;
}

export interface ProductMiniStatement {
  productId: string;
  productName: string;
  sku: string;
  currentStock: number;
  sellingPrice: number;
  purchasePrice: number;
  unit: string;
  entries: ProductMiniStatementEntry[];
}

export interface ExpiredStockItem {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  category: string;
  batchNumber?: string;
  expiryDate: string;
  stockQuantity: number;
  purchasePrice: number;
  totalLossValuation: number;
  daysExpired: number; // Positive if expired, negative if days remaining till expiry
  status: 'expired' | 'near_expiry' | 'safe';
}

export interface Warehouse {
  id: string;
  name: string;
  code: string;
  location: string;
  capacity?: string;
  managerName?: string;
  phone?: string;
  isPrimary?: boolean;
}

export interface Branch {
  id: string;
  name: string;
  code: string;
  address: string;
  city: string;
  state: string;
  phone: string;
  gstin?: string;
  isMainBranch: boolean;
}

export interface ExpenseCategory {
  id: string;
  name: string;
  code: string;
  description?: string;
}

export interface Role {
  id: string;
  name: string;
  description: string;
  permissions: string[];
}
