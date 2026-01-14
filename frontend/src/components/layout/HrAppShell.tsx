import { Outlet, Navigate } from 'react-router-dom';
import { useHrConsultantAuthStore } from '@/stores/hrConsultantAuthStore';
import { HrSidebar } from './HrSidebar';
import { HrHeader } from './HrHeader';
import { GradientBackground, PageTransition } from '@/components/ui/motion';

export function HrAppShell() {
  const { isAuthenticated } = useHrConsultantAuthStore();

  if (!isAuthenticated) {
    return <Navigate to="/hr/login" replace />;
  }

  return (
    <div className="min-h-screen relative">
      <GradientBackground />
      <HrSidebar />
      <HrHeader />
      <main className="ml-72 pt-20 relative z-10">
        <div className="p-6">
          <PageTransition>
            <Outlet />
          </PageTransition>
        </div>
      </main>
    </div>
  );
}
