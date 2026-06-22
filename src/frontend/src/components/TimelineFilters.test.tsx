import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import i18n from '../i18n'
import type { ContractExplorer, FilterDraft } from '../hooks/useContractExplorer'
import { emptyFilterDraft } from '../hooks/useContractExplorer'
import { TimelineFilters } from './TimelineFilters'

describe('TimelineFilters', () => {
  beforeEach(async () => {
    await i18n.changeLanguage('en')
  })

  it('blocks applying filters when date range is invalid', async () => {
    const user = userEvent.setup()
    const applyFilters = vi.fn()

    render(<TimelineFiltersHarness applyFilters={applyFilters} />)

    await user.type(screen.getByLabelText('From'), '2026-06-22T12:00')
    await user.type(screen.getByLabelText('To'), '2026-06-21T12:00')
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(screen.getByRole('alert')).toHaveTextContent(
      'The “From” date cannot be later than the “To” date.',
    )
    expect(screen.getByRole('button', { name: 'Apply' })).toBeDisabled()
    expect(applyFilters).not.toHaveBeenCalled()
  })

  it('applies filter draft chosen by the user', async () => {
    const user = userEvent.setup()
    const applyFilters = vi.fn()

    render(<TimelineFiltersHarness applyFilters={applyFilters} />)

    await user.type(screen.getByLabelText('Actor'), ' auditor@example.gov.pl ')
    await user.selectOptions(screen.getByLabelText('Change type'), '3')
    await user.selectOptions(screen.getByLabelText('Entity type'), '1')
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(applyFilters).toHaveBeenCalledWith({
      from: '',
      to: '',
      actor: ' auditor@example.gov.pl ',
      changeType: '3',
      entityType: '1',
    })
  })
})

function TimelineFiltersHarness({
  applyFilters,
}: Readonly<{ applyFilters: (draft: FilterDraft) => void }>) {
  const [draft, setDraft] = useState(emptyFilterDraft)
  const explorer = {
    filterDraft: draft,
    setFilterDraft: setDraft,
    applyFilters: () => applyFilters(draft),
    resetFilters: () => setDraft(emptyFilterDraft),
  } as ContractExplorer

  return <TimelineFilters explorer={explorer} />
}
