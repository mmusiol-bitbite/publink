import { useState } from 'react'
import type { StorageView } from './api'
import { AppHeader } from './components/AppHeader'
import { Footer } from './components/Footer'
import { HeroSection } from './components/HeroSection'
import { SearchPanel } from './components/SearchPanel'
import { TimelineSection } from './components/TimelineSection'
import { useContractExplorer } from './hooks/useContractExplorer'
import { useLanguage } from './hooks/useLanguage'

export default function App() {
  const [storage, setStorage] = useState<StorageView>('active')
  const { language, changeLanguage } = useLanguage()
  const explorer = useContractExplorer(storage, language)

  return (
    <div className="app-shell">
      <AppHeader
        storage={storage}
        language={language}
        onStorageChange={setStorage}
        onLanguageChange={(next) => void changeLanguage(next)}
      />
      <main id="top">
        <HeroSection storage={storage} language={language} />
        <SearchPanel explorer={explorer} storage={storage} language={language} />
        <TimelineSection explorer={explorer} storage={storage} language={language} />
      </main>
      <Footer />
    </div>
  )
}
