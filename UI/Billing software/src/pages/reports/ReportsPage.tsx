import React, { useState, useEffect } from 'react';
import {
  BarChart3,
  TrendingUp,
  FileSpreadsheet,
  Percent,
  Boxes,
} from 'lucide-react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { useStore } from '../../store/useStore';
import { exportToCSV } from '../../utils/exportUtils';
import { useToast } from '../../components/common/Toast';
import { api } from '../../services/apiClient';

export const ReportsPage: React.FC = () => {
  const { invoices, products, expenses, backendStatus } = useStore();
  const { showToast } = useToast();

  const [activeTab, setActiveTab] = useState<'sales' | 'profit' | 'tax' | 'inventory'>('sales');

  // API report data (loaded when backend is connected)
  const [apiSalesReport, setApiSalesReport] = useState<any>(null);
  const [apiProfitLoss, setApiProfitLoss] = useState<any>(null);
  const [apiTaxReport, setApiTaxReport] = useState<any>(null);
  const [apiStockReport, setApiStockReport] = useState<any>(null);

  useEffect(() => {
    if (backendStatus === 'connected') {
      api.reports.getSales().then(setApiSalesReport).catch(() => {});
      api.reports.getProfitLoss().then(setApiProfitLoss).catch(() => {});
      api.reports.getTax().then(setApiTaxReport).catch(() => {});
      api.reports.getProductStock().then(setApiStockReport).catch(() => {});
    }
  }, [backendStatus]);

  // Calculations — prefer API data, fall back to local store
  const grossSales = apiSalesReport?.totalSales ?? apiSalesReport?.grossSales ??
    invoices.reduce((sum, inv) => sum + inv.grandTotal, 0);
  const totalTaxCollected = apiTaxReport?.totalTax ?? apiTaxReport?.totalGst ??
    invoices.reduce((sum, inv) => sum + inv.taxTotal, 0);
  const cgstTotal = apiTaxReport?.cgst ?? invoices.reduce((sum, inv) => sum + inv.cgst, 0);
  const sgstTotal = apiTaxReport?.sgst ?? invoices.reduce((sum, inv) => sum + inv.sgst, 0);
  const totalExpenses = expenses.reduce((sum, e) => sum + e.amount, 0);
  const estimatedCostOfGoods = apiProfitLoss?.cogs ?? grossSales * 0.65;
  const netProfit = apiProfitLoss?.netProfit ??
    (grossSales - estimatedCostOfGoods - totalExpenses);

  const totalStockValuation = apiStockReport?.totalValuation ??
    products.reduce((sum, p) => sum + p.stock * p.sellingPrice, 0);

  // Category chart — compute from live invoice items if available
  const salesByCategoryData = (() => {
    const catMap: Record<string, number> = {};
    invoices.forEach((inv) => {
      inv.items.forEach((item: any) => {
        const cat = products.find((p) => p.id === item.productId)?.category || 'Other';
        catMap[cat] = (catMap[cat] || 0) + item.total;
      });
    });
    const entries = Object.entries(catMap).sort((a, b) => b[1] - a[1]).slice(0, 5);
    if (entries.length > 0) {
      return entries.map(([name, sales]) => ({ name, sales: Math.round(sales) }));
    }
    // Static fallback when no invoice data
    return [
      { name: 'Grocery', sales: 48500 },
      { name: 'Snacks', sales: 32400 },
      { name: 'Personal Care', sales: 22100 },
      { name: 'Home Clean', sales: 18900 },
      { name: 'Dairy', sales: 21000 },
    ];
  })();

  const handleExportCSV = () => {
    if (activeTab === 'sales') {
      exportToCSV('BillFlow_Sales_Report', invoices);
    } else if (activeTab === 'inventory') {
      exportToCSV('BillFlow_Inventory_Valuation', products);
    } else if (activeTab === 'tax') {
      exportToCSV('BillFlow_GST_Tax_Report', invoices);
    } else {
      exportToCSV('BillFlow_Profit_Loss_Statement', [
        { GrossSales: grossSales, COGS: estimatedCostOfGoods, OperatingExpenses: totalExpenses, NetProfit: netProfit },
      ]);
    }
    showToast('Report exported as CSV', 'success');
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Business Reports & Analytics</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            Sales trends, profit & loss statement, inventory valuation, and GST tax filings
          </p>
        </div>

        <button
          onClick={handleExportCSV}
          className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-md shadow-indigo-600/30 transition-all"
        >
          <FileSpreadsheet className="w-4 h-4" />
          <span>Export Active Report (CSV)</span>
        </button>
      </div>

      {/* Report Tabs */}
      <div className="flex items-center gap-2 border-b border-slate-200 pb-1">
        {[
          { id: 'sales', label: 'Sales Report', icon: TrendingUp },
          { id: 'profit', label: 'Profit & Loss', icon: BarChart3 },
          { id: 'tax', label: 'GST Tax Compliance', icon: Percent },
          { id: 'inventory', label: 'Inventory Valuation', icon: Boxes },
        ].map((tab) => {
          const Icon = tab.icon;
          const active = activeTab === tab.id;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as any)}
              className={`flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold transition-all ${
                active ? 'bg-indigo-600 text-white shadow-sm' : 'text-slate-600 hover:bg-slate-100'
              }`}
            >
              <Icon className="w-4 h-4" />
              <span>{tab.label}</span>
            </button>
          );
        })}
      </div>

      {/* Tab 1: Sales Report */}
      {activeTab === 'sales' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
              <span className="text-xs text-slate-500 font-semibold uppercase">Gross Sales Revenue</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-1">₹{grossSales.toLocaleString()}</h3>
            </div>
            <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
              <span className="text-xs text-slate-500 font-semibold uppercase">Total Invoices Issued</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-1">{invoices.length} Bills</h3>
            </div>
            <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
              <span className="text-xs text-slate-500 font-semibold uppercase">Average Ticket Order</span>
              <h3 className="text-2xl font-bold text-indigo-600 mt-1">
                ₹{(grossSales / (invoices.length || 1)).toFixed(0)}
              </h3>
            </div>
          </div>

          <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
            <h3 className="text-sm font-bold text-slate-900">Category Sales Breakdown</h3>
            <div className="h-64 w-full">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={salesByCategoryData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                  <XAxis dataKey="name" tick={{ fill: '#64748b', fontSize: 11 }} />
                  <YAxis tick={{ fill: '#64748b', fontSize: 11 }} tickFormatter={(val) => `₹${val / 1000}k`} />
                  <Tooltip formatter={(val: any) => [`₹${Number(val || 0).toLocaleString()}`, 'Sales']} />
                  <Bar dataKey="sales" fill="#4f46e5" radius={[6, 6, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>
      )}

      {/* Tab 2: Profit & Loss */}
      {activeTab === 'profit' && (
        <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-6 max-w-2xl">
          <h2 className="text-base font-bold text-slate-900 border-b border-slate-100 pb-3">
            Profit & Loss Summary Statement (Current Period)
          </h2>

          <div className="space-y-3 text-xs">
            <div className="flex justify-between py-2 border-b border-slate-100 font-semibold text-slate-800">
              <span>Gross Retail Sales Revenue</span>
              <span className="font-mono text-sm">₹{grossSales.toLocaleString()}</span>
            </div>

            <div className="flex justify-between py-2 border-b border-slate-100 text-rose-600">
              <span>Less: Estimated Cost of Goods Sold (COGS)</span>
              <span className="font-mono font-semibold">-₹{estimatedCostOfGoods.toLocaleString()}</span>
            </div>

            <div className="flex justify-between py-2 border-b border-slate-100 font-bold text-slate-900 bg-slate-50 p-2 rounded-lg">
              <span>Gross Margin</span>
              <span className="font-mono">₹{(grossSales - estimatedCostOfGoods).toLocaleString()}</span>
            </div>

            <div className="flex justify-between py-2 border-b border-slate-100 text-rose-600">
              <span>Less: Operating Expenses (Rent, Salary, Utilities)</span>
              <span className="font-mono font-semibold">-₹{totalExpenses.toLocaleString()}</span>
            </div>

            <div className="flex justify-between py-3 border-t border-slate-200 font-extrabold text-base text-emerald-700 bg-emerald-50 p-3 rounded-xl">
              <span>Net Operating Profit</span>
              <span className="font-mono">₹{netProfit.toLocaleString()}</span>
            </div>
          </div>
        </div>
      )}

      {/* Tab 3: GST Tax */}
      {activeTab === 'tax' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
              <span className="text-xs text-slate-500 font-semibold uppercase">Total GST Collected</span>
              <h3 className="text-2xl font-bold text-indigo-900 mt-1">₹{totalTaxCollected.toFixed(2)}</h3>
            </div>
            <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
              <span className="text-xs text-slate-500 font-semibold uppercase">CGST (Central Tax)</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-1">₹{cgstTotal.toFixed(2)}</h3>
            </div>
            <div className="bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
              <span className="text-xs text-slate-500 font-semibold uppercase">SGST (State Tax)</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-1">₹{sgstTotal.toFixed(2)}</h3>
            </div>
          </div>
        </div>
      )}

      {/* Tab 4: Inventory Valuation */}
      {activeTab === 'inventory' && (
        <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Total Inventory Stock Valuation</span>
            <h3 className="text-3xl font-extrabold text-slate-900 mt-1">₹{totalStockValuation.toLocaleString()}</h3>
            <p className="text-xs text-slate-500">Valued at current selling price across {products.length} product SKUs</p>
          </div>
        </div>
      )}
    </div>
  );
};
