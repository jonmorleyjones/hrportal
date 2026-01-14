import { NavLink } from 'react-router-dom';
import { useHrConsultantAuthStore } from '@/stores/hrConsultantAuthStore';
import { cn } from '@/lib/utils';
import { motion } from '@/components/ui/motion';
import {
  LayoutDashboard,
  ClipboardList,
  FileCheck,
  Settings,
  Building2,
  Sparkles,
  ChevronDown,
  Check,
  Palette,
  Users,
} from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Button } from '@/components/ui/button';

export function HrSidebar() {
  const { hrConsultant, assignedTenants, selectedTenant, selectTenant } = useHrConsultantAuthStore();

  const navItems = [
    { to: '/hr', label: 'Dashboard', icon: LayoutDashboard, end: true },
    ...(selectedTenant?.canViewResponses ? [
      { to: '/hr/responses', label: 'Responses', icon: FileCheck }
    ] : []),
    ...(selectedTenant?.canManageRequestTypes ? [
      { to: '/hr/request-types', label: 'Request Types', icon: ClipboardList }
    ] : []),
    ...(selectedTenant?.canManageSettings ? [
      { to: '/hr/settings', label: 'Tenant Settings', icon: Settings }
    ] : []),
    ...(selectedTenant?.canManageBranding ? [
      { to: '/hr/branding', label: 'Branding', icon: Palette }
    ] : []),
  ];

  return (
    <motion.aside
      initial={{ x: -20, opacity: 0 }}
      animate={{ x: 0, opacity: 1 }}
      transition={{ duration: 0.5, ease: [0.4, 0, 0.2, 1] }}
      className="fixed left-4 top-4 bottom-4 z-40 w-64 glass-strong rounded-2xl overflow-hidden flex flex-col"
    >
      {/* Gradient accent line */}
      <div className="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-accent via-primary to-accent" />

      {/* Logo section */}
      <div className="flex items-center gap-3 p-6 border-b border-border/30">
        <motion.div
          whileHover={{ scale: 1.05, rotate: 5 }}
          className="flex items-center justify-center w-10 h-10 rounded-xl bg-gradient-to-br from-accent to-primary"
        >
          <Users className="h-5 w-5 text-white" />
        </motion.div>
        <div className="flex-1 min-w-0">
          <h2 className="font-semibold truncate">HR Portal</h2>
          <p className="text-xs text-muted-foreground truncate">Consultant Dashboard</p>
        </div>
      </div>

      {/* Tenant Selector */}
      <div className="p-4 border-b border-border/30">
        <p className="text-xs text-muted-foreground uppercase tracking-wider mb-2">Current Tenant</p>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="ghost"
              className="w-full justify-between h-auto py-3 px-3 bg-white/5 hover:bg-white/10 rounded-lg"
            >
              <div className="flex items-center gap-2 min-w-0">
                <div className="p-1.5 rounded-md bg-primary/10">
                  <Building2 className="h-3.5 w-3.5 text-primary" />
                </div>
                <div className="text-left min-w-0">
                  <p className="text-sm font-medium truncate">
                    {selectedTenant?.tenantName || 'Select Tenant'}
                  </p>
                  {selectedTenant && (
                    <p className="text-xs text-muted-foreground truncate">
                      {selectedTenant.tenantSlug}
                    </p>
                  )}
                </div>
              </div>
              <ChevronDown className="h-4 w-4 text-muted-foreground shrink-0" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="start" className="w-56 glass-strong border-border/50">
            <DropdownMenuLabel>Switch Tenant</DropdownMenuLabel>
            <DropdownMenuSeparator className="bg-border/50" />
            {assignedTenants.map((tenant) => (
              <DropdownMenuItem
                key={tenant.tenantId}
                onClick={() => selectTenant(tenant)}
                className="cursor-pointer focus:bg-white/5"
              >
                <div className="flex items-center justify-between w-full">
                  <div className="flex items-center gap-2">
                    <Building2 className="h-4 w-4 text-muted-foreground" />
                    <div>
                      <p className="text-sm">{tenant.tenantName}</p>
                      <p className="text-xs text-muted-foreground">{tenant.tenantSlug}</p>
                    </div>
                  </div>
                  {selectedTenant?.tenantId === tenant.tenantId && (
                    <Check className="h-4 w-4 text-primary" />
                  )}
                </div>
              </DropdownMenuItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      {/* Navigation */}
      <nav className="p-4 space-y-1 flex-1">
        {navItems.map((item, index) => (
          <motion.div
            key={item.to}
            initial={{ x: -20, opacity: 0 }}
            animate={{ x: 0, opacity: 1 }}
            transition={{ delay: 0.1 + index * 0.05 }}
          >
            <NavLink
              to={item.to}
              end={item.end}
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
                      layoutId="hrActiveNav"
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

      {/* Consultant info badge */}
      <motion.div
        initial={{ y: 20, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        transition={{ delay: 0.4 }}
        className="p-4"
      >
        <div className="rounded-xl bg-gradient-to-r from-accent/10 to-primary/10 border border-accent/20 p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-gradient-to-br from-accent/20 to-primary/20">
              <Users className="h-4 w-4 text-accent" />
            </div>
            <div className="min-w-0">
              <p className="text-sm font-medium truncate">{hrConsultant?.name}</p>
              <p className="text-xs text-muted-foreground truncate">{hrConsultant?.email}</p>
            </div>
          </div>
          <div className="mt-3 flex items-center gap-2 text-xs text-muted-foreground">
            <span className="w-2 h-2 rounded-full bg-green-500" />
            <span>{assignedTenants.length} tenant{assignedTenants.length !== 1 ? 's' : ''} assigned</span>
          </div>
        </div>
      </motion.div>
    </motion.aside>
  );
}
