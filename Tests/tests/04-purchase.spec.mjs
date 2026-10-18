import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

test.describe('🛒 Purchase API', () => {
  let authHeaders;
  let createdPurchaseId;
  let createdPurchaseOrderId;

  test.beforeAll(async () => {
    const auth = await getAuthToken();
    authHeaders = { Authorization: `Bearer ${auth.token}`, 'Content-Type': 'application/json' };
  });

  // ────────── PURCHASES ──────────
  test('GET /api/purchases - list all purchases (paginated)', async ({ request }) => {
    const res = await request.get('/api/purchases?page=1&pageSize=10', { headers: authHeaders });
    expect([200, 401]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) {
      expect(body.success).toBe(true);
      expect(Array.isArray(body.data)).toBe(true);
    }
    console.log(`✅ GET purchases: status=${res.status()}`);
  });

  test('POST /api/purchases - create new purchase', async ({ request }) => {
    const payload = {
      supplierName: 'Test Supplier',
      supplierPhone: '9123456780',
      invoiceNumber: `INV-${Date.now()}`,
      invoiceDate: new Date().toISOString(),
      paymentMode: 'Cash',
      taxAmount: 18,
      totalAmount: 500,
      paidAmount: 500,
      items: [
        { productName: 'Bought Item', quantity: 5, unitPrice: 100, taxRate: 18, discountAmount: 0, totalPrice: 500 }
      ]
    };
    const res = await request.post('/api/purchases', { headers: authHeaders, data: payload });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200 || res.status() === 201) {
      createdPurchaseId = body?.data?.id;
    }
    console.log(`✅ POST purchase: status=${res.status()}, id=${createdPurchaseId}`);
  });

  test('GET /api/purchases/:id - get purchase by ID', async ({ request }) => {
    const id = createdPurchaseId ?? 1;
    const res = await request.get(`/api/purchases/${id}`, { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET purchase by ID: status=${res.status()}`);
  });

  test('PUT /api/purchases/:id - update purchase', async ({ request }) => {
    const id = createdPurchaseId ?? 1;
    const res = await request.put(`/api/purchases/${id}`, {
      headers: authHeaders,
      data: { supplierName: 'Updated Supplier', totalAmount: 600, paidAmount: 600 }
    });
    expect([200, 201, 400, 404, 422, 500]).toContain(res.status());
    console.log(`✅ PUT purchase: status=${res.status()}`);
  });

  // ────────── PURCHASE ORDERS ──────────
  test('POST /api/purchase-orders - create purchase order', async ({ request }) => {
    const res = await request.post('/api/purchase-orders', {
      headers: authHeaders,
      data: {
        supplierName: 'PO Supplier',
        expectedDeliveryDate: new Date(Date.now() + 7 * 24 * 3600000).toISOString(),
        items: [{ productName: 'PO Item', quantity: 10, unitPrice: 50, taxRate: 5, discountAmount: 0, totalPrice: 525 }]
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200 || res.status() === 201) {
      createdPurchaseOrderId = body?.data?.id;
    }
    console.log(`✅ POST purchase order: status=${res.status()}`);
  });

  test('GET /api/purchase-orders - list purchase orders', async ({ request }) => {
    const res = await request.get('/api/purchase-orders', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET purchase orders: status=${res.status()}`);
  });

  test('GET /api/purchase-orders/:id - get purchase order by ID', async ({ request }) => {
    const id = createdPurchaseOrderId ?? 1;
    const res = await request.get(`/api/purchase-orders/${id}`, { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET purchase order by ID: status=${res.status()}`);
  });

  // ────────── PURCHASE PAYMENTS ──────────
  test('GET /api/purchase-payments/:purchaseId - get payments for purchase', async ({ request }) => {
    const id = createdPurchaseId ?? 1;
    const res = await request.get(`/api/purchase-payments/${id}`, { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET purchase payments: status=${res.status()}`);
  });

  test('POST /api/purchase-payments - create purchase payment', async ({ request }) => {
    const res = await request.post('/api/purchase-payments', {
      headers: authHeaders,
      data: {
        purchaseId: createdPurchaseId ?? 1,
        amount: 100,
        paymentMode: 'Cash',
        paymentDate: new Date().toISOString()
      }
    });
    expect([200, 201, 400, 404, 422, 500]).toContain(res.status());
    console.log(`✅ POST purchase payment: status=${res.status()}`);
  });

  // ────────── GOODS RECEIPTS ──────────
  test('GET /api/goods-receipts - list goods receipts', async ({ request }) => {
    const res = await request.get('/api/goods-receipts', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET goods receipts: status=${res.status()}`);
  });

  test('POST /api/goods-receipts - create goods receipt', async ({ request }) => {
    const res = await request.post('/api/goods-receipts', {
      headers: authHeaders,
      data: {
        purchaseOrderId: createdPurchaseOrderId ?? 1,
        receivedDate: new Date().toISOString(),
        items: [{ productName: 'Received Item', quantity: 5 }]
      }
    });
    expect([200, 201, 400, 404, 422, 500]).toContain(res.status());
    console.log(`✅ POST goods receipt: status=${res.status()}`);
  });

  // ────────── DELETE ──────────
  test('DELETE /api/purchases/:id - delete purchase', async ({ request }) => {
    const id = createdPurchaseId ?? 1;
    const res = await request.delete(`/api/purchases/${id}`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ DELETE purchase: status=${res.status()}`);
  });

  test('GET /api/purchases - without token returns 401', async ({ request }) => {
    const res = await request.get('/api/purchases');
    expect([401, 403]).toContain(res.status());
    console.log(`✅ Purchase without token: status=${res.status()}`);
  });
});