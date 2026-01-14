import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useHrConsultantAuthStore } from '@/stores/hrConsultantAuthStore';
import { api, type HrRequestTypeVersionDto, type HrUpdateRequestTypeRequest } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { motion } from '@/components/ui/motion';
import {
  ArrowLeft,
  CheckCircle2,
  XCircle,
  AlertCircle,
  Clock,
  FileText,
  ChevronDown,
  ChevronRight,
  History,
  Pencil,
  Save,
  X
} from 'lucide-react';

const ICON_OPTIONS = [
  { value: 'clipboard-list', label: 'Clipboard List' },
  { value: 'file-text', label: 'File Text' },
  { value: 'user', label: 'User' },
  { value: 'briefcase', label: 'Briefcase' },
  { value: 'calendar', label: 'Calendar' },
  { value: 'heart', label: 'Heart' },
  { value: 'home', label: 'Home' },
  { value: 'car', label: 'Car' },
  { value: 'plane', label: 'Plane' },
  { value: 'graduation-cap', label: 'Graduation Cap' },
  { value: 'stethoscope', label: 'Stethoscope' },
  { value: 'shield', label: 'Shield' },
];

export function HrRequestTypeDetailPage() {
  const { requestTypeId } = useParams<{ requestTypeId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { selectedTenant } = useHrConsultantAuthStore();
  const [expandedVersions, setExpandedVersions] = useState<Set<string>>(new Set());
  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState<HrUpdateRequestTypeRequest>({
    name: '',
    description: null,
    icon: null,
    formJson: '',
    isActive: true,
  });
  const [jsonError, setJsonError] = useState<string | null>(null);

  const { data: requestType, isLoading } = useQuery({
    queryKey: ['hr-request-type-detail', selectedTenant?.tenantId, requestTypeId],
    queryFn: () => api.getHrTenantRequestTypeDetail(selectedTenant!.tenantId, requestTypeId!),
    enabled: !!selectedTenant && !!requestTypeId,
  });

  const updateMutation = useMutation({
    mutationFn: (data: HrUpdateRequestTypeRequest) =>
      api.updateHrTenantRequestType(selectedTenant!.tenantId, requestTypeId!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['hr-request-type-detail', selectedTenant?.tenantId, requestTypeId] });
      queryClient.invalidateQueries({ queryKey: ['hr-tenant-request-types', selectedTenant?.tenantId] });
      setIsEditing(false);
    },
  });

  useEffect(() => {
    if (requestType && !isEditing) {
      const currentVersion = requestType.versions.find(v => v.versionNumber === requestType.currentVersionNumber);
      setEditForm({
        name: requestType.name,
        description: requestType.description,
        icon: requestType.icon,
        formJson: currentVersion?.formJson || '',
        isActive: requestType.isActive,
      });
    }
  }, [requestType, isEditing]);

  const toggleVersion = (versionId: string) => {
    setExpandedVersions(prev => {
      const next = new Set(prev);
      if (next.has(versionId)) {
        next.delete(versionId);
      } else {
        next.add(versionId);
      }
      return next;
    });
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

  const handleSave = () => {
    if (!validateJson(editForm.formJson)) {
      return;
    }
    updateMutation.mutate(editForm);
  };

  const handleCancel = () => {
    setIsEditing(false);
    setJsonError(null);
    if (requestType) {
      const currentVersion = requestType.versions.find(v => v.versionNumber === requestType.currentVersionNumber);
      setEditForm({
        name: requestType.name,
        description: requestType.description,
        icon: requestType.icon,
        formJson: currentVersion?.formJson || '',
        isActive: requestType.isActive,
      });
    }
  };

  const canEdit = selectedTenant?.canManageRequestTypes ?? false;

  if (!selectedTenant) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
        <h2 className="text-xl font-semibold mb-2">No Tenant Selected</h2>
        <p className="text-muted-foreground">
          Please select a tenant from the sidebar.
        </p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="text-center py-12">
        <div className="animate-spin h-8 w-8 border-2 border-primary border-t-transparent rounded-full mx-auto mb-2" />
        <p className="text-muted-foreground">Loading request type...</p>
      </div>
    );
  }

  if (!requestType) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
        <h2 className="text-xl font-semibold mb-2">Request Type Not Found</h2>
        <p className="text-muted-foreground">
          The requested request type could not be found.
        </p>
      </div>
    );
  }

  return (
    <div>
      <button
        onClick={() => navigate('/hr/request-types')}
        className="flex items-center gap-2 text-muted-foreground hover:text-foreground mb-4 transition-colors"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Request Types
      </button>

      <div className="flex items-start justify-between mb-6">
        <PageHeader
          title={
            <div className="flex items-center gap-3">
              <span className="text-2xl">{requestType.icon || '📋'}</span>
              <span>{requestType.name}</span>
              <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${
                requestType.isActive
                  ? 'bg-green-500/10 text-green-400'
                  : 'bg-red-500/10 text-red-400'
              }`}>
                {requestType.isActive ? <CheckCircle2 className="h-3 w-3" /> : <XCircle className="h-3 w-3" />}
                {requestType.isActive ? 'Active' : 'Inactive'}
              </span>
            </div>
          }
          description={requestType.description || 'No description'}
        />
        {canEdit && !isEditing && (
          <button
            onClick={() => setIsEditing(true)}
            className="flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-colors"
          >
            <Pencil className="h-4 w-4" />
            Edit
          </button>
        )}
      </div>

      {/* Edit Form */}
      {isEditing && (
        <motion.div
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass rounded-xl p-6 mb-6"
        >
          <h3 className="text-lg font-semibold mb-4">Edit Request Type</h3>
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-1">Name</label>
                <input
                  type="text"
                  value={editForm.name}
                  onChange={(e) => setEditForm({ ...editForm, name: e.target.value })}
                  className="w-full px-3 py-2 bg-white/5 border border-border/30 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Icon</label>
                <select
                  value={editForm.icon || ''}
                  onChange={(e) => setEditForm({ ...editForm, icon: e.target.value || null })}
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
                value={editForm.description || ''}
                onChange={(e) => setEditForm({ ...editForm, description: e.target.value || null })}
                className="w-full px-3 py-2 bg-white/5 border border-border/30 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary"
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="isActive"
                checked={editForm.isActive}
                onChange={(e) => setEditForm({ ...editForm, isActive: e.target.checked })}
                className="w-4 h-4"
              />
              <label htmlFor="isActive" className="text-sm font-medium">Active</label>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">
                Form JSON (SurveyJS Schema)
                {jsonError && <span className="text-red-400 ml-2">{jsonError}</span>}
              </label>
              <textarea
                value={editForm.formJson}
                onChange={(e) => {
                  setEditForm({ ...editForm, formJson: e.target.value });
                  if (jsonError) validateJson(e.target.value);
                }}
                rows={15}
                className={`w-full px-3 py-2 bg-white/5 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary font-mono text-sm ${
                  jsonError ? 'border-red-500' : 'border-border/30'
                }`}
              />
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={handleSave}
                disabled={updateMutation.isPending}
                className="flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-colors disabled:opacity-50"
              >
                <Save className="h-4 w-4" />
                {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
              </button>
              <button
                onClick={handleCancel}
                disabled={updateMutation.isPending}
                className="flex items-center gap-2 px-4 py-2 bg-white/10 rounded-lg hover:bg-white/20 transition-colors"
              >
                <X className="h-4 w-4" />
                Cancel
              </button>
              {updateMutation.isError && (
                <span className="text-red-400 text-sm">
                  Error: {updateMutation.error instanceof Error ? updateMutation.error.message : 'Failed to save'}
                </span>
              )}
            </div>
          </div>
        </motion.div>
      )}

      {/* Metadata */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="glass rounded-xl p-6 mb-6"
      >
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div>
            <p className="text-sm text-muted-foreground">Current Version</p>
            <p className="text-lg font-semibold">v{requestType.currentVersionNumber}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Total Versions</p>
            <p className="text-lg font-semibold">{requestType.versions.length}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Created</p>
            <p className="text-lg font-semibold">{new Date(requestType.createdAt).toLocaleDateString()}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Last Updated</p>
            <p className="text-lg font-semibold">{new Date(requestType.updatedAt).toLocaleDateString()}</p>
          </div>
        </div>
      </motion.div>

      {/* Version History */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.1 }}
        className="glass rounded-xl p-6"
      >
        <div className="flex items-center gap-2 mb-6">
          <History className="h-5 w-5 text-primary" />
          <h3 className="text-lg font-semibold">Version History</h3>
        </div>

        <div className="space-y-3">
          {requestType.versions.map((version) => (
            <VersionCard
              key={version.id}
              version={version}
              isCurrentVersion={version.versionNumber === requestType.currentVersionNumber}
              isExpanded={expandedVersions.has(version.id)}
              onToggle={() => toggleVersion(version.id)}
            />
          ))}
        </div>
      </motion.div>
    </div>
  );
}

function VersionCard({
  version,
  isCurrentVersion,
  isExpanded,
  onToggle
}: {
  version: HrRequestTypeVersionDto;
  isCurrentVersion: boolean;
  isExpanded: boolean;
  onToggle: () => void;
}) {
  let formSchema: { title?: string; description?: string; pages?: Array<{ elements?: unknown[] }> } | null = null;
  try {
    formSchema = JSON.parse(version.formJson);
  } catch {
    // Invalid JSON
  }

  const questionCount = formSchema?.pages?.reduce((count, page) => {
    return count + (page.elements?.length || 0);
  }, 0) || 0;

  return (
    <div className={`rounded-lg border transition-colors ${
      isCurrentVersion
        ? 'border-primary/30 bg-primary/5'
        : 'border-border/30 bg-white/5'
    }`}>
      <button
        onClick={onToggle}
        className="w-full flex items-center justify-between p-4 text-left"
      >
        <div className="flex items-center gap-3">
          {isExpanded ? (
            <ChevronDown className="h-4 w-4 text-muted-foreground" />
          ) : (
            <ChevronRight className="h-4 w-4 text-muted-foreground" />
          )}
          <div>
            <div className="flex items-center gap-2">
              <span className="font-medium">Version {version.versionNumber}</span>
              {isCurrentVersion && (
                <span className="px-2 py-0.5 bg-primary/20 text-primary text-xs rounded-full">
                  Current
                </span>
              )}
            </div>
            <p className="text-sm text-muted-foreground">
              Created {new Date(version.createdAt).toLocaleDateString()}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-4 text-sm text-muted-foreground">
          <span className="flex items-center gap-1">
            <FileText className="h-4 w-4" />
            {questionCount} questions
          </span>
          <span className="flex items-center gap-1">
            <Clock className="h-4 w-4" />
            {version.responseCount} responses
          </span>
        </div>
      </button>

      {isExpanded && (
        <div className="px-4 pb-4 pt-0">
          <div className="border-t border-border/30 pt-4">
            {formSchema ? (
              <div className="space-y-4">
                {formSchema.title && (
                  <div>
                    <p className="text-sm text-muted-foreground">Form Title</p>
                    <p className="font-medium">{formSchema.title}</p>
                  </div>
                )}
                {formSchema.description && (
                  <div>
                    <p className="text-sm text-muted-foreground">Form Description</p>
                    <p>{formSchema.description}</p>
                  </div>
                )}
                <div>
                  <p className="text-sm text-muted-foreground mb-2">Form Schema (JSON)</p>
                  <pre className="bg-black/20 rounded-lg p-4 text-xs overflow-x-auto max-h-96">
                    {JSON.stringify(formSchema, null, 2)}
                  </pre>
                </div>
              </div>
            ) : (
              <p className="text-muted-foreground">Invalid form schema</p>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
