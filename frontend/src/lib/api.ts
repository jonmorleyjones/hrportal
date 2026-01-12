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
  UserRole
} from '@/types';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

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
    return this.request<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    });
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
}

export const api = new ApiClient();
