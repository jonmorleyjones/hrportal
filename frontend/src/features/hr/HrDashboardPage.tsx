import { useQuery } from '@tanstack/react-query';
import { useHrConsultantAuthStore } from '@/stores/hrConsultantAuthStore';
import { api, type HrResponseDto } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { StatCard } from '@/components/shared/StatCard';
import { motion, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import { DataTable, type Column } from '@/components/ui/data-table';
import {
  Building2,
  Users,
  ClipboardList,
  FileCheck,
  TrendingUp,
  Activity,
  CheckCircle2,
  Clock,
  FileText
} from 'lucide-react';

export function HrDashboardPage() {
  const { hrConsultant, assignedTenants, selectedTenant } = useHrConsultantAuthStore();

  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ['hr-dashboard-stats'],
    queryFn: () => api.getHrDashboardStats(),
  });

  // Fetch tenant-specific stats when a tenant is selected (for future use)
  useQuery({
    queryKey: ['hr-tenant-stats', selectedTenant?.tenantId],
    queryFn: () => api.getHrTenantStats(selectedTenant!.tenantId),
    enabled: !!selectedTenant,
  });

  // Fetch responses for the selected tenant
  const { data: responsesData, isLoading: responsesLoading } = useQuery({
    queryKey: ['hr-tenant-responses', selectedTenant?.tenantId],
    queryFn: () => api.getHrTenantResponses(selectedTenant!.tenantId, 1, 50),
    enabled: !!selectedTenant && selectedTenant.canViewResponses,
  });

  const responseColumns: Column<HrResponseDto>[] = [
    {
      key: 'requestTypeName',
      header: 'Request Type',
      sortable: true,
      render: (item) => (
        <div className="flex items-center gap-2">
          <span className="text-lg">{item.requestTypeIcon || '📋'}</span>
          <span className="font-medium">{item.requestTypeName}</span>
        </div>
      ),
    },
    {
      key: 'userName',
      header: 'User',
      sortable: true,
      render: (item) => (
        <div>
          <p className="font-medium">{item.userName}</p>
          <p className="text-xs text-muted-foreground">{item.userEmail}</p>
        </div>
      ),
    },
    {
      key: 'isComplete',
      header: 'Status',
      sortable: true,
      render: (item) => (
        <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${
          item.isComplete
            ? 'bg-green-500/10 text-green-400'
            : 'bg-yellow-500/10 text-yellow-400'
        }`}>
          {item.isComplete ? <CheckCircle2 className="h-3 w-3" /> : <Clock className="h-3 w-3" />}
          {item.isComplete ? 'Complete' : 'In Progress'}
        </span>
      ),
    },
    {
      key: 'startedAt',
      header: 'Started',
      sortable: true,
      getValue: (item) => new Date(item.startedAt).getTime(),
      render: (item) => (
        <span className="text-sm text-muted-foreground">
          {new Date(item.startedAt).toLocaleDateString()}
        </span>
      ),
    },
    {
      key: 'completedAt',
      header: 'Completed',
      sortable: true,
      getValue: (item) => item.completedAt ? new Date(item.completedAt).getTime() : 0,
      render: (item) => (
        <span className="text-sm text-muted-foreground">
          {item.completedAt ? new Date(item.completedAt).toLocaleDateString() : '-'}
        </span>
      ),
    },
  ];

  return (
    <div>
      <PageHeader
        title={`Welcome back, ${hrConsultant?.name?.split(' ')[0] || 'Consultant'}`}
        description={selectedTenant
          ? `Managing ${selectedTenant.tenantName}`
          : 'Select a tenant to get started'}
      />

      {/* Stats grid */}
      <StaggerContainer className="grid gap-4 md:grid-cols-2 lg:grid-cols-4 mb-8">
        <StaggerItem>
          <StatCard
            title="Assigned Tenants"
            value={statsLoading ? '-' : stats?.totalTenants ?? assignedTenants.length}
            description="Organizations you manage"
            icon={Building2}
          />
        </StaggerItem>
        <StaggerItem>
          <StatCard
            title="Total Responses"
            value={statsLoading ? '-' : stats?.totalResponses ?? 0}
            description="Across all tenants"
            icon={ClipboardList}
            trend="up"
            trendValue="+8%"
          />
        </StaggerItem>
        <StaggerItem>
          <StatCard
            title="Pending Review"
            value={statsLoading ? '-' : stats?.pendingReview ?? 0}
            description="Awaiting your review"
            icon={Clock}
          />
        </StaggerItem>
        <StaggerItem>
          <StatCard
            title="Completion Rate"
            value={statsLoading ? '-' : `${stats?.completionRate ?? 0}%`}
            description="Form completion"
            icon={TrendingUp}
            trend="up"
            trendValue="+3%"
          />
        </StaggerItem>
      </StaggerContainer>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Assigned Tenants Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="glass rounded-xl p-6"
        >
          <div className="flex items-center gap-2 mb-6">
            <Building2 className="h-5 w-5 text-primary" />
            <h3 className="text-lg font-semibold">Your Tenants</h3>
          </div>

          {assignedTenants.length === 0 ? (
            <div className="text-center py-8">
              <Building2 className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
              <p className="text-muted-foreground">No tenants assigned</p>
            </div>
          ) : (
            <div className="space-y-3">
              {assignedTenants.map((tenant, index) => (
                <motion.div
                  key={tenant.tenantId}
                  initial={{ opacity: 0, x: -10 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: 0.1 * index }}
                  className={`flex items-center justify-between p-3 rounded-lg transition-colors ${
                    selectedTenant?.tenantId === tenant.tenantId
                      ? 'bg-primary/10 border border-primary/20'
                      : 'bg-white/5 hover:bg-white/10'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <div className="p-2 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20">
                      <Building2 className="h-4 w-4 text-primary" />
                    </div>
                    <div>
                      <p className="font-medium">{tenant.tenantName}</p>
                      <p className="text-xs text-muted-foreground">{tenant.tenantSlug}.portal.com</p>
                    </div>
                  </div>
                  {selectedTenant?.tenantId === tenant.tenantId && (
                    <CheckCircle2 className="h-4 w-4 text-primary" />
                  )}
                </motion.div>
              ))}
            </div>
          )}
        </motion.div>

        {/* Quick Actions Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="glass rounded-xl p-6"
        >
          <div className="flex items-center gap-2 mb-6">
            <Activity className="h-5 w-5 text-primary" />
            <h3 className="text-lg font-semibold">Quick Actions</h3>
          </div>

          {!selectedTenant ? (
            <div className="text-center py-8">
              <Activity className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
              <p className="text-muted-foreground">Select a tenant to see available actions</p>
            </div>
          ) : (
            <div className="space-y-3">
              {selectedTenant.canViewResponses && (
                <QuickActionItem
                  icon={FileCheck}
                  title="View Responses"
                  description="Review submitted form responses"
                />
              )}
              {selectedTenant.canManageRequestTypes && (
                <QuickActionItem
                  icon={ClipboardList}
                  title="Manage Request Types"
                  description="Create and edit form templates"
                />
              )}
              {selectedTenant.canManageSettings && (
                <QuickActionItem
                  icon={Users}
                  title="Manage Users"
                  description="View and manage tenant users"
                />
              )}
            </div>
          )}
        </motion.div>
      </div>

      {/* Permissions Overview */}
      {selectedTenant && (
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
          className="glass rounded-xl p-6 mt-6"
        >
          <h3 className="text-lg font-semibold mb-4">Your Permissions for {selectedTenant.tenantName}</h3>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <PermissionBadge
              label="View Responses"
              enabled={selectedTenant.canViewResponses}
            />
            <PermissionBadge
              label="Manage Request Types"
              enabled={selectedTenant.canManageRequestTypes}
            />
            <PermissionBadge
              label="Manage Settings"
              enabled={selectedTenant.canManageSettings}
            />
            <PermissionBadge
              label="Manage Branding"
              enabled={selectedTenant.canManageBranding}
            />
          </div>
        </motion.div>
      )}

      {/* Responses Table */}
      {selectedTenant && selectedTenant.canViewResponses && (
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.6 }}
          className="glass rounded-xl p-6 mt-6"
        >
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-2">
              <FileText className="h-5 w-5 text-primary" />
              <h3 className="text-lg font-semibold">Responses for {selectedTenant.tenantName}</h3>
            </div>
            {responsesData && (
              <span className="text-sm text-muted-foreground">
                {responsesData.totalCount} total responses
              </span>
            )}
          </div>

          {responsesLoading ? (
            <div className="text-center py-8">
              <div className="animate-spin h-8 w-8 border-2 border-primary border-t-transparent rounded-full mx-auto mb-2" />
              <p className="text-muted-foreground">Loading responses...</p>
            </div>
          ) : responsesData?.responses.length === 0 ? (
            <div className="text-center py-8">
              <FileText className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
              <p className="text-muted-foreground">No responses yet</p>
            </div>
          ) : (
            <DataTable
              data={responsesData?.responses || []}
              columns={responseColumns}
              keyField="id"
              searchPlaceholder="Search by user or request type..."
              filterOptions={{
                key: 'isComplete',
                label: 'Status',
                options: [
                  { label: 'Complete', value: 'true' },
                  { label: 'In Progress', value: 'false' },
                ],
              }}
            />
          )}
        </motion.div>
      )}
    </div>
  );
}

function QuickActionItem({
  icon: Icon,
  title,
  description
}: {
  icon: typeof FileCheck;
  title: string;
  description: string;
}) {
  return (
    <motion.div
      whileHover={{ x: 4 }}
      className="flex items-center gap-3 p-3 rounded-lg bg-white/5 hover:bg-white/10 cursor-pointer transition-colors group"
    >
      <div className="p-2 rounded-lg bg-primary/10 group-hover:bg-primary/20 transition-colors">
        <Icon className="h-4 w-4 text-primary" />
      </div>
      <div>
        <p className="font-medium group-hover:text-primary transition-colors">{title}</p>
        <p className="text-xs text-muted-foreground">{description}</p>
      </div>
    </motion.div>
  );
}

function PermissionBadge({ label, enabled }: { label: string; enabled: boolean }) {
  return (
    <div className={`flex items-center gap-2 p-3 rounded-lg ${
      enabled ? 'bg-green-500/10 border border-green-500/20' : 'bg-muted/20 border border-border/20'
    }`}>
      <div className={`w-2 h-2 rounded-full ${enabled ? 'bg-green-500' : 'bg-muted-foreground'}`} />
      <span className={`text-sm ${enabled ? 'text-green-400' : 'text-muted-foreground'}`}>
        {label}
      </span>
    </div>
  );
}
