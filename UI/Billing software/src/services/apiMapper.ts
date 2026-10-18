/**
 * apiMapper.ts
 *
 * Normalizes raw API response objects (camelCase JSON from ASP.NET Core) into
 * the UI's internal types. This decouples the API schema from the store types,
 * so field renames on either side don't cause runtime crashes.
 *
 * All mappers are defensive — they use `|| fallback` so missing fields never
 * produce undefined where the UI expects a concrete value.
 */

import type { Product, Customer, Supplier, Invoice, Purchase, Expense, UserStaff, InvoiceItem, PurchaseItem } from '../types';
import type { SalesReturn, PurchaseReturn, CreditBill } from '../types/api';

// ─── Helper ───────────────────────────────────────────────────────────────────
const str = (v: any): string => (v != null ? String(v) : '');
const num = (v: any): number => (v != null ? Number(v) : 0);
const bool = (v: any): boolean => !!v;

export function mapApiStaff(raw: any): UserStaff {
  const apiRole = String(raw.role || 'Staff').toLowerCase();
  const role: UserStaff['role'] = apiRole === 'owner' || apiRole === 'superadmin' ? 'admin'
    : apiRole === 'manager' ? 'manager' : apiRole === 'cashier' ? 'cashier' : 'sales';
  const manager = role === 'admin' || role === 'manager';
  return {
    id: str(raw.id),
    name: str(raw.fullName || raw.name),
    email: str(raw.email),
    phone: str(raw.phone),
    role,
    status: raw.isActive === false ? 'inactive' : 'active',
    permissions: {
      createBill: true, editBill: manager, cancelBill: manager,
      viewProducts: true, manageProducts: manager,
      viewInventory: true, manageInventory: manager,
      viewReports: manager, viewProfit: role === 'admin',
      manageStaff: role === 'admin', manageSettings: role === 'admin',
    },
  };
}

// ─── Product ──────────────────────────────────────────────────────────────────
export function mapApiProduct(raw: any): Product {
  return {
    id:            str(raw.id),
    name:          str(raw.name),
    sku:           str(raw.sku || raw.sKU || raw.SKU || ''),
    barcode:       str(raw.barcode || raw.sku || ''),
    category:      str(raw.categoryName || raw.category || 'General'),
    brand:         str(raw.brandName || raw.brand || ''),
    description:   str(raw.description || ''),
    imageUrl:      raw.imageUrl || raw.ImageUrl || undefined,
    purchasePrice: num(raw.purchasePrice ?? raw.PurchasePrice),
    sellingPrice:  num(raw.sellingPrice ?? raw.SellingPrice),
    mrp:           num(raw.mRP || raw.mrp || raw.MRP || raw.sellingPrice),
    taxRate:       num(raw.taxPercentage || raw.TaxPercentage || raw.taxRate || 0),
    unit:          (raw.unitName || raw.unit || 'pcs') as any,
    stock:         num(raw.currentStock ?? raw.stock ?? 0),
    minStock:      num(raw.minStockLevel ?? raw.minStock ?? 5),
    supplierId:    str(raw.supplierId || ''),
    supplierName:  str(raw.supplierName || ''),
    batch:         raw.batch || undefined,
    expiryDate:    raw.expiryDate || raw.ExpiryDate || undefined,
    status:        bool(raw.isActive ?? true) ? 'active' : 'inactive',
    createdAt:     str(raw.createdAt || new Date().toISOString()),
  };
}

// ─── Customer ─────────────────────────────────────────────────────────────────
export function mapApiCustomer(raw: any): Customer {
  return {
    id:                 str(raw.id),
    name:               str(raw.name),
    phone:              str(raw.phone || ''),
    email:              raw.email || undefined,
    address:            raw.address || undefined,
    gstin:              raw.gSTNumber || raw.gstin || undefined,
    type:               (raw.customerGroupName?.toLowerCase() === 'vip' ? 'vip' : 'retail') as any,
    totalPurchases:     num(raw.totalPurchaseAmount || raw.totalPurchases || 0),
    outstandingBalance: num(raw.outstandingBalance || 0),
    lastPurchaseDate:   raw.lastPurchaseDate ? new Date(raw.lastPurchaseDate).toISOString().split('T')[0] : undefined,
    loyaltyPoints:      num(raw.loyaltyPoints || 0),
    createdAt:          str(raw.createdAt || new Date().toISOString()),
  };
}

// ─── Supplier ─────────────────────────────────────────────────────────────────
export function mapApiSupplier(raw: any): Supplier {
  return {
    id:                str(raw.id),
    name:              str(raw.name),
    companyName:       str(raw.companyName || raw.name),
    phone:             str(raw.phone || ''),
    email:             str(raw.email || ''),
    gstin:             str(raw.gSTNumber || raw.gstin || ''),
    address:           str(raw.address || ''),
    totalPurchases:    num(raw.totalPurchases || raw.outstandingBalance || 0),
    outstandingPayable:num(raw.outstandingBalance || 0),
    status:            bool(raw.isActive ?? true) ? 'active' : 'inactive',
  };
}

// ─── Invoice ──────────────────────────────────────────────────────────────────
export function mapApiInvoice(raw: any): Invoice {
  const items: InvoiceItem[] = (raw.items || []).map((item: any) => ({
    productId:   str(item.productId),
    productName: str(item.productName),
    sku:         str(item.hSNCode || item.sku || ''),
    unit:        str(item.unit || 'pcs'),
    quantity:    num(item.quantity),
    price:       num(item.unitPrice || item.price || 0),
    mrp:         num(item.unitPrice || item.price || 0),
    discount:    num(item.discountAmount || item.discount || 0),
    taxRate:     num(item.taxPercentage || item.taxRate || 0),
    taxAmount:   num(item.taxAmount || 0),
    total:       num(item.totalPrice || item.total || 0),
  }));

  const subTotal    = num(raw.subTotal || raw.subtotal || 0);
  const taxAmount   = num(raw.taxAmount || raw.taxTotal || 0);
  const discount    = num(raw.discountAmount || raw.itemDiscountTotal || 0);
  const grandTotal  = num(raw.totalAmount || raw.grandTotal || 0);
  const paidAmount  = num(raw.paidAmount ?? raw.amountReceived ?? 0);

  // Determine payment status from API status string
  const statusStr = (raw.status || raw.paymentStatus || '').toLowerCase();
  const paymentStatus =
    statusStr === 'paid'    ? 'paid' :
    statusStr === 'partial' ? 'partially_paid' :
    statusStr === 'cancelled' || statusStr === 'canceled' ? 'cancelled' :
    statusStr === 'refunded' ? 'refunded' :
    'pending';

  const paymentMethod = (raw.paymentMethodName || raw.paymentMethod || 'cash').toLowerCase() as any;

  return {
    id:                str(raw.id),
    invoiceNumber:     str(raw.invoiceNumber || raw.InvoiceNumber || ''),
    customerId:        str(raw.customerId || ''),
    customerName:      str(raw.customerName || 'Walk-in Customer'),
    customerPhone:     str(raw.customerPhone || ''),
    customerGstin:     raw.customerEmail || undefined, // API doesn't expose GSTIN separately
    items,
    subtotal:          subTotal,
    itemDiscountTotal: discount,
    extraDiscount:     0,
    cgst:              taxAmount / 2,
    sgst:              taxAmount / 2,
    igst:              0,
    taxTotal:          taxAmount,
    roundOff:          num(raw.roundOff || 0),
    grandTotal,
    amountReceived:    paidAmount,
    changeReturned:    Math.max(0, paidAmount - grandTotal),
    paymentMethod,
    paymentStatus:     paymentStatus as any,
    registerId:        str(raw.registerId || 'reg-01'),
    cashierName:       str(raw.cashierName || raw.createdByName || 'Staff'),
    createdAt:         raw.invoiceDate
      ? new Date(raw.invoiceDate).toISOString()
      : str(raw.createdAt || new Date().toISOString()),
    notes:             raw.notes || undefined,
  };
}

// ─── Purchase ─────────────────────────────────────────────────────────────────
export function mapApiPurchase(raw: any): Purchase {
  const items: PurchaseItem[] = (raw.items || []).map((item: any) => ({
    productId:    str(item.productId),
    productName:  str(item.productName),
    sku:          str(item.sku || item.hSNCode || ''),
    quantity:     num(item.quantity),
    purchasePrice:num(item.unitPrice || item.purchasePrice || 0),
    taxRate:      num(item.taxPercentage || item.taxRate || 0),
    total:        num(item.totalPrice || item.total || 0),
  }));

  return {
    id:             str(raw.id),
    purchaseNumber: str(raw.purchaseNumber || ''),
    supplierId:     str(raw.supplierId || ''),
    supplierName:   str(raw.supplierName || ''),
    items,
    subtotal:       num(raw.subTotal || raw.subtotal || 0),
    taxTotal:       num(raw.taxAmount || raw.taxTotal || 0),
    grandTotal:     num(raw.totalAmount || raw.grandTotal || 0),
    paymentStatus:  num(raw.dueAmount) <= 0 && num(raw.paidAmount) > 0 ? 'paid' : num(raw.paidAmount) > 0 ? 'partially_paid' : 'pending',
    status:         ((raw.status || 'received').toLowerCase()) as any,
    date:           raw.purchaseDate
      ? new Date(raw.purchaseDate).toISOString().split('T')[0]
      : str(raw.date || new Date().toISOString().split('T')[0]),
    invoiceRef:     raw.referenceNumber || undefined,
  };
}

export function mapApiExpense(raw: any): Expense {
  return {
    id: str(raw.id),
    expenseNumber: str(raw.expenseNumber || ''),
    title: str(raw.description || raw.paidTo || 'Expense'),
    category: (raw.categoryName?.toLowerCase() || 'other') as Expense['category'],
    amount: num(raw.amount),
    paymentMethod: (raw.paymentMethodName?.toLowerCase() || 'cash') as Expense['paymentMethod'],
    date: str(raw.expenseDate || new Date().toISOString()).split('T')[0],
    recordedBy: str(raw.paidTo || ''),
  };
}

// ─── Sales Return ─────────────────────────────────────────────────────────────
export function mapApiSalesReturn(raw: any): SalesReturn {
  return {
    id:                str(raw.id),
    returnNumber:      str(raw.returnNumber || ''),
    invoiceId:         str(raw.invoiceId || ''),
    invoiceNumber:     str(raw.invoiceNumber || ''),
    customerId:        str(raw.customerId || ''),
    customerName:      str(raw.customerName || ''),
    items:             (raw.items || []).map((item: any) => ({
      productId: str(item.productId), productName: str(item.productName || ''),
      quantity: num(item.quantity), unitPrice: num(item.unitPrice), refundAmount: num(item.totalPrice),
    })),
    totalRefundAmount: num(raw.totalAmount ?? raw.totalRefundAmount ?? 0),
    refundMethod:      (raw.refundMethod || 'cash') as any,
    returnReason:      str(raw.reason || raw.returnReason || ''),
    createdAt:         str(raw.returnDate || raw.createdAt || new Date().toISOString()),
    processedBy:       raw.processedBy || undefined,
  };
}

// ─── Purchase Return ──────────────────────────────────────────────────────────
export function mapApiPurchaseReturn(raw: any): PurchaseReturn {
  return {
    id:                str(raw.id),
    returnNumber:      str(raw.returnNumber || ''),
    purchaseId:        str(raw.purchaseId || ''),
    purchaseNumber:    str(raw.purchaseNumber || ''),
    supplierId:        str(raw.supplierId || ''),
    supplierName:      str(raw.supplierName || ''),
    items:             (raw.items || []).map((item: any) => ({
      productId: str(item.productId), productName: str(item.productName || ''),
      quantity: num(item.quantity), purchasePrice: num(item.unitPrice), refundAmount: num(item.totalPrice),
    })),
    totalRefundAmount: num(raw.totalAmount ?? raw.totalRefundAmount ?? 0),
    returnReason:      str(raw.reason || raw.returnReason || ''),
    date:              raw.returnDate || raw.date
      ? new Date(raw.returnDate || raw.date).toISOString().split('T')[0]
      : new Date().toISOString().split('T')[0],
    status:            (raw.status || 'completed') as any,
  };
}

// ─── Credit Bill ──────────────────────────────────────────────────────────────
export function mapApiCreditBill(raw: any): CreditBill {
  return {
    id:                str(raw.id),
    creditBillNumber:  str(raw.creditBillNumber || raw.invoiceNumber || `CR-${raw.id}`),
    invoiceId:         raw.invoiceId ? str(raw.invoiceId) : undefined,
    invoiceNumber:     raw.invoiceNumber || undefined,
    userId:            str(raw.userId || raw.staffId || '1'),
    userName:          str(raw.userName || raw.staffName || ''),
    customerId:        str(raw.customerId || ''),
    customerName:      str(raw.customerName || ''),
    customerPhone:     raw.customerPhone || undefined,
    amount:            num(raw.amount || raw.totalAmount || 0),
    settledAmount:     num(raw.settledAmount || raw.paidAmount || 0),
    balanceAmount:     num(raw.balanceAmount || raw.dueAmount || 0),
    dueDate:           raw.dueDate
      ? new Date(raw.dueDate).toISOString().split('T')[0]
      : new Date(Date.now() + 15 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    status:            (raw.status || 'pending') as any,
    notes:             raw.notes || undefined,
    createdAt:         str(raw.createdAt || new Date().toISOString()),
  };
}
