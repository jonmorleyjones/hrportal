import * as React from 'react';
import { useState, useMemo } from 'react';
import { motion } from '@/components/ui/motion';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';
import { Search, ChevronUp, ChevronDown, ChevronsUpDown, X } from 'lucide-react';

export interface Column<T> {
  key: keyof T | string;
  header: string;
  sortable?: boolean;
  filterable?: boolean;
  render?: (item: T) => React.ReactNode;
  getValue?: (item: T) => string | number;
}

export interface FilterOption {
  label: string;
  value: string;
}

export interface DataTableProps<T> {
  data: T[];
  columns: Column<T>[];
  keyField: keyof T;
  searchPlaceholder?: string;
  filterOptions?: {
    key: string;
    label: string;
    options: FilterOption[];
  };
  emptyState?: React.ReactNode;
  className?: string;
}

type SortDirection = 'asc' | 'desc' | null;

export function DataTable<T>({
  data,
  columns,
  keyField,
  searchPlaceholder = 'Search...',
  filterOptions,
  emptyState,
  className,
}: DataTableProps<T>) {
  const [searchQuery, setSearchQuery] = useState('');
  const [sortKey, setSortKey] = useState<string | null>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>(null);
  const [activeFilter, setActiveFilter] = useState<string | null>(null);

  const handleSort = (key: string) => {
    if (sortKey === key) {
      if (sortDirection === 'asc') {
        setSortDirection('desc');
      } else if (sortDirection === 'desc') {
        setSortKey(null);
        setSortDirection(null);
      }
    } else {
      setSortKey(key);
      setSortDirection('asc');
    }
  };

  const filteredAndSortedData = useMemo(() => {
    let result = [...data];

    // Apply search filter
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      result = result.filter((item) =>
        columns.some((col) => {
          const value = col.getValue
            ? col.getValue(item)
            : (item[col.key as keyof T] as unknown);
          return String(value).toLowerCase().includes(query);
        })
      );
    }

    // Apply dropdown filter
    if (activeFilter && filterOptions) {
      result = result.filter((item) => {
        const value = item[filterOptions.key as keyof T];
        return String(value).toLowerCase() === activeFilter.toLowerCase();
      });
    }

    // Apply sorting
    if (sortKey && sortDirection) {
      result.sort((a, b) => {
        const col = columns.find((c) => c.key === sortKey);
        const aValue = col?.getValue
          ? col.getValue(a)
          : (a[sortKey as keyof T] as unknown);
        const bValue = col?.getValue
          ? col.getValue(b)
          : (b[sortKey as keyof T] as unknown);

        if (typeof aValue === 'number' && typeof bValue === 'number') {
          return sortDirection === 'asc' ? aValue - bValue : bValue - aValue;
        }

        const aStr = String(aValue).toLowerCase();
        const bStr = String(bValue).toLowerCase();
        if (sortDirection === 'asc') {
          return aStr.localeCompare(bStr);
        }
        return bStr.localeCompare(aStr);
      });
    }

    return result;
  }, [data, searchQuery, activeFilter, filterOptions, sortKey, sortDirection, columns]);

  const getSortIcon = (key: string) => {
    if (sortKey !== key) {
      return <ChevronsUpDown className="h-4 w-4 text-muted-foreground/50" />;
    }
    if (sortDirection === 'asc') {
      return <ChevronUp className="h-4 w-4 text-primary" />;
    }
    return <ChevronDown className="h-4 w-4 text-primary" />;
  };

  const hasActiveFilters = searchQuery || activeFilter;

  return (
    <div className={cn('space-y-4', className)}>
      {/* Filter Controls */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder={searchPlaceholder}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9 bg-background/50 border-border/50 focus:border-primary/50"
          />
          {searchQuery && (
            <button
              onClick={() => setSearchQuery('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
            >
              <X className="h-4 w-4" />
            </button>
          )}
        </div>

        {filterOptions && (
          <div className="flex gap-2">
            <select
              value={activeFilter || ''}
              onChange={(e) => setActiveFilter(e.target.value || null)}
              className="h-10 px-3 rounded-md border border-border/50 bg-background/50 text-sm focus:outline-none focus:ring-2 focus:ring-ring focus:border-primary/50 min-w-[140px]"
            >
              <option value="">All {filterOptions.label}</option>
              {filterOptions.options.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
        )}

        {hasActiveFilters && (
          <motion.button
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            onClick={() => {
              setSearchQuery('');
              setActiveFilter(null);
            }}
            className="h-10 px-3 rounded-md border border-border/50 bg-background/50 text-sm text-muted-foreground hover:text-foreground hover:border-border transition-colors flex items-center gap-2"
          >
            <X className="h-4 w-4" />
            Clear
          </motion.button>
        )}
      </div>

      {/* Results count */}
      <div className="text-sm text-muted-foreground">
        Showing {filteredAndSortedData.length} of {data.length} results
      </div>

      {/* Table */}
      <div className="overflow-x-auto rounded-lg border border-border/30">
        <table className="w-full">
          <thead>
            <tr className="border-b border-border/50 bg-muted/30">
              {columns.map((col) => (
                <th
                  key={String(col.key)}
                  className={cn(
                    'text-left py-3 px-4 font-medium text-muted-foreground text-sm',
                    col.sortable && 'cursor-pointer hover:text-foreground transition-colors select-none'
                  )}
                  onClick={() => col.sortable && handleSort(String(col.key))}
                >
                  <div className="flex items-center gap-2">
                    {col.header}
                    {col.sortable && getSortIcon(String(col.key))}
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {filteredAndSortedData.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="py-8 text-center">
                  {emptyState || (
                    <div className="text-muted-foreground">
                      {data.length === 0 ? 'No data available' : 'No matching results'}
                    </div>
                  )}
                </td>
              </tr>
            ) : (
              filteredAndSortedData.map((item, index) => (
                <motion.tr
                  key={String(item[keyField])}
                  initial={{ opacity: 0, x: -10 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: 0.03 * index }}
                  className="border-b border-border/30 last:border-0 hover:bg-white/5 transition-colors"
                >
                  {columns.map((col) => (
                    <td key={String(col.key)} className="py-4 px-4">
                      {col.render
                        ? col.render(item)
                        : String(item[col.key as keyof T] ?? '')}
                    </td>
                  ))}
                </motion.tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
