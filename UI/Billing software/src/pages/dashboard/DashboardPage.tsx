import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  TrendingUp,
  ShoppingBag,
  Clock,
  AlertTriangle,
  Plus,
  ArrowUpRight,
  Users,
  Receipt,
  Monitor,
  Sparkles,
  ChevronRight,
  Package,
} from 'lucide-react';
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { api } from '../../services/apiClient';

export const DashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const {
    invoices,
    products,
    customers,
    registers,
    backendStatus,
    getActiveTenant,
  } = useStore();

  const activeTenant = getActiveTenant();
  const [latestInsight, setLatestInsight] = useState<{ title: string; content: string } | null>(null);

  useEffect(() => {
    if (backendStatus === 'connected') {
      api.request<{ title: string; content: string }[]>('/api/ai/insights?page=1&pageSize=1')
        .then((items) => setLatestInsight(items[0] || null))
        .catch(() => setLatestInsight(null));
    }
  }, [backendStatus]);

  const today = new Date().toDateString();
  const todaysInvoices = invoices.filter((invoice) => new Date(invoice.createdAt).toDateString() === today);
  const todaySales = todaysInvoices.reduce((sum, invoice) => sum + invoice.grandTotal, 0);
  const todayOrders = todaysInvoices.length;
  const pendingPaymentsTotal = customers.reduce((sum, customer) => sum + customer.outstandingBalance, 0);
  const lowStockProducts = products.filter(
    (p) => p.stock <= p.minStock || p.status === 'low_stock' || p.status === 'out_of_stock'
  );

  const salesTrendData = Array.from({ length: 24 }, (_, hour) => ({
    time: String(hour).padStart(2, '0') + ':00',
    sales: todaysInvoices
      .filter((invoice) => new Date(invoice.createdAt).getHours() === hour)
      .reduce((sum, invoice) => sum + invoice.grandTotal, 0),
  }));

  return (
    <div className="space-y-5 w-full">

      {/* Welcome Banner — white with green left border accent */}
      <div className="bg-white border border-green-200 rounded-2xl p-5 flex flex-col md:flex-row md:items-center justify-between gap-4 shadow-sm">
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <span className="flex items-center gap-1.5 text-[11px] font-bold text-emerald-700 bg-emerald-50 px-2.5 py-0.5 rounded-full border border-emerald-200">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
              Live
            </span>
          </div>
          <h1 className="text-2xl font-black text-slate-900 tracking-tight">{activeTenant.name}</h1>
          <p className="text-xs text-slate-400">Real-time sales velocity, inventory & customer khata analytics</p>
        </div>

        <div className="flex items-center gap-2 flex-wrap shrink-0">
          <button
            onClick={() => navigate('/billing')}
            className="flex items-center gap-2 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-xs font-bold shadow-sm shadow-emerald-600/20 transition-all"
          >
            <ShoppingBag className="w-4 h-4" />
            New Bill (F2)
          </button>
          <button
            onClick={() => navigate('/products/new')}
            className="flex items-center gap-2 px-4 py-2 bg-white hover:bg-green-50 text-slate-700 rounded-xl text-xs font-bold border border-green-200 transition-colors"
          >
            <Plus className="w-4 h-4 text-emerald-600" />
            Add Product
          </button>
          <button
            onClick={() => navigate('/customers')}
            className="flex items-center gap-2 px-4 py-2 bg-white hover:bg-green-50 text-slate-700 rounded-xl text-xs font-bold border border-green-200 transition-colors"
          >
            <Users className="w-4 h-4 text-emerald-600" />
            Khata Ledger
          </button>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Today's Sales */}
        <div className="bg-white p-5 rounded-2xl border border-green-100 shadow-sm hover:shadow-md transition-all space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider">Today's Sales</span>
            <div className="w-9 h-9 rounded-xl bg-emerald-50 flex items-center justify-center">
              <TrendingUp className="w-4.5 h-4.5 text-emerald-600" />
            </div>
          </div>
          <div>
            <p className="text-2xl font-black text-slate-900 tracking-tight font-mono">
              ₹{todaySales.toLocaleString('en-IN')}
            </p>
          </div>
        </div>

        {/* Today's Orders */}
        <div className="bg-white p-5 rounded-2xl border border-green-100 shadow-sm hover:shadow-md transition-all space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider">Today's Orders</span>
            <div className="w-9 h-9 rounded-xl bg-blue-50 flex items-center justify-center">
              <Receipt className="w-4.5 h-4.5 text-blue-500" />
            </div>
          </div>
          <div>
            <p className="text-2xl font-black text-slate-900 tracking-tight font-mono">{todayOrders} Bills</p>
            <div className="flex items-center gap-1 mt-1 text-xs font-semibold text-slate-500">
              <ArrowUpRight className="w-3.5 h-3.5 text-emerald-500" />
              <span>Avg ₹{(todaySales / (todayOrders || 1)).toFixed(0)} per bill</span>
            </div>
          </div>
        </div>

        {/* Khata Outstanding */}
        <div className="bg-white p-5 rounded-2xl border border-green-100 shadow-sm hover:shadow-md transition-all space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider">Khata Outstanding</span>
            <div className="w-9 h-9 rounded-xl bg-amber-50 flex items-center justify-center">
              <Clock className="w-4.5 h-4.5 text-amber-500" />
            </div>
          </div>
          <div>
            <p className="text-2xl font-black text-slate-900 tracking-tight font-mono">
              ₹{pendingPaymentsTotal.toLocaleString('en-IN')}
            </p>
            <div className="flex items-center gap-1 mt-1 text-xs font-semibold text-amber-600">
              <span>{customers.filter((c) => c.outstandingBalance > 0).length} customers due</span>
            </div>
          </div>
        </div>

        {/* Low Stock */}
        <div className="bg-white p-5 rounded-2xl border border-green-100 shadow-sm hover:shadow-md transition-all space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider">Low Stock Alerts</span>
            <div className="w-9 h-9 rounded-xl bg-rose-50 flex items-center justify-center">
              <AlertTriangle className="w-4.5 h-4.5 text-rose-500" />
            </div>
          </div>
          <div>
            <p className="text-2xl font-black text-slate-900 tracking-tight font-mono">{lowStockProducts.length} Items</p>
            <div className="flex items-center gap-1 mt-1 text-xs font-semibold text-rose-500">
              <span>Action needed in inventory</span>
            </div>
          </div>
        </div>
      </div>

      {/* Chart + Right Column */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        {/* Sales Chart */}
        <div className="xl:col-span-2 bg-white p-5 rounded-2xl border border-green-100 shadow-sm space-y-4">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
            <div>
              <h2 className="text-sm font-bold text-slate-800">Hourly Sales Velocity</h2>
              <p className="text-xs text-slate-400 mt-0.5">Live peak hours transaction performance across POS registers</p>
            </div>
            <span className="px-3 py-1.5 bg-green-50 border border-green-200 rounded-xl text-xs font-semibold text-slate-600">Today (Hourly)</span>
          </div>

          <div className="h-64 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={salesTrendData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                <defs>
                  <linearGradient id="salesGrad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#10b981" stopOpacity={0.25} />
                    <stop offset="95%" stopColor="#10b981" stopOpacity={0.0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0fdf4" />
                <XAxis dataKey="time" axisLine={false} tickLine={false} tick={{ fill: '#94a3b8', fontSize: 11 }} />
                <YAxis
                  axisLine={false}
                  tickLine={false}
                  tick={{ fill: '#94a3b8', fontSize: 11 }}
                  tickFormatter={(val) => `₹${val / 1000}k`}
                />
                <Tooltip
                  formatter={(val: any) => [`₹${Number(val || 0).toLocaleString()}`, 'Sales']}
                  contentStyle={{
                    backgroundColor: '#fff',
                    borderRadius: '12px',
                    color: '#0f172a',
                    fontSize: '12px',
                    border: '1px solid #d1fae5',
                    boxShadow: '0 4px 16px -2px rgba(16,185,129,0.12)',
                  }}
                />
                <Area type="monotone" dataKey="sales" stroke="#10b981" strokeWidth={2.5} fill="url(#salesGrad)" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Right column — low stock + AI */}
        <div className="space-y-4">
          {/* Low Stock Widget */}
          <div className="bg-white p-5 rounded-2xl border border-green-100 shadow-sm space-y-3">
            <div className="flex items-center justify-between border-b border-green-50 pb-3">
              <h3 className="text-sm font-bold text-slate-800 flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 text-amber-500" />
                Low Stock Threshold
              </h3>
              <button
                onClick={() => navigate('/inventory')}
                className="text-xs font-bold text-emerald-600 hover:text-emerald-800 transition-colors"
              >
                View All
              </button>
            </div>

            <div className="space-y-2 max-h-52 overflow-y-auto pr-1">
              {lowStockProducts.length > 0 ? (
                lowStockProducts.slice(0, 4).map((p) => (
                  <div
                    key={p.id}
                    className="flex items-center justify-between p-2.5 bg-green-50/50 rounded-xl border border-green-100"
                  >
                    <div className="min-w-0 pr-2">
                      <p className="text-xs font-semibold text-slate-800 truncate">{p.name}</p>
                      <p className="text-[10px] text-slate-400 font-mono">SKU: {p.sku}</p>
                    </div>
                    <Badge variant={p.stock === 0 ? 'danger' : 'warning'} size="sm">
                      {p.stock} {p.unit} left
                    </Badge>
                  </div>
                ))
              ) : (
                <p className="text-xs text-slate-400 py-3 text-center">All products are well stocked!</p>
              )}
            </div>
          </div>

          {/* AI Insight — white with green accent, no dark bg */}
          <div className="bg-white border border-green-200 p-5 rounded-2xl shadow-sm space-y-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-emerald-50 border border-emerald-200 text-[10px] font-black text-emerald-700">
                <Sparkles className="w-3 h-3 text-amber-500" />
                AI BUSINESS INSIGHT
              </div>
              <button
                onClick={() => navigate('/ai-insights')}
                className="text-xs text-emerald-600 hover:text-emerald-800 font-semibold flex items-center gap-0.5 transition-colors"
              >
                Explore <ChevronRight className="w-3.5 h-3.5" />
              </button>
            </div>
            <div>
              <h4 className="text-sm font-bold text-slate-800 mb-1">
                {latestInsight?.title || 'No insights yet'}
              </h4>
              <p className="text-xs text-slate-500 leading-relaxed">
                {latestInsight?.content || 'Insights generated for this business will appear here.'}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Recent Invoices + Registers */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        {/* Invoices Table */}
        <div className={`bg-white rounded-2xl border border-green-100 shadow-sm overflow-hidden ${backendStatus === 'connected' ? 'xl:col-span-3' : 'xl:col-span-2'}`}>
          <div className="p-5 border-b border-green-50 flex items-center justify-between">
            <div>
              <h2 className="text-sm font-bold text-slate-800">Recent Completed Invoices</h2>
              <p className="text-xs text-slate-400 mt-0.5">Live transaction stream from active POS registers</p>
            </div>
            <button
              onClick={() => navigate('/invoices')}
              className="text-xs font-bold text-emerald-600 hover:text-emerald-800 transition-colors"
            >
              View All
            </button>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs">
              <thead className="bg-green-50/60 text-slate-400 uppercase text-[10px] font-bold border-b border-green-100">
                <tr>
                  <th className="py-3 px-4">Invoice #</th>
                  <th className="py-3 px-4">Customer</th>
                  <th className="py-3 px-4">Method</th>
                  <th className="py-3 px-4">Amount</th>
                  <th className="py-3 px-4">Status</th>
                  <th className="py-3 px-4 text-right">Time</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-green-50 font-medium">
                {invoices.slice(0, 5).map((inv) => (
                  <tr key={inv.id} className="hover:bg-green-50/40 transition-colors">
                    <td className="py-3 px-4 font-mono font-bold text-slate-800">{inv.invoiceNumber}</td>
                    <td className="py-3 px-4 text-slate-700">{inv.customerName}</td>
                    <td className="py-3 px-4 uppercase font-mono text-[10px] text-slate-400">{inv.paymentMethod}</td>
                    <td className="py-3 px-4 font-black font-mono text-slate-900">₹{inv.grandTotal.toLocaleString()}</td>
                    <td className="py-3 px-4">
                      <Badge variant={inv.paymentStatus === 'paid' ? 'success' : 'warning'} size="sm">
                        {inv.paymentStatus}
                      </Badge>
                    </td>
                    <td className="py-3 px-4 text-right text-slate-400 text-[10px] font-mono">
                      {new Date(inv.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Registers */}
        {backendStatus !== 'connected' && <div className="bg-white p-5 rounded-2xl border border-green-100 shadow-sm space-y-4">
          <div className="border-b border-green-50 pb-3 flex items-center justify-between">
            <h3 className="text-sm font-bold text-slate-800 flex items-center gap-2">
              <Monitor className="w-4 h-4 text-emerald-600" />
              Registers & Counter Health
            </h3>
            <span className="text-[11px] text-emerald-600 font-bold">Shift Open</span>
          </div>

          <div className="space-y-3">
            {registers.map((reg) => (
              <div
                key={reg.id}
                className="p-3 bg-green-50/60 rounded-xl border border-green-100 space-y-1.5"
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="font-mono text-xs font-bold text-slate-800">{reg.code}</span>
                    <span className="text-xs text-slate-500">({reg.name})</span>
                  </div>
                  <Badge variant={reg.status === 'online' ? 'success' : 'warning'} size="sm">
                    {reg.status}
                  </Badge>
                </div>
                <div className="flex items-center justify-between text-[10px] text-slate-400">
                  <span>Cashier: {reg.currentCashier}</span>
                  <span className="font-mono font-bold text-slate-700">₹{reg.currentCash.toLocaleString()}</span>
                </div>
              </div>
            ))}
          </div>
        </div>}
      </div>
    </div>
  );
};
