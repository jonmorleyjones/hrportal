import { motion } from '@/components/ui/motion';
import type { LucideIcon } from 'lucide-react';

interface StatCardProps {
  title: string;
  value: string | number;
  description?: string;
  icon?: LucideIcon;
  trend?: 'up' | 'down' | 'neutral';
  trendValue?: string;
}

export function StatCard({ title, value, description, icon: Icon, trend, trendValue }: StatCardProps) {
  return (
    <motion.div
      whileHover={{ y: -4, scale: 1.02 }}
      transition={{ duration: 0.2 }}
      className="glass rounded-xl p-5 relative overflow-hidden group"
    >
      {/* Gradient accent on hover */}
      <div className="absolute inset-0 bg-gradient-to-br from-primary/5 to-accent/5 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

      <div className="relative z-10">
        <div className="flex items-center justify-between mb-3">
          <p className="text-sm font-medium text-muted-foreground">{title}</p>
          {Icon && (
            <div className="p-2 rounded-lg bg-primary/10 text-primary">
              <Icon className="h-4 w-4" />
            </div>
          )}
        </div>

        <div className="flex items-end gap-2">
          <motion.p
            key={value}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-3xl font-bold tracking-tight"
          >
            {value}
          </motion.p>
          {trend && trendValue && (
            <span
              className={`text-sm font-medium mb-1 ${
                trend === 'up'
                  ? 'text-green-400'
                  : trend === 'down'
                  ? 'text-red-400'
                  : 'text-muted-foreground'
              }`}
            >
              {trend === 'up' ? '↑' : trend === 'down' ? '↓' : ''} {trendValue}
            </span>
          )}
        </div>

        {description && (
          <p className="text-xs text-muted-foreground mt-1">{description}</p>
        )}
      </div>

      {/* Decorative corner gradient */}
      <div className="absolute -bottom-4 -right-4 w-24 h-24 bg-gradient-to-tl from-primary/10 to-transparent rounded-full blur-xl opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
    </motion.div>
  );
}
