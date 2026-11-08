import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Store, User, Mail, Building, ArrowRight, Loader2, AlertCircle, Zap, CheckCircle2 } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { useToast } from '../../components/common/Toast';
import { api } from '../../services/apiClient';

export const RegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const { syncWithBackend } = useStore();
  const { showToast } = useToast();

  const [companyName, setCompanyName] = useState('');
  const [ownerName, setOwnerName] = useState('');
  const [ownerEmail, setOwnerEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!companyName.trim() || !ownerName.trim() || !ownerEmail.trim()) {
      showToast('Please fill in all required fields.', 'warning');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const response = await api.auth.register({
        fullName: ownerName,
        email: ownerEmail,
        password,
        businessName: companyName,
      });
      if (!response.token) throw new Error('Registration did not return an access token.');
      api.setToken(response.token);
      await syncWithBackend();

      setLoading(false);
      showToast(`Welcome! ${companyName} is ready.`, 'success');
      navigate('/dashboard');
    } catch (err: any) {
      api.setToken(null);
      setLoading(false);
      setError(err.message || 'Registration failed');
    }
  };

  return (
    <div className="min-h-screen flex font-sans">
      {/* ── Left Panel — Branding ── */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-indigo-700 via-indigo-600 to-purple-700 relative flex-col justify-between p-12 text-white overflow-hidden">
        {/* Blobs */}
        <div className="absolute -top-20 -right-20 w-80 h-80 bg-white/5 rounded-full blur-3xl" />
        <div className="absolute -bottom-20 -left-20 w-80 h-80 bg-purple-900/30 rounded-full blur-3xl" />
        {/* Logo */}
        <Link to="/" className="flex items-center gap-3 relative z-10">
          <div className="w-10 h-10 bg-white/20 rounded-2xl flex items-center justify-center font-black text-xl backdrop-blur">M</div>
          <span className="font-black text-xl tracking-tight">Master Billing</span>
        </Link>
        {/* Hero text */}
        <div className="relative z-10 space-y-6">
          <div className="inline-flex items-center gap-2 bg-white/15 border border-white/20 rounded-full px-4 py-1.5 text-sm font-medium backdrop-blur">
            <Zap className="w-3.5 h-3.5 text-amber-300" />
            Trusted by 2 Lakh+ businesses
          </div>
          <h1 className="text-4xl font-black leading-tight">
            Grow Your Business<br />
            <span className="text-amber-300">With Our SaaS</span>
          </h1>
          <p className="text-indigo-100 leading-relaxed max-w-sm">
            Simple, powerful tools to manage invoices, inventory, and payments – all in one place.
          </p>
          <ul className="space-y-3">
            {[
              'Fast onboarding',
              'All‑in‑one dashboard',
              'Secure cloud storage',
              '24/7 support',
            ].map((f) => (
              <li key={f} className="flex items-center gap-2.5 text-sm text-indigo-100">
                <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0" />
                {f}
              </li>
            ))}
          </ul>
        </div>
      </div>

      {/* ── Right Panel — Form ── */}
      <div className="flex-1 flex items-center justify-center p-6 sm:p-10 bg-slate-50">
        <div className="w-full max-w-xl space-y-7">
          {/* Mobile logo */}
          <Link to="/" className="lg:hidden flex items-center gap-2.5 mb-2">
            <div className="w-9 h-9 bg-indigo-600 rounded-xl flex items-center justify-center text-white font-black text-lg">M</div>
            <span className="font-black text-slate-900 text-lg">Master Billing</span>
          </Link>

          <div className="bg-white dark:bg-slate-900 rounded-3xl shadow-2xl border border-slate-100 dark:border-slate-800 p-8 space-y-6">
            <div className="text-center space-y-2">
              <div className="w-12 h-12 rounded-2xl bg-indigo-600 text-white flex items-center justify-center font-black text-2xl mx-auto shadow-lg shadow-indigo-600/30">
                B
              </div>
              <h2 className="text-2xl font-black text-slate-900 dark:text-white tracking-tight">
                Create Business Account
              </h2>
              <p className="text-xs text-slate-500">
                Create an account for your store and start billing
              </p>
            </div>

            <form onSubmit={handleRegister} className="space-y-4">
              {error && (
                <div className="flex items-start gap-2.5 p-3 bg-red-50 border border-red-200 rounded-xl text-xs text-red-700 font-medium">
                  <AlertCircle className="w-4 h-4 shrink-0 mt-0.5" />
                  <span>{error}</span>
                </div>
              )}

              <div>
                <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Company / Business Name *
                </label>
                <div className="relative">
                  <Store className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                  <input
                    type="text"
                    required
                    placeholder="e.g. GreenLeaf Organics"
                    value={companyName}
                    onChange={(e) => setCompanyName(e.target.value)}
                    className="w-full pl-9 pr-3 py-2 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-xs text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-600/30"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                    Owner Name *
                  </label>
                  <div className="relative">
                    <User className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                    <input
                      type="text"
                      required
                      value={ownerName}
                      onChange={(e) => setOwnerName(e.target.value)}
                      className="w-full pl-9 pr-3 py-2 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-xs text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-600/30"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                    Work Email *
                  </label>
                  <div className="relative">
                    <Mail className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                    <input
                      type="email"
                      required
                      value={ownerEmail}
                      onChange={(e) => setOwnerEmail(e.target.value)}
                      className="w-full pl-9 pr-3 py-2 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-xs text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-600/30"
                    />
                  </div>
                </div>
              </div>

              {/* Password */}
              <div>
                <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                  Password *
                </label>
                <div className="relative">
                  <Building className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                  <input
                    type="password"
                    required
                    minLength={8}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="Min 8 characters"
                    className="w-full pl-9 pr-3 py-2 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-xs text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-600/30"
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full py-3 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white rounded-xl font-bold text-xs shadow-lg shadow-indigo-600/30 transition-all flex items-center justify-center gap-2 mt-4"
              >
                {loading ? (
                  <>
                    <Loader2 className="w-4 h-4 animate-spin" />
                    <span>Registering...</span>
                  </>
                ) : (
                  <>
                    <span>Create Business Account</span>
                    <ArrowRight className="w-4 h-4" />
                  </>
                )}
              </button>
            </form>

            <div className="text-center text-xs text-slate-500 pt-2">
              Already have an organization?{' '}
              <Link to="/login" className="text-indigo-600 font-bold hover:underline">
                Sign In
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
