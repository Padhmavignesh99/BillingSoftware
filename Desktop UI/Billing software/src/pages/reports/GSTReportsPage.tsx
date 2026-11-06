import React, { useState, useEffect } from 'react';
import { FileText, Download, AlertCircle } from 'lucide-react';
import { api } from '../../services/apiClient';

interface TaxData {
  totalTaxableAmount?: number; totalTaxAmount?: number; totalCGST?: number; totalSGST?: number; totalIGST?: number;
  gstr1Summary?: { b2b: number; b2c: number; exports: number; creditNotes: number };
  gstr3bSummary?: { outwardSupplies: number; inwardSupplies: number; itcAvailable: number; taxPayable: number };
  hsnSummary?: { hsn: string; description: string; qty: number; taxableValue: number; tax: number }[];
  byMonth?: { month: string; taxable: number; cgst: number; sgst: number; igst: number }[];
}

type TaxTab = 'gstr1' | 'gstr3b' | 'hsn' | 'raw';

const months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
const years = [2024, 2025, 2026];

export const GSTReportsPage: React.FC = () => {
  const [tab, setTab] = useState<TaxTab>('gstr1');
  const [data, setData] = useState<TaxData | null>(null);
  const [loading, setLoading] = useState(true);
  const [month, setMonth] = useState(new Date().getMonth());
  const [year, setYear] = useState(new Date().getFullYear());

  const load = async () => {
    setLoading(true);
    try {
      const from = `${year}-${String(month + 1).padStart(2, '0')}-01`;
      const lastDay = new Date(year, month + 1, 0).getDate();
      const to = `${year}-${String(month + 1).padStart(2, '0')}-${lastDay}`;
      const res = await api.request<TaxData>(`/api/reports/tax?from=${from}&to=${to}`);
      setData(res);
    } catch { setData(null); }
    setLoading(false);
  };

  useEffect(() => { load(); }, [month, year]);

  const exportGST = async () => {
    try {
      const from = `${year}-${String(month + 1).padStart(2, '0')}-01`;
      const lastDay = new Date(year, month + 1, 0).getDate();
      const to = `${year}-${String(month + 1).padStart(2, '0')}-${lastDay}`;
      const blob = await api.getBlob(`/api/reports/export?reportType=tax&format=EXCEL&from=${from}&to=${to}`);
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = `GST-Report-${months[month]}-${year}.xlsx`;
      a.click();
    } catch { }
  };

  return (
    <div className="p-4 md:p-6 space-y-4 max-w-5xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-blue-100 rounded-xl flex items-center justify-center"><FileText className="w-5 h-5 text-blue-600" /></div>
          <div><h1 className="text-lg font-bold text-slate-800">GST Reports</h1><p className="text-xs text-slate-500">GSTR-1, GSTR-3B, HSN Summary</p></div>
        </div>
        <div className="flex items-center gap-2">
          <select value={month} onChange={e => setMonth(Number(e.target.value))} className="px-2 py-1.5 text-xs border border-slate-200 rounded-lg focus:ring-1 focus:ring-emerald-400 outline-none">
            {months.map((m, i) => <option key={i} value={i}>{m}</option>)}
          </select>
          <select value={year} onChange={e => setYear(Number(e.target.value))} className="px-2 py-1.5 text-xs border border-slate-200 rounded-lg focus:ring-1 focus:ring-emerald-400 outline-none">
            {years.map(y => <option key={y} value={y}>{y}</option>)}
          </select>
          <button onClick={exportGST} className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 text-white text-xs font-semibold rounded-lg hover:bg-emerald-700">
            <Download className="w-3.5 h-3.5" /> Export Excel
          </button>
        </div>
      </div>

      {/* Notice */}
      <div className="flex items-start gap-2 p-3 bg-blue-50 border border-blue-100 rounded-xl">
        <AlertCircle className="w-4 h-4 text-blue-500 shrink-0 mt-0.5" />
        <p className="text-xs text-blue-700">GST data shown is based on your invoices. Verify with a CA before filing returns.</p>
      </div>

      {/* Tax Summary Cards */}
      {!loading && data && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          {[
            { label: 'Taxable Amount', value: data.totalTaxableAmount || 0 },
            { label: 'CGST', value: data.totalCGST || 0 },
            { label: 'SGST', value: data.totalSGST || 0 },
            { label: 'Total Tax', value: data.totalTaxAmount || 0 },
          ].map(card => (
            <div key={card.label} className="bg-white border border-green-100 rounded-xl p-3">
              <p className="text-xs text-slate-500 mb-1">{card.label}</p>
              <p className="text-base font-bold text-slate-800">₹{card.value.toLocaleString('en-IN', { maximumFractionDigits: 0 })}</p>
            </div>
          ))}
        </div>
      )}

      {/* Tabs */}
      <div className="flex gap-1 p-1 bg-slate-100 rounded-xl w-fit">
        {([['gstr1', 'GSTR-1'], ['gstr3b', 'GSTR-3B'], ['hsn', 'HSN Summary'], ['raw', 'Tax Report']] as [TaxTab, string][]).map(([t, label]) => (
          <button key={t} onClick={() => setTab(t)} className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-all ${tab === t ? 'bg-white text-emerald-700 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}>{label}</button>
        ))}
      </div>

      {loading ? <div className="p-8 text-center text-sm text-slate-400">Loading tax data...</div> : !data ? (
        <div className="p-8 text-center text-sm text-slate-400">No tax data for {months[month]} {year}</div>
      ) : (
        <>
          {/* GSTR-1 Summary */}
          {tab === 'gstr1' && (
            <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
              <div className="px-4 py-3 border-b border-green-100 bg-slate-50"><p className="text-sm font-bold text-slate-700">GSTR-1 Summary — {months[month]} {year}</p></div>
              <div className="p-4 space-y-2 text-xs">
                {[
                  { label: 'B2B Sales (Business to Business)', value: data.gstr1Summary?.b2b || 0 },
                  { label: 'B2C Sales (Business to Consumer)', value: data.gstr1Summary?.b2c || 0 },
                  { label: 'Exports', value: data.gstr1Summary?.exports || 0 },
                  { label: 'Credit Notes', value: data.gstr1Summary?.creditNotes || 0 },
                ].map((row, i) => (
                  <div key={i} className="flex justify-between py-2 border-b border-slate-50 last:border-0">
                    <span className="text-slate-600">{row.label}</span>
                    <span className="font-semibold text-slate-800">₹{row.value.toLocaleString('en-IN')}</span>
                  </div>
                ))}
                <div className="flex justify-between py-2 border-t border-slate-200 font-bold text-sm mt-1">
                  <span>Total Outward Supplies</span>
                  <span className="text-emerald-700">₹{(data.totalTaxableAmount || 0).toLocaleString('en-IN')}</span>
                </div>
              </div>
            </div>
          )}

          {/* GSTR-3B */}
          {tab === 'gstr3b' && (
            <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
              <div className="px-4 py-3 border-b border-green-100 bg-slate-50"><p className="text-sm font-bold text-slate-700">GSTR-3B Summary — {months[month]} {year}</p></div>
              <div className="p-4 space-y-2 text-xs">
                {[
                  { label: '3.1 Outward Taxable Supplies', value: data.gstr3bSummary?.outwardSupplies || data.totalTaxableAmount || 0 },
                  { label: '4. ITC Available (Purchases)', value: data.gstr3bSummary?.itcAvailable || 0 },
                  { label: '5. Net Tax Payable', value: data.gstr3bSummary?.taxPayable || data.totalTaxAmount || 0 },
                ].map((row, i) => (
                  <div key={i} className="flex justify-between py-2.5 border-b border-slate-50 last:border-0">
                    <span className="text-slate-600">{row.label}</span>
                    <span className="font-semibold text-slate-800">₹{row.value.toLocaleString('en-IN')}</span>
                  </div>
                ))}
                <div className="grid grid-cols-3 gap-3 mt-3">
                  {[['CGST', data.totalCGST || 0], ['SGST', data.totalSGST || 0], ['IGST', data.totalIGST || 0]].map(([label, val]) => (
                    <div key={label as string} className="bg-slate-50 rounded-xl p-3 text-center">
                      <p className="text-xs text-slate-500">{label}</p>
                      <p className="text-sm font-bold text-slate-800">₹{(val as number).toLocaleString('en-IN')}</p>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

          {/* HSN Summary */}
          {tab === 'hsn' && (
            <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
              <div className="px-4 py-3 border-b border-green-100 bg-slate-50"><p className="text-sm font-bold text-slate-700">HSN-wise Summary</p></div>
              {!data.hsnSummary || data.hsnSummary.length === 0 ? (
                <div className="p-6 text-center text-xs text-slate-400">No HSN data available. Add HSN codes to your products.</div>
              ) : (
                <table className="w-full text-xs">
                  <thead><tr className="border-b border-slate-100">
                    <th className="text-left px-3 py-2.5 font-semibold text-slate-600">HSN Code</th>
                    <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Description</th>
                    <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Qty</th>
                    <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Taxable Value</th>
                    <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Tax</th>
                  </tr></thead>
                  <tbody>
                    {data.hsnSummary.map((row, i) => (
                      <tr key={i} className="border-b border-slate-50 hover:bg-slate-50">
                        <td className="px-3 py-2.5 font-mono font-semibold text-slate-700">{row.hsn}</td>
                        <td className="px-3 py-2.5 text-slate-600">{row.description}</td>
                        <td className="px-3 py-2.5 text-right">{row.qty}</td>
                        <td className="px-3 py-2.5 text-right font-semibold">₹{row.taxableValue.toLocaleString('en-IN')}</td>
                        <td className="px-3 py-2.5 text-right text-blue-600 font-semibold">₹{row.tax.toLocaleString('en-IN')}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          )}

          {/* Raw Tax Report */}
          {tab === 'raw' && (
            <div className="bg-white border border-green-100 rounded-xl p-4">
              <pre className="text-[10px] text-slate-600 overflow-auto">{JSON.stringify(data, null, 2)}</pre>
            </div>
          )}
        </>
      )}
    </div>
  );
};

