export type UserRole = 'Viewer' | 'Member' | 'Admin';

export interface User {
  id: string;
  email: string;
  name: string;
  role: UserRole;
  lastLoginAt: string | null;
  isActive: boolean;
}

export interface Tenant {
  id: string;
  slug: string;
  name: string;
  subscriptionTier: string;
  settings: TenantSettings | null;
  branding: TenantBranding | null;
}

export interface TenantSettings {
  enableNotifications: boolean;
  timezone: string;
  language: string;
}

export interface TenantBranding {
  logoUrl: string | null;
  primaryColor: string;
  secondaryColor: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface DashboardStats {
  totalUsers: number;
  activeUsers: number;
  pendingInvitations: number;
  monthlyActiveRate: number;
}

export interface ActivityItem {
  id: string;
  action: string;
  entityType: string;
  description: string | null;
  userName: string | null;
  createdAt: string;
}

export interface ChartDataPoint {
  label: string;
  value: number;
}

export interface Invitation {
  id: string;
  email: string;
  role: UserRole;
  expiresAt: string;
  createdAt: string;
  invitedByName: string;
}

export interface Subscription {
  tier: string;
  status: string;
  currentPeriodStart: string | null;
  currentPeriodEnd: string | null;
  monthlyPrice: number | null;
  features: string[];
}

export interface Invoice {
  id: string;
  invoiceNumber: string;
  amount: number;
  status: string;
  issuedAt: string;
  paidAt: string | null;
}
