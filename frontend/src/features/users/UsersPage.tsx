import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { motion, AnimatePresence, Skeleton, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import { Plus, MoreHorizontal, UserMinus, Shield, Mail, X, Send, UserCircle } from 'lucide-react';
import { formatDateTime } from '@/lib/utils';
import type { UserRole } from '@/types';

export function UsersPage() {
  const queryClient = useQueryClient();
  const [showInviteForm, setShowInviteForm] = useState(false);
  const [inviteEmail, setInviteEmail] = useState('');
  const [inviteRole, setInviteRole] = useState<UserRole>('Member');

  const { data, isLoading } = useQuery({
    queryKey: ['users'],
    queryFn: () => api.getUsers(1, 50),
  });

  const inviteMutation = useMutation({
    mutationFn: ({ email, role }: { email: string; role: UserRole }) =>
      api.inviteUser(email, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setShowInviteForm(false);
      setInviteEmail('');
    },
  });

  const deactivateMutation = useMutation({
    mutationFn: (id: string) => api.deactivateUser(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
  });

  const handleInvite = (e: React.FormEvent) => {
    e.preventDefault();
    if (inviteEmail) {
      inviteMutation.mutate({ email: inviteEmail, role: inviteRole });
    }
  };

  return (
    <div>
      <PageHeader
        title="Users"
        description="Manage your organization's users"
        actions={
          <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
            <Button
              onClick={() => setShowInviteForm(!showInviteForm)}
              className="bg-gradient-to-r from-primary to-accent hover:opacity-90"
            >
              <Plus className="h-4 w-4 mr-2" />
              Invite User
            </Button>
          </motion.div>
        }
      />

      {/* Invite form */}
      <AnimatePresence>
        {showInviteForm && (
          <motion.div
            initial={{ opacity: 0, height: 0, marginBottom: 0 }}
            animate={{ opacity: 1, height: 'auto', marginBottom: 24 }}
            exit={{ opacity: 0, height: 0, marginBottom: 0 }}
            transition={{ duration: 0.3 }}
            className="overflow-hidden"
          >
            <div className="glass rounded-xl p-6">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-2">
                  <Mail className="h-5 w-5 text-primary" />
                  <h3 className="font-semibold">Invite a new user</h3>
                </div>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => setShowInviteForm(false)}
                  className="h-8 w-8"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
              <form onSubmit={handleInvite} className="flex items-end gap-4">
                <div className="flex-1 space-y-2">
                  <Label htmlFor="email">Email Address</Label>
                  <Input
                    id="email"
                    type="email"
                    placeholder="user@example.com"
                    value={inviteEmail}
                    onChange={(e) => setInviteEmail(e.target.value)}
                    className="bg-background/50"
                  />
                </div>
                <div className="w-40 space-y-2">
                  <Label htmlFor="role">Role</Label>
                  <select
                    id="role"
                    className="flex h-10 w-full rounded-lg border border-input bg-background/50 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/20"
                    value={inviteRole}
                    onChange={(e) => setInviteRole(e.target.value as UserRole)}
                  >
                    <option value="Viewer">Viewer</option>
                    <option value="Member">Member</option>
                    <option value="Admin">Admin</option>
                  </select>
                </div>
                <Button
                  type="submit"
                  disabled={inviteMutation.isPending}
                  className="bg-gradient-to-r from-primary to-accent"
                >
                  <Send className="h-4 w-4 mr-2" />
                  {inviteMutation.isPending ? 'Sending...' : 'Send Invite'}
                </Button>
              </form>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Users table */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.2 }}
        className="glass rounded-xl overflow-hidden"
      >
        {isLoading ? (
          <div className="p-6 space-y-4">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="flex items-center gap-4">
                <Skeleton className="h-10 w-10 rounded-full" />
                <div className="flex-1 space-y-2">
                  <Skeleton className="h-4 w-1/4" />
                  <Skeleton className="h-3 w-1/3" />
                </div>
              </div>
            ))}
          </div>
        ) : data?.users.length === 0 ? (
          <div className="p-12 text-center">
            <UserCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <p className="text-muted-foreground">No users found</p>
            <Button
              onClick={() => setShowInviteForm(true)}
              variant="outline"
              className="mt-4"
            >
              <Plus className="h-4 w-4 mr-2" />
              Invite your first user
            </Button>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border/50 bg-muted/30">
                  <th className="text-left p-4 font-medium text-muted-foreground text-sm">Name</th>
                  <th className="text-left p-4 font-medium text-muted-foreground text-sm">Email</th>
                  <th className="text-left p-4 font-medium text-muted-foreground text-sm">Role</th>
                  <th className="text-left p-4 font-medium text-muted-foreground text-sm">Last Login</th>
                  <th className="text-left p-4 font-medium text-muted-foreground text-sm">Status</th>
                  <th className="w-12"></th>
                </tr>
              </thead>
              <tbody>
                {data?.users.map((user, index) => (
                  <motion.tr
                    key={user.id}
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.05 * index }}
                    className="border-b border-border/30 last:border-0 hover:bg-white/5 transition-colors"
                  >
                    <td className="p-4">
                      <div className="flex items-center gap-3">
                        <div className="w-9 h-9 rounded-full bg-gradient-to-br from-primary to-accent flex items-center justify-center text-white text-sm font-medium">
                          {user.name.split(' ').map(n => n[0]).join('').slice(0, 2)}
                        </div>
                        <span className="font-medium">{user.name}</span>
                      </div>
                    </td>
                    <td className="p-4 text-muted-foreground">{user.email}</td>
                    <td className="p-4">
                      <span className="inline-flex items-center rounded-full bg-primary/10 border border-primary/20 px-2.5 py-1 text-xs font-medium text-primary">
                        {user.role}
                      </span>
                    </td>
                    <td className="p-4 text-muted-foreground text-sm">
                      {user.lastLoginAt
                        ? formatDateTime(user.lastLoginAt)
                        : 'Never'}
                    </td>
                    <td className="p-4">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ${
                          user.isActive
                            ? 'bg-green-500/10 border border-green-500/20 text-green-400'
                            : 'bg-red-500/10 border border-red-500/20 text-red-400'
                        }`}
                      >
                        <span className={`w-1.5 h-1.5 rounded-full mr-1.5 ${
                          user.isActive ? 'bg-green-400' : 'bg-red-400'
                        }`} />
                        {user.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="p-4">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="icon" className="h-8 w-8 hover:bg-white/10">
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end" className="glass-strong border-border/50">
                          <DropdownMenuItem className="focus:bg-white/5 cursor-pointer">
                            <Shield className="h-4 w-4 mr-2" />
                            Change Role
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            className="text-destructive focus:bg-destructive/10 cursor-pointer"
                            onClick={() => deactivateMutation.mutate(user.id)}
                          >
                            <UserMinus className="h-4 w-4 mr-2" />
                            Deactivate
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </td>
                  </motion.tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </motion.div>
    </div>
  );
}
