import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { GradientBackground, PageTransition } from '@/components/ui/motion';

export function AppShell() {
  return (
    <div className="min-h-screen relative">
      <GradientBackground />
      <Sidebar />
      <Header />
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
