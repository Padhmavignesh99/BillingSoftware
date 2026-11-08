import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  Lock, Mail, ArrowRight, ShieldCheck, AlertCircle, Loader2,
  CheckCircle2, Eye, EyeOff, Zap
} from 'lucide-react';
import { useStore } from '../../store/useStore';
import { api } from '../../services/apiClient';

export const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPwd, setShowPwd] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
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
            id: String(apiUser.id || state.currentUser.id),
            name: apiUser.fullName || apiUser.name || state.currentUser.name,
            email: apiUser.email || state.currentUser.email,
            role: (apiUser.role || state.currentUser.role) as any,
            phone: apiUser.phone || state.currentUser.phone,
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
            GST Billing<br />
            <span className="text-amber-300">Made Simple</span>
          </h1>
          <p className="text-indigo-100 leading-relaxed max-w-sm">
            Create invoices in seconds, track inventory, send bills on WhatsApp, and grow your business — all from one platform.
          </p>
          <ul className="space-y-3">
            {[
              'GST-compliant invoicing in 10 seconds',
              'WhatsApp & SMS billing',
              'Real-time inventory tracking',
              'GST reports: GSTR-1, GSTR-3B',
            ].map((f) => (
              <li key={f} className="flex items-center gap-2.5 text-sm text-indigo-100">
                <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0" />
                {f}
              </li>
            ))}
          </ul>
        </div>

        {/* Rating */}
        <div className="relative z-10 flex items-center gap-3 bg-white/10 border border-white/20 rounded-2xl px-5 py-4 backdrop-blur">
          <div className="text-3xl font-black text-amber-300">4.8★</div>
          <div>
            <div className="font-semibold text-sm">App Store Rating</div>
            <div className="text-indigo-200 text-xs">Based on 50,000+ reviews</div>
          </div>
        </div>
      </div>

      {/* ── Right Panel — Form ── */}
      <div className="flex-1 flex items-center justify-center p-6 sm:p-10 bg-slate-50">
        <div className="w-full max-w-md space-y-7">

          {/* Mobile logo */}
          <Link to="/" className="lg:hidden flex items-center gap-2.5 mb-2">
            <div className="w-9 h-9 bg-indigo-600 rounded-xl flex items-center justify-center text-white font-black text-lg">M</div>
            <span className="font-black text-slate-900 text-lg">Master Billing</span>
          </Link>

          <div>
            <h2 className="text-2xl font-black text-slate-900">Welcome back! 👋</h2>
            <p className="text-slate-500 text-sm mt-1">Sign in to manage your business</p>
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
              <label className="block text-sm font-semibold text-slate-700 mb-1.5">Work Email</label>
              <div className="relative">
                <Mail className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                <input
                  type="email"
                  required
                  placeholder="you@business.com"
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
                  placeholder="Enter your password"
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

            {/* Submit */}
            <button
              type="submit"
              disabled={loading}
              className="w-full py-3.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white rounded-xl font-bold text-sm shadow-lg shadow-indigo-600/25 transition-all flex items-center justify-center gap-2 mt-2"
            >
              {loading ? (
                <><Loader2 className="w-4 h-4 animate-spin" /><span>Signing in...</span></>
              ) : (
                <><span>Sign In</span><ArrowRight className="w-4 h-4" /></>
              )}
            </button>
          </form>

          {/* Register link */}
          <div className="text-center text-sm text-slate-500">
            Don't have an account?{' '}
            <Link to="/register" className="font-bold text-indigo-600 hover:underline">
              Register your business →
            </Link>
          </div>

          {/* API Status */}
          <div className="flex items-center gap-2.5 p-3.5 bg-white border border-slate-200 rounded-xl text-xs text-slate-500">
            <ShieldCheck className={`w-4 h-4 shrink-0 ${backendStatus === 'connected' ? 'text-emerald-500' : backendStatus === 'offline' ? 'text-red-400' : 'text-amber-400'}`} />
            <span>
              {backendStatus === 'connected'
                ? 'API connected — secure session'
                : backendStatus === 'offline'
                ? 'API unavailable. Start the server on localhost:5224.'
                : 'Checking API connection…'}
            </span>
          </div>

          <p className="text-center text-xs text-slate-400">
            By signing in, you agree to our{' '}
            <a href="#" className="underline hover:text-slate-600">Terms of Service</a> &{' '}
            <a href="#" className="underline hover:text-slate-600">Privacy Policy</a>
          </p>
        </div>
      </div>
    </div>
  );
};
