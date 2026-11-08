export interface WhatsAppSendPayload {
  customerName: string;
  phone: string;
  invoiceNumber: string;
  grandTotal: number;
  pdfAttachmentUrl?: string;
  customNote?: string;
}

export interface WhatsAppSendResult {
  success: boolean;
  messageId: string;
  timestamp: string;
  recipientPhone: string;
}

class WhatsAppService {
  async sendInvoice(payload: WhatsAppSendPayload): Promise<WhatsAppSendResult> {
    // Simulate network delay for realistic frontend feedback UX
    await new Promise((resolve) => setTimeout(resolve, 1500));

    if (!payload.phone || payload.phone === 'N/A') {
      throw new Error('Valid phone number with country code is required to send WhatsApp invoice.');
    }

    return {
      success: true,
      messageId: `WA-MSG-${Date.now()}`,
      timestamp: new Date().toISOString(),
      recipientPhone: payload.phone,
    };
  }

  generateMessagePreview(payload: WhatsAppSendPayload, businessName: string): string {
    return `Hello ${payload.customerName},\n\nThank you for shopping at *${businessName}*! 🙏\n\nYour Invoice *${payload.invoiceNumber}* for *₹${payload.grandTotal.toLocaleString('en-IN')}* has been generated successfully.\n\n📄 View/Download PDF Receipt: https://billflow.app/pdf/${payload.invoiceNumber}\n\nHave a great day!`;
  }
}

export const whatsappService = new WhatsAppService();
