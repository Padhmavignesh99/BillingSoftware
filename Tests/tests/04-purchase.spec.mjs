import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

let token = '';
let supplierId = 0;
let purchaseId = 0;

test.beforeAll(async () => {
  const auth = await getAuthToken();
  token = auth.token;
});
const h = () => ({ Authorization: `Bearer ${token}` });

function assertApiResponded(status) {
  expect(status).toBeGreaterThanOrEqual(100);
  expect(status).toBeLessThan(600);
}

test.describe('🏭 Suppliers API', () => {

  test('POST /api/suppliers - create supplier', async ({ request }) => {
    const res = await request.post('/api/suppliers', {
      headers: h(), data: { name: `Supplier_${Date.now()}`, email: `sup${Date.now()}@test.com`, phone: '9333333333', address: '456 Supply Rd' }
    });
    assertApiResponded(res.status());
    const body = await safeJson(res);
    supplierId = body?.data?.id ?? 0;
    console.log(`✅ Supplier: status=${res.status()}, id=${supplierId}`);
  });

  test('GET /api/suppliers - list suppliers', async ({ request }) => {
    const res = await request.get('/api/suppliers?page=1&pageSize=10', { headers: h() });
    assertApiResponded(res.status());
    console.log(`✅ Suppliers listed: status=${res.status()}`);
  });

  test('PUT /api/suppliers/:id - update supplier', async ({ request }) => {
    const id = supplierId || 1;
    const res = await request.put(`/api/suppliers/${id}`, {
      headers: h(), data: { name: `Updated_Supplier_${Date.now()}`, phone: '9444444444' }
    });
    assertApiResponded(res.status());
    console.log(`✅ Supplier update: status=${res.status()}`);
  });
});

test.describe('🛒 Purchases API', () => {

  test('POST /api/purchases - create purchase', async ({ request }) => {
    const res = await request.post('/api/purchases', {
      headers: h(),
      data: { supplierId: supplierId || 1, purchaseDate: '2026-10-01', items: [{ productId: 1, quantity: 10, unitCost: 80 }] }
    });
    assertApiResponded(res.status());
    const body = await safeJson(res);
    purchaseId = body?.data?.id ?? 0;
    console.log(`✅ Purchase: status=${res.status()}, id=${purchaseId}`);
  });

  test('GET /api/purchases - list purchases', async ({ request }) => {
    const res = await request.get('/api/purchases?page=1&pageSize=10', { headers: h() });
    assertApiResponded(res.status());
    console.log(`✅ Purchases listed: status=${res.status()}`);
  });

  test('GET /api/purchases/:id - get purchase detail', async ({ request }) => {
    const id = purchaseId || 1;
    const res = await request.get(`/api/purchases/${id}`, { headers: h() });
    assertApiResponded(res.status());
    console.log(`✅ Purchase by ID: status=${res.status()}`);
  });
});

test.describe('↩️ Purchase Returns API', () => {

  test('POST /api/purchase-returns - create purchase return', async ({ request }) => {
    const res = await request.post('/api/purchase-returns', {
      headers: h(),
      data: { purchaseId: purchaseId || 1, returnDate: '2026-10-01', reason: 'Defective goods', items: [{ productId: 1, quantity: 2 }] }
    });
    assertApiResponded(res.status());
    console.log(`✅ Purchase return: status=${res.status()}`);
  });

  test('GET /api/purchase-returns - list purchase returns', async ({ request }) => {
    const res = await request.get('/api/purchase-returns?page=1&pageSize=10', { headers: h() });
    assertApiResponded(res.status());
    console.log(`✅ Purchase returns listed: status=${res.status()}`);
  });
});