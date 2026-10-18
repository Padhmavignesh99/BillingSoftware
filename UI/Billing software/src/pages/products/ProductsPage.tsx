import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Search,
  Plus,
  Edit,
  Trash2,
  FileSpreadsheet,
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { Badge } from '../../components/ui/Badge';
import { exportToCSV } from '../../utils/exportUtils';
import { useToast } from '../../components/common/Toast';
import { Modal } from '../../components/ui/Modal';

export const ProductsPage: React.FC = () => {
  const navigate = useNavigate();
  const { products, categories, deleteProduct, adjustStock } = useStore();
  const { showToast } = useToast();

  const [searchQuery, setSearchQuery] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [stockStatusFilter, setStockStatusFilter] = useState('all');

  const [adjustingProduct, setAdjustingProduct] = useState<any>(null);
  const [adjDelta, setAdjDelta] = useState<number>(10);
  const [adjReason, setAdjReason] = useState('Stock Purchase Arrival');

  const filteredProducts = products.filter((prod) => {
    const matchesCat = categoryFilter === 'all' || prod.category === categoryFilter;
    const matchesStock =
      stockStatusFilter === 'all' ||
      (stockStatusFilter === 'low_stock' && (prod.stock <= prod.minStock || prod.status === 'low_stock')) ||
      (stockStatusFilter === 'out_of_stock' && prod.stock === 0) ||
      (stockStatusFilter === 'active' && prod.stock > prod.minStock);

    const q = searchQuery.toLowerCase().trim();
    const matchesQ =
      !q ||
      prod.name.toLowerCase().includes(q) ||
      prod.sku.toLowerCase().includes(q) ||
      prod.barcode.toLowerCase().includes(q) ||
      prod.brand.toLowerCase().includes(q);

    return matchesCat && matchesStock && matchesQ;
  });

  const handleExportCSV = () => {
    exportToCSV('BillFlow_Products_Catalog', filteredProducts, [
      { key: 'name', label: 'Product Name' },
      { key: 'sku', label: 'SKU' },
      { key: 'barcode', label: 'Barcode' },
      { key: 'category', label: 'Category' },
      { key: 'sellingPrice', label: 'Selling Price (₹)' },
      { key: 'mrp', label: 'MRP (₹)' },
      { key: 'taxRate', label: 'GST Tax %' },
      { key: 'stock', label: 'Current Stock' },
      { key: 'unit', label: 'Unit' },
      { key: 'status', label: 'Status' },
    ]);
    showToast('Product catalog exported as CSV', 'success');
  };

  const handleConfirmStockAdj = async () => {
    if (!adjustingProduct) return;
    try {
      await adjustStock(adjustingProduct.id, adjDelta, adjReason);
      setAdjustingProduct(null);
      showToast(`Stock updated for ${adjustingProduct.name}`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Could not adjust stock', 'error');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900 tracking-tight">Product Catalog</h1>
          <p className="text-xs sm:text-sm text-slate-500">
            Manage product inventory, pricing, SKUs, barcodes, and stock levels
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={handleExportCSV}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-xs font-semibold transition-colors"
          >
            <FileSpreadsheet className="w-4 h-4 text-emerald-600" />
            <span>Export CSV</span>
          </button>

          <button
            onClick={() => navigate('/products/new')}
            className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-md shadow-indigo-600/30 transition-all"
          >
            <Plus className="w-4 h-4" />
            <span>Add New Product</span>
          </button>
        </div>
      </div>

      {/* Filter Controls */}
      <div className="bg-white p-4 rounded-2xl border border-slate-200/80 shadow-2xs flex flex-wrap items-center justify-between gap-3">
        <div className="relative flex-1 min-w-[240px]">
          <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search by product name, SKU, barcode, brand..."
            className="w-full pl-10 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-600/30"
          />
        </div>

        <div className="flex items-center gap-2">
          <select
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
            className="px-3 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-700"
          >
            <option value="all">All Categories</option>
            {categories.map((c) => (
              <option key={c.id} value={c.name}>
                {c.name}
              </option>
            ))}
          </select>

          <select
            value={stockStatusFilter}
            onChange={(e) => setStockStatusFilter(e.target.value)}
            className="px-3 py-2 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-700"
          >
            <option value="all">All Stock Statuses</option>
            <option value="active">In Stock</option>
            <option value="low_stock">Low Stock Warning</option>
            <option value="out_of_stock">Out of Stock</option>
          </select>
        </div>
      </div>

      {/* Products Table */}
      <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50/80 text-slate-500 font-semibold border-b border-slate-100">
              <tr>
                <th className="py-3.5 px-4">Product Name</th>
                <th className="py-3.5 px-4">SKU / Barcode</th>
                <th className="py-3.5 px-4">Category</th>
                <th className="py-3.5 px-4">Selling Price</th>
                <th className="py-3.5 px-4">GST Rate</th>
                <th className="py-3.5 px-4">Stock Qty</th>
                <th className="py-3.5 px-4">Status</th>
                <th className="py-3.5 px-4 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 font-medium">
              {filteredProducts.length === 0 ? (
                <tr>
                  <td colSpan={8} className="text-center py-10 text-slate-400">
                    No products found matching filters.
                  </td>
                </tr>
              ) : (
                filteredProducts.map((prod) => (
                  <tr key={prod.id} className="hover:bg-slate-50/60 transition-colors">
                    <td className="py-3.5 px-4 font-bold text-slate-900">
                      <p>{prod.name}</p>
                      <p className="text-[10px] text-slate-400 font-normal">{prod.brand}</p>
                    </td>
                    <td className="py-3.5 px-4 font-mono text-slate-600">
                      <p>{prod.sku}</p>
                      <p className="text-[10px] text-slate-400">{prod.barcode}</p>
                    </td>
                    <td className="py-3.5 px-4 text-slate-700">{prod.category}</td>
                    <td className="py-3.5 px-4 font-bold text-slate-900">
                      ₹{prod.sellingPrice}{' '}
                      {prod.mrp > prod.sellingPrice && (
                        <span className="text-[10px] text-slate-400 line-through font-normal">₹{prod.mrp}</span>
                      )}
                    </td>
                    <td className="py-3.5 px-4 font-mono text-slate-600">{prod.taxRate}%</td>
                    <td className="py-3.5 px-4 font-bold text-slate-900">
                      {prod.stock} {prod.unit}
                    </td>
                    <td className="py-3.5 px-4">
                      <Badge
                        variant={prod.stock > prod.minStock ? 'success' : prod.stock > 0 ? 'warning' : 'danger'}
                        size="sm"
                      >
                        {prod.stock === 0 ? 'Out of stock' : prod.stock <= prod.minStock ? 'Low stock' : 'Active'}
                      </Badge>
                    </td>
                    <td className="py-3.5 px-4 text-right space-x-1">
                      <button
                        onClick={() => setAdjustingProduct(prod)}
                        className="px-2 py-1 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded text-[11px] font-semibold"
                      >
                        Adjust Stock
                      </button>

                      <button
                        onClick={() => navigate(`/products/new?edit=${prod.id}`)}
                        className="p-1.5 text-slate-600 hover:text-indigo-600 hover:bg-slate-100 rounded-lg"
                      >
                        <Edit className="w-4 h-4" />
                      </button>

                      <button
                        onClick={async () => {
                          try {
                            await deleteProduct(prod.id);
                            showToast(`Deleted ${prod.name}`, 'info');
                          } catch (err: any) {
                            showToast(err.message || 'Could not delete product', 'error');
                          }
                        }}
                        className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* QUICK STOCK ADJUSTMENT MODAL */}
      {adjustingProduct && (
        <Modal
          isOpen={!!adjustingProduct}
          onClose={() => setAdjustingProduct(null)}
          title={`Adjust Stock - ${adjustingProduct.name}`}
          maxWidth="md"
        >
          <div className="space-y-4">
            <div className="p-3 bg-slate-50 rounded-xl border border-slate-200 text-xs flex justify-between">
              <span>Current Stock: <strong>{adjustingProduct.stock} {adjustingProduct.unit}</strong></span>
              <span>Min Stock: <strong>{adjustingProduct.minStock} {adjustingProduct.unit}</strong></span>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">
                Stock Delta Quantity (+ for add stock, - for reduce)
              </label>
              <input
                type="number"
                value={adjDelta}
                onChange={(e) => setAdjDelta(Number(e.target.value))}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm font-bold"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">Reason for Stock Adjustment</label>
              <input
                type="text"
                value={adjReason}
                onChange={(e) => setAdjReason(e.target.value)}
                className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-medium"
              />
            </div>

            <button
              onClick={handleConfirmStockAdj}
              className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-bold text-xs shadow-md"
            >
              Update Stock Level
            </button>
          </div>
        </Modal>
      )}
    </div>
  );
};
