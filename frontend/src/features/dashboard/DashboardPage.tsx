import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { StatCard } from '@/components/shared/StatCard';
import { motion, Skeleton, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import { Users, UserCheck, Mail, TrendingUp, Activity } from 'lucide-react';
import { formatDateTime } from '@/lib/utils';
import {
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Area,
  AreaChart,
} from 'recharts';

export function DashboardPage() {
  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: () => api.getDashboardStats(),
  });

  const { data: activity, isLoading: activityLoading } = useQuery({
    queryKey: ['activity-feed'],
    queryFn: () => api.getActivityFeed(5),
  });

  const { data: chartData } = useQuery({
    queryKey: ['chart-users'],
    queryFn: () => api.getChartData('users'),
  });

  return (
    <div>
      <PageHeader
        title="Dashboard"
        description="Overview of your organization's activity"
      />

      {/* Stats grid */}
      <StaggerContainer className="grid gap-4 md:grid-cols-2 lg:grid-cols-4 mb-8">
        <StaggerItem>
          <StatCard
            title="Total Users"
            value={statsLoading ? '-' : stats?.totalUsers || 0}
            description="Active accounts"
            icon={Users}
            trend="up"
            trendValue="+12%"
          />
        </StaggerItem>
        <StaggerItem>
          <StatCard
            title="Active Users"
            value={statsLoading ? '-' : stats?.activeUsers || 0}
            description="Last 30 days"
            icon={UserCheck}
            trend="up"
            trendValue="+5%"
          />
        </StaggerItem>
        <StaggerItem>
          <StatCard
            title="Pending Invitations"
            value={statsLoading ? '-' : stats?.pendingInvitations || 0}
            description="Awaiting response"
            icon={Mail}
          />
        </StaggerItem>
        <StaggerItem>
          <StatCard
            title="Monthly Active Rate"
            value={statsLoading ? '-' : `${stats?.monthlyActiveRate || 0}%`}
            description="User engagement"
            icon={TrendingUp}
            trend="up"
            trendValue="+3%"
          />
        </StaggerItem>
      </StaggerContainer>

      {/* Charts section */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Chart card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="glass rounded-xl p-6"
        >
          <div className="flex items-center justify-between mb-6">
            <div>
              <h3 className="text-lg font-semibold">User Growth</h3>
              <p className="text-sm text-muted-foreground">Monthly trend</p>
            </div>
            <div className="flex items-center gap-2 text-sm">
              <span className="w-3 h-3 rounded-full bg-primary" />
              <span className="text-muted-foreground">Users</span>
            </div>
          </div>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={chartData?.data || []}>
                <defs>
                  <linearGradient id="colorValue" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="hsl(var(--primary))" stopOpacity={0.3} />
                    <stop offset="95%" stopColor="hsl(var(--primary))" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid
                  strokeDasharray="3 3"
                  stroke="hsl(var(--border))"
                  vertical={false}
                />
                <XAxis
                  dataKey="label"
                  axisLine={false}
                  tickLine={false}
                  tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }}
                />
                <YAxis
                  axisLine={false}
                  tickLine={false}
                  tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }}
                />
                <Tooltip
                  contentStyle={{
                    backgroundColor: 'hsl(var(--card))',
                    border: '1px solid hsl(var(--border))',
                    borderRadius: '8px',
                    boxShadow: '0 4px 12px rgba(0,0,0,0.3)',
                  }}
                  labelStyle={{ color: 'hsl(var(--foreground))' }}
                />
                <Area
                  type="monotone"
                  dataKey="value"
                  stroke="hsl(var(--primary))"
                  strokeWidth={2}
                  fill="url(#colorValue)"
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </motion.div>

        {/* Activity feed */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="glass rounded-xl p-6"
        >
          <div className="flex items-center gap-2 mb-6">
            <Activity className="h-5 w-5 text-primary" />
            <h3 className="text-lg font-semibold">Recent Activity</h3>
          </div>

          {activityLoading ? (
            <div className="space-y-4">
              {[...Array(5)].map((_, i) => (
                <div key={i} className="flex items-start gap-3">
                  <Skeleton className="w-2 h-2 mt-2 rounded-full" />
                  <div className="flex-1 space-y-2">
                    <Skeleton className="h-4 w-3/4" />
                    <Skeleton className="h-3 w-1/2" />
                  </div>
                </div>
              ))}
            </div>
          ) : activity?.activities.length === 0 ? (
            <div className="text-center py-8">
              <Activity className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
              <p className="text-muted-foreground">No recent activity</p>
            </div>
          ) : (
            <div className="space-y-4">
              {activity?.activities.map((item, index) => (
                <motion.div
                  key={item.id}
                  initial={{ opacity: 0, x: -10 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: 0.1 * index }}
                  className="flex items-start gap-3 text-sm group"
                >
                  <div className="relative">
                    <div className="w-2 h-2 mt-2 rounded-full bg-gradient-to-r from-primary to-accent" />
                    {index < (activity?.activities.length || 0) - 1 && (
                      <div className="absolute top-4 left-[3px] w-0.5 h-full bg-border/50" />
                    )}
                  </div>
                  <div className="flex-1 min-w-0 pb-4">
                    <p className="font-medium truncate group-hover:text-primary transition-colors">
                      {item.userName || 'System'}{' '}
                      <span className="text-muted-foreground font-normal">
                        {item.action}
                      </span>
                    </p>
                    <p className="text-muted-foreground text-xs mt-0.5">
                      {formatDateTime(item.createdAt)}
                    </p>
                  </div>
                </motion.div>
              ))}
            </div>
          )}
        </motion.div>
      </div>
    </div>
  );
}
