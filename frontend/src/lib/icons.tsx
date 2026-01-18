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
export const iconMap: Record<string, LucideIcon> = {
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

// Helper to get an icon component by name, with fallback
export function getIconComponent(iconName: string | undefined | null): LucideIcon {
  if (!iconName) return ClipboardList;
  return iconMap[iconName] || ClipboardList;
}

// Helper component for rendering icons by name
interface DynamicIconProps {
  name: string | undefined | null;
  className?: string;
}

export function DynamicIcon({ name, className }: DynamicIconProps) {
  const IconComponent = getIconComponent(name);
  return <IconComponent className={className} />;
}
