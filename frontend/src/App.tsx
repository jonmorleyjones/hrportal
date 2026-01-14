import { BrowserRouter, Routes, Route, useLocation } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { TenantProvider } from '@/providers/TenantProvider';
import { AuthProvider } from '@/providers/AuthProvider';
import { AppShell } from '@/components/layout/AppShell';
import { LoginPage } from '@/features/auth/LoginPage';
import { HrConsultantLoginPage } from '@/features/auth/HrConsultantLoginPage';
import { DashboardPage } from '@/features/dashboard/DashboardPage';
import { UsersPage } from '@/features/users/UsersPage';
import { SettingsPage } from '@/features/settings/SettingsPage';
import { BillingPage } from '@/features/billing/BillingPage';
import { RequestsPage } from '@/features/requests/RequestsPage';
import { RequestFormPage } from '@/features/requests/RequestFormPage';
import { CompletedRequestsPage } from '@/features/requests/CompletedRequestsPage';

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
    </Routes>
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
