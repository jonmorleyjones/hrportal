import { useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Model } from 'survey-core';
import { Survey } from 'survey-react-ui';
import 'survey-core/survey-core.min.css';
import { useHrConsultantAuthStore } from '@/stores/hrConsultantAuthStore';
import { api } from '@/lib/api';
import { PageHeader } from '@/components/shared/PageHeader';
import { motion } from '@/components/ui/motion';
import { auroraGlassTheme } from '@/features/requests/surveyjs-theme';
import '@/features/requests/surveyjs-overrides.css';
import {
  ArrowLeft,
  CheckCircle2,
  Clock,
  AlertCircle,
  User,
  Calendar,
  FileText
} from 'lucide-react';

export function HrResponseDetailPage() {
  const { responseId } = useParams<{ responseId: string }>();
  const navigate = useNavigate();
  const { selectedTenant } = useHrConsultantAuthStore();

  const { data: response, isLoading } = useQuery({
    queryKey: ['hr-response-detail', selectedTenant?.tenantId, responseId],
    queryFn: () => api.getHrTenantResponseDetail(selectedTenant!.tenantId, responseId!),
    enabled: !!selectedTenant && !!responseId,
  });

  const surveyModel = useMemo(() => {
    if (!response) return null;

    try {
      const model = new Model(JSON.parse(response.formJson));
      model.applyTheme(auroraGlassTheme);
      model.mode = 'display'; // Read-only mode
      model.data = JSON.parse(response.responseJson);
      return model;
    } catch (error) {
      console.error('Failed to parse form or response:', error);
      return null;
    }
  }, [response]);

  if (!selectedTenant) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
        <h2 className="text-xl font-semibold mb-2">No Tenant Selected</h2>
        <p className="text-muted-foreground">
          Please select a tenant from the sidebar.
        </p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="text-center py-12">
        <div className="animate-spin h-8 w-8 border-2 border-primary border-t-transparent rounded-full mx-auto mb-2" />
        <p className="text-muted-foreground">Loading response...</p>
      </div>
    );
  }

  if (!response) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
        <h2 className="text-xl font-semibold mb-2">Response Not Found</h2>
        <p className="text-muted-foreground">
          The requested response could not be found.
        </p>
      </div>
    );
  }

  return (
    <div>
      <button
        onClick={() => navigate('/hr/responses')}
        className="flex items-center gap-2 text-muted-foreground hover:text-foreground mb-4 transition-colors"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Responses
      </button>

      <PageHeader
        title={
          <div className="flex items-center gap-3">
            <span className="text-2xl">{response.requestTypeIcon || '📋'}</span>
            <span>{response.requestTypeName}</span>
            <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${
              response.isComplete
                ? 'bg-green-500/10 text-green-400'
                : 'bg-yellow-500/10 text-yellow-400'
            }`}>
              {response.isComplete ? <CheckCircle2 className="h-3 w-3" /> : <Clock className="h-3 w-3" />}
              {response.isComplete ? 'Complete' : 'In Progress'}
            </span>
          </div>
        }
        description={`Response from ${response.userName}`}
      />

      {/* Metadata */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="glass rounded-xl p-6 mb-6"
      >
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-primary/10">
              <User className="h-4 w-4 text-primary" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Submitted by</p>
              <p className="font-medium">{response.userName}</p>
              <p className="text-xs text-muted-foreground">{response.userEmail}</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-primary/10">
              <FileText className="h-4 w-4 text-primary" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Form Version</p>
              <p className="font-medium">v{response.versionNumber}</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-primary/10">
              <Calendar className="h-4 w-4 text-primary" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Started</p>
              <p className="font-medium">{new Date(response.startedAt).toLocaleDateString()}</p>
              <p className="text-xs text-muted-foreground">
                {new Date(response.startedAt).toLocaleTimeString()}
              </p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-primary/10">
              <CheckCircle2 className="h-4 w-4 text-primary" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Completed</p>
              {response.completedAt ? (
                <>
                  <p className="font-medium">{new Date(response.completedAt).toLocaleDateString()}</p>
                  <p className="text-xs text-muted-foreground">
                    {new Date(response.completedAt).toLocaleTimeString()}
                  </p>
                </>
              ) : (
                <p className="font-medium text-muted-foreground">Not yet</p>
              )}
            </div>
          </div>
        </div>
      </motion.div>

      {/* Survey Response */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.1 }}
        className="glass rounded-xl p-6"
      >
        <h3 className="text-lg font-semibold mb-4">Response Details</h3>

        {surveyModel ? (
          <div className="survey-container">
            <Survey model={surveyModel} />
          </div>
        ) : (
          <div className="text-center py-8">
            <AlertCircle className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
            <p className="text-muted-foreground">Failed to load form response</p>
          </div>
        )}
      </motion.div>
    </div>
  );
}
