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

// Request Types (formerly Onboarding)
export interface RequestTypeCard {
  id: string;
  name: string;
  description: string | null;
  icon: string;
  isActive: boolean;
}

export interface RequestType {
  id: string;
  name: string;
  description: string | null;
  icon: string;
  currentVersionNumber: number;
  formJson: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface RequestResponse {
  id: string;
  requestTypeId: string;
  requestTypeName: string;
  userId: string;
  userName: string;
  versionNumber: number;
  responseJson: string;
  isComplete: boolean;
  startedAt: string;
  completedAt: string | null;
}

export interface CreateRequestTypeRequest {
  name: string;
  description?: string | null;
  icon?: string | null;
  formJson: string;
}

export interface UpdateRequestTypeRequest {
  name: string;
  description?: string | null;
  icon?: string | null;
  formJson: string;
  isActive: boolean;
}

// File Upload
export interface FileUploadResponse {
  id: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  downloadUrl: string;
}

export interface FileInfo {
  id: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  downloadUrl: string;
  uploadedAt: string;
}

// ============================================
// HR CONSULTANT TYPES
// ============================================

export interface Consultant {
  id: string;
  email: string;
  name: string;
  lastLoginAt: string | null;
  isActive: boolean;
}

export interface TenantSummary {
  id: string;
  slug: string;
  name: string;
  subscriptionTier: string;
  userCount: number;
  activeRequestTypes: number;
  pendingResponses: number;
  canManageRequestTypes: boolean;
  canManageSettings: boolean;
  canManageBranding: boolean;
  canViewResponses: boolean;
}

export interface TenantPermissions {
  canManageRequestTypes: boolean;
  canManageSettings: boolean;
  canManageBranding: boolean;
  canViewResponses: boolean;
}

export interface TenantDetail {
  id: string;
  slug: string;
  name: string;
  subscriptionTier: string;
  settings: TenantSettings | null;
  branding: TenantBranding | null;
  userCount: number;
  activeRequestTypes: number;
  totalResponses: number;
  pendingResponses: number;
  createdAt: string;
  isActive: boolean;
  permissions: TenantPermissions;
}

export interface ConsultantLoginRequest {
  email: string;
  password: string;
}

export interface ConsultantLoginResponse {
  accessToken: string;
  refreshToken: string;
  consultant: Consultant;
  assignedTenants: TenantSummary[];
}

export interface CrossTenantRequest {
  id: string;
  tenantId: string;
  tenantName: string;
  tenantSlug: string;
  requestTypeId: string;
  requestTypeName: string;
  requestTypeIcon: string;
  userId: string;
  userName: string;
  userEmail: string;
  isComplete: boolean;
  startedAt: string;
  completedAt: string | null;
}

export interface ConsultantRequestType {
  id: string;
  name: string;
  description: string | null;
  icon: string;
  isActive: boolean;
  currentVersionNumber: number;
  totalResponses: number;
  completedResponses: number;
  createdAt: string;
  updatedAt: string;
}

export interface ConsultantDashboardStats {
  totalTenants: number;
  totalUsers: number;
  totalRequestTypes: number;
  totalResponses: number;
  pendingResponses: number;
  completedResponsesThisWeek: number;
}

export interface UpdateRequestTypeStatusRequest {
  isActive: boolean;
}
