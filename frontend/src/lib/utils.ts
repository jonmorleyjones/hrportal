import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function extractSubdomain(): string | null {
  const hostname = window.location.hostname;
  const parts = hostname.split('.');

  // For localhost development (e.g., acme.localhost)
  if (hostname.includes('localhost')) {
    if (parts.length >= 2 && parts[0] !== 'localhost') {
      return parts[0];
    }
    return null;
  }

  // For production (e.g., acme.portal.com)
  if (parts.length >= 3) {
    return parts[0];
  }

  return null;
}

export function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function formatDateTime(dateString: string): string {
  return new Date(dateString).toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
}
