import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import type { RequestTypeCard, RequestType, RequestResponse, CreateRequestTypeRequest, UpdateRequestTypeRequest, FileInfo } from '@/types';

// User hooks
export function useRequestTypes() {
  return useQuery<RequestTypeCard[]>({
    queryKey: ['requests', 'types'],
    queryFn: () => api.getRequestTypes(),
  });
}

export function useRequestType(id: string) {
  return useQuery<RequestType>({
    queryKey: ['requests', 'types', id],
    queryFn: () => api.getRequestType(id),
    enabled: !!id,
  });
}

export function useUserRequestResponses() {
  return useQuery<RequestResponse[]>({
    queryKey: ['requests', 'responses'],
    queryFn: () => api.getUserRequestResponses(),
  });
}

export function useSubmitRequestResponse() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ requestTypeId, responseJson, isComplete }: { requestTypeId: string; responseJson: string; isComplete: boolean }) =>
      api.submitRequestResponse(requestTypeId, responseJson, isComplete),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['requests'] });
    },
  });
}

// Admin hooks
export function useRequestTypesAdmin() {
  return useQuery<RequestType[]>({
    queryKey: ['requests', 'admin', 'types'],
    queryFn: () => api.getRequestTypesAdmin(),
  });
}

export function useCreateRequestType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateRequestTypeRequest) => api.createRequestType(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['requests'] });
    },
  });
}

export function useUpdateRequestType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateRequestTypeRequest }) =>
      api.updateRequestType(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['requests'] });
    },
  });
}

export function useDeleteRequestType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => api.deleteRequestType(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['requests'] });
    },
  });
}

export function useRequestTypeResponses(requestTypeId: string) {
  return useQuery<RequestResponse[]>({
    queryKey: ['requests', 'admin', 'types', requestTypeId, 'responses'],
    queryFn: () => api.getRequestTypeResponses(requestTypeId),
    enabled: !!requestTypeId,
  });
}

export function useAllRequestResponses() {
  return useQuery<RequestResponse[]>({
    queryKey: ['requests', 'admin', 'responses'],
    queryFn: () => api.getAllRequestResponses(),
  });
}

export function useResponseFiles(responseId: string | null) {
  return useQuery<FileInfo[]>({
    queryKey: ['requests', 'admin', 'responses', responseId, 'files'],
    queryFn: () => api.getResponseFiles(responseId!),
    enabled: !!responseId,
  });
}
