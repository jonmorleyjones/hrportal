import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { api } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { motion, Skeleton, Spinner } from '@/components/ui/motion';
import {
  Building2,
  Users,
  ClipboardList,
  Settings,
  Palette,
  ArrowLeft,
  Save,
  ChevronRight,
} from 'lucide-react';

const settingsSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  contactEmail: z.string().email('Invalid email').optional().or(z.literal('')),
  supportPhone: z.string().optional(),
  address: z.string().optional(),
});

const brandingSchema = z.object({
  primaryColor: z.string().optional(),
  secondaryColor: z.string().optional(),
  logoUrl: z.string().url().optional().or(z.literal('')),
  customCss: z.string().optional(),
});

type SettingsForm = z.infer<typeof settingsSchema>;
type BrandingForm = z.infer<typeof brandingSchema>;

export function ConsultantTenantDetailPage() {
  const { tenantId } = useParams<{ tenantId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<'overview' | 'settings' | 'branding'>('overview');

  const { data: tenant, isLoading } = useQuery({
    queryKey: ['consultant-tenant', tenantId],
    queryFn: () => api.getConsultantTenantDetail(tenantId!),
    enabled: !!tenantId,
  });

  const settingsForm = useForm<SettingsForm>({
    resolver: zodResolver(settingsSchema),
    values: {
      name: tenant?.name || '',
      contactEmail: tenant?.settings?.contactEmail || '',
      supportPhone: tenant?.settings?.supportPhone || '',
      address: tenant?.settings?.address || '',
    },
  });

  const brandingForm = useForm<BrandingForm>({
    resolver: zodResolver(brandingSchema),
    values: {
      primaryColor: tenant?.branding?.primaryColor || '',
      secondaryColor: tenant?.branding?.secondaryColor || '',
      logoUrl: tenant?.branding?.logoUrl || '',
      customCss: tenant?.branding?.customCss || '',
    },
  });

  const updateSettings = useMutation({
    mutationFn: (data: SettingsForm) => api.updateConsultantTenantSettings(tenantId!, data as unknown as import('@/types').TenantSettings),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['consultant-tenant', tenantId] });
    },
  });

  const updateBranding = useMutation({
    mutationFn: (data: BrandingForm) => api.updateConsultantTenantBranding(tenantId!, data as unknown as import('@/types').TenantBranding),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['consultant-tenant', tenantId] });
    },
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-64" />
        <Skeleton className="h-64 rounded-xl" />
      </div>
    );
  }

  if (!tenant) {
    return (
      <div className="text-center py-16">
        <Building2 className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
        <h3 className="text-lg font-semibold mb-2">Tenant Not Found</h3>
        <p className="text-muted-foreground mb-4">This tenant doesn't exist or you don't have access.</p>
        <Button onClick={() => navigate('/consultant/tenants')} variant="outline">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Tenants
        </Button>
      </div>
    );
  }

  return (
    <div>
      <PageHeader
        title={tenant.name}
        description={`${tenant.slug}.portal.com`}
        actions={
          <Button onClick={() => navigate('/consultant/tenants')} variant="outline" size="sm">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back
          </Button>
        }
      />

      {/* Quick Actions */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4 mb-8">
        <motion.div
          whileHover={{ y: -2 }}
          onClick={() => navigate(`/consultant/tenants/${tenantId}/request-types`)}
          className="glass rounded-xl p-4 cursor-pointer group"
        >
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-amber-500/10 text-amber-500">
              <ClipboardList className="h-5 w-5" />
            </div>
            <div className="flex-1">
              <p className="font-medium group-hover:text-amber-500 transition-colors">Request Types</p>
              <p className="text-sm text-muted-foreground">{tenant.activeRequestTypes || 0} active</p>
            </div>
            <ChevronRight className="h-5 w-5 text-muted-foreground group-hover:text-amber-500 group-hover:translate-x-1 transition-all" />
          </div>
        </motion.div>

        <motion.div
          whileHover={{ y: -2 }}
          className="glass rounded-xl p-4"
        >
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-blue-500/10 text-blue-500">
              <Users className="h-5 w-5" />
            </div>
            <div>
              <p className="font-medium">Users</p>
              <p className="text-sm text-muted-foreground">{tenant.userCount || 0} total</p>
            </div>
          </div>
        </motion.div>

        <motion.div
          whileHover={{ y: -2 }}
          onClick={() => setActiveTab('settings')}
          className="glass rounded-xl p-4 cursor-pointer group"
        >
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-purple-500/10 text-purple-500">
              <Settings className="h-5 w-5" />
            </div>
            <div className="flex-1">
              <p className="font-medium group-hover:text-purple-500 transition-colors">Settings</p>
              <p className="text-sm text-muted-foreground">Configure tenant</p>
            </div>
            <ChevronRight className="h-5 w-5 text-muted-foreground group-hover:text-purple-500 group-hover:translate-x-1 transition-all" />
          </div>
        </motion.div>

        <motion.div
          whileHover={{ y: -2 }}
          onClick={() => setActiveTab('branding')}
          className="glass rounded-xl p-4 cursor-pointer group"
        >
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-pink-500/10 text-pink-500">
              <Palette className="h-5 w-5" />
            </div>
            <div className="flex-1">
              <p className="font-medium group-hover:text-pink-500 transition-colors">Branding</p>
              <p className="text-sm text-muted-foreground">Customize look</p>
            </div>
            <ChevronRight className="h-5 w-5 text-muted-foreground group-hover:text-pink-500 group-hover:translate-x-1 transition-all" />
          </div>
        </motion.div>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 mb-6">
        {['overview', 'settings', 'branding'].map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab as typeof activeTab)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
              activeTab === tab
                ? 'bg-amber-500/10 text-amber-500'
                : 'text-muted-foreground hover:text-foreground hover:bg-white/5'
            }`}
          >
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      {activeTab === 'overview' && (
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass rounded-xl p-6"
        >
          <h3 className="text-lg font-semibold mb-6">Tenant Overview</h3>
          <div className="grid gap-6 md:grid-cols-2">
            <div>
              <p className="text-sm text-muted-foreground mb-1">Subscription Tier</p>
              <p className="font-medium">{tenant.subscriptionTier}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Created</p>
              <p className="font-medium">{new Date(tenant.createdAt).toLocaleDateString()}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Total Users</p>
              <p className="font-medium">{tenant.userCount || 0}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Active Request Types</p>
              <p className="font-medium">{tenant.activeRequestTypes || 0}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Pending Responses</p>
              <p className="font-medium">{tenant.pendingResponses || 0}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground mb-1">Contact Email</p>
              <p className="font-medium">{tenant.settings?.contactEmail ?? '-'}</p>
            </div>
          </div>

          {/* Permissions */}
          <div className="mt-6 pt-6 border-t border-border/30">
            <p className="text-sm text-muted-foreground mb-3">Your Permissions</p>
            <div className="flex flex-wrap gap-2">
              {tenant.permissions?.canManageRequestTypes && (
                <span className="px-3 py-1.5 rounded-lg bg-amber-500/10 text-amber-500 text-sm">
                  Manage Request Types
                </span>
              )}
              {tenant.permissions?.canManageSettings && (
                <span className="px-3 py-1.5 rounded-lg bg-blue-500/10 text-blue-500 text-sm">
                  Manage Settings
                </span>
              )}
              {tenant.permissions?.canManageBranding && (
                <span className="px-3 py-1.5 rounded-lg bg-purple-500/10 text-purple-500 text-sm">
                  Manage Branding
                </span>
              )}
              {tenant.permissions?.canViewResponses && (
                <span className="px-3 py-1.5 rounded-lg bg-green-500/10 text-green-500 text-sm">
                  View Responses
                </span>
              )}
            </div>
          </div>
        </motion.div>
      )}

      {activeTab === 'settings' && tenant.permissions?.canManageSettings && (
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass rounded-xl p-6"
        >
          <form onSubmit={settingsForm.handleSubmit((data) => updateSettings.mutate(data))}>
            <h3 className="text-lg font-semibold mb-6">Tenant Settings</h3>
            <div className="grid gap-6 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="name">Organization Name</Label>
                <Input
                  id="name"
                  {...settingsForm.register('name')}
                  className="bg-background/50"
                />
                {settingsForm.formState.errors.name && (
                  <p className="text-sm text-destructive">{settingsForm.formState.errors.name.message}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="contactEmail">Contact Email</Label>
                <Input
                  id="contactEmail"
                  type="email"
                  {...settingsForm.register('contactEmail')}
                  className="bg-background/50"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="supportPhone">Support Phone</Label>
                <Input
                  id="supportPhone"
                  {...settingsForm.register('supportPhone')}
                  className="bg-background/50"
                />
              </div>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="address">Address</Label>
                <Textarea
                  id="address"
                  {...settingsForm.register('address')}
                  className="bg-background/50"
                  rows={3}
                />
              </div>
            </div>
            <div className="mt-6 flex justify-end">
              <Button
                type="submit"
                disabled={updateSettings.isPending}
                className="bg-gradient-to-r from-amber-500 to-orange-500 hover:opacity-90"
              >
                {updateSettings.isPending ? (
                  <>
                    <Spinner size="sm" className="mr-2" />
                    Saving...
                  </>
                ) : (
                  <>
                    <Save className="mr-2 h-4 w-4" />
                    Save Settings
                  </>
                )}
              </Button>
            </div>
          </form>
        </motion.div>
      )}

      {activeTab === 'branding' && tenant.permissions?.canManageBranding && (
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass rounded-xl p-6"
        >
          <form onSubmit={brandingForm.handleSubmit((data) => updateBranding.mutate(data))}>
            <h3 className="text-lg font-semibold mb-6">Branding Settings</h3>
            <div className="grid gap-6 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="primaryColor">Primary Color</Label>
                <div className="flex gap-2">
                  <Input
                    id="primaryColor"
                    type="color"
                    {...brandingForm.register('primaryColor')}
                    className="w-14 h-10 p-1 bg-background/50"
                  />
                  <Input
                    {...brandingForm.register('primaryColor')}
                    placeholder="#6366f1"
                    className="flex-1 bg-background/50"
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="secondaryColor">Secondary Color</Label>
                <div className="flex gap-2">
                  <Input
                    id="secondaryColor"
                    type="color"
                    {...brandingForm.register('secondaryColor')}
                    className="w-14 h-10 p-1 bg-background/50"
                  />
                  <Input
                    {...brandingForm.register('secondaryColor')}
                    placeholder="#8b5cf6"
                    className="flex-1 bg-background/50"
                  />
                </div>
              </div>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="logoUrl">Logo URL</Label>
                <Input
                  id="logoUrl"
                  type="url"
                  {...brandingForm.register('logoUrl')}
                  placeholder="https://example.com/logo.png"
                  className="bg-background/50"
                />
              </div>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="customCss">Custom CSS</Label>
                <Textarea
                  id="customCss"
                  {...brandingForm.register('customCss')}
                  placeholder="/* Add custom styles here */"
                  className="bg-background/50 font-mono text-sm"
                  rows={6}
                />
              </div>
            </div>
            <div className="mt-6 flex justify-end">
              <Button
                type="submit"
                disabled={updateBranding.isPending}
                className="bg-gradient-to-r from-amber-500 to-orange-500 hover:opacity-90"
              >
                {updateBranding.isPending ? (
                  <>
                    <Spinner size="sm" className="mr-2" />
                    Saving...
                  </>
                ) : (
                  <>
                    <Save className="mr-2 h-4 w-4" />
                    Save Branding
                  </>
                )}
              </Button>
            </div>
          </form>
        </motion.div>
      )}

      {activeTab === 'settings' && !tenant.permissions?.canManageSettings && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="glass rounded-xl p-8 text-center"
        >
          <Settings className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-semibold mb-2">Access Denied</h3>
          <p className="text-muted-foreground">You don't have permission to manage settings for this tenant.</p>
        </motion.div>
      )}

      {activeTab === 'branding' && !tenant.permissions?.canManageBranding && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="glass rounded-xl p-8 text-center"
        >
          <Palette className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-semibold mb-2">Access Denied</h3>
          <p className="text-muted-foreground">You don't have permission to manage branding for this tenant.</p>
        </motion.div>
      )}
    </div>
  );
}
