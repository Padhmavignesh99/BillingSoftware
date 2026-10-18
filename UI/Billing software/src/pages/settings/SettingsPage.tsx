import React, { useState } from 'react';
import { Store, Receipt, Percent, Palette, Save } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { useToast } from '../../components/common/Toast';
import { api } from '../../services/apiClient';

export const SettingsPage: React.FC = () => {
  const { businessProfile, setBusinessProfile, theme, toggleTheme, backendStatus } = useStore();
  const { showToast } = useToast();

  const [activeTab, setActiveTab] = useState<'profile' | 'invoice' | 'tax' | 'payment'>('profile');
  const [saving, setSaving] = useState(false);

  // Form State
  const [storeName, setStoreName] = useState(businessProfile.storeName);
  const [address, setAddress] = useState(businessProfile.address);
  const [phone, setPhone] = useState(businessProfile.phone);
  const [gstin, setGstin] = useState(businessProfile.gstin);
  const [prefix, setPrefix] = useState(businessProfile.invoiceSettings.prefix);
  const [terms, setTerms] = useState(businessProfile.invoiceSettings.termsAndConditions);
  const [footer, setFooter] = useState(businessProfile.invoiceSettings.footerMessage);

  const handleSaveSettings = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.business.updateInfo(businessProfile.id, {
        name: storeName,
        address,
        phone,
        gSTNumber: gstin,
      });
      await api.business.updateInvoiceSettings({
        invoicePrefix: prefix,
        termsAndConditions: terms,
        footerNote: footer,
      });
      setBusinessProfile({
        name: storeName,
        storeName,
        address,
        phone,
        gstin,
        invoiceSettings: {
          ...businessProfile.invoiceSettings,
          prefix,
          termsAndConditions: terms,
          footerMessage: footer,
        },
      });
      showToast('Store settings saved', 'success');
    } catch (error: any) {
      showToast(error.message || 'Could not save settings', 'error');
    } finally {
      setSaving(false);
    }
  };
  return (
    <div className="space-y-6 w-full">
      <div className="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs">
        <div>
          <h1 className="text-xl font-bold text-slate-900 tracking-tight">Business & System Settings</h1>
          <p className="text-xs text-slate-500">Configure store details and invoice receipts</p>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex items-center gap-2 border-b border-slate-200 pb-1 overflow-x-auto">
        {[
          { id: 'profile', label: 'Business Profile', icon: Store },
          { id: 'invoice', label: 'Invoice Rules', icon: Receipt },
          { id: 'payment', label: 'Appearance & System', icon: Palette },
        ].map((tab) => {
          const Icon = tab.icon;
          const active = activeTab === tab.id;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as any)}
              className={`flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold transition-all whitespace-nowrap ${
                active ? 'bg-indigo-600 text-white shadow-sm' : 'text-slate-600 hover:bg-slate-100'
              }`}
            >
              <Icon className="w-4 h-4" />
              <span>{tab.label}</span>
            </button>
          );
        })}
      </div>

      <form onSubmit={handleSaveSettings} className="space-y-6">
        {activeTab === 'profile' && (
          <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
            <h3 className="text-sm font-bold text-slate-900 border-b border-slate-100 pb-2">Store Profile Details</h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Store / Branch Name</label>
                <input
                  type="text"
                  value={storeName}
                  onChange={(e) => setStoreName(e.target.value)}
                  className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-semibold"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">GSTIN Registration</label>
                <input
                  type="text"
                  value={gstin}
                  onChange={(e) => setGstin(e.target.value)}
                  className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono font-semibold"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Store Contact Phone</label>
                <input
                  type="text"
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Store Address</label>
                <input
                  type="text"
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs"
                />
              </div>
            </div>
          </div>
        )}

        {activeTab === 'invoice' && (
          <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
            <h3 className="text-sm font-bold text-slate-900 border-b border-slate-100 pb-2">Invoice & Receipt Template</h3>
            <div className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Invoice Prefix</label>
                <input
                  type="text"
                  value={prefix}
                  onChange={(e) => setPrefix(e.target.value.toUpperCase())}
                  className="w-32 px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs font-mono font-bold uppercase"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Receipt Terms & Conditions</label>
                <textarea
                  rows={3}
                  value={terms}
                  onChange={(e) => setTerms(e.target.value)}
                  className="w-full p-3 bg-slate-50 border border-slate-200 rounded-lg text-xs"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Footer Message</label>
                <input
                  type="text"
                  value={footer}
                  onChange={(e) => setFooter(e.target.value)}
                  className="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-xs"
                />
              </div>
            </div>
          </div>
        )}

        {activeTab === 'payment' && (
          <div className="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-2xs space-y-4">
            <h3 className="text-sm font-bold text-slate-900 border-b border-slate-100 pb-2">Appearance Mode</h3>
            <div className="flex items-center justify-between p-3 bg-slate-50 rounded-xl border border-slate-200">
              <div>
                <p className="text-xs font-bold text-slate-900">Current Theme</p>
                <p className="text-[11px] text-slate-500">Switch between Light theme and Dark mode architecture</p>
              </div>
              <button
                type="button"
                onClick={toggleTheme}
                className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-xs font-bold capitalize"
              >
                {theme} Mode
              </button>
            </div>
          </div>
        )}

        <div className="flex justify-end">
          <button
            type="submit"
            disabled={saving}
            className="px-6 py-2.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white rounded-xl text-xs font-bold shadow-md shadow-indigo-600/30 flex items-center gap-2 transition-all"
          >
            {saving ? (
              <>
                <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                </svg>
                <span>Saving...</span>
              </>
            ) : (
              <>
                <Save className="w-4 h-4" />
                <span>Save Settings Changes</span>
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
};
