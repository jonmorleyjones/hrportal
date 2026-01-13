import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import type { OnboardingStatus, OnboardingSurvey, OnboardingResponse } from '@/types';

export function useOnboardingStatus() {
  return useQuery<OnboardingStatus>({
    queryKey: ['onboarding', 'status'],
    queryFn: () => api.getOnboardingStatus(),
  });
}

export function useOnboardingSurvey() {
  return useQuery<OnboardingSurvey>({
    queryKey: ['onboarding', 'survey'],
    queryFn: () => api.getOnboardingSurvey(),
  });
}

export function useOnboardingResponse() {
  return useQuery<OnboardingResponse>({
    queryKey: ['onboarding', 'response'],
    queryFn: () => api.getOnboardingResponse(),
    retry: false,
  });
}

export function useSubmitOnboardingResponse() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ responseJson, isComplete }: { responseJson: string; isComplete: boolean }) =>
      api.submitOnboardingResponse(responseJson, isComplete),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['onboarding'] });
    },
  });
}

export function useResetOnboardingResponse() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => api.resetOnboardingResponse(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['onboarding'] });
    },
  });
}

// Admin hooks
export function useOnboardingSurveyAdmin() {
  return useQuery<OnboardingSurvey>({
    queryKey: ['onboarding', 'admin', 'survey'],
    queryFn: () => api.getOnboardingSurveyAdmin(),
    retry: false,
  });
}

export function useCreateOnboardingSurvey() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ name, surveyJson }: { name: string; surveyJson: string }) =>
      api.createOnboardingSurvey(name, surveyJson),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['onboarding'] });
    },
  });
}

export function useUpdateOnboardingSurvey() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      name,
      surveyJson,
      isActive,
    }: {
      name: string;
      surveyJson: string;
      isActive: boolean;
    }) => api.updateOnboardingSurvey(name, surveyJson, isActive),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['onboarding'] });
    },
  });
}

export function useOnboardingResponses() {
  return useQuery<OnboardingResponse[]>({
    queryKey: ['onboarding', 'admin', 'responses'],
    queryFn: () => api.getOnboardingResponses(),
  });
}
