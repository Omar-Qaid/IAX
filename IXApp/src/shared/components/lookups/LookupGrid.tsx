import React, { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react';
import {
  Box,
  IconButton,
  InputAdornment,
  Paper,
  Portal,
  ClickAwayListener,
  TextField,
  CircularProgress,
  Typography,
  Divider,
  Button,
} from '@mui/material';
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown';
import CloseIcon from '@mui/icons-material/Close';
import SearchIcon from '@mui/icons-material/Search';
import { useVirtualizer } from '@tanstack/react-virtual';
import { useTranslation } from 'react-i18next';

import type { GridLookupProps, GridLookupColumn } from './types';
import { useLookupGridField } from '@shared/hooks/useLookupGridField';

const DEFAULT_PAGE_SIZE = 50;
const DEFAULT_ROW_HEIGHT = 36;
const HEADER_HEIGHT = 36;

export function LookupGrid<T extends Record<string, any>>({
  value,
  displayText,
  onChange,
  columns,
  fetchPage,
  queryKey,
  valueField = 'id' as keyof T,
  labelField: _labelField = 'name' as keyof T,
  label,
  placeholder,
  error,
  disabled,
  required: _required,
  fullWidth = true,
  size = 'small',
  pageSize = DEFAULT_PAGE_SIZE,
  rowHeight = DEFAULT_ROW_HEIGHT,
  popupWidth,
  popupMaxHeight = 360,
  searchDebounceMs = 300,
  showClearButton = true,
  actions,
}: GridLookupProps<T>) {
  const { t } = useTranslation();
  const anchorRef = useRef<HTMLDivElement | null>(null);
  const scrollRef = useRef<HTMLDivElement | null>(null);
  const searchInputRef = useRef<HTMLInputElement | null>(null);

  const [open, setOpen] = useState(false);
  const [searchInput, setSearchInput] = useState('');
  const [activeIndex, setActiveIndex] = useState(-1);

  const {
    rows,
    totalRecords,
    debouncedSearch,
    isLoading,
    isFetching,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
    refetch,
  } = useLookupGridField<T>({
    queryKey,
    fetchPage,
    enabled: open,
    pageSize,
    search: searchInput,
    debounceMs: searchDebounceMs,
  });

  const rowCount = rows.length + (hasNextPage ? 1 : 0);

  // TanStack Virtual returns mutable functions by design; React Compiler safely skips this component.
  // eslint-disable-next-line react-hooks/incompatible-library
  const virtualizer = useVirtualizer({
    count: rowCount,
    getScrollElement: () => scrollRef.current,
    estimateSize: () => rowHeight,
    overscan: 8,
  });

  useEffect(() => {
    const items = virtualizer.getVirtualItems();
    const last = items[items.length - 1];
    if (!last) return;
    if (last.index >= rows.length - 1 && hasNextPage && !isFetchingNextPage) {
      fetchNextPage();
    }
  // Virtual items are maintained by the virtualizer outside React state.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [virtualizer.getVirtualItems(), rows.length, hasNextPage, isFetchingNextPage, fetchNextPage]);

  useEffect(() => {
    setActiveIndex(rows.length ? 0 : -1);
  }, [debouncedSearch, rows.length]);

  useEffect(() => {
    if (open && rows.length === 0 && !isFetching && !isLoading) {
      refetch();
    }
  }, [open, rows.length, isFetching, isLoading, refetch]);

  useLayoutEffect(() => {
    if (!open) return;
    const raf = requestAnimationFrame(() => {
      virtualizer.measure();
    });
    return () => cancelAnimationFrame(raf);
  }, [open, virtualizer]);

  const handleOpen = useCallback(() => {
    if (disabled) return;
    setOpen(true);
    window.setTimeout(() => searchInputRef.current?.focus(), 0);
  }, [disabled]);

  const handleClose = useCallback(() => setOpen(false), []);

  const handleSelect = useCallback(
    (row: T) => {
      onChange((row[valueField] ?? null) as T[keyof T] | null, row);
      handleClose();
    },
    [onChange, valueField, handleClose]
  );

  const handleClear = useCallback(
    (e: React.MouseEvent) => {
      e.stopPropagation();
      onChange(null, null);
      setSearchInput('');
    },
    [onChange]
  );

  const scrollActiveIntoView = useCallback(
    (index: number) => {
      if (index < 0) return;
      virtualizer.scrollToIndex(index, { align: 'auto' });
    },
    [virtualizer]
  );

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (!open) {
        if (e.key === 'ArrowDown' || e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          handleOpen();
        }
        return;
      }
      switch (e.key) {
        case 'ArrowDown':
          e.preventDefault();
          setActiveIndex((i) => {
            const next = Math.min(rows.length - 1, i + 1);
            scrollActiveIntoView(next);
            return next;
          });
          break;
        case 'ArrowUp':
          e.preventDefault();
          setActiveIndex((i) => {
            const next = Math.max(0, i - 1);
            scrollActiveIntoView(next);
            return next;
          });
          break;
        case 'Home':
          e.preventDefault();
          setActiveIndex(0);
          scrollActiveIntoView(0);
          break;
        case 'End':
          e.preventDefault();
          setActiveIndex(rows.length - 1);
          scrollActiveIntoView(rows.length - 1);
          break;
        case 'PageDown':
          e.preventDefault();
          setActiveIndex((i) => {
            const next = Math.min(rows.length - 1, i + 10);
            scrollActiveIntoView(next);
            return next;
          });
          break;
        case 'PageUp':
          e.preventDefault();
          setActiveIndex((i) => {
            const next = Math.max(0, i - 10);
            scrollActiveIntoView(next);
            return next;
          });
          break;
        case 'Enter':
          e.preventDefault();
          if (activeIndex >= 0 && rows[activeIndex]) handleSelect(rows[activeIndex]);
          break;
        case 'Escape':
          e.preventDefault();
          handleClose();
          break;
      }
    },
    [open, rows, activeIndex, handleOpen, handleClose, handleSelect, scrollActiveIntoView]
  );

  const resolvedDisplay = displayText ?? (value != null ? String(value) : '');
  const totalColumnFlex = columns.reduce((s, c) => s + (c.flex ?? 0), 0);

  const columnStyle = (col: GridLookupColumn<T>): React.CSSProperties => {
    if (col.width != null) return { width: col.width, flex: '0 0 auto' };
    if (col.flex != null) return { flex: `${col.flex} ${col.flex} 0`, minWidth: 0 };
    if (totalColumnFlex === 0) return { flex: '1 1 0', minWidth: 0 };
    return { flex: '0 0 auto', minWidth: 0 };
  };

  const FLEX_COL_MIN = 120;
  const contentMinWidth = columns.reduce((sum, c) => {
    if (typeof c.width === 'number') return sum + c.width;
    if (typeof c.width === 'string') return sum + FLEX_COL_MIN;
    return sum + FLEX_COL_MIN;
  }, 0);

  const renderCell = (col: GridLookupColumn<T>, row: T) => {
    if (col.render) return col.render(row);
    const v = (row as any)[col.field];
    return v == null ? '' : String(v);
  };

  const [pos, setPos] = useState<{
    top: number;
    left: number;
    width: number;
    maxHeight: number;
    placementY: 'bottom' | 'top';
    placementX: 'left' | 'right';
  }>({
    top: 0,
    left: 0,
    width: 0,
    maxHeight: popupMaxHeight,
    placementY: 'bottom',
    placementX: 'left',
  });

  const popupRef = useRef<HTMLDivElement | null>(null);

  const recalcPosition = useCallback(() => {
    const anchor = anchorRef.current;
    if (!anchor) return;
    const rect = anchor.getBoundingClientRect();
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const margin = 8;
    const gap = 4;
    const minHeightFloor = 160;
    const minWidthFloor = 280;
    const comfortableBelow = 240;

    const desiredWidth =
      typeof popupWidth === 'number'
        ? popupWidth
        : typeof popupWidth === 'string'
        ? rect.width
        : Math.max(rect.width, 320);
    const width = Math.max(minWidthFloor, Math.min(desiredWidth, vw - margin * 2));

    let left = rect.left;
    let placementX: 'left' | 'right' = 'left';

    if (left + width > vw - margin) {
      const rightAligned = rect.right - width;
      if (rightAligned >= margin) {
        left = rightAligned;
        placementX = 'right';
      } else {
        left = Math.max(margin, vw - width - margin);
        placementX = 'right';
      }
    }
    if (left < margin) left = margin;

    const spaceBelow = vh - rect.bottom - gap - margin;
    const spaceAbove = rect.top - gap - margin;
    const needed = popupMaxHeight;

    let placementY: 'bottom' | 'top' = 'bottom';
    let top = rect.bottom + gap;
    let maxHeight = Math.min(needed, spaceBelow);

    const comfortBelow = Math.min(needed, comfortableBelow);
    const shouldFlipUp = spaceBelow < comfortBelow && spaceAbove > spaceBelow;

    if (shouldFlipUp) {
      placementY = 'top';
      maxHeight = Math.max(minHeightFloor, Math.min(needed, spaceAbove));
      const actualH = popupRef.current?.offsetHeight ?? 0;
      const renderedH = actualH > 0 ? Math.min(actualH, maxHeight) : maxHeight;
      top = rect.top - gap - renderedH;
    } else {
      maxHeight = Math.max(minHeightFloor, Math.min(needed, spaceBelow));
      top = rect.bottom + gap;
    }

    maxHeight = Math.max(minHeightFloor, maxHeight);
    const popupH = popupRef.current?.offsetHeight ?? maxHeight;
    const clampH = Math.min(popupH, maxHeight);
    if (top + clampH > vh - margin) top = Math.max(margin, vh - margin - clampH);
    if (top < margin) top = margin;

    setPos((prev) =>
      prev.top === top &&
      prev.left === left &&
      prev.width === width &&
      prev.maxHeight === maxHeight &&
      prev.placementY === placementY &&
      prev.placementX === placementX
        ? prev
        : { top, left, width, maxHeight, placementY, placementX }
    );
  }, [popupWidth, popupMaxHeight]);

  useLayoutEffect(() => {
    if (!open) return;
    recalcPosition();

    let rafId: number | null = null;
    const schedule = () => {
      if (rafId != null) return;
      rafId = requestAnimationFrame(() => {
        rafId = null;
        recalcPosition();
      });
    };

    window.addEventListener('resize', schedule);
    window.addEventListener('scroll', schedule, true);
    window.addEventListener('orientationchange', schedule);

    const ro = typeof ResizeObserver !== 'undefined' ? new ResizeObserver(schedule) : null;
    if (ro) {
      if (anchorRef.current) ro.observe(anchorRef.current);
      if (popupRef.current) ro.observe(popupRef.current);
      ro.observe(document.documentElement);
    }

    const initialRaf = requestAnimationFrame(schedule);

    return () => {
      window.removeEventListener('resize', schedule);
      window.removeEventListener('scroll', schedule, true);
      window.removeEventListener('orientationchange', schedule);
      if (ro) ro.disconnect();
      if (rafId != null) cancelAnimationFrame(rafId);
      cancelAnimationFrame(initialRaf);
    };
  }, [open, recalcPosition]);

  useEffect(() => {
    if (open) recalcPosition();
  }, [open, rows.length, recalcPosition]);

  return (
    <Box sx={{ width: fullWidth ? '100%' : undefined }}>
      <TextField
        ref={anchorRef}
        label={label ? t(label) : undefined}
        fullWidth={fullWidth}
        size={size}
        disabled={disabled}
        error={!!error}
        helperText={error ? t(error) : undefined}
        value={resolvedDisplay}
        placeholder={placeholder ? t(placeholder) : undefined}
        onClick={handleOpen}
        onKeyDown={handleKeyDown}
        slotProps={{
          input: {
            readOnly: true,
            endAdornment: (
              <InputAdornment position="end">
                {showClearButton && value != null && !disabled && (
                  <IconButton
                    size="small"
                    onClick={handleClear}
                    aria-label={t('common.clear') === 'common.clear' ? 'Clear' : t('common.clear')}
                  >
                    <CloseIcon fontSize="small" />
                  </IconButton>
                )}
                <IconButton
                  size="small"
                  onClick={(e) => {
                    e.stopPropagation();
                    if (open) handleClose();
                    else handleOpen();
                  }}
                  disabled={disabled}
                  aria-label={t('common.open') === 'common.open' ? 'Open' : t('common.open')}
                >
                  <ArrowDropDownIcon
                    sx={{
                      transition: 'transform 120ms',
                      transform: open ? 'rotate(180deg)' : 'none',
                    }}
                  />
                </IconButton>
              </InputAdornment>
            ),
          },
        }}
        sx={{ bgcolor: 'background.paper', cursor: disabled ? 'not-allowed' : 'pointer' }}
      />

      {open && (
        <Portal>
          <ClickAwayListener onClickAway={handleClose}>
            <Paper
              ref={popupRef}
              elevation={8}
              sx={{
                position: 'fixed',
                top: pos.top,
                left: pos.left,
                width: pos.width,
                maxHeight: pos.maxHeight,
                zIndex: 1500,
                overflow: 'hidden',
                display: 'flex',
                flexDirection: 'column',
                minHeight: 0,
                transformOrigin: `${pos.placementY === 'top' ? 'bottom' : 'top'} ${pos.placementX}`,
                border: '1px solid',
                borderColor: 'divider',
                borderTopWidth: pos.placementY === 'bottom' ? 2 : 1,
                borderBottomWidth: pos.placementY === 'top' ? 2 : 1,
                borderTopColor: pos.placementY === 'bottom' ? 'primary.main' : 'divider',
                borderBottomColor: pos.placementY === 'top' ? 'primary.main' : 'divider',
              }}
            >
              <Box sx={{ p: 1 }}>
                <TextField
                  inputRef={searchInputRef}
                  size="small"
                  fullWidth
                  placeholder={t('common.search') === 'common.search' ? 'Search...' : t('common.search')}
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                  onKeyDown={handleKeyDown}
                  slotProps={{
                    input: {
                      startAdornment: (
                        <InputAdornment position="start">
                          <SearchIcon fontSize="small" />
                        </InputAdornment>
                      ),
                      endAdornment: searchInput ? (
                        <InputAdornment position="end">
                          <IconButton size="small" onClick={() => setSearchInput('')}>
                            <CloseIcon fontSize="small" />
                          </IconButton>
                        </InputAdornment>
                      ) : undefined,
                    },
                  }}
                />
              </Box>
              <Divider />

              <Box
                ref={scrollRef}
                sx={{
                  position: 'relative',
                  maxHeight: popupMaxHeight,
                  overflow: 'auto',
                  flex: '1 1 auto',
                  minHeight: 0,
                }}
              >
                <Box sx={{ minWidth: contentMinWidth, width: '100%' }}>
                  <Box
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      height: HEADER_HEIGHT,
                      px: 1,
                      bgcolor: 'grey.100',
                      fontSize: 12,
                      fontWeight: 600,
                      color: 'text.secondary',
                      borderBottom: '1px solid',
                      borderColor: 'divider',
                      position: 'sticky',
                      top: 0,
                      zIndex: 2,
                      minWidth: contentMinWidth,
                    }}
                  >
                    {columns.map((col) => (
                      <Box
                        key={String(col.field)}
                        sx={{
                          ...columnStyle(col),
                          px: 1,
                          textAlign: col.align ?? 'left',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          whiteSpace: 'nowrap',
                        }}
                      >
                        {t(col.header)}
                      </Box>
                    ))}
                  </Box>
                  {isLoading ? (
                    <Box sx={{ p: 3, textAlign: 'center' }}>
                      <CircularProgress size={22} />
                    </Box>
                  ) : rows.length === 0 ? (
                    <Box sx={{ p: 3, textAlign: 'center', color: 'text.disabled', fontSize: 13 }}>
                      {t('common.noResults') || 'No results'}
                    </Box>
                  ) : (
                    <Box sx={{ height: virtualizer.getTotalSize(), width: '100%', position: 'relative', minWidth: contentMinWidth }}>
                      {(virtualizer.getVirtualItems().length > 0
                        ? virtualizer.getVirtualItems()
                        : rows.map((_, index) => ({
                            index,
                            key: index,
                            start: index * rowHeight,
                            size: rowHeight,
                          }))
                      ).map((vRow) => {
                        const isLoaderRow = vRow.index >= rows.length;
                        const row = rows[vRow.index];
                        const selected = !isLoaderRow && row && row[valueField] === value;
                        const active = vRow.index === activeIndex;
                        return (
                          <Box
                            key={vRow.key}
                            onMouseEnter={() => setActiveIndex(vRow.index)}
                            onClick={() => row && handleSelect(row)}
                            sx={{
                              position: 'absolute',
                              top: 0,
                              left: 0,
                              right: 0,
                              transform: `translateY(${vRow.start}px)`,
                              height: vRow.size,
                              display: 'flex',
                              alignItems: 'center',
                              px: 1,
                              fontSize: 13,
                              cursor: 'pointer',
                              bgcolor: selected
                                ? 'primary.lighter'
                                : active
                                ? 'action.hover'
                                : 'transparent',
                              color: selected ? 'primary.main' : 'inherit',
                              borderLeft: '3px solid',
                              borderLeftColor: selected ? 'primary.main' : 'transparent',
                              fontWeight: selected ? 600 : 400,
                            }}
                          >
                            {isLoaderRow ? (
                              <Box sx={{ flex: 1, textAlign: 'center', color: 'text.disabled' }}>
                                <CircularProgress size={14} sx={{ mr: 1 }} />
                                {t('common.loadingMore') || 'Loading more...'}
                              </Box>
                            ) : (
                              columns.map((col) => (
                                <Box
                                  key={String(col.field)}
                                  sx={{
                                    ...columnStyle(col),
                                    px: 1,
                                    textAlign: col.align ?? 'left',
                                    overflow: 'hidden',
                                    textOverflow: 'ellipsis',
                                    whiteSpace: 'nowrap',
                                  }}
                                  title={String((row as any)[col.field] ?? '')}
                                >
                                  {renderCell(col, row!)}
                                </Box>
                              ))
                            )}
                          </Box>
                        );
                      })}
                    </Box>
                  )}
                </Box>
              </Box>

              <Divider />
              <Box
                sx={{
                  display: 'flex',
                  flexDirection: 'row',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  px: 1.5,
                  py: 0.75,
                  bgcolor: 'grey.50',
                }}
              >
                <Typography variant="caption" color="text.secondary">
                  {isFetching && !isFetchingNextPage
                    ? t('common.loading') || 'Loading...'
                    : `${rows.length} / ${totalRecords}`}
                </Typography>
                <Box sx={{ display: 'flex', gap: 0.5 }}>
                  <Button size="small" onClick={() => refetch()} disabled={isFetching}>
                    {t('common.refresh') || 'Refresh'}
                  </Button>
                  {actions?.map((a) => (
                    <Button
                      key={a.label}
                      size="small"
                      startIcon={a.icon}
                      onClick={a.onClick}
                      disabled={a.disabled}
                    >
                      {t(a.label)}
                    </Button>
                  ))}
                </Box>
              </Box>
            </Paper>
          </ClickAwayListener>
        </Portal>
      )}
    </Box>
  );
}
