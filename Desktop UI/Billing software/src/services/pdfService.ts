import type { Invoice, BusinessProfile } from '../types';

// ─── Paper size → CSS @page mapping ──────────────────────────────────────────
// Covers all sizes supported by the printer type catalogue in PrinterSettingsPage.

const PAGE_SIZE_MAP: Record<string, string> = {
  // Standard office
  'A4':               'A4 portrait',
  'A5':               'A5 portrait',
  'A6':               '105mm 148mm',
  'A3':               'A3 portrait',
  'B5':               '176mm 250mm',
  'B4':               '250mm 353mm',
  'Letter':           'letter portrait',
  'Legal':            'legal portrait',
  'Executive':        '184mm 267mm',
  'Folio':            '216mm 330mm',
  'Tabloid':          'tabloid portrait',
  'SRA3':             '320mm 450mm',
  // Photo sizes
  '4x6':              '102mm 152mm',
  '5x7':              '127mm 178mm',
  '8x10':             '203mm 254mm',
  // Thermal receipt rolls (continuous — height auto)
  '58mm':             '58mm auto',
  '80mm':             '80mm auto',
  '110mm':            '110mm auto',
  // Dot-matrix / continuous feed
  'Continuous9.5x11': '241mm 279mm',
  'Continuous9.5x12': '241mm 305mm',
  // Label sizes
  '25x15mm':          '25mm 15mm',
  '50x25mm':          '50mm 25mm',
  '100x50mm':         '100mm 50mm',
  '100x150mm':        '100mm 150mm',
  // Portable photo
  '2x3':              '51mm 76mm',
  '3x4':              '76mm 102mm',
  // Fallback
  'Custom':           'auto',
};

// ─── Printable container max-width per paper size ─────────────────────────────
const WIDTH_FOR_SIZE: Record<string, string> = {
  'A4': '210mm', 'A5': '148mm', 'A6': '105mm', 'A3': '297mm',
  'B5': '176mm', 'B4': '250mm',
  'Letter': '216mm', 'Legal': '216mm', 'Executive': '184mm', 'Folio': '216mm',
  'Tabloid': '279mm', 'SRA3': '320mm',
  '4x6': '102mm', '5x7': '127mm', '8x10': '203mm',
  '58mm': '48mm', '80mm': '72mm', '110mm': '100mm',
  'Continuous9.5x11': '241mm', 'Continuous9.5x12': '241mm',
  '25x15mm': '23mm', '50x25mm': '46mm', '100x50mm': '94mm', '100x150mm': '94mm',
  '2x3': '48mm', '3x4': '72mm',
  'Custom': '100%',
};

// ─── Font family per paper size ───────────────────────────────────────────────
// Narrow / receipt / label formats use Courier New for column alignment.
const NARROW_SIZES = new Set([
  '58mm', '80mm', '110mm',
  '25x15mm', '50x25mm', '100x50mm', '100x150mm',
  '2x3', '3x4',
]);

const FONT_FOR_SIZE = (paperSize: string): string =>
  NARROW_SIZES.has(paperSize)
    ? "'Courier New', Courier, monospace"
    : "'Inter', system-ui, -apple-system, sans-serif";

// ─── Font size + margin per paper size ───────────────────────────────────────
const FONT_SIZE_FOR_SIZE = (paperSize: string): string => {
  if (['25x15mm', '50x25mm', '2x3'].includes(paperSize)) return '8px';
  if (NARROW_SIZES.has(paperSize)) return '10px';
  if (['A6', '4x6', '5x7'].includes(paperSize)) return '11px';
  return '12px';
};

const MARGIN_FOR_SIZE = (paperSize: string): string =>
  NARROW_SIZES.has(paperSize) ? '2mm' : '10mm';

class PDFService {
  private styleId = 'billflow-print-style';

  /**
   * Inject (or update) a <style id="billflow-print-style"> with the correct
   * @page and #printable-invoice container rules for the given paper size.
   */
  applyPaperSize(paperSize: string): void {
    const pageSize  = PAGE_SIZE_MAP[paperSize]   ?? 'A4 portrait';
    const font      = FONT_FOR_SIZE(paperSize);
    const fontSize  = FONT_SIZE_FOR_SIZE(paperSize);
    const width     = WIDTH_FOR_SIZE[paperSize]  ?? '210mm';
    const margin    = MARGIN_FOR_SIZE(paperSize);
    const isNarrow  = NARROW_SIZES.has(paperSize);

    let el = document.getElementById(this.styleId) as HTMLStyleElement | null;
    if (!el) {
      el = document.createElement('style');
      el.id = this.styleId;
      document.head.appendChild(el);
    }

    el.textContent = `
      @page {
        size: ${pageSize};
        margin: ${margin};
      }
      @media print {
        body * { visibility: hidden; }
        #printable-invoice, #printable-invoice * { visibility: visible; }
        #printable-invoice {
          position: absolute;
          left: 0; top: 0;
          width: ${width} !important;
          max-width: ${width} !important;
          font-family: ${font};
          font-size: ${fontSize};
          line-height: ${isNarrow ? '1.3' : '1.5'};
          padding: ${isNarrow ? '2mm' : '8mm'};
          background: white;
          color: black;
        }
        .no-print { display: none !important; }
        ${isNarrow ? `
          #printable-invoice table { width: 100%; border-collapse: collapse; }
          #printable-invoice th, #printable-invoice td {
            font-size: ${fontSize}; padding: 1px 2px; word-break: break-word;
          }
          #printable-invoice .hide-on-thermal { display: none !important; }
        ` : ''}
      }
    `;
  }

  /** Remove the dynamic style (cleanup after print dialog closes) */
  removePaperStyle(): void {
    document.getElementById(this.styleId)?.remove();
  }

  triggerPrint(): void {
    window.print();
  }

  /**
   * Apply the correct @page rules for paperSize, trigger window.print(),
   * then clean up the injected style ~2 seconds after.
   */
  printWithPaperSize(paperSize: string): void {
    this.applyPaperSize(paperSize);
    // 100ms delay ensures the style is parsed before the print dialog opens
    setTimeout(() => {
      window.print();
      setTimeout(() => this.removePaperStyle(), 2000);
    }, 100);
  }

  downloadMockPDF(invoice: Invoice, business: BusinessProfile): void {
    const textContent = `
===================================================
${business.name.toUpperCase()} - TAX INVOICE
${business.address}, ${business.city}, ${business.state} - ${business.pincode}
GSTIN: ${business.gstin} | Phone: ${business.phone}
===================================================

Invoice No: ${invoice.invoiceNumber}
Date: ${new Date(invoice.createdAt).toLocaleString()}
Register: ${invoice.registerId} | Cashier: ${invoice.cashierName}

Customer: ${invoice.customerName} (${invoice.customerPhone})
${invoice.customerGstin ? `Customer GSTIN: ${invoice.customerGstin}` : ''}

---------------------------------------------------
ITEMS:
${invoice.items
  .map(
    (item, idx) =>
      `${idx + 1}. ${item.productName}\n   Qty: ${item.quantity} ${item.unit} x ₹${item.price.toFixed(2)} = ₹${item.total.toFixed(2)} (GST ${item.taxRate}%)`
  )
  .join('\n')}

---------------------------------------------------
Subtotal:          ₹${invoice.subtotal.toFixed(2)}
Discount:         -₹${(invoice.itemDiscountTotal + invoice.extraDiscount).toFixed(2)}
CGST:              ₹${invoice.cgst.toFixed(2)}
SGST:              ₹${invoice.sgst.toFixed(2)}
Round off:         ₹${invoice.roundOff.toFixed(2)}
---------------------------------------------------
GRAND TOTAL:       ₹${invoice.grandTotal.toFixed(2)}
Payment Method:    ${invoice.paymentMethod.toUpperCase()}
Payment Status:    ${invoice.paymentStatus.toUpperCase()}
---------------------------------------------------
${business.invoiceSettings.footerMessage}
${business.invoiceSettings.termsAndConditions}
===================================================
`;

    const blob = new Blob([textContent], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `${invoice.invoiceNumber}.txt`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  }
}

export const pdfService = new PDFService();
