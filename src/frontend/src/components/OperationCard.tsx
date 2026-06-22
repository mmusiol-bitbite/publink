import { useTranslation } from 'react-i18next'
import type { TimelineItem } from '../api'
import type { Language } from '../hooks/useLanguage'
import { formatDate } from '../utils/formatDate'

type Props = {
  group: { correlationId: string; events: TimelineItem[] }
  language: Language
}

export function OperationCard({ group, language }: Props) {
  const { t } = useTranslation()

  return (
    <article className="operation-card">
      <div className="operation-header">
        <div>
          <span className="operation-id">
            {t('timeline.operation', { id: group.correlationId.slice(0, 8) })}
          </span>
          <strong>{formatDate(group.events[0]?.occurredAt, language, t('common.missing'))}</strong>
        </div>
        <span>{t('timeline.recorded', { count: group.events.length })}</span>
      </div>

      {group.events.map((event) => (
        <div className="event-block" key={event.eventId}>
          <div className="event-summary">
            <span className={`change-pill change-${event.changeKind}`}>
              {t(event.changeKind === 'unknown' ? 'change.unknown' : `change.${event.changeKind}`, {
                code: event.changeKindCode,
              })}
            </span>
            <h3>
              {t(event.entityKind === 'unknown' ? 'entity.unknown' : `entity.${event.entityKind}`, {
                code: event.entityKindCode,
              })}
            </h3>
            <span className="actor">{t('timeline.actor', { actor: event.actor })}</span>
          </div>

          {event.dataQualityIssues.length > 0 && (
            <div className="quality-warning" role="note">
              <strong>{t('quality.title')}</strong>
              <ul>
                {event.dataQualityIssues.map((issue) => (
                  <li key={issue}>{t(`quality.${issue}`)}</li>
                ))}
              </ul>
            </div>
          )}

          <div className="changes-table" role="table">
            <div className="changes-head" role="row">
              <span role="columnheader"></span>
              <span role="columnheader">{t('timeline.before')}</span>
              <span role="columnheader">{t('timeline.after')}</span>
            </div>
            {event.changes.map((change) => (
              <div className="change-row" role="row" key={change.field}>
                <strong role="cell">
                  {t(`fields.${change.field}`, { defaultValue: change.field })}
                </strong>
                <span role="cell" className="before-value">
                  {change.before ?? t('common.missing')}
                </span>
                <span role="cell" className="after-value">
                  {change.after ?? t('common.missing')}
                </span>
              </div>
            ))}
          </div>

          <details>
            <summary>{t('common.technicalDetails')}</summary>
            <code>
              {event.eventId} · seq {event.sourceSequence}
            </code>
          </details>
        </div>
      ))}
    </article>
  )
}
