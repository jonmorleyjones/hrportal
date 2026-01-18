import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { useHrConsultantAuthStore } from '@/stores/hrConsultantAuthStore';
import { api, type HrResponseDto } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { DataTable, type Column } from '@/components/ui/data-table';
import { motion } from '@/components/ui/motion';
import {
  FileText,
  CheckCircle2,
  Clock,
  AlertCircle,
  ChevronRight
} from 'lucide-react';
import { DynamicIcon } from '@/lib/icons';

export function HrResponsesPage() {
  const { selectedTenant } = useHrConsultantAuthStore();

  const { data: responsesData, isLoading } = useQuery({
    queryKey: ['hr-tenant-responses', selectedTenant?.tenantId],
    queryFn: () => api.getHrTenantResponses(selectedTenant!.tenantId, 1, 100),
    enabled: !!selectedTenant && selectedTenant.canViewResponses,
  });

  const responseColumns: Column<HrResponseDto>[] = [
    {
      key: 'requestTypeName',
      header: 'Request Type',
      sortable: true,
      render: (item) => (
        <Link
          to={`/hr/responses/${item.id}`}
          className="flex items-center gap-2 group hover:text-primary transition-colors"
        >
          <div className="p-1.5 rounded-md bg-primary/10">
            <DynamicIcon name={item.requestTypeIcon} className="h-4 w-4 text-primary" />
          </div>
          <span className="font-medium group-hover:underline">{item.requestTypeName}</span>
          <ChevronRight className="h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors" />
        </Link>
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
      key: 'versionNumber',
      header: 'Version',
      sortable: true,
      render: (item) => (
        <span className="text-sm text-muted-foreground">v{item.versionNumber}</span>
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

  if (!selectedTenant) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
        <h2 className="text-xl font-semibold mb-2">No Tenant Selected</h2>
        <p className="text-muted-foreground">
          Please select a tenant from the sidebar to view responses.
        </p>
      </div>
    );
  }

  if (!selectedTenant.canViewResponses) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
        <h2 className="text-xl font-semibold mb-2">Access Denied</h2>
        <p className="text-muted-foreground">
          You don't have permission to view responses for this tenant.
        </p>
      </div>
    );
  }

  return (
    <div>
      <PageHeader
        title="Responses"
        description={`Form responses for ${selectedTenant.tenantName}`}
      />

      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="glass rounded-xl p-6"
      >
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-2">
            <FileText className="h-5 w-5 text-primary" />
            <h3 className="text-lg font-semibold">All Responses</h3>
          </div>
          {responsesData && (
            <span className="text-sm text-muted-foreground">
              {responsesData.totalCount} total responses
            </span>
          )}
        </div>

        {isLoading ? (
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
            pagination
            defaultPageSize={25}
            pageSizeOptions={[10, 25, 50, 100]}
          />
        )}
      </motion.div>
    </div>
  );
}
