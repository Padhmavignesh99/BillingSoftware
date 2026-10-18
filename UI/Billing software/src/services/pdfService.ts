import type { Invoice, BusinessProfile } from '../types';

class PDFService {
  triggerPrint(): void {
    window.print();
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
