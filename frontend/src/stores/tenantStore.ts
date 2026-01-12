import { create } from 'zustand';
import type { Tenant } from '@/types';
import { api } from '@/lib/api';
import { extractSubdomain } from '@/lib/utils';

interface TenantState {
  tenant: Tenant | null;
  isLoading: boolean;
  error: string | null;
  resolveTenant: () => Promise<boolean>;
  clearTenant: () => void;
}

export const useTenantStore = create<TenantState>((set) => ({
  tenant: null,
  isLoading: true,
  error: null,

  resolveTenant: async () => {
    const slug = extractSubdomain();

    if (!slug) {
      set({
        tenant: null,
        isLoading: false,
        error: 'No tenant specified. Please access via a tenant subdomain.',
      });
      return false;
    }

    try {
      set({ isLoading: true, error: null });
      api.setTenantSlug(slug);
      const tenant = await api.resolveTenant(slug);
      set({
        tenant,
        isLoading: false,
        error: null,
      });
      return true;
    } catch (err) {
      set({
        tenant: null,
        isLoading: false,
        error: err instanceof Error ? err.message : 'Failed to resolve tenant',
      });
      return false;
    }
  },

  clearTenant: () => {
    api.setTenantSlug(null);
    set({
      tenant: null,
      isLoading: false,
      error: null,
    });
  },
}));
