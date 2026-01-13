import { useState } from 'react';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { motion, Skeleton, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import { RequestTypeCard } from './components/RequestTypeCard';
import { RequestAdminPanel } from './components/RequestAdminPanel';
import { useRequestTypes } from './hooks/useRequests';
import { useAuthStore } from '@/stores/authStore';
import { ClipboardList, Settings2, AlertCircle } from 'lucide-react';

export function RequestsPage() {
  const { user } = useAuthStore();
  const isAdmin = user?.role === 'Admin';
  const [showAdminPanel, setShowAdminPanel] = useState(false);

  const { data: requestTypes, isLoading, error } = useRequestTypes();

  if (isLoading) {
    return (
      <div>
        <PageHeader
          title="Requests"
          description="Submit requests and forms"
        />
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <div key={i} className="glass rounded-xl p-6">
              <Skeleton className="h-12 w-12 rounded-lg mb-4" />
              <Skeleton className="h-6 w-3/4 mb-2" />
              <Skeleton className="h-4 w-full" />
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div>
        <PageHeader
          title="Requests"
          description="Submit requests and forms"
        />
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass rounded-xl p-8 text-center"
        >
          <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
          <h3 className="text-xl font-semibold mb-2">Unable to load requests</h3>
          <p className="text-muted-foreground">
            There was an error loading request types. Please try again later.
          </p>
        </motion.div>
      </div>
    );
  }

  return (
    <div>
      <PageHeader
        title="Requests"
        description="Submit requests and forms"
        actions={
          isAdmin && (
            <Button
              variant={showAdminPanel ? 'default' : 'outline'}
              onClick={() => setShowAdminPanel(!showAdminPanel)}
              className="gap-2"
            >
              <Settings2 className="h-4 w-4" />
              {showAdminPanel ? 'View Requests' : 'Manage Requests'}
            </Button>
          )
        }
      />

      <StaggerContainer className="space-y-6">
        {/* Admin Panel */}
        {isAdmin && showAdminPanel && (
          <StaggerItem>
            <RequestAdminPanel />
          </StaggerItem>
        )}

        {/* Request Type Cards */}
        {!showAdminPanel && (
          <>
            {!requestTypes || requestTypes.length === 0 ? (
              <StaggerItem>
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="glass rounded-xl p-8 text-center"
                >
                  <div className="p-4 rounded-full bg-muted/50 w-fit mx-auto mb-4">
                    <ClipboardList className="h-12 w-12 text-muted-foreground" />
                  </div>
                  <h3 className="text-xl font-semibold mb-2">No Request Types Available</h3>
                  <p className="text-muted-foreground max-w-md mx-auto">
                    There are no request types configured for your organization yet.
                    {isAdmin && ' Click "Manage Requests" to create one.'}
                  </p>
                  {isAdmin && (
                    <Button
                      className="mt-6 gap-2 bg-gradient-to-r from-primary to-accent hover:opacity-90"
                      onClick={() => setShowAdminPanel(true)}
                    >
                      <Settings2 className="h-4 w-4" />
                      Manage Requests
                    </Button>
                  )}
                </motion.div>
              </StaggerItem>
            ) : (
              <StaggerItem>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                  {requestTypes.map((requestType) => (
                    <RequestTypeCard key={requestType.id} requestType={requestType} />
                  ))}
                </div>
              </StaggerItem>
            )}
          </>
        )}
      </StaggerContainer>
    </div>
  );
}

export default RequestsPage;
