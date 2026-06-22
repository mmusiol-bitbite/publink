export type SynchronizationStatus = {
  source: string
  lastSourceEventId: number
  lastSynchronizedAt: string | null
  status: 'initializing' | 'healthy' | 'degraded'
  deadLetterEventCount: number | null
  activeRequestId: string | null
  activeRequestAcceptedAt: string | null
}

export type SynchronizationRequest = {
  requestId: string
  source: string
  acceptedAt: string
  status: 'Accepted'
  joined: boolean
}

export type ContractSearchResult = {
  contractId: string
  number: string | null
  internalNumber: string | null
  subject: string | null
  contractorName: string | null
  lastActivityAt: string
  matchedHistoricalValue: boolean
}

export type StorageView = 'active' | 'archive'

export type TimelineFieldChange = {
  field: string
  before: string | null
  after: string | null
  valueKind: string
}

export type TimelineItem = {
  eventId: string
  sourceSequence: number
  occurredAt: string
  correlationId: string
  changeKind: string
  changeKindCode: number
  entityKind: string
  entityKindCode: number
  actor: string
  changes: TimelineFieldChange[]
  dataQualityIssues: string[]
}

export type TimelinePage = {
  contractId: string
  snapshotSequence: number
  synchronizedAt: string | null
  items: TimelineItem[]
  nextCursor: string | null
}

export type TimelineFilters = {
  from?: string
  to?: string
  actor?: string
  changeType?: number
  entityType?: number
}

async function getJson<T>(url: string, language: string): Promise<T> {
  const response = await fetch(url, {
    headers: { 'Accept-Language': language },
  })

  if (!response.ok) {
    throw new Error(`requestFailed:${response.status}`)
  }

  return response.json() as Promise<T>
}

async function postJson<T>(url: string, language: string): Promise<T> {
  const response = await fetch(url, {
    method: 'POST',
    headers: { 'Accept-Language': language },
  })
  if (!response.ok) throw new Error(`requestFailed:${response.status}`)
  return response.json() as Promise<T>
}

function contractsUrl(storage: StorageView) {
  return storage === 'archive' ? '/api/v1/archive/contracts' : '/api/v1/contracts'
}

function timelineParameters(filters: TimelineFilters, cursor?: string) {
  const parameters = new URLSearchParams()
  if (filters.from) parameters.set('from', filters.from)
  if (filters.to) parameters.set('to', filters.to)
  if (filters.actor) parameters.set('actor', filters.actor)
  if (filters.changeType !== undefined) parameters.set('changeType', String(filters.changeType))
  if (filters.entityType !== undefined) parameters.set('entityType', String(filters.entityType))
  if (cursor) parameters.set('cursor', cursor)
  return parameters
}

export const api = {
  synchronizationStatus: (language: string) =>
    getJson<SynchronizationStatus>('/api/v1/synchronization/status', language),

  requestSynchronization: (language: string) =>
    postJson<SynchronizationRequest>('/api/v1/synchronization/requests', language),

  searchContracts: async (query: string, language: string, storage: StorageView) => {
    const response = await getJson<{ items: ContractSearchResult[] }>(
      `${contractsUrl(storage)}/search?searchPhrase=${encodeURIComponent(query)}`,
      language,
    )
    return response.items
  },

  contractTimeline: (
    contractId: string,
    filters: TimelineFilters,
    language: string,
    storage: StorageView,
    cursor?: string,
  ) => {
    const parameters = timelineParameters(filters, cursor)
    const query = parameters.size > 0 ? `?${parameters.toString()}` : ''
    return getJson<TimelinePage>(
      `${contractsUrl(storage)}/${encodeURIComponent(contractId)}/audit-events${query}`,
      language,
    )
  },

  contractExportUrl: (
    contractId: string,
    filters: TimelineFilters,
    language: string,
    storage: StorageView,
  ) => {
    const parameters = timelineParameters(filters)
    parameters.set('locale', language)
    return `${contractsUrl(storage)}/${encodeURIComponent(contractId)}/audit-events/export?${parameters}`
  },
}
