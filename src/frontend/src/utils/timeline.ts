import type { TimelineItem } from '../api'

export function groupTimelineItems(items: TimelineItem[]) {
  const groups = new Map<string, TimelineItem[]>()
  for (const item of items) {
    const existing = groups.get(item.correlationId) ?? []
    existing.push(item)
    groups.set(item.correlationId, existing)
  }
  return [...groups.entries()].map(([correlationId, events]) => ({ correlationId, events }))
}
