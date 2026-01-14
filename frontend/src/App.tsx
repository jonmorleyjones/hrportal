import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { TenantProvider } from '@/providers/TenantProvider';
import { AuthProvider } from '@/providers/AuthProvider';
import { ConsultantProvider } from '@/providers/ConsultantProvider';
import { AppShell } from '@/components/layout/AppShell';
import { ConsultantAppShell } from '@/components/consultant/ConsultantAppShell';
import { LoginPage } from '@/features/auth/LoginPage';
import { DashboardPage } from '@/features/dashboard/DashboardPage';
import { UsersPage } from '@/features/users/UsersPage';
import { SettingsPage } from '@/features/settings/SettingsPage';
import { BillingPage } from '@/features/billing/BillingPage';
import { RequestsPage } from '@/features/requests/RequestsPage';
import { RequestFormPage } from '@/features/requests/RequestFormPage';
import { CompletedRequestsPage } from '@/features/requests/CompletedRequestsPage';
// Consultant pages
import { ConsultantLoginPage } from '@/features/consultant/ConsultantLoginPage';
import { ConsultantDashboardPage } from '@/features/consultant/ConsultantDashboardPage';
import { ConsultantTenantsPage } from '@/features/consultant/ConsultantTenantsPage';
import { ConsultantTenantDetailPage } from '@/features/consultant/ConsultantTenantDetailPage';
import { ConsultantRequestTypesPage } from '@/features/consultant/ConsultantRequestTypesPage';
import { ConsultantRequestsPage } from '@/features/consultant/ConsultantRequestsPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
      retry: 1,
    },
  },
});

function TenantRoutes() {
  return (
    <TenantProvider>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<AppShell />}>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/requests" element={<RequestsPage />} />
            <Route path="/requests/completed" element={<CompletedRequestsPage />} />
            <Route path="/requests/:typeId" element={<RequestFormPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/settings" element={<SettingsPage />} />
            <Route path="/settings/billing" element={<BillingPage />} />
          </Route>
        </Routes>
      </AuthProvider>
    </TenantProvider>
  );
}

function ConsultantRoutes() {
  return (
    <ConsultantProvider>
      <Routes>
        <Route path="/consultant/login" element={<ConsultantLoginPage />} />
        <Route element={<ConsultantAppShell />}>
          <Route path="/consultant" element={<ConsultantDashboardPage />} />
          <Route path="/consultant/tenants" element={<ConsultantTenantsPage />} />
          <Route path="/consultant/tenants/:tenantId" element={<ConsultantTenantDetailPage />} />
          <Route path="/consultant/tenants/:tenantId/request-types" element={<ConsultantRequestTypesPage />} />
          <Route path="/consultant/requests" element={<ConsultantRequestsPage />} />
        </Route>
      </Routes>
    </ConsultantProvider>
  );
}

function AppRoutes() {
  // Check if path starts with /consultant to use consultant routes
  const isConsultantRoute = window.location.pathname.startsWith('/consultant');

  if (isConsultantRoute) {
    return <ConsultantRoutes />;
  }

  return <TenantRoutes />;
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </QueryClientProvider>
  );
}
