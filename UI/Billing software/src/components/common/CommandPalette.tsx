import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, ShoppingBag, Receipt, Users, Truck, ArrowRight, CornerDownLeft } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Modal } from '../ui/Modal';
import { Badge } from '../ui/Badge';

interface CommandPaletteProps {
  isOpen: boolean;
  onClose: () => void;
}

export const CommandPalette: React.FC<CommandPaletteProps> = ({ isOpen, onClose }) => {
  const [query, setQuery] = useState('');
  const navigate = useNavigate();

  const { products, invoices, customers, suppliers } = useStore();

  useEffect(() => {
    if (isOpen) {
      setQuery('');
    }
  }, [isOpen]);

  if (!isOpen) return null;

  const lowerQuery = query.toLowerCase().trim();

  const matchedProducts = lowerQuery
    ? products.filter(
        (p) =>
          p.name.toLowerCase().includes(lowerQuery) ||
          p.sku.toLowerCase().includes(lowerQuery) ||
          p.barcode.toLowerCase().includes(lowerQuery)
      ).slice(0, 4)
    : [];

  const matchedInvoices = lowerQuery
    ? invoices.filter(
        (inv) =>
          inv.invoiceNumber.toLowerCase().includes(lowerQuery) ||
          inv.customerName.toLowerCase().includes(lowerQuery)
      ).slice(0, 4)
    : [];

  const matchedCustomers = lowerQuery
    ? customers.filter(
        (c) =>
          c.name.toLowerCase().includes(lowerQuery) ||
          c.phone.includes(lowerQuery) ||
          (c.email && c.email.toLowerCase().includes(lowerQuery))
      ).slice(0, 4)
    : [];

  const matchedSuppliers = lowerQuery
    ? suppliers.filter(
        (s) =>
          s.name.toLowerCase().includes(lowerQuery) ||
          s.companyName.toLowerCase().includes(lowerQuery)
      ).slice(0, 3)
    : [];

  const hasResults =
    matchedProducts.length > 0 ||
    matchedInvoices.length > 0 ||
    matchedCustomers.length > 0 ||
    matchedSuppliers.length > 0;

  const handleSelect = (path: string) => {
    navigate(path);
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Global Search & Commands" maxWidth="2xl">
      <div className="space-y-4">
        {/* Search Bar */}
        <div className="relative">
          <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400" />
          <input
            type="text"
            autoFocus
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search products, SKU, barcodes, invoices, customers, suppliers..."
            className="w-full pl-11 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm font-medium text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-600/30 focus:border-indigo-600 transition-all placeholder:text-slate-400"
          />
          <div className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center gap-1 text-[11px] font-semibold text-slate-400 bg-slate-200/60 px-1.5 py-0.5 rounded">
            ESC to close
          </div>
        </div>

        {/* Quick Nav Shortcut Buttons when empty query */}
        {!query && (
          <div className="space-y-3 pt-2">
            <p className="text-xs font-semibold text-slate-400 tracking-wider uppercase">Quick Shortcuts</p>
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
              <button
                onClick={() => handleSelect('/billing')}
                className="flex flex-col items-center justify-center p-3 rounded-lg border border-slate-200 hover:border-indigo-300 hover:bg-indigo-50/50 transition-all group text-left"
              >
                <div className="w-8 h-8 rounded-full bg-indigo-100 text-indigo-600 flex items-center justify-center mb-2 group-hover:scale-110 transition-transform">
                  <ShoppingBag className="w-4 h-4" />
                </div>
                <span className="text-xs font-semibold text-slate-800">New Bill POS</span>
                <span className="text-[10px] text-slate-400">Press F2</span>
              </button>

              <button
                onClick={() => handleSelect('/invoices')}
                className="flex flex-col items-center justify-center p-3 rounded-lg border border-slate-200 hover:border-indigo-300 hover:bg-indigo-50/50 transition-all group text-left"
              >
                <div className="w-8 h-8 rounded-full bg-sky-100 text-sky-600 flex items-center justify-center mb-2 group-hover:scale-110 transition-transform">
                  <Receipt className="w-4 h-4" />
                </div>
                <span className="text-xs font-semibold text-slate-800">All Invoices</span>
                <span className="text-[10px] text-slate-400">View history</span>
              </button>

              <button
                onClick={() => handleSelect('/products')}
                className="flex flex-col items-center justify-center p-3 rounded-lg border border-slate-200 hover:border-indigo-300 hover:bg-indigo-50/50 transition-all group text-left"
              >
                <div className="w-8 h-8 rounded-full bg-emerald-100 text-emerald-600 flex items-center justify-center mb-2 group-hover:scale-110 transition-transform">
                  <ShoppingBag className="w-4 h-4" />
                </div>
                <span className="text-xs font-semibold text-slate-800">Products</span>
                <span className="text-[10px] text-slate-400">Catalog & stock</span>
              </button>

              <button
                onClick={() => handleSelect('/customers')}
                className="flex flex-col items-center justify-center p-3 rounded-lg border border-slate-200 hover:border-indigo-300 hover:bg-indigo-50/50 transition-all group text-left"
              >
                <div className="w-8 h-8 rounded-full bg-purple-100 text-purple-600 flex items-center justify-center mb-2 group-hover:scale-110 transition-transform">
                  <Users className="w-4 h-4" />
                </div>
                <span className="text-xs font-semibold text-slate-800">Customers</span>
                <span className="text-[10px] text-slate-400">Khata & ledger</span>
              </button>
            </div>
          </div>
        )}

        {/* Results List */}
        {query && (
          <div className="space-y-4 max-h-80 overflow-y-auto pr-1">
            {!hasResults && (
              <div className="py-8 text-center text-slate-400 text-sm">
                No matching products, invoices, or customers found for "<span className="font-semibold text-slate-600">{query}</span>"
              </div>
            )}

            {/* Products */}
            {matchedProducts.length > 0 && (
              <div>
                <p className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider mb-2">Products</p>
                <div className="space-y-1">
                  {matchedProducts.map((prod) => (
                    <button
                      key={prod.id}
                      onClick={() => handleSelect(`/products`)}
                      className="w-full flex items-center justify-between p-2.5 rounded-lg hover:bg-slate-100 transition-colors text-left group"
                    >
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-lg bg-slate-100 border border-slate-200 flex items-center justify-center shrink-0">
                          <ShoppingBag className="w-4 h-4 text-slate-500" />
                        </div>
                        <div>
                          <p className="text-xs font-semibold text-slate-900 group-hover:text-indigo-600 transition-colors">
                            {prod.name}
                          </p>
                          <p className="text-[11px] text-slate-500 font-mono">
                            SKU: {prod.sku} | Barcode: {prod.barcode}
                          </p>
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-semibold text-slate-900">₹{prod.sellingPrice}</span>
                        <Badge variant={prod.stock > 0 ? 'success' : 'danger'} size="sm">
                          {prod.stock} in stock
                        </Badge>
                      </div>
                    </button>
                  ))}
                </div>
              </div>
            )}

            {/* Invoices */}
            {matchedInvoices.length > 0 && (
              <div>
                <p className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider mb-2">Invoices</p>
                <div className="space-y-1">
                  {matchedInvoices.map((inv) => (
                    <button
                      key={inv.id}
                      onClick={() => handleSelect(`/invoices`)}
                      className="w-full flex items-center justify-between p-2.5 rounded-lg hover:bg-slate-100 transition-colors text-left group"
                    >
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-lg bg-indigo-50 border border-indigo-100 flex items-center justify-center shrink-0">
                          <Receipt className="w-4 h-4 text-indigo-600" />
                        </div>
                        <div>
                          <p className="text-xs font-semibold text-slate-900 group-hover:text-indigo-600 transition-colors font-mono">
                            {inv.invoiceNumber}
                          </p>
                          <p className="text-[11px] text-slate-500">
                            {inv.customerName} ({inv.customerPhone})
                          </p>
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-semibold text-slate-900">₹{inv.grandTotal}</span>
                        <Badge variant={inv.paymentStatus === 'paid' ? 'success' : 'warning'} size="sm">
                          {inv.paymentStatus}
                        </Badge>
                      </div>
                    </button>
                  ))}
                </div>
              </div>
            )}

            {/* Customers */}
            {matchedCustomers.length > 0 && (
              <div>
                <p className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider mb-2">Customers</p>
                <div className="space-y-1">
                  {matchedCustomers.map((cust) => (
                    <button
                      key={cust.id}
                      onClick={() => handleSelect(`/customers`)}
                      className="w-full flex items-center justify-between p-2.5 rounded-lg hover:bg-slate-100 transition-colors text-left group"
                    >
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-lg bg-purple-50 border border-purple-100 flex items-center justify-center shrink-0">
                          <Users className="w-4 h-4 text-purple-600" />
                        </div>
                        <div>
                          <p className="text-xs font-semibold text-slate-900 group-hover:text-indigo-600 transition-colors">
                            {cust.name}
                          </p>
                          <p className="text-[11px] text-slate-500 font-mono">{cust.phone}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        {cust.outstandingBalance > 0 && (
                          <Badge variant="warning" size="sm">
                            Khata Due: ₹{cust.outstandingBalance}
                          </Badge>
                        )}
                        <Badge variant="indigo" size="sm">
                          {cust.type}
                        </Badge>
                      </div>
                    </button>
                  ))}
                </div>
              </div>
            )}

            {/* Suppliers */}
            {matchedSuppliers.length > 0 && (
              <div>
                <p className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider mb-2">Suppliers</p>
                <div className="space-y-1">
                  {matchedSuppliers.map((sup) => (
                    <button
                      key={sup.id}
                      onClick={() => handleSelect(`/suppliers`)}
                      className="w-full flex items-center justify-between p-2.5 rounded-lg hover:bg-slate-100 transition-colors text-left group"
                    >
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-lg bg-amber-50 border border-amber-100 flex items-center justify-center shrink-0">
                          <Truck className="w-4 h-4 text-amber-600" />
                        </div>
                        <div>
                          <p className="text-xs font-semibold text-slate-900 group-hover:text-indigo-600 transition-colors">
                            {sup.companyName} ({sup.name})
                          </p>
                          <p className="text-[11px] text-slate-500 font-mono">{sup.phone}</p>
                        </div>
                      </div>
                      <Badge variant="neutral" size="sm">
                        Payable: ₹{sup.outstandingPayable}
                      </Badge>
                    </button>
                  ))}
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </Modal>
  );
};
