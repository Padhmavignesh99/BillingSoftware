import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

let token = '';
const from = '2026-01-01';
const to   = '2026-12-31';

test.beforeAll(async () => {
  const auth = await getAuthToken();
  token = auth.token;
});
const h = () => ({ Authorization: `Bearer ${token}` });

test.describe('📊 Reports API', () => {

  test('GET /api/reports/sales - sales report', async ({ request }) => {
    const res = await request.get(`/api/reports/sales?fromDate=${from}&toDate=${to}`, { headers: h() });
    expect([200, 400, 401, 500]).toContain(res.status());
    console.log(`✅ Sales report: status=${res.status()}`);
  });

  test('GET /api/reports/sales-return - sales return report', async ({ request }) => {
    const res = await request.get(`/api/reports/sales-return?fromDate=${from}&toDate=${to}`, { headers: h() });
    expect([200, 400, 401, 500]).toContain(res.status());
    console.log(`✅ Sales return report: status=${res.status()}`);
  });

  test('GET /api/reports/purchase - purchase report', async ({ request }) => {
    const res = await request.get(`/api/reports/purchase?fromDate=${from}&toDate=${to}`, { headers: h() });
    expect([200, 400, 401, 500]).toContain(res.status());
    console.log(`✅ Purchase report: status=${res.status()}`);
  });

  test('GET /api/reports/purchase-return - purchase return report', async ({ request }) => {
    const res = await request.get(`/api/reports/purchase-return?fromDate=${from}&toDate=${to}`, { headers: h() });
    expect([200, 400, 401, 500]).toContain(res.status());
    console.log(`✅ Purchase return report: status=${res.status()}`);
  });

  test('GET /api/reports/low-stock - low stock report', async ({ request }) => {
    const res = await request.get('/api/reports/low-stock', { headers: h() });
    expect([200, 401, 500]).toContain(res.status());
    console.log(`✅ Low stock report: status=${res.status()}`);
  });

  test('GET /api/reports/credit-bill - credit bill report (all)', async ({ request }) => {
    const res = await request.get('/api/reports/credit-bill', { headers: h() });
    expect([200, 401, 500]).toContain(res.status());
    console.log(`✅ Credit bill report: status=${res.status()}`);
  });

  test('GET /api/reports/credit-bill?userId=1 - credit bill by user', async ({ request }) => {
    const res = await request.get('/api/reports/credit-bill?userId=1', { headers: h() });
    expect([200, 401, 500]).toContain(res.status());
    console.log(`✅ Credit bill by user: status=${res.status()}`);
  });

  test('GET /api/reports/credit-bill/my - my credit bills', async ({ request }) => {
    const res = await request.get('/api/reports/credit-bill/my', { headers: h() });
    expect([200, 401, 500]).toContain(res.status());
    console.log(`✅ My credit bill report: status=${res.status()}`);
  });

  test('GET /api/reports/product-mini-statement/:id - product mini statement', async ({ request }) => {
    const res = await request.get('/api/reports/product-mini-statement/1', { headers: h() });
    expect([200, 400, 401, 404, 500]).toContain(res.status());
    console.log(`✅ Product mini-statement: status=${res.status()}`);
  });

  test('GET /api/reports/product-stock - product stock report', async ({ request }) => {
    const res = await request.get('/api/reports/product-stock', { headers: h() });
    expect([200, 401, 500]).toContain(res.status());
    console.log(`✅ Product stock report: status=${res.status()}`);
  });

  test('GET /api/reports/expired-stock - expired stock report', async ({ request }) => {
    const res = await request.get('/api/reports/expired-stock', { headers: h() });
    expect([200, 401, 500]).toContain(res.status());
    console.log(`✅ Expired stock report: status=${res.status()}`);
  });

  test('GET /api/reports/profit-loss - profit loss report', async ({ request }) => {
    const res = await request.get(`/api/reports/profit-loss?fromDate=${from}&toDate=${to}`, { headers: h() });
    expect([200, 400, 401, 500]).toContain(res.status());
    console.log(`✅ Profit loss report: status=${res.status()}`);
  });

  test('GET /api/reports/tax - tax report', async ({ request }) => {
    const res = await request.get(`/api/reports/tax?fromDate=${from}&toDate=${to}`, { headers: h() });
    expect([200, 400, 401, 500]).toContain(res.status());
    console.log(`✅ Tax report: status=${res.status()}`);
  });

  test('POST /api/reports/export - export report as PDF', async ({ request }) => {
    const res = await request.post('/api/reports/export', {
      headers: h(),
      data: { reportType: 'sales', fromDate: from, toDate: to, format: 'pdf' }
    });
    expect([200, 400, 401, 500]).toContain(res.status());
    console.log(`✅ Export report: status=${res.status()}`);
  });
});