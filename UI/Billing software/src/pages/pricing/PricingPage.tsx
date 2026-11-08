import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { CheckCircle2, X, ArrowRight, Zap, Star, ChevronDown } from 'lucide-react';

// ── Types ────────────────────────────────────────────────────────────────────

interface Plan {
  id: string;
  name: string;
  badge?: string;
  badgeColor?: string;
  popular?: boolean;
  monthlyPrice: number;
  yearlyPrice: number;
  description: string;
  accentColor: string;
  textColor: string;
  btnClass: string;
  features: Array<{ text: string; included: boolean }>;
  limits: { products: string; users: string; branches: string; invoices: string };
}

// ── Data ─────────────────────────────────────────────────────────────────────

const plans: Plan[] = [
  {
    id: 'starter',
    name: 'Starter',
    monthlyPrice: 0,
    yearlyPrice: 0,
    description: 'Perfect for small shops just getting started. Fully free, forever.',
    accentColor: 'border-slate-200',
    textColor: 'text-slate-900',
    btnClass: 'bg-slate-900 text-white hover:bg-slate-800',
    limits: { products: '50 Products', users: '2 Users', branches: '1 Branch', invoices: '100/month' },
    features: [
      { text: 'GST Invoicing', included: true },
      { text: 'Basic Inventory', included: true },
      { text: 'Customer Management', included: true },
      { text: 'Email Support', included: true },
      { text: 'Sales & Purchase Reports', included: false },
      { text: 'WhatsApp Billing', included: false },
      { text: 'SMS Reminders', included: false },
      { text: 'Multi-Branch', included: false },
      { text: 'Device Integration', included: false },
      { text: 'API Access', included: false },
    ],
  },
  {
    id: 'basic',
    name: 'Basic',
    monthlyPrice: 499,
    yearlyPrice: 4999,
    description: 'Ideal for growing businesses needing complete billing & reporting.',
    accentColor: 'border-blue-200',
    textColor: 'text-blue-700',
    btnClass: 'bg-blue-600 text-white hover:bg-blue-700',
    limits: { products: '500 Products', users: '5 Users', branches: '1 Branch', invoices: '1,000/month' },
    features: [
      { text: 'GST Invoicing', included: true },
      { text: 'Full Inventory', included: true },
      { text: 'Customer & Supplier Mgmt', included: true },
      { text: 'Email + Chat Support', included: true },
      { text: 'Sales & Purchase Reports', included: true },
      { text: 'GST Reports (GSTR-1, 3B)', included: true },
      { text: 'WhatsApp Billing', included: false },
      { text: 'SMS Reminders', included: false },
      { text: 'Multi-Branch', included: false },
      { text: 'API Access', included: false },
    ],
  },
  {
    id: 'professional',
    name: 'Professional',
    badge: '⚡ Most Popular',
    badgeColor: 'bg-indigo-600 text-white',
    popular: true,
    monthlyPrice: 999,
    yearlyPrice: 9999,
    description: 'For established businesses needing WhatsApp billing & integrations.',
    accentColor: 'border-indigo-500',
    textColor: 'text-indigo-700',
    btnClass: 'bg-indigo-600 text-white hover:bg-indigo-700 shadow-lg shadow-indigo-600/30',
    limits: { products: '5,000 Products', users: '15 Users', branches: '3 Branches', invoices: '10,000/month' },
    features: [
      { text: 'GST Invoicing', included: true },
      { text: 'Advanced Inventory & Warehouses', included: true },
      { text: 'Customer & Supplier Mgmt', included: true },
      { text: 'Priority Support', included: true },
      { text: 'Sales, Purchase & GST Reports', included: true },
      { text: 'Profit & Loss, Analytics', included: true },
      { text: 'WhatsApp Billing', included: true },
      { text: 'SMS Reminders', included: true },
      { text: 'Multi-Branch (up to 3)', included: true },
      { text: 'Barcode / Device Integration', included: true },
    ],
  },
  {
    id: 'enterprise',
    name: 'Enterprise',
    monthlyPrice: 2499,
    yearlyPrice: 24999,
    description: 'Unlimited everything for large enterprises with API access.',
    accentColor: 'border-purple-200',
    textColor: 'text-purple-700',
    btnClass: 'bg-purple-700 text-white hover:bg-purple-800',
    limits: { products: 'Unlimited', users: 'Unlimited', branches: 'Unlimited', invoices: 'Unlimited' },
    features: [
      { text: 'GST Invoicing', included: true },
      { text: 'Unlimited Inventory', included: true },
      { text: 'Customer & Supplier Mgmt', included: true },
      { text: 'Dedicated Account Manager', included: true },
      { text: 'All Reports + Custom Reports', included: true },
      { text: 'Profit & Loss, Analytics', included: true },
      { text: 'WhatsApp & SMS Billing', included: true },
      { text: 'Unlimited Multi-Branch', included: true },
      { text: 'All Device Integrations', included: true },
      { text: 'API Access + Custom Integrations', included: true },
    ],
  },
];

const faqs = [
  {
    q: 'Is Master Billing free to use?',
    a: 'Yes! Our Starter plan is completely free — no credit card needed. You can create invoices, manage up to 50 products and 2 users with no time limit.',
  },
  {
    q: 'Can I switch plans later?',
    a: 'Absolutely. You can upgrade or downgrade your plan at any time from your subscription settings. Upgrades take effect immediately.',
  },
  {
    q: 'Is GST included in the subscription price?',
    a: 'The prices listed above are exclusive of 18% GST, which will be added at checkout as per Indian tax laws.',
  },
  {
    q: 'What payment methods are accepted?',
    a: 'We accept UPI (PhonePe, GPay, Paytm), Credit/Debit Cards, Net Banking, and bank transfers.',
  },
  {
    q: 'Is my data safe?',
    a: 'Yes. Your data is encrypted at rest and in transit, backed up daily to secure cloud servers. You can also download a local backup at any time.',
  },
  {
    q: 'Do you offer a free trial for paid plans?',
    a: 'Yes! All plans come with a 30-day free trial. No credit card required to start your trial.',
  },
];

// ── Components ────────────────────────────────────────────────────────────────

const FAQ: React.FC<{ q: string; a: string }> = ({ q, a }) => {
  const [open, setOpen] = useState(false);
  return (
    <div className="border border-slate-200 rounded-xl overflow-hidden">
      <button
        onClick={() => setOpen(!open)}
        className="w-full flex items-center justify-between p-5 text-left bg-white hover:bg-slate-50 transition-colors gap-4"
      >
        <span className="font-semibold text-slate-900 text-sm">{q}</span>
        <ChevronDown className={`w-4 h-4 text-slate-400 shrink-0 transition-transform ${open ? 'rotate-180' : ''}`} />
      </button>
      {open && (
        <div className="px-5 pb-5 text-sm text-slate-600 leading-relaxed bg-slate-50 border-t border-slate-100">
          {a}
        </div>
      )}
    </div>
  );
};

// ── Main Page ─────────────────────────────────────────────────────────────────

export const PricingPage: React.FC = () => {
  const [yearly, setYearly] = useState(false);

  return (
    <div className="min-h-screen bg-slate-50 font-sans">

      {/* ── Navbar ── */}
      <nav className="sticky top-0 z-50 bg-white/80 backdrop-blur border-b border-slate-100 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 py-3 flex items-center justify-between">
          <Link to="/" className="flex items-center gap-2.5">
            <div className="w-9 h-9 bg-indigo-600 rounded-xl flex items-center justify-center text-white font-black text-lg shadow-md shadow-indigo-600/30">M</div>
            <span className="font-black text-slate-900 text-lg tracking-tight">Master Billing</span>
          </Link>
          <div className="hidden md:flex items-center gap-7 text-sm font-medium text-slate-600">
            <Link to="/" className="hover:text-indigo-600 transition-colors">Home</Link>
            <Link to="/pricing" className="text-indigo-600 font-bold">Pricing</Link>
          </div>
          <div className="flex items-center gap-3">
            <Link to="/login" className="text-sm font-semibold text-slate-700 hover:text-indigo-600 px-3 py-2 rounded-lg transition-colors">
              Sign In
            </Link>
            <Link to="/register" className="text-sm font-bold text-white bg-indigo-600 hover:bg-indigo-700 px-4 py-2.5 rounded-xl shadow-md shadow-indigo-600/25 transition-all flex items-center gap-1.5">
              Start Free <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          </div>
        </div>
      </nav>

      {/* ── Hero ── */}
      <section className="bg-gradient-to-b from-white to-slate-50 pt-16 pb-10 text-center px-4">
        <div className="inline-flex items-center gap-2 bg-indigo-50 border border-indigo-100 rounded-full px-4 py-1.5 text-xs font-semibold text-indigo-700 mb-4">
          <Zap className="w-3.5 h-3.5 text-indigo-500" /> Simple, transparent pricing
        </div>
        <h1 className="text-4xl md:text-5xl font-black text-slate-900 mb-3">Plans for every business</h1>
        <p className="text-slate-500 max-w-xl mx-auto text-sm mb-8">
          Start for free, scale as you grow. All plans include a 30-day free trial. Cancel anytime.
        </p>

        {/* ── Billing Toggle ── */}
        <div className="inline-flex items-center gap-3 bg-slate-100 rounded-2xl p-1.5 mb-2">
          <button
            onClick={() => setYearly(false)}
            className={`px-5 py-2 rounded-xl text-sm font-semibold transition-all ${!yearly ? 'bg-white shadow text-slate-900' : 'text-slate-500 hover:text-slate-700'}`}
          >
            Monthly
          </button>
          <button
            onClick={() => setYearly(true)}
            className={`px-5 py-2 rounded-xl text-sm font-semibold transition-all flex items-center gap-2 ${yearly ? 'bg-white shadow text-slate-900' : 'text-slate-500 hover:text-slate-700'}`}
          >
            Yearly
            <span className="bg-emerald-500 text-white text-[10px] font-bold px-2 py-0.5 rounded-full">Save 17%</span>
          </button>
        </div>
        {yearly && (
          <p className="text-xs text-emerald-600 font-semibold mt-1">🎉 You're saving up to ₹5,000/year!</p>
        )}
      </section>

      {/* ── Plans ── */}
      <section className="max-w-7xl mx-auto px-4 sm:px-6 pb-16">
        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-5">
          {plans.map((plan) => {
            const price = yearly ? plan.yearlyPrice : plan.monthlyPrice;
            const period = yearly ? '/year' : '/month';
            return (
              <div
                key={plan.id}
                className={`relative bg-white rounded-2xl border-2 ${plan.accentColor} ${plan.popular ? 'shadow-xl scale-[1.02] ring-2 ring-indigo-500/20' : 'shadow-sm'} flex flex-col overflow-hidden transition-all`}
              >
                {/* Badge */}
                {plan.badge && (
                  <div className={`${plan.badgeColor} text-xs font-bold px-4 py-1.5 text-center`}>
                    {plan.badge}
                  </div>
                )}

                <div className="p-6 flex-1 space-y-4">
                  {/* Header */}
                  <div>
                    <h3 className={`font-black text-xl ${plan.textColor}`}>{plan.name}</h3>
                    <p className="text-xs text-slate-500 mt-1 leading-relaxed">{plan.description}</p>
                  </div>

                  {/* Price */}
                  <div>
                    {price === 0 ? (
                      <div className="flex items-baseline gap-1">
                        <span className="text-4xl font-black text-slate-900">Free</span>
                        <span className="text-xs text-slate-400 font-medium">forever</span>
                      </div>
                    ) : (
                      <div className="flex items-baseline gap-1">
                        <span className="text-slate-400 text-lg font-bold">₹</span>
                        <span className="text-4xl font-black text-slate-900">{price.toLocaleString('en-IN')}</span>
                        <span className="text-xs text-slate-400 font-medium">{period}</span>
                      </div>
                    )}
                    {price > 0 && (
                      <p className="text-xs text-slate-400 mt-0.5">+ 18% GST</p>
                    )}
                  </div>

                  {/* Limits */}
                  <div className="bg-slate-50 rounded-xl p-3 grid grid-cols-2 gap-2">
                    {Object.entries(plan.limits).map(([k, v]) => (
                      <div key={k} className="text-center">
                        <div className="text-xs font-bold text-slate-800">{v}</div>
                        <div className="text-[10px] text-slate-400 capitalize">{k}</div>
                      </div>
                    ))}
                  </div>

                  {/* CTA */}
                  <Link
                    to="/register"
                    className={`w-full flex items-center justify-center gap-2 py-3 rounded-xl font-bold text-sm transition-all ${plan.btnClass}`}
                  >
                    {price === 0 ? 'Get Started Free' : 'Start 30-day Trial'}
                    <ArrowRight className="w-3.5 h-3.5" />
                  </Link>

                  {/* Features */}
                  <ul className="space-y-2.5 pt-1">
                    {plan.features.map((f) => (
                      <li key={f.text} className="flex items-center gap-2.5 text-xs">
                        {f.included ? (
                          <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500 shrink-0" />
                        ) : (
                          <X className="w-3.5 h-3.5 text-slate-300 shrink-0" />
                        )}
                        <span className={f.included ? 'text-slate-700' : 'text-slate-400 line-through decoration-slate-200'}>
                          {f.text}
                        </span>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            );
          })}
        </div>
      </section>

      {/* ── Trust Badges ── */}
      <section className="bg-white py-10 border-y border-slate-100">
        <div className="max-w-5xl mx-auto px-4 grid grid-cols-2 md:grid-cols-4 gap-6 text-center">
          {[
            { icon: '🔒', label: 'Bank-grade Security', sub: 'AES-256 Encryption' },
            { icon: '☁️', label: 'Cloud Backup', sub: 'Daily auto-backup' },
            { icon: '📱', label: 'Mobile & Desktop', sub: 'Android, iOS, Web' },
            { icon: '🇮🇳', label: 'GST Compliant', sub: 'GSTR-1, 3B, e-Way Bill' },
          ].map((b) => (
            <div key={b.label} className="space-y-1.5">
              <div className="text-3xl">{b.icon}</div>
              <div className="text-sm font-bold text-slate-900">{b.label}</div>
              <div className="text-xs text-slate-400">{b.sub}</div>
            </div>
          ))}
        </div>
      </section>

      {/* ── Comparison Table ── */}
      <section className="max-w-6xl mx-auto px-4 sm:px-6 py-16">
        <h2 className="text-2xl font-black text-slate-900 text-center mb-8">Full Feature Comparison</h2>
        <div className="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-auto">
          <table className="w-full text-sm min-w-[600px]">
            <thead>
              <tr className="bg-slate-50 border-b border-slate-100">
                <th className="text-left px-5 py-4 font-bold text-slate-900 w-1/3">Feature</th>
                {plans.map((p) => (
                  <th key={p.id} className={`px-4 py-4 font-bold text-center ${p.popular ? 'text-indigo-700 bg-indigo-50/60' : 'text-slate-700'}`}>
                    {p.name}
                    {p.popular && <div className="text-[9px] text-indigo-500 font-semibold">POPULAR</div>}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {[
                'GST Invoicing',
                'Inventory Management',
                'Customer & Supplier Mgmt',
                'Sales & Purchase Reports',
                'GST Reports (GSTR-1, 3B)',
                'Profit & Loss, Analytics',
                'WhatsApp Billing',
                'SMS Reminders',
                'Multi-Branch',
                'Barcode / Device Integration',
                'API Access',
                'Dedicated Account Manager',
              ].map((feature, idx) => (
                <tr key={feature} className={idx % 2 === 0 ? 'bg-white' : 'bg-slate-50/50'}>
                  <td className="px-5 py-3 font-medium text-slate-700">{feature}</td>
                  {plans.map((p) => {
                    const f = p.features.find((x) => x.text.includes(feature.split(' ')[0]));
                    const included = f?.included ?? false;
                    return (
                      <td key={p.id} className={`px-4 py-3 text-center ${p.popular ? 'bg-indigo-50/40' : ''}`}>
                        {included ? (
                          <CheckCircle2 className="w-4 h-4 text-emerald-500 mx-auto" />
                        ) : (
                          <X className="w-4 h-4 text-slate-200 mx-auto" />
                        )}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      {/* ── Testimonials ── */}
      <section className="bg-gradient-to-b from-indigo-700 to-indigo-900 py-16 px-4 text-white">
        <div className="max-w-5xl mx-auto text-center mb-10">
          <h2 className="text-2xl font-black mb-2">What our customers say</h2>
          <p className="text-indigo-200 text-sm">Over 2 lakh happy businesses across India</p>
        </div>
        <div className="max-w-5xl mx-auto grid sm:grid-cols-3 gap-5">
          {[
            { name: "Suresh Patel", role: "Kirana Store, Ahmedabad", text: "Master Billing is my shop's backbone. I bill 100+ customers daily with zero errors. The WhatsApp feature is amazing!", stars: 5 },
            { name: 'Deepika Nair', role: 'Boutique, Kochi', text: 'Switching from Tally was the best decision. So much simpler and the support team responds in minutes. Totally worth it!', stars: 5 },
            { name: 'Mohit Verma', role: 'Hardware Shop, Lucknow', text: 'Professional plan is excellent value. GST reports save me hours every quarter. My CA loves the Tally export feature.', stars: 5 },
          ].map((t) => (
            <div key={t.name} className="bg-white/10 backdrop-blur border border-white/20 rounded-2xl p-5 space-y-3">
              <div className="flex gap-1">
                {Array.from({ length: t.stars }).map((_, i) => (
                  <Star key={i} className="w-3.5 h-3.5 fill-amber-400 text-amber-400" />
                ))}
              </div>
              <p className="text-sm text-indigo-100 leading-relaxed">"{t.text}"</p>
              <div>
                <div className="font-bold text-white text-sm">{t.name}</div>
                <div className="text-xs text-indigo-300">{t.role}</div>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* ── FAQ ── */}
      <section className="max-w-3xl mx-auto px-4 py-16">
        <h2 className="text-2xl font-black text-slate-900 text-center mb-8">Frequently Asked Questions</h2>
        <div className="space-y-3">
          {faqs.map((faq) => (
            <FAQ key={faq.q} q={faq.q} a={faq.a} />
          ))}
        </div>
      </section>

      {/* ── Bottom CTA ── */}
      <section className="bg-indigo-600 py-14 px-4 text-center text-white">
        <h2 className="text-2xl md:text-3xl font-black mb-3">Ready to transform your billing?</h2>
        <p className="text-indigo-100 text-sm mb-6 max-w-md mx-auto">
          Join 2 lakh+ businesses. Start with the free plan — upgrade only when you need to.
        </p>
        <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
          <Link
            to="/register"
            className="inline-flex items-center gap-2 bg-white text-indigo-700 hover:bg-indigo-50 font-black px-8 py-4 rounded-2xl shadow-xl text-sm transition-all"
          >
            Start for Free <ArrowRight className="w-4 h-4" />
          </Link>
          <Link
            to="/login"
            className="inline-flex items-center gap-2 bg-white/15 hover:bg-white/25 border border-white/30 font-semibold px-8 py-4 rounded-2xl text-sm transition-all"
          >
            Already have an account? Sign In
          </Link>
        </div>
      </section>

      {/* ── Footer ── */}
      <footer className="bg-slate-900 text-slate-500 py-8 text-center text-xs">
        <p>© 2026 Master Billing. All rights reserved. &nbsp;·&nbsp; Made with ❤️ in India 🇮🇳</p>
        <div className="flex justify-center gap-6 mt-3">
          <a href="#" className="hover:text-white transition-colors">Privacy Policy</a>
          <a href="#" className="hover:text-white transition-colors">Terms of Service</a>
          <a href="#" className="hover:text-white transition-colors">Contact Us</a>
        </div>
      </footer>
    </div>
  );
};

