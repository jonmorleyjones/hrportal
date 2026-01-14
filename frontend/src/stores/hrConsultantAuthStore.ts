import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { HrConsultant, AssignedTenant } from '@/types';
import { api } from '@/lib/api';

interface HrConsultantAuthState {
  hrConsultant: HrConsultant | null;
  assignedTenants: AssignedTenant[];
  selectedTenant: AssignedTenant | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  setAuth: (hrConsultant: HrConsultant, assignedTenants: AssignedTenant[], accessToken: string, refreshToken: string) => void;
  selectTenant: (tenant: AssignedTenant) => void;
  clearAuth: () => void;
  refreshAuth: () => Promise<boolean>;
}

export const useHrConsultantAuthStore = create<HrConsultantAuthState>()(
  persist(
    (set, get) => ({
      hrConsultant: null,
      assignedTenants: [],
      selectedTenant: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      setAuth: (hrConsultant, assignedTenants, accessToken, refreshToken) => {
        api.setAccessToken(accessToken);
        const selectedTenant = assignedTenants.length > 0 ? assignedTenants[0] : null;
        if (selectedTenant) {
          api.setTenantSlug(selectedTenant.tenantSlug);
        }
        set({
          hrConsultant,
          assignedTenants,
          selectedTenant,
          accessToken,
          refreshToken,
          isAuthenticated: true,
        });
      },

      selectTenant: (tenant) => {
        api.setTenantSlug(tenant.tenantSlug);
        set({ selectedTenant: tenant });
      },

      clearAuth: () => {
        api.setAccessToken(null);
        set({
          hrConsultant: null,
          assignedTenants: [],
          selectedTenant: null,
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
        });
      },

      refreshAuth: async () => {
        const { refreshToken } = get();
        if (!refreshToken) return false;

        try {
          const response = await api.refresh(refreshToken);
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
    }),
    {
      name: 'hr-consultant-auth-storage',
      partialize: (state) => ({
        refreshToken: state.refreshToken,
        hrConsultant: state.hrConsultant,
        assignedTenants: state.assignedTenants,
        selectedTenant: state.selectedTenant,
      }),
    }
  )
);
