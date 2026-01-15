import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { useHrConsultantAuthStore } from '@/stores/hrConsultantAuthStore';
import { api, type HrRequestTypeDto } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { DataTable, type Column } from '@/components/ui/data-table';
import { motion } from '@/components/ui/motion';
import {
  ClipboardList,
  CheckCircle2,
  XCircle,
  AlertCircle,
  FileText,
  ChevronRight
} from 'lucide-react';

export function HrRequestTypesPage() {
  const { selectedTenant } = useHrConsultantAuthStore();

  const { data: requestTypes, isLoading } = useQuery({
    queryKey: ['hr-tenant-request-types', selectedTenant?.tenantId],
    queryFn: () => api.getHrTenantRequestTypes(selectedTenant!.tenantId),
    enabled: !!selectedTenant,
  });

  const columns: Column<HrRequestTypeDto>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (item) => (
        <Link
          to={`/hr/request-types/${item.id}`}
          className="flex items-center gap-2 group hover:text-primary transition-colors"
        >
          <span className="text-lg">{item.icon || '📋'}</span>
          <div className="flex-1">
            <p className="font-medium group-hover:underline">{item.name}</p>
            {item.description && (
              <p className="text-xs text-muted-foreground line-clamp-1">{item.description}</p>
            )}
          </div>
          <ChevronRight className="h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors" />
        </Link>
      ),
    },
    {
      key: 'isActive',
      header: 'Status',
      sortable: true,
      render: (item) => (
        <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${
          item.isActive
            ? 'bg-green-500/10 text-green-400'
            : 'bg-red-500/10 text-red-400'
        }`}>
          {item.isActive ? <CheckCircle2 className="h-3 w-3" /> : <XCircle className="h-3 w-3" />}
          {item.isActive ? 'Active' : 'Inactive'}
        </span>
      ),
    },
    {
      key: 'currentVersionNumber',
      header: 'Version',
      sortable: true,
      render: (item) => (
        <span className="text-sm text-muted-foreground">v{item.currentVersionNumber}</span>
      ),
    },
    {
      key: 'totalResponses',
      header: 'Responses',
      sortable: true,
      render: (item) => (
        <div className="text-sm">
          <span className="font-medium">{item.totalResponses}</span>
          <span className="text-muted-foreground"> total</span>
          <span className="text-green-400 ml-2">({item.completedResponses} complete)</span>
        </div>
      ),
    },
    {
      key: 'createdAt',
      header: 'Created',
      sortable: true,
      getValue: (item) => new Date(item.createdAt).getTime(),
      render: (item) => (
        <span className="text-sm text-muted-foreground">
          {new Date(item.createdAt).toLocaleDateString()}
        </span>
      ),
    },
    {
      key: 'updatedAt',
      header: 'Updated',
      sortable: true,
      getValue: (item) => new Date(item.updatedAt).getTime(),
      render: (item) => (
        <span className="text-sm text-muted-foreground">
          {new Date(item.updatedAt).toLocaleDateString()}
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
          Please select a tenant from the sidebar to view request types.
        </p>
      </div>
    );
  }

  return (
    <div>
      <PageHeader
        title="Request Types"
        description={`Form templates for ${selectedTenant.tenantName}`}
      />

      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="glass rounded-xl p-6"
      >
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-2">
            <ClipboardList className="h-5 w-5 text-primary" />
            <h3 className="text-lg font-semibold">All Request Types</h3>
          </div>
          {requestTypes && (
            <span className="text-sm text-muted-foreground">
              {requestTypes.length} request types
            </span>
          )}
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin h-8 w-8 border-2 border-primary border-t-transparent rounded-full mx-auto mb-2" />
            <p className="text-muted-foreground">Loading request types...</p>
          </div>
        ) : requestTypes?.length === 0 ? (
          <div className="text-center py-8">
            <FileText className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
            <p className="text-muted-foreground">No request types configured</p>
          </div>
        ) : (
          <DataTable
            data={requestTypes || []}
            columns={columns}
            keyField="id"
            searchPlaceholder="Search request types..."
            filterOptions={{
              key: 'isActive',
              label: 'Status',
              options: [
                { label: 'Active', value: 'true' },
                { label: 'Inactive', value: 'false' },
              ],
            }}
            pagination
            defaultPageSize={25}
            pageSizeOptions={[10, 25, 50]}
          />
        )}
      </motion.div>
    </div>
  );
}
