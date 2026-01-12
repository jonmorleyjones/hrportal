import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { useTenantStore } from '@/stores/tenantStore';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { motion, StaggerContainer, StaggerItem, Spinner } from '@/components/ui/motion';
import { Building2, Globe, Bell, Palette, Save, Check } from 'lucide-react';

export function SettingsPage() {
  const queryClient = useQueryClient();
  const { tenant } = useTenantStore();

  const [settings, setSettings] = useState({
    enableNotifications: tenant?.settings?.enableNotifications ?? true,
    timezone: tenant?.settings?.timezone ?? 'UTC',
    language: tenant?.settings?.language ?? 'en',
  });

  const [branding, setBranding] = useState({
    primaryColor: tenant?.branding?.primaryColor ?? '#3b82f6',
    secondaryColor: tenant?.branding?.secondaryColor ?? '#1e40af',
    logoUrl: tenant?.branding?.logoUrl ?? '',
  });

  const settingsMutation = useMutation({
    mutationFn: () => api.updateTenantSettings(settings),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant'] });
    },
  });

  const brandingMutation = useMutation({
    mutationFn: () => api.updateTenantBranding(branding),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant'] });
    },
  });

  return (
    <div>
      <PageHeader
        title="Settings"
        description="Manage your organization settings"
      />

      <StaggerContainer className="space-y-6">
        {/* Organization Details */}
        <StaggerItem>
          <motion.div
            whileHover={{ scale: 1.005 }}
            transition={{ duration: 0.2 }}
            className="glass rounded-xl overflow-hidden"
          >
            <div className="p-6 border-b border-border/30">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-primary/10">
                  <Building2 className="h-5 w-5 text-primary" />
                </div>
                <div>
                  <h3 className="font-semibold">Organization Details</h3>
                  <p className="text-sm text-muted-foreground">Basic information about your organization</p>
                </div>
              </div>
            </div>
            <div className="p-6">
              <div className="grid gap-6 md:grid-cols-2">
                <div className="space-y-2">
                  <Label>Organization Name</Label>
                  <Input value={tenant?.name || ''} disabled className="bg-muted/30" />
                </div>
                <div className="space-y-2">
                  <Label>Subdomain</Label>
                  <div className="flex items-center gap-2">
                    <Input value={tenant?.slug || ''} disabled className="bg-muted/30" />
                    <span className="text-muted-foreground text-sm">.portal.com</span>
                  </div>
                </div>
              </div>
            </div>
          </motion.div>
        </StaggerItem>

        {/* Preferences */}
        <StaggerItem>
          <motion.div
            whileHover={{ scale: 1.005 }}
            transition={{ duration: 0.2 }}
            className="glass rounded-xl overflow-hidden"
          >
            <div className="p-6 border-b border-border/30">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-accent/10">
                  <Globe className="h-5 w-5 text-accent" />
                </div>
                <div>
                  <h3 className="font-semibold">Preferences</h3>
                  <p className="text-sm text-muted-foreground">Configure your organization preferences</p>
                </div>
              </div>
            </div>
            <div className="p-6 space-y-6">
              <div className="grid gap-6 md:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="timezone">Timezone</Label>
                  <select
                    id="timezone"
                    className="flex h-10 w-full rounded-lg border border-input bg-background/50 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/20"
                    value={settings.timezone}
                    onChange={(e) => setSettings({ ...settings, timezone: e.target.value })}
                  >
                    <option value="UTC">UTC</option>
                    <option value="America/New_York">Eastern Time</option>
                    <option value="America/Chicago">Central Time</option>
                    <option value="America/Denver">Mountain Time</option>
                    <option value="America/Los_Angeles">Pacific Time</option>
                    <option value="Europe/London">London</option>
                    <option value="Europe/Paris">Paris</option>
                    <option value="Asia/Tokyo">Tokyo</option>
                  </select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="language">Language</Label>
                  <select
                    id="language"
                    className="flex h-10 w-full rounded-lg border border-input bg-background/50 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/20"
                    value={settings.language}
                    onChange={(e) => setSettings({ ...settings, language: e.target.value })}
                  >
                    <option value="en">English</option>
                    <option value="es">Spanish</option>
                    <option value="fr">French</option>
                    <option value="de">German</option>
                  </select>
                </div>
              </div>
              <div className="flex items-center gap-3 p-4 rounded-lg bg-muted/30 border border-border/30">
                <div className="relative">
                  <input
                    type="checkbox"
                    id="notifications"
                    checked={settings.enableNotifications}
                    onChange={(e) =>
                      setSettings({ ...settings, enableNotifications: e.target.checked })
                    }
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-muted rounded-full peer peer-checked:bg-gradient-to-r peer-checked:from-primary peer-checked:to-accent transition-all cursor-pointer" onClick={() => setSettings({ ...settings, enableNotifications: !settings.enableNotifications })}>
                    <div className={`absolute top-0.5 left-0.5 w-5 h-5 bg-white rounded-full transition-transform ${settings.enableNotifications ? 'translate-x-5' : ''}`} />
                  </div>
                </div>
                <div className="flex-1">
                  <Label htmlFor="notifications" className="cursor-pointer">Enable email notifications</Label>
                  <p className="text-xs text-muted-foreground">Receive updates about your organization</p>
                </div>
                <Bell className="h-5 w-5 text-muted-foreground" />
              </div>
              <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                <Button
                  onClick={() => settingsMutation.mutate()}
                  disabled={settingsMutation.isPending}
                  className="bg-gradient-to-r from-primary to-accent"
                >
                  {settingsMutation.isPending ? (
                    <>
                      <Spinner size="sm" className="mr-2 border-white/30 border-t-white" />
                      Saving...
                    </>
                  ) : settingsMutation.isSuccess ? (
                    <>
                      <Check className="h-4 w-4 mr-2" />
                      Saved!
                    </>
                  ) : (
                    <>
                      <Save className="h-4 w-4 mr-2" />
                      Save Preferences
                    </>
                  )}
                </Button>
              </motion.div>
            </div>
          </motion.div>
        </StaggerItem>

        {/* Branding */}
        <StaggerItem>
          <motion.div
            whileHover={{ scale: 1.005 }}
            transition={{ duration: 0.2 }}
            className="glass rounded-xl overflow-hidden"
          >
            <div className="p-6 border-b border-border/30">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20">
                  <Palette className="h-5 w-5 text-primary" />
                </div>
                <div>
                  <h3 className="font-semibold">Branding</h3>
                  <p className="text-sm text-muted-foreground">Customize your organization's appearance</p>
                </div>
              </div>
            </div>
            <div className="p-6 space-y-6">
              <div className="grid gap-6 md:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="primaryColor">Primary Color</Label>
                  <div className="flex gap-3">
                    <div className="relative">
                      <input
                        type="color"
                        id="primaryColor"
                        value={branding.primaryColor}
                        onChange={(e) =>
                          setBranding({ ...branding, primaryColor: e.target.value })
                        }
                        className="sr-only"
                      />
                      <label
                        htmlFor="primaryColor"
                        className="block w-12 h-10 rounded-lg border-2 border-border cursor-pointer transition-transform hover:scale-105"
                        style={{ backgroundColor: branding.primaryColor }}
                      />
                    </div>
                    <Input
                      value={branding.primaryColor}
                      onChange={(e) =>
                        setBranding({ ...branding, primaryColor: e.target.value })
                      }
                      className="font-mono bg-background/50"
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="secondaryColor">Secondary Color</Label>
                  <div className="flex gap-3">
                    <div className="relative">
                      <input
                        type="color"
                        id="secondaryColor"
                        value={branding.secondaryColor}
                        onChange={(e) =>
                          setBranding({ ...branding, secondaryColor: e.target.value })
                        }
                        className="sr-only"
                      />
                      <label
                        htmlFor="secondaryColor"
                        className="block w-12 h-10 rounded-lg border-2 border-border cursor-pointer transition-transform hover:scale-105"
                        style={{ backgroundColor: branding.secondaryColor }}
                      />
                    </div>
                    <Input
                      value={branding.secondaryColor}
                      onChange={(e) =>
                        setBranding({ ...branding, secondaryColor: e.target.value })
                      }
                      className="font-mono bg-background/50"
                    />
                  </div>
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="logoUrl">Logo URL</Label>
                <Input
                  id="logoUrl"
                  placeholder="https://example.com/logo.png"
                  value={branding.logoUrl || ''}
                  onChange={(e) => setBranding({ ...branding, logoUrl: e.target.value })}
                  className="bg-background/50"
                />
                <p className="text-xs text-muted-foreground">
                  Recommended size: 200x50px, PNG or SVG format
                </p>
              </div>
              <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                <Button
                  onClick={() => brandingMutation.mutate()}
                  disabled={brandingMutation.isPending}
                  className="bg-gradient-to-r from-primary to-accent"
                >
                  {brandingMutation.isPending ? (
                    <>
                      <Spinner size="sm" className="mr-2 border-white/30 border-t-white" />
                      Saving...
                    </>
                  ) : brandingMutation.isSuccess ? (
                    <>
                      <Check className="h-4 w-4 mr-2" />
                      Saved!
                    </>
                  ) : (
                    <>
                      <Save className="h-4 w-4 mr-2" />
                      Save Branding
                    </>
                  )}
                </Button>
              </motion.div>
            </div>
          </motion.div>
        </StaggerItem>
      </StaggerContainer>
    </div>
  );
}
