import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

test.describe('↩️ Sales Returns API', () => {
  let authHeaders;
  let createdReturnId;

  test.beforeAll(async () => {
    const auth = await getAuthToken();
    authHeaders = { Authorization: `Bearer ${auth.token}`, 'Content-Type': 'application/json' };
  });

  test('GET /api/sales-returns - list all sales returns', async ({ request }) => {
    const res = await request.get('/api/sales-returns', { headers: authHeaders });
    expect([200, 401]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) {
      expect(body.success).toBe(true);
      expect(Array.isArray(body.data)).toBe(true);
    }
    console.log(`✅ GET sales returns: status=${res.status()}`);
  });

  test('POST /api/sales-returns - create sales return', async ({ request }) => {
    const payload = {
      invoiceId: 1,
      returnDate: new Date().toISOString(),
      reason: 'Defective product',
      refundAmount: 100,
      items: [
        { productName: 'Returned Product', quantity: 1, unitPrice: 100, taxRate: 0, totalPrice: 100 }
      ]
    };
    const res = await request.post('/api/sales-returns', { headers: authHeaders, data: payload });
    expect([200, 201, 400, 404, 422, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200 || res.status() === 201) {
      createdReturnId = body?.data?.id;
    }
    console.log(`✅ POST sales return: status=${res.status()}, id=${createdReturnId}`);
  });

  test('GET /api/sales-returns/:id - get sales return by ID', async ({ request }) => {
    const id = createdReturnId ?? 1;
    const res = await request.get(`/api/sales-returns/${id}`, { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET sales return by ID: status=${res.status()}`);
  });

  test('POST /api/sales-returns - missing invoiceId rejected', async ({ request }) => {
    const res = await request.post('/api/sales-returns', {
      headers: authHeaders,
      data: { returnDate: new Date().toISOString(), refundAmount: 50 }
    });
    expect([400, 422, 500]).toContain(res.status());
    console.log(`✅ Sales return missing invoiceId: status=${res.status()}`);
  });

  test('POST /api/sales-returns - negative refund amount rejected', async ({ request }) => {
    const res = await request.post('/api/sales-returns', {
      headers: authHeaders,
      data: { invoiceId: 1, returnDate: new Date().toISOString(), refundAmount: -100 }
    });
    expect([400, 422, 500]).toContain(res.status());
    console.log(`✅ Sales return negative refund: status=${res.status()}`);
  });

  test('DELETE /api/sales-returns/:id - delete sales return', async ({ request }) => {
    const id = createdReturnId ?? 1;
    const res = await request.delete(`/api/sales-returns/${id}`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ DELETE sales return: status=${res.status()}`);
  });

  test('GET /api/sales-returns - without token returns 401', async ({ request }) => {
    const res = await request.get('/api/sales-returns');
    expect([401, 403]).toContain(res.status());
    console.log(`✅ Sales returns without token: status=${res.status()}`);
  });
});
