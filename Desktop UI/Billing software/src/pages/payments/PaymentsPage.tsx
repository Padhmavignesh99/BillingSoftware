import React from 'react';
import { useNavigate } from 'react-router-dom';
import { CreditCard, Users, Truck, Clock, CheckCircle2, ArrowRight } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';

export const PaymentsPage: React.FC = () => {
  const navigate = useNavigate();
  const { customers, suppliers, invoices } = useStore();

  const customerReceivablesTotal = customers.reduce((sum, c) => sum + c.outstandingBalance, 0);
  const supplierPayablesTotal = suppliers.reduce((sum, s) => sum + s.outstandingPayable, 0);

  return (
    <div className="space-y-6">
      <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Payments & Financial Receivables</h1>
        <p className="text-xs sm:text-sm text-slate-500">
          Track customer credit accounts (Khata) and supplier payables balance
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Customer Credit Khata Card */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
          <div className="flex items-center justify-between border-b border-slate-100 pb-3">
            <div className="flex items-center gap-2">
              <Users className="w-5 h-5 text-amber-600" />
              <h2 className="text-base font-bold text-slate-900">Customer Khata Receivables</h2>
            </div>
            <button onClick={() => navigate('/customers')} className="text-xs font-semibold text-indigo-600 hover:underline">
              Manage Customers
            </button>
          </div>

          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Total Customer Khata Due</span>
            <h3 className="text-3xl font-extrabold text-amber-600 tracking-tight mt-1">
              ₹{customerReceivablesTotal.toLocaleString()}
            </h3>
          </div>

          <div className="space-y-2">
            {customers
              .filter((c) => c.outstandingBalance > 0)
              .map((c) => (
                <div key={c.id} className="p-3 bg-slate-50 rounded-xl border border-slate-100 flex items-center justify-between">
                  <div>
                    <p className="text-xs font-bold text-slate-900">{c.name}</p>
                    <p className="text-[10px] text-slate-500 font-mono">{c.phone}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-xs font-bold text-amber-600 font-mono">₹{c.outstandingBalance.toLocaleString()}</p>
                    <button
                      onClick={() => navigate(`/customers/${c.id}/ledger`)}
                      className="text-[10px] text-indigo-600 font-semibold hover:underline"
                    >
                      View Ledger
                    </button>
                  </div>
                </div>
              ))}
          </div>
        </div>

        {/* Supplier Payables Card */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
          <div className="flex items-center justify-between border-b border-slate-100 pb-3">
            <div className="flex items-center gap-2">
              <Truck className="w-5 h-5 text-indigo-600" />
              <h2 className="text-base font-bold text-slate-900">Supplier Payables Balance</h2>
            </div>
            <button onClick={() => navigate('/suppliers')} className="text-xs font-semibold text-indigo-600 hover:underline">
              Manage Suppliers
            </button>
          </div>

          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Total Vendor Payables Due</span>
            <h3 className="text-3xl font-extrabold text-indigo-900 tracking-tight mt-1">
              ₹{supplierPayablesTotal.toLocaleString()}
            </h3>
          </div>

          <div className="space-y-2">
            {suppliers.map((s) => (
              <div key={s.id} className="p-3 bg-slate-50 rounded-xl border border-slate-100 flex items-center justify-between">
                <div>
                  <p className="text-xs font-bold text-slate-900">{s.companyName}</p>
                  <p className="text-[10px] text-slate-500 font-mono">{s.name} ({s.phone})</p>
                </div>
                <div className="text-right">
                  <p className="text-xs font-bold text-slate-900 font-mono">₹{s.outstandingPayable.toLocaleString()}</p>
                  <span className="text-[10px] text-slate-400">Payable</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};
