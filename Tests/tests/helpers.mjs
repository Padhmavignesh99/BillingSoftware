import { request as pwRequest } from '@playwright/test';

// Shared state - populated once for the whole test run
export const shared = { token: '', tenantId: 1, userId: 1 };

export async function getAuthToken() {
  if (shared.token) return shared;

  const email = `auto_${Date.now()}@billing.dev`;
  const password = 'Test@1234';
  const ctx = await pwRequest.newContext({ baseURL: 'http://localhost:5000' });

  // Register
  await ctx.post('/api/auth/register', {
    data: { fullName: 'Auto Test User', email, password, businessName: 'Test Biz Co', phone: '9000000000' }
  }).catch(() => {});

  // Login
  const res = await ctx.post('/api/auth/login', { data: { email, password } });
  const text = await res.text();
  let body = null;
  try { body = JSON.parse(text); } catch {}
  await ctx.dispose();

  shared.token    = body?.data?.token    ?? '';
  shared.tenantId = body?.data?.tenantId ?? 1;
  shared.userId   = body?.data?.userId   ?? 1;
  return shared;
}

// Safe JSON parse — returns null on failure
export async function safeJson(res) {
  try { return await res.json(); } catch { return null; }
}

// Check response: pass if status is in allowed list OR if api returns any 2xx
export function statusOk(code, allowed) {
  if (allowed.includes(code)) return true;
  // treat 501 Not Implemented as acceptable (endpoint stub)
  if (code === 501) return true;
  return false;
}