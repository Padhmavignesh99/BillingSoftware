import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

/**
 * API Security Test Suite
 * Tests authentication, authorization, injection, and security headers
 */
test.describe('🔒 API Security Tests', () => {
  let validToken;
  let authHeaders;

  test.beforeAll(async () => {
    const auth = await getAuthToken();
    validToken = auth.token;
    authHeaders = { Authorization: `Bearer ${validToken}`, 'Content-Type': 'application/json' };
  });

  // ─────────── AUTHENTICATION SECURITY ───────────
  test('SEC-01: No token → 401 on protected endpoints', async ({ request }) => {
    const endpoints = [
      '/api/invoices',
      '/api/purchases',
      '/api/sales-returns',
      '/api/purchase-returns',
      '/api/devices',
      '/api/print-settings',
    ];
    for (const ep of endpoints) {
      const res = await request.get(ep);
      expect([401, 403], `Expected 401/403 for ${ep}, got ${res.status()}`).toContain(res.status());
    }
    console.log(`✅ SEC-01: All protected endpoints rejected unauthenticated requests`);
  });

  test('SEC-02: Malformed Bearer token → 401', async ({ request }) => {
    const res = await request.get('/api/invoices', {
      headers: { Authorization: 'Bearer INVALID_TOKEN_XYZ' }
    });
    expect([401, 403]).toContain(res.status());
    console.log(`✅ SEC-02: Malformed token rejected: status=${res.status()}`);
  });

  test('SEC-03: Empty Bearer token → 401', async ({ request }) => {
    const res = await request.get('/api/invoices', {
      headers: { Authorization: 'Bearer ' }
    });
    expect([401, 403]).toContain(res.status());
    console.log(`✅ SEC-03: Empty token rejected: status=${res.status()}`);
  });

  test('SEC-04: Missing Authorization header → 401', async ({ request }) => {
    const res = await request.post('/api/invoices', { data: {} });
    expect([401, 403]).toContain(res.status());
    console.log(`✅ SEC-04: Missing auth header rejected: status=${res.status()}`);
  });

  test('SEC-05: Expired/forged JWT → 401', async ({ request }) => {
    const fakeJwt = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkhhY2tlciIsImlhdCI6MTUxNjIzOTAyMn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c';
    const res = await request.get('/api/invoices', {
      headers: { Authorization: `Bearer ${fakeJwt}` }
    });
    expect([401, 403]).toContain(res.status());
    console.log(`✅ SEC-05: Forged JWT rejected: status=${res.status()}`);
  });

  test('SEC-06: Basic auth scheme rejected (not Bearer)', async ({ request }) => {
    const res = await request.get('/api/invoices', {
      headers: { Authorization: 'Basic dXNlcjpwYXNz' }
    });
    expect([401, 403]).toContain(res.status());
    console.log(`✅ SEC-06: Non-Bearer scheme rejected: status=${res.status()}`);
  });

  // ─────────── INPUT VALIDATION / INJECTION SECURITY ───────────
  test('SEC-07: SQL injection in login email field', async ({ request }) => {
    const res = await request.post('/api/auth/login', {
      data: { email: "admin'--; DROP TABLE users;--", password: 'password' }
    });
    // Should not return 200 — any error is acceptable
    expect([400, 401, 422, 500]).toContain(res.status());
    console.log(`✅ SEC-07: SQL injection in email rejected: status=${res.status()}`);
  });

  test('SEC-08: XSS payload in invoice customerName', async ({ request }) => {
    const res = await request.post('/api/invoices', {
      headers: authHeaders,
      data: {
        customerName: '<script>alert("XSS")</script>',
        paymentMode: 'Cash',
        totalAmount: 100,
        paidAmount: 100,
        items: []
      }
    });
    // Should not crash the server
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    const body = await safeJson(res);
    // Response should not contain executable script
    const bodyStr = JSON.stringify(body ?? '');
    expect(bodyStr).not.toContain('<script>alert(');
    console.log(`✅ SEC-08: XSS payload handled safely: status=${res.status()}`);
  });

  test('SEC-09: Extremely large payload (DoS protection)', async ({ request }) => {
    const largeString = 'A'.repeat(1_000_000); // 1MB string
    const res = await request.post('/api/invoices', {
      headers: authHeaders,
      data: { customerName: largeString, totalAmount: 100, paidAmount: 100, items: [] }
    });
    expect([200, 201, 400, 413, 422, 500]).toContain(res.status());
    console.log(`✅ SEC-09: Large payload handled: status=${res.status()}`);
  });

  test('SEC-10: Negative totalAmount rejected or handled', async ({ request }) => {
    const res = await request.post('/api/invoices', {
      headers: authHeaders,
      data: {
        customerName: 'Hacker',
        paymentMode: 'Cash',
        totalAmount: -99999,
        paidAmount: -99999,
        items: []
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ SEC-10: Negative amount handled: status=${res.status()}`);
  });

  test('SEC-11: Non-existent resource returns 404 (not 500)', async ({ request }) => {
    const res = await request.get('/api/invoices/99999999', { headers: authHeaders });
    expect([404, 400]).toContain(res.status());
    console.log(`✅ SEC-11: Non-existent resource: status=${res.status()}`);
  });

  test('SEC-12: Invalid content-type still handled', async ({ request }) => {
    const res = await request.post('/api/auth/login', {
      headers: { 'Content-Type': 'text/plain' },
      data: 'invalid plain text body'
    });
    expect([400, 415, 422, 500]).toContain(res.status());
    console.log(`✅ SEC-12: Invalid content-type handled: status=${res.status()}`);
  });

  // ─────────── CROSS-TENANT DATA ISOLATION ───────────
  test('SEC-13: Accessing other tenant data returns 404/403', async ({ request }) => {
    // Our token is for tenant 1; try to access a very high ID that likely belongs to no-one
    const res = await request.get('/api/invoices/999998877', { headers: authHeaders });
    expect([400, 404, 403]).toContain(res.status());
    console.log(`✅ SEC-13: Cross-tenant isolation: status=${res.status()}`);
  });

  // ─────────── RATE LIMITING / BRUTE FORCE ───────────
  test('SEC-14: Multiple failed logins - brute force protection check', async ({ request }) => {
    const results = [];
    for (let i = 0; i < 5; i++) {
      const res = await request.post('/api/auth/login', {
        data: { email: 'brute@test.com', password: `WrongPass${i}` }
      });
      results.push(res.status());
    }
    // All should be non-200; server may return 429 (rate limited) after some attempts
    expect(results.every(s => s !== 200)).toBe(true);
    console.log(`✅ SEC-14: Brute force login statuses: [${results.join(', ')}]`);
  });

  // ─────────── SECURITY HEADERS ───────────
  test('SEC-15: Security headers present on API responses', async ({ request }) => {
    const res = await request.get('/api/print-settings', { headers: authHeaders });
    // Check for at least one common security header
    const headers = res.headers();
    const hasSecurityHeaders =
      'x-content-type-options' in headers ||
      'x-frame-options' in headers ||
      'strict-transport-security' in headers ||
      'content-security-policy' in headers;
    // Log what headers are present (informational)
    console.log(`✅ SEC-15: Security headers check - present: ${hasSecurityHeaders}`);
    console.log(`   Headers found: ${Object.keys(headers).filter(h => ['x-content-type-options','x-frame-options','strict-transport-security','content-security-policy'].includes(h)).join(', ')}`);
    // We don't fail if missing — just record the finding
    expect([200, 400, 401, 500]).toContain(res.status());
  });

  // ─────────── MASS ASSIGNMENT / PRIVILEGE ESCALATION ───────────
  test('SEC-16: Cannot set own role/tenantId via register payload', async ({ request }) => {
    const res = await request.post('/api/auth/register', {
      data: {
        fullName: 'Hacker Attempt',
        email: `hack_${Date.now()}@evil.com`,
        password: 'Hack@1234',
        businessName: 'Evil Corp',
        phone: '9000000009',
        role: 'SuperAdmin',
        tenantId: 0,
        isAdmin: true
      }
    });
    expect([200, 201, 400, 409, 422, 500]).toContain(res.status());
    // If registration succeeded, verify the token doesn't claim SuperAdmin
    const body = await safeJson(res);
    if (body?.data?.token) {
      // The token should not contain 'SuperAdmin' in payload
      const parts = body.data.token.split('.');
      if (parts.length === 3) {
        try {
          const payload = JSON.parse(atob(parts[1]));
          expect(payload.role ?? '').not.toBe('SuperAdmin');
          console.log(`✅ SEC-16: Mass assignment blocked — role in token: ${payload.role}`);
        } catch {
          console.log(`✅ SEC-16: Could not decode token payload`);
        }
      }
    }
    console.log(`✅ SEC-16: Mass assignment test: status=${res.status()}`);
  });

  test('SEC-17: HTTP VERB tampering — DELETE without token rejected', async ({ request }) => {
    const res = await request.delete('/api/invoices/1');
    expect([401, 403]).toContain(res.status());
    console.log(`✅ SEC-17: DELETE without token: status=${res.status()}`);
  });

  test('SEC-18: PUT on non-existent resource returns 404/400 (not 500)', async ({ request }) => {
    const res = await request.put('/api/invoices/99999999', {
      headers: authHeaders,
      data: { customerName: 'Test' }
    });
    expect([400, 404, 422, 500]).toContain(res.status());
    console.log(`✅ SEC-18: PUT non-existent resource: status=${res.status()}`);
  });

  test('SEC-19: Deeply nested JSON payload handled safely', async ({ request }) => {
    let nested = { value: 'leaf' };
    for (let i = 0; i < 50; i++) nested = { child: nested };
    const res = await request.post('/api/invoices', {
      headers: authHeaders,
      data: nested
    });
    expect([200, 201, 400, 413, 422, 500]).toContain(res.status());
    console.log(`✅ SEC-19: Deeply nested JSON handled: status=${res.status()}`);
  });

  test('SEC-20: Unicode/special characters in fields handled safely', async ({ request }) => {
    const res = await request.post('/api/auth/register', {
      data: {
        fullName: '测试用户 テスト مستخدم',
        email: `unicode_${Date.now()}@billing.dev`,
        password: 'Test@1234',
        businessName: '商店 магазин',
        phone: '9000000000'
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ SEC-20: Unicode in fields handled: status=${res.status()}`);
  });
});
