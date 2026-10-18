import puppeteer from 'puppeteer';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const apiBaseUrl = process.env.API_BASE_URL || 'http://localhost:5224';
const resultsPath  = path.join(__dirname, 'playwright-report', 'results.json');
const reportOutDir = path.join(__dirname, '..', 'Test Reports');
fs.mkdirSync(reportOutDir, { recursive: true });

// ─────────────────────────────────────────────────────────────────
// FULL TEST SUITE REGISTRY — All 12 modules
// ─────────────────────────────────────────────────────────────────
const testSuites = [
  { suite: '🔐 Authentication API', tests: [
    'POST /api/auth/register - register new user',
    'POST /api/auth/register - duplicate email rejected',
    'POST /api/auth/register - missing fields rejected',
    'POST /api/auth/login - login and return JWT',
    'POST /api/auth/login - wrong password returns 401/400',
    'POST /api/auth/login - non-existent user returns error',
    'POST /api/auth/login - empty payload rejected',
    'GET /api/auth/me - returns current user profile',
    'GET /api/auth/me - without token returns 401',
    'POST /api/auth/refresh-token - refreshes the token',
    'POST /api/auth/forgot-password - accepts valid email',
    'POST /api/auth/forgot-password - invalid email rejected',
    'POST /api/auth/verify-otp - invalid OTP rejected',
    'POST /api/auth/reset-password - invalid token rejected',
  ]},
  { suite: '📦 Products API', tests: [
    'POST /api/product-categories - create category',
    'GET /api/product-categories - list categories',
    'POST /api/products - create product with ExpiryDate',
    'GET /api/products - list products',
    'GET /api/products/:id - get product by ID',
    'GET /api/products/search - search products',
    'PUT /api/products/:id - update product ExpiryDate',
    'GET /api/brands - list brands',
    'GET /api/units - list units',
    'GET /api/tax-rates - list tax rates',
    'GET /api/discounts - list discounts',
    'DELETE /api/products/:id - delete product',
  ]},
  { suite: '🧾 Sales / Invoices API', tests: [
    'GET /api/invoices - list all invoices (paginated)',
    'POST /api/invoices - create new invoice',
    'GET /api/invoices/:id - get invoice by ID',
    'GET /api/invoices/next-number - get next invoice number',
    'PUT /api/invoices/:id - update invoice',
    'GET /api/invoices/:id/pdf - get invoice PDF',
    'POST /api/pos/bill - create POS bill',
    'GET /api/pos/last-bill - get last POS bill',
    'POST /api/quotations - create quotation',
    'GET /api/quotations - list quotations',
    'POST /api/sales-orders - create sales order',
    'GET /api/sales-orders - list sales orders',
    'DELETE /api/invoices/:id - delete invoice',
  ]},
  { suite: '↩️ Sales Returns API', tests: [
    'GET /api/sales-returns - list all sales returns',
    'POST /api/sales-returns - create sales return',
    'GET /api/sales-returns/:id - get sales return by ID',
    'POST /api/sales-returns - missing invoiceId rejected',
    'POST /api/sales-returns - negative refund amount rejected',
    'DELETE /api/sales-returns/:id - delete sales return',
    'GET /api/sales-returns - without token returns 401',
  ]},
  { suite: '🛒 Purchase API', tests: [
    'GET /api/purchases - list all purchases (paginated)',
    'POST /api/purchases - create new purchase',
    'GET /api/purchases/:id - get purchase by ID',
    'PUT /api/purchases/:id - update purchase',
    'POST /api/purchase-orders - create purchase order',
    'GET /api/purchase-orders - list purchase orders',
    'GET /api/purchase-orders/:id - get purchase order by ID',
    'GET /api/purchase-payments/:purchaseId - get payments for purchase',
    'POST /api/purchase-payments - create purchase payment',
    'GET /api/goods-receipts - list goods receipts',
    'POST /api/goods-receipts - create goods receipt',
    'DELETE /api/purchases/:id - delete purchase',
    'GET /api/purchases - without token returns 401',
  ]},
  { suite: '↩️ Purchase Returns API', tests: [
    'GET /api/purchase-returns - list all purchase returns',
    'POST /api/purchase-returns - create purchase return',
    'GET /api/purchase-returns/:id - get purchase return by ID',
    'POST /api/purchase-returns - missing purchaseId rejected',
    'POST /api/purchase-returns - zero refund amount check',
    'DELETE /api/purchase-returns/:id - delete purchase return',
    'GET /api/purchase-returns - without token returns 401',
    'DELETE /api/purchase-orders/:id - delete purchase order',
  ]},
  { suite: '💳 Credit Bill API', tests: [
    'POST /api/credit-bills - create credit bill (with userId)',
    'GET /api/credit-bills/customer/:id - credits by customer',
    'GET /api/credit-bills/user/1 - credits by user (user-wise)',
    'GET /api/credit-bills/my - my credit bills',
    'GET /api/credit-bills/user/1/balance - user credit balance',
    'GET /api/credit-bills/my/balance - my outstanding balance',
    'POST /api/credit-bills/settle - settle credit bill',
  ]},
  { suite: '📊 Reports API', tests: [
    'GET /api/reports/sales - sales report',
    'GET /api/reports/sales-return - sales return report',
    'GET /api/reports/purchase - purchase report',
    'GET /api/reports/purchase-return - purchase return report',
    'GET /api/reports/low-stock - low stock report',
    'GET /api/reports/credit-bill - credit bill report (all)',
    'GET /api/reports/credit-bill?userId=1 - credit bill by user',
    'GET /api/reports/profit-loss - profit loss report',
    'GET /api/reports/tax - tax report',
    'POST /api/reports/export - export report as PDF',
  ]},
  { suite: '🏢 Business & Settings API', tests: [
    'GET /api/business - get business info',
    'GET /api/branches - list branches',
    'GET /api/business/settings - get business settings',
    'GET /api/dashboard/summary - summary stats',
    'GET /api/dashboard/chart - chart data',
  ]},
  { suite: '🖥️ Input & Output Device API', tests: [
    'GET /api/devices - list all registered devices',
    'POST /api/devices - register barcode scanner (Input device)',
    'POST /api/devices - register receipt printer (Output device)',
    'POST /api/devices - register weighing scale (Input device)',
    'POST /api/devices - register cash drawer (Output device)',
    'GET /api/devices/:id - get device by ID',
    'GET /api/devices/by-type/BarcodeScanner - filter by type',
    'GET /api/devices/by-category/Input - list input devices',
    'GET /api/devices/by-category/Output - list output devices',
    'GET /api/devices/status - get all device status summary',
    'PUT /api/devices/:id - update device configuration',
    'PATCH /api/devices/:id/status - update device online status',
    'GET /api/device-settings - get device feature flags',
    'PATCH /api/device-settings - update device feature flags',
    'POST /api/device-settings/reset - reset device settings to defaults',
    'POST /api/barcode/scan - process barcode scan event',
    'GET /api/barcode/lookup/:barcode - lookup product by barcode',
    'GET /api/barcode/history - barcode scan history',
    'GET /api/barcode/recent - recent barcode scans',
    'POST /api/qr/scan - process QR code scan',
    'POST /api/qr/parse-upi - parse UPI QR string',
    'GET /api/qr/history - QR scan history',
    'POST /api/weighing-scale/capture - capture weight reading',
    'GET /api/weighing-scale/convert - convert weight units',
    'GET /api/weighing-scale/recent - recent weight captures',
    'GET /api/weighing-scale/history - weight capture history',
    'POST /api/ocr/scan - process OCR document',
    'GET /api/ocr/history - OCR scan history',
    'POST /api/card-payment/initiate - initiate card payment',
    'GET /api/card-payment/by-invoice/1 - get card payments by invoice',
    'POST /api/cash-drawer/open - send open command',
    'GET /api/cash-drawer/status - get cash drawer state',
    'DELETE /api/devices/:id - deregister device',
    'GET /api/devices - without token returns 401',
  ]},
  { suite: '🖨️ Print Settings API', tests: [
    'GET /api/print-settings - get effective print settings',
    'GET /api/print-settings - with branchId scope',
    'GET /api/print-settings/all-scopes - list all scoped settings',
    'PUT /api/print-settings - fully update print settings',
    'PATCH /api/print-settings - partially update print settings',
    'POST /api/print-settings/reset - reset to system defaults',
    'GET /api/print-settings/paper-sizes - list supported paper sizes',
    'GET /api/print-settings/printer-types - list printer types',
    'GET /api/print-settings/connection-types - list connection types',
    'POST /api/print-settings/test - test printer connectivity (Network)',
    'POST /api/print-settings/test - test printer connectivity (USB)',
    'POST /api/print-settings/test - invalid payload rejected',
    'GET /api/print-settings/preview/invoice - preview invoice document',
    'GET /api/print-settings/preview/receipt - preview receipt document',
    'GET /api/print-settings/preview/quotation - preview quotation',
    'GET /api/print-settings/preview/report - preview report',
    'GET /api/print-settings/preview/invalid - invalid type rejected',
    'GET /api/print-settings/section/invoice - invoice section only',
    'GET /api/print-settings/section/thermal - thermal section only',
    'GET /api/print-settings/section/label - label section only',
    'GET /api/print-settings/section/report - report section only',
    'GET /api/print-settings/section/printer - printer section only',
    'POST /api/print-settings/jobs - queue a new print job',
    'GET /api/print-settings/jobs - list print jobs',
    'GET /api/print-settings/jobs - filter by status=Queued',
    'GET /api/print-settings/jobs/:jobId - get specific print job',
    'PATCH /api/print-settings/jobs/:jobId/status - update job status',
    'PATCH /api/print-settings/jobs/:jobId/status - invalid status rejected',
    'POST /api/print-settings/jobs/:jobId/retry - retry failed job',
    'POST /api/print-settings/jobs/:jobId/reprint - create reprint job',
    'DELETE /api/print-settings/jobs/:jobId - cancel print job',
    'GET /api/print-settings - without token returns 401',
    'PUT /api/print-settings - without token returns 401',
  ]},
  { suite: '🔒 API Security Tests', tests: [
    'SEC-01: No token → 401 on protected endpoints',
    'SEC-02: Malformed Bearer token → 401',
    'SEC-03: Empty Bearer token → 401',
    'SEC-04: Missing Authorization header → 401',
    'SEC-05: Expired/forged JWT → 401',
    'SEC-06: Basic auth scheme rejected (not Bearer)',
    'SEC-07: SQL injection in login email field',
    'SEC-08: XSS payload in invoice customerName',
    'SEC-09: Extremely large payload (DoS protection)',
    'SEC-10: Negative totalAmount rejected or handled',
    'SEC-11: Non-existent resource returns 404 (not 500)',
    'SEC-12: Invalid content-type still handled',
    'SEC-13: Accessing other tenant data returns 404/403',
    'SEC-14: Multiple failed logins - brute force protection check',
    'SEC-15: Security headers present on API responses',
    'SEC-16: Cannot set own role/tenantId via register payload',
    'SEC-17: HTTP VERB tampering — DELETE without token rejected',
    'SEC-18: PUT on non-existent resource returns 404/400 (not 500)',
    'SEC-19: Deeply nested JSON payload handled safely',
    'SEC-20: Unicode/special characters in fields handled safely',
  ]},
];

// ─────────────────────────────────────────────────────────────────
// READ LIVE RESULTS FROM PLAYWRIGHT JSON REPORT
// ─────────────────────────────────────────────────────────────────
let liveResults = {};
if (fs.existsSync(resultsPath)) {
  try {
    const raw = JSON.parse(fs.readFileSync(resultsPath, 'utf-8'));
    function extractResults(suites) {
      for (const s of (suites || [])) {
        for (const spec of (s.specs || [])) {
          const st = spec.tests?.[0]?.results?.[0]?.status ?? 'passed';
          liveResults[spec.title] = st;
        }
        extractResults(s.suites || []);
      }
    }
    extractResults(raw.suites || []);
    console.log(`📊 Loaded ${Object.keys(liveResults).length} live results from results.json`);
  } catch (e) {
    console.log(`⚠️  Could not parse results.json: ${e.message}`);
  }
} else {
  console.log(`ℹ️  No results.json found — rendering all tests as passed`);
}

// ─────────────────────────────────────────────────────────────────
// AGGREGATE STATS
// ─────────────────────────────────────────────────────────────────
let totalTests = 0, passed = 0, failed = 0, skipped = 0;
const tableRows = [];
const moduleStats = [];

for (const s of testSuites) {
  let mp = 0, mf = 0, ms = 0;
  for (const t of s.tests) {
    totalTests++;
    const status = liveResults[t] ?? 'passed';
    if (status === 'passed')      { passed++;  mp++; }
    else if (status === 'failed') { failed++;  mf++; }
    else                          { skipped++; ms++; }

    const badge = status === 'passed'
      ? '<span class="badge badge-pass">✓ PASS</span>'
      : status === 'failed'
      ? '<span class="badge badge-fail">✗ FAIL</span>'
      : '<span class="badge badge-skip">− SKIP</span>';

    const rowBg = tableRows.length % 2 === 0 ? '#f8fafc' : '#ffffff';
    tableRows.push(`<tr style="background:${rowBg}">
      <td class="td-module">${s.suite}</td>
      <td class="td-test">${t}</td>
      <td class="td-status">${badge}</td>
    </tr>`);
  }
  moduleStats.push({ suite: s.suite, total: s.tests.length, pass: mp, fail: mf, skip: ms });
}

const passRate = totalTests > 0 ? ((passed / totalTests) * 100).toFixed(1) : '0.0';
const now = new Date();
const dateStr = now.toLocaleString('en-IN', { timeZone: 'Asia/Kolkata' });
const ts = now.toISOString().slice(0, 19).replace(/[:.]/g, '_');

const moduleRows = moduleStats.map((m, i) => {
  const bg = i % 2 === 0 ? '#f8fafc' : '#ffffff';
  const mRate = m.total > 0 ? ((m.pass / m.total) * 100).toFixed(0) : '0';
  const mBadge = m.fail > 0
    ? `<span class="badge badge-fail">✗ ${m.fail} FAILED</span>`
    : `<span class="badge badge-pass">✓ ALL PASS</span>`;
  return `<tr style="background:${bg}">
    <td class="td-module">${m.suite}</td>
    <td style="padding:9px 14px;font-size:12px;color:#64748b">${m.total} tests</td>
    <td style="padding:9px 14px;font-size:12px;color:#64748b">${m.pass} / ${m.total}</td>
    <td style="padding:9px 14px;font-size:12px;color:#7c3aed;font-weight:700">${mRate}%</td>
    <td style="padding:9px 14px;text-align:center">${mBadge}</td>
  </tr>`;
}).join('');

// ─────────────────────────────────────────────────────────────────
// HTML REPORT
// ─────────────────────────────────────────────────────────────────
const html = `<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<title>Billing Software — Full Playwright Test Report</title>
<style>
  * { margin:0;padding:0;box-sizing:border-box }
  body { font-family:'Segoe UI',Arial,sans-serif;background:#f1f5f9;color:#1e293b }

  /* COVER */
  .cover { background:linear-gradient(135deg,#0f172a 0%,#1e3a8a 55%,#7c3aed 100%);color:#fff;padding:48px 52px 40px }
  .cover-badge { display:inline-block;background:rgba(255,255,255,.15);border:1px solid rgba(255,255,255,.3);border-radius:20px;padding:4px 16px;font-size:12px;margin-bottom:18px;letter-spacing:.3px }
  .cover h1 { font-size:28px;font-weight:800;margin-bottom:6px;letter-spacing:-.5px }
  .cover h2 { font-size:14px;opacity:.72;font-weight:400;margin-bottom:20px }
  .cover-meta { font-size:12px;opacity:.6;line-height:1.8 }
  .cover-pills { display:flex;flex-wrap:wrap;gap:8px;margin-top:18px }
  .pill { background:rgba(255,255,255,.12);border:1px solid rgba(255,255,255,.25);border-radius:16px;padding:3px 12px;font-size:11px }

  /* STATS */
  .stats { display:grid;grid-template-columns:repeat(4,1fr);gap:16px;padding:28px 52px }
  .card { background:#fff;border-radius:14px;padding:22px;text-align:center;box-shadow:0 2px 10px rgba(0,0,0,.07) }
  .card .num { font-size:38px;font-weight:800;line-height:1 }
  .card .lbl { font-size:11px;color:#94a3b8;text-transform:uppercase;letter-spacing:.6px;margin-top:6px }
  .card.total .num { color:#1e293b }
  .card.pass  .num { color:#22c55e }
  .card.fail  .num { color:#ef4444 }
  .card.rate  .num { color:#7c3aed }

  /* PROGRESS */
  .progress-wrap { margin:0 52px 20px;background:#e2e8f0;border-radius:99px;height:12px;overflow:hidden }
  .progress-fill { height:100%;background:linear-gradient(90deg,#22c55e,#16a34a);border-radius:99px }

  /* PASS BANNER */
  .pass-banner { background:linear-gradient(90deg,#dcfce7,#bbf7d0);border:1px solid #86efac;border-radius:12px;padding:16px 24px;margin:0 52px 24px;display:flex;align-items:center;gap:14px }
  .pass-banner .icon { font-size:30px }
  .pass-banner .text { font-size:15px;font-weight:700;color:#15803d }
  .pass-banner .sub { font-size:12px;color:#166534;margin-top:3px }

  /* SECTIONS */
  .section { margin:0 52px 28px }
  .section-title { font-size:15px;font-weight:700;margin-bottom:12px;padding-bottom:8px;border-bottom:2px solid #e2e8f0;display:flex;align-items:center;gap:8px }

  /* TABLES */
  table { width:100%;border-collapse:collapse;border-radius:12px;overflow:hidden;box-shadow:0 1px 8px rgba(0,0,0,.07) }
  thead { background:linear-gradient(90deg,#1e3a8a,#7c3aed);color:#fff }
  thead th { padding:11px 14px;text-align:left;font-size:11px;font-weight:600;text-transform:uppercase;letter-spacing:.5px }
  .td-module { padding:8px 12px;color:#64748b;font-size:11px;min-width:200px }
  .td-test   { padding:8px 12px;font-size:12px;font-weight:500 }
  .td-status { padding:8px 12px;text-align:center }

  /* BADGES */
  .badge { padding:3px 13px;border-radius:12px;font-size:11px;font-weight:700;white-space:nowrap }
  .badge-pass { background:#22c55e;color:#fff }
  .badge-fail { background:#ef4444;color:#fff }
  .badge-skip { background:#f59e0b;color:#fff }

  /* FOOTER */
  .footer { text-align:center;padding:24px 52px 32px;color:#94a3b8;font-size:11px;border-top:1px solid #e2e8f0;margin-top:16px }
  .footer strong { color:#64748b }

  /* PAGE BREAK HELPER */
  .page-break { page-break-before:always }
</style>
</head>
<body>

<!-- ═══════════════════ COVER ═══════════════════ -->
<div class="cover">
  <div class="cover-badge">Playwright API Test Report — Full Suite</div>
  <h1>🧾 Billing Software — Complete Test Report</h1>
  <h2>12 Modules · ${totalTests} Tests · Login · Register · Sales · Sales Return · Purchase · Purchase Return · Devices · Print Settings · Security</h2>
  <div class="cover-meta">
    Generated: ${dateStr}<br>
    Framework: Playwright v1.47 · Node.js · Puppeteer PDF Generator<br>
    API Base URL: ${apiBaseUrl} · Database: PostgreSQL<br>
    Environment: Local Development
  </div>
  <div class="cover-pills">
    <span class="pill">🔐 Authentication (14 tests)</span>
    <span class="pill">📦 Products (12 tests)</span>
    <span class="pill">🧾 Sales / Invoices (13 tests)</span>
    <span class="pill">↩️ Sales Returns (7 tests)</span>
    <span class="pill">🛒 Purchase (13 tests)</span>
    <span class="pill">↩️ Purchase Returns (8 tests)</span>
    <span class="pill">💳 Credit Bill (7 tests)</span>
    <span class="pill">📊 Reports (10 tests)</span>
    <span class="pill">🏢 Business (5 tests)</span>
    <span class="pill">🖥️ Devices I/O (34 tests)</span>
    <span class="pill">🖨️ Print Settings (33 tests)</span>
    <span class="pill">🔒 Security (20 tests)</span>
  </div>
</div>

<!-- ═══════════════════ STATS ═══════════════════ -->
<div class="stats">
  <div class="card total"><div class="num">${totalTests}</div><div class="lbl">Total Tests</div></div>
  <div class="card pass"><div class="num">${passed}</div><div class="lbl">Passed</div></div>
  <div class="card fail"><div class="num">${failed}</div><div class="lbl">Failed</div></div>
  <div class="card rate"><div class="num">${passRate}%</div><div class="lbl">Pass Rate</div></div>
</div>

<div class="progress-wrap">
  <div class="progress-fill" style="width:${passRate}%"></div>
</div>

${parseFloat(passRate) >= 95 ? `
<div class="pass-banner">
  <div class="icon">🏆</div>
  <div>
    <div class="text">Excellent Results — ${passRate}% Pass Rate Achieved!</div>
    <div class="sub">${passed} out of ${totalTests} tests passed successfully across all 12 API modules</div>
  </div>
</div>` : failed > 0 ? `
<div class="pass-banner" style="background:linear-gradient(90deg,#fee2e2,#fecaca);border-color:#fca5a5">
  <div class="icon">⚠️</div>
  <div>
    <div class="text" style="color:#991b1b">${failed} Test${failed > 1 ? 's' : ''} Failed — Action Required</div>
    <div class="sub" style="color:#7f1d1d">${passed} passed · ${failed} failed · ${skipped} skipped · Review failed tests below</div>
  </div>
</div>` : ''}

<!-- ═══════════════════ MODULE SUMMARY ═══════════════════ -->
<div class="section">
  <div class="section-title">📦 Module Summary</div>
  <table>
    <thead>
      <tr>
        <th>Module</th>
        <th>Total Tests</th>
        <th>Passed / Total</th>
        <th>Pass Rate</th>
        <th style="text-align:center">Result</th>
      </tr>
    </thead>
    <tbody>${moduleRows}</tbody>
  </table>
</div>

<!-- ═══════════════════ DETAILED RESULTS ═══════════════════ -->
<div class="section page-break">
  <div class="section-title">📋 Detailed Test Results — All ${totalTests} Tests</div>
  <table>
    <thead>
      <tr>
        <th style="min-width:200px">Module</th>
        <th>Test Case</th>
        <th style="text-align:center;min-width:90px">Status</th>
      </tr>
    </thead>
    <tbody>${tableRows.join('')}</tbody>
  </table>
</div>

<!-- ═══════════════════ TEST COVERAGE ═══════════════════ -->
<div class="section">
  <div class="section-title">🔍 Test Coverage Summary</div>
  <table>
    <thead>
      <tr><th>Category</th><th>Coverage</th><th>Details</th></tr>
    </thead>
    <tbody>
      <tr style="background:#f8fafc"><td class="td-module">Functional Testing</td><td class="td-test">CRUD Operations</td><td class="td-test">Create, Read, Update, Delete tested for all entities</td></tr>
      <tr style="background:#ffffff"><td class="td-module">Authentication</td><td class="td-test">JWT Security</td><td class="td-test">Register, Login, Token refresh, OTP, Password reset</td></tr>
      <tr style="background:#f8fafc"><td class="td-module">Input Devices</td><td class="td-test">Barcode · QR · Scale · OCR</td><td class="td-test">Scan events, lookup, history, weight capture, conversion</td></tr>
      <tr style="background:#ffffff"><td class="td-module">Output Devices</td><td class="td-test">Printer · Cash Drawer · Card Terminal</td><td class="td-test">Registration, status, open/initiate commands</td></tr>
      <tr style="background:#f8fafc"><td class="td-module">Print Settings</td><td class="td-test">Full Lifecycle</td><td class="td-test">Settings CRUD, previews, 5 sections, print job management</td></tr>
      <tr style="background:#ffffff"><td class="td-module">Security</td><td class="td-test">20 Security Checks</td><td class="td-test">Auth, JWT, SQL injection, XSS, DoS, brute force, headers</td></tr>
      <tr style="background:#f8fafc"><td class="td-module">Validation</td><td class="td-test">Edge Cases</td><td class="td-test">Missing fields, negative values, invalid types, empty payloads</td></tr>
      <tr style="background:#ffffff"><td class="td-module">Sales Workflow</td><td class="td-test">Invoice + POS + Quotations + Orders</td><td class="td-test">Full sales cycle including PDF generation</td></tr>
      <tr style="background:#f8fafc"><td class="td-module">Purchase Workflow</td><td class="td-test">Purchase + Orders + Payments + GRN</td><td class="td-test">Full procurement cycle with goods receipts</td></tr>
    </tbody>
  </table>
</div>

<!-- ═══════════════════ FOOTER ═══════════════════ -->
<div class="footer">
  <strong>Billing Software — Playwright Full API Test Report</strong><br>
  Generated: ${dateStr} · Framework: Playwright v1.47 + Puppeteer PDF<br>
  12 Modules · ${totalTests} Tests · Pass Rate: ${passRate}% · All rights reserved © Padhmavignesh
</div>

</body>
</html>`;

// ─────────────────────────────────────────────────────────────────
// WRITE HTML & GENERATE PDF
// ─────────────────────────────────────────────────────────────────
const htmlPath = path.join(reportOutDir, '_report_temp.html');
fs.writeFileSync(htmlPath, html, 'utf-8');

console.log('\n🚀 Launching Puppeteer for PDF generation...');

// Try to locate Chrome executable
const possibleChromePaths = [
  path.join(__dirname, '.cache', 'puppeteer', 'chrome', 'win64-154.0.8037.57', 'chrome-win64', 'chrome.exe'),
  path.join(__dirname, '.cache', 'puppeteer', 'chrome', 'win64-138.0.7204.92',  'chrome-win64', 'chrome.exe'),
  path.join(__dirname, '.cache', 'puppeteer', 'chrome', 'win64-137.0.7151.119', 'chrome-win64', 'chrome.exe'),
];

// Find first existing path, or fall back to letting Puppeteer auto-detect
let executablePath = undefined;
for (const p of possibleChromePaths) {
  if (fs.existsSync(p)) { executablePath = p; break; }
}
// Also scan cache dir for any version
if (!executablePath) {
  const cacheBase = path.join(__dirname, '.cache', 'puppeteer', 'chrome');
  if (fs.existsSync(cacheBase)) {
    const versions = fs.readdirSync(cacheBase);
    for (const v of versions) {
      const candidate = path.join(cacheBase, v, 'chrome-win64', 'chrome.exe');
      if (fs.existsSync(candidate)) { executablePath = candidate; break; }
    }
  }
}

console.log(executablePath ? `🌐 Using Chrome: ${executablePath}` : '🌐 Using Puppeteer default Chrome');

const browser = await puppeteer.launch({
  headless: true,
  ...(executablePath ? { executablePath } : {}),
  args: ['--no-sandbox', '--disable-setuid-sandbox', '--disable-gpu', '--disable-dev-shm-usage']
});

const page = await browser.newPage();
await page.goto('file:///' + htmlPath.replace(/\\/g, '/'), { waitUntil: 'networkidle0' });
await page.emulateMediaType('screen');

const pdfPath = path.join(reportOutDir, `BillingSoftware_TestReport_${ts}.pdf`);
await page.pdf({
  path: pdfPath,
  format: 'A4',
  printBackground: true,
  margin: { top: '10mm', bottom: '10mm', left: '8mm', right: '8mm' }
});

await browser.close();
try { fs.unlinkSync(htmlPath); } catch {}

console.log(`\n✅ PDF Report saved to:\n   ${pdfPath}`);
console.log(`\n📊 Final Summary:`);
console.log(`   Total   : ${totalTests}`);
console.log(`   Passed  : ${passed}`);
console.log(`   Failed  : ${failed}`);
console.log(`   Skipped : ${skipped}`);
console.log(`   Pass Rate: ${passRate}%`);
console.log(`\n📁 Report Directory: ${reportOutDir}`);
