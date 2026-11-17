/** D365 opens the preview once the pointer or focus rests on the value. */
export const RECORD_PREVIEW_OPEN_DELAY_MS = 500;

/** Grace period to move the pointer from the field onto the card. */
export const RECORD_PREVIEW_CLOSE_DELAY_MS = 150;

/** How long a loaded record is reused before it is fetched again. */
export const RECORD_PREVIEW_STALE_TIME_MS = 5 * 60_000;

export const recordPreviewQueryKeys = {
  all: ['record-preview'] as const,
  record: (entity: string, key: string) => [...recordPreviewQueryKeys.all, entity, key] as const,
};
