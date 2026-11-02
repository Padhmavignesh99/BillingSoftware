import React, { useState, useEffect, useCallback } from 'react';
import { Star, Plus, Settings, Trophy, Trash2, ToggleLeft, ToggleRight, TrendingUp } from 'lucide-react';
import { api } from '../../services/apiClient';

interface LoyaltyProgram { id: number; name: string; isEnabled: boolean; pointsPerRupee: number; rupeePerPoint: number; minRedemptionPoints: number; maxRedemptionPercent: number; pointsExpiryDays: number; termsAndConditions?: string; }
interface LoyaltyTier { id: number; name: string; minPoints: number; maxPoints: number; bonusMultiplier: number; badgeColor?: string; benefits?: string; sortOrder: number; }
interface TopCustomer { id: number; customerId: number; customerName?: string; customerPhone?: string; tierName?: string; tierBadgeColor?: string; currentPoints: number; lifetimeSpend: number; }

const TIER_COLORS = ['#CD7F32', '#C0C0C0', '#FFD700', '#E5E4E2'];

export const LoyaltySettingsPage: React.FC = () => {
  const [tab, setTab] = useState<'program' | 'tiers' | 'leaderboard'>('program');
  const [program, setProgram] = useState<LoyaltyProgram | null>(null);
  const [tiers, setTiers] = useState<LoyaltyTier[]>([]);
  const [topCustomers, setTopCustomers] = useState<TopCustomer[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showTierModal, setShowTierModal] = useState(false);
  const [tierForm, setTierForm] = useState({ name: '', minPoints: 0, maxPoints: 9999, bonusMultiplier: 1, badgeColor: '#CD7F32', benefits: '' });

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [prog, tierList] = await Promise.all([
        api.request<LoyaltyProgram>('/api/loyalty/program'),
        api.request<LoyaltyTier[]>('/api/loyalty/tiers'),
      ]);
      setProgram(prog);
      setTiers(Array.isArray(tierList) ? tierList : []);
    } catch { }
    setLoading(false);
  }, []);

  const loadLeaderboard = useCallback(async () => {
    try {
      const res = await api.request<TopCustomer[]>('/api/loyalty/top-customers?take=20');
      setTopCustomers(Array.isArray(res) ? res : []);
    } catch { }
  }, []);

  useEffect(() => { load(); }, [load]);
  useEffect(() => { if (tab === 'leaderboard') loadLeaderboard(); }, [tab, loadLeaderboard]);

  const saveProgram = async () => {
    if (!program) return;
    setSaving(true);
    try {
      const updated = await api.request<LoyaltyProgram>('/api/loyalty/program', { method: 'PUT', body: JSON.stringify(program) });
      setProgram(updated);
      alert('Settings saved!');
    } catch { }
    setSaving(false);
  };

  const createTier = async () => {
    try {
      await api.request('/api/loyalty/tiers', { method: 'POST', body: JSON.stringify({ ...tierForm, sortOrder: tiers.length }) });
      setShowTierModal(false);
      load();
    } catch { }
  };

  const deleteTier = async (id: number) => {
    if (!confirm('Delete this tier?')) return;
    await api.request(`/api/loyalty/tiers/${id}`, { method: 'DELETE' });
    load();
  };

  if (loading) return <div className="p-8 text-center text-sm text-slate-400">Loading...</div>;

  return (
    <div className="p-4 md:p-6 space-y-4 max-w-4xl mx-auto">
      {/* Header */}
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 bg-amber-100 rounded-xl flex items-center justify-center"><Star className="w-5 h-5 text-amber-500" /></div>
        <div><h1 className="text-lg font-bold text-slate-800">Loyalty Program</h1><p className="text-xs text-slate-500">Reward your loyal customers</p></div>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 p-1 bg-slate-100 rounded-xl w-fit">
        {(['program', 'tiers', 'leaderboard'] as const).map(t => (
          <button key={t} onClick={() => setTab(t)} className={`px-4 py-1.5 text-xs font-semibold rounded-lg capitalize transition-all ${tab === t ? 'bg-white text-emerald-700 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}>{t === 'program' ? 'Settings' : t === 'tiers' ? 'Tiers' : 'Leaderboard'}</button>
        ))}
      </div>

      {/* Program Settings */}
      {tab === 'program' && program && (
        <div className="bg-white border border-green-100 rounded-xl p-5 space-y-4">
          <div className="flex items-center justify-between">
            <p className="text-sm font-semibold text-slate-800">Program Status</p>
            <button onClick={() => setProgram(p => p ? { ...p, isEnabled: !p.isEnabled } : p)} className="flex items-center gap-2">
              {program.isEnabled ? <ToggleRight className="w-8 h-8 text-emerald-600" /> : <ToggleLeft className="w-8 h-8 text-slate-300" />}
              <span className={`text-xs font-semibold ${program.isEnabled ? 'text-emerald-600' : 'text-slate-400'}`}>{program.isEnabled ? 'Enabled' : 'Disabled'}</span>
            </button>
          </div>
          <div className="grid grid-cols-2 gap-4">
            {[
              { label: 'Points Per ₹1 Spent', key: 'pointsPerRupee', hint: 'e.g. 1 = 1 point per rupee' },
              { label: '₹ Value Per Point', key: 'rupeePerPoint', hint: 'e.g. 0.10 = 10 paise per point' },
              { label: 'Min Points to Redeem', key: 'minRedemptionPoints', hint: 'Minimum balance required' },
              { label: 'Max Redemption %', key: 'maxRedemptionPercent', hint: '% of invoice redeemable' },
              { label: 'Points Expiry (days)', key: 'pointsExpiryDays', hint: '365 = expire after 1 year' },
            ].map(field => (
              <div key={field.key}>
                <label className="text-xs font-medium text-slate-600 mb-1 block">{field.label}</label>
                <input
                  type="number" step="0.01"
                  value={(program as any)[field.key]}
                  onChange={e => setProgram(p => p ? { ...p, [field.key]: Number(e.target.value) } : p)}
                  className="w-full text-xs border border-slate-200 rounded-lg px-3 py-2 focus:ring-1 focus:ring-emerald-400 outline-none"
                />
                <p className="text-[10px] text-slate-400 mt-0.5">{field.hint}</p>
              </div>
            ))}
          </div>
          <div>
            <label className="text-xs font-medium text-slate-600 mb-1 block">Terms & Conditions</label>
            <textarea value={program.termsAndConditions || ''} onChange={e => setProgram(p => p ? { ...p, termsAndConditions: e.target.value } : p)} rows={3} className="w-full text-xs border border-slate-200 rounded-lg px-3 py-2 focus:ring-1 focus:ring-emerald-400 outline-none resize-none" />
          </div>
          <button onClick={saveProgram} disabled={saving} className="w-full py-2 text-xs font-semibold bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 disabled:opacity-50">{saving ? 'Saving...' : 'Save Settings'}</button>
        </div>
      )}

      {/* Tiers */}
      {tab === 'tiers' && (
        <div className="space-y-3">
          <div className="flex justify-end">
            <button onClick={() => setShowTierModal(true)} className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 text-white text-xs font-semibold rounded-lg hover:bg-emerald-700">
              <Plus className="w-3.5 h-3.5" /> Add Tier
            </button>
          </div>
          {tiers.length === 0 ? (
            <div className="bg-white border border-green-100 rounded-xl p-8 text-center text-sm text-slate-400">No tiers configured yet</div>
          ) : tiers.map((tier, idx) => (
            <div key={tier.id} className="bg-white border border-green-100 rounded-xl p-4 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 rounded-xl flex items-center justify-center text-white text-xs font-bold" style={{ backgroundColor: tier.badgeColor || TIER_COLORS[idx] || '#888' }}>
                  {tier.name.slice(0, 2).toUpperCase()}
                </div>
                <div>
                  <p className="text-sm font-semibold text-slate-800">{tier.name}</p>
                  <p className="text-xs text-slate-500">{tier.minPoints.toLocaleString()} – {tier.maxPoints.toLocaleString()} pts · {tier.bonusMultiplier}x multiplier</p>
                  {tier.benefits && <p className="text-xs text-emerald-600 mt-0.5">{tier.benefits}</p>}
                </div>
              </div>
              <button onClick={() => deleteTier(tier.id)} className="p-1.5 text-slate-400 hover:text-rose-500 hover:bg-rose-50 rounded-lg transition-colors"><Trash2 className="w-3.5 h-3.5" /></button>
            </div>
          ))}
        </div>
      )}

      {/* Leaderboard */}
      {tab === 'leaderboard' && (
        <div className="bg-white border border-green-100 rounded-xl overflow-hidden">
          <div className="px-4 py-3 border-b border-green-100 flex items-center gap-2">
            <Trophy className="w-4 h-4 text-amber-500" />
            <p className="text-sm font-semibold text-slate-800">Top Loyalty Customers</p>
          </div>
          {topCustomers.length === 0 ? (
            <div className="p-8 text-center text-sm text-slate-400">No loyalty accounts yet</div>
          ) : (
            <table className="w-full text-xs">
              <thead><tr className="bg-slate-50 border-b border-slate-100">
                <th className="text-left px-3 py-2.5 font-semibold text-slate-600">#</th>
                <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Customer</th>
                <th className="text-left px-3 py-2.5 font-semibold text-slate-600">Tier</th>
                <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Points</th>
                <th className="text-right px-3 py-2.5 font-semibold text-slate-600">Lifetime Spend</th>
              </tr></thead>
              <tbody>
                {topCustomers.map((c, i) => (
                  <tr key={c.id} className="border-b border-slate-50 hover:bg-slate-50">
                    <td className="px-3 py-2.5 font-bold text-slate-400">#{i + 1}</td>
                    <td className="px-3 py-2.5">
                      <p className="font-semibold text-slate-800">{c.customerName}</p>
                      <p className="text-slate-400">{c.customerPhone}</p>
                    </td>
                    <td className="px-3 py-2.5">
                      {c.tierName && <span className="px-2 py-0.5 text-[10px] font-bold rounded-md text-white" style={{ backgroundColor: c.tierBadgeColor || '#888' }}>{c.tierName}</span>}
                    </td>
                    <td className="px-3 py-2.5 text-right font-bold text-amber-600">{c.currentPoints.toLocaleString()}</td>
                    <td className="px-3 py-2.5 text-right font-semibold text-slate-700">₹{c.lifetimeSpend.toLocaleString('en-IN')}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* Tier Modal */}
      {showTierModal && (
        <div className="fixed inset-0 z-50 bg-black/30 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-sm">
            <div className="p-5 border-b border-green-100"><h2 className="text-base font-bold text-slate-800">Add Loyalty Tier</h2></div>
            <div className="p-5 space-y-3">
              {[{ label: 'Tier Name', key: 'name', type: 'text', placeholder: 'e.g. Gold' },
                { label: 'Min Points', key: 'minPoints', type: 'number', placeholder: '0' },
                { label: 'Max Points', key: 'maxPoints', type: 'number', placeholder: '9999' },
                { label: 'Bonus Multiplier', key: 'bonusMultiplier', type: 'number', placeholder: '1.5' },
                { label: 'Benefits', key: 'benefits', type: 'text', placeholder: 'Free delivery, priority support...' },
              ].map(f => (
                <div key={f.key}>
                  <label className="text-xs font-medium text-slate-600 mb-1 block">{f.label}</label>
                  <input type={f.type} value={(tierForm as any)[f.key]} onChange={e => setTierForm(tf => ({ ...tf, [f.key]: f.type === 'number' ? Number(e.target.value) : e.target.value }))} placeholder={f.placeholder} className="w-full text-xs border border-slate-200 rounded-lg px-3 py-2 focus:ring-1 focus:ring-emerald-400 outline-none" />
                </div>
              ))}
              <div>
                <label className="text-xs font-medium text-slate-600 mb-1 block">Badge Color</label>
                <input type="color" value={tierForm.badgeColor} onChange={e => setTierForm(tf => ({ ...tf, badgeColor: e.target.value }))} className="w-full h-9 border border-slate-200 rounded-lg cursor-pointer" />
              </div>
            </div>
            <div className="p-4 border-t border-green-100 flex gap-2 justify-end">
              <button onClick={() => setShowTierModal(false)} className="px-4 py-2 text-xs font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50">Cancel</button>
              <button onClick={createTier} disabled={!tierForm.name} className="px-4 py-2 text-xs font-semibold bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 disabled:opacity-50">Add Tier</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

