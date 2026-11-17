import type React from 'react';
import { useEffect, useId, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import {
  RECORD_PREVIEW_CLOSE_DELAY_MS,
  RECORD_PREVIEW_OPEN_DELAY_MS,
  RECORD_PREVIEW_STALE_TIME_MS,
  recordPreviewQueryKeys,
} from '../constants';
import { getRecordPreview } from '../recordPreviewRegistry';
import type { RecordPreviewRow } from '../types';

/**
 * Hover/focus behaviour and data for one previewed value: opens after a short
 * rest, stays open while the pointer is on the card, closes on leave, typing
 * or Esc, and loads the record only once the card opens (cached per record).
 */
export function useRecordPreview(entity: string, recordKey: string, preloaded?: unknown) {
  const { t, i18n, isRtl } = useAppTranslation();
  const definition = getRecordPreview(entity);
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const openTimer = useRef<number | undefined>(undefined);
  const closeTimer = useRef<number | undefined>(undefined);
  const id = useId();

  useEffect(
    () => () => {
      window.clearTimeout(openTimer.current);
      window.clearTimeout(closeTimer.current);
    },
    []
  );

  const query = useQuery({
    queryKey: recordPreviewQueryKeys.record(entity, recordKey),
    queryFn: ({ signal }) => definition!.load!(recordKey, signal),
    // A record the page already holds is shown as is, without a request.
    enabled: anchor !== null && preloaded === undefined && definition?.load !== undefined,
    staleTime: definition?.staleTimeMs ?? RECORD_PREVIEW_STALE_TIME_MS,
    retry: false,
  });

  const clearTimers = () => {
    window.clearTimeout(openTimer.current);
    window.clearTimeout(closeTimer.current);
  };
  const scheduleOpen = (element: HTMLElement) => {
    clearTimers();
    openTimer.current = window.setTimeout(() => {
      // An open dropdown list owns the space under the field.
      if (element.querySelector('[aria-expanded="true"]')) return;
      setAnchor(element);
    }, RECORD_PREVIEW_OPEN_DELAY_MS);
  };
  const scheduleClose = () => {
    clearTimers();
    closeTimer.current = window.setTimeout(() => setAnchor(null), RECORD_PREVIEW_CLOSE_DELAY_MS);
  };
  const close = () => {
    clearTimers();
    setAnchor(null);
  };

  const record = preloaded ?? query.data;
  const language = i18n.language;
  const rows: RecordPreviewRow[] =
    record && definition
      ? definition.fields.map((field) => ({
          label: t(field.labelKey, field.defaultLabel),
          value:
            typeof field.value === 'function'
              ? field.value(record as never, { isRtl, language })
              : (record as Record<string, unknown>)[field.value as string],
          lookup: field.lookup,
        }))
      : [];

  /** Spread on the element that wraps the previewed value. */
  const triggerProps = {
    'aria-describedby': anchor ? id : undefined,
    onMouseEnter: (event: React.MouseEvent<HTMLElement>) => scheduleOpen(event.currentTarget),
    onMouseLeave: scheduleClose,
    onFocus: (event: React.FocusEvent<HTMLElement>) => scheduleOpen(event.currentTarget),
    onBlur: close,
    onKeyDown: (event: React.KeyboardEvent) => {
      // Typing, opening a list or Esc hides the preview; Tab leaves via blur.
      if (event.key !== 'Tab' && event.key !== 'Shift') close();
    },
  };

  /** Spread on the card so moving the pointer onto it keeps it open. */
  const cardProps = {
    onMouseEnter: () => window.clearTimeout(closeTimer.current),
    onMouseLeave: scheduleClose,
  };

  return {
    id,
    anchor,
    open: anchor !== null,
    rows,
    loading: preloaded === undefined && query.isLoading,
    error: query.isError ? t('recordPreview.loadFailed', 'Details could not be loaded.') : null,
    emptyText: t('recordPreview.notFound', 'No details found.'),
    triggerProps,
    cardProps,
  };
}
