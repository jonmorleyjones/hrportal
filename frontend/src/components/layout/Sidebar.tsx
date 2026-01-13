import { NavLink } from 'react-router-dom';
import { useTenantStore } from '@/stores/tenantStore';
import { cn } from '@/lib/utils';
import { motion } from '@/components/ui/motion';
import {
  LayoutDashboard,
  Users,
  Settings,
  CreditCard,
  Building2,
  Sparkles,
  ClipboardList,
} from 'lucide-react';

const navItems = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/onboarding', label: 'Onboarding', icon: ClipboardList },
  { to: '/users', label: 'Users', icon: Users },
  { to: '/settings', label: 'Settings', icon: Settings },
  { to: '/settings/billing', label: 'Billing', icon: CreditCard },
];

export function Sidebar() {
  const { tenant } = useTenantStore();

  return (
    <motion.aside
      initial={{ x: -20, opacity: 0 }}
      animate={{ x: 0, opacity: 1 }}
      transition={{ duration: 0.5, ease: [0.4, 0, 0.2, 1] }}
      className="fixed left-4 top-4 bottom-4 z-40 w-64 glass-strong rounded-2xl overflow-hidden"
    >
      {/* Gradient accent line */}
      <div className="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-primary via-accent to-primary" />

      {/* Logo section */}
      <div className="flex items-center gap-3 p-6 border-b border-border/30">
        <motion.div
          whileHover={{ scale: 1.05, rotate: 5 }}
          className="flex items-center justify-center w-10 h-10 rounded-xl bg-gradient-to-br from-primary to-accent"
        >
          <Building2 className="h-5 w-5 text-white" />
        </motion.div>
        <div className="flex-1 min-w-0">
          <h2 className="font-semibold truncate">{tenant?.name || 'Portal'}</h2>
          <p className="text-xs text-muted-foreground truncate">{tenant?.slug}.portal.com</p>
        </div>
      </div>

      {/* Navigation */}
      <nav className="p-4 space-y-1">
        {navItems.map((item, index) => (
          <motion.div
            key={item.to}
            initial={{ x: -20, opacity: 0 }}
            animate={{ x: 0, opacity: 1 }}
            transition={{ delay: 0.1 + index * 0.05 }}
          >
            <NavLink
              to={item.to}
              end={item.to === '/'}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-medium transition-all duration-300 group relative overflow-hidden',
                  isActive
                    ? 'bg-gradient-to-r from-primary/20 to-accent/20 text-foreground'
                    : 'text-muted-foreground hover:text-foreground hover:bg-white/5'
                )
              }
            >
              {({ isActive }) => (
                <>
                  {isActive && (
                    <motion.div
                      layoutId="activeNav"
                      className="absolute left-0 top-0 bottom-0 w-1 bg-gradient-to-b from-primary to-accent rounded-full"
                      transition={{ type: 'spring', stiffness: 300, damping: 30 }}
                    />
                  )}
                  <item.icon className={cn(
                    'h-5 w-5 transition-colors',
                    isActive ? 'text-primary' : 'group-hover:text-primary'
                  )} />
                  <span>{item.label}</span>
                  {isActive && (
                    <motion.div
                      initial={{ scale: 0 }}
                      animate={{ scale: 1 }}
                      className="ml-auto"
                    >
                      <Sparkles className="h-3 w-3 text-accent" />
                    </motion.div>
                  )}
                </>
              )}
            </NavLink>
          </motion.div>
        ))}
      </nav>

      {/* Subscription badge */}
      <motion.div
        initial={{ y: 20, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        transition={{ delay: 0.4 }}
        className="absolute bottom-4 left-4 right-4"
      >
        <div className="rounded-xl bg-gradient-to-r from-primary/10 to-accent/10 border border-primary/20 p-4">
          <div className="flex items-center justify-between mb-2">
            <p className="text-xs text-muted-foreground uppercase tracking-wider">Plan</p>
            <motion.div
              animate={{ scale: [1, 1.1, 1] }}
              transition={{ duration: 2, repeat: Infinity }}
              className="w-2 h-2 rounded-full bg-accent"
            />
          </div>
          <p className="font-semibold gradient-text capitalize">
            {tenant?.subscriptionTier || 'Free'}
          </p>
          <div className="mt-2 h-1 rounded-full bg-background/50 overflow-hidden">
            <motion.div
              initial={{ width: 0 }}
              animate={{ width: '60%' }}
              transition={{ delay: 0.6, duration: 1 }}
              className="h-full bg-gradient-to-r from-primary to-accent rounded-full"
            />
          </div>
        </div>
      </motion.div>
    </motion.aside>
  );
}
