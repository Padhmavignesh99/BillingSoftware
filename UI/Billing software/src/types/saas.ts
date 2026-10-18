export type TenantPlan = 'starter' | 'professional' | 'enterprise' | 'custom';
export type PlanBillingCycle = 'monthly' | 'yearly';
export type TenantStatus = 'active' | 'trial' | 'past_due' | 'suspended' | 'cancelled';

export interface PlanFeature {
  id: TenantPlan;
  name: string;
  badge?: string;
  tagline: string;
  priceMonthly: number;
  priceYearly: number; // Discounted annual price per month
  maxMonthlyInvoices: number; // -1 for unlimited
  maxProducts: number;
  maxStaff: number;
  maxRegisters: number;
  hasAiInsights: boolean;
  hasMultiRegister: boolean;
  hasCustomBranding: boolean;
  hasBarcodeThermal: boolean;
  hasApiAccess: boolean;
  hasWhatsappAlerts: boolean;
  hasAuditLogs: boolean;
  hasPrioritySupport: boolean;
  popular?: boolean;
}

export interface TenantQuotaUsage {
  invoicesThisMonth: number;
  productsCount: number;
  staffCount: number;
  registersCount: number;
}

export interface TenantSubscriptionInvoice {
  id: string;
  invoiceNumber: string;
  date: string;
  amount: number;
  planName: string;
  billingCycle: PlanBillingCycle;
  status: 'paid' | 'pending' | 'failed';
  paymentMethod: string;
  pdfUrl?: string;
}

export interface Tenant {
  id: string;
  name: string;
  storeName: string;
  slug: string; // Subdomain e.g. srimurugan.billflow.io
  subdomain: string;
  ownerName: string;
  ownerEmail: string;
  ownerPhone: string;
  industry: 'supermarket' | 'restaurant' | 'electronics' | 'clothing' | 'pharmacy' | 'general';
  planId: TenantPlan;
  status: TenantStatus;
  billingCycle: PlanBillingCycle;
  subscriptionStartDate: string;
  nextRenewalDate: string;
  trialEndsAt: string;
  monthlyFee: number;
  totalPaid: number;
  colorTheme: 'indigo' | 'emerald' | 'violet' | 'amber' | 'rose' | 'cyan';
  customDomain?: string;
  createdAt: string;
  subscriptionInvoices: TenantSubscriptionInvoice[];
}

export interface SaaSAuditLog {
  id: string;
  tenantId: string;
  tenantName: string;
  eventType:
    | 'tenant_created'
    | 'plan_upgraded'
    | 'plan_downgraded'
    | 'payment_received'
    | 'tenant_suspended'
    | 'tenant_activated'
    | 'trial_extended'
    | 'quota_warning'
    | 'staff_added';
  description: string;
  timestamp: string;
  severity: 'info' | 'success' | 'warning' | 'danger';
}

export interface SaaSPlatformStats {
  totalTenants: number;
  activePaidTenants: number;
  trialTenants: number;
  monthlyRecurringRevenue: number;
  annualRecurringRevenue: number;
  churnRate: number;
  totalInvoicesProcessed: number;
  platformGmv: number; // Gross Merchandise Value processed across all tenants
  growthPercentage: number;
}
