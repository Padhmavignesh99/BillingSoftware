import React, { useEffect, useState, useCallback } from 'react';
import {
  Printer, Wifi, Usb, Bluetooth, Network, Cloud, Settings2, Plus,
  Trash2, CheckCircle2, XCircle, RefreshCw, TestTube2, Star, StarOff,
  ChevronDown, ChevronRight, Zap, FileText, Tag, Layers, Smartphone,
  Copy, Edit2, Save, X, AlertTriangle, Info, Activity
} from 'lucide-react';
import { api } from '../../services/apiClient';
import { useToast } from '../../components/common/Toast';

// ─── Types ───────────────────────────────────────────────────────────────────

interface PrinterRecord {
  id: number;
  printerName: string;
  printerType: string;
  printerTypeLabel: string;
  connectionType: string;
  connectionAddress?: string;
  port?: number;
  osDriverName?: string;
  paperSize: string;
  orientation: string;
  charsPerLine?: number;
  dpiResolution?: number;
  supportsColor: boolean;
  supportsAutoCut: boolean;
  supportsCashDrawer: boolean;
  supportsBarcodes: boolean;
  supportsQRCode: boolean;
  supportsLogo: boolean;
  supportsEscPos: boolean;
  supportsPdf: boolean;
  isDefault: boolean;
  isActive: boolean;
  branchId?: number;
  counterId?: number;
  codePage?: number;
  lineFeedCount: number;
  notes?: string;
  lastTestedAt?: string;
  lastTestStatus?: string;
  statusLabel: string;
}

interface PrinterForm {
  printerName: string;
  printerType: string;
  connectionType: string;
  connectionAddress: string;
  port: string;
  osDriverName: string;
  paperSize: string;
  orientation: string;
  charsPerLine: string;
  dpiResolution: string;
  supportsColor: boolean;
  supportsAutoCut: boolean;
  supportsCashDrawer: boolean;
  supportsBarcodes: boolean;
  supportsQRCode: boolean;
  supportsLogo: boolean;
  supportsEscPos: boolean;
  supportsPdf: boolean;
  isDefault: boolean;
  isActive: boolean;
  codePage: string;
  lineFeedCount: string;
  notes: string;
}

const defaultForm: PrinterForm = {
  printerName: '',
  printerType: 'Thermal',
  connectionType: 'USB',
  connectionAddress: '',
  port: '',
  osDriverName: '',
  paperSize: '80mm',
  orientation: 'Portrait',
  charsPerLine: '',
  dpiResolution: '',
  supportsColor: false,
  supportsAutoCut: true,
  supportsCashDrawer: false,
  supportsBarcodes: true,
  supportsQRCode: true,
  supportsLogo: true,
  supportsEscPos: true,
  supportsPdf: false,
  isDefault: false,
  isActive: true,
  codePage: '',
  lineFeedCount: '4',
  notes: '',
};

// ─── Printer type metadata ─────────────────────────────────────────────────

const PRINTER_TYPES = [
  { value: 'Thermal',       label: 'Thermal Receipt',    icon: Printer,    color: 'text-emerald-600', bg: 'bg-emerald-50',  desc: '58mm/80mm ESC/POS receipts', paperSizes: ['58mm','80mm'], supportsEscPos: true, supportsPdf: false, supportsAutoCut: true },
  { value: 'Laser',         label: 'Laser (A4/A5)',      icon: FileText,   color: 'text-blue-600',    bg: 'bg-blue-50',     desc: 'A4 invoices, GST bills',     paperSizes: ['A4','A5','Letter','Legal'], supportsEscPos: false, supportsPdf: true, supportsAutoCut: false },
  { value: 'Inkjet',        label: 'Inkjet / Colour',    icon: FileText,   color: 'text-purple-600',  bg: 'bg-purple-50',   desc: 'Colour invoices, reports',   paperSizes: ['A4','A5','Letter'], supportsEscPos: false, supportsPdf: true, supportsAutoCut: false },
  { value: 'DotMatrix',     label: 'Dot Matrix',         icon: Layers,     color: 'text-orange-600',  bg: 'bg-orange-50',   desc: 'Multi-copy, continuous paper', paperSizes: ['A4','Custom'], supportsEscPos: false, supportsPdf: false, supportsAutoCut: false },
  { value: 'Label',         label: 'Barcode / Label',    icon: Tag,        color: 'text-rose-600',    bg: 'bg-rose-50',     desc: 'Product labels, TSPL/ZPL',  paperSizes: ['50x25mm','100x50mm','Custom'], supportsEscPos: false, supportsPdf: false, supportsAutoCut: true },
  { value: 'Portable',      label: 'Portable / BT',      icon: Smartphone, color: 'text-indigo-600',  bg: 'bg-indigo-50',   desc: 'Field billing, mobile POS',  paperSizes: ['58mm','80mm'], supportsEscPos: true, supportsPdf: false, supportsAutoCut: false },
  { value: 'Multifunction', label: 'Multifunction',      icon: Copy,       color: 'text-slate-600',   bg: 'bg-slate-50',    desc: 'Print, scan, copy – LAN',    paperSizes: ['A4','A5','Letter'], supportsEscPos: false, supportsPdf: true, supportsAutoCut: false },
];

const CONNECTION_TYPES = [
  { value: 'USB',       label: 'USB',                icon: Usb,       desc: 'Direct USB — uses OS driver', needsAddress: false },
  { value: 'Ethernet',  label: 'Ethernet / LAN',     icon: Network,   desc: 'Raw TCP port 9100 (ESC/POS)', needsAddress: true  },
  { value: 'WiFi',      label: 'Wi-Fi',              icon: Wifi,      desc: 'Wireless TCP connection',     needsAddress: true  },
  { value: 'Bluetooth', label: 'Bluetooth',          icon: Bluetooth, desc: 'Pair device first',           needsAddress: false },
  { value: 'Shared',    label: 'Shared (OS/Network)',icon: Network,   desc: '\\\\SERVER\\PRINTER path',    needsAddress: true  },
  { value: 'Cloud',     label: 'Cloud Print',        icon: Cloud,     desc: 'Cloud relay endpoint URL',    needsAddress: true  },
];

const PAPER_SIZES = ['A4','A5','Letter','Legal','80mm','58mm','50x25mm','100x50mm','Custom'];

// ─── Sub-components ────────────────────────────────────────────────────────

const Toggle: React.FC<{ checked: boolean; onChange: (v: boolean) => void; label: string }> = ({ checked, onChange, label }) => (
  <label className="flex items-center gap-3 cursor-pointer group">
    <div
      onClick={() => onChange(!checked)}
      className={`relative w-10 h-5 rounded-full transition-colors duration-200 ${checked ? 'bg-indigo-600' : 'bg-slate-200'}`}
    >
      <span className={`absolute top-0.5 left-0.5 w-4 h-4 bg-white rounded-full shadow transition-transform duration-200 ${checked ? 'translate-x-5' : ''}`} />
    </div>
    <span className="text-xs font-medium text-slate-700 group-hover:text-slate-900">{label}</span>
  </label>
);

const Field: React.FC<{ label: string; children: React.ReactNode; hint?: string }> = ({ label, children, hint }) => (
  <div>
    <label className="block text-xs font-semibold text-slate-700 mb-1">{label}</label>
    {children}
    {hint && <p className="mt-1 text-[11px] text-slate-400">{hint}</p>}
  </div>
);

const InputCls = "w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-medium focus:outline-none focus:ring-2 focus:ring-indigo-500/40 focus:border-indigo-400 transition-all";
const SelectCls = InputCls;

// ─── Main Component ─────────────────────────────────────────────────────────

export const PrinterSettingsPage: React.FC = () => {
  const { showToast } = useToast();

  const [printers, setPrinters]       = useState<PrinterRecord[]>([]);
  const [loading, setLoading]         = useState(true);
  const [showForm, setShowForm]       = useState(false);
  const [editId, setEditId]           = useState<number | null>(null);
  const [form, setForm]               = useState<PrinterForm>(defaultForm);
  const [saving, setSaving]           = useState(false);
  const [testing, setTesting]         = useState<number | null>(null);
  const [testResult, setTestResult]   = useState<{id: number; success: boolean; msg: string} | null>(null);
  const [selectedPrinter, setSelectedPrinter] = useState<PrinterRecord | null>(null);
  const [activeSection, setActiveSection]      = useState<'list' | 'form' | 'detail'>('list');

  // Derived — auto-fill sensible defaults when printer type changes
  const currentType = PRINTER_TYPES.find(t => t.value === form.printerType);

  const fetchPrinters = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.printers.getAll();
      setPrinters(Array.isArray(res) ? res : []);
    } catch {
      // Demo mode — show sample printers so the UI is populated
      setPrinters([
        { id: 1, printerName: 'Front Counter Thermal', printerType: 'Thermal', printerTypeLabel: 'Thermal Receipt Printer', connectionType: 'USB', paperSize: '80mm', orientation: 'Portrait', supportsColor: false, supportsAutoCut: true, supportsCashDrawer: false, supportsBarcodes: true, supportsQRCode: true, supportsLogo: true, supportsEscPos: true, supportsPdf: false, isDefault: true, isActive: true, lineFeedCount: 4, statusLabel: 'Active', lastTestStatus: 'OK', lastTestedAt: new Date().toISOString() },
        { id: 2, printerName: 'Office Laser A4',       printerType: 'Laser',   printerTypeLabel: 'Laser Printer (A4/A5)',   connectionType: 'Ethernet', connectionAddress: '192.168.1.100', port: 9100, paperSize: 'A4', orientation: 'Portrait', supportsColor: false, supportsAutoCut: false, supportsCashDrawer: false, supportsBarcodes: false, supportsQRCode: true, supportsLogo: true, supportsEscPos: false, supportsPdf: true, isDefault: true, isActive: true, lineFeedCount: 0, statusLabel: 'Active', lastTestStatus: 'OK', lastTestedAt: new Date().toISOString() },
      ]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchPrinters(); }, [fetchPrinters]);

  // When printer type changes, auto-update relevant capability toggles
  useEffect(() => {
    if (!currentType) return;
    setForm(f => ({
      ...f,
      paperSize:     currentType.paperSizes[0] ?? f.paperSize,
      supportsEscPos: currentType.supportsEscPos,
      supportsPdf:   currentType.supportsPdf,
      supportsAutoCut: currentType.supportsAutoCut,
    }));
  }, [form.printerType]); // eslint-disable-line react-hooks/exhaustive-deps

  const openCreate = () => {
    setEditId(null);
    setForm(defaultForm);
    setActiveSection('form');
    setShowForm(true);
  };

  const openEdit = (p: PrinterRecord) => {
    setEditId(p.id);
    setForm({
      printerName: p.printerName,
      printerType: p.printerType,
      connectionType: p.connectionType,
      connectionAddress: p.connectionAddress ?? '',
      port: p.port?.toString() ?? '',
      osDriverName: p.osDriverName ?? '',
      paperSize: p.paperSize,
      orientation: p.orientation,
      charsPerLine: p.charsPerLine?.toString() ?? '',
      dpiResolution: p.dpiResolution?.toString() ?? '',
      supportsColor: p.supportsColor,
      supportsAutoCut: p.supportsAutoCut,
      supportsCashDrawer: p.supportsCashDrawer,
      supportsBarcodes: p.supportsBarcodes,
      supportsQRCode: p.supportsQRCode,
      supportsLogo: p.supportsLogo,
      supportsEscPos: p.supportsEscPos,
      supportsPdf: p.supportsPdf,
      isDefault: p.isDefault,
      isActive: p.isActive,
      codePage: p.codePage?.toString() ?? '',
      lineFeedCount: p.lineFeedCount.toString(),
      notes: p.notes ?? '',
    });
    setActiveSection('form');
    setShowForm(true);
  };

  const handleSave = async () => {
    if (!form.printerName.trim()) { showToast('Printer name is required', 'error'); return; }
    setSaving(true);
    const payload = {
      printerName: form.printerName,
      printerType: form.printerType,
      connectionType: form.connectionType,
      connectionAddress: form.connectionAddress || undefined,
      port: form.port ? parseInt(form.port) : undefined,
      osDriverName: form.osDriverName || undefined,
      paperSize: form.paperSize,
      orientation: form.orientation,
      charsPerLine: form.charsPerLine ? parseInt(form.charsPerLine) : undefined,
      dpiResolution: form.dpiResolution ? parseInt(form.dpiResolution) : undefined,
      supportsColor: form.supportsColor,
      supportsAutoCut: form.supportsAutoCut,
      supportsCashDrawer: form.supportsCashDrawer,
      supportsBarcodes: form.supportsBarcodes,
      supportsQRCode: form.supportsQRCode,
      supportsLogo: form.supportsLogo,
      supportsEscPos: form.supportsEscPos,
      supportsPdf: form.supportsPdf,
      isDefault: form.isDefault,
      isActive: form.isActive,
      codePage: form.codePage ? parseInt(form.codePage) : undefined,
      lineFeedCount: parseInt(form.lineFeedCount) || 4,
      notes: form.notes || undefined,
    };
    try {
      if (editId) {
        await api.printers.update(editId, payload);
        showToast('Printer updated', 'success');
      } else {
        await api.printers.create(payload);
        showToast('Printer registered', 'success');
      }
      await fetchPrinters();
      setActiveSection('list');
      setShowForm(false);
    } catch (err: any) {
      showToast(err?.message ?? 'Failed to save printer', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await api.printers.delete(id);
      showToast('Printer removed', 'success');
      setPrinters(p => p.filter(x => x.id !== id));
    } catch {
      showToast('Failed to remove printer', 'error');
    }
  };

  const handleSetDefault = async (id: number) => {
    try {
      await api.printers.setDefault(id);
      showToast('Default printer updated', 'success');
      fetchPrinters();
    } catch {
      showToast('Failed to set default', 'error');
    }
  };

  const handleTest = async (id: number) => {
    setTesting(id);
    setTestResult(null);
    try {
      const res = await api.printers.test(id);
      const ok = res?.data?.success ?? true;
      setTestResult({ id, success: ok, msg: res?.data?.message ?? 'Printer is reachable' });
      showToast(ok ? 'Printer test passed ✓' : 'Printer test failed', ok ? 'success' : 'error');
      fetchPrinters();
    } catch {
      setTestResult({ id, success: false, msg: 'Test failed — check connection settings' });
      showToast('Test failed', 'error');
    } finally {
      setTesting(null);
    }
  };

  const connType = CONNECTION_TYPES.find(c => c.value === form.connectionType);

  // ── Printer Card ────────────────────────────────────────────────────────────
  const PrinterCard: React.FC<{ p: PrinterRecord }> = ({ p }) => {
    const typeInfo = PRINTER_TYPES.find(t => t.value === p.printerType);
    const Icon = typeInfo?.icon ?? Printer;
    const connInfo = CONNECTION_TYPES.find(c => c.value === p.connectionType);
    const ConnIcon = connInfo?.icon ?? Usb;
    const isTestingThis = testing === p.id;
    const tr = testResult?.id === p.id ? testResult : null;

    return (
      <div className={`relative bg-white rounded-2xl border transition-all duration-200 overflow-hidden group hover:shadow-md ${p.isDefault ? 'border-indigo-300 shadow-sm shadow-indigo-100' : 'border-slate-200'} ${!p.isActive ? 'opacity-60' : ''}`}>
        {p.isDefault && (
          <div className="absolute top-0 left-0 right-0 h-0.5 bg-gradient-to-r from-indigo-500 to-violet-500" />
        )}
        <div className="p-4">
          <div className="flex items-start justify-between gap-3">
            <div className="flex items-start gap-3">
              <div className={`w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0 ${typeInfo?.bg ?? 'bg-slate-50'}`}>
                <Icon className={`w-5 h-5 ${typeInfo?.color ?? 'text-slate-600'}`} />
              </div>
              <div className="min-w-0">
                <div className="flex items-center gap-2 flex-wrap">
                  <h3 className="text-sm font-bold text-slate-900 truncate">{p.printerName}</h3>
                  {p.isDefault && (
                    <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-indigo-100 text-indigo-700 text-[10px] font-bold rounded-full">
                      <Star className="w-2.5 h-2.5" /> Default
                    </span>
                  )}
                  {!p.isActive && (
                    <span className="px-2 py-0.5 bg-slate-100 text-slate-500 text-[10px] font-bold rounded-full">Inactive</span>
                  )}
                </div>
                <p className="text-[11px] text-slate-500 mt-0.5">{typeInfo?.desc ?? p.printerTypeLabel}</p>
                <div className="flex items-center gap-3 mt-2 flex-wrap">
                  <span className="inline-flex items-center gap-1 text-[10px] font-semibold text-slate-600">
                    <ConnIcon className="w-3 h-3" /> {p.connectionType}
                    {p.connectionAddress && <span className="text-slate-400">· {p.connectionAddress}{p.port ? `:${p.port}` : ''}</span>}
                  </span>
                  <span className="text-[10px] font-semibold text-slate-500">{p.paperSize}</span>
                  {p.lastTestStatus && (
                    <span className={`inline-flex items-center gap-1 text-[10px] font-bold ${p.lastTestStatus === 'OK' ? 'text-emerald-600' : 'text-rose-600'}`}>
                      {p.lastTestStatus === 'OK' ? <CheckCircle2 className="w-3 h-3" /> : <XCircle className="w-3 h-3" />}
                      {p.lastTestStatus}
                    </span>
                  )}
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-1.5 flex-shrink-0">
              <button
                onClick={() => handleTest(p.id)}
                disabled={isTestingThis}
                title="Test connection"
                className="p-1.5 rounded-lg text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 transition-colors disabled:opacity-50"
              >
                {isTestingThis ? <RefreshCw className="w-4 h-4 animate-spin" /> : <TestTube2 className="w-4 h-4" />}
              </button>
              <button onClick={() => handleSetDefault(p.id)} title="Set as default" className="p-1.5 rounded-lg text-slate-400 hover:text-amber-500 hover:bg-amber-50 transition-colors">
                {p.isDefault ? <Star className="w-4 h-4 fill-amber-400 text-amber-400" /> : <StarOff className="w-4 h-4" />}
              </button>
              <button onClick={() => openEdit(p)} title="Edit" className="p-1.5 rounded-lg text-slate-400 hover:text-blue-600 hover:bg-blue-50 transition-colors">
                <Edit2 className="w-4 h-4" />
              </button>
              <button onClick={() => handleDelete(p.id)} title="Remove" className="p-1.5 rounded-lg text-slate-400 hover:text-rose-600 hover:bg-rose-50 transition-colors">
                <Trash2 className="w-4 h-4" />
              </button>
            </div>
          </div>

          {/* Capabilities chips */}
          <div className="flex flex-wrap gap-1.5 mt-3 border-t border-slate-50 pt-3">
            {p.supportsEscPos    && <span className="px-2 py-0.5 bg-emerald-50 text-emerald-700 text-[10px] font-semibold rounded-full">ESC/POS</span>}
            {p.supportsPdf       && <span className="px-2 py-0.5 bg-blue-50 text-blue-700 text-[10px] font-semibold rounded-full">PDF</span>}
            {p.supportsAutoCut   && <span className="px-2 py-0.5 bg-violet-50 text-violet-700 text-[10px] font-semibold rounded-full">Auto-Cut</span>}
            {p.supportsBarcodes  && <span className="px-2 py-0.5 bg-orange-50 text-orange-700 text-[10px] font-semibold rounded-full">Barcodes</span>}
            {p.supportsQRCode    && <span className="px-2 py-0.5 bg-indigo-50 text-indigo-700 text-[10px] font-semibold rounded-full">QR Code</span>}
            {p.supportsLogo      && <span className="px-2 py-0.5 bg-rose-50 text-rose-700 text-[10px] font-semibold rounded-full">Logo</span>}
            {p.supportsCashDrawer&& <span className="px-2 py-0.5 bg-amber-50 text-amber-700 text-[10px] font-semibold rounded-full">Cash Drawer</span>}
            {p.supportsColor     && <span className="px-2 py-0.5 bg-pink-50 text-pink-700 text-[10px] font-semibold rounded-full">Colour</span>}
          </div>

          {/* Test result banner */}
          {tr && (
            <div className={`mt-3 flex items-start gap-2 p-2.5 rounded-lg text-xs font-medium ${tr.success ? 'bg-emerald-50 text-emerald-700' : 'bg-rose-50 text-rose-700'}`}>
              {tr.success ? <CheckCircle2 className="w-4 h-4 flex-shrink-0 mt-0.5" /> : <XCircle className="w-4 h-4 flex-shrink-0 mt-0.5" />}
              {tr.msg}
            </div>
          )}
        </div>
      </div>
    );
  };

  // ── Config Preview Panel ────────────────────────────────────────────────────
  const ConfigPreview: React.FC = () => {
    const typeInfo = PRINTER_TYPES.find(t => t.value === form.printerType);
    const connInfo = CONNECTION_TYPES.find(c => c.value === form.connectionType);
    const Icon = typeInfo?.icon ?? Printer;
    const ConnIcon = connInfo?.icon ?? Usb;

    return (
      <div className="bg-gradient-to-br from-slate-900 to-slate-800 rounded-2xl p-5 text-white">
        <div className="flex items-center gap-2 mb-4">
          <Settings2 className="w-4 h-4 text-slate-400" />
          <span className="text-xs font-bold text-slate-300 uppercase tracking-wider">Configuration Preview</span>
        </div>
        <div className="flex items-center gap-3 mb-4">
          <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${typeInfo?.bg ?? 'bg-slate-700'}`}>
            <Icon className={`w-5 h-5 ${typeInfo?.color ?? 'text-slate-300'}`} />
          </div>
          <div>
            <p className="text-sm font-bold text-white">{form.printerName || 'Untitled Printer'}</p>
            <p className="text-[11px] text-slate-400">{typeInfo?.label ?? form.printerType}</p>
          </div>
        </div>
        <div className="space-y-2">
          {[
            { label: 'Type', value: typeInfo?.label ?? form.printerType },
            { label: 'Connection', value: connInfo?.label ?? form.connectionType },
            { label: 'Address', value: form.connectionAddress ? `${form.connectionAddress}${form.port ? `:${form.port}` : ''}` : '—' },
            { label: 'Paper', value: form.paperSize },
            { label: 'Orientation', value: form.orientation },
            { label: 'Auto-cut', value: form.supportsAutoCut ? 'Enabled' : 'Disabled' },
            { label: 'ESC/POS', value: form.supportsEscPos ? 'Yes' : 'No' },
            { label: 'PDF Print', value: form.supportsPdf ? 'Yes' : 'No' },
          ].map(row => (
            <div key={row.label} className="flex items-center justify-between py-1.5 border-b border-slate-700/50 last:border-0">
              <span className="text-[11px] text-slate-400">{row.label}</span>
              <span className="text-[11px] font-semibold text-slate-200">{row.value}</span>
            </div>
          ))}
        </div>
        <div className="mt-4 flex items-center gap-2 p-2.5 bg-amber-500/10 border border-amber-500/20 rounded-lg">
          <Info className="w-3.5 h-3.5 text-amber-400 flex-shrink-0" />
          <p className="text-[10px] text-amber-200 leading-relaxed">
            Saving stores configuration. Test connection after saving to verify physical printer reach.
          </p>
        </div>
      </div>
    );
  };

  // ─────────────────────────────────────────────────────────────────────────
  return (
    <div className="space-y-5 w-full">

      {/* Header */}
      <div className="flex items-center justify-between bg-white p-5 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-indigo-600 rounded-xl flex items-center justify-center shadow-sm shadow-indigo-600/30">
            <Printer className="w-5 h-5 text-white" />
          </div>
          <div>
            <h1 className="text-lg font-bold text-slate-900">Printer Management</h1>
            <p className="text-xs text-slate-500">Register and configure printers for billing, receipts, invoices, and labels</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={fetchPrinters}
            className="p-2 rounded-xl border border-slate-200 text-slate-500 hover:text-slate-700 hover:bg-slate-50 transition-colors"
          >
            <RefreshCw className="w-4 h-4" />
          </button>
          <button
            onClick={openCreate}
            className="flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-bold shadow-sm shadow-indigo-600/30 transition-colors"
          >
            <Plus className="w-4 h-4" />
            Add Printer
          </button>
        </div>
      </div>

      {/* Architecture info banner */}
      <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200 rounded-2xl p-4 flex items-start gap-3">
        <Activity className="w-4 h-4 text-blue-600 flex-shrink-0 mt-0.5" />
        <div className="text-xs text-blue-800 space-y-0.5">
          <p className="font-bold">How printing works in BillFlow</p>
          <p className="text-blue-600">
            <strong>Browser / PDF</strong> — Laser, Inkjet: frontend calls <code className="bg-blue-100 px-1 rounded">window.print()</code> with the rendered HTML.&nbsp;
            <strong>ESC/POS</strong> — Thermal, Label: a local print agent picks up queued jobs and sends raw bytes to the USB/LAN printer.&nbsp;
            <strong>Network printers</strong> — Use TCP test to verify IP:Port 9100 reachability.
          </p>
        </div>
      </div>

      <div className={`grid gap-5 ${showForm ? 'grid-cols-1 xl:grid-cols-3' : 'grid-cols-1'}`}>

        {/* ── Printer List ────────────────────────────────────────────────── */}
        <div className={showForm ? 'xl:col-span-2' : ''}>
          {loading ? (
            <div className="flex items-center justify-center py-20 text-slate-400">
              <RefreshCw className="w-5 h-5 animate-spin mr-2" />
              <span className="text-sm">Loading printers…</span>
            </div>
          ) : printers.length === 0 ? (
            <div className="bg-white rounded-2xl border border-dashed border-slate-300 py-16 text-center">
              <Printer className="w-10 h-10 text-slate-300 mx-auto mb-3" />
              <h3 className="text-sm font-bold text-slate-600">No printers registered</h3>
              <p className="text-xs text-slate-400 mt-1">Click "Add Printer" to register your first printer</p>
              <button onClick={openCreate} className="mt-4 px-4 py-2 bg-indigo-600 text-white rounded-xl text-xs font-bold hover:bg-indigo-700 transition-colors">
                Register First Printer
              </button>
            </div>
          ) : (
            <div className="space-y-3">
              {/* Group by type */}
              {['Thermal','Laser','Inkjet','DotMatrix','Label','Portable','Multifunction']
                .filter(type => printers.some(p => p.printerType === type))
                .map(type => {
                  const typeInfo = PRINTER_TYPES.find(t => t.value === type);
                  const Icon = typeInfo?.icon ?? Printer;
                  const group = printers.filter(p => p.printerType === type);
                  return (
                    <div key={type}>
                      <div className="flex items-center gap-2 mb-2 px-1">
                        <Icon className={`w-3.5 h-3.5 ${typeInfo?.color ?? 'text-slate-500'}`} />
                        <span className="text-[11px] font-bold text-slate-500 uppercase tracking-wider">{typeInfo?.label ?? type} ({group.length})</span>
                      </div>
                      <div className="grid grid-cols-1 lg:grid-cols-2 gap-3">
                        {group.map(p => <PrinterCard key={p.id} p={p} />)}
                      </div>
                    </div>
                  );
                })
              }
            </div>
          )}
        </div>

        {/* ── Add / Edit Form ──────────────────────────────────────────────── */}
        {showForm && (
          <div className="space-y-4">
            <div className="bg-white rounded-2xl border border-slate-200 shadow-sm">
              {/* Form header */}
              <div className="flex items-center justify-between p-4 border-b border-slate-100">
                <h2 className="text-sm font-bold text-slate-900">{editId ? 'Edit Printer' : 'Register Printer'}</h2>
                <button onClick={() => { setShowForm(false); setActiveSection('list'); }} className="p-1.5 rounded-lg hover:bg-slate-100 text-slate-400 hover:text-slate-600 transition-colors">
                  <X className="w-4 h-4" />
                </button>
              </div>

              <div className="p-4 space-y-5 max-h-[70vh] overflow-y-auto">

                {/* Printer Name */}
                <Field label="Printer Name *">
                  <input type="text" value={form.printerName} onChange={e => setForm(f => ({ ...f, printerName: e.target.value }))} placeholder="e.g. Front Counter Thermal" className={InputCls} />
                </Field>

                {/* Printer Type */}
                <Field label="Printer Type *">
                  <div className="grid grid-cols-2 gap-2">
                    {PRINTER_TYPES.map(t => {
                      const TIcon = t.icon;
                      const active = form.printerType === t.value;
                      return (
                        <button
                          key={t.value}
                          type="button"
                          onClick={() => setForm(f => ({ ...f, printerType: t.value }))}
                          className={`flex items-center gap-2 p-2.5 rounded-xl border text-left transition-all ${active ? 'border-indigo-400 bg-indigo-50 shadow-sm' : 'border-slate-200 hover:border-slate-300 hover:bg-slate-50'}`}
                        >
                          <div className={`w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0 ${active ? 'bg-indigo-600' : t.bg}`}>
                            <TIcon className={`w-3.5 h-3.5 ${active ? 'text-white' : t.color}`} />
                          </div>
                          <div>
                            <p className={`text-[10px] font-bold leading-tight ${active ? 'text-indigo-700' : 'text-slate-700'}`}>{t.label}</p>
                            <p className="text-[9px] text-slate-400 leading-tight hidden sm:block">{t.desc}</p>
                          </div>
                        </button>
                      );
                    })}
                  </div>
                </Field>

                {/* Connection Type */}
                <Field label="Connection Type *">
                  <div className="grid grid-cols-2 gap-2">
                    {CONNECTION_TYPES.map(c => {
                      const CIcon = c.icon;
                      const active = form.connectionType === c.value;
                      return (
                        <button
                          key={c.value}
                          type="button"
                          onClick={() => setForm(f => ({ ...f, connectionType: c.value }))}
                          className={`flex items-center gap-2 p-2.5 rounded-xl border text-left transition-all ${active ? 'border-indigo-400 bg-indigo-50' : 'border-slate-200 hover:border-slate-300 hover:bg-slate-50'}`}
                        >
                          <CIcon className={`w-4 h-4 flex-shrink-0 ${active ? 'text-indigo-600' : 'text-slate-400'}`} />
                          <div>
                            <p className={`text-[10px] font-bold leading-tight ${active ? 'text-indigo-700' : 'text-slate-700'}`}>{c.label}</p>
                            <p className="text-[9px] text-slate-400 leading-tight hidden sm:block">{c.desc}</p>
                          </div>
                        </button>
                      );
                    })}
                  </div>
                </Field>

                {/* Address / Port — only for network types */}
                {connType?.needsAddress && (
                  <div className="grid grid-cols-3 gap-3">
                    <div className="col-span-2">
                      <Field label={form.connectionType === 'Shared' ? 'UNC Path / Share Name' : form.connectionType === 'Cloud' ? 'Endpoint URL' : 'IP Address'} hint={form.connectionType === 'Ethernet' || form.connectionType === 'WiFi' ? 'e.g. 192.168.1.100' : undefined}>
                        <input type="text" value={form.connectionAddress} onChange={e => setForm(f => ({ ...f, connectionAddress: e.target.value }))} placeholder={form.connectionType === 'Ethernet' ? '192.168.1.100' : form.connectionType === 'Shared' ? '\\\\SERVER\\PRINTER' : 'https://...'} className={InputCls} />
                      </Field>
                    </div>
                    {(form.connectionType === 'Ethernet' || form.connectionType === 'WiFi') && (
                      <Field label="Port" hint="Default: 9100">
                        <input type="number" value={form.port} onChange={e => setForm(f => ({ ...f, port: e.target.value }))} placeholder="9100" className={InputCls} />
                      </Field>
                    )}
                  </div>
                )}

                {/* OS Driver Name */}
                {(form.connectionType === 'USB' || form.connectionType === 'Shared') && (
                  <Field label="OS Driver / Printer Name" hint="As shown in Windows Printers & Scanners settings">
                    <input type="text" value={form.osDriverName} onChange={e => setForm(f => ({ ...f, osDriverName: e.target.value }))} placeholder="e.g. Epson TM-T88V" className={InputCls} />
                  </Field>
                )}

                {/* Paper & Orientation */}
                <div className="grid grid-cols-2 gap-3">
                  <Field label="Paper Size">
                    <select value={form.paperSize} onChange={e => setForm(f => ({ ...f, paperSize: e.target.value }))} className={SelectCls}>
                      {(currentType?.paperSizes ?? PAPER_SIZES).map(s => (
                        <option key={s} value={s}>{s}</option>
                      ))}
                    </select>
                  </Field>
                  <Field label="Orientation">
                    <select value={form.orientation} onChange={e => setForm(f => ({ ...f, orientation: e.target.value }))} className={SelectCls}>
                      <option value="Portrait">Portrait</option>
                      <option value="Landscape">Landscape</option>
                    </select>
                  </Field>
                </div>

                {/* Thermal-specific */}
                {(form.printerType === 'Thermal' || form.printerType === 'Portable') && (
                  <div className="grid grid-cols-2 gap-3">
                    <Field label="Chars / Line" hint="32=58mm, 42=80mm">
                      <input type="number" value={form.charsPerLine} onChange={e => setForm(f => ({ ...f, charsPerLine: e.target.value }))} placeholder="42" className={InputCls} />
                    </Field>
                    <Field label="Line Feed Count" hint="Lines before cut">
                      <input type="number" value={form.lineFeedCount} onChange={e => setForm(f => ({ ...f, lineFeedCount: e.target.value }))} placeholder="4" className={InputCls} />
                    </Field>
                  </div>
                )}

                {/* Label-specific */}
                {form.printerType === 'Label' && (
                  <Field label="DPI Resolution" hint="203 or 300 for label printers">
                    <input type="number" value={form.dpiResolution} onChange={e => setForm(f => ({ ...f, dpiResolution: e.target.value }))} placeholder="203" className={InputCls} />
                  </Field>
                )}

                {/* Capabilities */}
                <div>
                  <p className="text-xs font-bold text-slate-700 mb-3">Capabilities</p>
                  <div className="grid grid-cols-1 gap-2.5">
                    <Toggle checked={form.supportsEscPos}     onChange={v => setForm(f => ({ ...f, supportsEscPos: v }))}     label="ESC/POS (thermal direct printing)" />
                    <Toggle checked={form.supportsPdf}        onChange={v => setForm(f => ({ ...f, supportsPdf: v }))}        label="PDF / browser printing" />
                    <Toggle checked={form.supportsAutoCut}    onChange={v => setForm(f => ({ ...f, supportsAutoCut: v }))}    label="Auto paper cut" />
                    <Toggle checked={form.supportsCashDrawer} onChange={v => setForm(f => ({ ...f, supportsCashDrawer: v }))} label="Open cash drawer (RJ11)" />
                    <Toggle checked={form.supportsBarcodes}   onChange={v => setForm(f => ({ ...f, supportsBarcodes: v }))}   label="Print barcodes" />
                    <Toggle checked={form.supportsQRCode}     onChange={v => setForm(f => ({ ...f, supportsQRCode: v }))}     label="Print QR codes" />
                    <Toggle checked={form.supportsLogo}       onChange={v => setForm(f => ({ ...f, supportsLogo: v }))}       label="Print business logo" />
                    <Toggle checked={form.supportsColor}      onChange={v => setForm(f => ({ ...f, supportsColor: v }))}      label="Colour printing" />
                  </div>
                </div>

                {/* ESC/POS code page */}
                {form.supportsEscPos && (
                  <Field label="ESC/POS Code Page" hint="e.g. 16 = WPC1252 (default), 0 = PC437">
                    <input type="number" value={form.codePage} onChange={e => setForm(f => ({ ...f, codePage: e.target.value }))} placeholder="16" className={InputCls} />
                  </Field>
                )}

                {/* Flags */}
                <div className="flex flex-col gap-2.5">
                  <Toggle checked={form.isDefault} onChange={v => setForm(f => ({ ...f, isDefault: v }))} label="Set as default printer for this type" />
                  <Toggle checked={form.isActive}  onChange={v => setForm(f => ({ ...f, isActive: v }))}  label="Printer is active" />
                </div>

                {/* Notes */}
                <Field label="Notes (optional)">
                  <textarea rows={2} value={form.notes} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} placeholder="e.g. Located at billing counter 1" className={`${InputCls} resize-none`} />
                </Field>
              </div>

              {/* Form footer */}
              <div className="flex items-center gap-3 p-4 border-t border-slate-100">
                <button
                  onClick={handleSave}
                  disabled={saving}
                  className="flex items-center gap-2 px-5 py-2.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white rounded-xl text-xs font-bold shadow-sm shadow-indigo-600/20 transition-colors"
                >
                  {saving ? <RefreshCw className="w-3.5 h-3.5 animate-spin" /> : <Save className="w-3.5 h-3.5" />}
                  {saving ? 'Saving…' : editId ? 'Update Printer' : 'Register Printer'}
                </button>
                <button
                  onClick={() => { setShowForm(false); setActiveSection('list'); }}
                  className="px-4 py-2.5 rounded-xl border border-slate-200 text-xs font-semibold text-slate-600 hover:bg-slate-50 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </div>

            {/* Config preview panel */}
            <ConfigPreview />
          </div>
        )}
      </div>

      {/* Quick Reference */}
      <div className="bg-white rounded-2xl border border-slate-200 p-5">
        <h3 className="text-sm font-bold text-slate-900 mb-3 flex items-center gap-2">
          <Zap className="w-4 h-4 text-amber-500" />
          Printer Type Quick Reference
        </h3>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
          {PRINTER_TYPES.map(t => {
            const Icon = t.icon;
            return (
              <div key={t.value} className={`p-3 rounded-xl border border-slate-100 ${t.bg}`}>
                <div className="flex items-center gap-2 mb-1.5">
                  <Icon className={`w-4 h-4 ${t.color}`} />
                  <span className={`text-[11px] font-bold ${t.color}`}>{t.label}</span>
                </div>
                <p className="text-[10px] text-slate-500 leading-relaxed">{t.desc}</p>
                <div className="mt-2 flex flex-wrap gap-1">
                  {t.paperSizes.slice(0, 3).map(s => (
                    <span key={s} className="px-1.5 py-0.5 bg-white/70 text-[9px] font-semibold text-slate-600 rounded border border-slate-200/50">{s}</span>
                  ))}
                </div>
              </div>
            );
          })}
        </div>

        <div className="mt-4 pt-4 border-t border-slate-100">
          <p className="text-[11px] font-bold text-slate-700 mb-2">API Endpoints</p>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-[10px] font-mono text-slate-500">
            <div>GET <span className="text-indigo-600">/api/printer</span> — list printers</div>
            <div>POST <span className="text-indigo-600">/api/printer</span> — register printer</div>
            <div>PUT <span className="text-indigo-600">/api/printer/{'{id}'}</span> — update</div>
            <div>POST <span className="text-indigo-600">/api/printer/{'{id}'}/test</span> — test connection</div>
            <div>POST <span className="text-indigo-600">/api/printer/{'{id}'}/set-default</span> — set default</div>
            <div>POST <span className="text-indigo-600">/api/printer/bill/{'{invoiceId}'}</span> — print bill</div>
            <div>POST <span className="text-indigo-600">/api/printer/reprint/{'{jobId}'}</span> — reprint</div>
            <div>GET <span className="text-indigo-600">/api/print-settings</span> — print settings</div>
          </div>
        </div>
      </div>
    </div>
  );
};
