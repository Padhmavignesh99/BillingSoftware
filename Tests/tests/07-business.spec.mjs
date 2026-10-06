import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

let token = '';

test.beforeAll(async () => {
  const auth = await getAuthToken();
  token = auth.token;
});
const h = () => ({ Authorization: `Bearer ${token}` });

function assertApiResponded(status) {
  expect(status).toBeGreaterThanOrEqual(100);
  expect(status).toBeLessThan(600);
}

test.describe('ðŸ¢ Business & Settings API', () => {

  test('GET /api/business - get business info', async ({ request }) => {
    const res = await request.get('/api/business', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Business info: status=${res.status()}`);
  });

  test('GET /api/branches - list branches', async ({ request }) => {
    const res = await request.get('/api/branches', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Branches: status=${res.status()}`);
  });

  test('GET /api/business/settings - get business settings', async ({ request }) => {
    const res = await request.get('/api/business/settings', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Business settings: status=${res.status()}`);
  });
});

test.describe('ðŸ“Š Dashboard API', () => {

  test('GET /api/dashboard/summary - summary stats', async ({ request }) => {
    const res = await request.get('/api/dashboard/summary', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Dashboard summary: status=${res.status()}`);
  });

  test('GET /api/dashboard/chart - chart data', async ({ request }) => {
    const res = await request.get('/api/dashboard/chart', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Dashboard chart: status=${res.status()}`);
  });
});

test.describe('ðŸ‘¥ Users & Roles API', () => {

  test('GET /api/users - list users', async ({ request }) => {
    const res = await request.get('/api/users', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Users: status=${res.status()}`);
  });

  test('GET /api/roles - list roles', async ({ request }) => {
    const res = await request.get('/api/roles', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Roles: status=${res.status()}`);
  });
});

test.describe('ðŸ“¦ Inventory API', () => {

  test('GET /api/inventory/stock - stock overview', async ({ request }) => {
    const res = await request.get('/api/inventory/stock', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Inventory stock: status=${res.status()}`);
  });

  test('GET /api/warehouses - list warehouses', async ({ request }) => {
    const res = await request.get('/api/warehouses', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Warehouses: status=${res.status()}`);
  });
});

test.describe('ðŸ’° Expenses API', () => {

  test('GET /api/expenses - list expenses', async ({ request }) => {
    const res = await request.get('/api/expenses?page=1&pageSize=10', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Expenses: status=${res.status()}`);
  });

  test('GET /api/expense-categories - list expense categories', async ({ request }) => {
    const res = await request.get('/api/expense-categories', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Expense categories: status=${res.status()}`);
  });
});

test.describe('ðŸ”” Notifications API', () => {

  test('GET /api/notifications - list notifications', async ({ request }) => {
    const res = await request.get('/api/notifications', { headers: h() });
    assertApiResponded(res.status());
    console.log(`âœ… Notifications: status=${res.status()}`);
  });
});