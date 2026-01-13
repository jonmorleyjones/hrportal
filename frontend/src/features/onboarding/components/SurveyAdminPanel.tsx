import { useState, useMemo } from 'react';
import { Button } from '@/components/ui/button';
import { motion, Skeleton } from '@/components/ui/motion';
import { DataTable, Column } from '@/components/ui/data-table';
import {
  useOnboardingSurveyAdmin,
  useUpdateOnboardingSurvey,
  useCreateOnboardingSurvey,
  useOnboardingResponses,
} from '../hooks/useOnboarding';
import {
  Save,
  FileJson,
  Users,
  CheckCircle2,
  Clock,
  AlertCircle,
  ChevronDown,
  ChevronUp,
} from 'lucide-react';
import { formatDate } from '@/lib/utils';
import type { OnboardingResponse } from '@/types';

const defaultSurveyJson = {
  title: 'Onboarding Survey',
  description: 'Please complete this short survey to help us personalize your experience.',
  pages: [
    {
      name: 'page1',
      title: 'About You',
      elements: [
        {
          type: 'text',
          name: 'role',
          title: 'What is your role?',
          isRequired: true,
        },
        {
          type: 'dropdown',
          name: 'teamSize',
          title: 'What is your team size?',
          choices: ['1-5', '6-20', '21-50', '51-200', '200+'],
        },
      ],
    },
  ],
  showProgressBar: 'top',
  completeText: 'Complete',
  showQuestionNumbers: 'off',
};

export function SurveyAdminPanel() {
  const { data: survey, isLoading: surveyLoading, error: surveyError } = useOnboardingSurveyAdmin();
  const { data: responses, isLoading: responsesLoading } = useOnboardingResponses();
  const updateSurvey = useUpdateOnboardingSurvey();
  const createSurvey = useCreateOnboardingSurvey();

  const [name, setName] = useState('');
  const [surveyJson, setSurveyJson] = useState('');
  const [isActive, setIsActive] = useState(true);
  const [jsonError, setJsonError] = useState<string | null>(null);
  const [showResponses, setShowResponses] = useState(false);

  // Initialize form with existing survey data
  useState(() => {
    if (survey) {
      setName(survey.name);
      setSurveyJson(survey.surveyJson);
      setIsActive(survey.isActive);
    } else if (!surveyLoading && surveyError) {
      // No survey exists, use default
      setName('Onboarding Survey');
      setSurveyJson(JSON.stringify(defaultSurveyJson, null, 2));
    }
  });

  // Update form when survey loads
  useMemo(() => {
    if (survey) {
      setName(survey.name);
      try {
        setSurveyJson(JSON.stringify(JSON.parse(survey.surveyJson), null, 2));
      } catch {
        setSurveyJson(survey.surveyJson);
      }
      setIsActive(survey.isActive);
    } else if (!surveyLoading && surveyError) {
      setName('Onboarding Survey');
      setSurveyJson(JSON.stringify(defaultSurveyJson, null, 2));
    }
  }, [survey, surveyLoading, surveyError]);

  const validateJson = (json: string): boolean => {
    try {
      JSON.parse(json);
      setJsonError(null);
      return true;
    } catch (e) {
      setJsonError(e instanceof Error ? e.message : 'Invalid JSON');
      return false;
    }
  };

  const handleSave = async () => {
    if (!validateJson(surveyJson)) return;

    try {
      if (survey) {
        await updateSurvey.mutateAsync({ name, surveyJson, isActive });
      } else {
        await createSurvey.mutateAsync({ name, surveyJson });
      }
    } catch (err) {
      console.error('Failed to save survey:', err);
    }
  };

  const completedCount = responses?.filter((r) => r.isComplete).length || 0;
  const inProgressCount = responses?.filter((r) => !r.isComplete).length || 0;

  const responseColumns: Column<OnboardingResponse>[] = [
    {
      key: 'userName',
      header: 'User',
      sortable: true,
      render: (item) => (
        <div className="flex items-center gap-3">
          <div
            className={`p-2 rounded-lg ${
              item.isComplete ? 'bg-green-500/10' : 'bg-yellow-500/10'
            }`}
          >
            {item.isComplete ? (
              <CheckCircle2 className="h-4 w-4 text-green-400" />
            ) : (
              <Clock className="h-4 w-4 text-yellow-400" />
            )}
          </div>
          <span className="font-medium">{item.userName}</span>
        </div>
      ),
    },
    {
      key: 'startedAt',
      header: 'Started',
      sortable: true,
      render: (item) => (
        <span className="text-muted-foreground">{formatDate(item.startedAt)}</span>
      ),
      getValue: (item) => new Date(item.startedAt).getTime(),
    },
    {
      key: 'completedAt',
      header: 'Completed',
      sortable: true,
      render: (item) => (
        <span className="text-muted-foreground">
          {item.completedAt ? formatDate(item.completedAt) : '—'}
        </span>
      ),
      getValue: (item) => item.completedAt ? new Date(item.completedAt).getTime() : 0,
    },
    {
      key: 'isComplete',
      header: 'Status',
      sortable: true,
      render: (item) => (
        <span
          className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ${
            item.isComplete
              ? 'bg-green-500/10 border border-green-500/20 text-green-400'
              : 'bg-yellow-500/10 border border-yellow-500/20 text-yellow-400'
          }`}
        >
          {item.isComplete ? 'Completed' : 'In Progress'}
        </span>
      ),
      getValue: (item) => item.isComplete ? 'Completed' : 'In Progress',
    },
  ];

  if (surveyLoading) {
    return (
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="glass rounded-xl p-6"
      >
        <Skeleton className="h-8 w-48 mb-4" />
        <Skeleton className="h-4 w-full mb-2" />
        <Skeleton className="h-64 w-full" />
      </motion.div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Survey Editor */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="glass rounded-xl overflow-hidden"
      >
        <div className="p-6 border-b border-border/30">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20">
              <FileJson className="h-5 w-5 text-primary" />
            </div>
            <div>
              <h3 className="font-semibold">Survey Configuration</h3>
              <p className="text-sm text-muted-foreground">
                Edit the survey JSON to customize questions
              </p>
            </div>
          </div>
        </div>

        <div className="p-6 space-y-4">
          {/* Survey Name */}
          <div>
            <label className="block text-sm font-medium mb-2">Survey Name</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full bg-background/50 border border-border rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-primary/50"
              placeholder="Enter survey name..."
            />
          </div>

          {/* Active Toggle */}
          <div className="flex items-center justify-between">
            <div>
              <label className="block text-sm font-medium">Survey Active</label>
              <p className="text-sm text-muted-foreground">
                When disabled, users won't see the survey
              </p>
            </div>
            <button
              onClick={() => setIsActive(!isActive)}
              className={`relative w-12 h-6 rounded-full transition-colors ${
                isActive ? 'bg-primary' : 'bg-muted'
              }`}
            >
              <motion.div
                className="absolute top-1 left-1 w-4 h-4 bg-white rounded-full"
                animate={{ x: isActive ? 24 : 0 }}
                transition={{ type: 'spring', stiffness: 500, damping: 30 }}
              />
            </button>
          </div>

          {/* JSON Editor */}
          <div>
            <label className="block text-sm font-medium mb-2">Survey JSON (SurveyJS format)</label>
            <textarea
              value={surveyJson}
              onChange={(e) => {
                setSurveyJson(e.target.value);
                if (jsonError) validateJson(e.target.value);
              }}
              onBlur={() => validateJson(surveyJson)}
              className={`w-full h-96 bg-background/50 border rounded-lg px-4 py-3 font-mono text-sm focus:outline-none focus:ring-2 focus:ring-primary/50 ${
                jsonError ? 'border-destructive' : 'border-border'
              }`}
              placeholder="Enter SurveyJS JSON configuration..."
            />
            {jsonError && (
              <div className="flex items-center gap-2 mt-2 text-sm text-destructive">
                <AlertCircle className="h-4 w-4" />
                {jsonError}
              </div>
            )}
          </div>

          {/* Save Button */}
          <div className="flex justify-end">
            <Button
              onClick={handleSave}
              disabled={updateSurvey.isPending || createSurvey.isPending || !!jsonError}
              className="gap-2 bg-gradient-to-r from-primary to-accent hover:opacity-90"
            >
              <Save className="h-4 w-4" />
              {updateSurvey.isPending || createSurvey.isPending ? 'Saving...' : 'Save Changes'}
            </Button>
          </div>
        </div>
      </motion.div>

      {/* Responses Table */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.1 }}
        className="glass rounded-xl overflow-hidden"
      >
        <button
          onClick={() => setShowResponses(!showResponses)}
          className="w-full p-6 flex items-center justify-between hover:bg-white/5 transition-colors"
        >
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-accent/10">
              <Users className="h-5 w-5 text-accent" />
            </div>
            <div className="text-left">
              <h3 className="font-semibold">Survey Responses</h3>
              <p className="text-sm text-muted-foreground">
                {responsesLoading
                  ? 'Loading...'
                  : `${completedCount} completed, ${inProgressCount} in progress`}
              </p>
            </div>
          </div>
          {showResponses ? (
            <ChevronUp className="h-5 w-5 text-muted-foreground" />
          ) : (
            <ChevronDown className="h-5 w-5 text-muted-foreground" />
          )}
        </button>

        {showResponses && (
          <div className="border-t border-border/30 p-6">
            {responsesLoading ? (
              <div className="space-y-4">
                {[...Array(3)].map((_, i) => (
                  <Skeleton key={i} className="h-16 w-full" />
                ))}
              </div>
            ) : (
              <DataTable
                data={responses || []}
                columns={responseColumns}
                keyField="id"
                searchPlaceholder="Search by user name..."
                filterOptions={{
                  key: 'isComplete',
                  label: 'Status',
                  options: [
                    { label: 'Completed', value: 'true' },
                    { label: 'In Progress', value: 'false' },
                  ],
                }}
                emptyState={
                  <div className="text-center py-8">
                    <Users className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
                    <p className="text-muted-foreground">No responses yet</p>
                  </div>
                }
              />
            )}
          </div>
        )}
      </motion.div>
    </div>
  );
}

export default SurveyAdminPanel;
