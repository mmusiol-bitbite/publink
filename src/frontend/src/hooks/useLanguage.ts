import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'

export type Language = 'pl' | 'en'

export function useLanguage() {
  const { i18n } = useTranslation()
  const language: Language = i18n.language.startsWith('en') ? 'en' : 'pl'

  useEffect(() => {
    document.documentElement.lang = language
  }, [language])

  async function changeLanguage(next: Language) {
    localStorage.setItem('audit-language', next)
    await i18n.changeLanguage(next)
  }

  return { language, changeLanguage }
}
