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
  TablePagination,
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
  Chip,
} from '@mui/material';
import { Search, X, Filter } from 'lucide-react';
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
  pagination?: boolean;
  defaultPageSize?: number;
  pageSizeOptions?: number[];
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
  pagination = true,
  defaultPageSize = 10,
  pageSizeOptions = [10, 25, 50, 100],
}: DataTableProps<T>) {
  const [searchQuery, setSearchQuery] = useState('');
  const [sortKey, setSortKey] = useState<string | null>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');
  const [activeFilter, setActiveFilter] = useState<string>('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(defaultPageSize);

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

  // Paginated data
  const paginatedData = useMemo(() => {
    if (!pagination) return filteredAndSortedData;
    const startIndex = page * rowsPerPage;
    return filteredAndSortedData.slice(startIndex, startIndex + rowsPerPage);
  }, [filteredAndSortedData, page, rowsPerPage, pagination]);

  // Reset page when filters change
  React.useEffect(() => {
    setPage(0);
  }, [searchQuery, activeFilter]);

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const hasActiveFilters = searchQuery || activeFilter;

  return (
    <Box className={cn('space-y-4', className)}>
      {/* Filter Controls */}
      <Box
        sx={{
          display: 'flex',
          flexDirection: { xs: 'column', sm: 'row' },
          gap: 2,
          p: 2.5,
          borderRadius: 3,
          bgcolor: 'rgba(255, 255, 255, 0.02)',
          border: '1px solid',
          borderColor: 'rgba(255, 255, 255, 0.08)',
          backdropFilter: 'blur(8px)',
        }}
      >
        <TextField
          placeholder={searchPlaceholder}
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          size="small"
          sx={{
            flex: 1,
            '& .MuiOutlinedInput-root': {
              bgcolor: 'rgba(255, 255, 255, 0.03)',
              borderRadius: 2,
              transition: 'all 0.2s ease',
              '& .MuiOutlinedInput-notchedOutline': {
                borderColor: 'rgba(255, 255, 255, 0.1)',
              },
              '&:hover': {
                bgcolor: 'rgba(255, 255, 255, 0.05)',
                '& .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'rgba(139, 92, 246, 0.4)',
                },
              },
              '&.Mui-focused': {
                bgcolor: 'rgba(255, 255, 255, 0.05)',
                '& .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'primary.main',
                  borderWidth: 2,
                },
              },
            },
          }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search className="h-4 w-4 text-muted-foreground" />
              </InputAdornment>
            ),
            endAdornment: searchQuery && (
              <InputAdornment position="end">
                <IconButton
                  size="small"
                  onClick={() => setSearchQuery('')}
                  sx={{
                    '&:hover': {
                      bgcolor: 'rgba(255, 255, 255, 0.1)',
                    },
                  }}
                >
                  <X className="h-4 w-4" />
                </IconButton>
              </InputAdornment>
            ),
          }}
        />

        {filterOptions && (
          <FormControl
            size="small"
            sx={{
              minWidth: 180,
              '& .MuiOutlinedInput-root': {
                bgcolor: 'rgba(255, 255, 255, 0.03)',
                borderRadius: 2,
                transition: 'all 0.2s ease',
                '& .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'rgba(255, 255, 255, 0.1)',
                },
                '&:hover': {
                  bgcolor: 'rgba(255, 255, 255, 0.05)',
                  '& .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'rgba(139, 92, 246, 0.4)',
                  },
                },
                '&.Mui-focused': {
                  '& .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'primary.main',
                  },
                },
              },
              '& .MuiInputLabel-root': {
                color: 'rgba(255, 255, 255, 0.6)',
              },
            }}
          >
            <InputLabel>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <Filter className="h-3 w-3" />
                {filterOptions.label}
              </Box>
            </InputLabel>
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
            <Chip
              label="Clear filters"
              onDelete={() => {
                setSearchQuery('');
                setActiveFilter('');
              }}
              deleteIcon={<X className="h-3 w-3" />}
              size="medium"
              sx={{
                height: 40,
                borderRadius: 2,
                bgcolor: 'rgba(239, 68, 68, 0.15)',
                color: '#ef4444',
                border: '1px solid rgba(239, 68, 68, 0.3)',
                fontWeight: 500,
                '& .MuiChip-deleteIcon': {
                  color: 'inherit',
                },
                '&:hover': {
                  bgcolor: 'rgba(239, 68, 68, 0.25)',
                },
                transition: 'all 0.2s ease',
              }}
            />
          </Fade>
        )}
      </Box>

      {/* Results count */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="body2" color="text.secondary">
          {pagination ? (
            <>
              Showing {paginatedData.length > 0 ? page * rowsPerPage + 1 : 0}-
              {Math.min((page + 1) * rowsPerPage, filteredAndSortedData.length)} of {filteredAndSortedData.length} results
              {filteredAndSortedData.length !== data.length && (
                <span className="text-muted-foreground/70"> (filtered from {data.length})</span>
              )}
            </>
          ) : (
            <>Showing {filteredAndSortedData.length} of {data.length} results</>
          )}
        </Typography>
      </Box>

      {/* Table */}
      <TableContainer
        component={Paper}
        sx={{
          borderRadius: 3,
          boxShadow: '0 4px 24px rgba(0, 0, 0, 0.25)',
          border: '1px solid',
          borderColor: 'rgba(255, 255, 255, 0.08)',
          overflow: 'hidden',
          backdropFilter: 'blur(12px)',
          background: 'linear-gradient(180deg, rgba(255, 255, 255, 0.03) 0%, rgba(255, 255, 255, 0.01) 100%)',
        }}
      >
        <Table>
          <TableHead>
            <TableRow
              sx={{
                background: 'linear-gradient(90deg, rgba(139, 92, 246, 0.08) 0%, rgba(139, 92, 246, 0.03) 100%)',
                borderBottom: '1px solid',
                borderColor: 'rgba(139, 92, 246, 0.2)',
              }}
            >
              {columns.map((col) => (
                <TableCell
                  key={String(col.key)}
                  sx={{
                    fontWeight: 600,
                    width: col.width,
                    color: 'rgba(255, 255, 255, 0.9)',
                    fontSize: '0.75rem',
                    textTransform: 'uppercase',
                    letterSpacing: '0.08em',
                    py: 2,
                    px: 2.5,
                    borderBottom: 'none',
                  }}
                  sortDirection={sortKey === col.key ? sortDirection : false}
                >
                  {col.sortable ? (
                    <TableSortLabel
                      active={sortKey === col.key}
                      direction={sortKey === col.key ? sortDirection : 'asc'}
                      onClick={() => handleSort(String(col.key))}
                      sx={{
                        '&:hover': { color: 'primary.light' },
                        '&.Mui-active': {
                          color: 'primary.light',
                          '& .MuiTableSortLabel-icon': {
                            color: 'primary.light',
                          },
                        },
                      }}
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
            {paginatedData.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} align="center" sx={{ py: 8, borderBottom: 'none' }}>
                  {emptyState || (
                    <Box sx={{ py: 4 }}>
                      <Typography color="text.secondary" sx={{ fontSize: '0.9375rem' }}>
                        {data.length === 0 ? 'No data available' : 'No matching results'}
                      </Typography>
                      {hasActiveFilters && (
                        <Typography
                          variant="body2"
                          color="text.secondary"
                          sx={{ mt: 1, opacity: 0.6 }}
                        >
                          Try adjusting your search or filters
                        </Typography>
                      )}
                    </Box>
                  )}
                </TableCell>
              </TableRow>
            ) : (
              paginatedData.map((item, index) => (
                <TableRow
                  key={String(item[keyField])}
                  hover
                  sx={{
                    '&:last-child td': { borderBottom: 0 },
                    '&:hover': {
                      bgcolor: 'rgba(139, 92, 246, 0.06)',
                      '& td': {
                        color: 'rgba(255, 255, 255, 0.95)',
                      },
                    },
                    bgcolor: index % 2 === 0 ? 'transparent' : 'rgba(255, 255, 255, 0.015)',
                    transition: 'all 0.2s ease',
                  }}
                >
                  {columns.map((col) => (
                    <TableCell
                      key={String(col.key)}
                      sx={{
                        py: 2,
                        px: 2.5,
                        borderColor: 'rgba(255, 255, 255, 0.05)',
                        color: 'rgba(255, 255, 255, 0.8)',
                        fontSize: '0.875rem',
                        transition: 'color 0.2s ease',
                      }}
                    >
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

        {/* Pagination */}
        {pagination && filteredAndSortedData.length > 0 && (
          <Box
            sx={{
              borderTop: '1px solid',
              borderColor: 'rgba(255, 255, 255, 0.06)',
              background: 'linear-gradient(90deg, rgba(139, 92, 246, 0.04) 0%, rgba(139, 92, 246, 0.02) 100%)',
              px: 1,
            }}
          >
            <TablePagination
              component="div"
              count={filteredAndSortedData.length}
              page={page}
              onPageChange={handleChangePage}
              rowsPerPage={rowsPerPage}
              onRowsPerPageChange={handleChangeRowsPerPage}
              rowsPerPageOptions={pageSizeOptions}
              labelRowsPerPage="Rows per page:"
              sx={{
                '& .MuiTablePagination-select': {
                  borderRadius: 1.5,
                  bgcolor: 'rgba(255, 255, 255, 0.03)',
                  '&:hover': {
                    bgcolor: 'rgba(255, 255, 255, 0.06)',
                  },
                },
                '& .MuiTablePagination-selectLabel, & .MuiTablePagination-displayedRows': {
                  fontSize: '0.8125rem',
                  color: 'rgba(255, 255, 255, 0.6)',
                },
                '& .MuiTablePagination-actions': {
                  gap: 1,
                  ml: 2,
                },
                '& .MuiIconButton-root': {
                  borderRadius: 1.5,
                  border: '1px solid',
                  borderColor: 'rgba(255, 255, 255, 0.1)',
                  bgcolor: 'rgba(255, 255, 255, 0.02)',
                  '&:hover': {
                    bgcolor: 'primary.main',
                    borderColor: 'primary.main',
                    color: 'white',
                    transform: 'scale(1.05)',
                  },
                  '&.Mui-disabled': {
                    opacity: 0.25,
                    borderColor: 'rgba(255, 255, 255, 0.05)',
                  },
                  transition: 'all 0.2s ease',
                },
              }}
              slotProps={{
                actions: {
                  previousButton: {
                    'aria-label': 'Previous page',
                  },
                  nextButton: {
                    'aria-label': 'Next page',
                  },
                },
              }}
            />
          </Box>
        )}
      </TableContainer>
    </Box>
  );
}
