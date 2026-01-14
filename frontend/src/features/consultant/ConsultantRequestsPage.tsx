import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import type { CrossTenantRequest } from '@/types';
import { useConsultantStore } from '@/stores/consultantStore';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { motion, Skeleton, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  ClipboardList,
  Building2,
  Search,
  Filter,
  Calendar,
  User,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';
import { formatDateTime } from '@/lib/utils';

export function ConsultantRequestsPage() {
  const { assignedTenants } = useConsultantStore();
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [tenantFilter, setTenantFilter] = useState<string>('all');
  const [page, setPage] = useState(1);
  const limit = 20;

  const { data: allRequests, isLoading } = useQuery({
    queryKey: ['consultant-requests'],
    queryFn: () => api.getConsultantCrossTenantRequests(),
  });

  // Client-side filtering
  const filteredRequests = useMemo(() => {
    if (!allRequests) return [];
    return allRequests.filter((request: CrossTenantRequest) => {
      // Status filter - convert isComplete to status
      if (statusFilter === 'Complete' && !request.isComplete) return false;
      if (statusFilter === 'InProgress' && request.isComplete) return false;
      if (tenantFilter !== 'all' && request.tenantId !== tenantFilter) return false;
      if (searchQuery) {
        const search = searchQuery.toLowerCase();
        return (
          request.requestTypeName?.toLowerCase().includes(search) ||
          request.userName?.toLowerCase().includes(search) ||
          request.tenantName?.toLowerCase().includes(search)
        );
      }
      return true;
    });
  }, [allRequests, statusFilter, tenantFilter, searchQuery]);

  const totalPages = Math.ceil(filteredRequests.length / limit) || 1;
  const paginatedRequests = filteredRequests.slice((page - 1) * limit, page * limit);
  const data = { requests: paginatedRequests, total: filteredRequests.length };

  return (
    <div>
      <PageHeader
        title="All Requests"
        description="View and manage requests across all your tenants"
      />

      {/* Filters */}
      <div className="glass rounded-xl p-4 mb-6">
        <div className="flex flex-wrap items-center gap-4">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              type="text"
              placeholder="Search requests..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9 bg-background/50"
            />
          </div>

          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-[150px] bg-background/50">
              <Filter className="h-4 w-4 mr-2" />
              <SelectValue placeholder="Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Statuses</SelectItem>
              <SelectItem value="InProgress">In Progress</SelectItem>
              <SelectItem value="Complete">Complete</SelectItem>
            </SelectContent>
          </Select>

          <Select value={tenantFilter} onValueChange={setTenantFilter}>
            <SelectTrigger className="w-[200px] bg-background/50">
              <Building2 className="h-4 w-4 mr-2" />
              <SelectValue placeholder="Tenant" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Tenants</SelectItem>
              {assignedTenants.map((tenant) => (
                <SelectItem key={tenant.id} value={tenant.id}>
                  {tenant.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Results */}
      {isLoading ? (
        <div className="space-y-4">
          {[...Array(5)].map((_, i) => (
            <Skeleton key={i} className="h-24 rounded-xl" />
          ))}
        </div>
      ) : data.requests.length === 0 ? (
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          className="text-center py-16"
        >
          <ClipboardList className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-semibold mb-2">No Requests Found</h3>
          <p className="text-muted-foreground">
            {searchQuery || statusFilter !== 'all' || tenantFilter !== 'all'
              ? 'Try adjusting your filters'
              : 'No requests have been submitted yet'}
          </p>
        </motion.div>
      ) : (
        <>
          <StaggerContainer className="space-y-3">
            {data.requests.map((request: CrossTenantRequest) => (
              <StaggerItem key={request.id}>
                <motion.div
                  whileHover={{ x: 4 }}
                  transition={{ duration: 0.2 }}
                  className="glass rounded-xl p-4 cursor-pointer group"
                >
                  <div className="flex items-center gap-4">
                    {/* Status indicator */}
                    <div className={`w-1 h-16 rounded-full ${
                      request.isComplete
                        ? 'bg-gradient-to-b from-green-500 to-emerald-500'
                        : 'bg-gradient-to-b from-yellow-500 to-amber-500'
                    }`} />

                    {/* Icon */}
                    <div className={`p-3 rounded-lg ${
                      request.isComplete
                        ? 'bg-green-500/10 text-green-500'
                        : 'bg-yellow-500/10 text-yellow-500'
                    }`}>
                      <ClipboardList className="h-5 w-5" />
                    </div>

                    {/* Main content */}
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <h3 className="font-semibold group-hover:text-amber-500 transition-colors">
                          {request.requestTypeName}
                        </h3>
                        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                          request.isComplete
                            ? 'bg-green-500/10 text-green-500'
                            : 'bg-yellow-500/10 text-yellow-500'
                        }`}>
                          {request.isComplete ? 'Complete' : 'In Progress'}
                        </span>
                      </div>
                      <div className="flex items-center gap-4 text-sm text-muted-foreground">
                        <div className="flex items-center gap-1">
                          <Building2 className="h-3 w-3" />
                          <span>{request.tenantName}</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <User className="h-3 w-3" />
                          <span>{request.userName}</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          <span>{formatDateTime(request.startedAt)}</span>
                        </div>
                      </div>
                    </div>

                    {/* Right side info */}
                    <div className="text-right">
                      <p className="text-sm font-medium">{request.userEmail}</p>
                      {request.completedAt && (
                        <p className="text-xs text-muted-foreground">
                          Completed {formatDateTime(request.completedAt)}
                        </p>
                      )}
                    </div>
                  </div>
                </motion.div>
              </StaggerItem>
            ))}
          </StaggerContainer>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between mt-6 pt-6 border-t border-border/30">
              <p className="text-sm text-muted-foreground">
                Showing {((page - 1) * limit) + 1} - {Math.min(page * limit, data.total)} of {data.total} requests
              </p>
              <div className="flex items-center gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage(p => Math.max(1, p - 1))}
                  disabled={page === 1}
                >
                  <ChevronLeft className="h-4 w-4" />
                  Previous
                </Button>
                <div className="flex items-center gap-1">
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum: number;
                    if (totalPages <= 5) {
                      pageNum = i + 1;
                    } else if (page <= 3) {
                      pageNum = i + 1;
                    } else if (page >= totalPages - 2) {
                      pageNum = totalPages - 4 + i;
                    } else {
                      pageNum = page - 2 + i;
                    }
                    return (
                      <Button
                        key={pageNum}
                        variant={page === pageNum ? 'default' : 'outline'}
                        size="sm"
                        onClick={() => setPage(pageNum)}
                        className={page === pageNum ? 'bg-amber-500 hover:bg-amber-600' : ''}
                      >
                        {pageNum}
                      </Button>
                    );
                  })}
                </div>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                >
                  Next
                  <ChevronRight className="h-4 w-4" />
                </Button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
