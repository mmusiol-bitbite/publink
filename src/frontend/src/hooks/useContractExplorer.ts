import { useMemo, useState } from 'react'
import { useInfiniteQuery, useQuery } from '@tanstack/react-query'
import { api } from '../api'
import type { ContractSearchResult, StorageView, TimelineFilters } from '../api'
import { groupTimelineItems } from '../utils/timeline'
import type { Language } from './useLanguage'

export type FilterDraft = {
  from: string
  to: string
  actor: string
  changeType: string
  entityType: string
}

export const emptyFilterDraft: FilterDraft = {
  from: '',
  to: '',
  actor: '',
  changeType: '',
  entityType: '',
}

const initialInputs: Record<StorageView, string> = { active: 'a23', archive: 'test' }

export function useContractExplorer(storage: StorageView, language: Language) {
  const [inputs, setInputs] = useState(initialInputs)
  const [queries, setQueries] = useState(initialInputs)
  const [selections, setSelections] = useState<Record<StorageView, ContractSearchResult | null>>({
    active: null,
    archive: null,
  })
  const [drafts, setDrafts] = useState<Record<StorageView, FilterDraft>>({
    active: emptyFilterDraft,
    archive: emptyFilterDraft,
  })
  const [filtersByStorage, setFiltersByStorage] = useState<Record<StorageView, TimelineFilters>>({
    active: {},
    archive: {},
  })

  const input = inputs[storage]
  const query = queries[storage]
  const selected = selections[storage]
  const filterDraft = drafts[storage]
  const filters = filtersByStorage[storage]

  const search = useQuery({
    queryKey: ['contracts', storage, query, language],
    queryFn: () => api.searchContracts(query, language, storage),
    enabled: query.trim().length >= 2,
  })

  const timeline = useInfiniteQuery({
    queryKey: ['timeline', storage, selected?.contractId, filters, language],
    queryFn: ({ pageParam }) =>
      api.contractTimeline(selected!.contractId, filters, language, storage, pageParam),
    enabled: Boolean(selected),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  })

  const timelineItems = useMemo(
    () => timeline.data?.pages.flatMap((page) => page.items) ?? [],
    [timeline.data?.pages],
  )
  const groupedTimeline = useMemo(() => groupTimelineItems(timelineItems), [timelineItems])

  function updateForStorage<T>(
    setter: React.Dispatch<React.SetStateAction<Record<StorageView, T>>>,
    value: T,
  ) {
    setter((current) => ({ ...current, [storage]: value }))
  }

  return {
    input,
    setInput: (value: string) => updateForStorage<string>(setInputs, value),
    submitSearch: () => {
      const next = input.trim()
      if (next.length >= 2) {
        updateForStorage<string>(setQueries, next)
        updateForStorage<ContractSearchResult | null>(setSelections, null)
      }
    },
    selected,
    setSelected: (value: ContractSearchResult) =>
      updateForStorage<ContractSearchResult | null>(setSelections, value),
    search,
    filterDraft,
    setFilterDraft: (value: FilterDraft) => updateForStorage<FilterDraft>(setDrafts, value),
    filters,
    applyFilters: () =>
      updateForStorage<TimelineFilters>(setFiltersByStorage, {
        from: filterDraft.from ? new Date(filterDraft.from).toISOString() : undefined,
        to: filterDraft.to ? new Date(filterDraft.to).toISOString() : undefined,
        actor: filterDraft.actor.trim() || undefined,
        changeType: filterDraft.changeType ? Number(filterDraft.changeType) : undefined,
        entityType: filterDraft.entityType ? Number(filterDraft.entityType) : undefined,
      }),
    resetFilters: () => {
      updateForStorage<FilterDraft>(setDrafts, emptyFilterDraft)
      updateForStorage<TimelineFilters>(setFiltersByStorage, {})
    },
    timeline,
    timelineItems,
    groupedTimeline,
  }
}

export type ContractExplorer = ReturnType<typeof useContractExplorer>
