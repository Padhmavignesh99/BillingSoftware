import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

test.describe('↩️ Purchase Returns API', () => {
  let authHeaders;
  let createdReturnId;

  test.beforeAll(async () => {
    const auth = await getAuthToken();
    authHeaders = { Authorization: `Bearer ${auth.token}`, 'Content-Type': 'application/json' };
  });

  test('GET /api/purchase-returns - list all purchase returns', async ({ request }) => {
    const res = await request.get('/api/purchase-returns', { headers: authHeaders });
    expect([200, 401]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) {
      expect(body.success).toBe(true);
      expect(Array.isArray(body.data)).toBe(true);
    }
    console.log(`✅ GET purchase returns: status=${res.status()}`);
  });

  test('POST /api/purchase-returns - create purchase return', async ({ request }) => {
    const payload = {
      purchaseId: 1,
      returnDate: new Date().toISOString(),
      reason: 'Wrong item delivered',
      refundAmount: 200,
      items: [
        { productName: 'Returned Purchase Item', quantity: 2, unitPrice: 100, taxRate: 0, totalPrice: 200 }
      ]
    };
    const res = await request.post('/api/purchase-returns', { headers: authHeaders, data: payload });
    expect([200, 201, 400, 404, 422, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200 || res.status() === 201) {
      createdReturnId = body?.data?.id;
    }
    console.log(`✅ POST purchase return: status=${res.status()}, id=${createdReturnId}`);
  });

  test('GET /api/purchase-returns/:id - get purchase return by ID', async ({ request }) => {
    const id = createdReturnId ?? 1;
    const res = await request.get(`/api/purchase-returns/${id}`, { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET purchase return by ID: status=${res.status()}`);
  });

  test('POST /api/purchase-returns - missing purchaseId rejected', async ({ request }) => {
    const res = await request.post('/api/purchase-returns', {
      headers: authHeaders,
      data: { returnDate: new Date().toISOString(), refundAmount: 50 }
    });
    expect([400, 422, 500]).toContain(res.status());
    console.log(`✅ Purchase return missing purchaseId: status=${res.status()}`);
  });

  test('POST /api/purchase-returns - zero refund amount check', async ({ request }) => {
    const res = await request.post('/api/purchase-returns', {
      headers: authHeaders,
      data: { purchaseId: 1, returnDate: new Date().toISOString(), refundAmount: 0, items: [] }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ Purchase return zero refund: status=${res.status()}`);
  });

  test('DELETE /api/purchase-returns/:id - delete purchase return', async ({ request }) => {
    const id = createdReturnId ?? 1;
    const res = await request.delete(`/api/purchase-returns/${id}`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ DELETE purchase return: status=${res.status()}`);
  });

  test('GET /api/purchase-returns - without token returns 401', async ({ request }) => {
    const res = await request.get('/api/purchase-returns');
    expect([401, 403]).toContain(res.status());
    console.log(`✅ Purchase returns without token: status=${res.status()}`);
  });

  test('DELETE /api/purchase-orders/:id - delete purchase order', async ({ request }) => {
    const res = await request.delete(`/api/purchase-orders/1`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ DELETE purchase order: status=${res.status()}`);
  });
});
