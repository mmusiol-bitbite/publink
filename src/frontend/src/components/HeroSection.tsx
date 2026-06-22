import { useTranslation } from 'react-i18next'
import type { StorageView } from '../api'
import type { Language } from '../hooks/useLanguage'
import { ArchiveWidget, SynchronizationWidget } from './SynchronizationWidget'

type Props = { storage: StorageView; language: Language }

export function HeroSection({ storage, language }: Props) {
  const { t } = useTranslation()

  return (
    <section className="hero">
      <div>
        <p className="eyebrow">{t('app.eyebrow')}</p>
        <h1>{t('app.title')}</h1>
        <p className="hero-copy">
          {t(storage === 'archive' ? 'app.archiveSubtitle' : 'app.subtitle')}
        </p>
      </div>
      {storage === 'active' ? <SynchronizationWidget language={language} /> : <ArchiveWidget />}
    </section>
  )
}
