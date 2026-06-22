import { useTranslation } from 'react-i18next'
import { api } from '../api'
import type { StorageView } from '../api'
import type { ContractExplorer } from '../hooks/useContractExplorer'
import type { Language } from '../hooks/useLanguage'
import { OperationCard } from './OperationCard'
import { TimelineFilters } from './TimelineFilters'

type Props = { explorer: ContractExplorer; storage: StorageView; language: Language }

export function TimelineSection({ explorer, storage, language }: Props) {
  const { t } = useTranslation()
  const selected = explorer.selected
  if (!selected) return null

  return (
    <section className="timeline-section" aria-labelledby="timeline-heading">
      <div className="section-heading">
        <div>
          <p className="eyebrow">{t('timeline.title')}</p>
          <h2 id="timeline-heading">
            {selected.number ?? selected.internalNumber ?? selected.subject ?? selected.contractId}
          </h2>
        </div>
        <div className="heading-actions">
          {explorer.timeline.data?.pages[0] && (
            <span className="snapshot-badge">
              {t('timeline.snapshot', {
                value: explorer.timeline.data.pages[0].snapshotSequence,
              })}
            </span>
          )}
          <a
            className="export-button"
            href={api.contractExportUrl(selected.contractId, explorer.filters, language, storage)}
          >
            {t('export.action')}
          </a>
        </div>
      </div>

      <TimelineFilters explorer={explorer} />

      {explorer.timeline.isFetching && <div className="empty-card">{t('timeline.loading')}</div>}
      {explorer.timeline.isError && (
        <div className="empty-card state-message error" role="alert">
          {t('timeline.error')}
        </div>
      )}
      {!explorer.timeline.isFetching &&
        !explorer.timeline.isError &&
        explorer.timelineItems.length === 0 && (
          <div className="empty-card">{t('timeline.empty')}</div>
        )}

      <div className="timeline">
        {explorer.groupedTimeline.map((group) => (
          <OperationCard key={group.correlationId} group={group} language={language} />
        ))}
      </div>
      {explorer.timeline.hasNextPage && (
        <button
          className="load-more-button"
          type="button"
          disabled={explorer.timeline.isFetchingNextPage}
          onClick={() => void explorer.timeline.fetchNextPage()}
        >
          {explorer.timeline.isFetchingNextPage
            ? t('timeline.loadingMore')
            : t('timeline.loadMore')}
        </button>
      )}
    </section>
  )
}
