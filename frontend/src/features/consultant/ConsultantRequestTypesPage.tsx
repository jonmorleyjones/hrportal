import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { api } from '@/lib/api';
import type { ConsultantRequestType } from '@/types';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { motion, Skeleton, Spinner, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  ClipboardList,
  ArrowLeft,
  Plus,
  ToggleLeft,
  ToggleRight,
  Edit2,
  Calendar,
  FileText,
} from 'lucide-react';
import { formatDateTime } from '@/lib/utils';

const requestTypeSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  description: z.string().optional(),
});

type RequestTypeForm = z.infer<typeof requestTypeSchema>;

export function ConsultantRequestTypesPage() {
  const { tenantId } = useParams<{ tenantId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [editingType, setEditingType] = useState<string | null>(null);

  const { data: tenant } = useQuery({
    queryKey: ['consultant-tenant', tenantId],
    queryFn: () => api.getConsultantTenantDetail(tenantId!),
    enabled: !!tenantId,
  });

  const { data: requestTypes, isLoading } = useQuery({
    queryKey: ['consultant-request-types', tenantId],
    queryFn: () => api.getConsultantTenantRequestTypes(tenantId!),
    enabled: !!tenantId,
  });

  const form = useForm<RequestTypeForm>({
    resolver: zodResolver(requestTypeSchema),
    defaultValues: {
      name: '',
      description: '',
    },
  });

  const createRequestType = useMutation({
    mutationFn: (data: RequestTypeForm) =>
      api.createConsultantTenantRequestType(tenantId!, {
        name: data.name,
        description: data.description,
        formJson: '{}', // Default empty form
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['consultant-request-types', tenantId] });
      setShowCreateDialog(false);
      form.reset();
    },
  });

  const updateRequestType = useMutation({
    mutationFn: ({ id, data }: { id: string; data: RequestTypeForm }) =>
      api.updateConsultantTenantRequestType(tenantId!, id, { name: data.name, description: data.description }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['consultant-request-types', tenantId] });
      setEditingType(null);
      form.reset();
    },
  });

  const toggleStatus = useMutation({
    mutationFn: (requestType: ConsultantRequestType) =>
      api.updateConsultantTenantRequestTypeStatus(tenantId!, requestType.id, { isActive: !requestType.isActive }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['consultant-request-types', tenantId] });
    },
  });

  const handleEdit = (requestType: ConsultantRequestType) => {
    form.reset({
      name: requestType.name,
      description: requestType.description || '',
    });
    setEditingType(requestType.id);
  };

  const handleCreate = () => {
    form.reset({ name: '', description: '' });
    setShowCreateDialog(true);
  };

  return (
    <div>
      <PageHeader
        title="Request Types"
        description={tenant ? `Manage request types for ${tenant.name}` : 'Loading...'}
        actions={
          <div className="flex items-center gap-2">
            <Button onClick={() => navigate(`/consultant/tenants/${tenantId}`)} variant="outline" size="sm">
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back
            </Button>
            {tenant?.permissions?.canManageRequestTypes && (
              <Button onClick={handleCreate} size="sm" className="bg-gradient-to-r from-amber-500 to-orange-500">
                <Plus className="mr-2 h-4 w-4" />
                New Type
              </Button>
            )}
          </div>
        }
      />

      {isLoading ? (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {[...Array(6)].map((_, i) => (
            <Skeleton key={i} className="h-48 rounded-xl" />
          ))}
        </div>
      ) : requestTypes?.length === 0 ? (
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          className="text-center py-16"
        >
          <ClipboardList className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-semibold mb-2">No Request Types</h3>
          <p className="text-muted-foreground mb-4">Create your first request type to get started.</p>
          {tenant?.permissions?.canManageRequestTypes && (
            <Button onClick={handleCreate} className="bg-gradient-to-r from-amber-500 to-orange-500">
              <Plus className="mr-2 h-4 w-4" />
              Create Request Type
            </Button>
          )}
        </motion.div>
      ) : (
        <StaggerContainer className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {requestTypes?.map((requestType: ConsultantRequestType) => (
            <StaggerItem key={requestType.id}>
              <motion.div
                whileHover={{ y: -2 }}
                transition={{ duration: 0.2 }}
                className={`glass rounded-xl p-5 relative overflow-hidden group ${
                  !requestType.isActive ? 'opacity-60' : ''
                }`}
              >
                {/* Status indicator */}
                <div className={`absolute top-0 left-0 right-0 h-1 ${
                  requestType.isActive
                    ? 'bg-gradient-to-r from-green-500 to-emerald-500'
                    : 'bg-gradient-to-r from-gray-500 to-gray-600'
                }`} />

                <div className="relative z-10">
                  {/* Header */}
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex items-center gap-2">
                      <div className={`p-2 rounded-lg ${
                        requestType.isActive
                          ? 'bg-amber-500/10 text-amber-500'
                          : 'bg-gray-500/10 text-gray-500'
                      }`}>
                        <ClipboardList className="h-4 w-4" />
                      </div>
                      <div>
                        <h3 className="font-semibold">{requestType.name}</h3>
                      </div>
                    </div>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                      requestType.isActive
                        ? 'bg-green-500/10 text-green-500'
                        : 'bg-gray-500/10 text-gray-500'
                    }`}>
                      {requestType.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </div>

                  {/* Description */}
                  {requestType.description && (
                    <p className="text-sm text-muted-foreground mb-4 line-clamp-2">
                      {requestType.description}
                    </p>
                  )}

                  {/* Meta info */}
                  <div className="flex items-center gap-4 text-xs text-muted-foreground mb-4">
                    <div className="flex items-center gap-1">
                      <FileText className="h-3 w-3" />
                      <span>v{requestType.currentVersionNumber}</span>
                    </div>
                    <div className="flex items-center gap-1">
                      <Calendar className="h-3 w-3" />
                      <span>{formatDateTime(requestType.createdAt)}</span>
                    </div>
                  </div>

                  {/* Stats */}
                  <div className="grid grid-cols-2 gap-2 mb-4">
                    <div className="text-center p-2 rounded-lg bg-background/50">
                      <p className="text-lg font-semibold">{requestType.totalResponses}</p>
                      <p className="text-xs text-muted-foreground">Responses</p>
                    </div>
                    <div className="text-center p-2 rounded-lg bg-background/50">
                      <p className="text-lg font-semibold">{requestType.completedResponses}</p>
                      <p className="text-xs text-muted-foreground">Completed</p>
                    </div>
                  </div>

                  {/* Actions */}
                  {tenant?.permissions?.canManageRequestTypes && (
                    <div className="flex items-center gap-2 pt-3 border-t border-border/30">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleEdit(requestType)}
                        className="flex-1 hover:bg-white/5"
                      >
                        <Edit2 className="mr-2 h-3 w-3" />
                        Edit
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => toggleStatus.mutate(requestType)}
                        disabled={toggleStatus.isPending}
                        className={`flex-1 hover:bg-white/5 ${
                          requestType.isActive ? 'text-yellow-500' : 'text-green-500'
                        }`}
                      >
                        {requestType.isActive ? (
                          <>
                            <ToggleLeft className="mr-2 h-3 w-3" />
                            Deactivate
                          </>
                        ) : (
                          <>
                            <ToggleRight className="mr-2 h-3 w-3" />
                            Activate
                          </>
                        )}
                      </Button>
                    </div>
                  )}
                </div>
              </motion.div>
            </StaggerItem>
          ))}
        </StaggerContainer>
      )}

      {/* Create Dialog */}
      <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
        <DialogContent className="glass-strong border-border/50">
          <DialogHeader>
            <DialogTitle>Create Request Type</DialogTitle>
            <DialogDescription>
              Add a new request type for {tenant?.name}
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={form.handleSubmit((data) => createRequestType.mutate(data))} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Name</Label>
              <Input
                id="name"
                {...form.register('name')}
                placeholder="e.g., Leave Request"
                className="bg-background/50"
              />
              {form.formState.errors.name && (
                <p className="text-sm text-destructive">{form.formState.errors.name.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="description">Description (optional)</Label>
              <Textarea
                id="description"
                {...form.register('description')}
                placeholder="Describe what this request type is for..."
                className="bg-background/50"
                rows={3}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button type="button" variant="outline" onClick={() => setShowCreateDialog(false)}>
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={createRequestType.isPending}
                className="bg-gradient-to-r from-amber-500 to-orange-500"
              >
                {createRequestType.isPending ? (
                  <>
                    <Spinner size="sm" className="mr-2" />
                    Creating...
                  </>
                ) : (
                  'Create'
                )}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>

      {/* Edit Dialog */}
      <Dialog open={!!editingType} onOpenChange={() => setEditingType(null)}>
        <DialogContent className="glass-strong border-border/50">
          <DialogHeader>
            <DialogTitle>Edit Request Type</DialogTitle>
            <DialogDescription>
              Update the request type details
            </DialogDescription>
          </DialogHeader>
          <form
            onSubmit={form.handleSubmit((data) =>
              updateRequestType.mutate({ id: editingType!, data })
            )}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label htmlFor="edit-name">Name</Label>
              <Input
                id="edit-name"
                {...form.register('name')}
                className="bg-background/50"
              />
              {form.formState.errors.name && (
                <p className="text-sm text-destructive">{form.formState.errors.name.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="edit-description">Description (optional)</Label>
              <Textarea
                id="edit-description"
                {...form.register('description')}
                className="bg-background/50"
                rows={3}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button type="button" variant="outline" onClick={() => setEditingType(null)}>
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={updateRequestType.isPending}
                className="bg-gradient-to-r from-amber-500 to-orange-500"
              >
                {updateRequestType.isPending ? (
                  <>
                    <Spinner size="sm" className="mr-2" />
                    Saving...
                  </>
                ) : (
                  'Save Changes'
                )}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
