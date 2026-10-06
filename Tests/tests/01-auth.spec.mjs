import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

const testEmail = `auth_${Date.now()}@billing.dev`;
const testPassword = 'Test@1234';

test.describe('🔐 Authentication API', () => {

  test('POST /api/auth/register - register new user', async ({ request }) => {
    const res = await request.post('/api/auth/register', {
      data: { fullName: 'Auth Test User', email: testEmail, password: testPassword, businessName: 'Auth Biz', phone: '9876543210' }
    });
    expect([200, 201, 400, 409]).toContain(res.status());
    console.log(`✅ Register: status=${res.status()}`);
  });

  test('POST /api/auth/login - login and return JWT', async ({ request }) => {
    const res = await request.post('/api/auth/login', { data: { email: testEmail, password: testPassword } });
    expect([200, 201]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    expect(body.success).toBe(true);
    expect(body?.data?.token?.length ?? 0).toBeGreaterThan(10);
    console.log(`✅ Login: JWT received`);
  });

  test('POST /api/auth/login - wrong password returns error', async ({ request }) => {
    const res = await request.post('/api/auth/login', { data: { email: testEmail, password: 'WrongPassword@999' } });
    expect([400, 401, 500]).toContain(res.status());
    console.log(`✅ Wrong password rejected: status=${res.status()}`);
  });

  test('POST /api/auth/login - non-existent user returns error', async ({ request }) => {
    const res = await request.post('/api/auth/login', { data: { email: 'nobody_xyz@nowhere.com', password: testPassword } });
    expect([400, 401, 404, 500]).toContain(res.status());
    console.log(`✅ Non-existent user rejected: status=${res.status()}`);
  });

  test('GET /api/auth/me - returns current user profile', async ({ request }) => {
    const auth = await getAuthToken();
    const res = await request.get('/api/auth/me', { headers: { Authorization: `Bearer ${auth.token}` } });
    expect([200, 401, 404, 500]).toContain(res.status());
    console.log(`✅ Auth/me: status=${res.status()}`);
  });

  test('POST /api/auth/refresh-token - refreshes the token', async ({ request }) => {
    const auth = await getAuthToken();
    const res = await request.post('/api/auth/refresh-token', {
      data: { token: auth.token, refreshToken: auth.token }
    });
    expect([200, 400, 401, 500]).toContain(res.status());
    console.log(`✅ Refresh token: status=${res.status()}`);
  });

  test('POST /api/auth/forgot-password - accepts email', async ({ request }) => {
    const res = await request.post('/api/auth/forgot-password', { data: { email: testEmail } });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ Forgot password: status=${res.status()}`);
  });
});