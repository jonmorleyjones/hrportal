import type {
  LoginRequest,
  LoginResponse,
  Tenant,
  User,
  DashboardStats,
  ActivityItem,
  ChartDataPoint,
  Subscription,
  Invoice,
  UserRole,
  RequestTypeCard,
  RequestType,
  RequestResponse,
  CreateRequestTypeRequest,
  UpdateRequestTypeRequest,
  FileUploadResponse,
  FileInfo,
  HrConsultantLoginRequest,
  HrConsultantLoginResponse,
  TenantListItem,
  TenantListResponse,
  CreateTenantRequest
} from '@/types';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

// Map numeric role values from backend enum to string
const roleMap: Record<number, UserRole> = {
  0: 'Viewer',
  1: 'Member',
  2: 'Admin',
};

function mapUserRole(user: User & { role: number | UserRole }): User {
  return {
    ...user,
    role: typeof user.role === 'number' ? roleMap[user.role] || 'Viewer' : user.role,
  };
}

class ApiClient {
  private accessToken: string | null = null;
  private tenantSlug: string | null = null;

  setAccessToken(token: string | null) {
    this.accessToken = token;
  }

  setTenantSlug(slug: string | null) {
    this.tenantSlug = slug;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    if (this.accessToken) {
      (headers as Record<string, string>)['Authorization'] = `Bearer ${this.accessToken}`;
    }

    if (this.tenantSlug) {
      (headers as Record<string, string>)['X-Tenant-ID'] = this.tenantSlug;
    }

    const response = await fetch(`${API_URL}${endpoint}`, {
      ...options,
      headers,
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ error: 'Request failed' }));
      throw new Error(error.error || 'Request failed');
    }

    return response.json();
  }

  // Auth
  async login(data: LoginRequest): Promise<LoginResponse> {
    const response = await this.request<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    // Map numeric role to string
    return {
      ...response,
      user: mapUserRole(response.user as User & { role: number | UserRole }),
    };
  }

  async refresh(refreshToken: string): Promise<{ accessToken: string; refreshToken: string }> {
    return this.request('/api/auth/refresh', {
      method: 'POST',
      body: JSON.stringify({ refreshToken }),
    });
  }

  async logout(refreshToken: string): Promise<void> {
    await this.request('/api/auth/logout', {
      method: 'POST',
      body: JSON.stringify({ refreshToken }),
    });
  }

  async hrConsultantLogin(data: HrConsultantLoginRequest): Promise<HrConsultantLoginResponse> {
    return this.request<HrConsultantLoginResponse>('/api/auth/hr-login', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Tenant
  async resolveTenant(slug: string): Promise<Tenant> {
    return this.request<Tenant>(`/api/tenants/resolve?slug=${encodeURIComponent(slug)}`);
  }

  async getCurrentTenant(): Promise<Tenant> {
    return this.request<Tenant>('/api/tenants/current');
  }

  async updateTenantSettings(settings: Tenant['settings']): Promise<void> {
    await this.request('/api/tenants/settings', {
      method: 'PUT',
      body: JSON.stringify({ settings }),
    });
  }

  async updateTenantBranding(branding: Tenant['branding']): Promise<void> {
    await this.request('/api/tenants/branding', {
      method: 'PUT',
      body: JSON.stringify({ branding }),
    });
  }

  // Users
  async getUsers(page = 1, pageSize = 10): Promise<{ users: User[]; totalCount: number }> {
    return this.request(`/api/users?page=${page}&pageSize=${pageSize}`);
  }

  async getUser(id: string): Promise<User> {
    return this.request<User>(`/api/users/${id}`);
  }

  async inviteUser(email: string, role: UserRole): Promise<void> {
    await this.request('/api/users/invite', {
      method: 'POST',
      body: JSON.stringify({ email, role }),
    });
  }

  async updateUserRole(id: string, role: UserRole): Promise<void> {
    await this.request(`/api/users/${id}/role`, {
      method: 'PUT',
      body: JSON.stringify({ role }),
    });
  }

  async deactivateUser(id: string): Promise<void> {
    await this.request(`/api/users/${id}`, {
      method: 'DELETE',
    });
  }

  // Dashboard
  async getDashboardStats(): Promise<DashboardStats> {
    return this.request<DashboardStats>('/api/dashboard/stats');
  }

  async getActivityFeed(limit = 10): Promise<{ activities: ActivityItem[]; totalCount: number }> {
    return this.request(`/api/dashboard/activity?limit=${limit}`);
  }

  async getChartData(chartType: string): Promise<{ chartType: string; data: ChartDataPoint[] }> {
    return this.request(`/api/dashboard/charts/${chartType}`);
  }

  // Billing
  async getSubscription(): Promise<Subscription> {
    return this.request<Subscription>('/api/billing/subscription');
  }

  async getInvoices(page = 1, pageSize = 10): Promise<{ invoices: Invoice[]; totalCount: number }> {
    return this.request(`/api/billing/invoices?page=${page}&pageSize=${pageSize}`);
  }

  async upgradePlan(newTier: string): Promise<void> {
    await this.request('/api/billing/upgrade', {
      method: 'POST',
      body: JSON.stringify({ newTier }),
    });
  }

  // Request Types (User endpoints)
  async getRequestTypes(): Promise<RequestTypeCard[]> {
    return this.request<RequestTypeCard[]>('/api/requests/types');
  }

  async getRequestType(id: string): Promise<RequestType> {
    return this.request<RequestType>(`/api/requests/types/${id}`);
  }

  async submitRequestResponse(requestTypeId: string, responseJson: string, isComplete: boolean): Promise<{ message: string; id: string }> {
    return this.request(`/api/requests/types/${requestTypeId}/responses`, {
      method: 'POST',
      body: JSON.stringify({ responseJson, isComplete }),
    });
  }

  async getUserRequestResponses(): Promise<RequestResponse[]> {
    return this.request<RequestResponse[]>('/api/requests/responses');
  }

  // Request Types Admin
  async getRequestTypesAdmin(): Promise<RequestType[]> {
    return this.request<RequestType[]>('/api/requests/admin/types');
  }

  async createRequestType(data: CreateRequestTypeRequest): Promise<RequestType> {
    return this.request<RequestType>('/api/requests/admin/types', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async updateRequestType(id: string, data: UpdateRequestTypeRequest): Promise<RequestType> {
    return this.request<RequestType>(`/api/requests/admin/types/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  async deleteRequestType(id: string): Promise<void> {
    await this.request(`/api/requests/admin/types/${id}`, {
      method: 'DELETE',
    });
  }

  async getRequestTypeResponses(requestTypeId: string): Promise<RequestResponse[]> {
    return this.request<RequestResponse[]>(`/api/requests/admin/types/${requestTypeId}/responses`);
  }

  async getAllRequestResponses(): Promise<RequestResponse[]> {
    return this.request<RequestResponse[]>('/api/requests/admin/responses');
  }

  // File Upload
  async uploadFile(file: File, questionName: string): Promise<FileUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);

    const headers: HeadersInit = {};
    if (this.accessToken) {
      headers['Authorization'] = `Bearer ${this.accessToken}`;
    }
    if (this.tenantSlug) {
      headers['X-Tenant-ID'] = this.tenantSlug;
    }

    const response = await fetch(
      `${API_URL}/api/files/upload?questionName=${encodeURIComponent(questionName)}`,
      {
        method: 'POST',
        headers,
        body: formData,
      }
    );

    if (!response.ok) {
      const error = await response.json().catch(() => ({ error: 'Upload failed' }));
      throw new Error(error.error || 'Upload failed');
    }

    return response.json();
  }

  async deleteFile(fileId: string): Promise<void> {
    await this.request(`/api/files/${fileId}`, { method: 'DELETE' });
  }

  async linkFilesToResponse(requestResponseId: string, fileIds: string[]): Promise<void> {
    await this.request('/api/files/link', {
      method: 'POST',
      body: JSON.stringify({ requestResponseId, fileIds }),
    });
  }

  getFileDownloadUrl(fileId: string): string {
    return `${API_URL}/api/files/${fileId}`;
  }

  async getResponseFiles(responseId: string): Promise<FileInfo[]> {
    return this.request<FileInfo[]>(`/api/files/admin/response/${responseId}`);
  }

  // HR Consultant endpoints
  async getHrDashboardStats(): Promise<HrDashboardStats> {
    return this.request<HrDashboardStats>('/api/hr/dashboard/stats');
  }

  async getHrTenantStats(tenantId: string): Promise<HrTenantStats> {
    return this.request<HrTenantStats>(`/api/hr/tenants/${tenantId}/stats`);
  }

  async getHrTenantResponses(
    tenantId: string,
    page = 1,
    pageSize = 20,
    isComplete?: boolean
  ): Promise<HrResponsesListResponse> {
    let url = `/api/hr/tenants/${tenantId}/responses?page=${page}&pageSize=${pageSize}`;
    if (isComplete !== undefined) {
      url += `&isComplete=${isComplete}`;
    }
    return this.request<HrResponsesListResponse>(url);
  }

  async getHrTenantRequestTypes(tenantId: string): Promise<HrRequestTypeDto[]> {
    return this.request<HrRequestTypeDto[]>(`/api/hr/tenants/${tenantId}/request-types`);
  }

  async getHrTenantRequestTypeDetail(tenantId: string, requestTypeId: string): Promise<HrRequestTypeDetailDto> {
    return this.request<HrRequestTypeDetailDto>(`/api/hr/tenants/${tenantId}/request-types/${requestTypeId}`);
  }

  async updateHrTenantRequestType(
    tenantId: string,
    requestTypeId: string,
    data: HrUpdateRequestTypeRequest
  ): Promise<HrRequestTypeDetailDto> {
    return this.request<HrRequestTypeDetailDto>(`/api/hr/tenants/${tenantId}/request-types/${requestTypeId}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  async createHrTenantRequestType(
    tenantId: string,
    data: HrCreateRequestTypeRequest
  ): Promise<HrRequestTypeDto> {
    return this.request<HrRequestTypeDto>(`/api/hr/tenants/${tenantId}/request-types`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async getHrTenantResponseDetail(tenantId: string, responseId: string): Promise<HrResponseDetailDto> {
    return this.request<HrResponseDetailDto>(`/api/hr/tenants/${tenantId}/responses/${responseId}`);
  }

  // HR Tenant Management
  async getHrTenants(): Promise<TenantListResponse> {
    return this.request<TenantListResponse>('/api/hr/tenants');
  }

  async createHrTenant(data: CreateTenantRequest): Promise<TenantListItem> {
    return this.request<TenantListItem>('/api/hr/tenants', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async toggleTenantStatus(tenantId: string, isActive: boolean): Promise<void> {
    await this.request(`/api/hr/tenants/${tenantId}/status`, {
      method: 'PUT',
      body: JSON.stringify({ isActive }),
    });
  }
}

// HR Consultant specific types
export interface HrDashboardStats {
  totalTenants: number;
  totalResponses: number;
  pendingReview: number;
  completionRate: number;
}

export interface HrTenantStats {
  totalUsers: number;
  totalResponses: number;
  pendingResponses: number;
  requestTypesCount: number;
  completionRate: number;
}

export interface HrResponseDto {
  id: string;
  requestTypeId: string;
  requestTypeName: string;
  requestTypeIcon: string;
  userId: string;
  userName: string;
  userEmail: string;
  versionNumber: number;
  isComplete: boolean;
  startedAt: string;
  completedAt: string | null;
}

export interface HrResponsesListResponse {
  responses: HrResponseDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface HrRequestTypeDto {
  id: string;
  name: string;
  description: string | null;
  icon: string;
  currentVersionNumber: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  totalResponses: number;
  completedResponses: number;
}

export interface HrRequestTypeDetailDto {
  id: string;
  name: string;
  description: string | null;
  icon: string;
  currentVersionNumber: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  versions: HrRequestTypeVersionDto[];
}

export interface HrRequestTypeVersionDto {
  id: string;
  versionNumber: number;
  formJson: string;
  createdAt: string;
  responseCount: number;
}

export interface HrUpdateRequestTypeRequest {
  name: string;
  description: string | null;
  icon: string | null;
  formJson: string;
  isActive: boolean;
}

export interface HrCreateRequestTypeRequest {
  name: string;
  description: string | null;
  icon: string | null;
  formJson: string;
}

export interface HrResponseDetailDto {
  id: string;
  requestTypeId: string;
  requestTypeName: string;
  requestTypeIcon: string;
  userId: string;
  userName: string;
  userEmail: string;
  versionNumber: number;
  responseJson: string;
  formJson: string;
  isComplete: boolean;
  startedAt: string;
  completedAt: string | null;
}

export const api = new ApiClient();
