import React, { useEffect, useState } from 'react';
import { Fab, Zoom } from '@mui/material';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import { useTranslation } from 'react-i18next';

/** Appears once the user has scrolled this many viewports down. */
const SHOW_AFTER_VIEWPORTS = 2;

interface ScrollToTopButtonProps {
  scrollContainerRef: React.RefObject<HTMLDivElement | null>;
}

/**
 * Floating "back to top" control for long, infinitely scrolled grids and card
 * lists. It listens passively and only re-renders when its visibility flips.
 */
export function ScrollToTopButton({ scrollContainerRef }: ScrollToTopButtonProps) {
  const { t } = useTranslation();
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const el = scrollContainerRef.current;
    if (!el) return;
    let frame: number | null = null;
    const update = () => {
      frame = null;
      setVisible(el.scrollTop > el.clientHeight * SHOW_AFTER_VIEWPORTS);
    };
    const onScroll = () => {
      if (frame === null) frame = requestAnimationFrame(update);
    };
    el.addEventListener('scroll', onScroll, { passive: true });
    return () => {
      el.removeEventListener('scroll', onScroll);
      if (frame !== null) cancelAnimationFrame(frame);
    };
  }, [scrollContainerRef]);

  const label = t('grid.scroll_to_top');
  return (
    <Zoom in={visible} unmountOnExit>
      <Fab
        size="small"
        color="primary"
        aria-label={label}
        title={label}
        onClick={() => {
          const el = scrollContainerRef.current;
          if (!el) return;
          const reduceMotion =
            typeof window.matchMedia === 'function' &&
            window.matchMedia('(prefers-reduced-motion: reduce)').matches;
          el.scrollTo({ top: 0, behavior: reduceMotion ? 'auto' : 'smooth' });
        }}
        sx={{
          position: 'absolute',
          bottom: 16,
          insetInlineEnd: 24,
          zIndex: 4,
          boxShadow: 3,
        }}
      >
        <KeyboardArrowUpIcon />
      </Fab>
    </Zoom>
  );
}
