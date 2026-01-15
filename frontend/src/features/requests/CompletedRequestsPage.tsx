import { useState } from 'react';
import { Link } from 'react-router-dom';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { motion, Skeleton, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import { DataTable, Column } from '@/components/ui/data-table';
import { useUserRequestResponses, useResponseFiles } from './hooks/useRequests';
import {
  CheckCircle2,
  Clock,
  FileText,
  Eye,
  X,
  Download,
  Image,
  File,
  Paperclip,
  ClipboardList,
  UserPlus,
  Laptop,
  Calendar,
  Send,
  HelpCircle,
  Settings,
  Briefcase,
  Package,
  CreditCard,
  Phone,
  type LucideIcon,
} from 'lucide-react';
import { formatDate, formatBytes } from '@/lib/utils';
import type { RequestResponse, FileInfo } from '@/types';
import { api } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';

function getFileIcon(contentType: string) {
  if (contentType.startsWith('image/')) {
    return Image;
  }
  if (contentType === 'application/pdf' || contentType.includes('document') || contentType.includes('word')) {
    return FileText;
  }
  return File;
}

// Map icon string names to Lucide components
const iconMap: Record<string, LucideIcon> = {
  'clipboard-list': ClipboardList,
  'user-plus': UserPlus,
  'laptop': Laptop,
  'calendar': Calendar,
  'file-text': FileText,
  'send': Send,
  'help-circle': HelpCircle,
  'settings': Settings,
  'briefcase': Briefcase,
  'package': Package,
  'credit-card': CreditCard,
  'phone': Phone,
};

function getRequestTypeIcon(iconName: string): LucideIcon {
  return iconMap[iconName] || ClipboardList;
}

function ResponseDetailPanel({
  response,
  onClose,
}: {
  response: RequestResponse;
  onClose: () => void;
}) {
  const { data: files, isLoading: filesLoading } = useResponseFiles(response.id);
  const { user } = useAuthStore();
  const isAdmin = user?.role === 'Admin';

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

          {isAdmin && (
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
          )}
        </div>

        <div className="mt-4 pt-4 border-t border-border/30">
          <div className="flex flex-wrap gap-4 text-xs text-muted-foreground">
            <span>Version: v{response.versionNumber}</span>
            <span>Started: {formatDate(response.startedAt)}</span>
            {response.completedAt && <span>Completed: {formatDate(response.completedAt)}</span>}
          </div>
        </div>
      </div>
    </motion.div>
  );
}

export function CompletedRequestsPage() {
  const { data: responses, isLoading } = useUserRequestResponses();
  const [viewingResponse, setViewingResponse] = useState<RequestResponse | null>(null);

  const completedResponses = responses?.filter((r) => r.isComplete) || [];

  const columns: Column<RequestResponse>[] = [
    {
      key: 'requestTypeName',
      header: 'Request Type',
      sortable: true,
      render: (item) => {
        const IconComponent = getRequestTypeIcon(item.requestTypeIcon);
        return (
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20">
              <IconComponent className="h-4 w-4 text-primary" />
            </div>
            <span className="font-medium">{item.requestTypeName}</span>
          </div>
        );
      },
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
      key: 'completedAt',
      header: 'Submitted',
      sortable: true,
      render: (item) => (
        <span className="text-muted-foreground">
          {item.completedAt ? formatDate(item.completedAt) : '-'}
        </span>
      ),
      getValue: (item) => item.completedAt ? new Date(item.completedAt).getTime() : 0,
    },
    {
      key: 'isComplete',
      header: 'Status',
      sortable: true,
      render: (item) => (
        <span
          className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ${
            item.isComplete
              ? 'bg-green-500/10 border border-green-500/20 text-green-400'
              : 'bg-yellow-500/10 border border-yellow-500/20 text-yellow-400'
          }`}
        >
          {item.isComplete ? (
            <CheckCircle2 className="h-3 w-3" />
          ) : (
            <Clock className="h-3 w-3" />
          )}
          {item.isComplete ? 'Submitted' : 'In Progress'}
        </span>
      ),
      getValue: (item) => (item.isComplete ? 'Submitted' : 'In Progress'),
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

  if (isLoading) {
    return (
      <div>
        <PageHeader
          title="Completed Requests"
          description="View your submitted requests"
        />
        <div className="glass rounded-xl p-6">
          <div className="space-y-4">
            {[...Array(3)].map((_, i) => (
              <Skeleton key={i} className="h-16 w-full" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <PageHeader
        title="Completed Requests"
        description={`${completedResponses.length} completed submission${completedResponses.length !== 1 ? 's' : ''}`}
        actions={
          <Link to="/requests">
            <Button variant="outline" className="gap-2">
              <ClipboardList className="h-4 w-4" />
              New Request
            </Button>
          </Link>
        }
      />

      <StaggerContainer>
        <StaggerItem>
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="glass rounded-xl overflow-hidden"
          >
            <div className="p-6">
              <DataTable
                data={completedResponses}
                columns={columns}
                keyField="id"
                searchPlaceholder="Search completed requests..."
                pagination
                defaultPageSize={10}
                pageSizeOptions={[10, 25, 50]}
                emptyState={
                  <div className="text-center py-12">
                    <CheckCircle2 className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                    <h3 className="text-lg font-semibold mb-2">No completed requests</h3>
                    <p className="text-muted-foreground mb-4">
                      You haven't completed any requests yet.
                    </p>
                    <Link to="/requests">
                      <Button className="gap-2">
                        <ClipboardList className="h-4 w-4" />
                        Submit a Request
                      </Button>
                    </Link>
                  </div>
                }
              />
            </div>

            {viewingResponse && (
              <ResponseDetailPanel
                response={viewingResponse}
                onClose={() => setViewingResponse(null)}
              />
            )}
          </motion.div>
        </StaggerItem>
      </StaggerContainer>
    </div>
  );
}

export default CompletedRequestsPage;
