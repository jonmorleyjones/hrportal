import { NavLink } from 'react-router-dom';
import { useConsultantStore } from '@/stores/consultantStore';
import { cn } from '@/lib/utils';
import { motion } from '@/components/ui/motion';
import {
  LayoutDashboard,
  Building2,
  ClipboardList,
  Sparkles,
  UserCog,
  ChevronDown,
} from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

const navItems = [
  { to: '/consultant', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/consultant/tenants', label: 'Tenants', icon: Building2 },
  { to: '/consultant/requests', label: 'All Requests', icon: ClipboardList },
];

export function ConsultantSidebar() {
  const { consultant, assignedTenants, activeTenantId, setActiveTenant } = useConsultantStore();
  const activeTenant = assignedTenants.find(t => t.id === activeTenantId);

  return (
    <motion.aside
      initial={{ x: -20, opacity: 0 }}
      animate={{ x: 0, opacity: 1 }}
      transition={{ duration: 0.5, ease: [0.4, 0, 0.2, 1] }}
      className="fixed left-4 top-4 bottom-4 z-40 w-64 glass-strong rounded-2xl overflow-hidden"
    >
      {/* Gradient accent line - different color for consultant */}
      <div className="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-amber-500 via-orange-500 to-amber-500" />

      {/* Logo section */}
      <div className="flex items-center gap-3 p-6 border-b border-border/30">
        <motion.div
          whileHover={{ scale: 1.05, rotate: 5 }}
          className="flex items-center justify-center w-10 h-10 rounded-xl bg-gradient-to-br from-amber-500 to-orange-500"
        >
          <UserCog className="h-5 w-5 text-white" />
        </motion.div>
        <div className="flex-1 min-w-0">
          <h2 className="font-semibold truncate">HR Consultant</h2>
          <p className="text-xs text-muted-foreground truncate">{consultant?.name || 'Portal'}</p>
        </div>
      </div>

      {/* Tenant selector */}
      {assignedTenants.length > 0 && (
        <div className="px-4 py-3 border-b border-border/30">
          <p className="text-xs text-muted-foreground uppercase tracking-wider mb-2">Active Tenant</p>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button className="w-full flex items-center justify-between px-3 py-2 rounded-lg bg-background/50 border border-border/50 hover:border-amber-500/50 transition-colors text-sm">
                <span className="truncate">{activeTenant?.name || 'Select tenant'}</span>
                <ChevronDown className="h-4 w-4 text-muted-foreground flex-shrink-0" />
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="start" className="w-56 glass-strong border-border/50">
              <DropdownMenuItem
                onClick={() => setActiveTenant(null)}
                className={cn(
                  "cursor-pointer focus:bg-white/5",
                  !activeTenantId && "bg-amber-500/10"
                )}
              >
                All Tenants
              </DropdownMenuItem>
              {assignedTenants.map(tenant => (
                <DropdownMenuItem
                  key={tenant.id}
                  onClick={() => setActiveTenant(tenant.id)}
                  className={cn(
                    "cursor-pointer focus:bg-white/5",
                    activeTenantId === tenant.id && "bg-amber-500/10"
                  )}
                >
                  {tenant.name}
                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      )}

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
              end={item.to === '/consultant'}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-medium transition-all duration-300 group relative overflow-hidden',
                  isActive
                    ? 'bg-gradient-to-r from-amber-500/20 to-orange-500/20 text-foreground'
                    : 'text-muted-foreground hover:text-foreground hover:bg-white/5'
                )
              }
            >
              {({ isActive }) => (
                <>
                  {isActive && (
                    <motion.div
                      layoutId="activeConsultantNav"
                      className="absolute left-0 top-0 bottom-0 w-1 bg-gradient-to-b from-amber-500 to-orange-500 rounded-full"
                      transition={{ type: 'spring', stiffness: 300, damping: 30 }}
                    />
                  )}
                  <item.icon className={cn(
                    'h-5 w-5 transition-colors',
                    isActive ? 'text-amber-500' : 'group-hover:text-amber-500'
                  )} />
                  <span>{item.label}</span>
                  {isActive && (
                    <motion.div
                      initial={{ scale: 0 }}
                      animate={{ scale: 1 }}
                      className="ml-auto"
                    >
                      <Sparkles className="h-3 w-3 text-orange-500" />
                    </motion.div>
                  )}
                </>
              )}
            </NavLink>
          </motion.div>
        ))}
      </nav>

      {/* Consultant badge */}
      <motion.div
        initial={{ y: 20, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        transition={{ delay: 0.4 }}
        className="absolute bottom-4 left-4 right-4"
      >
        <div className="rounded-xl bg-gradient-to-r from-amber-500/10 to-orange-500/10 border border-amber-500/20 p-4">
          <div className="flex items-center justify-between mb-2">
            <p className="text-xs text-muted-foreground uppercase tracking-wider">Role</p>
            <motion.div
              animate={{ scale: [1, 1.1, 1] }}
              transition={{ duration: 2, repeat: Infinity }}
              className="w-2 h-2 rounded-full bg-amber-500"
            />
          </div>
          <p className="font-semibold text-amber-500">
            HR Consultant
          </p>
          <p className="text-xs text-muted-foreground mt-1">
            {assignedTenants.length} tenant{assignedTenants.length !== 1 ? 's' : ''} assigned
          </p>
        </div>
      </motion.div>
    </motion.aside>
  );
}
