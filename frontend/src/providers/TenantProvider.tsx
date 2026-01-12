import { useEffect, type ReactNode } from 'react';
import { useTenantStore } from '@/stores/tenantStore';

interface TenantProviderProps {
  children: ReactNode;
}

export function TenantProvider({ children }: TenantProviderProps) {
  const { tenant, isLoading, error, resolveTenant } = useTenantStore();

  useEffect(() => {
    resolveTenant();
  }, [resolveTenant]);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Loading...</p>
        </div>
      </div>
    );
  }

  if (error || !tenant) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <div className="text-center max-w-md px-4">
          <h1 className="text-2xl font-bold text-destructive mb-2">Tenant Not Found</h1>
          <p className="text-muted-foreground">
            {error || 'Please access the portal through a valid tenant subdomain.'}
          </p>
          <p className="text-sm text-muted-foreground mt-4">
            Example: <code className="bg-muted px-2 py-1 rounded">acme.localhost:5173</code>
          </p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
