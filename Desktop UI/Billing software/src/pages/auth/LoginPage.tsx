import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Lock, Mail, ArrowRight, ShieldCheck, AlertCircle, Loader2 } from 'lucide-react';
import { useStore } from '../../store/useStore';
import { api } from '../../services/apiClient';

export const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
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
      // The API returns AuthResponse: { token, refreshToken, user }
      if (!response.token) throw new Error('Login did not return an access token.');
      api.setToken(response.token);

      // Update currentUser in store from API response if available
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
    <div className="bg-white rounded-2xl shadow-2xl border border-slate-100 p-8 space-y-6">
      <div className="text-center space-y-2">
        <div className="w-12 h-12 rounded-2xl bg-indigo-600 text-white flex items-center justify-center font-bold text-2xl mx-auto shadow-lg shadow-indigo-600/30">
          B
        </div>
        <h2 className="text-2xl font-bold text-slate-900 tracking-tight">Welcome to BillFlow</h2>
        <p className="text-xs text-slate-500">Sign in to manage your business</p>
      </div>

      {error && (
        <div className="flex items-start gap-2.5 p-3 bg-red-50 border border-red-200 rounded-xl text-xs text-red-700 font-medium">
          <AlertCircle className="w-4 h-4 shrink-0 mt-0.5" />
          <span>{error}</span>
        </div>
      )}

      <form onSubmit={handleLogin} className="space-y-4">
        <div>
          <label className="block text-xs font-semibold text-slate-700 mb-1">Work Email</label>
          <div className="relative">
            <Mail className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full pl-9 pr-3 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-600/30 focus:border-indigo-600"
            />
          </div>
        </div>

        <div>
          <div className="flex items-center justify-between mb-1">
            <label className="block text-xs font-semibold text-slate-700">Password</label>
            <a href="#" className="text-xs font-medium text-indigo-600 hover:underline">
              Forgot password?
            </a>
          </div>
          <div className="relative">
            <Lock className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full pl-9 pr-3 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-indigo-600/30 focus:border-indigo-600"
            />
          </div>
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full py-2.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white rounded-lg font-semibold text-sm shadow-md shadow-indigo-600/30 transition-all flex items-center justify-center gap-2"
        >
          {loading ? (
            <>
              <Loader2 className="w-4 h-4 animate-spin" />
              <span>Signing in...</span>
            </>
          ) : (
            <>
          <span>Sign In</span>
              <ArrowRight className="w-4 h-4" />
            </>
          )}
        </button>
      </form>

      <div className="pt-4 border-t border-slate-100 flex items-center justify-between text-xs text-slate-500">
        <span>Need a store account?</span>
        <Link to="/register" className="font-semibold text-indigo-600 hover:underline">
          Register Business
        </Link>
      </div>

      <div className="p-3 bg-slate-50 rounded-xl border border-slate-200/60 flex items-center gap-2.5 text-xs text-slate-600">
        <ShieldCheck className="w-4 h-4 text-emerald-600 shrink-0" />
        <span>
          {backendStatus === 'connected'
            ? 'API connected'
            : backendStatus === 'offline'
            ? 'API unavailable. Start the server on localhost:5224.'
            : 'Checking API connection'}
        </span>
      </div>
    </div>
  );
};
