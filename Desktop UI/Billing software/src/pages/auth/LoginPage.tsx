import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  Lock, Mail, ArrowRight, ShieldCheck, AlertCircle, Loader2,
  Eye, EyeOff, LayoutGrid, Shield, Cpu, ServerCrash
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { api } from '../../services/apiClient';

/* ── Tiny helpers ── */
const Badge: React.FC<{ children: React.ReactNode; className?: string }> = ({ children, className = '' }) => (
  <span className={`inline-flex items-center gap-1.5 text-xs font-semibold tracking-wide ${className}`}>{children}</span>
);

export const LoginPage: React.FC = () => {
  const [email, setEmail]       = useState('');
  const [password, setPassword] = useState('');
  const [showPwd, setShowPwd]   = useState(false);
  const [rememberMe, setRememberMe] = useState(true);
  const [loading, setLoading]   = useState(false);
  const [error, setError]       = useState('');
  const navigate = useNavigate();
  const { backendStatus, setBackendStatus, syncWithBackend } = useStore();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const response = await api.auth.login({ email, password });
      if (!response.token) throw new Error('Login did not return an access token.');
      api.setToken(response.token);

      const apiUser = response.user;
      if (apiUser) {
        useStore.setState((state) => ({
          currentUser: {
            ...state.currentUser,
            id:    String(apiUser.id    || state.currentUser.id),
            name:  apiUser.fullName || apiUser.name  || state.currentUser.name,
            email: apiUser.email   || state.currentUser.email,
            role:  (apiUser.role   || state.currentUser.role) as any,
            phone: apiUser.phone   || state.currentUser.phone,
          },
        }));
      }

      setBackendStatus('connected');
      if (!api.hasRole('SuperAdmin')) await syncWithBackend();
      setLoading(false);
      navigate(api.hasRole('SuperAdmin') ? '/platform-admin' : '/dashboard');
    } catch (err: any) {
      api.setToken(null);
      setError(err.message || 'Login failed. Please check your credentials.');
      setLoading(false);
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
            <span className="w-2 h-2 bg-emerald-500 rounded-full animate-pulse" />
            Continuous Settlement Infrastructure • 99.999% SLA
          </span>
        </div>

        {/* Hero */}
        <div className="relative z-10 flex-1 flex flex-col justify-center space-y-8 py-10">
          <div>
            <h1 className="text-5xl font-black text-slate-900 leading-[1.1] tracking-tight">
              Unified Billing &amp;<br />Revenue Architecture
            </h1>
            <p className="mt-4 text-slate-500 text-base leading-relaxed max-w-sm">
              Institutional-grade revenue recognition, automated subscription reconciliation, and programmatic compliance protocols.
            </p>
          </div>

          {/* Feature pills grid */}
          <div className="grid grid-cols-2 gap-3">
            {[
              { icon: <LayoutGrid className="w-4 h-4 text-indigo-500" />, label: 'Multi-Entity Ledgers' },
              { icon: <Cpu className="w-4 h-4 text-indigo-500" />,        label: 'FIPS 140-3 Hardware Keys' },
              { icon: <ShieldCheck className="w-4 h-4 text-indigo-500" />, label: 'Deterministic Close' },
              { icon: <Shield className="w-4 h-4 text-indigo-500" />,     label: 'Zero-Trust Federation' },
            ].map(({ icon, label }) => (
              <div key={label} className="flex items-center gap-2.5 bg-white/70 border border-slate-200/80 rounded-xl px-4 py-3 backdrop-blur shadow-sm">
                {icon}
                <span className="text-sm font-semibold text-slate-700">{label}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Footer badges */}
        <div className="relative z-10 flex items-center gap-6 text-xs text-slate-500">
          <span className="flex items-center gap-1.5"><ShieldCheck className="w-3.5 h-3.5 text-emerald-500" /> SOC2 Type II</span>
          <span className="text-slate-300">•</span>
          <span className="flex items-center gap-1.5"><Lock className="w-3.5 h-3.5 text-amber-500" /> ISO 27001</span>
          <span className="text-slate-300">•</span>
          <span className="flex items-center gap-1.5"><ServerCrash className="w-3.5 h-3.5 text-blue-500" /> PCI-DSS Level 1</span>
        </div>
      </div>

      {/* ── RIGHT PANEL — Form ── */}
      <div className="flex-1 flex items-center justify-center p-6 sm:p-10 bg-[#F0F2F5]">
        <div className="w-full max-w-md">

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
              <Badge className="bg-emerald-50 text-emerald-700 border border-emerald-200 rounded-full px-3 py-1">
                <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full" />
                ZERO-TRUST ENFORCED
              </Badge>
              <Badge className="text-slate-500">
                <Shield className="w-3.5 h-3.5" />
                FIPS 140-3
              </Badge>
            </div>

            {/* Title */}
            <div>
              <h2 className="text-2xl font-black text-slate-900">Welcome back</h2>
              <p className="text-slate-500 text-sm mt-1 leading-relaxed">
                Sign in to manage automated billing pipelines, recurring subscriptions, and institutional revenue reconciliation.
              </p>
            </div>

            {/* SSO Buttons */}
            <div className="grid grid-cols-2 gap-3">
              <button type="button" className="flex items-center justify-center gap-2 py-2.5 border border-slate-200 rounded-xl text-sm font-semibold text-slate-700 hover:bg-slate-50 transition">
                {/* Google G */}
                <svg className="w-4 h-4" viewBox="0 0 24 24"><path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/><path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/><path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z"/><path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/></svg>
                Google Workspace
              </button>
              <button type="button" className="flex items-center justify-center gap-2 py-2.5 border border-slate-200 rounded-xl text-sm font-semibold text-slate-700 hover:bg-slate-50 transition">
                <Shield className="w-4 h-4 text-slate-500" />
                Okta / SAML SSO
              </button>
            </div>

            {/* Divider */}
            <div className="relative flex items-center gap-3">
              <div className="flex-1 h-px bg-slate-100" />
              <span className="text-xs text-slate-400 font-semibold tracking-widest uppercase">or sign in with email</span>
              <div className="flex-1 h-px bg-slate-100" />
            </div>

            {/* Error */}
            {error && (
              <div className="flex items-start gap-2.5 p-3.5 bg-red-50 border border-red-200 rounded-xl text-sm text-red-700 font-medium">
                <AlertCircle className="w-4 h-4 shrink-0 mt-0.5" />
                <span>{error}</span>
              </div>
            )}

            <form onSubmit={handleLogin} className="space-y-4">
              {/* Email */}
              <div>
                <div className="flex items-center justify-between mb-1.5">
                  <label className="text-sm font-semibold text-slate-700">Institutional Email</label>
                  <span className="text-[10px] font-bold text-slate-400 tracking-widest uppercase">SSO Auto-detect</span>
                </div>
                <div className="relative">
                  <Mail className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                  <input
                    type="email"
                    required
                    placeholder="name@company.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="w-full pl-10 pr-4 py-3 bg-white border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition"
                  />
                </div>
              </div>

              {/* Password */}
              <div>
                <div className="flex items-center justify-between mb-1.5">
                  <label className="text-sm font-semibold text-slate-700">Password</label>
                  <a href="#" className="text-xs font-semibold text-indigo-600 hover:underline">Forgot password?</a>
                </div>
                <div className="relative">
                  <Lock className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                  <input
                    type={showPwd ? 'text' : 'password'}
                    required
                    placeholder="••••••••••••••"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="w-full pl-10 pr-11 py-3 bg-white border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPwd(!showPwd)}
                    className="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                  >
                    {showPwd ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                  </button>
                </div>
              </div>

              {/* Remember me + TLS badge */}
              <div className="flex items-center justify-between">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={rememberMe}
                    onChange={(e) => setRememberMe(e.target.checked)}
                    className="w-4 h-4 rounded accent-indigo-600"
                  />
                  <span className="text-sm text-slate-600">Keep me signed in for 30 days</span>
                </label>
                <Badge className="bg-emerald-50 text-emerald-700 border border-emerald-200 rounded-full px-2.5 py-1">
                  TLS 1.3 Active
                </Badge>
              </div>

              {/* Submit */}
              <button
                type="submit"
                disabled={loading}
                className="w-full py-3.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white rounded-xl font-bold text-sm shadow-lg shadow-indigo-600/25 transition-all flex items-center justify-center gap-2"
              >
                {loading ? (
                  <><Loader2 className="w-4 h-4 animate-spin" /><span>Signing in...</span></>
                ) : (
                  <><span>Sign In to Dashboard</span><ArrowRight className="w-4 h-4" /></>
                )}
              </button>
            </form>

            {/* Footer row */}
            <div className="flex items-center justify-between pt-1">
              <span className="flex items-center gap-1.5 text-[10px] font-bold text-slate-400 tracking-widest uppercase">
                <Shield className="w-3 h-3" />
                256-BIT AES &amp; WEBAUTHN
              </span>
              <span className="text-xs text-slate-500">
                New organization?{' '}
                <Link to="/register" className="font-bold text-indigo-600 hover:underline">Register</Link>
              </span>
            </div>
          </div>

          {/* API status */}
          <div className="mt-4 flex items-center gap-2 px-1 text-xs text-slate-400">
            <ShieldCheck className={`w-3.5 h-3.5 shrink-0 ${backendStatus === 'connected' ? 'text-emerald-500' : backendStatus === 'offline' ? 'text-red-400' : 'text-amber-400'}`} />
            <span>
              {backendStatus === 'connected'
                ? 'API connected — secure session'
                : backendStatus === 'offline'
                ? 'API unavailable. Start the server on localhost:5224.'
                : 'Checking API connection…'}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
};
