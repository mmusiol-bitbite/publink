import { useTranslation } from 'react-i18next'
import type { StorageView } from '../api'
import type { Language } from '../hooks/useLanguage'

type Props = {
  storage: StorageView
  language: Language
  onStorageChange: (storage: StorageView) => void
  onLanguageChange: (language: Language) => void
}

export function AppHeader({ storage, language, onStorageChange, onLanguageChange }: Props) {
  const { t } = useTranslation()

  return (
    <header className="topbar">
      <a className="brand" href="#top" aria-label="Audit Explorer">
        <span className="brand-mark" aria-hidden="true">
          AE
        </span>
        <span>Audit Explorer</span>
      </a>
      <nav className="storage-tabs" aria-label={t('navigation.label')}>
        {(['active', 'archive'] as const).map((item) => (
          <button
            key={item}
            type="button"
            className={storage === item ? 'active' : ''}
            aria-current={storage === item ? 'page' : undefined}
            onClick={() => onStorageChange(item)}
          >
            {t(`navigation.${item}`)}
          </button>
        ))}
      </nav>
      <div className="language-switch" aria-label={t('language.label')}>
        {(['pl', 'en'] as const).map((code) => (
          <button
            key={code}
            className={language === code ? 'active' : ''}
            onClick={() => onLanguageChange(code)}
            type="button"
            aria-pressed={language === code}
          >
            {t(`language.${code}`)}
          </button>
        ))}
      </div>
    </header>
  )
}
