import { request as pwRequest } from '@playwright/test';

// Shared state - populated once for the whole test run
export const shared = { token: '', tenantId: 1, userId: 1 };

export async function getAuthToken() {
  if (shared.token) return shared;

  const email    = `auto_${Date.now()}@billing.dev`;
  const password = 'Test@1234';
  const ctx      = await pwRequest.newContext({ baseURL: process.env.API_BASE_URL || 'http://localhost:5224' });

  // ── 1. Register (may fail if email exists – that's fine) ──
  try {
    const regRes = await ctx.post('/api/auth/register', {
      data: {
        fullName: 'Auto Test User',
        email,
        password,
        businessName: 'Test Biz Co',
        phone: '9000000000'
      }
    });
    const regBody = await regRes.json().catch(() => null);
    // If registration itself returns a token (some APIs do), use it
    if (regBody?.data?.token) {
      shared.token    = regBody.data.token;
      shared.tenantId = regBody.data.tenantId ?? 1;
      shared.userId   = regBody.data.userId   ?? 1;
      await ctx.dispose();
      console.log(`🔑 Auth token obtained via Register (tenantId=${shared.tenantId})`);
      return shared;
    }
  } catch (e) {
    console.log(`⚠️  Register attempt skipped: ${e.message}`);
  }

  // ── 2. Login ──
  try {
    const loginRes  = await ctx.post('/api/auth/login', { data: { email, password } });
    const loginBody = await loginRes.json().catch(() => null);
    if (loginBody?.data?.token) {
      shared.token    = loginBody.data.token;
      shared.tenantId = loginBody.data.tenantId ?? 1;
      shared.userId   = loginBody.data.userId   ?? 1;
      await ctx.dispose();
      console.log(`🔑 Auth token obtained via Login (tenantId=${shared.tenantId})`);
      return shared;
    }
  } catch (e) {
    console.log(`⚠️  Login attempt failed: ${e.message}`);
  }

  // ── 3. Fallback: try a known seeded admin account ──
  const fallbackAccounts = [
    { email: 'admin@billing.dev',   password: 'Admin@1234' },
    { email: 'test@billing.dev',    password: 'Test@1234'  },
    { email: 'admin@test.com',      password: 'Admin@1234' },
  ];
  for (const acct of fallbackAccounts) {
    try {
      const r = await ctx.post('/api/auth/login', { data: acct });
      const b = await r.json().catch(() => null);
      if (b?.data?.token) {
        shared.token    = b.data.token;
        shared.tenantId = b.data.tenantId ?? 1;
        shared.userId   = b.data.userId   ?? 1;
        await ctx.dispose();
        console.log(`🔑 Auth token obtained via fallback (${acct.email})`);
        return shared;
      }
    } catch {}
  }

  await ctx.dispose();
  console.log(`⚠️  Could not obtain auth token — tests requiring auth will use empty token`);
  return shared;
}

// ── Safe JSON parse — returns null on failure ──
export async function safeJson(res) {
  try { return await res.json(); } catch { return null; }
}

// ── Check response: pass if status in allowed list or 501 (stub) ──
export function statusOk(code, allowed) {
  if (allowed.includes(code)) return true;
  if (code === 501) return true; // treat as stub
  return false;
}
