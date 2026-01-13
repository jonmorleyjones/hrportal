import { useState, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { motion, Skeleton, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import { SurveyRenderer } from './components/SurveyRenderer';
import { useRequestType, useSubmitRequestResponse } from './hooks/useRequests';
import { ArrowLeft, CheckCircle2, AlertCircle, Send } from 'lucide-react';
import { api } from '@/lib/api';
import {
  ClipboardList,
  UserPlus,
  Laptop,
  Calendar,
  FileText,
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

export function RequestFormPage() {
  const { typeId } = useParams<{ typeId: string }>();
  const [justSubmitted, setJustSubmitted] = useState(false);

  const { data: requestType, isLoading, error } = useRequestType(typeId || '');
  const submitResponse = useSubmitRequestResponse();

  const handleFormComplete = useCallback(
    async (data: Record<string, unknown>, fileIds: string[]) => {
      if (!typeId) return;
      try {
        const result = await submitResponse.mutateAsync({
          requestTypeId: typeId,
          responseJson: JSON.stringify(data),
          isComplete: true,
        });

        // Link uploaded files to the response
        if (fileIds.length > 0 && result.id) {
          try {
            await api.linkFilesToResponse(result.id, fileIds);
          } catch (linkErr) {
            console.error('Failed to link files to response:', linkErr);
            // Don't fail the submission if linking fails
          }
        }

        setJustSubmitted(true);
      } catch (err) {
        console.error('Failed to submit response:', err);
      }
    },
    [submitResponse, typeId]
  );

  const handleSubmitAnother = useCallback(() => {
    setJustSubmitted(false);
  }, []);

  const IconComponent = requestType ? (iconMap[requestType.icon] || ClipboardList) : ClipboardList;

  if (isLoading) {
    return (
      <div>
        <PageHeader
          title="Loading..."
          description="Please wait"
          actions={
            <Link to="/requests">
              <Button variant="outline" className="gap-2">
                <ArrowLeft className="h-4 w-4" />
                Back to Requests
              </Button>
            </Link>
          }
        />
        <div className="glass rounded-xl p-8">
          <Skeleton className="h-8 w-64 mb-4" />
          <Skeleton className="h-4 w-full mb-2" />
          <Skeleton className="h-4 w-3/4 mb-6" />
          <Skeleton className="h-48 w-full" />
        </div>
      </div>
    );
  }

  if (error || !requestType) {
    return (
      <div>
        <PageHeader
          title="Request Not Found"
          description="The requested form could not be found"
          actions={
            <Link to="/requests">
              <Button variant="outline" className="gap-2">
                <ArrowLeft className="h-4 w-4" />
                Back to Requests
              </Button>
            </Link>
          }
        />
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass rounded-xl p-8 text-center"
        >
          <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
          <h3 className="text-xl font-semibold mb-2">Unable to load form</h3>
          <p className="text-muted-foreground">
            The request type you're looking for doesn't exist or is no longer available.
          </p>
          <Link to="/requests">
            <Button className="mt-6 gap-2">
              <ArrowLeft className="h-4 w-4" />
              Back to Requests
            </Button>
          </Link>
        </motion.div>
      </div>
    );
  }

  return (
    <div>
      <PageHeader
        title={requestType.name}
        description={requestType.description || 'Fill out this form to submit your request'}
        actions={
          <Link to="/requests">
            <Button variant="outline" className="gap-2">
              <ArrowLeft className="h-4 w-4" />
              Back to Requests
            </Button>
          </Link>
        }
      />

      <StaggerContainer className="space-y-6">
        {justSubmitted ? (
          <StaggerItem>
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="glass rounded-xl p-8 text-center"
            >
              <motion.div
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                transition={{ type: 'spring', stiffness: 200, damping: 15, delay: 0.2 }}
                className="p-4 rounded-full bg-gradient-to-br from-primary/20 to-accent/20 w-fit mx-auto mb-4"
              >
                <CheckCircle2 className="h-12 w-12 text-primary" />
              </motion.div>
              <h3 className="text-2xl font-semibold mb-2 gradient-text">
                Request Submitted!
              </h3>
              <p className="text-muted-foreground max-w-md mx-auto mb-6">
                Thank you for submitting your request. Your response has been saved.
              </p>
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.5 }}
                className="flex gap-4 justify-center"
              >
                <Button
                  onClick={handleSubmitAnother}
                  variant="outline"
                  className="gap-2"
                >
                  <Send className="h-4 w-4" />
                  Submit Another
                </Button>
                <Link to="/requests">
                  <Button className="gap-2 bg-gradient-to-r from-primary to-accent hover:opacity-90">
                    <ArrowLeft className="h-4 w-4" />
                    Back to Requests
                  </Button>
                </Link>
              </motion.div>
            </motion.div>
          </StaggerItem>
        ) : (
          <StaggerItem>
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="glass rounded-xl p-6"
            >
              <div className="flex items-center gap-3 mb-6 pb-4 border-b border-border/30">
                <div className="p-2 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20">
                  <IconComponent className="h-5 w-5 text-primary" />
                </div>
                <div>
                  <h3 className="font-semibold">{requestType.name}</h3>
                  {requestType.description && (
                    <p className="text-sm text-muted-foreground">
                      {requestType.description}
                    </p>
                  )}
                </div>
              </div>

              <SurveyRenderer
                surveyJson={requestType.formJson}
                onComplete={handleFormComplete}
              />
            </motion.div>
          </StaggerItem>
        )}
      </StaggerContainer>
    </div>
  );
}

export default RequestFormPage;
