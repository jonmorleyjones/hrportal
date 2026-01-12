import { useEffect, type ReactNode } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';
import { api } from '@/lib/api';

interface AuthProviderProps {
  children: ReactNode;
}

const publicPaths = ['/login', '/forgot-password', '/reset-password', '/accept-invitation'];

export function AuthProvider({ children }: AuthProviderProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const { isAuthenticated, accessToken, refreshAuth } = useAuthStore();

  useEffect(() => {
    // Initialize API with token from store
    if (accessToken) {
      api.setAccessToken(accessToken);
    }
  }, [accessToken]);

  useEffect(() => {
    const isPublicPath = publicPaths.some(path => location.pathname.startsWith(path));

    if (!isAuthenticated && !isPublicPath) {
      // Try to refresh token before redirecting
      refreshAuth().then(success => {
        if (!success) {
          navigate('/login', { replace: true, state: { from: location } });
        }
      });
    }
  }, [isAuthenticated, location, navigate, refreshAuth]);

  return <>{children}</>;
}
