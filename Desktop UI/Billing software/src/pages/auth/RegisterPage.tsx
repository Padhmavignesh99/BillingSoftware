import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  Lock, Mail, User, Building2, Phone, ArrowRight, Loader2,
  AlertCircle, Shield, ShieldCheck, LayoutGrid, ChevronDown, ServerCrash
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { useToast } from '../../components/common/Toast';
import { api } from '../../services/apiClient';

const BUSINESS_TYPES = [
  'SaaS & Software',
  'E-commerce & Retail',
  'Financial Services',
  'Healthcare & Pharma',
  'Manufacturing & Supply Chain',
  'Professional Services',
  'Hospitality & Food',
  'Education & Non-Profit',
  'Other',
];

/* entropy helper */
function passwordEntropy(pwd: string): { score: number; label: string; color: string } {
  if (!pwd) return { score: 0, label: 'Minimal', color: '#E2E8F0' };
  let s = 0;
  if (pwd.length >= 8)  s++;
  if (/[A-Z]/.test(pwd)) s++;
  if (/[0-9]/.test(pwd)) s++;
  if (/[^A-Za-z0-9]/.test(pwd)) s++;
  const map: Array<{ label: string; color: string }> = [
    { label: 'Weak',   color: '#EF4444' },
    { label: 'Fair',   color: '#F59E0B' },
    { label: 'Good',   color: '#3B82F6' },
    { label: 'Strong', color: '#10B981' },
    { label: 'Elite',  color: '#6366F1' },
  ];
  return { score: s, ...map[Math.min(s, 4)] };
}

const Badge: React.FC<{ children: React.ReactNode; className?: string }> = ({ children, className = '' }) => (
  <span className={`inline-flex items-center gap-1.5 text-xs font-semibold tracking-wide ${className}`}>{children}</span>
);

export const RegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const { syncWithBackend } = useStore();
  const { showToast } = useToast();

  const [fullName,     setFullName]     = useState('');
  const [workEmail,    setWorkEmail]    = useState('');
  const [phone,        setPhone]        = useState('');
  const [companyName,  setCompanyName]  = useState('');
  const [businessType, setBusinessType] = useState('');
  const [password,     setPassword]     = useState('');
  const [agreed,       setAgreed]       = useState(false);
  const [loading,      setLoading]      = useState(false);
  const [error,        setError]        = useState('');

  const entropy = passwordEntropy(password);

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!fullName.trim() || !workEmail.trim() || !companyName.trim()) {
      showToast('Please fill in all required fields.', 'warning');
      return;
    }
    if (!agreed) {
      showToast('Please accept the Master Service Agreement.', 'warning');
      return;
    }
    setLoading(true);
    setError('');
    try {
      const response = await api.auth.register({
        fullName,
        email:        workEmail,
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
    <div className="min-h-screen flex font-sans bg-[#F0F2F5]">

      {/* ── LEFT PANEL — Branding ── */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-slate-200 via-gray-100 to-slate-300 relative flex-col justify-between p-12 overflow-hidden">
        {/* Decorative blobs */}
        <div className="absolute top-0 right-0 w-96 h-96 bg-indigo-100/60 rounded-full blur-3xl -translate-y-1/2 translate-x-1/3" />
        <div className="absolute bottom-0 left-0 w-80 h-80 bg-purple-100/50 rounded-full blur-3xl translate-y-1/3 -translate-x-1/4" />

        {/* Logo */}
        <div className="relative z-10 flex items-center gap-3">
          <div className="w-11 h-11 bg-indigo-600 rounded-2xl flex items-center justify-center shadow-lg shadow-indigo-600/30">
            <LayoutGrid className="w-6 h-6 text-white" />
          </div>
          <div>
            <div className="font-black text-lg text-slate-900 leading-none">Master Billing</div>
            <div className="text-[10px] font-bold text-indigo-600 tracking-widest uppercase leading-none mt-0.5">Enterprise Rails</div>
          </div>
        </div>

        {/* Status pill */}
        <div className="relative z-10 mt-6 self-start">
          <span className="inline-flex items-center gap-2 bg-white/80 border border-slate-200 rounded-full px-4 py-1.5 text-xs font-semibold text-slate-700 backdrop-blur shadow-sm">
            <span className="w-2 h-2 bg-blue-500 rounded-full" />
            SOC2 Type II Certified • PCI-DSS Level 1 Infrastructure
          </span>
        </div>

        {/* Hero */}
        <div className="relative z-10 flex-1 flex flex-col justify-center space-y-8 py-10">
          <div>
            <h1 className="text-5xl font-black text-slate-900 leading-[1.1] tracking-tight">
              Deploy High-Velocity<br />
              <span className="text-indigo-600">Enterprise Billing Rails</span>
            </h1>
            <p className="mt-4 text-slate-500 text-base leading-relaxed max-w-sm">
              Scale subscription logic, automated multi-entity reconciliation, and deterministic tax routing with zero latency overhead.
            </p>
          </div>

          {/* Features list */}
          <ul className="space-y-5">
            {[
              {
                icon: <Shield className="w-5 h-5 text-indigo-600" />,
                title: 'Multi-Currency Automated Settlement',
                desc:  'Autonomous real-time reconciliation across 140+ jurisdictions with dynamic foreign exchange settlement.',
              },
              {
                icon: <LayoutGrid className="w-5 h-5 text-indigo-600" />,
                title: 'Zero-Code Gateway Orchestration',
                desc:  'Pre-wired orchestration layer for Stripe, Adyen, and Plaid with millisecond failover routing.',
              },
              {
                icon: <Building2 className="w-5 h-5 text-indigo-600" />,
                title: 'ASC 606 Automated Revenue Schedules',
                desc:  'Continuous double-entry ledger tracking with immutable audit exports ready for NetSuite, Workday, and SAP.',
              },
            ].map(({ icon, title, desc }) => (
              <li key={title} className="flex items-start gap-4">
                <div className="w-9 h-9 bg-white/70 border border-slate-200 rounded-xl flex items-center justify-center shrink-0 shadow-sm">
                  {icon}
                </div>
                <div>
                  <div className="text-sm font-bold text-slate-800">{title}</div>
                  <div className="text-xs text-slate-500 mt-0.5 leading-relaxed">{desc}</div>
                </div>
              </li>
            ))}
          </ul>
        </div>

        {/* Footer badges */}
        <div className="relative z-10 flex items-center gap-6 text-xs text-slate-500">
          <span className="flex items-center gap-1.5"><ShieldCheck className="w-3.5 h-3.5 text-blue-500" /> PCI-DSS Level 1</span>
          <span className="text-slate-300">•</span>
          <span className="flex items-center gap-1.5"><Lock className="w-3.5 h-3.5 text-amber-500" /> FIPS 140-3 HSM</span>
          <span className="text-slate-300">•</span>
          <span className="flex items-center gap-1.5"><ServerCrash className="w-3.5 h-3.5 text-emerald-500" /> 99.999% SLA</span>
        </div>
      </div>

      {/* ── RIGHT PANEL — Form ── */}
      <div className="flex-1 flex items-center justify-center p-6 sm:p-10 bg-[#F0F2F5]">
        <div className="w-full max-w-lg">

          {/* Mobile logo */}
          <Link to="/" className="lg:hidden flex items-center gap-2.5 mb-8">
            <div className="w-9 h-9 bg-indigo-600 rounded-xl flex items-center justify-center shadow">
              <LayoutGrid className="w-5 h-5 text-white" />
            </div>
            <span className="font-black text-slate-900 text-lg">Master Billing</span>
          </Link>

          {/* Card */}
          <div className="bg-white rounded-3xl shadow-xl border border-slate-100 p-8 space-y-6">

            {/* Card header badges */}
            <div className="flex items-center justify-between">
              <Badge className="text-indigo-600 font-bold tracking-widest uppercase text-[10px]">
                Enterprise Sandbox
              </Badge>
              <Badge className="text-slate-500">
                <Shield className="w-3.5 h-3.5" />
                256-bit SSL
              </Badge>
            </div>

            {/* Title */}
            <div>
              <h2 className="text-2xl font-black text-slate-900">Create Business Account</h2>
              <p className="text-slate-500 text-sm mt-1">
                Deploy high-frequency billing infrastructure in under 90 seconds.
              </p>
            </div>

            {/* Error */}
            {error && (
              <div className="flex items-start gap-2.5 p-3.5 bg-red-50 border border-red-200 rounded-xl text-sm text-red-700 font-medium">
                <AlertCircle className="w-4 h-4 shrink-0 mt-0.5" />
                <span>{error}</span>
              </div>
            )}

            <form onSubmit={handleRegister} className="space-y-4">
              {/* Row 1: Full Name + Work Email */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-bold text-slate-600 uppercase tracking-widest mb-1.5">
                    Full Name <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <User className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                    <input
                      type="text"
                      required
                      placeholder="Alex Morgan"
                      value={fullName}
                      onChange={(e) => setFullName(e.target.value)}
                      className="w-full pl-10 pr-4 py-3 bg-white border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-xs font-bold text-slate-600 uppercase tracking-widest mb-1.5">
                    Work Email <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <Mail className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                    <input
                      type="email"
                      required
                      placeholder="alex@acme.com"
                      value={workEmail}
                      onChange={(e) => setWorkEmail(e.target.value)}
                      className="w-full pl-10 pr-4 py-3 bg-white border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition"
                    />
                  </div>
                </div>
              </div>

              {/* Row 2: Phone + Legal Entity Name */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                  <div className="flex items-center justify-between mb-1.5">
                    <label className="text-xs font-bold text-slate-600 uppercase tracking-widest">Direct Contact</label>
                    <Badge className="text-indigo-500 text-[10px] font-bold">MFA</Badge>
                  </div>
                  <div className="flex gap-2">
                    <div className="flex items-center gap-1.5 px-3 py-3 border border-slate-200 rounded-xl bg-white text-sm font-semibold text-slate-700 shrink-0">
                      🇮🇳 <span>+91</span>
                    </div>
                    <div className="relative flex-1">
                      <Phone className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                      <input
                        type="tel"
                        placeholder="(555) 019-2834"
                        value={phone}
                        onChange={(e) => setPhone(e.target.value)}
                        className="w-full pl-10 pr-4 py-3 bg-white border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition"
                      />
                    </div>
                  </div>
                </div>
                <div>
                  <label className="block text-xs font-bold text-slate-600 uppercase tracking-widest mb-1.5">
                    Legal Entity Name <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <Building2 className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                    <input
                      type="text"
                      required
                      placeholder="Acme Technologies Inc."
                      value={companyName}
                      onChange={(e) => setCompanyName(e.target.value)}
                      className="w-full pl-10 pr-4 py-3 bg-white border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition"
                    />
                  </div>
                </div>
              </div>

              {/* Operating Model */}
              <div>
                <label className="block text-xs font-bold text-slate-600 uppercase tracking-widest mb-1.5">
                  Operating Model
                </label>
                <div className="relative">
                  <LayoutGrid className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                  <select
                    value={businessType}
                    onChange={(e) => setBusinessType(e.target.value)}
                    className="w-full pl-10 pr-10 py-3 bg-white border border-slate-200 rounded-xl text-sm text-slate-900 appearance-none focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition cursor-pointer"
                  >
                    <option value="" disabled>SaaS &amp; Software</option>
                    {BUSINESS_TYPES.map((t) => (
                      <option key={t} value={t}>{t}</option>
                    ))}
                  </select>
                  <ChevronDown className="w-4 h-4 text-slate-400 absolute right-3.5 top-1/2 -translate-y-1/2 pointer-events-none" />
                </div>
              </div>

              {/* Master Password */}
              <div>
                <div className="flex items-center justify-between mb-1.5">
                  <label className="text-xs font-bold text-slate-600 uppercase tracking-widest">Master Password</label>
                  <span className="text-[10px] font-bold tracking-widest uppercase" style={{ color: entropy.color }}>
                    Entropy: {entropy.label}
                  </span>
                </div>
                <div className="relative">
                  <Lock className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                  <input
                    type="password"
                    required
                    minLength={8}
                    placeholder="••••••••••••"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="w-full pl-10 pr-4 py-3 bg-white border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition"
                  />
                </div>
                {/* Strength bar */}
                <div className="mt-2 flex gap-1">
                  {[0,1,2,3].map((i) => (
                    <div
                      key={i}
                      className="flex-1 h-1 rounded-full transition-all duration-300"
                      style={{ backgroundColor: i < entropy.score ? entropy.color : '#E2E8F0' }}
                    />
                  ))}
                </div>
                <div className="flex gap-4 mt-1.5">
                  {['8+ Chars','Number','Special'].map((hint) => (
                    <span key={hint} className="text-[10px] text-slate-400 flex items-center gap-1">
                      <span className="w-1.5 h-1.5 rounded-full bg-slate-300 inline-block" />
                      {hint}
                    </span>
                  ))}
                </div>
              </div>

              {/* MSA checkbox */}
              <label className="flex items-start gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={agreed}
                  onChange={(e) => setAgreed(e.target.checked)}
                  className="mt-0.5 w-4 h-4 rounded accent-indigo-600 shrink-0"
                />
                <span className="text-sm text-slate-600">
                  I agree to the{' '}
                  <a href="#" className="text-indigo-600 font-semibold hover:underline">Master Service Agreement</a>
                  {' '}and{' '}
                  <a href="#" className="text-indigo-600 font-semibold hover:underline">Privacy Policy</a>.
                </span>
              </label>

              {/* Submit */}
              <button
                type="submit"
                disabled={loading || !agreed}
                className="w-full py-3.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white rounded-xl font-bold text-sm shadow-lg shadow-indigo-600/25 transition-all flex items-center justify-center gap-2"
              >
                {loading ? (
                  <><Loader2 className="w-4 h-4 animate-spin" /><span>Creating account...</span></>
                ) : (
                  <><span>Create Business Account</span><ArrowRight className="w-4 h-4" /></>
                )}
              </button>
            </form>

            {/* Footer */}
            <div className="text-center space-y-2">
              <p className="text-xs text-slate-500">
                Already have an account?{' '}
                <Link to="/login" className="font-bold text-indigo-600 hover:underline">Sign In ›</Link>
              </p>
              <div className="flex items-center justify-center gap-2 text-[10px] text-slate-400">
                <span>✓ No credit card required</span>
                <span>•</span>
                <span>✓ Instant webhook access</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
