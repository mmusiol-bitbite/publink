import { useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { api } from '../api'
import { isSynchronizationCompleted } from '../utils/synchronization'
import type { Language } from './useLanguage'

export type SynchronizationNotice = 'requested' | 'joined' | 'completed' | 'degraded' | 'error'

export function useSynchronization(language: Language) {
  const [acceptedAt, setAcceptedAt] = useState<string | null>(null)
  const [requestId, setRequestId] = useState<string | null>(null)
  const [noticeDismissed, setNoticeDismissed] = useState(false)
  const [joinedExistingRequest, setJoinedExistingRequest] = useState(false)

  const status = useQuery({
    queryKey: ['synchronization', language],
    queryFn: () => api.synchronizationStatus(language),
    refetchInterval: (query) =>
      requestId && query.state.data?.activeRequestId === requestId ? 2_000 : 30_000,
  })

  const request = useMutation({
    mutationFn: () => api.requestSynchronization(language),
    onMutate: () => {
      setAcceptedAt(null)
      setRequestId(null)
      setNoticeDismissed(false)
      setJoinedExistingRequest(false)
    },
    onSuccess: (result) => {
      setAcceptedAt(result.acceptedAt)
      setRequestId(result.requestId)
      setJoinedExistingRequest(result.joined)
      void status.refetch()
    },
  })

  const completed = Boolean(
    requestId &&
    status.data &&
    status.data.activeRequestId !== requestId &&
    isSynchronizationCompleted(status.data.lastSynchronizedAt, acceptedAt),
  )
  const inProgress = Boolean(requestId) && !completed
  const notice: SynchronizationNotice | null = request.isError
    ? 'error'
    : request.isPending || inProgress
      ? joinedExistingRequest
        ? 'joined'
        : 'requested'
      : acceptedAt && completed
        ? status.data?.status === 'degraded'
          ? 'degraded'
          : 'completed'
        : null

  return {
    status: status.data,
    request: () => request.mutate(),
    requestDisabled: request.isPending || inProgress,
    notice: noticeDismissed ? null : notice,
    dismissNotice: () => setNoticeDismissed(true),
  }
}
