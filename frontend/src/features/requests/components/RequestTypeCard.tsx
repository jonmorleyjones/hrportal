import { Link } from 'react-router-dom';
import { motion } from '@/components/ui/motion';
import type { RequestTypeCard as RequestTypeCardType } from '@/types';
import {
  ClipboardList,
  UserPlus,
  Laptop,
  Calendar,
  FileText,
  Send,
  HelpCircle,
  Settings,
  Briefcase,
  Package,
  CreditCard,
  Phone,
  type LucideIcon
} from 'lucide-react';

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

interface RequestTypeCardProps {
  requestType: RequestTypeCardType;
}

export function RequestTypeCard({ requestType }: RequestTypeCardProps) {
  const IconComponent = iconMap[requestType.icon] || ClipboardList;

  return (
    <Link to={`/requests/${requestType.id}`}>
      <motion.div
        whileHover={{ scale: 1.02, y: -4 }}
        whileTap={{ scale: 0.98 }}
        className="glass rounded-xl p-6 cursor-pointer transition-all duration-200 hover:shadow-lg hover:shadow-primary/10 border border-transparent hover:border-primary/20 h-full"
      >
        <div className="flex items-start gap-4">
          <div className="p-3 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20 shrink-0">
            <IconComponent className="h-6 w-6 text-primary" />
          </div>
          <div className="flex-1 min-w-0">
            <h3 className="font-semibold text-lg mb-1 truncate">{requestType.name}</h3>
            {requestType.description && (
              <p className="text-sm text-muted-foreground line-clamp-2">
                {requestType.description}
              </p>
            )}
          </div>
        </div>
      </motion.div>
    </Link>
  );
}

export default RequestTypeCard;
