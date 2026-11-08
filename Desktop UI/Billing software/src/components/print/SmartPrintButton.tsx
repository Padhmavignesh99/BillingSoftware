/**
 * SmartPrintButton
 *
 * Detects the default printer type registered in the printer registry and
 * routes the print action accordingly:
 *
 *   • PDF/Browser printers (Laser, Inkjet, Multifunction, DotMatrix)
 *       → calls pdfService.printWithPaperSize(paperSize) which injects
 *         correct @page CSS then calls window.print()
 *
 *   • ESC/POS printers (Thermal, Label, Portable)
 *       → queues a print job via api.printers.printBill(invoiceId)
 *         which a local print agent (QZ Tray / custom bridge) picks up
 *
 *   • No printer registered → falls back to A4 browser print
 *
 * Props:
 *   invoiceId   — numeric DB id of the invoice/bill (for ESC/POS path)
 *   label       — optional button label (default: "Print")
 *   variant     — "primary" | "secondary" | "ghost"
 *   onPrinted   — optional callback after successful queue/print
 */

import React, { useState, useCallback } from 'react';
import { Printer, ChevronDown, RefreshCw, FileText, Zap, Settings, Maximize2 } from 'lucide-react';
import { api } from '../../services/apiClient';
import { useToast } from '../common/Toast';
import { useNavigate } from 'react-router-dom';
import { pdfService } from '../../services/pdfService';
import { usePrinterSettings } from '../../hooks/usePrinterSettings';

// ESC/POS printer types that go through the print-job queue
const ESC_POS_TYPES = new Set(['Thermal', 'Label', 'Portable']);

// Human-readable labels for paper sizes
const PAPER_LABELS: Record<string, string> = {
  // Standard office
  'A4': 'A4 (210 x 297 mm)',
  'A5': 'A5 (148 x 210 mm)',
  'A6': 'A6 (105 x 148 mm)',
  'A3': 'A3 (297 x 420 mm)',
  'B5': 'B5 (176 x 250 mm)',
  'B4': 'B4 (250 x 353 mm)',
  'Letter': 'Letter (215.9 x 279.4 mm)',
  'Legal': 'Legal (215.9 x 355.6 mm)',
  'Executive': 'Executive (184.1 x 266.7 mm)',
  'Folio': 'Folio (215.9 x 330.2 mm)',
  'Tabloid': 'Tabloid / Ledger (279.4 x 431.8 mm)',
  'SRA3': 'SRA3 (320 x 450 mm)',
  // Photo sizes
  '4x6': '4x6 Photo (101.6 x 152.4 mm)',
  '5x7': '5x7 Photo (127 x 177.8 mm)',
  '8x10': '8x10 Photo (203.2 x 254 mm)',
  // Thermal receipt rolls
  '58mm': '58 mm Thermal Roll',
  '80mm': '80 mm Thermal Roll',
  '110mm': '110 mm Thermal Roll',
  // Dot-matrix / continuous
  'Continuous9.5x11': 'Continuous 9.5x11 (241.3 x 279.4 mm)',
  'Continuous9.5x12': 'Continuous 9.5x12 (241.3 x 304.8 mm)',
  // Label sizes
  '25x15mm': '25x15 mm Label',
  '50x25mm': '50x25 mm Label',
  '100x50mm': '100x50 mm Label',
  '100x150mm': '100x150 mm Shipping Label',
  // Portable photo
  '2x3': '2x3 Pocket Photo (50.8 x 76.2 mm)',
  '3x4': '3x4 Photo Sticker (76.2 x 101.6 mm)',
  // Fallback
  'Custom': 'Custom Size',
};

interface SmartPrintButtonProps {
  invoiceId?: string | number;
  label?: string;
  variant?: 'primary' | 'secondary' | 'ghost';
  className?: string;
  onPrinted?: () => void;
}

type PrintMode = 'browser' | 'escpos' | 'detecting' | 'idle';

export const SmartPrintButton: React.FC<SmartPrintButtonProps> = ({
  invoiceId,
  label = 'Print',
  variant = 'primary',
  className = '',
  onPrinted,
}) => {
  const { showToast } = useToast();
  const navigate = useNavigate();
  const [mode, setMode] = useState<PrintMode>('idle');
  const [showDropdown, setShowDropdown] = useState(false);
  
  // Use the centralized printer settings hook
  const { defaultPrinter, paperSize, printerType, isEscPos, isLoading } = usePrinterSettings();

  const handlePrint = useCallback(async () => {
    setShowDropdown(false);

    if (isEscPos && invoiceId) {
      // ESC/POS path — queue job via API
      setMode('escpos');
      try {
        await api.printers.printBill(Number(invoiceId));
        showToast(
          `Print job queued → ${defaultPrinter?.printerName ?? 'Thermal printer'} (${paperSize})`,
          'success'
        );
        onPrinted?.();
      } catch (err: any) {
        showToast(err?.message ?? 'Failed to queue print job', 'error');
      } finally {
        setMode('idle');
      }
    } else {
      // Browser / PDF path — use paper-size aware print
      setMode('browser');
      try {
        pdfService.printWithPaperSize(paperSize);
        onPrinted?.();
      } finally {
        setTimeout(() => setMode('idle'), 500);
      }
    }
  }, [isEscPos, invoiceId, defaultPrinter, paperSize, showToast, onPrinted]);

  const handleBrowserPrint = (overridePaperSize?: string) => {
    setShowDropdown(false);
    setMode('browser');
    pdfService.printWithPaperSize(overridePaperSize ?? paperSize);
    setTimeout(() => setMode('idle'), 500);
    onPrinted?.();
  };

  const handleEscPosPrint = async () => {
    setShowDropdown(false);
    if (!invoiceId) {
      showToast('No invoice ID available for thermal printing', 'error');
      return;
    }
    setMode('escpos');
    try {
      await api.printers.printBill(Number(invoiceId));
      showToast('Print job queued for thermal printer', 'success');
      onPrinted?.();
    } catch (err: any) {
      showToast(err?.message ?? 'Failed to queue print job', 'error');
    } finally {
      setMode('idle');
    }
  };

  const busy = mode !== 'idle';

  // Variant styles
  const primaryCls = 'bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm shadow-indigo-600/30';
  const secondaryCls = 'bg-slate-100 hover:bg-slate-200 text-slate-700';
  const ghostCls = 'border border-slate-200 hover:bg-slate-50 text-slate-600';

  const baseCls =
    variant === 'primary' ? primaryCls : variant === 'secondary' ? secondaryCls : ghostCls;

  const paperLabel = PAPER_LABELS[paperSize] ?? paperSize;
  const isNarrow = ['80mm', '58mm', '110mm', '25x15mm', '50x25mm', '100x50mm', '100x150mm', '2x3', '3x4'].includes(paperSize);

  return (
    <div className="relative inline-flex">
      {/* Main print button */}
      <button
        onClick={handlePrint}
        disabled={busy || isLoading}
        className={`flex items-center gap-1.5 px-4 py-2 rounded-l-xl text-xs font-semibold transition-all disabled:opacity-60 ${baseCls} ${className}`}
      >
        {busy ? (
          <RefreshCw className="w-4 h-4 animate-spin" />
        ) : (
          <Printer className="w-4 h-4" />
        )}
        <span>{busy ? (mode === 'escpos' ? 'Queuing…' : 'Printing…') : label}</span>
        {/* Paper size badge on main button */}
        {!busy && (
          <span className={`text-[9px] font-bold px-1.5 py-0.5 rounded ml-1 ${
            variant === 'primary' 
              ? 'bg-indigo-500/50 text-indigo-100' 
              : 'bg-slate-200 text-slate-500'
          }`}>
            {paperSize}
          </span>
        )}
      </button>

      {/* Dropdown toggle */}
      <button
        onClick={() => setShowDropdown(v => !v)}
        disabled={busy}
        className={`flex items-center px-2 py-2 rounded-r-xl border-l transition-all disabled:opacity-60 ${
          variant === 'primary'
            ? 'bg-indigo-700 hover:bg-indigo-800 text-white border-indigo-500'
            : variant === 'secondary'
            ? 'bg-slate-200 hover:bg-slate-300 text-slate-700 border-slate-300'
            : 'border border-slate-200 hover:bg-slate-50 text-slate-500'
        }`}
      >
        <ChevronDown className="w-3.5 h-3.5" />
      </button>

      {/* Dropdown menu */}
      {showDropdown && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setShowDropdown(false)} />
          <div className="absolute right-0 top-full mt-1.5 w-72 bg-white rounded-2xl border border-slate-200 shadow-xl z-20 overflow-hidden">
            
            {/* Active printer info */}
            <div className={`px-3 py-2.5 border-b ${defaultPrinter ? 'bg-indigo-50 border-indigo-100' : 'bg-amber-50 border-amber-100'}`}>
              <div className="flex items-center gap-2">
                <Printer className={`w-3.5 h-3.5 ${defaultPrinter ? 'text-indigo-500' : 'text-amber-500'}`} />
                <div className="flex-1 min-w-0">
                  {defaultPrinter ? (
                    <>
                      <p className="text-[10px] font-bold text-indigo-700 leading-tight truncate">
                        {defaultPrinter.printerName}
                      </p>
                      <p className="text-[9px] text-indigo-400">
                        {defaultPrinter.printerType} · {defaultPrinter.connectionType}
                      </p>
                    </>
                  ) : (
                    <p className="text-[10px] text-amber-700">No printer registered — using A4 default</p>
                  )}
                </div>
                {/* Paper size pill */}
                <span className={`flex-shrink-0 text-[9px] font-bold px-2 py-0.5 rounded-full ${
                  isNarrow 
                    ? 'bg-emerald-100 text-emerald-700' 
                    : 'bg-blue-100 text-blue-700'
                }`}>
                  {paperSize}
                </span>
              </div>
              <p className="text-[9px] text-indigo-300 mt-0.5 pl-5">{paperLabel}</p>
            </div>

            {/* Print options */}
            <div className="p-1">
              {/* Smart Print (auto-detect) */}
              <button
                onClick={handlePrint}
                className="w-full flex items-center gap-2.5 px-3 py-2.5 rounded-xl hover:bg-indigo-50 transition-colors text-left"
              >
                <div className="w-7 h-7 rounded-lg bg-indigo-600 flex items-center justify-center flex-shrink-0">
                  <Printer className="w-3.5 h-3.5 text-white" />
                </div>
                <div className="flex-1">
                  <p className="text-[11px] font-bold text-slate-800">Smart Print</p>
                  <p className="text-[10px] text-slate-400">Auto: {isEscPos ? 'ESC/POS' : 'Browser'} · {paperSize}</p>
                </div>
              </button>

              {/* Browser/PDF with auto paper size */}
              <button
                onClick={() => handleBrowserPrint()}
                className="w-full flex items-center gap-2.5 px-3 py-2.5 rounded-xl hover:bg-slate-50 transition-colors text-left"
              >
                <div className="w-7 h-7 rounded-lg bg-blue-50 flex items-center justify-center flex-shrink-0">
                  <FileText className="w-3.5 h-3.5 text-blue-600" />
                </div>
                <div className="flex-1">
                  <p className="text-[11px] font-bold text-slate-800">Browser / PDF Print</p>
                  <p className="text-[10px] text-slate-400">OS dialog · {paperLabel}</p>
                </div>
              </button>

              {/* Force A4 option (always available as fallback) */}
              {paperSize !== 'A4' && (
                <button
                  onClick={() => handleBrowserPrint('A4')}
                  className="w-full flex items-center gap-2.5 px-3 py-2.5 rounded-xl hover:bg-slate-50 transition-colors text-left"
                >
                  <div className="w-7 h-7 rounded-lg bg-slate-100 flex items-center justify-center flex-shrink-0">
                    <Maximize2 className="w-3.5 h-3.5 text-slate-500" />
                  </div>
                  <div className="flex-1">
                    <p className="text-[11px] font-bold text-slate-700">Print as A4</p>
                    <p className="text-[10px] text-slate-400">Override — force A4 layout</p>
                  </div>
                </button>
              )}

              {/* ESC/POS Thermal option */}
              {invoiceId && (
                <button
                  onClick={handleEscPosPrint}
                  className="w-full flex items-center gap-2.5 px-3 py-2.5 rounded-xl hover:bg-slate-50 transition-colors text-left"
                >
                  <div className="w-7 h-7 rounded-lg bg-emerald-50 flex items-center justify-center flex-shrink-0">
                    <Zap className="w-3.5 h-3.5 text-emerald-600" />
                  </div>
                  <div>
                    <p className="text-[11px] font-bold text-slate-800">ESC/POS Thermal Print</p>
                    <p className="text-[10px] text-slate-400">Queues job to thermal printer</p>
                  </div>
                </button>
              )}

              <div className="border-t border-slate-100 mt-1 pt-1">
                <button
                  onClick={() => { setShowDropdown(false); navigate('/settings/printers'); }}
                  className="w-full flex items-center gap-2.5 px-3 py-2.5 rounded-xl hover:bg-slate-50 transition-colors text-left"
                >
                  <div className="w-7 h-7 rounded-lg bg-slate-50 flex items-center justify-center flex-shrink-0">
                    <Settings className="w-3.5 h-3.5 text-slate-500" />
                  </div>
                  <div>
                    <p className="text-[11px] font-bold text-slate-600">Printer Settings</p>
                    <p className="text-[10px] text-slate-400">Change default printer & paper size</p>
                  </div>
                </button>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default SmartPrintButton;
