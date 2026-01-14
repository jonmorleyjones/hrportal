import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Menu,
  MenuItem,
  Chip,
  Avatar,
  Skeleton as MuiSkeleton,
  Box,
} from '@mui/material';
import { motion, AnimatePresence } from '@/components/ui/motion';
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
          <Box sx={{ p: 3 }}>
            {[...Array(5)].map((_, i) => (
              <Box key={i} sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                <MuiSkeleton variant="circular" width={40} height={40} />
                <Box sx={{ flex: 1 }}>
                  <MuiSkeleton variant="text" width="25%" />
                  <MuiSkeleton variant="text" width="33%" />
                </Box>
              </Box>
            ))}
          </Box>
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
          <TableContainer component={Paper} sx={{ boxShadow: 'none', bgcolor: 'transparent' }}>
            <Table>
              <TableHead>
                <TableRow sx={{ bgcolor: 'action.hover' }}>
                  <TableCell sx={{ fontWeight: 600 }}>Name</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Email</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Role</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Last Login</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                  <TableCell sx={{ width: 48 }}></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data?.users.map((user) => (
                  <UserTableRow
                    key={user.id}
                    user={user}
                    onDeactivate={() => deactivateMutation.mutate(user.id)}
                  />
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </motion.div>
    </div>
  );
}

function UserTableRow({
  user,
  onDeactivate
}: {
  user: { id: string; name: string; email: string; role: string; lastLoginAt?: string | null; isActive: boolean };
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
    <TableRow hover sx={{ '&:last-child td': { borderBottom: 0 } }}>
      <TableCell>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
          <Avatar
            sx={{
              width: 36,
              height: 36,
              background: 'linear-gradient(135deg, var(--primary) 0%, var(--accent) 100%)',
              fontSize: '0.875rem',
              fontWeight: 500
            }}
          >
            {user.name.split(' ').map(n => n[0]).join('').slice(0, 2)}
          </Avatar>
          <span className="font-medium">{user.name}</span>
        </Box>
      </TableCell>
      <TableCell sx={{ color: 'text.secondary' }}>{user.email}</TableCell>
      <TableCell>
        <Chip
          label={user.role}
          size="small"
          variant="outlined"
          color="primary"
        />
      </TableCell>
      <TableCell sx={{ color: 'text.secondary', fontSize: '0.875rem' }}>
        {user.lastLoginAt ? formatDateTime(user.lastLoginAt) : 'Never'}
      </TableCell>
      <TableCell>
        <Chip
          label={user.isActive ? 'Active' : 'Inactive'}
          size="small"
          color={user.isActive ? 'success' : 'error'}
          variant="outlined"
          icon={
            <span
              style={{
                width: 6,
                height: 6,
                borderRadius: '50%',
                backgroundColor: user.isActive ? '#4caf50' : '#f44336',
                marginLeft: 8
              }}
            />
          }
        />
      </TableCell>
      <TableCell>
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
            onClick={() => { onDeactivate(); handleClose(); }}
            sx={{ color: 'error.main' }}
          >
            <UserMinus className="h-4 w-4 mr-2" />
            Deactivate
          </MenuItem>
        </Menu>
      </TableCell>
    </TableRow>
  );
}
