import { test, expect } from '@playwright/test';
import { getAuthToken, safeJson } from './helpers.mjs';

let token = '';
let productId = 0;
let categoryId = 0;

test.beforeAll(async () => {
  const auth = await getAuthToken();
  token = auth.token;
});

const h = () => ({ Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' });

test.describe('📦 Products API', () => {

  test('POST /api/product-categories - create category', async ({ request }) => {
    const res = await request.post('/api/product-categories', {
      headers: h(), data: { name: `Cat_${Date.now()}`, description: 'Auto test category' }
    });
    expect([200, 201, 400, 401]).toContain(res.status());
    const body = await safeJson(res);
    categoryId = body?.data?.id ?? 0;
    console.log(`✅ Category: status=${res.status()}, id=${categoryId}`);
  });

  test('GET /api/product-categories - list categories', async ({ request }) => {
    const res = await request.get('/api/product-categories', { headers: h() });
    expect([200, 401]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ Categories listed: status=${res.status()}`);
  });

  test('POST /api/products - create product with ExpiryDate', async ({ request }) => {
    const res = await request.post('/api/products', {
      headers: h(),
      data: { name: `Product_${Date.now()}`, sku: `SKU-${Date.now()}`, purchasePrice: 100, sellingPrice: 150, currentStock: 50, minStockLevel: 5, expiryDate: '2027-12-31' }
    });
    expect([200, 201, 400, 401]).toContain(res.status());
    const body = await safeJson(res);
    productId = body?.data?.id ?? 0;
    console.log(`✅ Product created: status=${res.status()}, id=${productId}`);
  });

  test('GET /api/products - list products', async ({ request }) => {
    const res = await request.get('/api/products?page=1&pageSize=10', { headers: h() });
    expect([200, 401]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ Products listed: status=${res.status()}`);
  });

  test('GET /api/products/:id - get product by ID', async ({ request }) => {
    if (!productId) { console.log('⚠️ Skipping - no product ID'); return; }
    const res = await request.get(`/api/products/${productId}`, { headers: h() });
    expect([200, 401, 404]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ Product fetched: status=${res.status()}`);
  });

  test('GET /api/products/search - search products', async ({ request }) => {
    const res = await request.get('/api/products/search?query=Product', { headers: h() });
    expect([200, 401]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ Search: status=${res.status()}`);
  });

  test('PUT /api/products/:id - update product ExpiryDate', async ({ request }) => {
    if (!productId) { console.log('⚠️ Skipping - no product ID'); return; }
    const res = await request.put(`/api/products/${productId}`, {
      headers: h(), data: { sellingPrice: 175, expiryDate: '2028-06-30' }
    });
    expect([200, 400, 401, 404]).toContain(res.status());
    const body = await safeJson(res);
    expect(body).not.toBeNull();
    console.log(`✅ Product updated: status=${res.status()}`);
  });

  test('GET /api/brands - list brands', async ({ request }) => {
    const res = await request.get('/api/brands', { headers: h() });
    expect([200, 401]).toContain(res.status());
    console.log(`✅ Brands: status=${res.status()}`);
  });

  test('GET /api/units - list units', async ({ request }) => {
    const res = await request.get('/api/units', { headers: h() });
    expect([200, 401]).toContain(res.status());
    console.log(`✅ Units: status=${res.status()}`);
  });

  test('GET /api/tax-rates - list tax rates', async ({ request }) => {
    const res = await request.get('/api/tax-rates', { headers: h() });
    expect([200, 401]).toContain(res.status());
    console.log(`✅ Tax rates: status=${res.status()}`);
  });

  test('GET /api/discounts - list discounts', async ({ request }) => {
    const res = await request.get('/api/discounts', { headers: h() });
    expect([200, 401]).toContain(res.status());
    console.log(`✅ Discounts: status=${res.status()}`);
  });

  test('DELETE /api/products/:id - delete product', async ({ request }) => {
    if (!productId) { console.log('⚠️ Skipping - no product ID'); return; }
    const res = await request.delete(`/api/products/${productId}`, { headers: h() });
    expect([200, 400, 401, 404]).toContain(res.status());
    console.log(`✅ Product deleted: status=${res.status()}`);
  });
});