import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { api } from '@/lib/api';
import type { CrossTenantRequest } from '@/types';
import { useConsultantStore } from '@/stores/consultantStore';
import { PageHeader } from '@/components/shared/PageHeader';
import { StatCard } from '@/components/shared/StatCard';
import { motion, Skeleton, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import { Building2, ClipboardList, Clock, CheckCircle2, Users, Activity } from 'lucide-react';
import { formatDateTime } from '@/lib/utils';

export function ConsultantDashboardPage() {
  const navigate = useNavigate();
  const { assignedTenants, activeTenantId } = useConsultantStore();

  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ['consultant-dashboard-stats', activeTenantId],
    queryFn: () => api.getConsultantDashboardStats(),
  });

  const { data: requests, isLoading: requestsLoading } = useQuery({
    queryKey: ['consultant-recent-requests', activeTenantId],
    queryFn: () => api.getConsultantCrossTenantRequests(),
  });

  const recentRequests = { requests: requests?.slice(0, 5) || [] };

  return (
    <div>
      <PageHeader
        title="Dashboard"
        description="Overview of all your assigned tenants"
      />

      {/* Stats grid */}
      <StaggerContainer className="grid gap-4 md:grid-cols-2 lg:grid-cols-4 mb-8">
        <StaggerItem>
          <StatCard
            title="Total Tenants"
            value={statsLoading ? '-' : stats?.totalTenants || assignedTenants.length}
            description="Assigned to you"
            icon={Building2}
          />
        </StaggerItem>
        <StaggerItem>
          <StatCard
            title="Active Request Types"
            value={statsLoading ? '-' : stats?.totalRequestTypes || 0}
            description="Across all tenants"
            icon={ClipboardList}
          />
        </StaggerItem>
        <StaggerItem>
          <StatCard
            title="Pending Responses"
            value={statsLoading ? '-' : stats?.pendingResponses || 0}
            description="Awaiting review"
            icon={Clock}
            trend={stats?.pendingResponses && stats.pendingResponses > 0 ? 'up' : 'neutral'}
            trendValue={stats?.pendingResponses && stats.pendingResponses > 0 ? 'Needs attention' : ''}
          />
        </StaggerItem>
        <StaggerItem>
          <StatCard
            title="Completed This Week"
            value={statsLoading ? '-' : stats?.completedResponsesThisWeek || 0}
            description="Responses processed"
            icon={CheckCircle2}
            trend="up"
            trendValue="+12%"
          />
        </StaggerItem>
      </StaggerContainer>

      {/* Main content grid */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Tenant Overview */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="glass rounded-xl p-6"
        >
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-2">
              <Building2 className="h-5 w-5 text-amber-500" />
              <h3 className="text-lg font-semibold">Your Tenants</h3>
            </div>
            <button
              onClick={() => navigate('/consultant/tenants')}
              className="text-sm text-amber-500 hover:text-amber-400 transition-colors"
            >
              View all
            </button>
          </div>

          {assignedTenants.length === 0 ? (
            <div className="text-center py-8">
              <Building2 className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
              <p className="text-muted-foreground">No tenants assigned yet</p>
            </div>
          ) : (
            <div className="space-y-3">
              {assignedTenants.slice(0, 5).map((tenant, index) => (
                <motion.div
                  key={tenant.id}
                  initial={{ opacity: 0, x: -10 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: 0.1 * index }}
                  onClick={() => navigate(`/consultant/tenants/${tenant.id}`)}
                  className="flex items-center gap-3 p-3 rounded-lg bg-background/50 border border-border/50 hover:border-amber-500/50 cursor-pointer transition-all group"
                >
                  <div className="flex items-center justify-center w-10 h-10 rounded-lg bg-gradient-to-br from-amber-500/10 to-orange-500/10 text-amber-500 group-hover:from-amber-500/20 group-hover:to-orange-500/20 transition-colors">
                    <Building2 className="h-5 w-5" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="font-medium truncate group-hover:text-amber-500 transition-colors">
                      {tenant.name}
                    </p>
                    <p className="text-xs text-muted-foreground">{tenant.slug}.portal.com</p>
                  </div>
                  <div className="text-right">
                    <div className="flex items-center gap-1 text-sm">
                      <Users className="h-3 w-3 text-muted-foreground" />
                      <span className="text-muted-foreground">{tenant.userCount}</span>
                    </div>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      {tenant.pendingResponses} pending
                    </p>
                  </div>
                </motion.div>
              ))}
            </div>
          )}
        </motion.div>

        {/* Recent Activity */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="glass rounded-xl p-6"
        >
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-2">
              <Activity className="h-5 w-5 text-amber-500" />
              <h3 className="text-lg font-semibold">Recent Requests</h3>
            </div>
            <button
              onClick={() => navigate('/consultant/requests')}
              className="text-sm text-amber-500 hover:text-amber-400 transition-colors"
            >
              View all
            </button>
          </div>

          {requestsLoading ? (
            <div className="space-y-4">
              {[...Array(5)].map((_, i) => (
                <div key={i} className="flex items-start gap-3">
                  <Skeleton className="w-2 h-2 mt-2 rounded-full" />
                  <div className="flex-1 space-y-2">
                    <Skeleton className="h-4 w-3/4" />
                    <Skeleton className="h-3 w-1/2" />
                  </div>
                </div>
              ))}
            </div>
          ) : recentRequests.requests.length === 0 ? (
            <div className="text-center py-8">
              <Activity className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
              <p className="text-muted-foreground">No recent requests</p>
            </div>
          ) : (
            <div className="space-y-4">
              {recentRequests.requests.map((request: CrossTenantRequest, index: number) => (
                <motion.div
                  key={request.id}
                  initial={{ opacity: 0, x: -10 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: 0.1 * index }}
                  className="flex items-start gap-3 text-sm group"
                >
                  <div className="relative">
                    <div className="w-2 h-2 mt-2 rounded-full bg-gradient-to-r from-amber-500 to-orange-500" />
                    {index < recentRequests.requests.length - 1 && (
                      <div className="absolute top-4 left-[3px] w-0.5 h-full bg-border/50" />
                    )}
                  </div>
                  <div className="flex-1 min-w-0 pb-4">
                    <p className="font-medium truncate group-hover:text-amber-500 transition-colors">
                      {request.requestTypeName}
                    </p>
                    <p className="text-muted-foreground text-xs">
                      {request.tenantName} • {request.userName}
                    </p>
                    <p className="text-muted-foreground text-xs mt-0.5">
                      {formatDateTime(request.startedAt)}
                    </p>
                  </div>
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                    request.isComplete
                      ? 'bg-green-500/10 text-green-500'
                      : 'bg-yellow-500/10 text-yellow-500'
                  }`}>
                    {request.isComplete ? 'Complete' : 'In Progress'}
                  </span>
                </motion.div>
              ))}
            </div>
          )}
        </motion.div>
      </div>
    </div>
  );
}
