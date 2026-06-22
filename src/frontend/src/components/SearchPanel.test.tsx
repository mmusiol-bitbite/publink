import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { ContractSearchResult } from '../api'
import i18n from '../i18n'
import type { ContractExplorer } from '../hooks/useContractExplorer'
import { SearchPanel } from './SearchPanel'

const contracts: ContractSearchResult[] = [
  {
    contractId: 'contract-1',
    number: 'UM/001',
    internalNumber: null,
    subject: 'Road maintenance',
    contractorName: 'Publink Services',
    lastActivityAt: '2026-06-22T10:00:00Z',
    matchedHistoricalValue: false,
  },
  {
    contractId: 'contract-2',
    number: 'UM/002',
    internalNumber: null,
    subject: 'Bridge inspection',
    contractorName: 'Audit Works',
    lastActivityAt: '2026-06-21T10:00:00Z',
    matchedHistoricalValue: false,
  },
]

const archiveContracts: ContractSearchResult[] = [
  {
    contractId: 'archive-contract-1',
    number: 'UM/A01',
    internalNumber: null,
    subject: 'Archived maintenance',
    contractorName: 'Archive Services',
    lastActivityAt: '2025-01-10T10:00:00Z',
    matchedHistoricalValue: false,
  },
  {
    contractId: 'archive-contract-2',
    number: 'UM/A02',
    internalNumber: null,
    subject: 'Archived inspection',
    contractorName: 'Archive Works',
    lastActivityAt: '2024-11-18T10:00:00Z',
    matchedHistoricalValue: false,
  },
]

describe('SearchPanel', () => {
  beforeEach(async () => {
    await i18n.changeLanguage('en')
  })

  it('collapses results to the selected contract and expands them again', async () => {
    const user = userEvent.setup()

    render(<SearchPanelHarness />)

    expect(screen.getByText('UM/001')).toBeInTheDocument()
    expect(screen.getByText('UM/002')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /UM\/001/i }))

    expect(screen.getByText('UM/001')).toBeInTheDocument()
    expect(screen.queryByText('UM/002')).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'More' }))

    expect(screen.getByText('UM/001')).toBeInTheDocument()
    expect(screen.getByText('UM/002')).toBeInTheDocument()
  })

  it('keeps active results collapsed after switching to archive and back', async () => {
    const user = userEvent.setup()

    render(<SearchPanelStorageSwitchHarness />)

    await user.click(screen.getByRole('button', { name: /UM\/001/i }))
    expect(screen.queryByText('UM/002')).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Archive' }))
    await user.click(screen.getByRole('button', { name: 'Active' }))

    expect(screen.getByText('UM/001')).toBeInTheDocument()
    expect(screen.queryByText('UM/002')).not.toBeInTheDocument()
  })

  it('keeps archive results collapsed after switching to active and back', async () => {
    const user = userEvent.setup()

    render(<SearchPanelStorageSwitchHarness />)

    await user.click(screen.getByRole('button', { name: 'Archive' }))
    await user.click(screen.getByRole('button', { name: /UM\/A01/i }))
    expect(screen.queryByText('UM/A02')).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Active' }))
    await user.click(screen.getByRole('button', { name: 'Archive' }))

    expect(screen.getByText('UM/A01')).toBeInTheDocument()
    expect(screen.queryByText('UM/A02')).not.toBeInTheDocument()
  })
})

function SearchPanelHarness() {
  const [selected, setSelected] = useState<ContractSearchResult | null>(null)
  const explorer = {
    input: 'UM',
    setInput: vi.fn(),
    submitSearch: vi.fn(),
    selected,
    setSelected,
    search: {
      isFetching: false,
      isError: false,
      data: contracts,
    },
  } as unknown as ContractExplorer

  return <SearchPanel explorer={explorer} storage="active" language="en" />
}

function SearchPanelStorageSwitchHarness() {
  const [storage, setStorage] = useState<'active' | 'archive'>('active')
  const [selections, setSelections] = useState<{
    active: ContractSearchResult | null
    archive: ContractSearchResult | null
  }>({
    active: null,
    archive: null,
  })

  const searchData = storage === 'active' ? contracts : archiveContracts
  const selected = selections[storage]
  const explorer = {
    input: 'UM',
    setInput: vi.fn(),
    submitSearch: vi.fn(),
    selected,
    setSelected: (value: ContractSearchResult) =>
      setSelections((current) => ({ ...current, [storage]: value })),
    search: {
      isFetching: false,
      isError: false,
      data: searchData,
    },
  } as unknown as ContractExplorer

  return (
    <>
      <button type="button" onClick={() => setStorage('active')}>
        Active
      </button>
      <button type="button" onClick={() => setStorage('archive')}>
        Archive
      </button>
      <SearchPanel explorer={explorer} storage={storage} language="en" />
    </>
  )
}