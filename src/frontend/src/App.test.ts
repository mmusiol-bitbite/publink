import type { TimelineItem } from './api'
import { isSynchronizationCompleted } from './utils/synchronization'
import { groupTimelineItems } from './utils/timeline'
import { describe, expect, it } from 'vitest'

const baseEvent: TimelineItem = {
  eventId: 'event-1',
  sourceSequence: 1,
  occurredAt: '2026-06-19T10:00:00Z',
  correlationId: 'operation-1',
  changeKind: 'modified',
  changeKindCode: 3,
  entityKind: 'contractHeader',
  entityKindCode: 1,
  actor: 'user@example.gov.pl',
  changes: [],
  dataQualityIssues: [],
}

describe('groupTimelineItems', () => {
  it('groups technical events into logical operations without changing order', () => {
    const result = groupTimelineItems([
      baseEvent,
      { ...baseEvent, eventId: 'event-2', sourceSequence: 2 },
      { ...baseEvent, eventId: 'event-3', correlationId: 'operation-2', sourceSequence: 3 },
    ])

    expect(result).toHaveLength(2)
    expect(result[0]?.events.map((event) => event.eventId)).toEqual(['event-1', 'event-2'])
    expect(result[1]?.correlationId).toBe('operation-2')
  })
})

describe('isSynchronizationCompleted', () => {
  it('waits until the successful checkpoint reaches the accepted command time', () => {
    expect(isSynchronizationCompleted('2026-06-20T10:00:00Z', '2026-06-20T10:00:01Z')).toBe(false)
    expect(isSynchronizationCompleted('2026-06-20T10:00:01Z', '2026-06-20T10:00:01Z')).toBe(true)
  })
})
