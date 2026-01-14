import { Outlet } from 'react-router-dom';
import { ConsultantSidebar } from './ConsultantSidebar';
import { ConsultantHeader } from './ConsultantHeader';
import { GradientBackground, PageTransition } from '@/components/ui/motion';

export function ConsultantAppShell() {
  return (
    <div className="min-h-screen relative">
      <GradientBackground />
      <ConsultantSidebar />
      <ConsultantHeader />
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
