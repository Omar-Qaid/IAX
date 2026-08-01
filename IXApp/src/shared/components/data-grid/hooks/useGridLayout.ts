import { useState, useRef, useCallback, useLayoutEffect, useEffect } from 'react';

const SCROLLBAR_RESERVE = 8;

export function useGridLayout() {
  const [containerWidth, setContainerWidth] = useState<number>(1000);
  const [scrollbarWidth, setScrollbarWidth] = useState(SCROLLBAR_RESERVE);
  const scrollContainerRef = useRef<HTMLDivElement | null>(null);
  const headerScrollRef = useRef<HTMLDivElement | null>(null);

  const onScrollReset = useCallback(() => {
    if (scrollContainerRef.current) scrollContainerRef.current.scrollTop = 0;
    if (scrollContainerRef.current) scrollContainerRef.current.scrollLeft = 0;
    if (headerScrollRef.current) headerScrollRef.current.scrollLeft = 0;
  }, []);

  const handleBodyScroll = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    if (headerScrollRef.current) {
      headerScrollRef.current.scrollLeft = e.currentTarget.scrollLeft;
    }
  }, []);

  useLayoutEffect(() => {
    const el = scrollContainerRef.current;
    if (!el) return;

    const sb = Math.max(el.offsetWidth - el.clientWidth, SCROLLBAR_RESERVE);
    const width = el.offsetWidth - sb;
    if (width > 0) {
      setContainerWidth(width);
      setScrollbarWidth(sb);
    }
  }, []);

  useEffect(() => {
    const el = scrollContainerRef.current;
    if (!el) return;
    if (typeof ResizeObserver === 'undefined') return;
    let rafId: number | null = null;
    const ro = new ResizeObserver(() => {
      if (rafId !== null) cancelAnimationFrame(rafId);
      rafId = requestAnimationFrame(() => {
        rafId = null;
        const sb = Math.max(el.offsetWidth - el.clientWidth, SCROLLBAR_RESERVE);
        const w = el.offsetWidth - sb;
        setContainerWidth((prev) => (prev === w ? prev : w));
        setScrollbarWidth((prev) => (prev === sb ? prev : sb));
      });
    });
    ro.observe(el);
    const mo = new MutationObserver(() => {
      const sb = Math.max(el.offsetWidth - el.clientWidth, SCROLLBAR_RESERVE);
      const w = el.offsetWidth - sb;
      setContainerWidth((prev) => (prev === w ? prev : w));
      setScrollbarWidth((prev) => (prev === sb ? prev : sb));
    });
    mo.observe(el, { childList: true, subtree: true });
    return () => {
      if (rafId !== null) cancelAnimationFrame(rafId);
      ro.disconnect();
      mo.disconnect();
    };
  }, []);

  return {
    containerWidth,
    scrollbarWidth,
    scrollContainerRef,
    headerScrollRef,
    onScrollReset,
    handleBodyScroll,
  };
}
