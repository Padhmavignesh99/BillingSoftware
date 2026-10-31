import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Download } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { pdfService } from '../../services/pdfService';
import { useToast } from '../../components/common/Toast';
import { SmartPrintButton } from '../../components/print/SmartPrintButton';

export const InvoiceDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { invoices, businessProfile } = useStore();
  const { showToast } = useToast();

  const invoice = invoices.find((inv) => inv.id === id || inv.invoiceNumber === id) || invoices[0];

  if (!invoice) {
    return (
      <div className="p-8 text-center space-y-4">
        <p className="text-sm text-slate-500">Invoice not found.</p>
        <button onClick={() => navigate('/invoices')} className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-xs">
          Back to Invoices
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      {/* Top Navigation & Actions */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-4 rounded-2xl border border-slate-200/80 shadow-2xs no-print">
        <button
          onClick={() => navigate('/invoices')}
          className="flex items-center gap-2 text-xs font-semibold text-slate-600 hover:text-slate-900"
        >
          <ArrowLeft className="w-4 h-4" />
          <span>Back to Invoices List</span>
        </button>

        <div className="flex items-center gap-2">
          <button
            onClick={() => pdfService.downloadMockPDF(invoice, businessProfile)}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-xs font-semibold transition-colors"
          >
            <Download className="w-4 h-4 text-indigo-600" />
            <span>Download PDF</span>
          </button>

          <SmartPrintButton
            invoiceId={invoice.id}
            label="Print Tax Invoice"
            variant="primary"
          />
        </div>
      </div>

      {/* Printable Invoice Container */}
      <div id="printable-invoice" className="bg-white p-8 rounded-2xl border border-slate-200/80 shadow-md space-y-6">
        {/* Header Store Details */}
        <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4 border-b border-slate-200 pb-6">
          <div className="space-y-1">
            <h1 className="text-2xl font-black text-indigo-900 tracking-tight">{businessProfile.name}</h1>
            <p className="text-xs text-slate-600">{businessProfile.storeName}</p>
            <p className="text-xs text-slate-500">{businessProfile.address}, {businessProfile.city}, {businessProfile.state} - {businessProfile.pincode}</p>
            <p className="text-xs font-mono text-slate-700">GSTIN: {businessProfile.gstin} | Phone: {businessProfile.phone}</p>
          </div>

          <div className="text-left sm:text-right space-y-1">
            <Badge variant="indigo" size="md">
              TAX INVOICE
            </Badge>
            <h2 className="text-xl font-bold font-mono text-slate-900 mt-1">{invoice.invoiceNumber}</h2>
            <p className="text-xs text-slate-500">Date: {new Date(invoice.createdAt).toLocaleString()}</p>
            <p className="text-xs text-slate-500">Register: {invoice.registerId} | Cashier: {invoice.cashierName}</p>
          </div>
        </div>

        {/* Billed To Customer Info */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 bg-slate-50 p-4 rounded-xl border border-slate-100 text-xs">
          <div>
            <p className="font-semibold text-slate-400 uppercase tracking-wider text-[10px] mb-1">Customer Billed To</p>
            <p className="font-bold text-slate-900 text-sm">{invoice.customerName}</p>
            <p className="text-slate-600 font-mono">Phone: {invoice.customerPhone}</p>
            {invoice.customerGstin && <p className="text-slate-600 font-mono">GSTIN: {invoice.customerGstin}</p>}
          </div>

          <div className="sm:text-right">
            <p className="font-semibold text-slate-400 uppercase tracking-wider text-[10px] mb-1">Payment Information</p>
            <p className="font-bold text-slate-900 uppercase font-mono">{invoice.paymentMethod} Payment</p>
            <p className="text-slate-600">Status: <span className="font-bold text-emerald-600 uppercase">{invoice.paymentStatus}</span></p>
          </div>
        </div>

        {/* Line Items Table */}
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-100 text-slate-700 font-bold border-b border-slate-200">
              <tr>
                <th className="py-2.5 px-3">#</th>
                <th className="py-2.5 px-3">Product Description</th>
                <th className="py-2.5 px-3">SKU</th>
                <th className="py-2.5 px-3 text-center">Qty</th>
                <th className="py-2.5 px-3 text-right">Price (₹)</th>
                <th className="py-2.5 px-3 text-right">GST %</th>
                <th className="py-2.5 px-3 text-right">Amount (₹)</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {invoice.items.map((item, idx) => (
                <tr key={idx}>
                  <td className="py-3 px-3 font-mono text-slate-400">{idx + 1}</td>
                  <td className="py-3 px-3 font-semibold text-slate-900">{item.productName}</td>
                  <td className="py-3 px-3 font-mono text-slate-500">{item.sku}</td>
                  <td className="py-3 px-3 text-center font-bold text-slate-800">
                    {item.quantity} {item.unit}
                  </td>
                  <td className="py-3 px-3 text-right font-mono">₹{item.price.toFixed(2)}</td>
                  <td className="py-3 px-3 text-right font-mono text-slate-500">{item.taxRate}%</td>
                  <td className="py-3 px-3 text-right font-mono font-bold text-slate-900">₹{item.total.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Invoice Summary Totals */}
        <div className="flex flex-col sm:flex-row justify-between gap-6 pt-4 border-t border-slate-200 text-xs">
          <div className="max-w-xs space-y-1">
            <p className="font-semibold text-slate-700">Terms & Conditions:</p>
            <p className="text-[11px] text-slate-500 leading-relaxed">{businessProfile.invoiceSettings.termsAndConditions}</p>
          </div>

          <div className="w-full sm:w-72 space-y-2 bg-slate-50 p-4 rounded-xl border border-slate-100">
            <div className="flex justify-between text-slate-600">
              <span>Subtotal</span>
              <span className="font-mono font-semibold">₹{invoice.subtotal.toFixed(2)}</span>
            </div>

            <div className="flex justify-between text-slate-600">
              <span>Discounts</span>
              <span className="font-mono text-emerald-600">-₹{(invoice.itemDiscountTotal + invoice.extraDiscount).toFixed(2)}</span>
            </div>

            <div className="flex justify-between text-slate-600">
              <span>CGST</span>
              <span className="font-mono">₹{invoice.cgst.toFixed(2)}</span>
            </div>

            <div className="flex justify-between text-slate-600">
              <span>SGST</span>
              <span className="font-mono">₹{invoice.sgst.toFixed(2)}</span>
            </div>

            <div className="flex justify-between text-slate-400">
              <span>Round off</span>
              <span className="font-mono">₹{invoice.roundOff.toFixed(2)}</span>
            </div>

            <div className="pt-2 border-t border-slate-200 flex justify-between font-black text-sm text-indigo-900">
              <span>Grand Total</span>
              <span>₹{invoice.grandTotal.toLocaleString('en-IN')}</span>
            </div>
          </div>
        </div>

        <div className="pt-6 border-t border-slate-100 text-center text-xs text-slate-400">
          {businessProfile.invoiceSettings.footerMessage}
        </div>
      </div>
    </div>
  );
};
