export interface EmailSendPayload {
  toEmail: string;
  customerName: string;
  invoiceNumber: string;
  grandTotal: number;
  subject?: string;
  message?: string;
}

class EmailService {
  async sendInvoiceEmail(payload: EmailSendPayload): Promise<{ success: boolean; id: string }> {
    await new Promise((resolve) => setTimeout(resolve, 1400));
    if (!payload.toEmail || !payload.toEmail.includes('@')) {
      throw new Error('Valid email address is required.');
    }
    return {
      success: true,
      id: `EMAIL-${Date.now()}`,
    };
  }
}

export const emailService = new EmailService();
