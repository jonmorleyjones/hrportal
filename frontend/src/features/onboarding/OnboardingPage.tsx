import { useState, useCallback } from 'react';
import { PageHeader } from '@/components/shared/PageHeader';
import { Button } from '@/components/ui/button';
import { motion, Skeleton, StaggerContainer, StaggerItem } from '@/components/ui/motion';
import { SurveyRenderer } from './components/SurveyRenderer';
import { SurveyAdminPanel } from './components/SurveyAdminPanel';
import { useOnboardingStatus, useSubmitOnboardingResponse } from './hooks/useOnboarding';
import { useAuthStore } from '@/stores/authStore';
import { ClipboardList, CheckCircle2, Settings2, AlertCircle, UserPlus } from 'lucide-react';

export function OnboardingPage() {
  const { user } = useAuthStore();
  const isAdmin = user?.role === 'Admin';
  const [showAdminPanel, setShowAdminPanel] = useState(false);
  const [justSubmitted, setJustSubmitted] = useState(false);

  const { data: status, isLoading, error } = useOnboardingStatus();
  const submitResponse = useSubmitOnboardingResponse();

  const handleSurveyComplete = useCallback(
    async (data: Record<string, unknown>) => {
      try {
        await submitResponse.mutateAsync({
          responseJson: JSON.stringify(data),
          isComplete: true,
        });
        setJustSubmitted(true);
      } catch (err) {
        console.error('Failed to submit new starter:', err);
      }
    },
    [submitResponse]
  );

  const handleSubmitAnother = useCallback(() => {
    setJustSubmitted(false);
  }, []);

  if (isLoading) {
    return (
      <div>
        <PageHeader
          title="Onboarding"
          description="Complete your onboarding survey"
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

  if (error) {
    return (
      <div>
        <PageHeader
          title="Onboarding"
          description="Complete your onboarding survey"
        />
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass rounded-xl p-8 text-center"
        >
          <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
          <h3 className="text-xl font-semibold mb-2">Unable to load survey</h3>
          <p className="text-muted-foreground">
            There was an error loading the onboarding survey. Please try again later.
          </p>
        </motion.div>
      </div>
    );
  }

  return (
    <div>
      <PageHeader
        title="Onboarding"
        description="Complete your onboarding survey"
        actions={
          isAdmin && (
            <Button
              variant={showAdminPanel ? 'default' : 'outline'}
              onClick={() => setShowAdminPanel(!showAdminPanel)}
              className="gap-2"
            >
              <Settings2 className="h-4 w-4" />
              {showAdminPanel ? 'View Survey' : 'Configure Survey'}
            </Button>
          )
        }
      />

      <StaggerContainer className="space-y-6">
        {/* Admin Panel */}
        {isAdmin && showAdminPanel && (
          <StaggerItem>
            <SurveyAdminPanel />
          </StaggerItem>
        )}

        {/* Survey Content */}
        {!showAdminPanel && (
          <>
            {!status?.hasSurvey ? (
              <StaggerItem>
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="glass rounded-xl p-8 text-center"
                >
                  <div className="p-4 rounded-full bg-muted/50 w-fit mx-auto mb-4">
                    <ClipboardList className="h-12 w-12 text-muted-foreground" />
                  </div>
                  <h3 className="text-xl font-semibold mb-2">No Survey Available</h3>
                  <p className="text-muted-foreground max-w-md mx-auto">
                    There is no onboarding survey configured for your organization yet.
                    {isAdmin && ' Click "Configure Survey" to set one up.'}
                  </p>
                  {isAdmin && (
                    <Button
                      className="mt-6 gap-2 bg-gradient-to-r from-primary to-accent hover:opacity-90"
                      onClick={() => setShowAdminPanel(true)}
                    >
                      <Settings2 className="h-4 w-4" />
                      Configure Survey
                    </Button>
                  )}
                </motion.div>
              </StaggerItem>
            ) : justSubmitted ? (
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
                    New Starter Submitted!
                  </h3>
                  <p className="text-muted-foreground max-w-md mx-auto mb-6">
                    Thank you for submitting the new starter details. The information has been saved.
                  </p>
                  <motion.div
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: 0.5 }}
                  >
                    <Button
                      onClick={handleSubmitAnother}
                      className="gap-2 bg-gradient-to-r from-primary to-accent hover:opacity-90"
                    >
                      <UserPlus className="h-4 w-4" />
                      Submit Another New Starter
                    </Button>
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
                      <UserPlus className="h-5 w-5 text-primary" />
                    </div>
                    <div>
                      <h3 className="font-semibold">{status.survey?.name || 'New Starter Form'}</h3>
                      <p className="text-sm text-muted-foreground">
                        Submit details for a new starter
                      </p>
                    </div>
                  </div>

                  {status.survey && (
                    <SurveyRenderer
                      surveyJson={status.survey.surveyJson}
                      onComplete={handleSurveyComplete}
                    />
                  )}
                </motion.div>
              </StaggerItem>
            )}
          </>
        )}
      </StaggerContainer>
    </div>
  );
}

export default OnboardingPage;
