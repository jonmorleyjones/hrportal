import { useEffect, type ReactNode } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useConsultantStore } from '@/stores/consultantStore';
import { api } from '@/lib/api';

interface ConsultantProviderProps {
  children: ReactNode;
}

const publicPaths = ['/consultant/login'];

export function ConsultantProvider({ children }: ConsultantProviderProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const { isAuthenticated, accessToken, activeTenantId, assignedTenants, refreshAuth } = useConsultantStore();

  useEffect(() => {
    // Initialize API with token from store
    if (accessToken) {
      api.setAccessToken(accessToken);
    }
  }, [accessToken]);

  useEffect(() => {
    // Set active tenant slug for API requests
    if (activeTenantId && assignedTenants.length > 0) {
      const tenant = assignedTenants.find(t => t.id === activeTenantId);
      api.setActiveTenantSlug(tenant?.slug ?? null);
    }
  }, [activeTenantId, assignedTenants]);

  useEffect(() => {
    const isPublicPath = publicPaths.some(path => location.pathname.startsWith(path));

    if (!isAuthenticated && !isPublicPath) {
      // Try to refresh token before redirecting
      refreshAuth().then(success => {
        if (!success) {
          navigate('/consultant/login', { replace: true, state: { from: location } });
        }
      });
    }
  }, [isAuthenticated, location, navigate, refreshAuth]);

  return <>{children}</>;
}
