/**
 * SmartPrintButton
 *
 * Detects the default printer type registered in the printer registry and
 * routes the print action accordingly:
 *
 *   • PDF/Browser printers (Laser, Inkjet, Multifunction, DotMatrix)
 *       → calls window.print() (uses OS printer dialog)
 *
 *   • ESC/POS printers (Thermal, Label, Portable)
 *       → queues a print job via api.printers.printBill(invoiceId)
 *         which a local print agent (QZ Tray / custom bridge) picks up
 *
 *   • No printer registered → falls back to window.print()
 *
 * Props:
 *   invoiceId   — numeric DB id of the invoice/bill (for ESC/POS path)
 *   label       — optional button label (default: "Print")
 *   variant     — "primary" | "secondary" | "ghost"
 *   onPrinted   — optional callback after successful queue/print
 */

import React, { useState, useCallback } from 'react';
import { Printer, ChevronDown, RefreshCw, FileText, Zap, Settings } from 'lucide-react';
import { api } from '../../services/apiClient';
import { useToast } from '../common/Toast';
import { useNavigate } from 'react-router-dom';
import { pdfService } from '../../services/pdfService';

// ESC/POS printer types that go through the print-job queue
const ESC_POS_TYPES = new Set(['Thermal', 'Label', 'Portable']);

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
  const [defaultPrinter, setDefaultPrinter] = useState<any>(null);
  const [printerLoaded, setPrinterLoaded] = useState(false);

  // Lazy-load the default printer info once
  const loadDefaultPrinter = useCallback(async () => {
    if (printerLoaded) return;
    try {
      const printers: any[] = await api.printers.getAll({ activeOnly: true });
      const def = printers.find((p) => p.isDefault) ?? printers[0] ?? null;
      setDefaultPrinter(def);
    } catch {
      setDefaultPrinter(null);
    } finally {
      setPrinterLoaded(true);
    }
  }, [printerLoaded]);

  const handlePrint = useCallback(async () => {
    setShowDropdown(false);
    await loadDefaultPrinter();

    const isEscPos = defaultPrinter
      ? ESC_POS_TYPES.has(defaultPrinter.printerType)
      : false;

    if (isEscPos && invoiceId) {
      // ESC/POS path — queue job via API
      setMode('escpos');
      try {
        await api.printers.printBill(Number(invoiceId));
        showToast(
          `Print job queued → ${defaultPrinter?.printerName ?? 'Thermal printer'}`,
          'success'
        );
        onPrinted?.();
      } catch (err: any) {
        showToast(err?.message ?? 'Failed to queue print job', 'error');
      } finally {
        setMode('idle');
      }
    } else {
      // Browser / PDF path
      setMode('browser');
      try {
        pdfService.triggerPrint();
        onPrinted?.();
      } finally {
        setMode('idle');
      }
    }
  }, [loadDefaultPrinter, defaultPrinter, invoiceId, showToast, onPrinted]);

  const handleBrowserPrint = () => {
    setShowDropdown(false);
    setMode('browser');
    pdfService.triggerPrint();
    setMode('idle');
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
  const primaryCls =
    'bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm shadow-indigo-600/30';
  const secondaryCls =
    'bg-slate-100 hover:bg-slate-200 text-slate-700';
  const ghostCls =
    'border border-slate-200 hover:bg-slate-50 text-slate-600';

  const baseCls =
    variant === 'primary'
      ? primaryCls
      : variant === 'secondary'
      ? secondaryCls
      : ghostCls;

  return (
    <div className="relative inline-flex">
      {/* Main print button */}
      <button
        onClick={handlePrint}
        disabled={busy}
        onMouseEnter={loadDefaultPrinter}
        className={`flex items-center gap-1.5 px-4 py-2 rounded-l-xl text-xs font-semibold transition-all disabled:opacity-60 ${baseCls} ${className}`}
      >
        {busy ? (
          <RefreshCw className="w-4 h-4 animate-spin" />
        ) : (
          <Printer className="w-4 h-4" />
        )}
        <span>{busy ? (mode === 'escpos' ? 'Queuing…' : 'Printing…') : label}</span>
      </button>

      {/* Dropdown toggle */}
      <button
        onClick={() => { loadDefaultPrinter(); setShowDropdown((v) => !v); }}
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
          <div
            className="fixed inset-0 z-10"
            onClick={() => setShowDropdown(false)}
          />
          <div className="absolute right-0 top-full mt-1.5 w-64 bg-white rounded-2xl border border-slate-200 shadow-xl z-20 overflow-hidden">
            {/* Default printer info */}
            {printerLoaded && defaultPrinter && (
              <div className="px-3 py-2.5 bg-indigo-50 border-b border-indigo-100 flex items-center gap-2">
                <Printer className="w-3.5 h-3.5 text-indigo-500" />
                <div>
                  <p className="text-[10px] font-bold text-indigo-700 leading-tight">
                    Default: {defaultPrinter.printerName}
                  </p>
                  <p className="text-[9px] text-indigo-400">
                    {defaultPrinter.printerType} · {defaultPrinter.connectionType} · {defaultPrinter.paperSize}
                  </p>
                </div>
              </div>
            )}
            {printerLoaded && !defaultPrinter && (
              <div className="px-3 py-2.5 bg-amber-50 border-b border-amber-100 flex items-center gap-2">
                <Settings className="w-3.5 h-3.5 text-amber-500" />
                <p className="text-[10px] text-amber-700">No printer registered yet</p>
              </div>
            )}

            {/* Print options */}
            <div className="p-1">
              <button
                onClick={handleBrowserPrint}
                className="w-full flex items-center gap-2.5 px-3 py-2.5 rounded-xl hover:bg-slate-50 transition-colors text-left"
              >
                <div className="w-7 h-7 rounded-lg bg-blue-50 flex items-center justify-center flex-shrink-0">
                  <FileText className="w-3.5 h-3.5 text-blue-600" />
                </div>
                <div>
                  <p className="text-[11px] font-bold text-slate-800">Browser / PDF Print</p>
                  <p className="text-[10px] text-slate-400">Opens OS printer dialog</p>
                </div>
              </button>

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
                    <p className="text-[10px] text-slate-400">Manage registered printers</p>
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

