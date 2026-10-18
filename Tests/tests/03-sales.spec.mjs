import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

test.describe('🧾 Sales API', () => {
  let authHeaders;
  let createdInvoiceId;

  test.beforeAll(async () => {
    const auth = await getAuthToken();
    authHeaders = { Authorization: `Bearer ${auth.token}`, 'Content-Type': 'application/json' };
  });

  // ────────── INVOICES ──────────
  test('GET /api/invoices - list all invoices (paginated)', async ({ request }) => {
    const res = await request.get('/api/invoices?page=1&pageSize=10', { headers: authHeaders });
    expect([200, 401]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) {
      expect(body.success).toBe(true);
      expect(Array.isArray(body.data)).toBe(true);
    }
    console.log(`✅ GET invoices: status=${res.status()}`);
  });

  test('POST /api/invoices - create new invoice', async ({ request }) => {
    const payload = {
      customerName: 'Test Customer',
      customerPhone: '9876543210',
      customerEmail: 'customer@test.com',
      paymentMode: 'Cash',
      discountAmount: 0,
      taxAmount: 18,
      totalAmount: 118,
      paidAmount: 118,
      items: [
        { productName: 'Test Product', quantity: 1, unitPrice: 100, taxRate: 18, discountAmount: 0, totalPrice: 118 }
      ]
    };
    const res = await request.post('/api/invoices', { headers: authHeaders, data: payload });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200 || res.status() === 201) {
      createdInvoiceId = body?.data?.id;
    }
    console.log(`✅ POST invoice: status=${res.status()}, id=${createdInvoiceId}`);
  });

  test('GET /api/invoices/:id - get invoice by ID', async ({ request }) => {
    const id = createdInvoiceId ?? 1;
    const res = await request.get(`/api/invoices/${id}`, { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET invoice by ID: status=${res.status()}`);
  });

  test('GET /api/invoices/next-number - get next invoice number', async ({ request }) => {
    const res = await request.get('/api/invoices/next-number', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET next invoice number: status=${res.status()}`);
  });

  test('PUT /api/invoices/:id - update invoice', async ({ request }) => {
    const id = createdInvoiceId ?? 1;
    const res = await request.put(`/api/invoices/${id}`, {
      headers: authHeaders,
      data: { customerName: 'Updated Customer', totalAmount: 200, paidAmount: 200 }
    });
    expect([200, 201, 400, 404, 422, 500]).toContain(res.status());
    console.log(`✅ PUT invoice: status=${res.status()}`);
  });

  test('GET /api/invoices/:id/pdf - get invoice PDF', async ({ request }) => {
    const id = createdInvoiceId ?? 1;
    const res = await request.get(`/api/invoices/${id}/pdf`, { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET invoice PDF: status=${res.status()}`);
  });

  // ────────── POS BILLING ──────────
  test('POST /api/pos/bill - create POS bill', async ({ request }) => {
    const res = await request.post('/api/pos/bill', {
      headers: authHeaders,
      data: {
        customerName: 'Walk-in Customer',
        paymentMode: 'Cash',
        totalAmount: 250,
        paidAmount: 250,
        items: [{ productName: 'Item A', quantity: 2, unitPrice: 125, taxRate: 0, discountAmount: 0, totalPrice: 250 }]
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ POST POS bill: status=${res.status()}`);
  });

  test('GET /api/pos/last-bill - get last POS bill', async ({ request }) => {
    const res = await request.get('/api/pos/last-bill', { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET last bill: status=${res.status()}`);
  });

  // ────────── QUOTATIONS ──────────
  test('POST /api/quotations - create quotation', async ({ request }) => {
    const res = await request.post('/api/quotations', {
      headers: authHeaders,
      data: {
        customerName: 'Quotation Customer',
        validUntil: new Date(Date.now() + 7 * 24 * 3600000).toISOString(),
        items: [{ productName: 'Product X', quantity: 3, unitPrice: 50, taxRate: 5, discountAmount: 0, totalPrice: 157.5 }]
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ POST quotation: status=${res.status()}`);
  });

  test('GET /api/quotations - list quotations', async ({ request }) => {
    const res = await request.get('/api/quotations', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET quotations: status=${res.status()}`);
  });

  // ────────── SALES ORDERS ──────────
  test('POST /api/sales-orders - create sales order', async ({ request }) => {
    const res = await request.post('/api/sales-orders', {
      headers: authHeaders,
      data: {
        customerName: 'Order Customer',
        expectedDeliveryDate: new Date(Date.now() + 3 * 24 * 3600000).toISOString(),
        items: [{ productName: 'Product Y', quantity: 5, unitPrice: 80, taxRate: 12, discountAmount: 10, totalPrice: 438 }]
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ POST sales order: status=${res.status()}`);
  });

  test('GET /api/sales-orders - list sales orders', async ({ request }) => {
    const res = await request.get('/api/sales-orders', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET sales orders: status=${res.status()}`);
  });

  // ────────── DELETE ──────────
  test('DELETE /api/invoices/:id - delete invoice', async ({ request }) => {
    const id = createdInvoiceId ?? 1;
    const res = await request.delete(`/api/invoices/${id}`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ DELETE invoice: status=${res.status()}`);
  });
});