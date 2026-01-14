import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { api } from '@/lib/api';
import type { TenantSummary } from '@/types';
import { PageHeader } from '@/components/shared/PageHeader';
import { motion, Skeleton, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import { Building2, Users, ClipboardList, Clock, Settings, Palette, Eye, ChevronRight } from 'lucide-react';

export function ConsultantTenantsPage() {
  const navigate = useNavigate();

  const { data: tenants, isLoading } = useQuery({
    queryKey: ['consultant-tenants'],
    queryFn: () => api.getConsultantTenants(),
  });

  return (
    <div>
      <PageHeader
        title="Tenants"
        description="Manage all your assigned tenant organizations"
      />

      {isLoading ? (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {[...Array(6)].map((_, i) => (
            <Skeleton key={i} className="h-64 rounded-xl" />
          ))}
        </div>
      ) : tenants?.length === 0 ? (
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          className="text-center py-16"
        >
          <Building2 className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-semibold mb-2">No Tenants Assigned</h3>
          <p className="text-muted-foreground">You don't have any tenants assigned to you yet.</p>
        </motion.div>
      ) : (
        <StaggerContainer className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {tenants?.map((tenant: TenantSummary) => (
            <StaggerItem key={tenant.id}>
              <motion.div
                whileHover={{ y: -4, scale: 1.02 }}
                transition={{ duration: 0.2 }}
                className="glass rounded-xl p-6 relative overflow-hidden group cursor-pointer"
                onClick={() => navigate(`/consultant/tenants/${tenant.id}`)}
              >
                {/* Gradient accent on hover */}
                <div className="absolute inset-0 bg-gradient-to-br from-amber-500/5 to-orange-500/5 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

                <div className="relative z-10">
                  {/* Header */}
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex items-center gap-3">
                      <div className="flex items-center justify-center w-12 h-12 rounded-xl bg-gradient-to-br from-amber-500/10 to-orange-500/10 text-amber-500 group-hover:from-amber-500/20 group-hover:to-orange-500/20 transition-colors">
                        <Building2 className="h-6 w-6" />
                      </div>
                      <div>
                        <h3 className="font-semibold group-hover:text-amber-500 transition-colors">
                          {tenant.name}
                        </h3>
                        <p className="text-sm text-muted-foreground">{tenant.slug}.portal.com</p>
                      </div>
                    </div>
                    <ChevronRight className="h-5 w-5 text-muted-foreground group-hover:text-amber-500 group-hover:translate-x-1 transition-all" />
                  </div>

                  {/* Subscription tier badge */}
                  <div className="mb-4">
                    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium ${
                      tenant.subscriptionTier === 'Enterprise'
                        ? 'bg-purple-500/10 text-purple-400'
                        : tenant.subscriptionTier === 'Pro'
                        ? 'bg-blue-500/10 text-blue-400'
                        : 'bg-gray-500/10 text-gray-400'
                    }`}>
                      {tenant.subscriptionTier}
                    </span>
                  </div>

                  {/* Stats */}
                  <div className="grid grid-cols-3 gap-3 mb-4">
                    <div className="text-center p-2 rounded-lg bg-background/50">
                      <div className="flex items-center justify-center gap-1 text-muted-foreground mb-1">
                        <Users className="h-3 w-3" />
                      </div>
                      <p className="text-lg font-semibold">{tenant.userCount}</p>
                      <p className="text-xs text-muted-foreground">Users</p>
                    </div>
                    <div className="text-center p-2 rounded-lg bg-background/50">
                      <div className="flex items-center justify-center gap-1 text-muted-foreground mb-1">
                        <ClipboardList className="h-3 w-3" />
                      </div>
                      <p className="text-lg font-semibold">{tenant.activeRequestTypes}</p>
                      <p className="text-xs text-muted-foreground">Types</p>
                    </div>
                    <div className="text-center p-2 rounded-lg bg-background/50">
                      <div className="flex items-center justify-center gap-1 text-muted-foreground mb-1">
                        <Clock className="h-3 w-3" />
                      </div>
                      <p className="text-lg font-semibold">{tenant.pendingResponses}</p>
                      <p className="text-xs text-muted-foreground">Pending</p>
                    </div>
                  </div>

                  {/* Permissions */}
                  <div className="flex items-center gap-2 pt-3 border-t border-border/30">
                    <span className="text-xs text-muted-foreground mr-1">Permissions:</span>
                    {tenant.canManageRequestTypes && (
                      <span className="p-1.5 rounded-md bg-amber-500/10 text-amber-500" title="Request Types">
                        <ClipboardList className="h-3 w-3" />
                      </span>
                    )}
                    {tenant.canManageSettings && (
                      <span className="p-1.5 rounded-md bg-blue-500/10 text-blue-500" title="Settings">
                        <Settings className="h-3 w-3" />
                      </span>
                    )}
                    {tenant.canManageBranding && (
                      <span className="p-1.5 rounded-md bg-purple-500/10 text-purple-500" title="Branding">
                        <Palette className="h-3 w-3" />
                      </span>
                    )}
                    {tenant.canViewResponses && (
                      <span className="p-1.5 rounded-md bg-green-500/10 text-green-500" title="View Responses">
                        <Eye className="h-3 w-3" />
                      </span>
                    )}
                  </div>
                </div>

                {/* Decorative corner gradient */}
                <div className="absolute -bottom-4 -right-4 w-24 h-24 bg-gradient-to-tl from-amber-500/10 to-transparent rounded-full blur-xl opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
              </motion.div>
            </StaggerItem>
          ))}
        </StaggerContainer>
      )}
    </div>
  );
}
