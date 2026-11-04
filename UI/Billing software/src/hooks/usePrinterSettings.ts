// src/hooks/usePrinterSettings.ts
import { useState, useEffect } from 'react';
import { api } from '../services/apiClient';

export interface DefaultPrinterInfo {
  id: number;
  printerName: string;
  printerType: string;
  paperSize: string;
  connectionType: string;
  supportsEscPos: boolean;
  supportsPdf: boolean;
  isDefault: boolean;
  charsPerLine?: number;
}

export const usePrinterSettings = () => {
  const [defaultPrinter, setDefaultPrinter] = useState<DefaultPrinterInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const cached = sessionStorage.getItem('billflow_default_printer');
    if (cached) {
      try { setDefaultPrinter(JSON.parse(cached)); setIsLoading(false); return; } catch {}
    }
    api.printers.getAll({ activeOnly: true })
      .then((printers: DefaultPrinterInfo[]) => {
        const def = printers.find(p => p.isDefault) ?? printers[0] ?? null;
        setDefaultPrinter(def);
        if (def) sessionStorage.setItem('billflow_default_printer', JSON.stringify(def));
      })
      .catch(() => setDefaultPrinter(null))
      .finally(() => setIsLoading(false));
  }, []);

  const paperSize = defaultPrinter?.paperSize ?? 'A4';
  const printerType = defaultPrinter?.printerType ?? 'Laser';
  const ESC_POS_TYPES = new Set(['Thermal', 'Label', 'Portable']);
  const isEscPos = ESC_POS_TYPES.has(printerType);

  return { defaultPrinter, paperSize, printerType, isEscPos, isLoading };
};
