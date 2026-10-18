import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Users, Receipt, Smartphone, CheckCircle2, DollarSign, Calendar } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { useToast } from '../../components/common/Toast';
import { whatsappService } from '../../services/whatsappService';

export const CustomerLedgerPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { customers, invoices, recordCustomerPayment } = useStore();
  const { showToast } = useToast();

  const customer = customers.find((c) => c.id === id) || customers[1];
  const customerInvoices = invoices.filter((inv) => inv.customerId === customer?.id);

  if (!customer) {
    return <div className="p-8 text-center text-slate-500">Customer account not found.</div>;
  }

  const handleSendWaLedger = async () => {
    try {
      await whatsappService.sendInvoice({
        customerName: customer.name,
        phone: customer.phone,
        invoiceNumber: 'KHATA-STATEMENT',
        grandTotal: customer.outstandingBalance,
      });
      showToast(`Sent simulated WhatsApp Khata statement to ${customer.name}`, 'success');
    } catch (err: any) {
      showToast(err.message || 'WhatsApp failed', 'error');
    }
  };

  return (
    <div className="space-y-6 w-full">
      {/* Header Bar */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div className="flex items-center gap-3">
          <button
            onClick={() => navigate('/customers')}
            className="p-2 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <h1 className="text-xl font-bold text-slate-900">{customer.name} - Khata Ledger</h1>
            <p className="text-xs text-slate-500 font-mono">
              Phone: {customer.phone} | Account Type: {customer.type.toUpperCase()}
            </p>
          </div>
        </div>

        <button
          onClick={handleSendWaLedger}
          className="flex items-center gap-2 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-xs font-bold shadow-md"
        >
          <Smartphone className="w-4 h-4" />
          <span>Send WhatsApp Ledger</span>
        </button>
      </div>

      {/* Customer Profile Summary Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <span className="text-xs text-slate-500 font-semibold uppercase">Total Purchases</span>
          <h3 className="text-2xl font-bold text-slate-900 mt-1">₹{customer.totalPurchases.toLocaleString()}</h3>
          <p className="text-[11px] text-slate-400">{customerInvoices.length} billing receipts</p>
        </div>

        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <span className="text-xs text-slate-500 font-semibold uppercase">Khata Outstanding</span>
          <h3 className="text-2xl font-bold text-amber-600 mt-1">₹{customer.outstandingBalance.toLocaleString()}</h3>
          <p className="text-[11px] text-amber-600 font-medium">Pending credit due</p>
        </div>

        <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
          <span className="text-xs text-slate-500 font-semibold uppercase">Loyalty Reward Points</span>
          <h3 className="text-2xl font-bold text-indigo-600 mt-1">{customer.loyaltyPoints} PTS</h3>
          <p className="text-[11px] text-indigo-500">1 pt per ₹100 spend</p>
        </div>
      </div>

      {/* Invoices & Khata Transactions History */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs p-5 space-y-4">
        <h3 className="text-sm font-bold text-slate-900 border-b border-slate-100 pb-3 flex items-center gap-2">
          <Receipt className="w-4 h-4 text-indigo-600" />
          <span>Transaction & Billing History</span>
        </h3>

        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 text-slate-500 font-semibold border-b border-slate-100">
              <tr>
                <th className="py-3 px-3">Date</th>
                <th className="py-3 px-3">Invoice #</th>
                <th className="py-3 px-3">Payment Method</th>
                <th className="py-3 px-3">Amount (₹)</th>
                <th className="py-3 px-3">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 font-medium">
              {customerInvoices.length === 0 ? (
                <tr>
                  <td colSpan={5} className="text-center py-6 text-slate-400">
                    No transactions found for this customer.
                  </td>
                </tr>
              ) : (
                customerInvoices.map((inv) => (
                  <tr key={inv.id} className="hover:bg-slate-50/60">
                    <td className="py-3 px-3 text-slate-500 font-mono text-[11px]">
                      {new Date(inv.createdAt).toLocaleDateString()}
                    </td>
                    <td className="py-3 px-3 font-mono font-bold text-slate-900">{inv.invoiceNumber}</td>
                    <td className="py-3 px-3 font-mono uppercase">{inv.paymentMethod}</td>
                    <td className="py-3 px-3 font-bold text-slate-900">₹{inv.grandTotal.toLocaleString()}</td>
                    <td className="py-3 px-3">
                      <Badge variant={inv.paymentStatus === 'paid' ? 'success' : 'warning'} size="sm">
                        {inv.paymentStatus}
                      </Badge>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
