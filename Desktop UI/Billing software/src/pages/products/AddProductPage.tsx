import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { ArrowLeft, Save, Package, Percent, Layers, ChevronRight } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { useToast } from '../../components/common/Toast';
import {
  getRootCategories,
  getSubCategories,
  getLeafPath,
} from '../../utils/categoryUtils';

export const AddProductPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const editId = searchParams.get('edit');

  const { products, categories, suppliers, addProduct, updateProduct } = useStore();
  const { showToast } = useToast();

  const existing = editId ? products.find((p) => p.id === editId) : null;

  // ── Basic fields ───────────────────────────────────────────────────────────
  const [name, setName]               = useState(existing?.name || '');
  const [sku, setSku]                 = useState(existing?.sku || `SKU-${Math.floor(1000 + Math.random() * 9000)}`);
  const [barcode, setBarcode]         = useState(existing?.barcode || `890${Math.floor(10000000000 + Math.random() * 90000000000)}`);
  const [brand, setBrand]             = useState(existing?.brand || 'Standard');
  const [sellingPrice, setSellingPrice] = useState(existing?.sellingPrice || 125);
  const [mrp, setMrp]                 = useState(existing?.mrp || 130);
  const [purchasePrice, setPurchasePrice] = useState(existing?.purchasePrice || 100);
  const [taxRate, setTaxRate]         = useState(existing?.taxRate || 18);
  const [unit, setUnit]               = useState<any>(existing?.unit || 'pcs');
  const [openingStock, setOpeningStock] = useState(existing?.stock || 50);
  const [minStock, setMinStock]       = useState(existing?.minStock || 10);
  const [supplierId, setSupplierId]   = useState(existing?.supplierId || suppliers[0]?.id || '');

  // ── Category cascade ───────────────────────────────────────────────────────
  const roots = getRootCategories(categories);

  // Pre-fill from existing product
  const initRootId = existing?.categoryId || '';
  const initSubId  = existing?.subCategoryId || '';
  const initSubSubId = existing?.subSubCategoryId || '';

  const [rootCatId, setRootCatId]       = useState(initRootId);
  const [subCatId, setSubCatId]         = useState(initSubId);
  const [subSubCatId, setSubSubCatId]   = useState(initSubSubId);

  // Derived child lists
  const subCats    = rootCatId ? getSubCategories(categories, rootCatId) : [];
  const subSubCats = subCatId  ? getSubCategories(categories, subCatId)  : [];

  // Reset children when parent changes
  useEffect(() => { setSubCatId(''); setSubSubCatId(''); }, [rootCatId]);
  useEffect(() => { setSubSubCatId(''); }, [subCatId]);

  // Compute the final leaf name for saving (deepest selected level)
  const leafCatId   = subSubCatId || subCatId || rootCatId;
  const leafCatName = leafCatId
    ? (categories.find((c) => c.id === leafCatId)?.name ?? '')
    : '';
  const categoryPath = leafCatId ? getLeafPath(categories, leafCatId) : '';

  // ── Submit ─────────────────────────────────────────────────────────────────
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name || !sku) return;
    if (!rootCatId) { showToast('Please select a category', 'error'); return; }

    const catPayload = {
      category:        categories.find((c) => c.id === rootCatId)?.name ?? '',
      categoryId:      rootCatId,
      subCategory:     subCatId    ? categories.find((c) => c.id === subCatId)?.name    : undefined,
      subCategoryId:   subCatId    || undefined,
      subSubCategory:  subSubCatId ? categories.find((c) => c.id === subSubCatId)?.name : undefined,
      subSubCategoryId: subSubCatId || undefined,
    };

    try {
      if (existing) {
        await updateProduct(existing.id, {
          name, sku, barcode, ...catPayload, brand,
          purchasePrice, sellingPrice, mrp, taxRate,
          unit, stock: openingStock, minStock, supplierId,
        });
        showToast(`Updated product ${name}`, 'success');
      } else {
        await addProduct({
          name, sku, barcode, ...catPayload, brand,
          description: '',
          purchasePrice, sellingPrice, mrp, taxRate,
          unit, stock: openingStock, minStock, supplierId,
          supplierName: suppliers.find((s) => s.id === supplierId)?.companyName,
          status: openingStock > minStock ? 'active' : 'low_stock',
        });
        showToast(`Created product ${name}`, 'success');
      }
      navigate('/products');
    } catch (err: any) {
      showToast(err.message || 'Could not save product', 'error');
    }
  };

  // ── UI ─────────────────────────────────────────────────────────────────────
  const selectCls = 'w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold text-slate-700 focus:outline-none focus:ring-2 focus:ring-indigo-500/30';

  return (
    <div className="space-y-6 w-full">
      {/* Top Header */}
      <div className="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div className="flex items-center gap-3">
          <button
            onClick={() => navigate('/products')}
            className="p-2 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <h1 className="text-xl font-bold text-slate-900">{existing ? 'Edit Product' : 'Add New Product'}</h1>
            <p className="text-xs text-slate-500">Configure catalog details, tax GST, prices and opening stock</p>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Section 1: Basic Information */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
          <h3 className="text-sm font-bold text-slate-900 border-b border-slate-100 pb-2 flex items-center gap-2">
            <Package className="w-4 h-4 text-indigo-600" />
            <span>Basic Product Information</span>
          </h3>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {/* Product Name */}
            <div className="sm:col-span-2">
              <label className="block text-xs font-semibold text-slate-700 mb-1">Product Title / Name *</label>
              <input
                type="text"
                required
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
              />
            </div>

            {/* ── Category Cascade ── */}
            <div className="sm:col-span-2">
              <label className="block text-xs font-semibold text-slate-700 mb-2">
                Category *
                {categoryPath && (
                  <span className="ml-2 text-indigo-600 font-normal">
                    <ChevronRight className="w-3 h-3 inline" /> {categoryPath}
                  </span>
                )}
              </label>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                {/* Level 0: Root Category */}
                <div>
                  <label className="block text-[10px] font-medium text-slate-500 mb-1 uppercase tracking-wide">Category *</label>
                  <select
                    value={rootCatId}
                    onChange={(e) => setRootCatId(e.target.value)}
                    className={selectCls}
                    required
                  >
                    <option value="">— Select Category —</option>
                    {roots.map((c) => (
                      <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                  </select>
                </div>

                {/* Level 1: Sub-Category (only if root has children) */}
                <div>
                  <label className="block text-[10px] font-medium text-slate-500 mb-1 uppercase tracking-wide">Sub-Category</label>
                  <select
                    value={subCatId}
                    onChange={(e) => setSubCatId(e.target.value)}
                    className={selectCls}
                    disabled={subCats.length === 0}
                  >
                    <option value="">{subCats.length === 0 ? '— None —' : '— Select Sub-Category —'}</option>
                    {subCats.map((c) => (
                      <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                  </select>
                </div>

                {/* Level 2: Sub-Sub-Category (only if sub has children) */}
                <div>
                  <label className="block text-[10px] font-medium text-slate-500 mb-1 uppercase tracking-wide">Sub-Sub Category</label>
                  <select
                    value={subSubCatId}
                    onChange={(e) => setSubSubCatId(e.target.value)}
                    className={selectCls}
                    disabled={subSubCats.length === 0}
                  >
                    <option value="">{subSubCats.length === 0 ? '— None —' : '— Select Sub-Sub Category —'}</option>
                    {subSubCats.map((c) => (
                      <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                  </select>
                </div>
              </div>

              {/* Breadcrumb Preview */}
              {categoryPath && (
                <div className="mt-2 flex items-center gap-1.5 text-[11px] text-slate-500 bg-indigo-50 border border-indigo-100 rounded-lg px-3 py-1.5">
                  <span className="font-medium text-indigo-700">Selected Path:</span>
                  <span className="text-indigo-600 font-semibold">{categoryPath}</span>
                </div>
              )}
            </div>

            {/* Brand */}
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Brand Name</label>
              <input
                type="text"
                value={brand}
                onChange={(e) => setBrand(e.target.value)}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-medium focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
              />
            </div>

            {/* SKU */}
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">SKU Code *</label>
              <input
                type="text"
                required
                value={sku}
                onChange={(e) => setSku(e.target.value)}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono font-semibold focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
              />
            </div>

            {/* Barcode */}
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Barcode Number *</label>
              <input
                type="text"
                required
                value={barcode}
                onChange={(e) => setBarcode(e.target.value)}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono font-semibold focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
              />
            </div>
          </div>
        </div>

        {/* Section 2: Pricing & Tax */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
          <h3 className="text-sm font-bold text-slate-900 border-b border-slate-100 pb-2 flex items-center gap-2">
            <Percent className="w-4 h-4 text-emerald-600" />
            <span>Pricing & Tax GST</span>
          </h3>

          <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Selling Price (₹) *</label>
              <input
                type="number" step="0.1" required value={sellingPrice}
                onChange={(e) => setSellingPrice(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-bold text-slate-900"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">MRP Price (₹) *</label>
              <input
                type="number" step="0.1" required value={mrp}
                onChange={(e) => setMrp(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-bold text-slate-900"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Purchase Cost (₹)</label>
              <input
                type="number" step="0.1" value={purchasePrice}
                onChange={(e) => setPurchasePrice(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-medium text-slate-700"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">GST Tax Rate %</label>
              <select
                value={taxRate}
                onChange={(e) => setTaxRate(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
              >
                <option value={0}>0% (Exempt)</option>
                <option value={5}>5% GST</option>
                <option value={12}>12% GST</option>
                <option value={18}>18% GST</option>
                <option value={28}>28% GST</option>
              </select>
            </div>
          </div>
        </div>

        {/* Section 3: Inventory & Supplier */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
          <h3 className="text-sm font-bold text-slate-900 border-b border-slate-100 pb-2 flex items-center gap-2">
            <Layers className="w-4 h-4 text-amber-600" />
            <span>Stock & Supplier Setup</span>
          </h3>

          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Opening Stock Qty</label>
              <input
                type="number" value={openingStock}
                onChange={(e) => setOpeningStock(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-bold text-slate-900"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Min Low Stock Threshold</label>
              <input
                type="number" value={minStock}
                onChange={(e) => setMinStock(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-bold text-slate-900"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Measurement Unit</label>
              <select
                value={unit}
                onChange={(e) => setUnit(e.target.value)}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
              >
                <option value="pcs">Pcs / Units</option>
                <option value="pack">Pack</option>
                <option value="kg">Kg (Kilograms)</option>
                <option value="box">Box</option>
                <option value="liter">Liter</option>
              </select>
            </div>
          </div>
        </div>

        {/* Form Action Submit Buttons */}
        <div className="flex items-center justify-end gap-3 pt-2">
          <button
            type="button"
            onClick={() => navigate('/products')}
            className="px-5 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-xs font-semibold"
          >
            Cancel
          </button>
          <button
            type="submit"
            className="px-6 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-bold shadow-md shadow-indigo-600/30 flex items-center gap-2"
          >
            <Save className="w-4 h-4" />
            <span>{existing ? 'Update Product' : 'Save & Publish Product'}</span>
          </button>
        </div>
      </form>
    </div>
  );
};
