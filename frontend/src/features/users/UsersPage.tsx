import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { DataTable, type Column } from '@/components/ui/data-table';
import {
  IconButton,
  Menu,
  MenuItem,
  Chip,
  Avatar,
  Box,
} from '@mui/material';
import { motion, AnimatePresence, Skeleton } from '@/components/ui/motion';
import { Plus, MoreHorizontal, UserMinus, Shield, Mail, X, Send, UserCircle, CheckCircle2, XCircle } from 'lucide-react';
import { formatDateTime } from '@/lib/utils';
import type { UserRole } from '@/types';

interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  lastLoginAt?: string | null;
  isActive: boolean;
}

export function UsersPage() {
  const queryClient = useQueryClient();
  const [showInviteForm, setShowInviteForm] = useState(false);
  const [inviteEmail, setInviteEmail] = useState('');
  const [inviteRole, setInviteRole] = useState<UserRole>('Member');

  const { data, isLoading } = useQuery({
    queryKey: ['users'],
    queryFn: () => api.getUsers(1, 200),
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

  const columns: Column<User>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (user) => (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
          <Avatar
            sx={{
              width: 36,
              height: 36,
              background: 'linear-gradient(135deg, var(--primary) 0%, var(--accent) 100%)',
              fontSize: '0.875rem',
              fontWeight: 500,
            }}
          >
            {user.name
              .split(' ')
              .map((n) => n[0])
              .join('')
              .slice(0, 2)}
          </Avatar>
          <span className="font-medium">{user.name}</span>
        </Box>
      ),
    },
    {
      key: 'email',
      header: 'Email',
      sortable: true,
      render: (user) => <span className="text-muted-foreground">{user.email}</span>,
    },
    {
      key: 'role',
      header: 'Role',
      sortable: true,
      render: (user) => (
        <Chip label={user.role} size="small" variant="outlined" color="primary" />
      ),
    },
    {
      key: 'lastLoginAt',
      header: 'Last Login',
      sortable: true,
      getValue: (user) => (user.lastLoginAt ? new Date(user.lastLoginAt).getTime() : 0),
      render: (user) => (
        <span className="text-sm text-muted-foreground">
          {user.lastLoginAt ? formatDateTime(user.lastLoginAt) : 'Never'}
        </span>
      ),
    },
    {
      key: 'isActive',
      header: 'Status',
      sortable: true,
      render: (user) => (
        <span
          className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${
            user.isActive
              ? 'bg-green-500/10 text-green-400'
              : 'bg-red-500/10 text-red-400'
          }`}
        >
          {user.isActive ? (
            <CheckCircle2 className="h-3 w-3" />
          ) : (
            <XCircle className="h-3 w-3" />
          )}
          {user.isActive ? 'Active' : 'Inactive'}
        </span>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: 48,
      render: (user) => (
        <UserActionsMenu
          user={user}
          onDeactivate={() => deactivateMutation.mutate(user.id)}
        />
      ),
    },
  ];

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
        className="glass rounded-xl p-6"
      >
        {isLoading ? (
          <div className="space-y-4">
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
        ) : (
          <DataTable
            data={(data?.users || []) as User[]}
            columns={columns}
            keyField="id"
            searchPlaceholder="Search users by name or email..."
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
            pageSizeOptions={[10, 25, 50, 100]}
            emptyState={
              <div className="text-center py-12">
                <UserCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <h3 className="text-lg font-semibold mb-2">No users found</h3>
                <p className="text-muted-foreground mb-4">
                  Get started by inviting your first user
                </p>
                <Button
                  onClick={() => setShowInviteForm(true)}
                  className="bg-gradient-to-r from-primary to-accent"
                >
                  <Plus className="h-4 w-4 mr-2" />
                  Invite User
                </Button>
              </div>
            }
          />
        )}
      </motion.div>
    </div>
  );
}

function UserActionsMenu({
  onDeactivate,
}: {
  user: User;
  onDeactivate: () => void;
}) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  return (
    <>
      <IconButton size="small" onClick={handleClick}>
        <MoreHorizontal className="h-4 w-4" />
      </IconButton>
      <Menu
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        <MenuItem onClick={handleClose}>
          <Shield className="h-4 w-4 mr-2" />
          Change Role
        </MenuItem>
        <MenuItem
          onClick={() => {
            onDeactivate();
            handleClose();
          }}
          sx={{ color: 'error.main' }}
        >
          <UserMinus className="h-4 w-4 mr-2" />
          Deactivate
        </MenuItem>
      </Menu>
    </>
  );
}
