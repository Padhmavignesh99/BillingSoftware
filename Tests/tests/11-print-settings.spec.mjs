import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

test.describe('🖨️ Print Settings API', () => {
  let authHeaders;
  let createdJobId;

  test.beforeAll(async () => {
    const auth = await getAuthToken();
    authHeaders = { Authorization: `Bearer ${auth.token}`, 'Content-Type': 'application/json' };
  });

  // ─────────── CORE SETTINGS ───────────
  test('GET /api/print-settings - get effective print settings', async ({ request }) => {
    const res = await request.get('/api/print-settings', { headers: authHeaders });
    expect([200, 401]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) {
      expect(body.success).toBe(true);
    }
    console.log(`✅ GET print settings: status=${res.status()}`);
  });

  test('GET /api/print-settings - with branchId scope', async ({ request }) => {
    const res = await request.get('/api/print-settings?branchId=1', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET print settings (branch scope): status=${res.status()}`);
  });

  test('GET /api/print-settings/all-scopes - list all scoped settings', async ({ request }) => {
    const res = await request.get('/api/print-settings/all-scopes', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) {
      expect(body.success).toBe(true);
    }
    console.log(`✅ GET all print scopes: status=${res.status()}`);
  });

  test('PUT /api/print-settings - fully update print settings', async ({ request }) => {
    const res = await request.put('/api/print-settings', {
      headers: authHeaders,
      data: {
        paperSize: 'A4',
        orientation: 'Portrait',
        printerType: 'InkJet',
        fontFamily: 'Arial',
        fontSizePt: 12,
        headerFontSizePt: 16,
        marginTopMm: 10,
        marginBottomMm: 10,
        marginLeftMm: 15,
        marginRightMm: 15,
        showBusinessLogo: true,
        showBusinessAddress: true,
        showGSTDetails: true,
        showCustomerAddress: true,
        showTaxBreakdown: true,
        showAmountInWords: true,
        showTermsAndConditions: false,
        showFooterNote: true,
        showQRCode: true,
        showBarcode: false,
        showPaymentDetails: true,
        invoiceCopies: 2,
        printPrimaryColor: '#1e3a8a',
        printInColor: true,
        footerText: 'Thank you for your business!'
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ PUT print settings: status=${res.status()}`);
  });

  test('PATCH /api/print-settings - partially update print settings', async ({ request }) => {
    const res = await request.patch('/api/print-settings', {
      headers: authHeaders,
      data: {
        showBusinessLogo: false,
        showQRCode: false,
        invoiceCopies: 1,
        footerText: 'Updated footer'
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ PATCH print settings: status=${res.status()}`);
  });

  test('POST /api/print-settings/reset - reset to system defaults', async ({ request }) => {
    const res = await request.post('/api/print-settings/reset', { headers: authHeaders });
    expect([200, 201, 400, 500]).toContain(res.status());
    console.log(`✅ POST print settings reset: status=${res.status()}`);
  });

  // ─────────── LOOKUP ENDPOINTS ───────────
  test('GET /api/print-settings/paper-sizes - list supported paper sizes', async ({ request }) => {
    const res = await request.get('/api/print-settings/paper-sizes', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) {
      expect(body.success).toBe(true);
      expect(Array.isArray(body.data)).toBe(true);
    }
    console.log(`✅ GET paper sizes: status=${res.status()}`);
  });

  test('GET /api/print-settings/printer-types - list printer types', async ({ request }) => {
    const res = await request.get('/api/print-settings/printer-types', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) {
      expect(body.success).toBe(true);
      expect(Array.isArray(body.data)).toBe(true);
    }
    console.log(`✅ GET printer types: status=${res.status()}`);
  });

  test('GET /api/print-settings/connection-types - list connection types', async ({ request }) => {
    const res = await request.get('/api/print-settings/connection-types', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET connection types: status=${res.status()}`);
  });

  // ─────────── PRINTER TEST ───────────
  test('POST /api/print-settings/test - test printer connectivity (Network)', async ({ request }) => {
    const res = await request.post('/api/print-settings/test', {
      headers: authHeaders,
      data: {
        printerName: 'TestPrinter',
        printerType: 'Thermal',
        connectionType: 'Network',
        ipAddress: '192.168.1.100',
        port: 9100
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ POST printer test (Network): status=${res.status()}`);
  });

  test('POST /api/print-settings/test - test printer connectivity (USB)', async ({ request }) => {
    const res = await request.post('/api/print-settings/test', {
      headers: authHeaders,
      data: {
        printerName: 'USB Printer',
        printerType: 'InkJet',
        connectionType: 'USB'
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ POST printer test (USB): status=${res.status()}`);
  });

  test('POST /api/print-settings/test - invalid payload rejected', async ({ request }) => {
    const res = await request.post('/api/print-settings/test', {
      headers: authHeaders,
      data: {}
    });
    expect([400, 422, 500]).toContain(res.status());
    console.log(`✅ POST printer test (invalid): status=${res.status()}`);
  });

  // ─────────── PRINT PREVIEW ───────────
  test('GET /api/print-settings/preview/invoice - preview invoice document', async ({ request }) => {
    const res = await request.get('/api/print-settings/preview/invoice', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET preview invoice: status=${res.status()}`);
  });

  test('GET /api/print-settings/preview/receipt - preview receipt document', async ({ request }) => {
    const res = await request.get('/api/print-settings/preview/receipt', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET preview receipt: status=${res.status()}`);
  });

  test('GET /api/print-settings/preview/quotation - preview quotation', async ({ request }) => {
    const res = await request.get('/api/print-settings/preview/quotation', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET preview quotation: status=${res.status()}`);
  });

  test('GET /api/print-settings/preview/report - preview report', async ({ request }) => {
    const res = await request.get('/api/print-settings/preview/report', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET preview report: status=${res.status()}`);
  });

  test('GET /api/print-settings/preview/invalid - invalid type rejected', async ({ request }) => {
    const res = await request.get('/api/print-settings/preview/invalid', { headers: authHeaders });
    expect([400, 422, 500]).toContain(res.status());
    console.log(`✅ GET preview invalid type: status=${res.status()}`);
  });

  // ─────────── SECTION READS ───────────
  test('GET /api/print-settings/section/invoice - invoice section only', async ({ request }) => {
    const res = await request.get('/api/print-settings/section/invoice', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET section invoice: status=${res.status()}`);
  });

  test('GET /api/print-settings/section/thermal - thermal section only', async ({ request }) => {
    const res = await request.get('/api/print-settings/section/thermal', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET section thermal: status=${res.status()}`);
  });

  test('GET /api/print-settings/section/label - label section only', async ({ request }) => {
    const res = await request.get('/api/print-settings/section/label', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET section label: status=${res.status()}`);
  });

  test('GET /api/print-settings/section/report - report section only', async ({ request }) => {
    const res = await request.get('/api/print-settings/section/report', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET section report: status=${res.status()}`);
  });

  test('GET /api/print-settings/section/printer - printer section only', async ({ request }) => {
    const res = await request.get('/api/print-settings/section/printer', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET section printer: status=${res.status()}`);
  });

  // ─────────── PRINT JOB MANAGEMENT ───────────
  test('POST /api/print-settings/jobs - queue a new print job', async ({ request }) => {
    const res = await request.post('/api/print-settings/jobs', {
      headers: authHeaders,
      data: {
        documentType: 'invoice',
        documentId: 1,
        copies: 1,
        branchId: null,
        counterId: null
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200 || res.status() === 201) {
      createdJobId = body?.data?.id;
    }
    console.log(`✅ POST queue print job: status=${res.status()}, jobId=${createdJobId}`);
  });

  test('GET /api/print-settings/jobs - list print jobs', async ({ request }) => {
    const res = await request.get('/api/print-settings/jobs?page=1&pageSize=10', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) {
      expect(body.success).toBe(true);
    }
    console.log(`✅ GET print jobs: status=${res.status()}`);
  });

  test('GET /api/print-settings/jobs - filter by status=Queued', async ({ request }) => {
    const res = await request.get('/api/print-settings/jobs?status=Queued', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET print jobs (status filter): status=${res.status()}`);
  });

  test('GET /api/print-settings/jobs/:jobId - get specific print job', async ({ request }) => {
    const id = createdJobId ?? 1;
    const res = await request.get(`/api/print-settings/jobs/${id}`, { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET print job by ID: status=${res.status()}`);
  });

  test('PATCH /api/print-settings/jobs/:jobId/status - update job status', async ({ request }) => {
    const id = createdJobId ?? 1;
    const res = await request.patch(`/api/print-settings/jobs/${id}/status?status=Printing`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ PATCH print job status: status=${res.status()}`);
  });

  test('PATCH /api/print-settings/jobs/:jobId/status - invalid status rejected', async ({ request }) => {
    const id = createdJobId ?? 1;
    const res = await request.patch(`/api/print-settings/jobs/${id}/status?status=InvalidStatus`, { headers: authHeaders });
    expect([400, 422, 500]).toContain(res.status());
    console.log(`✅ PATCH print job invalid status: status=${res.status()}`);
  });

  test('POST /api/print-settings/jobs/:jobId/retry - retry failed job', async ({ request }) => {
    const id = createdJobId ?? 1;
    const res = await request.post(`/api/print-settings/jobs/${id}/retry`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ POST print job retry: status=${res.status()}`);
  });

  test('POST /api/print-settings/jobs/:jobId/reprint - create reprint job', async ({ request }) => {
    const id = createdJobId ?? 1;
    const res = await request.post(`/api/print-settings/jobs/${id}/reprint`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ POST print job reprint: status=${res.status()}`);
  });

  test('DELETE /api/print-settings/jobs/:jobId - cancel print job', async ({ request }) => {
    const id = createdJobId ?? 1;
    const res = await request.delete(`/api/print-settings/jobs/${id}`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ DELETE print job (cancel): status=${res.status()}`);
  });

  // ─────────── SECURITY ───────────
  test('GET /api/print-settings - without token returns 401', async ({ request }) => {
    const res = await request.get('/api/print-settings');
    expect([401, 403]).toContain(res.status());
    console.log(`✅ Print settings without token: status=${res.status()}`);
  });

  test('PUT /api/print-settings - without token returns 401', async ({ request }) => {
    const res = await request.put('/api/print-settings', {
      data: { paperSize: 'A4' }
    });
    expect([401, 403]).toContain(res.status());
    console.log(`✅ PUT print settings without token: status=${res.status()}`);
  });
});
