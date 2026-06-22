export function formatDate(
  value: string | null | undefined,
  language: 'pl' | 'en',
  missingValue: string,
) {
  if (!value) return missingValue

  return new Intl.DateTimeFormat(language === 'pl' ? 'pl-PL' : 'en-GB', {
    dateStyle: 'medium',
    timeStyle: 'medium',
    timeZone: 'Europe/Warsaw',
  }).format(new Date(value))
}
