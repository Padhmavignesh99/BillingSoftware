import { useState, useEffect } from 'react';
import { Package, Plus, Trash2, Tag, ChevronRight, Edit3 } from 'lucide-react';
import apiClient from '../../services/apiClient';

interface AttributeValue {
  id: number;
  value: string;
  colorHex?: string;
}

interface Attribute {
  id: number;
  name: string;
  description?: string;
  values: AttributeValue[];
}

interface Variant {
  id: number;
  sku: string;
  sellingPrice?: number;
  purchasePrice?: number;
  currentStock: number;
  isActive: boolean;
  variantAttributes: { attributeValue: { value: string; attribute: { name: string } } }[];
}

export default function ProductVariantsPage() {
  const [attributes, setAttributes] = useState<Attribute[]>([]);
  const [selectedAttr, setSelectedAttr] = useState<Attribute | null>(null);
  const [newAttrName, setNewAttrName] = useState('');
  const [newValue, setNewValue] = useState('');
  const [newColorHex, setNewColorHex] = useState('');
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState<'attributes' | 'variants'>('attributes');

  useEffect(() => { loadAttributes(); }, []);

  const loadAttributes = async () => {
    setLoading(true);
    try {
      const res = await apiClient.get('/product-variants/attributes');
      setAttributes(res.data.data || []);
    } catch { /* ignore */ } finally { setLoading(false); }
  };

  const createAttribute = async () => {
    if (!newAttrName.trim()) return;
    await apiClient.post('/product-variants/attributes', { name: newAttrName, description: null });
    setNewAttrName('');
    loadAttributes();
  };

  const addValue = async (attrId: number) => {
    if (!newValue.trim()) return;
    await apiClient.post(`/product-variants/attributes/${attrId}/values`, {
      value: newValue, colorHex: newColorHex || null
    });
    setNewValue(''); setNewColorHex('');
    loadAttributes();
  };

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center gap-3">
        <Package className="w-7 h-7 text-emerald-600" />
        <div>
          <h1 className="text-xl font-bold text-gray-800">Product Variants</h1>
          <p className="text-xs text-gray-500">Manage size, color, material & other variant attributes</p>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 border-b border-gray-200">
        {(['attributes', 'variants'] as const).map(t => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-4 py-2 text-sm font-medium capitalize border-b-2 transition-colors ${tab === t ? 'border-emerald-500 text-emerald-600' : 'border-transparent text-gray-500 hover:text-gray-700'}`}>
            {t}
          </button>
        ))}
      </div>

      {tab === 'attributes' && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Create Attribute */}
          <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
            <h2 className="font-semibold text-gray-700 flex items-center gap-2"><Tag className="w-4 h-4 text-emerald-500" /> New Attribute</h2>
            <div className="flex gap-2">
              <input value={newAttrName} onChange={e => setNewAttrName(e.target.value)}
                placeholder="e.g. Size, Color, Material"
                className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-emerald-500 focus:outline-none" />
              <button onClick={createAttribute}
                className="bg-emerald-600 text-white px-4 py-2 rounded-lg hover:bg-emerald-700 text-sm flex items-center gap-1">
                <Plus className="w-4 h-4" /> Add
              </button>
            </div>

            <h3 className="font-medium text-gray-600 text-sm mt-4">Existing Attributes</h3>
            {loading ? (
              <p className="text-gray-400 text-sm">Loading...</p>
            ) : (
              <div className="space-y-2 max-h-64 overflow-y-auto">
                {attributes.map(attr => (
                  <button key={attr.id} onClick={() => setSelectedAttr(attr)}
                    className={`w-full text-left px-3 py-2 rounded-lg border text-sm flex items-center justify-between transition-colors
                      ${selectedAttr?.id === attr.id ? 'border-emerald-400 bg-emerald-50 text-emerald-700' : 'border-gray-200 hover:bg-gray-50'}`}>
                    <span className="font-medium">{attr.name} <span className="text-gray-400">({attr.values.length} values)</span></span>
                    <ChevronRight className="w-4 h-4" />
                  </button>
                ))}
                {attributes.length === 0 && <p className="text-gray-400 text-sm">No attributes yet</p>}
              </div>
            )}
          </div>

          {/* Attribute Values */}
          {selectedAttr && (
            <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
              <h2 className="font-semibold text-gray-700 flex items-center gap-2">
                <Edit3 className="w-4 h-4 text-emerald-500" /> Values for "{selectedAttr.name}"
              </h2>
              <div className="flex gap-2">
                <input value={newValue} onChange={e => setNewValue(e.target.value)}
                  placeholder="e.g. Red, XL, Cotton"
                  className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-emerald-500 focus:outline-none" />
                <input type="color" value={newColorHex || '#10b981'} onChange={e => setNewColorHex(e.target.value)}
                  className="w-10 h-10 rounded border border-gray-300 cursor-pointer" title="Color (optional)" />
                <button onClick={() => addValue(selectedAttr.id)}
                  className="bg-emerald-600 text-white px-4 py-2 rounded-lg hover:bg-emerald-700 text-sm flex items-center gap-1">
                  <Plus className="w-4 h-4" /> Add
                </button>
              </div>

              <div className="space-y-2 max-h-64 overflow-y-auto">
                {selectedAttr.values.map(val => (
                  <div key={val.id} className="flex items-center gap-2 px-3 py-2 bg-gray-50 rounded-lg border border-gray-100">
                    {val.colorHex && (
                      <span className="w-5 h-5 rounded-full border border-gray-300 flex-shrink-0" style={{ backgroundColor: val.colorHex }} />
                    )}
                    <span className="text-sm text-gray-700 flex-1">{val.value}</span>
                    <button className="text-red-400 hover:text-red-600"><Trash2 className="w-4 h-4" /></button>
                  </div>
                ))}
                {selectedAttr.values.length === 0 && <p className="text-gray-400 text-sm">No values yet</p>}
              </div>
            </div>
          )}
        </div>
      )}

      {tab === 'variants' && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <p className="text-sm text-gray-500">
            Select a product from the <strong>Products</strong> page and use the <strong>Variants</strong> tab there to manage its specific size/color combinations.
          </p>
        </div>
      )}
    </div>
  );
}

