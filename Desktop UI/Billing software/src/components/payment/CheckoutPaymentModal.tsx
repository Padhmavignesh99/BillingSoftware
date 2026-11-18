import React, { useState, useEffect, useCallback, useRef } from 'react';
import {
  X, Plus, Trash2, CreditCard, Smartphone, Banknote, Wallet,
  Building2, FileText, Gift, Users, ArrowRightLeft, CheckCircle2,
  Clock, AlertCircle, Monitor, Loader2, ChevronDown,
} from 'lucide-react';
import api from '../../services/apiClient';

// ─── Types ───────────────────────────────────────────────────────────────────
type PaymentMethodCode =
  | 'Cash' | 'UPI' | 'DebitCard' | 'CreditCard' | 'NetBanking'
  | 'Wallet' | 'GiftCard' | 'StoreCredit' | 'CustomerCredit'
  | 'BankTransfer' | 'Cheque' | 'EMI' | 'PaymentOnDelivery' | 'Online';

interface PaymentLine {
  id: string;
  method: PaymentMethodCode;
  amount: string;
  // cash
  cashReceived: string;
  // upi
  upiApp: string;
  upiRef: string;
  // card
  cardLast4: string;
  cardNetwork: string;
  cardApproval: string;
  terminalId: number | null;
  // emi
  emiBank: string;
  emiTenure: string;
  // cheque
  chequeNumber: string;
  chequeDate: string;
  chequeBank: string;
  // other
  referenceNumber: string;
  notes: string;
  // terminal tx
  terminalTxId: number | null;
  terminalStatus: 'idle' | 'pending' | 'success' | 'failed';
  terminalMessage: string;
}

interface PaymentMethodOption {
  code: PaymentMethodCode;
  label: string;
  icon: React.ReactNode;
  color: string;
}

interface Props {
  invoiceId: number;
  customerId?: number;
  customerName?: string;
  totalAmount: number;
  invoiceNumber: string;
  onSuccess: (payment: any) => void;
  onClose: () => void;
}

// ─── Constants ────────────────────────────────────────────────────────────────
const ALL_METHODS: PaymentMethodOption[] = [
  { code: 'Cash',            label: 'Cash',           icon: <Banknote className="w-4 h-4" />,        color: 'bg-green-50 border-green-200 text-green-700' },
  { code: 'UPI',             label: 'UPI',            icon: <Smartphone className="w-4 h-4" />,      color: 'bg-purple-50 border-purple-200 text-purple-700' },
  { code: 'DebitCard',       label: 'Debit Card',     icon: <CreditCard className="w-4 h-4" />,      color: 'bg-blue-50 border-blue-200 text-blue-700' },
  { code: 'CreditCard',      label: 'Credit Card',    icon: <CreditCard className="w-4 h-4" />,      color: 'bg-indigo-50 border-indigo-200 text-indigo-700' },
  { code: 'NetBanking',      label: 'Net Banking',    icon: <Building2 className="w-4 h-4" />,       color: 'bg-cyan-50 border-cyan-200 text-cyan-700' },
  { code: 'Wallet',          label: 'Wallet',         icon: <Wallet className="w-4 h-4" />,          color: 'bg-orange-50 border-orange-200 text-orange-700' },
  { code: 'GiftCard',        label: 'Gift Card',      icon: <Gift className="w-4 h-4" />,            color: 'bg-pink-50 border-pink-200 text-pink-700' },
  { code: 'StoreCredit',     label: 'Store Credit',   icon: <Wallet className="w-4 h-4" />,          color: 'bg-teal-50 border-teal-200 text-teal-700' },
  { code: 'CustomerCredit',  label: 'Khata / Credit', icon: <Users className="w-4 h-4" />,           color: 'bg-amber-50 border-amber-200 text-amber-700' },
  { code: 'BankTransfer',    label: 'Bank Transfer',  icon: <ArrowRightLeft className="w-4 h-4" />,  color: 'bg-sky-50 border-sky-200 text-sky-700' },
  { code: 'Cheque',          label: 'Cheque',         icon: <FileText className="w-4 h-4" />,        color: 'bg-stone-50 border-stone-200 text-stone-700' },
  { code: 'EMI',             label: 'EMI',            icon: <CreditCard className="w-4 h-4" />,      color: 'bg-violet-50 border-violet-200 text-violet-700' },
  { code: 'PaymentOnDelivery', label: 'Pay on Delivery', icon: <Clock className="w-4 h-4" />,       color: 'bg-rose-50 border-rose-200 text-rose-700' },
  { code: 'Online',          label: 'Online',         icon: <Monitor className="w-4 h-4" />,         color: 'bg-lime-50 border-lime-200 text-lime-700' },
];

const DENOMINATIONS = [10, 20, 50, 100, 200, 500, 2000];
const UPI_APPS = ['Google Pay', 'PhonePe', 'Paytm', 'BHIM', 'Amazon Pay', 'Other'];
const CARD_NETWORKS = ['Visa', 'Mastercard', 'RuPay', 'Amex'];
const EMI_BANKS = ['HDFC', 'ICICI', 'SBI', 'Axis', 'Kotak', 'Yes Bank', 'Other'];
const EMI_TENURES = ['3', '6', '9', '12', '18', '24'];

function newLine(method: PaymentMethodCode, amount = ''): PaymentLine {
  return {
    id: Math.random().toString(36).slice(2),
    method,
    amount,
    cashReceived: '', upiApp: 'Google Pay', upiRef: '',
    cardLast4: '', cardNetwork: 'Visa', cardApproval: '',
    terminalId: null,
    emiBank: 'HDFC', emiTenure: '6',
    chequeNumber: '', chequeDate: '', chequeBank: '',
    referenceNumber: '', notes: '',
    terminalTxId: null, terminalStatus: 'idle', terminalMessage: '',
  };
}

function fmt(n: number) {
  return '₹' + n.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

// ─── Component ────────────────────────────────────────────────────────────────
export const CheckoutPaymentModal: React.FC<Props> = ({
  invoiceId, customerId, customerName, totalAmount, invoiceNumber, onSuccess, onClose,
}) => {
  const [lines, setLines] = useState<PaymentLine[]>([newLine('Cash', String(totalAmount))]);
  const [enabledMethods, setEnabledMethods] = useState<PaymentMethodCode[]>(
    ALL_METHODS.map(m => m.code)
  );
  const [terminals, setTerminals] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showMethodPicker, setShowMethodPicker] = useState(false);
  const pollingRef = useRef<Record<string, ReturnType<typeof setInterval>>>({});

  // Load config & terminals
  useEffect(() => {
    api.paymentConfig.get()
      .then(r => {
        const enabled = (r.data as any[]).filter(m => m.isEnabled).map(m => m.methodCode as PaymentMethodCode);
        if (enabled.length) setEnabledMethods(enabled);
      })
      .catch(() => {}); // fall back to all methods

    api.paymentTerminals.list()
      .then(r => setTerminals(r.data || []))
      .catch(() => {});
  }, []);

  // Cleanup polling on unmount
  useEffect(() => {
    return () => {
      Object.values(pollingRef.current).forEach(clearInterval);
    };
  }, []);

  // ── derived totals ────────────────────────────────────────────────────────
  const paidTotal = lines.reduce((s, l) => s + (parseFloat(l.amount) || 0), 0);
  const remaining = totalAmount - paidTotal;
  const cashLine = lines.find(l => l.method === 'Cash');
  const cashReceived = parseFloat(cashLine?.cashReceived || '0') || 0;
  const cashChange = cashLine
    ? Math.max(0, cashReceived - (parseFloat(cashLine.amount) || 0))
    : 0;
  const canComplete = paidTotal >= totalAmount && paidTotal > 0;

  // ── line updater ──────────────────────────────────────────────────────────
  const updateLine = useCallback((id: string, patch: Partial<PaymentLine>) => {
    setLines(prev => prev.map(l => l.id === id ? { ...l, ...patch } : l));
  }, []);

  const removeLine = (id: string) => {
    setLines(prev => prev.filter(l => l.id !== id));
    if (pollingRef.current[id]) {
      clearInterval(pollingRef.current[id]);
      delete pollingRef.current[id];
    }
  };

  const addLine = (method: PaymentMethodCode) => {
    const rem = totalAmount - lines.reduce((s, l) => s + (parseFloat(l.amount) || 0), 0);
    setLines(prev => [...prev, newLine(method, rem > 0 ? String(rem) : '')]);
    setShowMethodPicker(false);
  };

  // ── POS terminal payment ──────────────────────────────────────────────────
  const sendToTerminal = async (lineId: string) => {
    const line = lines.find(l => l.id === lineId);
    if (!line || !line.terminalId) return;
    updateLine(lineId, { terminalStatus: 'pending', terminalMessage: 'Sending to terminal…' });
    try {
      const r = await api.paymentTransactions.initiate({
        paymentId: 0,
        terminalId: line.terminalId,
        amount: parseFloat(line.amount),
        method: line.method,
      });
      const txId = r.data?.id;
      updateLine(lineId, { terminalTxId: txId, terminalMessage: 'Waiting for customer…' });

      // Poll every 3s
      pollingRef.current[lineId] = setInterval(async () => {
        try {
          const st = await api.paymentTransactions.getStatus(txId);
          const status = st.data?.status?.toLowerCase();
          if (status === 'success' || status === 'approved') {
            clearInterval(pollingRef.current[lineId]);
            delete pollingRef.current[lineId];
            updateLine(lineId, {
              terminalStatus: 'success',
              terminalMessage: 'Payment approved ✓',
              cardApproval: st.data?.providerTransactionId || '',
            });
          } else if (status === 'failed' || status === 'declined') {
            clearInterval(pollingRef.current[lineId]);
            delete pollingRef.current[lineId];
            updateLine(lineId, { terminalStatus: 'failed', terminalMessage: st.data?.statusMessage || 'Payment declined' });
          }
        } catch {}
      }, 3000);
    } catch (e: any) {
      updateLine(lineId, { terminalStatus: 'failed', terminalMessage: e.message || 'Failed to reach terminal' });
    }
  };

  // ── submit ────────────────────────────────────────────────────────────────
  const handleComplete = async () => {
    setError('');
    if (!canComplete) { setError('Payment amount must equal or exceed the bill total.'); return; }

    setLoading(true);
    try {
      const payload = {
        invoiceId,
        customerId,
        totalAmount,
        paidAmount: paidTotal,
        changeAmount: cashChange,
        notes: '',
        details: lines.map(l => ({
          method: l.method,
          amount: parseFloat(l.amount),
          cashReceived: l.method === 'Cash' ? parseFloat(l.cashReceived) || undefined : undefined,
          changeGiven: l.method === 'Cash' ? cashChange : undefined,
          upiApp: l.method === 'UPI' ? l.upiApp : undefined,
          referenceNumber: l.referenceNumber || l.upiRef || l.cardApproval || undefined,
          cardLastFour: l.cardLast4 || undefined,
          cardNetwork: l.cardNetwork || undefined,
          chequeNumber: l.chequeNumber || undefined,
          chequeDate: l.chequeDate || undefined,
          chequeBankName: l.chequeBank || undefined,
          emiBankName: l.emiBank || undefined,
          emiTenureMonths: l.emiTenure ? parseInt(l.emiTenure) : undefined,
          terminalId: l.terminalId || undefined,
          terminalTransactionId: l.terminalTxId || undefined,
          notes: l.notes || undefined,
        })),
      };
      const res = await api.payments.create(payload);
      onSuccess(res.data);
    } catch (e: any) {
      setError(e.message || 'Payment failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const methodInfo = (code: PaymentMethodCode) => ALL_METHODS.find(m => m.code === code)!;
  const availableMethods = ALL_METHODS.filter(m => enabledMethods.includes(m.code));

  // ─── render ───────────────────────────────────────────────────────────────
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-2 sm:p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl max-h-[95vh] flex flex-col">

        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
          <div>
            <h2 className="text-lg font-bold text-slate-900">Payment Checkout</h2>
            <p className="text-xs text-slate-500">Invoice #{invoiceNumber}{customerName ? ` · ${customerName}` : ''}</p>
          </div>
          <button onClick={onClose} className="p-2 rounded-lg hover:bg-slate-100 text-slate-500">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Totals bar */}
        <div className="grid grid-cols-3 gap-3 px-6 py-4 bg-slate-50 border-b border-slate-200">
          <div className="text-center">
            <p className="text-xs text-slate-500 font-semibold">Bill Total</p>
            <p className="text-lg font-extrabold text-slate-900">{fmt(totalAmount)}</p>
          </div>
          <div className="text-center">
            <p className="text-xs text-slate-500 font-semibold">Paid</p>
            <p className={`text-lg font-extrabold ${paidTotal >= totalAmount ? 'text-green-600' : 'text-amber-600'}`}>
              {fmt(paidTotal)}
            </p>
          </div>
          <div className="text-center">
            {remaining > 0 ? (
              <>
                <p className="text-xs text-red-500 font-semibold">Remaining</p>
                <p className="text-lg font-extrabold text-red-600">{fmt(remaining)}</p>
              </>
            ) : (
              <>
                <p className="text-xs text-green-600 font-semibold">Change</p>
                <p className="text-lg font-extrabold text-green-600">{fmt(Math.abs(remaining))}</p>
              </>
            )}
          </div>
        </div>

        {/* Payment lines */}
        <div className="flex-1 overflow-y-auto px-6 py-4 space-y-4">
          {lines.map((line, idx) => {
            const info = methodInfo(line.method);
            return (
              <div key={line.id} className={`rounded-xl border-2 p-4 ${info.color}`}>
                {/* Line header */}
                <div className="flex items-center justify-between mb-3">
                  <div className="flex items-center gap-2">
                    {info.icon}
                    <span className="text-sm font-bold">{info.label}</span>
                    {lines.length > 1 && <span className="text-xs opacity-60">#{idx + 1}</span>}
                  </div>
                  {lines.length > 1 && (
                    <button onClick={() => removeLine(line.id)} className="p-1 hover:bg-black/10 rounded-lg">
                      <Trash2 className="w-4 h-4" />
                    </button>
                  )}
                </div>

                {/* Amount */}
                <div className="mb-3">
                  <label className="text-xs font-semibold opacity-70 block mb-1">Payment Amount (₹)</label>
                  <input
                    type="number"
                    min={0}
                    step={0.01}
                    value={line.amount}
                    onChange={e => updateLine(line.id, { amount: e.target.value })}
                    className="w-full px-3 py-2 rounded-lg border border-current/20 bg-white/70 text-sm font-bold focus:outline-none focus:ring-2 focus:ring-current/30"
                    placeholder="0.00"
                  />
                </div>

                {/* Cash-specific */}
                {line.method === 'Cash' && (
                  <>
                    <div className="mb-2">
                      <label className="text-xs font-semibold opacity-70 block mb-1">Cash Received (₹)</label>
                      <input
                        type="number"
                        min={0}
                        step={0.01}
                        value={line.cashReceived}
                        onChange={e => updateLine(line.id, { cashReceived: e.target.value })}
                        className="w-full px-3 py-2 rounded-lg border border-current/20 bg-white/70 text-sm font-bold focus:outline-none focus:ring-2 focus:ring-current/30"
                        placeholder="Enter cash received"
                      />
                    </div>
                    {/* Quick denominations */}
                    <div className="flex flex-wrap gap-1 mb-2">
                      {DENOMINATIONS.map(d => (
                        <button
                          key={d}
                          onClick={() => updateLine(line.id, { cashReceived: String(d) })}
                          className="px-2 py-1 text-xs font-bold rounded-lg bg-white/70 border border-current/20 hover:bg-white"
                        >
                          ₹{d}
                        </button>
                      ))}
                    </div>
                    {cashChange > 0 && (
                      <div className="flex items-center gap-2 bg-green-100 rounded-lg px-3 py-2">
                        <CheckCircle2 className="w-4 h-4 text-green-600" />
                        <span className="text-sm font-bold text-green-700">Change to return: {fmt(cashChange)}</span>
                      </div>
                    )}
                  </>
                )}

                {/* UPI-specific */}
                {line.method === 'UPI' && (
                  <div className="grid grid-cols-2 gap-2">
                    <div>
                      <label className="text-xs font-semibold opacity-70 block mb-1">UPI App</label>
                      <select
                        value={line.upiApp}
                        onChange={e => updateLine(line.id, { upiApp: e.target.value })}
                        className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs font-semibold focus:outline-none"
                      >
                        {UPI_APPS.map(a => <option key={a}>{a}</option>)}
                      </select>
                    </div>
                    <div>
                      <label className="text-xs font-semibold opacity-70 block mb-1">UPI Reference</label>
                      <input
                        type="text"
                        value={line.upiRef}
                        onChange={e => updateLine(line.id, { upiRef: e.target.value })}
                        className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs focus:outline-none"
                        placeholder="Transaction ID"
                      />
                    </div>
                  </div>
                )}

                {/* Card-specific (Debit / Credit) */}
                {(line.method === 'DebitCard' || line.method === 'CreditCard') && (
                  <>
                    <div className="grid grid-cols-2 gap-2 mb-2">
                      <div>
                        <label className="text-xs font-semibold opacity-70 block mb-1">Card Network</label>
                        <select
                          value={line.cardNetwork}
                          onChange={e => updateLine(line.id, { cardNetwork: e.target.value })}
                          className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs font-semibold focus:outline-none"
                        >
                          {CARD_NETWORKS.map(n => <option key={n}>{n}</option>)}
                        </select>
                      </div>
                      <div>
                        <label className="text-xs font-semibold opacity-70 block mb-1">Last 4 Digits</label>
                        <input
                          type="text"
                          maxLength={4}
                          value={line.cardLast4}
                          onChange={e => updateLine(line.id, { cardLast4: e.target.value.replace(/\D/g, '') })}
                          className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs font-mono focus:outline-none"
                          placeholder="XXXX"
                        />
                      </div>
                    </div>
                    <div className="grid grid-cols-2 gap-2 mb-2">
                      <div>
                        <label className="text-xs font-semibold opacity-70 block mb-1">Approval Code</label>
                        <input
                          type="text"
                          value={line.cardApproval}
                          onChange={e => updateLine(line.id, { cardApproval: e.target.value })}
                          className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs font-mono focus:outline-none"
                          placeholder="From receipt"
                          readOnly={line.terminalStatus === 'success'}
                        />
                      </div>
                      <div>
                        <label className="text-xs font-semibold opacity-70 block mb-1">POS Terminal</label>
                        <select
                          value={line.terminalId ?? ''}
                          onChange={e => updateLine(line.id, { terminalId: e.target.value ? Number(e.target.value) : null })}
                          className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs font-semibold focus:outline-none"
                        >
                          <option value="">Manual entry</option>
                          {terminals.map(t => <option key={t.id} value={t.id}>{t.terminalName}</option>)}
                        </select>
                      </div>
                    </div>
                    {line.terminalId && (
                      <div className="flex items-center gap-2 mt-1">
                        <button
                          onClick={() => sendToTerminal(line.id)}
                          disabled={line.terminalStatus === 'pending' || line.terminalStatus === 'success'}
                          className="flex items-center gap-1 px-3 py-1.5 bg-white/80 border border-current/30 rounded-lg text-xs font-bold hover:bg-white disabled:opacity-50"
                        >
                          {line.terminalStatus === 'pending'
                            ? <><Loader2 className="w-3 h-3 animate-spin" /> Waiting…</>
                            : line.terminalStatus === 'success'
                            ? <><CheckCircle2 className="w-3 h-3 text-green-600" /> Approved</>
                            : <><Monitor className="w-3 h-3" /> Send to Machine</>
                          }
                        </button>
                        {line.terminalMessage && (
                          <span className={`text-xs font-semibold ${
                            line.terminalStatus === 'success' ? 'text-green-700'
                            : line.terminalStatus === 'failed' ? 'text-red-600'
                            : 'text-current opacity-70'}`}>
                            {line.terminalMessage}
                          </span>
                        )}
                      </div>
                    )}
                  </>
                )}

                {/* EMI-specific */}
                {line.method === 'EMI' && (
                  <div className="grid grid-cols-3 gap-2">
                    <div>
                      <label className="text-xs font-semibold opacity-70 block mb-1">EMI Bank</label>
                      <select
                        value={line.emiBank}
                        onChange={e => updateLine(line.id, { emiBank: e.target.value })}
                        className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs focus:outline-none"
                      >
                        {EMI_BANKS.map(b => <option key={b}>{b}</option>)}
                      </select>
                    </div>
                    <div>
                      <label className="text-xs font-semibold opacity-70 block mb-1">Tenure (months)</label>
                      <select
                        value={line.emiTenure}
                        onChange={e => updateLine(line.id, { emiTenure: e.target.value })}
                        className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs focus:outline-none"
                      >
                        {EMI_TENURES.map(t => <option key={t}>{t}</option>)}
                      </select>
                    </div>
                    <div>
                      <label className="text-xs font-semibold opacity-70 block mb-1">Card Last 4</label>
                      <input
                        type="text"
                        maxLength={4}
                        value={line.cardLast4}
                        onChange={e => updateLine(line.id, { cardLast4: e.target.value.replace(/\D/g, '') })}
                        className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs font-mono focus:outline-none"
                        placeholder="XXXX"
                      />
                    </div>
                  </div>
                )}

                {/* Cheque-specific */}
                {line.method === 'Cheque' && (
                  <div className="grid grid-cols-3 gap-2">
                    <div>
                      <label className="text-xs font-semibold opacity-70 block mb-1">Cheque No.</label>
                      <input
                        type="text"
                        value={line.chequeNumber}
                        onChange={e => updateLine(line.id, { chequeNumber: e.target.value })}
                        className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs font-mono focus:outline-none"
                        placeholder="123456"
                      />
                    </div>
                    <div>
                      <label className="text-xs font-semibold opacity-70 block mb-1">Date</label>
                      <input
                        type="date"
                        value={line.chequeDate}
                        onChange={e => updateLine(line.id, { chequeDate: e.target.value })}
                        className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs focus:outline-none"
                      />
                    </div>
                    <div>
                      <label className="text-xs font-semibold opacity-70 block mb-1">Bank</label>
                      <input
                        type="text"
                        value={line.chequeBank}
                        onChange={e => updateLine(line.id, { chequeBank: e.target.value })}
                        className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs focus:outline-none"
                        placeholder="Bank name"
                      />
                    </div>
                  </div>
                )}

                {/* Generic reference for other methods */}
                {!['Cash', 'UPI', 'DebitCard', 'CreditCard', 'EMI', 'Cheque'].includes(line.method) && (
                  <div>
                    <label className="text-xs font-semibold opacity-70 block mb-1">Reference / Notes</label>
                    <input
                      type="text"
                      value={line.referenceNumber}
                      onChange={e => updateLine(line.id, { referenceNumber: e.target.value })}
                      className="w-full px-2 py-1.5 rounded-lg border border-current/20 bg-white/70 text-xs focus:outline-none"
                      placeholder="Transaction / reference number"
                    />
                  </div>
                )}
              </div>
            );
          })}

          {/* Add another method */}
          <div className="relative">
            <button
              onClick={() => setShowMethodPicker(p => !p)}
              className="w-full flex items-center justify-center gap-2 py-3 rounded-xl border-2 border-dashed border-slate-300 text-slate-500 hover:border-indigo-400 hover:text-indigo-600 transition-colors text-sm font-semibold"
            >
              <Plus className="w-4 h-4" />
              Add Another Payment Method (Split)
              <ChevronDown className={`w-4 h-4 transition-transform ${showMethodPicker ? 'rotate-180' : ''}`} />
            </button>

            {showMethodPicker && (
              <div className="absolute z-10 mt-2 w-full bg-white rounded-xl shadow-xl border border-slate-200 p-3 grid grid-cols-2 sm:grid-cols-3 gap-2">
                {availableMethods
                  .filter(m => !lines.some(l => l.method === m.code) || m.code === 'Cash')
                  .map(m => (
                    <button
                      key={m.code}
                      onClick={() => addLine(m.code)}
                      className={`flex items-center gap-2 px-3 py-2 rounded-lg border text-xs font-semibold ${m.color} hover:opacity-80`}
                    >
                      {m.icon}
                      {m.label}
                    </button>
                  ))}
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="px-6 py-4 border-t border-slate-200 bg-slate-50 rounded-b-2xl">
          {error && (
            <div className="flex items-center gap-2 mb-3 p-3 bg-red-50 border border-red-200 rounded-lg">
              <AlertCircle className="w-4 h-4 text-red-600 flex-shrink-0" />
              <p className="text-xs text-red-700 font-semibold">{error}</p>
            </div>
          )}

          {/* Split summary */}
          {lines.length > 1 && (
            <div className="mb-3 space-y-1">
              {lines.map(l => (
                <div key={l.id} className="flex items-center justify-between text-xs">
                  <span className="text-slate-600 flex items-center gap-1">
                    {methodInfo(l.method).icon}
                    {methodInfo(l.method).label}
                  </span>
                  <span className="font-bold text-slate-800 font-mono">{fmt(parseFloat(l.amount) || 0)}</span>
                </div>
              ))}
              <div className="flex items-center justify-between text-xs pt-1 border-t border-slate-200">
                <span className="font-bold text-slate-700">Total Paid</span>
                <span className={`font-extrabold font-mono ${canComplete ? 'text-green-600' : 'text-amber-600'}`}>
                  {fmt(paidTotal)}
                </span>
              </div>
            </div>
          )}

          <div className="flex gap-3">
            <button
              onClick={onClose}
              className="flex-1 py-3 rounded-xl border border-slate-300 text-sm font-bold text-slate-700 hover:bg-slate-100"
            >
              Cancel
            </button>
            <button
              onClick={handleComplete}
              disabled={!canComplete || loading}
              className="flex-[2] py-3 rounded-xl bg-indigo-600 text-white text-sm font-bold hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              {loading
                ? <><Loader2 className="w-4 h-4 animate-spin" /> Processing…</>
                : <><CheckCircle2 className="w-4 h-4" /> Complete Payment · {fmt(totalAmount)}</>
              }
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
