import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Search,
  Download,
  Eye,
  Printer,
  Smartphone,
  Mail,
  Filter,
  Calendar,
  FileSpreadsheet,
  Trash2,
  Plus,
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { exportToCSV } from '../../utils/exportUtils';
import { pdfService } from '../../services/pdfService';
import { Modal } from '../../components/ui/Modal';
import { whatsappService } from '../../services/whatsappService';
import { useToast } from '../../components/common/Toast';

export const InvoicesPage: React.FC = () => {
  const navigate = useNavigate();
  const { invoices, businessProfile } = useStore();
  const { showToast } = useToast();

  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');

  const [sendWaModalInvoice, setSendWaModalInvoice] = useState<any>(null);
  const [waPhone, setWaPhone] = useState('');
  const [isSendingWa, setIsSendingWa] = useState(false);

  const filteredInvoices = invoices.filter((inv) => {
    const matchesStatus = statusFilter === 'all' || inv.paymentStatus === statusFilter;
    const q = searchQuery.toLowerCase().trim();
    const matchesQuery =
      !q ||
      inv.invoiceNumber.toLowerCase().includes(q) ||
      inv.customerName.toLowerCase().includes(q) ||
      inv.customerPhone.includes(q);
    return matchesStatus && matchesQuery;
  });

  const handleExportCSV = () => {
    exportToCSV('BillFlow_Invoices_Export', filteredInvoices, [
      { key: 'invoiceNumber', label: 'Invoice Number' },
      { key: 'customerName', label: 'Customer Name' },
      { key: 'customerPhone', label: 'Customer Phone' },
      { key: 'grandTotal', label: 'Grand Total (₹)' },
      { key: 'paymentMethod', label: 'Payment Method' },
      { key: 'paymentStatus', label: 'Payment Status' },
      { key: 'cashierName', label: 'Cashier' },
      { key: 'createdAt', label: 'Date & Time' },
    ]);
    showToast('Invoices exported as CSV', 'success');
  };

  const handleSendWa = async () => {
    if (!sendWaModalInvoice) return;
    setIsSendingWa(true);
    try {
      await whatsappService.sendInvoice({
        customerName: sendWaModalInvoice.customerName,
        phone: waPhone || sendWaModalInvoice.customerPhone,
        invoiceNumber: sendWaModalInvoice.invoiceNumber,
        grandTotal: sendWaModalInvoice.grandTotal,
      });
      setIsSendingWa(false);
      setSendWaModalInvoice(null);
      showToast(`Invoice sent via WhatsApp to ${waPhone}`, 'success');
    } catch (err: any) {
      setIsSendingWa(false);
      showToast(err.message || 'WhatsApp sending failed', 'error');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Invoice Management</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            View, search, print, download and share all billing receipts
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={handleExportCSV}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-xs font-semibold transition-colors"
          >
            <FileSpreadsheet className="w-4 h-4 text-emerald-600" />
            <span>Export CSV</span>
          </button>

          <button
            onClick={() => navigate('/billing')}
            className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-md shadow-indigo-600/30 transition-all"
          >
            <Plus className="w-4 h-4" />
            <span>New Bill (F2)</span>
          </button>
        </div>
      </div>

      {/* Filters Bar */}
      <div className="bg-white p-4 rounded-2xl border border-slate-200/80 shadow-2xs flex flex-wrap items-center justify-between gap-3">
        <div className="relative flex-1 min-w-[240px]">
          <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search by Invoice #, customer name or phone..."
            className="w-full pl-10 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-600/30"
          />
        </div>

        <div className="flex items-center gap-2">
          <Filter className="w-4 h-4 text-slate-400" />
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="px-3 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-700"
          >
            <option value="all">All Payment Statuses</option>
            <option value="paid">Paid Only</option>
            <option value="pending">Pending / Khata Credit</option>
            <option value="partially_paid">Partially Paid</option>
            <option value="refunded">Refunded</option>
          </select>
        </div>
      </div>

      {/* Invoices Data Table */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50/80 text-slate-500 font-semibold border-b border-slate-100">
              <tr>
                <th className="py-3.5 px-4">Invoice #</th>
                <th className="py-3.5 px-4">Customer</th>
                <th className="py-3.5 px-4">Date & Time</th>
                <th className="py-3.5 px-4">Method</th>
                <th className="py-3.5 px-4">Amount</th>
                <th className="py-3.5 px-4">Status</th>
                <th className="py-3.5 px-4 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 font-medium">
              {filteredInvoices.length === 0 ? (
                <tr>
                  <td colSpan={7} className="text-center py-10 text-slate-400">
                    No invoices match your search criteria.
                  </td>
                </tr>
              ) : (
                filteredInvoices.map((inv) => (
                  <tr key={inv.id} className="hover:bg-slate-50/60 transition-colors">
                    <td className="py-3.5 px-4 font-mono font-bold text-slate-900">{inv.invoiceNumber}</td>
                    <td className="py-3.5 px-4">
                      <p className="font-semibold text-slate-800">{inv.customerName}</p>
                      <p className="text-[11px] text-slate-400 font-mono">{inv.customerPhone}</p>
                    </td>
                    <td className="py-3.5 px-4 text-slate-500 text-[11px]">
                      {new Date(inv.createdAt).toLocaleString()}
                    </td>
                    <td className="py-3.5 px-4 uppercase font-mono text-[11px] text-slate-600">{inv.paymentMethod}</td>
                    <td className="py-3.5 px-4 font-extrabold text-slate-900">₹{inv.grandTotal.toLocaleString()}</td>
                    <td className="py-3.5 px-4">
                      <Badge variant={inv.paymentStatus === 'paid' ? 'success' : 'warning'} size="sm">
                        {inv.paymentStatus}
                      </Badge>
                    </td>
                    <td className="py-3.5 px-4 text-right space-x-1">
                      <button
                        onClick={() => navigate(`/invoices/${inv.id}`)}
                        className="p-1.5 text-slate-600 hover:text-indigo-600 hover:bg-slate-100 rounded-lg"
                        title="View Details"
                      >
                        <Eye className="w-4 h-4" />
                      </button>

                      <button
                        onClick={() => pdfService.downloadMockPDF(inv, businessProfile)}
                        className="p-1.5 text-slate-600 hover:text-indigo-600 hover:bg-slate-100 rounded-lg"
                        title="Download PDF"
                      >
                        <Download className="w-4 h-4" />
                      </button>

                      <button
                        onClick={() => {
                          setSendWaModalInvoice(inv);
                          setWaPhone(inv.customerPhone);
                        }}
                        className="p-1.5 text-slate-600 hover:text-emerald-600 hover:bg-slate-100 rounded-lg"
                        title="Send WhatsApp"
                      >
                        <Smartphone className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* WHATSAPP MODAL FROM INVOICES TABLE */}
      {sendWaModalInvoice && (
        <Modal
          isOpen={!!sendWaModalInvoice}
          onClose={() => setSendWaModalInvoice(null)}
          title={`Send Invoice ${sendWaModalInvoice.invoiceNumber}`}
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

            <button
              onClick={handleSendWa}
              disabled={isSendingWa}
              className="w-full py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-xs shadow-md"
            >
              {isSendingWa ? 'Sending...' : 'Confirm WhatsApp Send'}
            </button>
          </div>
        </Modal>
      )}
    </div>
  );
};
