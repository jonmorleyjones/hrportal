import * as React from 'react';
import { useState, useMemo } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  Paper,
  TextField,
  InputAdornment,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  IconButton,
  Typography,
  Box,
  Fade,
} from '@mui/material';
import { Search, X } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface Column<T> {
  key: keyof T | string;
  header: string;
  sortable?: boolean;
  filterable?: boolean;
  render?: (item: T) => React.ReactNode;
  getValue?: (item: T) => string | number;
  width?: number | string;
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

type SortDirection = 'asc' | 'desc';

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
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');
  const [activeFilter, setActiveFilter] = useState<string>('');

  const handleSort = (key: string) => {
    if (sortKey === key) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
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
    if (sortKey) {
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

  const hasActiveFilters = searchQuery || activeFilter;

  return (
    <Box className={cn('space-y-4', className)}>
      {/* Filter Controls */}
      <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, gap: 2 }}>
        <TextField
          placeholder={searchPlaceholder}
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          size="small"
          sx={{ flex: 1 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search className="h-4 w-4 text-muted-foreground" />
              </InputAdornment>
            ),
            endAdornment: searchQuery && (
              <InputAdornment position="end">
                <IconButton size="small" onClick={() => setSearchQuery('')}>
                  <X className="h-4 w-4" />
                </IconButton>
              </InputAdornment>
            ),
          }}
        />

        {filterOptions && (
          <FormControl size="small" sx={{ minWidth: 140 }}>
            <InputLabel>{filterOptions.label}</InputLabel>
            <Select
              value={activeFilter}
              onChange={(e) => setActiveFilter(e.target.value)}
              label={filterOptions.label}
            >
              <MenuItem value="">All {filterOptions.label}</MenuItem>
              {filterOptions.options.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )}

        {hasActiveFilters && (
          <Fade in>
            <IconButton
              onClick={() => {
                setSearchQuery('');
                setActiveFilter('');
              }}
              size="small"
              sx={{ border: 1, borderColor: 'divider', borderRadius: 1, px: 2 }}
            >
              <X className="h-4 w-4" />
              <Typography variant="body2" sx={{ ml: 1 }}>Clear</Typography>
            </IconButton>
          </Fade>
        )}
      </Box>

      {/* Results count */}
      <Typography variant="body2" color="text.secondary">
        Showing {filteredAndSortedData.length} of {data.length} results
      </Typography>

      {/* Table */}
      <TableContainer component={Paper} sx={{ borderRadius: 2, boxShadow: 'none', border: 1, borderColor: 'divider' }}>
        <Table>
          <TableHead>
            <TableRow sx={{ bgcolor: 'action.hover' }}>
              {columns.map((col) => (
                <TableCell
                  key={String(col.key)}
                  sx={{ fontWeight: 600, width: col.width }}
                  sortDirection={sortKey === col.key ? sortDirection : false}
                >
                  {col.sortable ? (
                    <TableSortLabel
                      active={sortKey === col.key}
                      direction={sortKey === col.key ? sortDirection : 'asc'}
                      onClick={() => handleSort(String(col.key))}
                    >
                      {col.header}
                    </TableSortLabel>
                  ) : (
                    col.header
                  )}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredAndSortedData.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} align="center" sx={{ py: 4 }}>
                  {emptyState || (
                    <Typography color="text.secondary">
                      {data.length === 0 ? 'No data available' : 'No matching results'}
                    </Typography>
                  )}
                </TableCell>
              </TableRow>
            ) : (
              filteredAndSortedData.map((item) => (
                <TableRow
                  key={String(item[keyField])}
                  hover
                  sx={{ '&:last-child td': { borderBottom: 0 } }}
                >
                  {columns.map((col) => (
                    <TableCell key={String(col.key)}>
                      {col.render
                        ? col.render(item)
                        : String(item[col.key as keyof T] ?? '')}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
