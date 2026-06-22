export function isSynchronizationCompleted(
  lastSynchronizedAt: string | null | undefined,
  acceptedAt: string | null,
) {
  return Boolean(
    acceptedAt &&
    lastSynchronizedAt &&
    new Date(lastSynchronizedAt).getTime() >= new Date(acceptedAt).getTime(),
  )
}
