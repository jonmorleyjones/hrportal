import { useNavigate } from 'react-router-dom';
import { useHrConsultantAuthStore } from '@/stores/hrConsultantAuthStore';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { motion } from '@/components/ui/motion';
import { LogOut, User, Bell, Search, Command, Building2 } from 'lucide-react';

export function HrHeader() {
  const navigate = useNavigate();
  const { hrConsultant, selectedTenant, clearAuth } = useHrConsultantAuthStore();

  const handleLogout = () => {
    clearAuth();
    navigate('/hr/login');
  };

  const initials = hrConsultant?.name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2) || 'HR';

  return (
    <motion.header
      initial={{ y: -20, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 0.5, ease: [0.4, 0, 0.2, 1] }}
      className="fixed top-4 right-4 left-72 z-30 h-14 ml-4"
    >
      <div className="h-full glass rounded-xl px-4 flex items-center justify-between">
        {/* Search bar */}
        <div className="flex items-center gap-2 flex-1 max-w-md">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <input
              type="text"
              placeholder="Search responses, tenants..."
              className="w-full h-9 pl-9 pr-12 rounded-lg bg-background/50 border border-border/50 text-sm placeholder:text-muted-foreground focus:outline-none focus:border-primary/50 focus:ring-1 focus:ring-primary/20 transition-all"
            />
            <div className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center gap-1 text-xs text-muted-foreground">
              <kbd className="px-1.5 py-0.5 rounded bg-muted text-[10px] font-mono">
                <Command className="inline h-2.5 w-2.5" />
              </kbd>
              <kbd className="px-1.5 py-0.5 rounded bg-muted text-[10px] font-mono">K</kbd>
            </div>
          </div>
        </div>

        {/* Center - Current tenant indicator */}
        {selectedTenant && (
          <div className="hidden md:flex items-center gap-2 px-3 py-1.5 rounded-lg bg-primary/10 border border-primary/20">
            <Building2 className="h-3.5 w-3.5 text-primary" />
            <span className="text-sm font-medium text-primary">{selectedTenant.tenantName}</span>
          </div>
        )}

        {/* Right side */}
        <div className="flex items-center gap-2">
          {/* Notifications */}
          <motion.div whileHover={{ scale: 1.05 }} whileTap={{ scale: 0.95 }}>
            <Button variant="ghost" size="icon" className="relative h-9 w-9 rounded-lg hover:bg-white/5">
              <Bell className="h-4 w-4" />
              <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-accent rounded-full" />
            </Button>
          </motion.div>

          {/* User menu */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                <Button
                  variant="ghost"
                  className="relative h-9 px-2 rounded-lg hover:bg-white/5 flex items-center gap-2"
                >
                  <Avatar className="h-7 w-7">
                    <AvatarFallback className="bg-gradient-to-br from-accent to-primary text-white text-xs font-medium">
                      {initials}
                    </AvatarFallback>
                  </Avatar>
                  <span className="text-sm font-medium hidden sm:block">{hrConsultant?.name?.split(' ')[0]}</span>
                </Button>
              </motion.div>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-56 glass-strong border-border/50">
              <DropdownMenuLabel>
                <div className="flex flex-col space-y-1">
                  <p className="text-sm font-medium">{hrConsultant?.name}</p>
                  <p className="text-xs text-muted-foreground">{hrConsultant?.email}</p>
                  <p className="text-xs text-accent">HR Consultant</p>
                </div>
              </DropdownMenuLabel>
              <DropdownMenuSeparator className="bg-border/50" />
              <DropdownMenuItem className="cursor-pointer focus:bg-white/5">
                <User className="mr-2 h-4 w-4" />
                Profile
              </DropdownMenuItem>
              <DropdownMenuSeparator className="bg-border/50" />
              <DropdownMenuItem
                onClick={handleLogout}
                className="cursor-pointer text-destructive focus:bg-destructive/10 focus:text-destructive"
              >
                <LogOut className="mr-2 h-4 w-4" />
                Log out
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
    </motion.header>
  );
}
