import React, { useState } from 'react';
import { X, Building2, Globe, Sparkles, Check, ArrowRight } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { useToast } from '../common/Toast';
import type { TenantPlan, PlanBillingCycle } from '../../types';

interface CreateTenantModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const CreateTenantModal: React.FC<CreateTenantModalProps> = ({ isOpen, onClose }) => {
  const { plans, createTenant } = useStore();
  const { showToast } = useToast();

  const [name, setName] = useState('');
  const [storeName, setStoreName] = useState('');
  const [subdomain, setSubdomain] = useState('');
  const [ownerName, setOwnerName] = useState('');
  const [ownerEmail, setOwnerEmail] = useState('');
  const [ownerPhone, setOwnerPhone] = useState('');
  const [industry, setIndustry] = useState<'supermarket' | 'restaurant' | 'electronics' | 'clothing' | 'pharmacy' | 'general'>('supermarket');
  const [selectedPlan, setSelectedPlan] = useState<TenantPlan>('professional');
  const [billingCycle, setBillingCycle] = useState<PlanBillingCycle>('monthly');

  if (!isOpen) return null;

  const handleNameChange = (val: string) => {
    setName(val);
    if (!storeName || storeName === name) {
      setStoreName(val);
    }
    const slug = val
      .toLowerCase()
      .replace(/[^a-z0-9]/g, '')
      .slice(0, 15);
    setSubdomain(slug);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !subdomain.trim() || !ownerName.trim() || !ownerEmail.trim()) {
      showToast('Please fill in all required tenant details', 'warning');
      return;
    }

    try {
      const newTenant = createTenant({
        name,
        storeName: storeName || name,
        subdomain,
        ownerName,
        ownerEmail,
        ownerPhone: ownerPhone || '+91 98000 00000',
        industry,
        planId: selectedPlan,
        billingCycle,
      });

      showToast(`Tenant "${newTenant.name}" provisioned and workspace activated!`, 'success');
      onClose();
    } catch (err: any) {
      showToast(err.message || 'Failed to create tenant', 'error');
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-xs animate-in fade-in duration-150">
      <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-2xl border border-slate-200 dark:border-slate-800 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div className="p-6 border-b border-slate-100 dark:border-slate-800 flex items-center justify-between sticky top-0 bg-white/95 dark:bg-slate-900/95 backdrop-blur-sm z-10">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-indigo-600 text-white flex items-center justify-center shadow-md shadow-indigo-600/30">
              <Building2 className="w-5 h-5" />
            </div>
            <div>
              <h2 className="text-lg font-bold text-slate-900 dark:text-white">Provision New Tenant Workspace</h2>
              <p className="text-xs text-slate-500">Create a new isolated SaaS store database and tenant account</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 p-1.5 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {/* Tenant Business Details */}
          <div className="space-y-4">
            <h3 className="text-xs font-bold uppercase tracking-wider text-slate-400">1. Business Profile</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Company / Organization Name *
                </label>
                <input
                  type="text"
                  required
                  placeholder="e.g. Royal Organic Supermart"
                  value={name}
                  onChange={(e) => handleNameChange(e.target.value)}
                  className="w-full px-3.5 py-2 text-xs border border-slate-200 dark:border-slate-700 rounded-xl dark:bg-slate-800 dark:text-white focus:outline-none focus:border-indigo-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Store / Outlet Display Name
                </label>
                <input
                  type="text"
                  placeholder="e.g. Royal Organic (Flagship)"
                  value={storeName}
                  onChange={(e) => setStoreName(e.target.value)}
                  className="w-full px-3.5 py-2 text-xs border border-slate-200 dark:border-slate-700 rounded-xl dark:bg-slate-800 dark:text-white focus:outline-none focus:border-indigo-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Subdomain Slug *
                </label>
                <div className="flex items-center">
                  <span className="px-2.5 py-2 bg-slate-100 dark:bg-slate-800 text-slate-500 border border-r-0 border-slate-200 dark:border-slate-700 rounded-l-xl text-xs font-mono">
                    https://
                  </span>
                  <input
                    type="text"
                    required
                    placeholder="royalorganics"
                    value={subdomain}
                    onChange={(e) => setSubdomain(e.target.value.toLowerCase().replace(/[^a-z0-9]/g, ''))}
                    className="w-full px-3 py-2 text-xs border-y border-slate-200 dark:border-slate-700 dark:bg-slate-800 dark:text-white font-mono focus:outline-none focus:border-indigo-500"
                  />
                  <span className="px-2.5 py-2 bg-slate-100 dark:bg-slate-800 text-slate-500 border border-l-0 border-slate-200 dark:border-slate-700 rounded-r-xl text-xs font-mono">
                    .billflow.io
                  </span>
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Industry / Business Type
                </label>
                <select
                  value={industry}
                  onChange={(e) => setIndustry(e.target.value as any)}
                  className="w-full px-3.5 py-2 text-xs border border-slate-200 dark:border-slate-700 rounded-xl dark:bg-slate-800 dark:text-white focus:outline-none focus:border-indigo-500"
                >
                  <option value="supermarket">Supermarket & Grocery Retail</option>
                  <option value="restaurant">Restaurant, Cafe & Bakery</option>
                  <option value="electronics">Electronics & Gadgets</option>
                  <option value="clothing">Fashion & Apparel</option>
                  <option value="pharmacy">Pharmacy & Health</option>
                  <option value="general">General Retail & Wholesale</option>
                </select>
              </div>
            </div>
          </div>

          {/* Tenant Owner / Admin Contact */}
          <div className="space-y-4">
            <h3 className="text-xs font-bold uppercase tracking-wider text-slate-400">2. Owner Credentials</h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Owner Full Name *
                </label>
                <input
                  type="text"
                  required
                  placeholder="e.g. Arvind Mehta"
                  value={ownerName}
                  onChange={(e) => setOwnerName(e.target.value)}
                  className="w-full px-3.5 py-2 text-xs border border-slate-200 dark:border-slate-700 rounded-xl dark:bg-slate-800 dark:text-white focus:outline-none focus:border-indigo-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Owner Email Address *
                </label>
                <input
                  type="email"
                  required
                  placeholder="arvind@store.com"
                  value={ownerEmail}
                  onChange={(e) => setOwnerEmail(e.target.value)}
                  className="w-full px-3.5 py-2 text-xs border border-slate-200 dark:border-slate-700 rounded-xl dark:bg-slate-800 dark:text-white focus:outline-none focus:border-indigo-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Phone Number
                </label>
                <input
                  type="tel"
                  placeholder="+91 98765 00000"
                  value={ownerPhone}
                  onChange={(e) => setOwnerPhone(e.target.value)}
                  className="w-full px-3.5 py-2 text-xs border border-slate-200 dark:border-slate-700 rounded-xl dark:bg-slate-800 dark:text-white focus:outline-none focus:border-indigo-500"
                />
              </div>
            </div>
          </div>

          {/* Subscription Tier Selection */}
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="text-xs font-bold uppercase tracking-wider text-slate-400">3. SaaS Subscription Plan</h3>
              <div className="flex items-center gap-2 bg-slate-100 dark:bg-slate-800 p-1 rounded-lg text-[11px] font-semibold">
                <button
                  type="button"
                  onClick={() => setBillingCycle('monthly')}
                  className={`px-2.5 py-1 rounded-md transition-all ${
                    billingCycle === 'monthly' ? 'bg-white dark:bg-slate-700 text-indigo-600 dark:text-indigo-400 shadow-2xs' : 'text-slate-500'
                  }`}
                >
                  Monthly
                </button>
                <button
                  type="button"
                  onClick={() => setBillingCycle('yearly')}
                  className={`px-2.5 py-1 rounded-md transition-all flex items-center gap-1 ${
                    billingCycle === 'yearly' ? 'bg-white dark:bg-slate-700 text-indigo-600 dark:text-indigo-400 shadow-2xs' : 'text-slate-500'
                  }`}
                >
                  Annual <span className="text-[9px] bg-emerald-100 text-emerald-700 px-1 rounded font-bold">20% OFF</span>
                </button>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
              {plans.map((p) => {
                const isSelected = selectedPlan === p.id;
                const price = billingCycle === 'yearly' ? p.priceYearly : p.priceMonthly;
                return (
                  <div
                    key={p.id}
                    onClick={() => setSelectedPlan(p.id)}
                    className={`p-4 rounded-xl border-2 cursor-pointer transition-all ${
                      isSelected
                        ? 'border-indigo-600 bg-indigo-50/40 dark:bg-indigo-950/30 shadow-md'
                        : 'border-slate-200 dark:border-slate-700 hover:border-slate-300'
                    }`}
                  >
                    <div className="flex items-center justify-between mb-2">
                      <span className="font-bold text-xs text-slate-900 dark:text-white">{p.name}</span>
                      {isSelected && <Check className="w-4 h-4 text-indigo-600" />}
                    </div>
                    <p className="text-lg font-extrabold text-slate-900 dark:text-white">
                      ₹{price.toLocaleString()}
                      <span className="text-[10px] text-slate-400 font-normal">/mo</span>
                    </p>
                    <p className="text-[11px] text-slate-500 mt-1 line-clamp-2">{p.tagline}</p>
                    <div className="mt-3 pt-2 border-t border-slate-200/60 dark:border-slate-800 text-[10px] text-slate-600 dark:text-slate-400 space-y-0.5 font-medium">
                      <p>• {p.maxMonthlyInvoices === -1 ? 'Unlimited' : `${p.maxMonthlyInvoices} Invoices/mo`}</p>
                      <p>• {p.maxRegisters} POS Register{p.maxRegisters > 1 ? 's' : ''}</p>
                      <p>• {p.maxStaff} Team Members</p>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="pt-4 border-t border-slate-100 dark:border-slate-800 flex items-center justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-xs font-semibold text-slate-600 hover:text-slate-800 dark:text-slate-300"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="flex items-center gap-2 px-5 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-bold shadow-lg shadow-indigo-600/30 transition-all"
            >
              <span>Provision & Switch Workspace</span>
              <ArrowRight className="w-4 h-4" />
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
