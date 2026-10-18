import React, { useState, useEffect, useRef } from 'react';
import {
  Search,
  Barcode,
  Plus,
  Trash2,
  PauseCircle,
  CreditCard,
  QrCode,
  Banknote,
  Clock,
  Send,
  Printer,
  Download,
  CheckCircle2,
  Users,
  Percent,
  X,
  Sparkles,
  Smartphone,
  Mail,
  FileText,
  AlertTriangle,
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import type { Product, Customer, PaymentMethod, Invoice, SplitPaymentDetail } from '../../types';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/common/Toast';
import { whatsappService } from '../../services/whatsappService';
import { emailService } from '../../services/emailService';
import { pdfService } from '../../services/pdfService';

export const BillingPage: React.FC = () => {
  const { showToast } = useToast();

  const {
    products,
    categories,
    customers,
    selectedCustomer,
    setSelectedCustomer,
    cart,
    addToCart,
    removeFromCart,
    updateCartQuantity,
    updateCartDiscount,
    clearCart,
    posExtraDiscount,
    setPosExtraDiscount,
    holdCurrentBill,
    heldBills,
    resumeHeldBill,
    deleteHeldBill,
    completeSale,
    addCustomer,
  } = useStore();

  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('All');
  const [showAddCustomerModal, setShowAddCustomerModal] = useState(false);
  const [showHeldBillsModal, setShowHeldBillsModal] = useState(false);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  const [showWhatsAppModal, setShowWhatsAppModal] = useState(false);
  const [showEmailModal, setShowEmailModal] = useState(false);

  // Payment Modal States
  const [selectedPaymentMethod, setSelectedPaymentMethod] = useState<PaymentMethod>('cash');
  const [amountReceived, setAmountReceived] = useState<number>(0);
  const [upiRefId, setUpiRefId] = useState('UPI/' + Math.floor(100000000000 + Math.random() * 900000000000));
  const [splitDetails, setSplitDetails] = useState<SplitPaymentDetail>({ cash: 0, upi: 0, card: 0, credit: 0 });
  const [paymentNotes, setPaymentNotes] = useState('');

  // Generated Invoice Result State
  const [lastInvoice, setLastInvoice] = useState<Invoice | null>(null);

  // Sending status
  const [isSendingWhatsApp, setIsSendingWhatsApp] = useState(false);
  const [whatsAppSuccess, setWhatsAppSuccess] = useState(false);
  const [waPhone, setWaPhone] = useState('');

  const [isSendingEmail, setIsSendingEmail] = useState(false);
  const [emailTo, setEmailTo] = useState('');

  // New Customer Form State
  const [newCustName, setNewCustName] = useState('');
  const [newCustPhone, setNewCustPhone] = useState('');
  const [newCustGstin, setNewCustGstin] = useState('');

  const searchInputRef = useRef<HTMLInputElement>(null);

  // Filter Products
  const filteredProducts = products.filter((prod) => {
    const matchesCat = selectedCategory === 'All' || prod.category === selectedCategory;
    const q = searchQuery.toLowerCase().trim();
    const matchesSearch =
      !q ||
      prod.name.toLowerCase().includes(q) ||
      prod.sku.toLowerCase().includes(q) ||
      prod.barcode.toLowerCase().includes(q);
    return matchesCat && matchesSearch;
  });

  // Calculations
  const subtotal = cart.reduce((sum, item) => sum + item.product.sellingPrice * item.quantity, 0);
  const itemDiscountTotal = cart.reduce(
    (sum, item) => sum + item.product.sellingPrice * item.quantity * (item.discountPercentage / 100),
    0
  );
  const taxTotal = cart.reduce((sum, item) => sum + item.taxAmount, 0);
  const rawGrandTotal = Math.max(0, subtotal - itemDiscountTotal - posExtraDiscount);
  const grandTotal = Math.round(rawGrandTotal);
  const roundOff = (grandTotal - rawGrandTotal).toFixed(2);

  // Update amount received default when grand total changes
  useEffect(() => {
    setAmountReceived(grandTotal);
  }, [grandTotal]);

  // Keyboard Shortcuts Listener inside Billing Page (F4 Customer, F6 Pay, F8 Hold, Esc)
  useEffect(() => {
    const handlePOSShortcuts = (e: KeyboardEvent) => {
      if (e.key === 'F4') {
        e.preventDefault();
        setShowAddCustomerModal(true);
      } else if (e.key === 'F6' && cart.length > 0) {
        e.preventDefault();
        setShowPaymentModal(true);
      } else if (e.key === 'F8' && cart.length > 0) {
        e.preventDefault();
        handleHoldBill();
      } else if (e.key === 'Escape') {
        if (showPaymentModal) setShowPaymentModal(false);
        else if (showHeldBillsModal) setShowHeldBillsModal(false);
      }
    };
    window.addEventListener('keydown', handlePOSShortcuts);
    return () => window.removeEventListener('keydown', handlePOSShortcuts);
  }, [cart, showPaymentModal, showHeldBillsModal]);

  const handleHoldBill = () => {
    const held = holdCurrentBill('Hold from cashier POS');
    if (held) {
      showToast(`Bill held as ${held.billNumber}`, 'info', 'Bill Saved');
    }
  };

  const handleExecutePayment = async () => {
    try {
      const invoice = await completeSale({
        paymentMethod: selectedPaymentMethod,
        splitDetails: selectedPaymentMethod === 'split' ? splitDetails : undefined,
        amountReceived,
        notes: paymentNotes,
      });

      setLastInvoice(invoice);
      setShowPaymentModal(false);
      setShowSuccessModal(true);
      showToast(`Invoice ${invoice.invoiceNumber} generated!`, 'success', 'Sale Completed');
    } catch (err: any) {
      showToast(err.message || 'Payment failed', 'error');
    }
  };

  const handleCreateCustomer = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCustName || !newCustPhone) return;
    try {
    const created = await addCustomer({
      name: newCustName,
      phone: newCustPhone,
      gstin: newCustGstin || undefined,
      type: 'retail',
    });
    setSelectedCustomer(created);
    setShowAddCustomerModal(false);
    setNewCustName('');
    setNewCustPhone('');
    setNewCustGstin('');
    showToast(`Customer ${created.name} added`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not add customer', 'error');
    }
  };

  const handleSendWhatsApp = async () => {
    if (!lastInvoice) return;
    setIsSendingWhatsApp(true);
    try {
      await whatsappService.sendInvoice({
        customerName: lastInvoice.customerName,
        phone: waPhone || lastInvoice.customerPhone,
        invoiceNumber: lastInvoice.invoiceNumber,
        grandTotal: lastInvoice.grandTotal,
      });
      setIsSendingWhatsApp(false);
      setWhatsAppSuccess(true);
      showToast(`Invoice sent via WhatsApp to ${waPhone || lastInvoice.customerPhone}`, 'success');
    } catch (err: any) {
      setIsSendingWhatsApp(false);
      showToast(err.message || 'WhatsApp sending failed', 'error');
    }
  };

  const handleSendEmail = async () => {
    if (!lastInvoice) return;
    setIsSendingEmail(true);
    try {
      await emailService.sendInvoiceEmail({
        toEmail: emailTo,
        customerName: lastInvoice.customerName,
        invoiceNumber: lastInvoice.invoiceNumber,
        grandTotal: lastInvoice.grandTotal,
      });
      setIsSendingEmail(false);
      setShowEmailModal(false);
      showToast(`Invoice emailed to ${emailTo}`, 'success');
    } catch (err: any) {
      setIsSendingEmail(false);
      showToast(err.message || 'Email failed', 'error');
    }
  };

  return (
    <div className="h-[calc(100vh-5rem)] flex flex-col gap-4 -m-4 sm:-m-6 lg:-m-8 p-4 sm:p-6 bg-slate-100/70 overflow-hidden">
      {/* Top POS Header Bar */}
      <div className="bg-white p-3.5 rounded-2xl border border-slate-200/80 shadow-2xs flex flex-wrap items-center justify-between gap-3 shrink-0">
        {/* Customer Selector */}
        <div className="flex items-center gap-2 flex-1 min-w-[260px]">
          <Users className="w-4 h-4 text-indigo-600 shrink-0" />
          <div className="flex-1 min-w-0">
            <select
              value={selectedCustomer.id}
              onChange={(e) => {
                const found = customers.find((c) => c.id === e.target.value);
                if (found) setSelectedCustomer(found);
              }}
              className="w-full bg-slate-50 border border-slate-200 rounded-lg px-2.5 py-1.5 text-xs font-semibold text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-600/30"
            >
              {customers.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name} {c.phone !== 'N/A' ? `(${c.phone})` : ''} {c.outstandingBalance > 0 ? `• Khata Due ₹${c.outstandingBalance}` : ''}
                </option>
              ))}
            </select>
          </div>

          <button
            onClick={() => setShowAddCustomerModal(true)}
            className="p-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-xs font-medium flex items-center gap-1 shrink-0"
            title="Add New Customer (F4)"
          >
            <Plus className="w-3.5 h-3.5" />
            <span className="hidden sm:inline">Add</span>
          </button>

          {selectedCustomer.outstandingBalance > 0 && (
            <Badge variant="warning" size="sm" className="hidden md:inline-flex shrink-0">
              Khata Due: ₹{selectedCustomer.outstandingBalance.toLocaleString()}
            </Badge>
          )}
        </div>

        {/* Action Controls: Hold Bills & Quick Clear */}
        <div className="flex items-center gap-2">
          <button
            onClick={() => setShowHeldBillsModal(true)}
            className="relative flex items-center gap-1.5 px-3 py-1.5 bg-amber-50 hover:bg-amber-100 text-amber-700 border border-amber-200 rounded-lg text-xs font-semibold transition-colors"
          >
            <PauseCircle className="w-4 h-4" />
            <span>Held Bills</span>
            {heldBills.length > 0 && (
              <span className="w-5 h-5 rounded-full bg-amber-600 text-white text-[10px] font-bold flex items-center justify-center">
                {heldBills.length}
              </span>
            )}
          </button>

          {cart.length > 0 && (
            <button
              onClick={clearCart}
              className="px-2.5 py-1.5 text-xs text-rose-600 hover:bg-rose-50 rounded-lg font-medium transition-colors"
            >
              Clear Cart
            </button>
          )}
        </div>
      </div>

      {/* Main Billing Layout Grid (Products Left, Cart Right) */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 flex-1 min-h-0">
        {/* LEFT PRODUCT CATALOG SECTION (7 Cols) */}
        <div className="lg:col-span-7 bg-white rounded-2xl border border-slate-200/80 shadow-2xs p-4 flex flex-col min-h-0">
          {/* Product Search Input */}
          <div className="relative mb-3 shrink-0">
            <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
            <input
              ref={searchInputRef}
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Scan barcode or search by product name, SKU..."
              className="w-full pl-10 pr-10 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-600/30"
            />
            <Barcode className="w-5 h-5 text-indigo-600 absolute right-3 top-1/2 -translate-y-1/2" />
          </div>

          {/* Category Tabs */}
          <div className="flex items-center gap-1.5 overflow-x-auto pb-2 mb-3 shrink-0 scrollbar-none">
            <button
              onClick={() => setSelectedCategory('All')}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold whitespace-nowrap transition-colors ${
                selectedCategory === 'All'
                  ? 'bg-indigo-600 text-white shadow-xs'
                  : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
              }`}
            >
              All Items ({products.length})
            </button>
            {categories.map((cat) => (
              <button
                key={cat.id}
                onClick={() => setSelectedCategory(cat.name)}
                className={`px-3 py-1.5 rounded-lg text-xs font-semibold whitespace-nowrap transition-colors ${
                  selectedCategory === cat.name
                    ? 'bg-indigo-600 text-white shadow-xs'
                    : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                }`}
              >
                {cat.name}
              </button>
            ))}
          </div>

          {/* Product Cards Grid */}
          <div className="grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-4 gap-3 overflow-y-auto flex-1 pr-1">
            {filteredProducts.map((prod) => (
              <button
                key={prod.id}
                onClick={() => {
                  if (prod.stock > 0) addToCart(prod);
                  else showToast(`${prod.name} is out of stock`, 'warning');
                }}
                className={`p-3 rounded-xl border text-left flex flex-col justify-between transition-all group relative overflow-hidden ${
                  prod.stock === 0
                    ? 'bg-slate-50/60 border-slate-200 opacity-60'
                    : 'bg-white hover:bg-indigo-50/40 border-slate-200/90 hover:border-indigo-300 shadow-2xs hover:shadow-md'
                }`}
              >
                <div>
                  <div className="flex items-center justify-between gap-1 mb-1.5">
                    <span className="text-[10px] font-mono font-semibold text-slate-400 truncate">{prod.sku}</span>
                    <Badge variant={prod.stock > prod.minStock ? 'success' : prod.stock > 0 ? 'warning' : 'danger'} size="sm">
                      {prod.stock > 0 ? `${prod.stock} left` : 'Out'}
                    </Badge>
                  </div>
                  <h4 className="text-xs font-bold text-slate-900 group-hover:text-indigo-600 line-clamp-2 leading-snug mb-1">
                    {prod.name}
                  </h4>
                </div>

                <div className="pt-2 border-t border-slate-100 flex items-center justify-between mt-2">
                  <div>
                    <span className="text-xs font-extrabold text-slate-900">₹{prod.sellingPrice}</span>
                    {prod.mrp > prod.sellingPrice && (
                      <span className="text-[10px] text-slate-400 line-through ml-1">₹{prod.mrp}</span>
                    )}
                  </div>
                  <div className="w-6 h-6 rounded-lg bg-indigo-600 text-white flex items-center justify-center group-hover:scale-110 transition-transform">
                    <Plus className="w-3.5 h-3.5" />
                  </div>
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* RIGHT POS CART & CALCULATION SECTION (5 Cols) */}
        <div className="lg:col-span-5 bg-white rounded-2xl border border-slate-200/80 shadow-2xs p-4 flex flex-col min-h-0">
          <div className="flex items-center justify-between pb-3 border-b border-slate-100 shrink-0">
            <h3 className="text-sm font-bold text-slate-900">Current Cart Items ({cart.length})</h3>
            <span className="text-xs font-mono text-slate-400">Customer: {selectedCustomer.name}</span>
          </div>

          {/* Cart Items List */}
          <div className="flex-1 overflow-y-auto py-2 space-y-2 pr-1 min-h-[180px]">
            {cart.length === 0 ? (
              <div className="h-full flex flex-col items-center justify-center text-center p-6 text-slate-400">
                <Barcode className="w-12 h-12 stroke-1 text-slate-300 mb-2" />
                <p className="text-xs font-medium">Cart is empty</p>
                <p className="text-[11px] text-slate-400">Click products or scan barcode to add</p>
              </div>
            ) : (
              cart.map((item) => (
                <div key={item.product.id} className="p-2.5 bg-slate-50 rounded-xl border border-slate-100 flex items-center gap-3">
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-semibold text-slate-900 truncate">{item.product.name}</p>
                    <p className="text-[10px] text-slate-500 font-mono">
                      ₹{item.product.sellingPrice} x {item.quantity} | GST {item.product.taxRate}%
                    </p>
                  </div>

                  {/* Quantity controls */}
                  <div className="flex items-center gap-1 bg-white border border-slate-200 rounded-lg p-0.5">
                    <button
                      onClick={() => updateCartQuantity(item.product.id, item.quantity - 1)}
                      className="w-5 h-5 flex items-center justify-center text-slate-600 hover:bg-slate-100 rounded text-xs font-bold"
                    >
                      -
                    </button>
                    <span className="w-6 text-center text-xs font-bold text-slate-900">{item.quantity}</span>
                    <button
                      onClick={() => updateCartQuantity(item.product.id, item.quantity + 1)}
                      className="w-5 h-5 flex items-center justify-center text-slate-600 hover:bg-slate-100 rounded text-xs font-bold"
                    >
                      +
                    </button>
                  </div>

                  {/* Line Total */}
                  <div className="text-right shrink-0 min-w-[60px]">
                    <p className="text-xs font-bold text-slate-900">₹{item.lineTotal.toFixed(2)}</p>
                  </div>

                  <button
                    onClick={() => removeFromCart(item.product.id)}
                    className="text-slate-400 hover:text-rose-600 p-1"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                </div>
              ))
            )}
          </div>

          {/* Cart Calculation Footer Summary */}
          <div className="pt-3 border-t border-slate-100 space-y-2 shrink-0 bg-slate-50/50 -mx-4 -mb-4 p-4 rounded-b-2xl">
            <div className="flex items-center justify-between text-xs text-slate-600">
              <span>Subtotal</span>
              <span className="font-mono">₹{subtotal.toFixed(2)}</span>
            </div>

            {itemDiscountTotal > 0 && (
              <div className="flex items-center justify-between text-xs text-emerald-600 font-medium">
                <span>Item Discounts</span>
                <span className="font-mono">-₹{itemDiscountTotal.toFixed(2)}</span>
              </div>
            )}

            {/* Extra Order Discount Input */}
            <div className="flex items-center justify-between text-xs text-slate-600">
              <span className="flex items-center gap-1">
                Extra Discount (₹)
              </span>
              <input
                type="number"
                min="0"
                value={posExtraDiscount}
                onChange={(e) => setPosExtraDiscount(Number(e.target.value))}
                className="w-20 px-2 py-0.5 bg-white border border-slate-200 rounded text-right text-xs font-mono font-bold"
              />
            </div>

            <div className="flex items-center justify-between text-xs text-slate-600">
              <span>GST Tax (CGST + SGST)</span>
              <span className="font-mono text-slate-700">₹{taxTotal.toFixed(2)}</span>
            </div>

            <div className="flex items-center justify-between text-xs text-slate-400">
              <span>Round off</span>
              <span className="font-mono">₹{roundOff}</span>
            </div>

            <div className="pt-2 border-t border-slate-200 flex items-center justify-between">
              <div>
                <span className="text-xs font-bold text-slate-500 uppercase">Grand Total</span>
                <p className="text-2xl font-black text-indigo-900 tracking-tight">₹{grandTotal.toLocaleString()}</p>
              </div>

              <div className="flex items-center gap-2">
                <button
                  onClick={handleHoldBill}
                  disabled={cart.length === 0}
                  className="px-3 py-3 bg-slate-200 hover:bg-slate-300 disabled:opacity-50 text-slate-800 rounded-xl text-xs font-bold transition-all flex items-center gap-1"
                  title="Hold Bill (F8)"
                >
                  <PauseCircle className="w-4 h-4" />
                  <span className="hidden sm:inline">Hold</span>
                </button>

                <button
                  onClick={() => {
                    if (cart.length === 0) {
                      showToast('Please add items to cart first', 'warning');
                      return;
                    }
                    setShowPaymentModal(true);
                  }}
                  disabled={cart.length === 0}
                  className="px-6 py-3 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white rounded-xl text-xs font-extrabold shadow-lg shadow-indigo-600/30 transition-all flex items-center gap-2"
                >
                  <CreditCard className="w-4 h-4" />
                  <span>PAY & BILL (F6)</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* PAYMENT MODAL FLOW (Requirement 12) */}
      <Modal
        isOpen={showPaymentModal}
        onClose={() => setShowPaymentModal(false)}
        title={`Complete Payment - Bill Total ₹${grandTotal}`}
        subtitle={`Customer: ${selectedCustomer.name}`}
        maxWidth="2xl"
      >
        <div className="space-y-5">
          {/* Payment Method Tabs */}
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-2">Select Payment Method</label>
            <div className="grid grid-cols-5 gap-2">
              {[
                { id: 'cash', label: 'Cash', icon: Banknote },
                { id: 'upi', label: 'UPI / QR', icon: QrCode },
                { id: 'card', label: 'Card', icon: CreditCard },
                { id: 'credit', label: 'Khata Credit', icon: Users },
                { id: 'split', label: 'Split Pay', icon: Percent },
              ].map((method) => {
                const Icon = method.icon;
                const active = selectedPaymentMethod === method.id;
                return (
                  <button
                    key={method.id}
                    type="button"
                    onClick={() => setSelectedPaymentMethod(method.id as PaymentMethod)}
                    className={`p-2.5 rounded-xl border flex flex-col items-center justify-center gap-1 transition-all ${
                      active
                        ? 'border-indigo-600 bg-indigo-50/80 text-indigo-700 font-bold ring-2 ring-indigo-600/20'
                        : 'border-slate-200 text-slate-600 hover:bg-slate-50'
                    }`}
                  >
                    <Icon className="w-4 h-4" />
                    <span className="text-[11px]">{method.label}</span>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Cash Flow */}
          {selectedPaymentMethod === 'cash' && (
            <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 space-y-3">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Amount Received (₹)</label>
                  <input
                    type="number"
                    value={amountReceived}
                    onChange={(e) => setAmountReceived(Number(e.target.value))}
                    className="w-full px-3 py-2 bg-white border border-slate-300 rounded-lg text-base font-bold text-slate-900"
                  />
                </div>
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1">Change to Return (₹)</label>
                  <div className="px-3 py-2 bg-emerald-50 border border-emerald-200 rounded-lg text-base font-bold text-emerald-700 font-mono">
                    ₹{Math.max(0, amountReceived - grandTotal).toFixed(2)}
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* UPI Flow */}
          {selectedPaymentMethod === 'upi' && (
            <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 flex flex-col items-center text-center space-y-3">
              <div className="w-36 h-36 bg-white p-2 border border-slate-300 rounded-xl shadow-xs flex items-center justify-center">
                <QrCode className="w-28 h-28 text-slate-800" />
              </div>
              <div>
                <p className="text-xs font-bold text-slate-900">Scan UPI QR Code to Pay ₹{grandTotal}</p>
                <p className="text-[11px] text-slate-500 font-mono">VPA: srimuruganmart@okhdfcbank</p>
              </div>
              <div className="w-full text-left">
                <label className="block text-xs font-semibold text-slate-700 mb-1">UPI Transaction Reference ID</label>
                <input
                  type="text"
                  value={upiRefId}
                  onChange={(e) => setUpiRefId(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-lg text-xs font-mono text-slate-800"
                />
              </div>
            </div>
          )}

          {/* Card Flow */}
          {selectedPaymentMethod === 'card' && (
            <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 space-y-2 text-center">
              <CreditCard className="w-10 h-10 text-indigo-600 mx-auto" />
              <p className="text-xs font-bold text-slate-900">Insert/Tap Customer Card on POS Machine</p>
              <p className="text-[11px] text-slate-500">POS Terminal connected to register POS-1</p>
            </div>
          )}

          {/* Khata Credit Flow */}
          {selectedPaymentMethod === 'credit' && (
            <div className="p-4 bg-amber-50 rounded-xl border border-amber-200 space-y-2">
              <div className="flex items-center gap-2 text-amber-800 font-semibold text-xs">
                <AlertTriangle className="w-4 h-4" />
                <span>Credit Khata Sale Confirmation</span>
              </div>
              <p className="text-xs text-amber-700">
                This bill amount of <strong>₹{grandTotal}</strong> will be added to{' '}
                <strong>{selectedCustomer.name}</strong>'s outstanding ledger balance.
              </p>
              <p className="text-[11px] text-amber-800 font-mono">
                Current Khata Due: ₹{selectedCustomer.outstandingBalance} ➔ New Due: ₹
                {selectedCustomer.outstandingBalance + grandTotal}
              </p>
            </div>
          )}

          {/* Split Payment Flow */}
          {selectedPaymentMethod === 'split' && (
            <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 space-y-3">
              <p className="text-xs font-bold text-slate-900">Specify Split Combination (Total ₹{grandTotal}):</p>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-[11px] font-semibold text-slate-600">Cash Amount</label>
                  <input
                    type="number"
                    value={splitDetails.cash}
                    onChange={(e) => setSplitDetails({ ...splitDetails, cash: Number(e.target.value) })}
                    className="w-full px-2.5 py-1.5 bg-white border border-slate-300 rounded text-xs font-bold"
                  />
                </div>
                <div>
                  <label className="block text-[11px] font-semibold text-slate-600">UPI Amount</label>
                  <input
                    type="number"
                    value={splitDetails.upi}
                    onChange={(e) => setSplitDetails({ ...splitDetails, upi: Number(e.target.value) })}
                    className="w-full px-2.5 py-1.5 bg-white border border-slate-300 rounded text-xs font-bold"
                  />
                </div>
              </div>
            </div>
          )}

          {/* Complete Payment Button */}
          <button
            onClick={handleExecutePayment}
            className="w-full py-3 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-sm shadow-md shadow-emerald-600/30 transition-all flex items-center justify-center gap-2"
          >
            <CheckCircle2 className="w-5 h-5" />
            <span>CONFIRM PAYMENT & GENERATE INVOICE</span>
          </button>
        </div>
      </Modal>

      {/* POST PAYMENT SUCCESS MODAL (Requirement 14) */}
      <Modal
        isOpen={showSuccessModal}
        onClose={() => setShowSuccessModal(false)}
        title="Payment Successful!"
        subtitle={`Invoice ${lastInvoice?.invoiceNumber} generated`}
        maxWidth="lg"
      >
        <div className="space-y-4 text-center py-2">
          <div className="w-14 h-14 rounded-full bg-emerald-100 text-emerald-600 flex items-center justify-center mx-auto">
            <CheckCircle2 className="w-8 h-8" />
          </div>

          <div>
            <h3 className="text-xl font-extrabold text-slate-900">₹{lastInvoice?.grandTotal.toLocaleString()} Paid</h3>
            <p className="text-xs text-slate-500 font-mono mt-0.5">
              Customer: {lastInvoice?.customerName} | Method: {lastInvoice?.paymentMethod.toUpperCase()}
            </p>
          </div>

          {/* Actions grid */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 pt-2 border-t border-slate-100">
            <button
              onClick={() => {
                setWaPhone(lastInvoice?.customerPhone || '');
                setShowWhatsAppModal(true);
              }}
              className="p-3 rounded-xl border border-emerald-200 bg-emerald-50 hover:bg-emerald-100 text-emerald-800 text-xs font-semibold flex flex-col items-center gap-1 transition-colors"
            >
              <Smartphone className="w-5 h-5 text-emerald-600" />
              <span>WhatsApp</span>
            </button>

            <button
              onClick={() => {
                setEmailTo(selectedCustomer.email || '');
                setShowEmailModal(true);
              }}
              className="p-3 rounded-xl border border-sky-200 bg-sky-50 hover:bg-sky-100 text-sky-800 text-xs font-semibold flex flex-col items-center gap-1 transition-colors"
            >
              <Mail className="w-5 h-5 text-sky-600" />
              <span>Email</span>
            </button>

            <button
              onClick={() => {
                if (lastInvoice) pdfService.downloadMockPDF(lastInvoice, useStore.getState().businessProfile);
              }}
              className="p-3 rounded-xl border border-indigo-200 bg-indigo-50 hover:bg-indigo-100 text-indigo-800 text-xs font-semibold flex flex-col items-center gap-1 transition-colors"
            >
              <Download className="w-5 h-5 text-indigo-600" />
              <span>PDF Bill</span>
            </button>

            <button
              onClick={() => pdfService.triggerPrint()}
              className="p-3 rounded-xl border border-slate-200 bg-slate-50 hover:bg-slate-100 text-slate-800 text-xs font-semibold flex flex-col items-center gap-1 transition-colors"
            >
              <Printer className="w-5 h-5 text-slate-600" />
              <span>Print</span>
            </button>
          </div>

          <button
            onClick={() => {
              setShowSuccessModal(false);
              clearCart();
            }}
            className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-bold transition-all"
          >
            Start Next Bill (F2)
          </button>
        </div>
      </Modal>

      {/* WHATSAPP MODAL (Requirement 15) */}
      <Modal
        isOpen={showWhatsAppModal}
        onClose={() => setShowWhatsAppModal(false)}
        title="Send Invoice via WhatsApp"
        maxWidth="md"
      >
        <div className="space-y-4">
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Customer Phone Number</label>
            <input
              type="text"
              value={waPhone}
              onChange={(e) => setWaPhone(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono font-semibold"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Message Preview</label>
            <textarea
              rows={5}
              readOnly
              value={
                lastInvoice
                  ? whatsappService.generateMessagePreview(
                      {
                        customerName: lastInvoice.customerName,
                        phone: waPhone,
                        invoiceNumber: lastInvoice.invoiceNumber,
                        grandTotal: lastInvoice.grandTotal,
                      },
                      'Sri Murugan Mart'
                    )
                  : ''
              }
              className="w-full p-3 bg-emerald-50/50 border border-emerald-200 rounded-lg text-xs font-mono text-slate-800"
            />
          </div>

          <button
            onClick={handleSendWhatsApp}
            disabled={isSendingWhatsApp}
            className="w-full py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-xs shadow-md transition-all flex items-center justify-center gap-2"
          >
            {isSendingWhatsApp ? 'Sending via WhatsApp Service...' : 'Send WhatsApp Invoice'}
          </button>
        </div>
      </Modal>

      {/* EMAIL MODAL */}
      <Modal isOpen={showEmailModal} onClose={() => setShowEmailModal(false)} title="Send Invoice via Email" maxWidth="md">
        <div className="space-y-4">
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Customer Email Address</label>
            <input
              type="email"
              value={emailTo}
              onChange={(e) => setEmailTo(e.target.value)}
              placeholder="customer@example.com"
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-medium"
            />
          </div>
          <button
            onClick={handleSendEmail}
            disabled={isSendingEmail}
            className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-bold text-xs"
          >
            {isSendingEmail ? 'Sending Email...' : 'Send Email Invoice'}
          </button>
        </div>
      </Modal>

      {/* HELD BILLS MODAL (Requirement 13) */}
      <Modal
        isOpen={showHeldBillsModal}
        onClose={() => setShowHeldBillsModal(false)}
        title={`Held Bills (${heldBills.length})`}
        maxWidth="lg"
      >
        <div className="space-y-3">
          {heldBills.length === 0 ? (
            <p className="text-xs text-slate-400 text-center py-6">No held bills currently in queue.</p>
          ) : (
            heldBills.map((bill) => (
              <div key={bill.id} className="p-3 bg-slate-50 rounded-xl border border-slate-200 flex items-center justify-between">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <span className="font-mono font-bold text-xs text-indigo-900">{bill.billNumber}</span>
                    <span className="text-xs font-semibold text-slate-700">{bill.customer.name}</span>
                  </div>
                  <p className="text-[11px] text-slate-500">
                    {bill.items.length} items • Held at {bill.heldAt}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-xs font-extrabold text-slate-900 pr-2">₹{bill.grandTotal}</span>
                  <button
                    onClick={() => {
                      resumeHeldBill(bill.id);
                      setShowHeldBillsModal(false);
                      showToast(`Resumed bill ${bill.billNumber}`, 'info');
                    }}
                    className="px-3 py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold rounded-lg"
                  >
                    Resume
                  </button>
                  <button
                    onClick={() => deleteHeldBill(bill.id)}
                    className="p-1.5 text-rose-600 hover:bg-rose-50 rounded-lg"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            ))
          )}
        </div>
      </Modal>

      {/* ADD CUSTOMER MODAL */}
      <Modal
        isOpen={showAddCustomerModal}
        onClose={() => setShowAddCustomerModal(false)}
        title="Add New Customer (F4)"
        maxWidth="md"
      >
        <form onSubmit={handleCreateCustomer} className="space-y-3">
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Customer Full Name *</label>
            <input
              type="text"
              required
              value={newCustName}
              onChange={(e) => setNewCustName(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-medium"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Phone Number *</label>
            <input
              type="text"
              required
              value={newCustPhone}
              onChange={(e) => setNewCustPhone(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">GSTIN Number (Optional)</label>
            <input
              type="text"
              value={newCustGstin}
              onChange={(e) => setNewCustGstin(e.target.value)}
              className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono uppercase"
            />
          </div>
          <button
            type="submit"
            className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-bold shadow-md"
          >
            Save Customer & Select
          </button>
        </form>
      </Modal>
    </div>
  );
};
