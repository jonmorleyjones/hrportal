import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { useHrConsultantAuthStore } from '@/stores/hrConsultantAuthStore';
import { api, type HrRequestTypeDto, type HrCreateRequestTypeRequest } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { DataTable, type Column } from '@/components/ui/data-table';
import { motion } from '@/components/ui/motion';
import {
  ClipboardList,
  CheckCircle2,
  XCircle,
  AlertCircle,
  FileText,
  ChevronRight,
  Plus,
  X,
  Save
} from 'lucide-react';
import { DynamicIcon } from '@/lib/icons';

const ICON_OPTIONS = [
  { value: 'clipboard-list', label: 'Clipboard List' },
  { value: 'file-text', label: 'File Text' },
  { value: 'user-plus', label: 'User Plus' },
  { value: 'briefcase', label: 'Briefcase' },
  { value: 'calendar', label: 'Calendar' },
  { value: 'laptop', label: 'Laptop' },
  { value: 'send', label: 'Send' },
  { value: 'help-circle', label: 'Help Circle' },
  { value: 'settings', label: 'Settings' },
  { value: 'package', label: 'Package' },
  { value: 'credit-card', label: 'Credit Card' },
  { value: 'phone', label: 'Phone' },
];

const DEFAULT_FORM_JSON = JSON.stringify({
  title: "New Request Form",
  description: "Please fill out the form below",
  pages: [
    {
      name: "page1",
      elements: [
        {
          type: "text",
          name: "question1",
          title: "Your first question"
        }
      ]
    }
  ]
}, null, 2);

export function HrRequestTypesPage() {
  const { selectedTenant } = useHrConsultantAuthStore();
  const queryClient = useQueryClient();
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [createForm, setCreateForm] = useState<HrCreateRequestTypeRequest>({
    name: '',
    description: null,
    icon: 'file-text',
    formJson: DEFAULT_FORM_JSON,
  });
  const [jsonError, setJsonError] = useState<string | null>(null);

  const { data: requestTypes, isLoading } = useQuery({
    queryKey: ['hr-tenant-request-types', selectedTenant?.tenantId],
    queryFn: () => api.getHrTenantRequestTypes(selectedTenant!.tenantId),
    enabled: !!selectedTenant,
  });

  const createMutation = useMutation({
    mutationFn: (data: HrCreateRequestTypeRequest) =>
      api.createHrTenantRequestType(selectedTenant!.tenantId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['hr-tenant-request-types', selectedTenant?.tenantId] });
      setIsCreateDialogOpen(false);
      resetCreateForm();
    },
  });

  const resetCreateForm = () => {
    setCreateForm({
      name: '',
      description: null,
      icon: 'file-text',
      formJson: DEFAULT_FORM_JSON,
    });
    setJsonError(null);
  };

  const validateJson = (json: string): boolean => {
    try {
      JSON.parse(json);
      setJsonError(null);
      return true;
    } catch (e) {
      setJsonError(e instanceof Error ? e.message : 'Invalid JSON');
      return false;
    }
  };

  const handleCreate = () => {
    if (!createForm.name.trim()) {
      return;
    }
    if (!validateJson(createForm.formJson)) {
      return;
    }
    createMutation.mutate(createForm);
  };

  const handleCloseDialog = () => {
    setIsCreateDialogOpen(false);
    resetCreateForm();
  };

  const canCreate = selectedTenant?.canManageRequestTypes ?? false;

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
          <div className="p-1.5 rounded-md bg-primary/10">
            <DynamicIcon name={item.icon} className="h-4 w-4 text-primary" />
          </div>
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
          <div className="flex items-center gap-4">
            {requestTypes && (
              <span className="text-sm text-muted-foreground">
                {requestTypes.length} request types
              </span>
            )}
            {canCreate && (
              <button
                onClick={() => setIsCreateDialogOpen(true)}
                className="flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-colors"
              >
                <Plus className="h-4 w-4" />
                Create Request Type
              </button>
            )}
          </div>
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="animate-spin h-8 w-8 border-2 border-primary border-t-transparent rounded-full mx-auto mb-2" />
            <p className="text-muted-foreground">Loading request types...</p>
          </div>
        ) : requestTypes?.length === 0 ? (
          <div className="text-center py-8">
            <FileText className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
            <p className="text-muted-foreground mb-4">No request types configured</p>
            {canCreate && (
              <button
                onClick={() => setIsCreateDialogOpen(true)}
                className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-colors"
              >
                <Plus className="h-4 w-4" />
                Create your first request type
              </button>
            )}
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

      {/* Create Request Type Dialog */}
      {isCreateDialogOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          {/* Backdrop */}
          <div
            className="absolute inset-0 bg-black/60 backdrop-blur-sm"
            onClick={handleCloseDialog}
          />

          {/* Dialog */}
          <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            className="relative z-10 w-full max-w-2xl max-h-[90vh] overflow-y-auto glass rounded-xl p-6 mx-4"
          >
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-semibold">Create Request Type</h2>
              <button
                onClick={handleCloseDialog}
                className="p-2 hover:bg-white/10 rounded-lg transition-colors"
              >
                <X className="h-5 w-5" />
              </button>
            </div>

            <div className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium mb-1">
                    Name <span className="text-red-400">*</span>
                  </label>
                  <input
                    type="text"
                    value={createForm.name}
                    onChange={(e) => setCreateForm({ ...createForm, name: e.target.value })}
                    placeholder="e.g., Leave Request"
                    className="w-full px-3 py-2 bg-white/5 border border-border/30 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Icon</label>
                  <select
                    value={createForm.icon || 'file-text'}
                    onChange={(e) => setCreateForm({ ...createForm, icon: e.target.value })}
                    className="w-full px-3 py-2 bg-white/5 border border-border/30 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary"
                  >
                    {ICON_OPTIONS.map((opt) => (
                      <option key={opt.value} value={opt.value}>
                        {opt.label}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Description</label>
                <input
                  type="text"
                  value={createForm.description || ''}
                  onChange={(e) => setCreateForm({ ...createForm, description: e.target.value || null })}
                  placeholder="Brief description of this request type"
                  className="w-full px-3 py-2 bg-white/5 border border-border/30 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">
                  Form JSON (SurveyJS Schema) <span className="text-red-400">*</span>
                  {jsonError && <span className="text-red-400 ml-2 font-normal">{jsonError}</span>}
                </label>
                <textarea
                  value={createForm.formJson}
                  onChange={(e) => {
                    setCreateForm({ ...createForm, formJson: e.target.value });
                    if (jsonError) validateJson(e.target.value);
                  }}
                  rows={12}
                  className={`w-full px-3 py-2 bg-white/5 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary font-mono text-sm ${
                    jsonError ? 'border-red-500' : 'border-border/30'
                  }`}
                />
                <p className="text-xs text-muted-foreground mt-1">
                  Enter a valid SurveyJS form schema. You can edit this later after creation.
                </p>
              </div>

              <div className="flex items-center justify-end gap-3 pt-4 border-t border-border/30">
                <button
                  onClick={handleCloseDialog}
                  disabled={createMutation.isPending}
                  className="px-4 py-2 text-muted-foreground hover:text-foreground transition-colors"
                >
                  Cancel
                </button>
                <button
                  onClick={handleCreate}
                  disabled={createMutation.isPending || !createForm.name.trim()}
                  className="flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <Save className="h-4 w-4" />
                  {createMutation.isPending ? 'Creating...' : 'Create Request Type'}
                </button>
              </div>

              {createMutation.isError && (
                <p className="text-red-400 text-sm text-center">
                  Error: {createMutation.error instanceof Error ? createMutation.error.message : 'Failed to create request type'}
                </p>
              )}
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
}
