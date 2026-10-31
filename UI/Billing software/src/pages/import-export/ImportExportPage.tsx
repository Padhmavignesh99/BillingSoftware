import React, { useState } from 'react';
import { FileUp, FileDown, Upload, Download, CheckCircle, XCircle, AlertCircle, FileSpreadsheet } from 'lucide-react';
import { api } from '../../services/apiClient';

type Tab = 'export' | 'import';

interface ImportResult {
  success: number;
  failed: number;
  errors: string[];
  warnings: string[];
}

const ExportCard: React.FC<{ title: string; desc: string; onExport: () => void; loading: boolean }> = ({ title, desc, onExport, loading }) => (
  <div className="bg-white border border-green-100 rounded-xl p-4 flex items-center justify-between hover:shadow-md transition-shadow">
    <div className="flex items-center gap-3">
      <div className="w-10 h-10 bg-emerald-50 rounded-xl flex items-center justify-center">
        <FileSpreadsheet className="w-5 h-5 text-emerald-600" />
      </div>
      <div>
        <p className="text-sm font-semibold text-slate-800">{title}</p>
        <p className="text-xs text-slate-500">{desc}</p>
      </div>
    </div>
    <button
      onClick={onExport}
      disabled={loading}
      className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 text-white text-xs font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition-colors"
    >
      <Download className="w-3.5 h-3.5" />
      {loading ? 'Exporting...' : 'Export CSV'}
    </button>
  </div>
);

export const ImportExportPage: React.FC = () => {
  const [tab, setTab] = useState<Tab>('export');
  const [loading, setLoading] = useState<string | null>(null);
  const [importType, setImportType] = useState<'products' | 'customers'>('products');
  const [importing, setImporting] = useState(false);
  const [result, setResult] = useState<ImportResult | null>(null);
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [dragOver, setDragOver] = useState(false);

  const doExport = async (type: string, params?: Record<string, string>) => {
    setLoading(type);
    try {
      let url = `/api/export/${type}`;
      if (params) url += '?' + new URLSearchParams(params).toString();
      const blob = await api.getBlob(url);
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = `${type}-${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(null);
    }
  };

  const downloadTemplate = async () => {
    setLoading('template');
    try {
      const blob = await api.getBlob(`/api/import/template/${importType}`);
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = `${importType}-template.csv`;
      a.click();
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(null);
    }
  };

  const handleFile = async (file: File) => {
    if (!file) return;
    setImporting(true);
    setResult(null);
    try {
      const form = new FormData();
      form.append('file', file);
      const res = await api.postForm(`/api/import/${importType}`, form);
      setResult(res.data);
    } catch (e) {
      console.error(e);
    } finally {
      setImporting(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    const file = e.dataTransfer.files[0];
    if (file) handleFile(file);
  };

  return (
    <div className="p-4 md:p-6 space-y-5 max-w-4xl mx-auto">
      {/* Header */}
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 bg-emerald-100 rounded-xl flex items-center justify-center">
          <FileUp className="w-5 h-5 text-emerald-600" />
        </div>
        <div>
          <h1 className="text-lg font-bold text-slate-800">Import / Export</h1>
          <p className="text-xs text-slate-500">Bulk data operations via CSV</p>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 p-1 bg-slate-100 rounded-xl w-fit">
        {(['export', 'import'] as Tab[]).map(t => (
          <button
            key={t}
            onClick={() => { setTab(t); setResult(null); }}
            className={`px-4 py-1.5 text-xs font-semibold rounded-lg capitalize transition-all ${tab === t ? 'bg-white text-emerald-700 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}
          >
            {t === 'export' ? <><FileDown className="w-3.5 h-3.5 inline mr-1" />Export</> : <><FileUp className="w-3.5 h-3.5 inline mr-1" />Import</>}
          </button>
        ))}
      </div>

      {/* Export Tab */}
      {tab === 'export' && (
        <div className="space-y-3">
          <p className="text-xs text-slate-500 font-medium uppercase tracking-wide">Select data to export</p>
          <ExportCard title="Products" desc="All active products with pricing and stock" onExport={() => doExport('products')} loading={loading === 'products'} />
          <ExportCard title="Customers" desc="All customers with contact and credit info" onExport={() => doExport('customers')} loading={loading === 'customers'} />
          <ExportCard title="Suppliers" desc="All suppliers with contact and bank info" onExport={() => doExport('suppliers')} loading={loading === 'suppliers'} />
          <div className="bg-white border border-green-100 rounded-xl p-4">
            <div className="flex items-center gap-3 mb-3">
              <div className="w-10 h-10 bg-emerald-50 rounded-xl flex items-center justify-center">
                <FileSpreadsheet className="w-5 h-5 text-emerald-600" />
              </div>
              <div>
                <p className="text-sm font-semibold text-slate-800">Invoices</p>
                <p className="text-xs text-slate-500">Export invoices within date range</p>
              </div>
            </div>
            <div className="flex items-center gap-2 flex-wrap">
              <input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} className="border border-slate-200 rounded-lg px-2 py-1.5 text-xs text-slate-700 focus:ring-1 focus:ring-emerald-400 outline-none" />
              <span className="text-xs text-slate-400">to</span>
              <input type="date" value={toDate} onChange={e => setToDate(e.target.value)} className="border border-slate-200 rounded-lg px-2 py-1.5 text-xs text-slate-700 focus:ring-1 focus:ring-emerald-400 outline-none" />
              <button
                onClick={() => doExport('invoices', fromDate || toDate ? { from: fromDate, to: toDate } : {})}
                disabled={loading === 'invoices'}
                className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 text-white text-xs font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition-colors"
              >
                <Download className="w-3.5 h-3.5" />
                {loading === 'invoices' ? 'Exporting...' : 'Export CSV'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Import Tab */}
      {tab === 'import' && (
        <div className="space-y-4">
          {/* Import Type Selector */}
          <div className="flex gap-2">
            {(['products', 'customers'] as const).map(t => (
              <button
                key={t}
                onClick={() => { setImportType(t); setResult(null); }}
                className={`px-4 py-1.5 text-xs font-semibold rounded-lg capitalize transition-all border ${importType === t ? 'bg-emerald-600 text-white border-emerald-600' : 'border-slate-200 text-slate-600 hover:border-emerald-300'}`}
              >
                {t}
              </button>
            ))}
          </div>

          {/* Template Download */}
          <div className="flex items-center gap-2 p-3 bg-blue-50 border border-blue-100 rounded-xl">
            <AlertCircle className="w-4 h-4 text-blue-500 shrink-0" />
            <p className="text-xs text-blue-700 flex-1">Download the template CSV to ensure correct column format</p>
            <button
              onClick={downloadTemplate}
              disabled={loading === 'template'}
              className="flex items-center gap-1 px-3 py-1.5 bg-white border border-blue-200 text-blue-700 text-xs font-medium rounded-lg hover:bg-blue-50 transition-colors"
            >
              <Download className="w-3 h-3" />
              Template
            </button>
          </div>

          {/* Drop Zone */}
          <div
            onDragOver={e => { e.preventDefault(); setDragOver(true); }}
            onDragLeave={() => setDragOver(false)}
            onDrop={handleDrop}
            className={`border-2 border-dashed rounded-xl p-8 text-center transition-all ${dragOver ? 'border-emerald-400 bg-emerald-50' : 'border-slate-200 hover:border-emerald-300 hover:bg-emerald-50/30'}`}
          >
            <FileUp className="w-10 h-10 text-slate-300 mx-auto mb-3" />
            <p className="text-sm font-semibold text-slate-700 mb-1">Drop your CSV file here</p>
            <p className="text-xs text-slate-400 mb-4">or click to browse</p>
            <label className="cursor-pointer">
              <input type="file" accept=".csv" className="hidden" onChange={e => { const f = e.target.files?.[0]; if (f) handleFile(f); }} />
              <span className="px-4 py-2 bg-emerald-600 text-white text-xs font-semibold rounded-lg hover:bg-emerald-700 transition-colors">
                {importing ? 'Importing...' : 'Choose File'}
              </span>
            </label>
          </div>

          {/* Results */}
          {result && (
            <div className="bg-white border border-green-100 rounded-xl p-4 space-y-3">
              <p className="text-sm font-semibold text-slate-800">Import Results</p>
              <div className="flex gap-4">
                <div className="flex items-center gap-2 text-emerald-700">
                  <CheckCircle className="w-4 h-4" />
                  <span className="text-sm font-bold">{result.success}</span>
                  <span className="text-xs">imported</span>
                </div>
                <div className="flex items-center gap-2 text-rose-600">
                  <XCircle className="w-4 h-4" />
                  <span className="text-sm font-bold">{result.failed}</span>
                  <span className="text-xs">failed</span>
                </div>
              </div>
              {result.errors.length > 0 && (
                <div className="bg-rose-50 border border-rose-100 rounded-lg p-3 max-h-40 overflow-y-auto">
                  {result.errors.map((err, i) => (
                    <p key={i} className="text-xs text-rose-600">{err}</p>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

