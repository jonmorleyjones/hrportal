import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { motion, Skeleton } from '@/components/ui/motion';
import { DataTable, Column } from '@/components/ui/data-table';
import {
  useRequestTypesAdmin,
  useCreateRequestType,
  useUpdateRequestType,
  useDeleteRequestType,
  useAllRequestResponses,
  useResponseFiles,
} from '../hooks/useRequests';
import {
  Save,
  FileJson,
  Users,
  CheckCircle2,
  Clock,
  AlertCircle,
  ChevronDown,
  ChevronUp,
  Plus,
  Pencil,
  Trash2,
  X,
  ClipboardList,
  Eye,
  Download,
  FileText,
  Image,
  File,
  Paperclip,
} from 'lucide-react';
import { formatDate, formatBytes } from '@/lib/utils';
import type { RequestType, RequestResponse, FileInfo } from '@/types';
import { api } from '@/lib/api';

const defaultFormJson = {
  title: 'Request Form',
  description: 'Please fill out this form.',
  pages: [
    {
      name: 'page1',
      elements: [
        {
          type: 'text',
          name: 'field1',
          title: 'Field 1',
          isRequired: true,
        },
      ],
    },
  ],
  showProgressBar: 'top',
  completeText: 'Submit',
  showQuestionNumbers: 'off',
};

const iconOptions = [
  { value: 'clipboard-list', label: 'Clipboard List' },
  { value: 'user-plus', label: 'User Plus' },
  { value: 'laptop', label: 'Laptop' },
  { value: 'calendar', label: 'Calendar' },
  { value: 'file-text', label: 'File Text' },
  { value: 'send', label: 'Send' },
  { value: 'help-circle', label: 'Help Circle' },
  { value: 'settings', label: 'Settings' },
  { value: 'briefcase', label: 'Briefcase' },
  { value: 'package', label: 'Package' },
  { value: 'credit-card', label: 'Credit Card' },
  { value: 'phone', label: 'Phone' },
];

// Helper to get file icon based on content type
function getFileIcon(contentType: string) {
  if (contentType.startsWith('image/')) {
    return Image;
  }
  if (contentType === 'application/pdf' || contentType.includes('document') || contentType.includes('word')) {
    return FileText;
  }
  return File;
}

// Response Detail Panel Component
function ResponseDetailPanel({
  response,
  onClose,
}: {
  response: RequestResponse;
  onClose: () => void;
}) {
  const { data: files, isLoading: filesLoading } = useResponseFiles(response.id);

  // Parse response JSON
  let responseData: Record<string, unknown> = {};
  try {
    responseData = JSON.parse(response.responseJson);
  } catch {
    // Invalid JSON
  }

  const handleDownload = (file: FileInfo) => {
    const url = api.getFileDownloadUrl(file.id);
    window.open(url, '_blank');
  };

  return (
    <motion.div
      initial={{ opacity: 0, height: 0 }}
      animate={{ opacity: 1, height: 'auto' }}
      exit={{ opacity: 0, height: 0 }}
      className="border-t border-border/30 bg-background/30"
    >
      <div className="p-6">
        <div className="flex items-center justify-between mb-4">
          <h4 className="font-semibold flex items-center gap-2">
            <Eye className="h-4 w-4" />
            Response Details
          </h4>
          <Button variant="ghost" size="sm" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
        </div>

        <div className="grid gap-6 md:grid-cols-2">
          {/* Response Data */}
          <div>
            <h5 className="text-sm font-medium text-muted-foreground mb-3">Form Data</h5>
            <div className="bg-background/50 rounded-lg p-4 space-y-3 max-h-80 overflow-y-auto">
              {Object.entries(responseData).length > 0 ? (
                Object.entries(responseData).map(([key, value]) => (
                  <div key={key} className="border-b border-border/20 pb-2 last:border-0 last:pb-0">
                    <span className="text-xs text-muted-foreground uppercase tracking-wide">
                      {key.replace(/([A-Z])/g, ' $1').trim()}
                    </span>
                    <p className="text-sm mt-0.5">
                      {Array.isArray(value) ? (
                        <span className="text-muted-foreground">
                          {value.map((v) =>
                            typeof v === 'object' ? JSON.stringify(v) : String(v)
                          ).join(', ')}
                        </span>
                      ) : typeof value === 'object' && value !== null ? (
                        <code className="text-xs bg-muted/50 px-1 py-0.5 rounded">
                          {JSON.stringify(value)}
                        </code>
                      ) : (
                        String(value)
                      )}
                    </p>
                  </div>
                ))
              ) : (
                <p className="text-sm text-muted-foreground">No data available</p>
              )}
            </div>
          </div>

          {/* Attached Files */}
          <div>
            <h5 className="text-sm font-medium text-muted-foreground mb-3 flex items-center gap-2">
              <Paperclip className="h-4 w-4" />
              Attached Files
            </h5>
            <div className="bg-background/50 rounded-lg p-4">
              {filesLoading ? (
                <div className="space-y-2">
                  <Skeleton className="h-12 w-full" />
                  <Skeleton className="h-12 w-full" />
                </div>
              ) : files && files.length > 0 ? (
                <div className="space-y-2">
                  {files.map((file) => {
                    const FileIcon = getFileIcon(file.contentType);
                    return (
                      <div
                        key={file.id}
                        className="flex items-center justify-between p-3 bg-muted/30 rounded-lg hover:bg-muted/50 transition-colors"
                      >
                        <div className="flex items-center gap-3 min-w-0">
                          <div className="p-2 rounded-lg bg-primary/10">
                            <FileIcon className="h-4 w-4 text-primary" />
                          </div>
                          <div className="min-w-0">
                            <p className="text-sm font-medium truncate">
                              {file.originalFileName}
                            </p>
                            <p className="text-xs text-muted-foreground">
                              {formatBytes(file.fileSizeBytes)} • {formatDate(file.uploadedAt)}
                            </p>
                          </div>
                        </div>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDownload(file)}
                          className="shrink-0"
                        >
                          <Download className="h-4 w-4" />
                        </Button>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <div className="text-center py-6">
                  <File className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
                  <p className="text-sm text-muted-foreground">No files attached</p>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Metadata */}
        <div className="mt-4 pt-4 border-t border-border/30">
          <div className="flex flex-wrap gap-4 text-xs text-muted-foreground">
            <span>Response ID: {response.id}</span>
            <span>Version: v{response.versionNumber}</span>
            <span>Started: {formatDate(response.startedAt)}</span>
            {response.completedAt && <span>Completed: {formatDate(response.completedAt)}</span>}
          </div>
        </div>
      </div>
    </motion.div>
  );
}

export function RequestAdminPanel() {
  const { data: requestTypes, isLoading: typesLoading } = useRequestTypesAdmin();
  const { data: responses, isLoading: responsesLoading } = useAllRequestResponses();
  const createRequestType = useCreateRequestType();
  const updateRequestType = useUpdateRequestType();
  const deleteRequestType = useDeleteRequestType();

  const [editingType, setEditingType] = useState<RequestType | null>(null);
  const [isCreating, setIsCreating] = useState(false);
  const [showResponses, setShowResponses] = useState(false);
  const [viewingResponse, setViewingResponse] = useState<RequestResponse | null>(null);

  // Form state
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [icon, setIcon] = useState('clipboard-list');
  const [formJson, setFormJson] = useState('');
  const [isActive, setIsActive] = useState(true);
  const [jsonError, setJsonError] = useState<string | null>(null);

  // Reset form
  const resetForm = () => {
    setName('');
    setDescription('');
    setIcon('clipboard-list');
    setFormJson(JSON.stringify(defaultFormJson, null, 2));
    setIsActive(true);
    setJsonError(null);
    setEditingType(null);
    setIsCreating(false);
  };

  // Populate form when editing
  useEffect(() => {
    if (editingType) {
      setName(editingType.name);
      setDescription(editingType.description || '');
      setIcon(editingType.icon);
      try {
        setFormJson(JSON.stringify(JSON.parse(editingType.formJson), null, 2));
      } catch {
        setFormJson(editingType.formJson);
      }
      setIsActive(editingType.isActive);
      setJsonError(null);
    }
  }, [editingType]);

  const handleStartCreate = () => {
    resetForm();
    setIsCreating(true);
    setFormJson(JSON.stringify(defaultFormJson, null, 2));
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

  const handleSave = async () => {
    if (!validateJson(formJson)) return;

    try {
      if (editingType) {
        await updateRequestType.mutateAsync({
          id: editingType.id,
          data: { name, description: description || null, icon, formJson, isActive },
        });
      } else {
        await createRequestType.mutateAsync({
          name,
          description: description || null,
          icon,
          formJson,
        });
      }
      resetForm();
    } catch (err) {
      console.error('Failed to save request type:', err);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this request type?')) return;
    try {
      await deleteRequestType.mutateAsync(id);
    } catch (err) {
      console.error('Failed to delete request type:', err);
    }
  };

  const requestTypeColumns: Column<RequestType>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (item) => (
        <div className="flex items-center gap-3">
          <div className="p-2 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20">
            <ClipboardList className="h-4 w-4 text-primary" />
          </div>
          <div>
            <span className="font-medium">{item.name}</span>
            {item.description && (
              <p className="text-xs text-muted-foreground truncate max-w-[200px]">
                {item.description}
              </p>
            )}
          </div>
        </div>
      ),
    },
    {
      key: 'currentVersionNumber',
      header: 'Version',
      sortable: true,
      render: (item) => (
        <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-muted">
          v{item.currentVersionNumber}
        </span>
      ),
    },
    {
      key: 'isActive',
      header: 'Status',
      sortable: true,
      render: (item) => (
        <span
          className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ${
            item.isActive
              ? 'bg-green-500/10 border border-green-500/20 text-green-400'
              : 'bg-red-500/10 border border-red-500/20 text-red-400'
          }`}
        >
          {item.isActive ? 'Active' : 'Inactive'}
        </span>
      ),
      getValue: (item) => item.isActive ? 'Active' : 'Inactive',
    },
    {
      key: 'updatedAt',
      header: 'Updated',
      sortable: true,
      render: (item) => (
        <span className="text-muted-foreground">{formatDate(item.updatedAt)}</span>
      ),
      getValue: (item) => new Date(item.updatedAt).getTime(),
    },
    {
      key: 'actions',
      header: '',
      render: (item) => (
        <div className="flex items-center gap-2 justify-end">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setEditingType(item)}
            className="h-8 w-8 p-0"
          >
            <Pencil className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleDelete(item.id)}
            className="h-8 w-8 p-0 text-destructive hover:text-destructive"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  const responseColumns: Column<RequestResponse>[] = [
    {
      key: 'requestTypeName',
      header: 'Request Type',
      sortable: true,
      render: (item) => (
        <span className="font-medium">{item.requestTypeName}</span>
      ),
    },
    {
      key: 'userName',
      header: 'Submitted By',
      sortable: true,
      render: (item) => (
        <div className="flex items-center gap-3">
          <div
            className={`p-2 rounded-lg ${
              item.isComplete ? 'bg-green-500/10' : 'bg-yellow-500/10'
            }`}
          >
            {item.isComplete ? (
              <CheckCircle2 className="h-4 w-4 text-green-400" />
            ) : (
              <Clock className="h-4 w-4 text-yellow-400" />
            )}
          </div>
          <span className="font-medium">{item.userName}</span>
        </div>
      ),
    },
    {
      key: 'versionNumber',
      header: 'Version',
      sortable: true,
      render: (item) => (
        <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-muted">
          v{item.versionNumber}
        </span>
      ),
    },
    {
      key: 'startedAt',
      header: 'Submitted',
      sortable: true,
      render: (item) => (
        <span className="text-muted-foreground">{formatDate(item.startedAt)}</span>
      ),
      getValue: (item) => new Date(item.startedAt).getTime(),
    },
    {
      key: 'isComplete',
      header: 'Status',
      sortable: true,
      render: (item) => (
        <span
          className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ${
            item.isComplete
              ? 'bg-green-500/10 border border-green-500/20 text-green-400'
              : 'bg-yellow-500/10 border border-yellow-500/20 text-yellow-400'
          }`}
        >
          {item.isComplete ? 'Completed' : 'In Progress'}
        </span>
      ),
      getValue: (item) => item.isComplete ? 'Completed' : 'In Progress',
    },
    {
      key: 'actions',
      header: '',
      render: (item) => (
        <Button
          variant="ghost"
          size="sm"
          onClick={() => setViewingResponse(viewingResponse?.id === item.id ? null : item)}
          className={`h-8 w-8 p-0 ${viewingResponse?.id === item.id ? 'bg-primary/10' : ''}`}
        >
          <Eye className="h-4 w-4" />
        </Button>
      ),
    },
  ];

  if (typesLoading) {
    return (
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="glass rounded-xl p-6"
      >
        <Skeleton className="h-8 w-48 mb-4" />
        <Skeleton className="h-4 w-full mb-2" />
        <Skeleton className="h-64 w-full" />
      </motion.div>
    );
  }

  const isEditing = editingType || isCreating;

  return (
    <div className="space-y-6">
      {/* Request Types List or Editor */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="glass rounded-xl overflow-hidden"
      >
        {!isEditing ? (
          <>
            <div className="p-6 border-b border-border/30 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20">
                  <ClipboardList className="h-5 w-5 text-primary" />
                </div>
                <div>
                  <h3 className="font-semibold">Request Types</h3>
                  <p className="text-sm text-muted-foreground">
                    {requestTypes?.length || 0} request types configured
                  </p>
                </div>
              </div>
              <Button onClick={handleStartCreate} className="gap-2">
                <Plus className="h-4 w-4" />
                Add Request Type
              </Button>
            </div>
            <div className="p-6">
              <DataTable
                data={requestTypes || []}
                columns={requestTypeColumns}
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
                emptyState={
                  <div className="text-center py-8">
                    <ClipboardList className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
                    <p className="text-muted-foreground">No request types configured</p>
                    <Button onClick={handleStartCreate} className="mt-4 gap-2">
                      <Plus className="h-4 w-4" />
                      Create First Request Type
                    </Button>
                  </div>
                }
              />
            </div>
          </>
        ) : (
          <>
            <div className="p-6 border-b border-border/30 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20">
                  <FileJson className="h-5 w-5 text-primary" />
                </div>
                <div>
                  <h3 className="font-semibold">
                    {editingType ? 'Edit Request Type' : 'Create Request Type'}
                  </h3>
                  <p className="text-sm text-muted-foreground">
                    Configure the form and display settings
                  </p>
                </div>
              </div>
              <Button variant="ghost" size="sm" onClick={resetForm} className="gap-2">
                <X className="h-4 w-4" />
                Cancel
              </Button>
            </div>

            <div className="p-6 space-y-4">
              {/* Name */}
              <div>
                <label className="block text-sm font-medium mb-2">Name</label>
                <input
                  type="text"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  className="w-full bg-background/50 border border-border rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-primary/50"
                  placeholder="Enter request type name..."
                />
              </div>

              {/* Description */}
              <div>
                <label className="block text-sm font-medium mb-2">Description</label>
                <input
                  type="text"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  className="w-full bg-background/50 border border-border rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-primary/50"
                  placeholder="Short description for the card..."
                />
              </div>

              {/* Icon */}
              <div>
                <label className="block text-sm font-medium mb-2">Icon</label>
                <select
                  value={icon}
                  onChange={(e) => setIcon(e.target.value)}
                  className="w-full bg-background/50 border border-border rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-primary/50"
                >
                  {iconOptions.map((opt) => (
                    <option key={opt.value} value={opt.value}>
                      {opt.label}
                    </option>
                  ))}
                </select>
              </div>

              {/* Active Toggle */}
              {editingType && (
                <div className="flex items-center justify-between">
                  <div>
                    <label className="block text-sm font-medium">Active</label>
                    <p className="text-sm text-muted-foreground">
                      When disabled, users won't see this request type
                    </p>
                  </div>
                  <button
                    onClick={() => setIsActive(!isActive)}
                    className={`relative w-12 h-6 rounded-full transition-colors ${
                      isActive ? 'bg-primary' : 'bg-muted'
                    }`}
                  >
                    <motion.div
                      className="absolute top-1 left-1 w-4 h-4 bg-white rounded-full"
                      animate={{ x: isActive ? 24 : 0 }}
                      transition={{ type: 'spring', stiffness: 500, damping: 30 }}
                    />
                  </button>
                </div>
              )}

              {/* JSON Editor */}
              <div>
                <label className="block text-sm font-medium mb-2">Form JSON (SurveyJS format)</label>
                <textarea
                  value={formJson}
                  onChange={(e) => {
                    setFormJson(e.target.value);
                    if (jsonError) validateJson(e.target.value);
                  }}
                  onBlur={() => validateJson(formJson)}
                  className={`w-full h-96 bg-background/50 border rounded-lg px-4 py-3 font-mono text-sm focus:outline-none focus:ring-2 focus:ring-primary/50 ${
                    jsonError ? 'border-destructive' : 'border-border'
                  }`}
                  placeholder="Enter SurveyJS JSON configuration..."
                />
                {jsonError && (
                  <div className="flex items-center gap-2 mt-2 text-sm text-destructive">
                    <AlertCircle className="h-4 w-4" />
                    {jsonError}
                  </div>
                )}
              </div>

              {/* Save Button */}
              <div className="flex justify-end gap-2">
                <Button variant="outline" onClick={resetForm}>
                  Cancel
                </Button>
                <Button
                  onClick={handleSave}
                  disabled={
                    createRequestType.isPending ||
                    updateRequestType.isPending ||
                    !!jsonError ||
                    !name.trim()
                  }
                  className="gap-2 bg-gradient-to-r from-primary to-accent hover:opacity-90"
                >
                  <Save className="h-4 w-4" />
                  {createRequestType.isPending || updateRequestType.isPending
                    ? 'Saving...'
                    : editingType
                    ? 'Update Request Type'
                    : 'Create Request Type'}
                </Button>
              </div>
            </div>
          </>
        )}
      </motion.div>

      {/* Responses Table */}
      {!isEditing && (
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="glass rounded-xl overflow-hidden"
        >
          <button
            onClick={() => setShowResponses(!showResponses)}
            className="w-full p-6 flex items-center justify-between hover:bg-white/5 transition-colors"
          >
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-accent/10">
                <Users className="h-5 w-5 text-accent" />
              </div>
              <div className="text-left">
                <h3 className="font-semibold">All Submissions</h3>
                <p className="text-sm text-muted-foreground">
                  {responsesLoading
                    ? 'Loading...'
                    : `${responses?.length || 0} total submissions`}
                </p>
              </div>
            </div>
            {showResponses ? (
              <ChevronUp className="h-5 w-5 text-muted-foreground" />
            ) : (
              <ChevronDown className="h-5 w-5 text-muted-foreground" />
            )}
          </button>

          {showResponses && (
            <div className="border-t border-border/30">
              <div className="p-6">
                {responsesLoading ? (
                  <div className="space-y-4">
                    {[...Array(3)].map((_, i) => (
                      <Skeleton key={i} className="h-16 w-full" />
                    ))}
                  </div>
                ) : (
                  <DataTable
                    data={responses || []}
                    columns={responseColumns}
                    keyField="id"
                    searchPlaceholder="Search submissions..."
                    filterOptions={{
                      key: 'isComplete',
                      label: 'Status',
                      options: [
                        { label: 'Completed', value: 'true' },
                        { label: 'In Progress', value: 'false' },
                      ],
                    }}
                    emptyState={
                      <div className="text-center py-8">
                        <Users className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
                        <p className="text-muted-foreground">No submissions yet</p>
                      </div>
                    }
                  />
                )}
              </div>

              {/* Response Detail Panel */}
              {viewingResponse && (
                <ResponseDetailPanel
                  response={viewingResponse}
                  onClose={() => setViewingResponse(null)}
                />
              )}
            </div>
          )}
        </motion.div>
      )}
    </div>
  );
}

export default RequestAdminPanel;
