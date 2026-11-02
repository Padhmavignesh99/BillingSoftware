import React, { useState, useEffect } from 'react';
import { TrendingUp, TrendingDown, DollarSign, BarChart3, Download } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { api } from '../../services/apiClient';

interface PLData {
  totalRevenue: number; totalCOGS: number; grossProfit: number; grossMargin: number;
  totalExpenses: number; netProfit: number; netMargin: number;
  revenueByCategory?: { name: string; amount: number }[];
  expensesByCategory?: { name: string; amount: number }[];
  monthlyData?: { month: string; revenue: number; expenses: number; profit: number }[];
}

const StatCard: React.FC<{ label: string; value: number; sub?: string; positive?: boolean; icon: React.ElementType }> = ({ label, value, sub, positive, icon: Icon }) => (
  <div className="bg-white border border-green-100 rounded-xl p-4">
    <div className="flex items-center justify-between mb-2">
      <p className="text-xs text-slate-500 font-medium">{label}</p>
      <Icon className={`w-4 h-4 ${positive === false ? 'text-rose-400' : 'text-emerald-500'}`} />
    </div>
    <p className={`text-xl font-bold ${positive === false ? 'text-rose-600' : value < 0 ? 'text-rose-600' : 'text-slate-800'}`}>
      ₹{Math.abs(value).toLocaleString('en-IN', { maximumFractionDigits: 0 })}
    </p>
    {sub && <p className={`text-xs mt-0.5 font-semibold ${value < 0 ? 'text-rose-500' : 'text-emerald-600'}`}>{sub}</p>}
  </div>
);

export const ProfitLossPage: React.FC = () => {
  const [data, setData] = useState<PLData | null>(null);
  const [loading, setLoading] = useState(true);
  const [from, setFrom] = useState(() => { const d = new Date(); d.setDate(1); return d.toISOString().slice(0, 10); });
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.request<PLData>(`/api/reports/profit-loss?from=${from}&to=${to}`);
      setData(res);
    } catch { setData(null); }
    setLoading(false);
  };

  useEffect(() => { load(); }, [from, to]);

  const chartData = data?.monthlyData ?? (data ? [{ month: 'Period', revenue: data.totalRevenue, expenses: data.totalExpenses, profit: data.netProfit }] : []);

  return (
    <div className="p-4 md:p-6 space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-emerald-100 rounded-xl flex items-center justify-center"><TrendingUp className="w-5 h-5 text-emerald-600" /></div>
          <div><h1 className="text-lg font-bold text-slate-800">Profit & Loss Statement</h1><p className="text-xs text-slate-500">Income, expenses and net profit</p></div>
        </div>
        <div className="flex items-center gap-2">
          <input type="date" value={from} onChange={e => setFrom(e.target.value)} className="px-2 py-1.5 text-xs border border-slate-200 rounded-lg focus:ring-1 focus:ring-emerald-400 outline-none" />
          <span className="text-xs text-slate-400">to</span>
          <input type="date" value={to} onChange={e => setTo(e.target.value)} className="px-2 py-1.5 text-xs border border-slate-200 rounded-lg focus:ring-1 focus:ring-emerald-400 outline-none" />
        </div>
      </div>

      {loading ? <div className="p-10 text-center text-sm text-slate-400">Loading...</div> : !data ? (
        <div className="p-10 text-center text-sm text-slate-400">No data available for selected period</div>
      ) : (
        <>
          {/* Summary Cards */}
          <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
            <StatCard label="Total Revenue" value={data.totalRevenue} icon={TrendingUp} />
            <StatCard label="Cost of Goods" value={data.totalCOGS} icon={TrendingDown} positive={false} />
            <StatCard label="Gross Profit" value={data.grossProfit} sub={`${data.grossMargin?.toFixed(1)}% margin`} icon={DollarSign} />
            <StatCard label="Total Expenses" value={data.totalExpenses} icon={BarChart3} positive={false} />
            <StatCard label="Net Profit" value={data.netProfit} sub={`${data.netMargin?.toFixed(1)}% margin`} icon={TrendingUp} />
          </div>

          {/* Chart */}
          {chartData.length > 0 && (
            <div className="bg-white border border-green-100 rounded-xl p-4">
              <p className="text-sm font-semibold text-slate-700 mb-4">Revenue vs Expenses vs Profit</p>
              <ResponsiveContainer width="100%" height={220}>
                <BarChart data={chartData} margin={{ top: 0, right: 0, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
                  <XAxis dataKey="month" tick={{ fontSize: 10 }} />
                  <YAxis tick={{ fontSize: 10 }} tickFormatter={(v) => `₹${(v / 1000).toFixed(0)}k`} />
                  <Tooltip formatter={(v: number) => `₹${v.toLocaleString('en-IN')}`} />
                  <Legend wrapperStyle={{ fontSize: 11 }} />
                  <Bar dataKey="revenue" name="Revenue" fill="#10b981" radius={[3, 3, 0, 0]} />
                  <Bar dataKey="expenses" name="Expenses" fill="#f87171" radius={[3, 3, 0, 0]} />
                  <Bar dataKey="profit" name="Net Profit" fill="#3b82f6" radius={[3, 3, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          )}

          {/* P&L Statement */}
          <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
            <div className="px-4 py-3 border-b border-green-100 bg-slate-50">
              <p className="text-sm font-bold text-slate-700">Profit & Loss Statement</p>
            </div>
            <div className="p-4 space-y-1 text-xs">
              {[
                { label: 'Revenue', value: data.totalRevenue, bold: false, indent: false },
                { label: 'Less: Cost of Goods Sold', value: -data.totalCOGS, bold: false, indent: true },
                { label: 'Gross Profit', value: data.grossProfit, bold: true, indent: false },
                { label: 'Less: Operating Expenses', value: -data.totalExpenses, bold: false, indent: true },
                { label: 'Net Profit / (Loss)', value: data.netProfit, bold: true, indent: false },
              ].map((row, i) => (
                <div key={i} className={`flex justify-between py-1.5 ${row.bold ? 'border-t border-b border-slate-200 font-bold text-sm' : ''} ${i === 0 ? '' : ''}`}>
                  <span className={`${row.indent ? 'pl-4 text-slate-500' : 'text-slate-700'} ${row.bold ? 'text-slate-800' : ''}`}>{row.label}</span>
                  <span className={`${row.value < 0 ? 'text-rose-600' : 'text-slate-800'} ${row.bold ? 'text-slate-800' : ''}`}>
                    {row.value < 0 ? `(₹${Math.abs(row.value).toLocaleString('en-IN')})` : `₹${row.value.toLocaleString('en-IN')}`}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  );
};

