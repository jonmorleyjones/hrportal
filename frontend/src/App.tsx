import { BrowserRouter, Routes, Route, useLocation } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { TenantProvider } from '@/providers/TenantProvider';
import { AuthProvider } from '@/providers/AuthProvider';
import { AppShell } from '@/components/layout/AppShell';
import { HrAppShell } from '@/components/layout/HrAppShell';
import { LoginPage } from '@/features/auth/LoginPage';
import { HrConsultantLoginPage } from '@/features/auth/HrConsultantLoginPage';
import { DashboardPage } from '@/features/dashboard/DashboardPage';
import { UsersPage } from '@/features/users/UsersPage';
import { SettingsPage } from '@/features/settings/SettingsPage';
import { BillingPage } from '@/features/billing/BillingPage';
import { RequestsPage } from '@/features/requests/RequestsPage';
import { RequestFormPage } from '@/features/requests/RequestFormPage';
import { CompletedRequestsPage } from '@/features/requests/CompletedRequestsPage';
import { HrDashboardPage } from '@/features/hr/HrDashboardPage';
import { HrResponsesPage } from '@/features/hr/HrResponsesPage';
import { HrRequestTypesPage } from '@/features/hr/HrRequestTypesPage';
import { HrRequestTypeDetailPage } from '@/features/hr/HrRequestTypeDetailPage';
import { HrResponseDetailPage } from '@/features/hr/HrResponseDetailPage';

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

function HrRoutes() {
  return (
    <Routes>
      <Route path="/hr/login" element={<HrConsultantLoginPage />} />
      <Route element={<HrAppShell />}>
        <Route path="/hr" element={<HrDashboardPage />} />
        <Route path="/hr/responses" element={<HrResponsesPage />} />
        <Route path="/hr/responses/:responseId" element={<HrResponseDetailPage />} />
        <Route path="/hr/request-types" element={<HrRequestTypesPage />} />
        <Route path="/hr/request-types/:requestTypeId" element={<HrRequestTypeDetailPage />} />
        <Route path="/hr/settings" element={<HrSettingsPlaceholder />} />
        <Route path="/hr/branding" element={<HrBrandingPlaceholder />} />
      </Route>
    </Routes>
  );
}

// Placeholder components for future implementation
function HrSettingsPlaceholder() {
  return (
    <div className="text-center py-12">
      <h2 className="text-2xl font-bold gradient-text mb-2">Tenant Settings</h2>
      <p className="text-muted-foreground">Configure settings for this tenant.</p>
    </div>
  );
}

function HrBrandingPlaceholder() {
  return (
    <div className="text-center py-12">
      <h2 className="text-2xl font-bold gradient-text mb-2">Branding</h2>
      <p className="text-muted-foreground">Customize the look and feel for this tenant.</p>
    </div>
  );
}

function AppRouter() {
  const location = useLocation();
  const isHrRoute = location.pathname.startsWith('/hr');

  if (isHrRoute) {
    return <HrRoutes />;
  }

  return <TenantRoutes />;
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AppRouter />
      </BrowserRouter>
    </QueryClientProvider>
  );
}
