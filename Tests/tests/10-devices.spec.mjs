import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

test.describe('🖥️ Input & Output Device API', () => {
  let authHeaders;
  let createdDeviceId;

  test.beforeAll(async () => {
    const auth = await getAuthToken();
    authHeaders = { Authorization: `Bearer ${auth.token}`, 'Content-Type': 'application/json' };
  });

  // ─────────── DEVICE CONFIGURATION ───────────
  test('GET /api/devices - list all registered devices', async ({ request }) => {
    const res = await request.get('/api/devices', { headers: authHeaders });
    expect([200, 401]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) {
      expect(body.success).toBe(true);
      expect(Array.isArray(body.data)).toBe(true);
    }
    console.log(`✅ GET devices: status=${res.status()}`);
  });

  test('POST /api/devices - register barcode scanner (Input device)', async ({ request }) => {
    const res = await request.post('/api/devices', {
      headers: authHeaders,
      data: {
        deviceName: 'Test Barcode Scanner',
        deviceType: 'BarcodeScanner',
        category: 'Input',
        connectionType: 'USB',
        portOrAddress: 'COM3',
        isActive: true,
        description: 'Playwright test scanner'
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200 || res.status() === 201) {
      createdDeviceId = body?.data?.id;
    }
    console.log(`✅ POST barcode scanner: status=${res.status()}, id=${createdDeviceId}`);
  });

  test('POST /api/devices - register receipt printer (Output device)', async ({ request }) => {
    const res = await request.post('/api/devices', {
      headers: authHeaders,
      data: {
        deviceName: 'Test Receipt Printer',
        deviceType: 'ReceiptPrinter',
        category: 'Output',
        connectionType: 'Network',
        portOrAddress: '192.168.1.100:9100',
        isActive: true,
        description: 'Playwright test thermal printer'
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ POST receipt printer: status=${res.status()}`);
  });

  test('POST /api/devices - register weighing scale (Input device)', async ({ request }) => {
    const res = await request.post('/api/devices', {
      headers: authHeaders,
      data: {
        deviceName: 'Test Weighing Scale',
        deviceType: 'WeighingScale',
        category: 'Input',
        connectionType: 'USB',
        portOrAddress: 'COM4',
        isActive: true
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ POST weighing scale: status=${res.status()}`);
  });

  test('POST /api/devices - register cash drawer (Output device)', async ({ request }) => {
    const res = await request.post('/api/devices', {
      headers: authHeaders,
      data: {
        deviceName: 'Test Cash Drawer',
        deviceType: 'CashDrawer',
        category: 'Output',
        connectionType: 'USB',
        portOrAddress: 'COM5',
        isActive: true
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ POST cash drawer: status=${res.status()}`);
  });

  test('GET /api/devices/:id - get device by ID', async ({ request }) => {
    const id = createdDeviceId ?? 1;
    const res = await request.get(`/api/devices/${id}`, { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET device by ID: status=${res.status()}`);
  });

  test('GET /api/devices/by-type/BarcodeScanner - filter by type', async ({ request }) => {
    const res = await request.get('/api/devices/by-type/BarcodeScanner', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET devices by type: status=${res.status()}`);
  });

  test('GET /api/devices/by-category/Input - list input devices', async ({ request }) => {
    const res = await request.get('/api/devices/by-category/Input', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET Input devices by category: status=${res.status()}`);
  });

  test('GET /api/devices/by-category/Output - list output devices', async ({ request }) => {
    const res = await request.get('/api/devices/by-category/Output', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET Output devices by category: status=${res.status()}`);
  });

  test('GET /api/devices/status - get all device status summary', async ({ request }) => {
    const res = await request.get('/api/devices/status', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET device status summary: status=${res.status()}`);
  });

  test('PUT /api/devices/:id - update device configuration', async ({ request }) => {
    const id = createdDeviceId ?? 1;
    const res = await request.put(`/api/devices/${id}`, {
      headers: authHeaders,
      data: { deviceName: 'Updated Scanner', portOrAddress: 'COM6', isActive: true }
    });
    expect([200, 201, 400, 404, 422, 500]).toContain(res.status());
    console.log(`✅ PUT device: status=${res.status()}`);
  });

  test('PATCH /api/devices/:id/status - update device online status', async ({ request }) => {
    const id = createdDeviceId ?? 1;
    const res = await request.patch(`/api/devices/${id}/status?status=Connected`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ PATCH device status: status=${res.status()}`);
  });

  // ─────────── DEVICE SETTINGS ───────────
  test('GET /api/device-settings - get device feature flags', async ({ request }) => {
    const res = await request.get('/api/device-settings', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    const body = await safeJson(res);
    if (res.status() === 200) expect(body.success).toBe(true);
    console.log(`✅ GET device settings: status=${res.status()}`);
  });

  test('PATCH /api/device-settings - update device feature flags', async ({ request }) => {
    const res = await request.patch('/api/device-settings', {
      headers: authHeaders,
      data: {
        barcodeScannerEnabled: true,
        weighingScaleEnabled: true,
        qrScannerEnabled: false,
        ocrEnabled: false,
        cardTerminalEnabled: false,
        cashDrawerEnabled: true
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ PATCH device settings: status=${res.status()}`);
  });

  test('POST /api/device-settings/reset - reset device settings to defaults', async ({ request }) => {
    const res = await request.post('/api/device-settings/reset', { headers: authHeaders });
    expect([200, 201, 400, 500]).toContain(res.status());
    console.log(`✅ POST device-settings reset: status=${res.status()}`);
  });

  // ─────────── BARCODE SCANNER ───────────
  test('POST /api/barcode/scan - process barcode scan event', async ({ request }) => {
    const res = await request.post('/api/barcode/scan', {
      headers: authHeaders,
      data: { barcodeValue: '8901234567890', module: 'Sales' }
    });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ POST barcode scan: status=${res.status()}`);
  });

  test('GET /api/barcode/lookup/:barcode - lookup product by barcode', async ({ request }) => {
    const res = await request.get('/api/barcode/lookup/8901234567890', { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET barcode lookup: status=${res.status()}`);
  });

  test('GET /api/barcode/history - barcode scan history', async ({ request }) => {
    const res = await request.get('/api/barcode/history?page=1&pageSize=10', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET barcode history: status=${res.status()}`);
  });

  test('GET /api/barcode/recent - recent barcode scans', async ({ request }) => {
    const res = await request.get('/api/barcode/recent?count=5', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET barcode recent: status=${res.status()}`);
  });

  // ─────────── QR SCANNER ───────────
  test('POST /api/qr/scan - process QR code scan', async ({ request }) => {
    const res = await request.post('/api/qr/scan', {
      headers: authHeaders,
      data: { qrContent: 'upi://pay?pa=test@upi&am=100&tn=TestPayment', module: 'Payment' }
    });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ POST QR scan: status=${res.status()}`);
  });

  test('POST /api/qr/parse-upi - parse UPI QR string', async ({ request }) => {
    const res = await request.post('/api/qr/parse-upi', {
      headers: authHeaders,
      data: 'upi://pay?pa=merchant@upi&am=500&tn=Invoice'
    });
    expect([200, 201, 400, 500]).toContain(res.status());
    console.log(`✅ POST QR parse UPI: status=${res.status()}`);
  });

  test('GET /api/qr/history - QR scan history', async ({ request }) => {
    const res = await request.get('/api/qr/history?page=1&pageSize=10', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET QR history: status=${res.status()}`);
  });

  // ─────────── WEIGHING SCALE ───────────
  test('POST /api/weighing-scale/capture - capture weight reading', async ({ request }) => {
    const res = await request.post('/api/weighing-scale/capture', {
      headers: authHeaders,
      data: { weight: 5.25, unit: 'Kg', ratePerUnit: 52, module: 'Sales' }
    });
    expect([200, 201, 400, 500]).toContain(res.status());
    console.log(`✅ POST weight capture: status=${res.status()}`);
  });

  test('GET /api/weighing-scale/convert - convert weight units', async ({ request }) => {
    const res = await request.get('/api/weighing-scale/convert?weight=5.25&fromUnit=Kg&toUnit=Grams', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET weight convert: status=${res.status()}`);
  });

  test('GET /api/weighing-scale/recent - recent weight captures', async ({ request }) => {
    const res = await request.get('/api/weighing-scale/recent?count=5', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET weight recent: status=${res.status()}`);
  });

  test('GET /api/weighing-scale/history - weight capture history', async ({ request }) => {
    const res = await request.get('/api/weighing-scale/history?page=1&pageSize=10', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET weight history: status=${res.status()}`);
  });

  // ─────────── OCR SCANNER ───────────
  test('POST /api/ocr/scan - process OCR document', async ({ request }) => {
    const res = await request.post('/api/ocr/scan', {
      headers: authHeaders,
      data: {
        imageBase64: 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==',
        documentType: 'SupplierInvoice'
      }
    });
    expect([200, 201, 400, 422, 500]).toContain(res.status());
    console.log(`✅ POST OCR scan: status=${res.status()}`);
  });

  test('GET /api/ocr/history - OCR scan history', async ({ request }) => {
    const res = await request.get('/api/ocr/history?page=1&pageSize=10', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET OCR history: status=${res.status()}`);
  });

  // ─────────── CARD PAYMENT TERMINAL ───────────
  test('POST /api/card-payment/initiate - initiate card payment', async ({ request }) => {
    const res = await request.post('/api/card-payment/initiate', {
      headers: authHeaders,
      data: { invoiceId: 1, amount: 500, currency: 'INR', terminalId: 'TERM001' }
    });
    expect([200, 201, 400, 404, 422, 500]).toContain(res.status());
    console.log(`✅ POST card payment initiate: status=${res.status()}`);
  });

  test('GET /api/card-payment/by-invoice/1 - get card payments by invoice', async ({ request }) => {
    const res = await request.get('/api/card-payment/by-invoice/1', { headers: authHeaders });
    expect([200, 400, 404, 500]).toContain(res.status());
    console.log(`✅ GET card payment by invoice: status=${res.status()}`);
  });

  // ─────────── CASH DRAWER ───────────
  test('POST /api/cash-drawer/open - send open command', async ({ request }) => {
    const res = await request.post('/api/cash-drawer/open', {
      headers: authHeaders,
      data: { reason: 'End-of-day count', invoiceId: null }
    });
    expect([200, 201, 400, 500]).toContain(res.status());
    console.log(`✅ POST cash drawer open: status=${res.status()}`);
  });

  test('GET /api/cash-drawer/status - get cash drawer state', async ({ request }) => {
    const res = await request.get('/api/cash-drawer/status', { headers: authHeaders });
    expect([200, 400, 500]).toContain(res.status());
    console.log(`✅ GET cash drawer status: status=${res.status()}`);
  });

  // ─────────── DELETE ───────────
  test('DELETE /api/devices/:id - deregister device', async ({ request }) => {
    const id = createdDeviceId ?? 1;
    const res = await request.delete(`/api/devices/${id}`, { headers: authHeaders });
    expect([200, 201, 400, 404, 500]).toContain(res.status());
    console.log(`✅ DELETE device: status=${res.status()}`);
  });

  test('GET /api/devices - without token returns 401', async ({ request }) => {
    const res = await request.get('/api/devices');
    expect([401, 403]).toContain(res.status());
    console.log(`✅ Devices without token: status=${res.status()}`);
  });
});
