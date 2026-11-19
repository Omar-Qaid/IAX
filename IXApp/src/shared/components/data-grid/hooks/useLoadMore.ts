import { useRef, useEffect, useCallback } from 'react';
import type React from 'react';

/** Prefetch when the remaining content is shorter than this many rows... */
const LOAD_THRESHOLD_ROWS = 5;
/** ...or than this fraction of the viewport, whichever is larger (mobile cards are tall). */
const LOAD_THRESHOLD_VIEWPORTS = 1;
/** A request that never answers (no rows, no loading flag) stops blocking after this long. */
const REQUEST_TIMEOUT_MS = 15_000;
/** After a page attempt brings no rows, wait before trying again on the next scroll. */
const RETRY_BACKOFF_MS = 2_000;

interface UseLoadMoreOptions {
  rowCount: number;
  hasMore: boolean;
  loading: boolean;
  rowHeight: number;
  loadNextPage: () => void;
  scrollContainerRef: React.RefObject<HTMLDivElement | null>;
}

/**
 * Infinite scroll for the grid's scroll container (desktop rows and mobile cards).
 *
 * One page request is in flight at a time. The lock is released when rows arrive,
 * when `loading` turns false, or after a timeout. A page that brought no rows
 * (failed or empty) is retried only by a later user scroll after a back-off, never
 * automatically, so a failing endpoint is not called in a loop.
 */
export function useLoadMore({
  rowCount,
  hasMore,
  loading,
  rowHeight,
  loadNextPage,
  scrollContainerRef,
}: UseLoadMoreOptions): { onScroll: () => void } {
  const inFlightRef = useRef<{ rowCount: number; startedAt: number } | null>(null);
  const failedAtRef = useRef<{ rowCount: number; at: number } | null>(null);
  const hasMoreRef = useRef(hasMore);
  const loadingRef = useRef(loading);
  const rowHeightRef = useRef(rowHeight);
  const rowCountRef = useRef(rowCount);
  const loadNextPageRef = useRef(loadNextPage);

  useEffect(() => {
    hasMoreRef.current = hasMore;
    loadingRef.current = loading;
    rowHeightRef.current = rowHeight;
    rowCountRef.current = rowCount;
    loadNextPageRef.current = loadNextPage;
  }, [hasMore, loading, rowHeight, rowCount, loadNextPage]);

  // Release the lock when the request settles. Rows arriving means success;
  // loading ending with the same count means it failed or returned nothing.
  useEffect(() => {
    const request = inFlightRef.current;
    if (!request) return;
    if (rowCount !== request.rowCount) {
      inFlightRef.current = null;
      failedAtRef.current = null;
    } else if (!loading) {
      inFlightRef.current = null;
      failedAtRef.current = { rowCount, at: Date.now() };
    }
  }, [loading, rowCount]);

  const tryLoad = useCallback(
    (userScroll: boolean) => {
      if (!hasMoreRef.current || loadingRef.current) return;
      if (rowCountRef.current === 0) return; // the first page is the page's own query

      const now = Date.now();
      const request = inFlightRef.current;
      if (request) {
        if (now - request.startedAt < REQUEST_TIMEOUT_MS) return;
        inFlightRef.current = null;
        failedAtRef.current = { rowCount: request.rowCount, at: now };
      }
      const failed = failedAtRef.current;
      if (failed && failed.rowCount === rowCountRef.current) {
        if (!userScroll || now - failed.at < RETRY_BACKOFF_MS) return;
      }

      const el = scrollContainerRef.current;
      if (!el) return;
      const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
      const threshold = Math.max(
        rowHeightRef.current * LOAD_THRESHOLD_ROWS,
        el.clientHeight * LOAD_THRESHOLD_VIEWPORTS
      );
      if (distanceFromBottom < threshold) {
        inFlightRef.current = { rowCount: rowCountRef.current, startedAt: now };
        loadNextPageRef.current();
      }
    },
    [scrollContainerRef]
  );

  const onScroll = useCallback(() => tryLoad(true), [tryLoad]);

  // Short pages or a tall viewport leave no scrollbar: keep filling until it scrolls.
  useEffect(() => {
    const el = scrollContainerRef.current;
    if (!el || typeof ResizeObserver === 'undefined') return;
    const ro = new ResizeObserver(() => tryLoad(false));
    ro.observe(el);
    return () => ro.disconnect();
  }, [scrollContainerRef, tryLoad]);

  useEffect(() => {
    if (!loading) tryLoad(false);
  }, [rowCount, loading, hasMore, tryLoad]);

  return { onScroll };
}
