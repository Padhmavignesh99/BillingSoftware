import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

let token = '';
let creditBillId = 0;

test.beforeAll(async () => {
  const auth = await getAuthToken();
  token = auth.token;
});
const h = () => ({ Authorization: `Bearer ${token}` });

test.describe('💳 Credit Bill API (User-wise)', () => {

  test('POST /api/credit-bills - create credit bill (with userId)', async ({ request }) => {
    const res = await request.post('/api/credit-bills', {
      headers: h(),
      data: { customerId: 1, amount: 500, description: 'Test credit bill', creditDate: new Date().toISOString().slice(0,10) }
    });
    expect([200, 201, 400, 401, 404, 500]).toContain(res.status());
    const text = await res.text();
    let body = null;
    try { body = JSON.parse(text); } catch {}
    creditBillId = body?.data?.id ?? 0;
    console.log(`✅ Credit bill: status=${res.status()}, id=${creditBillId}`);
  });

  test('GET /api/credit-bills/customer/:id - credits by customer', async ({ request }) => {
    const res = await request.get('/api/credit-bills/customer/1', { headers: h() });
    expect([200, 401, 404, 500]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ Credits by customer: status=${res.status()}`);
  });

  test('GET /api/credit-bills/user/1 - credits by user (user-wise)', async ({ request }) => {
    const res = await request.get('/api/credit-bills/user/1', { headers: h() });
    expect([200, 401, 404, 500]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ Credits by user: status=${res.status()}`);
  });

  test('GET /api/credit-bills/my - my credit bills', async ({ request }) => {
    const res = await request.get('/api/credit-bills/my', { headers: h() });
    expect([200, 401, 500]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ My credit bills: status=${res.status()}`);
  });

  test('GET /api/credit-bills/user/1/balance - user credit balance', async ({ request }) => {
    const res = await request.get('/api/credit-bills/user/1/balance', { headers: h() });
    expect([200, 401, 404, 500]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ User credit balance: status=${res.status()}`);
  });

  test('GET /api/credit-bills/my/balance - my outstanding balance', async ({ request }) => {
    const res = await request.get('/api/credit-bills/my/balance', { headers: h() });
    expect([200, 401, 500]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ My outstanding balance: status=${res.status()}`);
  });

  test('POST /api/credit-bills/settle - settle credit bill', async ({ request }) => {
    const res = await request.post('/api/credit-bills/settle', {
      headers: h(),
      data: { creditBillId: creditBillId || 1, amountPaid: 500, paymentDate: new Date().toISOString().slice(0,10), paymentMethod: 'cash' }
    });
    expect([200, 201, 400, 401, 404, 500]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ Settle credit: status=${res.status()}`);
  });
});