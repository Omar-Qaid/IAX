import { useRef, useEffect, useCallback } from 'react';
import type React from 'react';

const LOAD_THRESHOLD_ROWS = 5;

interface UseLoadMoreOptions {
    rowCount: number;
    hasMore: boolean;
    loading: boolean;
    rowHeight: number;
    loadNextPage: () => void;
    scrollContainerRef: React.RefObject<HTMLDivElement | null>;
}

export function useLoadMore({
    rowCount,
    hasMore,
    loading,
    rowHeight,
    loadNextPage,
    scrollContainerRef,
}: UseLoadMoreOptions): { onScroll: () => void } {

    const guardRef = useRef(false);
    const hasMoreRef = useRef(hasMore);
    const rowHeightRef = useRef(rowHeight);
    const rowCountRef = useRef(rowCount);
    const loadNextPageRef = useRef(loadNextPage);

    useEffect(() => {
        hasMoreRef.current = hasMore;
        rowHeightRef.current = rowHeight;
        rowCountRef.current = rowCount;
        loadNextPageRef.current = loadNextPage;
    }, [hasMore, rowHeight, rowCount, loadNextPage]);

    useEffect(() => {
        if (!loading) guardRef.current = false;
    }, [loading, rowCount]);

    const tryLoad = useCallback((skipRowCountCheck = false) => {
        if (guardRef.current || !hasMoreRef.current) return;
        if (!skipRowCountCheck && rowCountRef.current === 0) return;

        const el = scrollContainerRef.current;
        if (!el) return;

        const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
        if (distanceFromBottom < rowHeightRef.current * LOAD_THRESHOLD_ROWS) {
            guardRef.current = true;
            loadNextPageRef.current();
        }
    }, [scrollContainerRef]);

    const onScroll = useCallback(() => tryLoad(false), [tryLoad]);

    const attachedElRef = useRef<HTMLDivElement | null>(null);
    const resizeObserverRef = useRef<ResizeObserver | null>(null);

    useEffect(() => {
        const el = scrollContainerRef.current;
        if (el === attachedElRef.current) return;

        if (attachedElRef.current) {
            attachedElRef.current.removeEventListener('scroll', onScroll);
        }
        resizeObserverRef.current?.disconnect();
        resizeObserverRef.current = null;

        attachedElRef.current = el;

        if (el) {
            el.addEventListener('scroll', onScroll, { passive: true });
            if (typeof ResizeObserver !== 'undefined') {
                const ro = new ResizeObserver(() => tryLoad(true));
                ro.observe(el);
                resizeObserverRef.current = ro;
            }
        }
    });

    useEffect(() => () => {
        attachedElRef.current?.removeEventListener('scroll', onScroll);
        resizeObserverRef.current?.disconnect();
        resizeObserverRef.current = null;
        attachedElRef.current = null;
    }, [onScroll]);

    useEffect(() => {
        if (!loading) tryLoad(true);
    }, [rowCount, loading, tryLoad]);

    return { onScroll };
}
