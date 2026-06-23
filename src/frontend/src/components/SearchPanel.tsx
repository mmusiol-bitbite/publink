import type { FormEventHandler } from 'react'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { StorageView } from '../api'
import type { ContractExplorer } from '../hooks/useContractExplorer'
import type { Language } from '../hooks/useLanguage'
import { formatDate } from '../utils/formatDate'

type Props = Readonly<{ explorer: ContractExplorer; storage: StorageView; language: Language }>

export function SearchPanel({ explorer, storage, language }: Props) {
  const { t } = useTranslation()
  const [expandedFor, setExpandedFor] = useState<{ id: string | null; storage: StorageView } | null>(null)

  const results = useMemo(() => explorer.search.data ?? [], [explorer.search.data])
  const selectedId = explorer.selected?.contractId ?? null
  const isExpandedByUser =
    expandedFor !== null && expandedFor.id === selectedId && expandedFor.storage === storage
  const collapsedResult =
    selectedId && !isExpandedByUser
      ? results.find((contract) => contract.contractId === selectedId)
      : undefined
  const isCollapsed = Boolean(collapsedResult)
  const visibleResults = useMemo(
    () => (isCollapsed && collapsedResult ? [collapsedResult] : results),
    [collapsedResult, isCollapsed, results],
  )
  const canShowMoreResults = isCollapsed && results.length > 1

  const submit: FormEventHandler<HTMLFormElement> = (event) => {
    event.preventDefault()
    explorer.submitSearch()
  }

  return (
    <section className="search-panel" aria-labelledby="search-heading">
      <form onSubmit={submit}>
        <label id="search-heading" htmlFor="contract-search">
          {t(storage === 'archive' ? 'search.archiveLabel' : 'search.label')}
        </label>
        <div className="search-row">
          <input
            id="contract-search"
            value={explorer.input}
            onChange={(event) => explorer.setInput(event.target.value)}
            placeholder={t('search.placeholder')}
            minLength={2}
            maxLength={200}
            required
          />
          <button className="primary-button" type="submit">
            {t('search.action')}
          </button>
        </div>
      </form>

      <div className="search-results" aria-live="polite">
        {explorer.search.isFetching && <p className="state-message">{t('search.searching')}</p>}
        {explorer.search.isError && <p className="state-message error">{t('search.error')}</p>}
        {!explorer.search.isFetching && explorer.search.data?.length === 0 && (
          <p className="state-message">{t('search.empty')}</p>
        )}
        {visibleResults.map((contract) => (
          <button
            type="button"
            key={contract.contractId}
            className={`contract-result ${explorer.selected?.contractId === contract.contractId ? 'selected' : ''}`}
            onClick={() => {
              explorer.setSelected(contract)
            }}
          >
            <span className="contract-number">
              {contract.number ?? contract.internalNumber ?? contract.contractId}
            </span>
            <span>{contract.subject ?? t('common.missing')}</span>
            <span className="contractor">{contract.contractorName ?? t('common.missing')}</span>
            <span className="last-activity">
              {t('search.lastActivity', {
                date: formatDate(contract.lastActivityAt, language, t('common.missing')),
              })}
            </span>
            {contract.matchedHistoricalValue && (
              <span className="historical-badge">{t('search.historical')}</span>
            )}
          </button>
        ))}
        {canShowMoreResults && (
          <button
            className="search-more-button secondary-button"
            type="button"
            onClick={() => setExpandedFor({ id: selectedId, storage })}
          >
            {t('search.more')}
          </button>
        )}
      </div>
    </section>
  )
}
