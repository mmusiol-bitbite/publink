import type { FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import type { ContractExplorer } from '../hooks/useContractExplorer'

type Props = { explorer: ContractExplorer }

export function TimelineFilters({ explorer }: Props) {
  const { t } = useTranslation()
  const draft = explorer.filterDraft
  const invalidDateRange = Boolean(draft.from && draft.to && draft.from > draft.to)

  function submit(event: FormEvent) {
    event.preventDefault()
    if (invalidDateRange) return
    explorer.applyFilters()
  }

  return (
    <form className="timeline-filters" onSubmit={submit}>
      <strong>{t('filters.title')}</strong>
      <label>
        {t('filters.from')}
        <input
          type="datetime-local"
          value={draft.from}
          max={draft.to || undefined}
          onChange={(event) => explorer.setFilterDraft({ ...draft, from: event.target.value })}
        />
      </label>
      <label>
        {t('filters.to')}
        <input
          type="datetime-local"
          value={draft.to}
          min={draft.from || undefined}
          onChange={(event) => explorer.setFilterDraft({ ...draft, to: event.target.value })}
        />
      </label>
      <label>
        {t('filters.actor')}
        <input
          value={draft.actor}
          onChange={(event) => explorer.setFilterDraft({ ...draft, actor: event.target.value })}
          placeholder={t('filters.actorPlaceholder')}
          maxLength={200}
        />
      </label>
      <label>
        {t('filters.change')}
        <select
          value={draft.changeType}
          onChange={(event) =>
            explorer.setFilterDraft({ ...draft, changeType: event.target.value })
          }
        >
          <option value="">{t('filters.all')}</option>
          <option value="1">{t('change.added')}</option>
          <option value="2">{t('change.deleted')}</option>
          <option value="3">{t('change.modified')}</option>
        </select>
      </label>
      <label>
        {t('filters.entity')}
        <select
          value={draft.entityType}
          onChange={(event) =>
            explorer.setFilterDraft({ ...draft, entityType: event.target.value })
          }
        >
          <option value="">{t('filters.all')}</option>
          <option value="1">{t('entity.contractHeader')}</option>
          <option value="2">{t('entity.annexHeader')}</option>
          <option value="3">{t('entity.annexChange')}</option>
          <option value="4">{t('entity.file')}</option>
          <option value="5">{t('entity.invoice')}</option>
          <option value="6">{t('entity.paymentSchedule')}</option>
          <option value="7">{t('entity.contractFunding')}</option>
        </select>
      </label>
      {invalidDateRange && (
        <p className="state-message error" role="alert">
          {t('filters.dateRangeInvalid')}
        </p>
      )}
      <div className="filter-actions">
        <button className="secondary-button" type="button" onClick={explorer.resetFilters}>
          {t('filters.reset')}
        </button>
        <button className="primary-button" type="submit" disabled={invalidDateRange}>
          {t('filters.apply')}
        </button>
      </div>
    </form>
  )
}
