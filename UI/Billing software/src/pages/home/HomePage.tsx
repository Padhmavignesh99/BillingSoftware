import React from 'react';
import { Link } from 'react-router-dom';
import {
  Zap, FileText, BarChart2, Package, Users, MessageCircle,
  ArrowRight, CheckCircle2, Star, Shield, Smartphone, ChevronRight,
  Receipt, TrendingUp, Printer, RefreshCw
} from 'lucide-react';

// ── Reusable components ──────────────────────────────────────────────────────

const Badge: React.FC<{ children: React.ReactNode; color?: string }> = ({
  children, color = 'indigo',
}) => (
  <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-${color}-50 text-${color}-700 border border-${color}-100`}>
    {children}
  </span>
);

const StatCard: React.FC<{ value: string; label: string }> = ({ value, label }) => (
  <div className="text-center px-6 py-4">
    <div className="text-3xl font-black text-indigo-600">{value}</div>
    <div className="text-sm text-slate-500 mt-1">{label}</div>
  </div>
);

const FeatureCard: React.FC<{
  icon: React.ReactNode;
  title: string;
  desc: string;
  color: string;
}> = ({ icon, title, desc, color }) => (
  <div className="bg-white rounded-2xl p-6 border border-slate-100 shadow-sm hover:shadow-md transition-shadow group">
    <div className={`w-12 h-12 rounded-xl ${color} flex items-center justify-center mb-4 group-hover:scale-105 transition-transform`}>
      {icon}
    </div>
    <h3 className="font-bold text-slate-900 mb-2">{title}</h3>
    <p className="text-sm text-slate-500 leading-relaxed">{desc}</p>
  </div>
);

const TestimonialCard: React.FC<{
  name: string; role: string; text: string; rating: number;
}> = ({ name, role, text, rating }) => (
  <div className="bg-white rounded-2xl p-6 border border-slate-100 shadow-sm">
    <div className="flex gap-1 mb-3">
      {Array.from({ length: rating }).map((_, i) => (
        <Star key={i} className="w-4 h-4 fill-amber-400 text-amber-400" />
      ))}
    </div>
    <p className="text-sm text-slate-600 leading-relaxed mb-4">"{text}"</p>
    <div>
      <div className="font-semibold text-slate-900 text-sm">{name}</div>
      <div className="text-xs text-slate-400">{role}</div>
    </div>
  </div>
);

const CheckItem: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <li className="flex items-center gap-2.5 text-sm text-slate-700">
    <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0" />
    {children}
  </li>
);

// ── Main Page ────────────────────────────────────────────────────────────────

export const HomePage: React.FC = () => {
  return (
    <div className="min-h-screen bg-slate-50 font-sans">

      {/* ── Navbar ── */}
      <nav className="sticky top-0 z-50 bg-white/80 backdrop-blur border-b border-slate-100 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 py-3 flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <div className="w-9 h-9 bg-indigo-600 rounded-xl flex items-center justify-center text-white font-black text-lg shadow-md shadow-indigo-600/30">
              M
            </div>
            <span className="font-black text-slate-900 text-lg tracking-tight">Master Billing</span>
          </div>
          <div className="hidden md:flex items-center gap-7 text-sm font-medium text-slate-600">
            <a href="#features" className="hover:text-indigo-600 transition-colors">Features</a>
            <Link to="/pricing" className="hover:text-indigo-600 transition-colors">Pricing</Link>
            <a href="#testimonials" className="hover:text-indigo-600 transition-colors">Reviews</a>
          </div>
          <div className="flex items-center gap-3">
            <Link
              to="/login"
              className="text-sm font-semibold text-slate-700 hover:text-indigo-600 px-3 py-2 rounded-lg transition-colors"
            >
              Sign In
            </Link>
            <Link
              to="/register"
              className="text-sm font-bold text-white bg-indigo-600 hover:bg-indigo-700 px-4 py-2.5 rounded-xl shadow-md shadow-indigo-600/25 transition-all flex items-center gap-1.5"
            >
              Start Free <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          </div>
        </div>
      </nav>

      {/* ── Hero ── */}
      <section className="relative overflow-hidden bg-gradient-to-br from-indigo-700 via-indigo-600 to-purple-700 text-white">
        {/* Blobs */}
        <div className="absolute -top-32 -right-32 w-96 h-96 bg-white/5 rounded-full blur-3xl" />
        <div className="absolute -bottom-32 -left-32 w-96 h-96 bg-purple-900/30 rounded-full blur-3xl" />

        <div className="max-w-7xl mx-auto px-4 sm:px-6 py-20 md:py-28 relative z-10">
          <div className="max-w-3xl mx-auto text-center space-y-6">
            <div className="inline-flex items-center gap-2 bg-white/10 border border-white/20 rounded-full px-4 py-1.5 text-sm font-medium backdrop-blur">
              <Zap className="w-3.5 h-3.5 text-amber-300" />
              India's #1 GST Billing Software for SMEs
            </div>
            <h1 className="text-4xl md:text-6xl font-black tracking-tight leading-tight">
              Bill Smarter,<br />
              <span className="text-amber-300">Grow Faster</span>
            </h1>
            <p className="text-lg text-indigo-100 max-w-xl mx-auto leading-relaxed">
              Create GST invoices in seconds, track inventory, manage customers, and send bills via WhatsApp — all in one powerful platform.
            </p>
            <div className="flex flex-col sm:flex-row items-center justify-center gap-4 pt-2">
              <Link
                to="/register"
                className="w-full sm:w-auto inline-flex items-center justify-center gap-2 bg-white text-indigo-700 hover:bg-indigo-50 font-black px-8 py-4 rounded-2xl shadow-xl text-sm transition-all"
              >
                Start Free Trial <ArrowRight className="w-4 h-4" />
              </Link>
              <Link
                to="/pricing"
                className="w-full sm:w-auto inline-flex items-center justify-center gap-2 bg-white/10 hover:bg-white/20 border border-white/30 font-semibold px-8 py-4 rounded-2xl text-sm transition-all"
              >
                View Pricing <ChevronRight className="w-4 h-4" />
              </Link>
            </div>
            <p className="text-xs text-indigo-200">
              ✓ No credit card required &nbsp;·&nbsp; ✓ 30-day free trial &nbsp;·&nbsp; ✓ Cancel anytime
            </p>
          </div>
        </div>

        {/* Wave divider */}
        <svg viewBox="0 0 1440 80" fill="none" xmlns="http://www.w3.org/2000/svg" className="w-full block">
          <path d="M0,80 L1440,80 L1440,30 Q720,0 0,30 Z" fill="#f8fafc" />
        </svg>
      </section>

      {/* ── Stats ── */}
      <section className="bg-slate-50">
        <div className="max-w-5xl mx-auto px-4 py-8">
          <div className="bg-white rounded-2xl border border-slate-100 shadow-sm grid grid-cols-2 md:grid-cols-4 divide-x divide-y md:divide-y-0 divide-slate-100">
            <StatCard value="2 Lakh+" label="Businesses Trust Us" />
            <StatCard value="5 Cr+" label="Invoices Generated" />
            <StatCard value="4.8★" label="App Store Rating" />
            <StatCard value="₹0" label="to Get Started" />
          </div>
        </div>
      </section>

      {/* ── Features ── */}
      <section id="features" className="py-20 bg-slate-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6">
          <div className="text-center mb-12">
            <Badge color="indigo">Everything you need</Badge>
            <h2 className="text-3xl md:text-4xl font-black text-slate-900 mt-3 mb-3">
              All-in-one Billing Platform
            </h2>
            <p className="text-slate-500 max-w-xl mx-auto text-sm">
              From billing to inventory to GST reports — Master Billing handles it all so you can focus on your business.
            </p>
          </div>
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5">
            <FeatureCard
              icon={<Receipt className="w-6 h-6 text-indigo-600" />}
              color="bg-indigo-50"
              title="GST Invoicing in Seconds"
              desc="Create beautiful, GST-compliant invoices in under 10 seconds. Supports CGST, SGST, IGST, HSN codes, and e-invoicing."
            />
            <FeatureCard
              icon={<Package className="w-6 h-6 text-emerald-600" />}
              color="bg-emerald-50"
              title="Inventory Management"
              desc="Track stock in real-time across multiple warehouses. Get low-stock alerts and auto-reorder suggestions before you run out."
            />
            <FeatureCard
              icon={<MessageCircle className="w-6 h-6 text-green-600" />}
              color="bg-green-50"
              title="WhatsApp & SMS Billing"
              desc="Send invoices, payment reminders, and receipts directly to customers on WhatsApp and SMS with one tap."
            />
            <FeatureCard
              icon={<BarChart2 className="w-6 h-6 text-purple-600" />}
              color="bg-purple-50"
              title="Reports & Analytics"
              desc="Profit & Loss, GST reports (GSTR-1, GSTR-3B), sales trends, customer analytics — all in one dashboard."
            />
            <FeatureCard
              icon={<Users className="w-6 h-6 text-blue-600" />}
              color="bg-blue-50"
              title="Customer & Supplier Management"
              desc="Maintain ledgers, track outstanding balances, record payments, and send automatic payment reminders."
            />
            <FeatureCard
              icon={<Printer className="w-6 h-6 text-rose-600" />}
              color="bg-rose-50"
              title="Thermal & Regular Printing"
              desc="Print bills on 58mm, 80mm thermal printers or A4/A5 paper. Supports barcode scanners and weighing scales."
            />
            <FeatureCard
              icon={<Smartphone className="w-6 h-6 text-amber-600" />}
              color="bg-amber-50"
              title="Works on Mobile & Desktop"
              desc="Access your billing data from anywhere — desktop browser, Android, or iOS. Cloud-synced automatically."
            />
            <FeatureCard
              icon={<TrendingUp className="w-6 h-6 text-teal-600" />}
              color="bg-teal-50"
              title="Multi-Branch Support"
              desc="Manage multiple shop branches from a single account. Consolidated reports across all your locations."
            />
            <FeatureCard
              icon={<RefreshCw className="w-6 h-6 text-slate-600" />}
              color="bg-slate-100"
              title="Recurring Invoices & Challans"
              desc="Auto-generate recurring invoices for regular customers. Convert delivery challans to invoices in one click."
            />
          </div>
        </div>
      </section>

      {/* ── Why Us ── */}
      <section className="py-20 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6">
          <div className="grid md:grid-cols-2 gap-12 items-center">
            <div className="space-y-6">
              <Badge color="emerald">Why businesses choose us</Badge>
              <h2 className="text-3xl md:text-4xl font-black text-slate-900">
                Built for Indian Businesses, by Indians
              </h2>
              <p className="text-slate-500 leading-relaxed">
                Master Billing is designed specifically for the Indian market — full GST compliance, Hindi & Tamil language support, UPI payment integration, and local customer support in your language.
              </p>
              <ul className="space-y-3">
                <CheckItem>100% GST compliant — GSTR-1, GSTR-3B, e-Way Bills</CheckItem>
                <CheckItem>Multi-language: English, Hindi, Tamil</CheckItem>
                <CheckItem>UPI, Card, Cash, Credit payment modes</CheckItem>
                <CheckItem>Works offline — no internet? No problem</CheckItem>
                <CheckItem>Data backup to cloud & local drive</CheckItem>
                <CheckItem>Tally export for CA/accountant compatibility</CheckItem>
              </ul>
              <Link
                to="/register"
                className="inline-flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white font-bold px-6 py-3 rounded-xl shadow-md shadow-indigo-600/25 text-sm transition-all"
              >
                Start Your Free Trial <ArrowRight className="w-4 h-4" />
              </Link>
            </div>
            <div className="grid grid-cols-2 gap-4">
              {[
                { icon: '🧾', title: 'GST Invoice', desc: 'CGST / SGST / IGST' },
                { icon: '📦', title: 'Inventory', desc: 'Real-time stock tracking' },
                { icon: '💬', title: 'WhatsApp', desc: 'Send bills instantly' },
                { icon: '📊', title: 'Reports', desc: 'Profit, GST, Sales' },
                { icon: '🏪', title: 'Multi-Branch', desc: 'All shops in one place' },
                { icon: '🔒', title: 'Secure', desc: 'Your data, your control' },
              ].map((item) => (
                <div key={item.title} className="bg-slate-50 rounded-2xl p-5 border border-slate-100 hover:border-indigo-200 hover:bg-indigo-50/40 transition-colors">
                  <div className="text-3xl mb-2">{item.icon}</div>
                  <div className="font-bold text-slate-900 text-sm">{item.title}</div>
                  <div className="text-xs text-slate-500 mt-0.5">{item.desc}</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* ── Testimonials ── */}
      <section id="testimonials" className="py-20 bg-slate-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6">
          <div className="text-center mb-12">
            <Badge color="amber">Customer Stories</Badge>
            <h2 className="text-3xl font-black text-slate-900 mt-3">
              Trusted by 2 Lakh+ Business Owners
            </h2>
          </div>
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5">
            <TestimonialCard
              name="Ramesh Kumar"
              role="Textile Wholesale, Surat"
              rating={5}
              text="Billings that used to take 20 minutes now take 2 minutes. WhatsApp sharing is a game-changer for my business. My customers love getting bills instantly!"
            />
            <TestimonialCard
              name="Priya Sharma"
              role="Medical Store, Jaipur"
              rating={5}
              text="Expiry date tracking and batch management is perfect for our pharmacy. GST reports are generated automatically — no more manual calculation errors."
            />
            <TestimonialCard
              name="Arun Krishnan"
              role="Electronics Retail, Chennai"
              rating={5}
              text="Managing 3 branches from one dashboard is incredible. The multi-language support in Tamil made it easy for all my staff to use. Highly recommended!"
            />
          </div>
        </div>
      </section>

      {/* ── CTA Banner ── */}
      <section className="py-20 bg-gradient-to-r from-indigo-600 to-purple-700 text-white">
        <div className="max-w-4xl mx-auto px-4 text-center space-y-6">
          <div className="inline-flex items-center gap-2 bg-white/15 border border-white/20 rounded-full px-4 py-1.5 text-sm font-medium">
            <Zap className="w-3.5 h-3.5 text-amber-300" />
            Limited offer: First 3 months free on Professional plan
          </div>
          <h2 className="text-3xl md:text-4xl font-black">
            Start Billing Smarter Today
          </h2>
          <p className="text-indigo-100 max-w-xl mx-auto">
            Join 2 lakh+ Indian business owners who rely on Master Billing every day. No credit card required.
          </p>
          <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
            <Link
              to="/register"
              className="w-full sm:w-auto inline-flex items-center justify-center gap-2 bg-white text-indigo-700 hover:bg-indigo-50 font-black px-8 py-4 rounded-2xl shadow-xl text-sm transition-all"
            >
              Create Free Account <ArrowRight className="w-4 h-4" />
            </Link>
            <Link
              to="/pricing"
              className="w-full sm:w-auto inline-flex items-center justify-center gap-2 bg-white/15 hover:bg-white/25 border border-white/30 font-semibold px-8 py-4 rounded-2xl text-sm transition-all"
            >
              View Pricing
            </Link>
          </div>
          <div className="flex flex-wrap justify-center gap-6 text-xs text-indigo-200 pt-2">
            <span className="flex items-center gap-1.5"><Shield className="w-3.5 h-3.5" /> Secure & Encrypted</span>
            <span className="flex items-center gap-1.5"><CheckCircle2 className="w-3.5 h-3.5" /> 30-day free trial</span>
            <span className="flex items-center gap-1.5"><CheckCircle2 className="w-3.5 h-3.5" /> No credit card needed</span>
          </div>
        </div>
      </section>

      {/* ── Footer ── */}
      <footer className="bg-slate-900 text-slate-400 py-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6">
          <div className="grid sm:grid-cols-2 md:grid-cols-4 gap-8 mb-10">
            <div className="space-y-3">
              <div className="flex items-center gap-2.5">
                <div className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center text-white font-black text-sm">M</div>
                <span className="font-black text-white">Master Billing</span>
              </div>
              <p className="text-xs leading-relaxed">India's fastest GST billing software for retailers, wholesalers & service businesses.</p>
            </div>
            <div>
              <h4 className="text-white font-semibold text-sm mb-3">Product</h4>
              <ul className="space-y-2 text-xs">
                <li><a href="#features" className="hover:text-white transition-colors">Features</a></li>
                <li><Link to="/pricing" className="hover:text-white transition-colors">Pricing</Link></li>
                <li><a href="#" className="hover:text-white transition-colors">Mobile App</a></li>
                <li><a href="#" className="hover:text-white transition-colors">API Docs</a></li>
              </ul>
            </div>
            <div>
              <h4 className="text-white font-semibold text-sm mb-3">Company</h4>
              <ul className="space-y-2 text-xs">
                <li><a href="#" className="hover:text-white transition-colors">About Us</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Blog</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Careers</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Contact</a></li>
              </ul>
            </div>
            <div>
              <h4 className="text-white font-semibold text-sm mb-3">Legal</h4>
              <ul className="space-y-2 text-xs">
                <li><a href="#" className="hover:text-white transition-colors">Privacy Policy</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Terms of Service</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Refund Policy</a></li>
              </ul>
            </div>
          </div>
          <div className="border-t border-slate-800 pt-6 flex flex-col sm:flex-row items-center justify-between gap-3 text-xs">
            <span>© 2026 Master Billing. All rights reserved.</span>
            <span>Made with ❤️ in India 🇮🇳</span>
          </div>
        </div>
      </footer>
    </div>
  );
};

