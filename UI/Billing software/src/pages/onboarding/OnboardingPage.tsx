import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { CheckCircle2, Store, Percent, Printer, Sparkles, ArrowRight } from 'lucide-react';
import { useStore } from '../../store/useStore';
import type { BusinessType } from '../../types';

export const OnboardingPage: React.FC = () => {
  const navigate = useNavigate();
  const { businessProfile, setBusinessProfile } = useStore();

  const [bizType, setBizType] = useState<BusinessType>('supermarket');
  const [defaultTax, setDefaultTax] = useState(18);
  const [prefix, setPrefix] = useState('INV');

  const handleComplete = () => {
    setBusinessProfile({
      businessType: bizType,
      taxSettings: {
        ...businessProfile.taxSettings,
        defaultGstRate: defaultTax,
      },
      invoiceSettings: {
        ...businessProfile.invoiceSettings,
        prefix,
      },
    });
    navigate('/dashboard');
  };

  const businessTypes: { type: BusinessType; title: string; desc: string }[] = [
    { type: 'grocery', title: 'Grocery Store', desc: 'Staples, packaged food, pulse & grains' },
    { type: 'supermarket', title: 'Supermarket', desc: 'Multi-aisle FMCG, household & fresh produce' },
    { type: 'clothing', title: 'Clothing Store', desc: 'Apparel, garments, size/color batches' },
    { type: 'electronics', title: 'Electronics & Hardware', desc: 'Serial tracking, warranty & accessories' },
  ];

  return (
    <div className="bg-white rounded-2xl shadow-2xl border border-slate-100 p-8 space-y-6 max-w-xl">
      <div className="text-center space-y-2">
        <div className="w-12 h-12 rounded-2xl bg-indigo-600 text-white flex items-center justify-center font-bold text-2xl mx-auto shadow-lg shadow-indigo-600/30">
          <Sparkles className="w-6 h-6" />
        </div>
        <h2 className="text-2xl font-bold text-slate-900 tracking-tight">Configure Your Store</h2>
        <p className="text-xs text-slate-500">Tailor BillFlow POS for {businessProfile.name}</p>
      </div>

      <div className="space-y-4">
        <div>
          <label className="block text-xs font-semibold text-slate-700 mb-2">Select Business Type</label>
          <div className="grid grid-cols-2 gap-2.5">
            {businessTypes.map((item) => (
              <button
                key={item.type}
                type="button"
                onClick={() => setBizType(item.type)}
                className={`p-3 rounded-xl border text-left transition-all ${
                  bizType === item.type
                    ? 'border-indigo-600 bg-indigo-50/60 ring-2 ring-indigo-600/20'
                    : 'border-slate-200 hover:border-slate-300 bg-slate-50/50'
                }`}
              >
                <p className="text-xs font-semibold text-slate-900">{item.title}</p>
                <p className="text-[10px] text-slate-500 mt-0.5">{item.desc}</p>
              </button>
            ))}
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Default GST Tax %</label>
            <div className="relative">
              <Percent className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="number"
                value={defaultTax}
                onChange={(e) => setDefaultTax(Number(e.target.value))}
                className="w-full pl-9 pr-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm text-slate-900"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">Invoice Prefix</label>
            <div className="relative">
              <Printer className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                value={prefix}
                onChange={(e) => setPrefix(e.target.value.toUpperCase())}
                className="w-full pl-9 pr-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm text-slate-900 font-mono uppercase"
              />
            </div>
          </div>
        </div>
      </div>

      <button
        onClick={handleComplete}
        className="w-full py-3 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-semibold text-sm shadow-lg shadow-indigo-600/30 transition-all flex items-center justify-center gap-2"
      >
        <CheckCircle2 className="w-5 h-5" />
        <span>Launch POS & Dashboard</span>
      </button>
    </div>
  );
};
