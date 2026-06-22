import { afterEach, describe, expect, it, vi } from 'vitest'
import { api } from './api'

describe('api client', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('requests active contract search with encoded query and language header', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ items: [] }), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      }),
    )

    await api.searchContracts('UM/2026 & annex', 'en', 'active')

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/contracts/search?searchPhrase=UM%2F2026%20%26%20annex',
      { headers: { 'Accept-Language': 'en' } },
    )
  })

  it('builds archive timeline request with only meaningful filters and cursor', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          contractId: 'contract-1',
          snapshotSequence: 7,
          items: [],
          nextCursor: null,
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        },
      ),
    )

    await api.contractTimeline(
      'contract/1',
      {
        from: '2026-06-21T10:00:00.000Z',
        actor: 'auditor@example.gov.pl',
        changeType: 3,
      },
      'pl',
      'archive',
      'cursor-2',
    )

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/archive/contracts/contract%2F1/audit-events?from=2026-06-21T10%3A00%3A00.000Z&actor=auditor%40example.gov.pl&changeType=3&cursor=cursor-2',
      { headers: { 'Accept-Language': 'pl' } },
    )
  })

  it('throws status-coded error when backend rejects request', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response(null, { status: 503 }))

    await expect(api.synchronizationStatus('en')).rejects.toThrow('requestFailed:503')
  })

  it('builds export URL with locale and timeline filters', () => {
    const url = api.contractExportUrl(
      'contract-1',
      { to: '2026-06-22T12:00:00.000Z', entityType: 1 },
      'en',
      'active',
    )

    expect(url).toBe(
      '/api/v1/contracts/contract-1/audit-events/export?to=2026-06-22T12%3A00%3A00.000Z&entityType=1&locale=en',
    )
  })
})
