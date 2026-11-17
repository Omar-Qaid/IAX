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
    const measure = () => {
      if (rafId !== null) return;
      rafId = requestAnimationFrame(() => {
        rafId = null;
        const sb = Math.max(el.offsetWidth - el.clientWidth, SCROLLBAR_RESERVE);
        const w = el.offsetWidth - sb;
        setContainerWidth((prev) => (prev === w ? prev : w));
        setScrollbarWidth((prev) => (prev === sb ? prev : sb));
      });
    };
    // Watch the container and its content box (content height decides whether a
    // vertical scrollbar appears). Observing the whole subtree for mutations fired
    // on every virtual row mount during scroll and forced a layout read per frame.
    const ro = new ResizeObserver(measure);
    ro.observe(el);
    let content = el.firstElementChild;
    if (content) ro.observe(content);
    const mo = new MutationObserver(() => {
      if (el.firstElementChild === content) return;
      if (content) ro.unobserve(content);
      content = el.firstElementChild;
      if (content) ro.observe(content);
      measure();
    });
    mo.observe(el, { childList: true });
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
