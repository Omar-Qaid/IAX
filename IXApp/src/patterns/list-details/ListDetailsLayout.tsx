import React, { useEffect, useRef, useState } from 'react';
import { Accordion, AccordionDetails, AccordionSummary, Box, MenuItem, Switch, TextField, Typography } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import type { DetailSectionConfig, DetailValue, DetailValues } from './types';

export interface ListDetailsLayoutProps {
  listPane: React.ReactNode;
  header: React.ReactNode;
  sections: DetailSectionConfig[];
  values: DetailValues;
  editing: boolean;
  yesLabel: string;
  noLabel: string;
  onChange: (name: string, value: DetailValue) => void;
  listWidth?: number;
  listMinWidth?: number;
  listMaxWidth?: number;
  listResizable?: boolean;
  listPaneVisible?: boolean;
  listWidthStorageKey?: string;
}

export function ListDetailsLayout({ listPane, header, sections, values, editing, yesLabel, noLabel, onChange, listWidth = 264, listMinWidth = 176, listMaxWidth = 520, listResizable = true, listPaneVisible = true, listWidthStorageKey }: ListDetailsLayoutProps): React.ReactElement {
  const storageKey = listWidthStorageKey ? `ixapp.list-details.width.${listWidthStorageKey}` : null;
  const constrainWidth = (width: number) => Math.min(listMaxWidth, Math.max(listMinWidth, width));
  const readStoredWidth = () => {
    if (!storageKey) return listWidth;
    try {
      const storedWidth = Number(globalThis.localStorage?.getItem(storageKey));
      return Number.isFinite(storedWidth) && storedWidth > 0 ? constrainWidth(storedWidth) : listWidth;
    } catch { return listWidth; }
  };
  const [currentListWidth, setCurrentListWidth] = useState(readStoredWidth);
  const [resizing, setResizing] = useState(false);
  const dragState = useRef<{ startX: number; startWidth: number; direction: 'ltr' | 'rtl' } | null>(null);

  useEffect(() => setCurrentListWidth(readStoredWidth()), [listWidth, listMinWidth, listMaxWidth, storageKey]);
  useEffect(() => {
    if (!storageKey) return undefined;
    const timeout = globalThis.setTimeout(() => {
      try { globalThis.localStorage?.setItem(storageKey, String(Math.round(currentListWidth))); } catch { /* Storage can be unavailable in restricted browser contexts. */ }
    }, 150);
    return () => globalThis.clearTimeout(timeout);
  }, [currentListWidth, storageKey]);

  const startResize = (event: React.PointerEvent<HTMLDivElement>) => {
    dragState.current = { startX: event.clientX, startWidth: currentListWidth, direction: getComputedStyle(event.currentTarget).direction as 'ltr' | 'rtl' };
    setResizing(true);
    event.currentTarget.setPointerCapture(event.pointerId);
    event.preventDefault();
  };
  const resize = (event: React.PointerEvent<HTMLDivElement>) => {
    if (!dragState.current) return;
    const delta = event.clientX - dragState.current.startX;
    setCurrentListWidth(constrainWidth(dragState.current.startWidth + (dragState.current.direction === 'rtl' ? -delta : delta)));
  };
  const stopResize = (event: React.PointerEvent<HTMLDivElement>) => {
    dragState.current = null;
    setResizing(false);
    if (event.currentTarget.hasPointerCapture(event.pointerId)) event.currentTarget.releasePointerCapture(event.pointerId);
  };
  const resizeWithKeyboard = (event: React.KeyboardEvent<HTMLDivElement>) => {
    const direction = getComputedStyle(event.currentTarget).direction;
    const step = event.shiftKey ? 40 : 10;
    if (event.key === 'Home') setCurrentListWidth(listMinWidth);
    else if (event.key === 'End') setCurrentListWidth(listMaxWidth);
    else if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
      const delta = event.key === 'ArrowRight' ? step : -step;
      setCurrentListWidth((width) => constrainWidth(width + (direction === 'rtl' ? -delta : delta)));
    } else return;
    event.preventDefault();
  };

  return <Box sx={{ minHeight: 0, flex: 1, display: 'flex', gap: 0, overflow: 'hidden' }}>
    {listPaneVisible && <Box sx={{ width: { xs: '100%', md: currentListWidth }, flexShrink: 0, display: { xs: 'none', md: 'block' }, bgcolor: 'background.paper', border: 1, borderColor: 'divider', borderRadius: 1, overflow: 'hidden', boxShadow: '0 1px 4px rgba(0,0,0,.12)', willChange: resizing ? 'width' : 'auto', transition: resizing ? 'none' : 'width 160ms ease-out' }}>{listPane}</Box>}
    {listPaneVisible && listResizable && <Box role="separator" aria-label="Resize record list" aria-orientation="vertical" aria-valuemin={listMinWidth} aria-valuemax={listMaxWidth} aria-valuenow={Math.round(currentListWidth)} tabIndex={0} onPointerDown={startResize} onPointerMove={resize} onPointerUp={stopResize} onPointerCancel={stopResize} onDoubleClick={() => setCurrentListWidth(listWidth)} onKeyDown={resizeWithKeyboard} sx={{ width: 4, flexShrink: 0, position: 'relative', zIndex: 1, display: { xs: 'none', md: 'flex' }, alignItems: 'stretch', justifyContent: 'center', cursor: 'col-resize', touchAction: 'none', userSelect: 'none', outline: 'none', '&::before': { content: '""', position: 'absolute', insetBlock: 0, insetInline: -4 }, '&::after': { content: '""', width: resizing ? 2 : 1, bgcolor: resizing ? 'primary.main' : 'transparent', borderRadius: 1, opacity: resizing ? 1 : 0, transition: 'background-color 120ms ease, width 120ms ease, opacity 120ms ease' }, '&:hover::after, &:focus-visible::after': { width: 2, bgcolor: 'primary.main', opacity: 1 } }} />}
    {listPaneVisible && !listResizable && <Box sx={{ width: 16, flexShrink: 0, display: { xs: 'none', md: 'block' } }} />}
    <Box sx={{ minWidth: 0, flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
      {header}
      <Box sx={{ minHeight: 0, flex: 1, overflowY: 'auto', pr: 0.5 }}>
        {sections.map((section) => <Accordion key={section.id} defaultExpanded={section.defaultExpanded ?? true} disableGutters elevation={0} sx={{ mb: 0.5, border: 1, borderColor: 'divider', borderRadius: '7px !important', '&::before': { display: 'none' } }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon sx={{ fontSize: 17 }} />} sx={{ px: 1.25, minHeight: 42, borderBottom: 1, borderColor: 'divider', '&.Mui-expanded': { minHeight: 42 }, '& .MuiAccordionSummary-content, & .MuiAccordionSummary-content.Mui-expanded': { my: 0.75 }, '& .MuiAccordionSummary-expandIconWrapper': { border: 1, borderColor: 'divider', borderRadius: 0.5, p: 0.2 } }}><Typography sx={{ fontSize: '0.875rem', fontWeight: 600 }}>{section.title}</Typography></AccordionSummary>
          <AccordionDetails sx={{ p: 1.25 }}>
            {section.link && <Box sx={{ mb: 1 }}>{section.link}</Box>}
            {section.content ?? <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2,1fr)', lg: `repeat(${section.columns ?? Math.min(5, section.groups?.length ?? 1)}, minmax(120px,1fr))` }, gap: 3 }}>
              {(section.groups ?? []).map((group) => <Box key={group.id} sx={{ minWidth: 0 }}>
                {group.title && <Typography sx={{ mb: 1, fontSize: '0.6875rem', fontWeight: 700, textTransform: 'uppercase' }}>{group.title}</Typography>}
                <Box sx={{ display: 'grid', gridTemplateColumns: group.columns ? `repeat(${group.columns}, minmax(0, 1fr))` : '1fr', columnGap: 2, rowGap: 1.1 }}>{group.fields.map((field) => {
                  const value = values[field.name]; const editable = editing && !field.disabled && field.type !== 'display';
                  return <Box key={field.name} sx={{ minWidth: 0, width: field.width ?? '100%', gridColumn: field.column, gridRow: field.row }}><Typography noWrap title={field.label} sx={{ mb: 0.25, fontSize: '0.6875rem', color: 'text.secondary' }}>{field.label}</Typography>
                    {field.type === 'boolean' ? <Box sx={{ display: 'flex', alignItems: 'center', height: 28 }}><Switch size="small" checked={Boolean(value)} disabled={!editable} onChange={(_, checked) => onChange(field.name, checked)} sx={{ ml: -0.75, mr: 0.25, '& .MuiSwitch-thumb': { width: 13, height: 13 } }} /><Typography sx={{ fontSize: '0.75rem' }}>{value ? yesLabel : noLabel}</Typography></Box>
                    : editable && field.type === 'select' ? <TextField select value={value ?? ''} onChange={(event) => onChange(field.name, event.target.value)} sx={editFieldSx}>{(field.options ?? []).map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}</TextField>
                    : editable ? <TextField type={field.type === 'number' ? 'number' : 'text'} value={value ?? ''} onChange={(event) => onChange(field.name, field.type === 'number' ? Number(event.target.value) : event.target.value)} sx={editFieldSx} />
                    : <ViewField value={field.type === 'select' ? (field.options?.find((option) => option.value === String(value))?.label ?? value) : value} numeric={field.type === 'number'} />}
                  </Box>;
                })}</Box>
              </Box>)}
            </Box>}
          </AccordionDetails>
        </Accordion>)}
      </Box>
    </Box>
  </Box>;
}

function ViewField({ value, numeric = false }: { value: DetailValue | undefined; numeric?: boolean }): React.ReactElement {
  return <Box sx={{ width: '100%', minHeight: 29, display: 'flex', alignItems: 'center', justifyContent: numeric ? 'flex-end' : 'flex-start', borderBottom: '1px solid', borderColor: 'text.secondary', px: 0.5, overflow: 'hidden' }}><Typography noWrap sx={{ fontSize: '0.75rem' }}>{String(value ?? '')}</Typography></Box>;
}

const editFieldSx = { width: '100%', '& .MuiInputBase-root': { height: 29, borderRadius: 0.5, fontSize: '0.75rem' }, '& .MuiInputBase-input': { px: 0.75, py: 0.5 } };

