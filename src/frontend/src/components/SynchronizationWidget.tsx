import { useTranslation } from 'react-i18next'
import { useSynchronization } from '../hooks/useSynchronization'
import type { Language } from '../hooks/useLanguage'
import { formatDate } from '../utils/formatDate'

type Props = { language: Language }

export function SynchronizationWidget({ language }: Props) {
  const { t } = useTranslation()
  const synchronization = useSynchronization(language)
  const status = synchronization.status?.status ?? 'initializing'
  const tooltipKey = synchronization.requestDisabled ? 'syncing' : status

  return (
    <aside className={`sync-card status-${status}`}>
      <div className="sync-card-header">
        <span className="sync-label">{t('sync.label')}</span>
        <button
          type="button"
          className="status-icon"
          aria-label={t(`sync.statusTooltip.${tooltipKey}`)}
          data-tooltip={t(`sync.statusTooltip.${tooltipKey}`)}
        />
      </div>
      <strong>{t(`sync.${status}`)}</strong>
      <span>
        {t('sync.last', {
          date: formatDate(
            synchronization.status?.lastSynchronizedAt,
            language,
            t('common.missing'),
          ),
        })}
      </span>
      <span>{t('sync.watermark', { value: synchronization.status?.lastSourceEventId ?? 0 })}</span>
      <span
        className={
          (synchronization.status?.deadLetterEventCount ?? 0) > 0 ? 'dlq-warning' : undefined
        }
      >
        {t('sync.deadLetters', {
          value: synchronization.status?.deadLetterEventCount ?? t('common.missing'),
        })}
      </span>
      {synchronization.notice ? (
        <div
          className={`sync-notice notice-${synchronization.notice}`}
          role={synchronization.notice === 'error' ? 'alert' : 'status'}
        >
          <span>
            {t(
              synchronization.notice === 'degraded'
                ? 'sync.completedDegraded'
                : `sync.${synchronization.notice}`,
            )}
          </span>
          <button
            type="button"
            className="sync-notice-close"
            aria-label={t('sync.dismiss')}
            onClick={synchronization.dismissNotice}
          >
            ×
          </button>
        </div>
      ) : (
        <button
          className="sync-button"
          type="button"
          disabled={synchronization.requestDisabled}
          onClick={synchronization.request}
          aria-label={t('sync.action')}
          title={t('sync.action')}
        >
          <svg aria-hidden="true" viewBox="0 0 24 24">
            <path d="M20 7v5h-5M4 17v-5h5" />
            <path d="M6.1 9a7 7 0 0 1 11.4-2.6L20 12M4 12l2.5 5.6A7 7 0 0 0 17.9 15" />
          </svg>
        </button>
      )}
    </aside>
  )
}

export function ArchiveWidget() {
  const { t } = useTranslation()

  return (
    <aside className="sync-card archive-card">
      <div className="sync-card-header">
        <span className="sync-label">{t('archive.label')}</span>
        <button
          type="button"
          className="status-icon"
          aria-label={t('archive.statusTooltip')}
          data-tooltip={t('archive.statusTooltip')}
        />
      </div>
      <strong>{t('archive.coldStorage')}</strong>
    </aside>
  )
}
