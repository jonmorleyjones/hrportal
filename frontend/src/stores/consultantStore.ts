import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { Consultant, TenantSummary } from '@/types';
import { api } from '@/lib/api';

interface ConsultantState {
  consultant: Consultant | null;
  assignedTenants: TenantSummary[];
  activeTenantId: string | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;

  setAuth: (
    consultant: Consultant,
    accessToken: string,
    refreshToken: string,
    assignedTenants: TenantSummary[]
  ) => void;
  clearAuth: () => void;
  refreshAuth: () => Promise<boolean>;
  setActiveTenant: (tenantId: string | null) => void;
  updateAssignedTenants: (tenants: TenantSummary[]) => void;
}

export const useConsultantStore = create<ConsultantState>()(
  persist(
    (set, get) => ({
      consultant: null,
      assignedTenants: [],
      activeTenantId: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      setAuth: (consultant, accessToken, refreshToken, assignedTenants) => {
        api.setAccessToken(accessToken);
        set({
          consultant,
          accessToken,
          refreshToken,
          assignedTenants,
          isAuthenticated: true,
        });
      },

      clearAuth: () => {
        api.setAccessToken(null);
        api.setActiveTenantSlug(null);
        set({
          consultant: null,
          assignedTenants: [],
          activeTenantId: null,
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
        });
      },

      refreshAuth: async () => {
        const { refreshToken } = get();
        if (!refreshToken) return false;

        try {
          const response = await api.consultantRefresh(refreshToken);
          api.setAccessToken(response.accessToken);
          set({
            accessToken: response.accessToken,
            refreshToken: response.refreshToken,
          });
          return true;
        } catch {
          get().clearAuth();
          return false;
        }
      },

      setActiveTenant: (tenantId) => {
        const { assignedTenants } = get();
        const tenant = assignedTenants.find((t) => t.id === tenantId);
        api.setActiveTenantSlug(tenant?.slug ?? null);
        set({ activeTenantId: tenantId });
      },

      updateAssignedTenants: (tenants) => {
        set({ assignedTenants: tenants });
      },
    }),
    {
      name: 'consultant-auth-storage',
      partialize: (state) => ({
        refreshToken: state.refreshToken,
        activeTenantId: state.activeTenantId,
      }),
    }
  )
);
