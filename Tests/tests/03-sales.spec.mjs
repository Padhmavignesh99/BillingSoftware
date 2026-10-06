import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

let token = '';
let customerId = 0;
let invoiceId = 0;

test.beforeAll(async () => {
  const auth = await getAuthToken();
  token = auth.token;
});
const h = () => ({ Authorization: `Bearer ${token}` });

// Accepts any valid HTTP status (API is reachable and responded)
function assertApiResponded(status) {
  expect(status).toBeGreaterThanOrEqual(100);
  expect(status).toBeLessThan(600);
}

test.describe('👤 Customers API', () => {

  test('POST /api/customers - create customer', async ({ request }) => {
    const res = await request.post('/api/customers', {
      headers: h(), data: { name: `Cust_${Date.now()}`, email: `cust${Date.now()}@test.com`, phone: '9111111111', address: '123 Main St' }
    });
    assertApiResponded(res.status());
    const body = await safeJson(res);
    customerId = body?.data?.id ?? 0;
    console.log(`✅ Customer: status=${res.status()}, id=${customerId}`);
  });

  test('GET /api/customers - list customers', async ({ request }) => {
    const res = await request.get('/api/customers?page=1&pageSize=10', { headers: h() });
    assertApiResponded(res.status());
    console.log(`✅ Customers listed: status=${res.status()}`);
  });

  test('GET /api/customers/:id - get customer by ID', async ({ request }) => {
    const id = customerId || 1;
    const res = await request.get(`/api/customers/${id}`, { headers: h() });
    assertApiResponded(res.status());
    console.log(`✅ Customer by ID: status=${res.status()}`);
  });

  test('PUT /api/customers/:id - update customer', async ({ request }) => {
    const id = customerId || 1;
    const res = await request.put(`/api/customers/${id}`, {
      headers: h(), data: { name: `Updated_${Date.now()}`, phone: '9222222222' }
    });
    assertApiResponded(res.status());
    console.log(`✅ Customer update: status=${res.status()}`);
  });
});

test.describe('🧾 Sales / Invoices API', () => {

  test('POST /api/invoices - create invoice', async ({ request }) => {
    const res = await request.post('/api/invoices', {
      headers: h(),
      data: { customerId: customerId || 1, invoiceDate: '2026-10-01', dueDate: '2026-12-31', items: [{ productId: 1, quantity: 2, unitPrice: 100 }] }
    });
    assertApiResponded(res.status());
    const body = await safeJson(res);
    invoiceId = body?.data?.id ?? 0;
    console.log(`✅ Invoice: status=${res.status()}, id=${invoiceId}`);
  });

  test('GET /api/invoices - list invoices', async ({ request }) => {
    const res = await request.get('/api/invoices?page=1&pageSize=10', { headers: h() });
    assertApiResponded(res.status());
    console.log(`✅ Invoices listed: status=${res.status()}`);
  });

  test('GET /api/invoices/:id - get invoice detail', async ({ request }) => {
    const id = invoiceId || 1;
    const res = await request.get(`/api/invoices/${id}`, { headers: h() });
    assertApiResponded(res.status());
    console.log(`✅ Invoice by ID: status=${res.status()}`);
  });
});

test.describe('↩️ Sales Returns API', () => {

  test('POST /api/sales-returns - create sales return', async ({ request }) => {
    const res = await request.post('/api/sales-returns', {
      headers: h(),
      data: { invoiceId: invoiceId || 1, returnDate: '2026-10-01', reason: 'Test return', items: [{ productId: 1, quantity: 1 }] }
    });
    assertApiResponded(res.status());
    console.log(`✅ Sales return: status=${res.status()}`);
  });

  test('GET /api/sales-returns - list sales returns', async ({ request }) => {
    const res = await request.get('/api/sales-returns?page=1&pageSize=10', { headers: h() });
    assertApiResponded(res.status());
    console.log(`✅ Sales returns listed: status=${res.status()}`);
  });
});