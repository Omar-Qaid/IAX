import React, { useCallback, useEffect, useRef, useState } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Drawer,
  IconButton,
  Switch,
  TextField,
  Typography,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import type { DetailSectionConfig, DetailValue, DetailValues } from './types';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { getLogicalDrawerAnchor } from '@shared/hooks/useLogicalDrawerAnchor';
import { LookupTitleMenu } from '@shared/components/lookups/LookupTitleMenu';
import { lookupMasterRoute } from '@shared/components/lookups/lookupMasterRoutes';
import { LookupField } from '@shared/components/lookups/LookupField';
import { EditableViewField } from '@shared/components/fields/EditableViewField';
import { FieldViewModeProvider } from '@shared/components/fields/FieldViewModeContext';

export interface ListDetailsLayoutProps {
  detailEndPadding?: number;
  listPane: React.ReactNode;
  header: React.ReactNode;
  sections: DetailSectionConfig[];
  values: DetailValues;
  editing: boolean;
  yesLabel: string;
  noLabel: string;
  onChange: (name: string, value: DetailValue) => void;
  onFieldEdit?: (fieldName: string) => void;
  canEditField?: (fieldName: string) => boolean;
  listWidth?: number;
  listMinWidth?: number;
  listMaxWidth?: number;
  listResizable?: boolean;
  listPaneVisible?: boolean;
  onListPaneClose?: () => void;
  listWidthStorageKey?: string;
}

export function ListDetailsLayout({
  detailEndPadding,
  listPane,
  header,
  sections,
  values,
  editing,
  yesLabel,
  noLabel,
  onChange,
  onFieldEdit,
  canEditField,
  listWidth = 264,
  listMinWidth = 176,
  listMaxWidth = 520,
  listResizable = true,
  listPaneVisible = true,
  onListPaneClose,
  listWidthStorageKey,
}: ListDetailsLayoutProps): React.ReactElement {
  const { t } = useAppTranslation();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const storageKey = listWidthStorageKey ? `ixapp.list-details.width.${listWidthStorageKey}` : null;
  const constrainWidth = useCallback(
    (width: number) => Math.min(listMaxWidth, Math.max(listMinWidth, width)),
    [listMaxWidth, listMinWidth]
  );
  const readStoredWidth = useCallback(() => {
    if (!storageKey) return listWidth;
    try {
      const storedWidth = Number(globalThis.localStorage?.getItem(storageKey));
      return Number.isFinite(storedWidth) && storedWidth > 0
        ? constrainWidth(storedWidth)
        : listWidth;
    } catch {
      return listWidth;
    }
  }, [constrainWidth, listWidth, storageKey]);
  const [currentListWidth, setCurrentListWidth] = useState(readStoredWidth);
  const [resizing, setResizing] = useState(false);
  const dragState = useRef<{ startX: number; startWidth: number; direction: 'ltr' | 'rtl' } | null>(
    null
  );

  useEffect(() => setCurrentListWidth(readStoredWidth()), [readStoredWidth]);
  useEffect(() => {
    if (!storageKey) return undefined;
    const timeout = globalThis.setTimeout(() => {
      try {
        globalThis.localStorage?.setItem(storageKey, String(Math.round(currentListWidth)));
      } catch {
        /* Storage can be unavailable in restricted browser contexts. */
      }
    }, 150);
    return () => globalThis.clearTimeout(timeout);
  }, [currentListWidth, storageKey]);

  const startResize = (event: React.PointerEvent<HTMLDivElement>) => {
    dragState.current = {
      startX: event.clientX,
      startWidth: currentListWidth,
      direction: getComputedStyle(event.currentTarget).direction as 'ltr' | 'rtl',
    };
    setResizing(true);
    event.currentTarget.setPointerCapture(event.pointerId);
    event.preventDefault();
  };
  const resize = (event: React.PointerEvent<HTMLDivElement>) => {
    if (!dragState.current) return;
    const delta = event.clientX - dragState.current.startX;
    setCurrentListWidth(
      constrainWidth(
        dragState.current.startWidth + (dragState.current.direction === 'rtl' ? -delta : delta)
      )
    );
  };
  const stopResize = (event: React.PointerEvent<HTMLDivElement>) => {
    dragState.current = null;
    setResizing(false);
    if (event.currentTarget.hasPointerCapture(event.pointerId))
      event.currentTarget.releasePointerCapture(event.pointerId);
  };
  const resizeWithKeyboard = (event: React.KeyboardEvent<HTMLDivElement>) => {
    const direction = getComputedStyle(event.currentTarget).direction;
    const step = event.shiftKey ? 40 : 10;
    if (event.key === 'Home') setCurrentListWidth(listMinWidth);
    else if (event.key === 'End') setCurrentListWidth(listMaxWidth);
    else if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
      const delta = event.key === 'ArrowRight' ? step : -step;
      setCurrentListWidth((width) =>
        constrainWidth(width + (direction === 'rtl' ? -delta : delta))
      );
    } else return;
    event.preventDefault();
  };

  return (
    <Box
      sx={{
        minHeight: 0,
        flex: 1,
        display: 'flex',
        gap: 0,
        overflow: 'hidden',
        fontFamily: d365.fontFamily,
        color: d365.text,
      }}
    >
      {isMobile && (
        <Drawer
          anchor={getLogicalDrawerAnchor('start')}
          open={listPaneVisible}
          onClose={onListPaneClose}
          slotProps={{
            paper: {
              sx: {
                width: 'min(90vw, 360px)',
                maxWidth: 'calc(100vw - 24px)',
                mt: '58px',
                height: 'calc(100% - 58px)',
              },
            },
          }}
        >
          {listPane}
        </Drawer>
      )}
      {listPaneVisible && (
        <Box
          sx={{
            width: currentListWidth,
            flexShrink: 0,
            display: { xs: 'none', md: 'block' },
            bgcolor: d365.surface,
            border: `1px solid ${d365.border}`,
            borderRadius: '9px',
            overflow: 'hidden',
            boxShadow: '0 1px 4px rgba(0,0,0,.14)',
            willChange: resizing ? 'width' : 'auto',
            transition: resizing ? 'none' : 'width 160ms ease-out',
          }}
        >
          {listPane}
        </Box>
      )}
      {listPaneVisible && listResizable && (
        <Box
          role="separator"
          aria-label={t('accessibility.resizeRecordList')}
          aria-orientation="vertical"
          aria-valuemin={listMinWidth}
          aria-valuemax={listMaxWidth}
          aria-valuenow={Math.round(currentListWidth)}
          tabIndex={0}
          onPointerDown={startResize}
          onPointerMove={resize}
          onPointerUp={stopResize}
          onPointerCancel={stopResize}
          onDoubleClick={() => setCurrentListWidth(listWidth)}
          onKeyDown={resizeWithKeyboard}
          sx={{
            width: 23,
            flexShrink: 0,
            position: 'relative',
            zIndex: 1,
            display: { xs: 'none', md: 'flex' },
            alignItems: 'stretch',
            justifyContent: 'center',
            cursor: 'col-resize',
            touchAction: 'none',
            userSelect: 'none',
            outline: 'none',
            '&::before': { content: '""', position: 'absolute', insetBlock: 0, insetInline: -4 },
            '&::after': {
              content: '""',
              width: resizing ? 2 : 1,
              bgcolor: resizing ? 'primary.main' : 'transparent',
              borderRadius: 1,
              opacity: resizing ? 1 : 0,
              transition: 'background-color 120ms ease, width 120ms ease, opacity 120ms ease',
            },
            '&:hover::after, &:focus-visible::after': {
              width: 2,
              bgcolor: 'primary.main',
              opacity: 1,
            },
          }}
        />
      )}
      {listPaneVisible && !listResizable && (
        <Box sx={{ width: 23, flexShrink: 0, display: { xs: 'none', md: 'block' } }} />
      )}
      <Box
        sx={{ minWidth: 0, flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}
      >
        {header}
        <Box
          sx={{
            minHeight: 0,
            flex: 1,
            overflowY: 'auto',
            overflowX: 'hidden',
            paddingInlineEnd:
              detailEndPadding != null ? `${detailEndPadding}px` : { xs: 0, sm: 1, md: '28px' },
            scrollbarWidth: 'thin',
          }}
        >
          {sections.map((section) => {
            const legalEntity = section.visualVariant === 'legalEntity';
            if (section.hideHeader)
              return (
                <Box key={section.id} sx={{ mb: `${d365.sectionGap}px` }}>
                  {section.content}
                </Box>
              );
            return (
              <Accordion
                key={section.id}
                defaultExpanded={section.defaultExpanded ?? true}
                disableGutters
                elevation={0}
                sx={{
                  mb: `${d365.sectionGap}px !important`,
                  border: `1px solid ${legalEntity ? '#d1d1d1' : d365.border}`,
                  borderRadius: `${legalEntity ? 9 : d365.sectionRadius}px !important`,
                  bgcolor: d365.surface,
                  overflow: 'hidden',
                  boxShadow: legalEntity ? '0 1px 3px rgba(0,0,0,.12)' : 'none',
                  '&::before': { display: 'none' },
                }}
              >
                <AccordionSummary
                  expandIcon={<ExpandMoreIcon sx={{ fontSize: 16, color: '#323130' }} />}
                  sx={{
                    px: legalEntity ? '10px' : 1,
                    minHeight: `${d365.sectionHeaderHeight}px !important`,
                    borderBottom: 1,
                    borderColor: legalEntity ? '#c8c6c4' : d365.border,
                    '&.Mui-expanded': { minHeight: `${d365.sectionHeaderHeight}px !important` },
                    '& .MuiAccordionSummary-content, & .MuiAccordionSummary-content.Mui-expanded': {
                      my: 0.5,
                    },
                    '& .MuiAccordionSummary-expandIconWrapper': {
                      width: legalEntity ? 30 : 'auto',
                      height: legalEntity ? 30 : 'auto',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      border: `1px solid ${legalEntity ? '#c8c6c4' : d365.border}`,
                      borderRadius: legalEntity ? '4px' : d365.radius,
                      p: legalEntity ? 0 : '2px',
                    },
                  }}
                >
                  <Typography
                    sx={{
                      fontFamily: d365.fontFamily,
                      fontSize: 14,
                      lineHeight: 1.2,
                      color: '#111',
                      fontWeight: 600,
                    }}
                  >
                    {section.title}
                  </Typography>
                  {Boolean(section.summaryItems?.length) && (
                    <Box
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        marginInlineStart: 'auto',
                        paddingInlineStart: 2,
                        paddingInlineEnd: 1.5,
                        minWidth: 0,
                        overflow: 'hidden',
                      }}
                    >
                      {section.summaryItems?.map((item, index) => (
                        <Typography
                          component="span"
                          key={item.id}
                          title={item.label}
                          aria-label={item.label}
                          sx={{
                            fontSize: 11,
                            lineHeight: '20px',
                            whiteSpace: 'nowrap',
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            maxWidth: 220,
                            px: 1.25,
                            borderInlineStart: index ? '1px solid #c8c6c4' : undefined,
                            color: item.accent ? 'primary.main' : 'text.primary',
                          }}
                        >
                          {item.value ?? '-'}
                        </Typography>
                      ))}
                    </Box>
                  )}
                </AccordionSummary>
                <AccordionDetails
                  sx={{
                    p: section.detailsPadding ?? '11px 10px 10px',
                    minHeight: section.minHeight,
                    boxSizing: 'border-box',
                  }}
                >
                  {section.link && <Box sx={{ mb: 1 }}>{section.link}</Box>}
                  {section.content ?? (
                    <Box
                      sx={{
                        display: 'grid',
                        gridTemplateColumns: {
                          xs: 'minmax(0, 1fr)',
                          sm: 'repeat(2, minmax(0, 1fr))',
                          lg: 'repeat(auto-fit, minmax(180px, 1fr))',
                          xl:
                            section.gridTemplateColumns ??
                            `repeat(${section.columns ?? Math.min(5, section.groups?.length ?? 1)}, minmax(120px,1fr))`,
                        },
                        justifyContent: section.gridTemplateColumns ? 'start' : 'space-between',
                        columnGap: section.columnGap ?? `${d365.fieldGap}px`,
                        rowGap: 1,
                      }}
                    >
                      {(section.groups ?? []).map((group) => (
                        <Box
                          key={group.id}
                          sx={{
                            minWidth: 0,
                            maxWidth: '100%',
                            width: { xs: '100%', xl: group.width },
                            gridColumn: { xs: 'auto', xl: group.column },
                          }}
                        >
                          {(group.title || section.groups?.some((candidate) => candidate.title)) && (
                            <Typography
                              aria-hidden={!group.title}
                              sx={{
                                mb: 1,
                                fontSize: d365.labelFontSize,
                                fontWeight: 700,
                                textTransform: 'uppercase',
                                visibility: group.title ? 'visible' : 'hidden',
                              }}
                            >
                              {group.title || '\u00a0'}
                            </Typography>
                          )}
                          <Box
                            sx={{
                              display: 'grid',
                              gridTemplateColumns: group.columns
                                ? {
                                    xs: 'minmax(0, 1fr)',
                                    sm: `repeat(${Math.min(2, group.columns)}, minmax(0, 1fr))`,
                                    lg: `repeat(${group.columns}, minmax(0, 1fr))`,
                                  }
                                : '1fr',
                              columnGap: 2,
                              rowGap: '12px',
                            }}
                          >
                            {group.fields.map((field) => {
                              const value = values[field.name];
                              const editable =
                                editing && !field.disabled && field.type !== 'display';
                              const sectionTitle =
                                field.sectionTitle ??
                                (legalEntity && field.name === 'timeZone'
                                  ? 'TIME ZONE'
                                  : undefined);
                              const fieldLabel =
                                legalEntity && field.name === 'timeZone'
                                  ? 'Time zone'
                                  : field.label;
                              const editField = !editing && !field.disabled && field.type !== 'display' &&
                                (!canEditField || canEditField(field.name)) && onFieldEdit
                                  ? () => onFieldEdit(field.name)
                                  : undefined;
                              return (
                                <Box
                                  key={field.name}
                                  sx={{
                                    minWidth: 0,
                                    maxWidth: '100%',
                                    width: { xs: '100%', lg: field.width ?? '100%' },
                                    gridColumn: { xs: 'auto', lg: field.column },
                                    gridRow: { xs: 'auto', lg: field.row },
                                  }}
                                >
                                  {sectionTitle && (
                                    <Typography
                                      sx={{
                                        mt: '10px',
                                        mb: '8px',
                                        fontFamily: d365.fontFamily,
                                        fontSize: 12,
                                        lineHeight: 1.2,
                                        fontWeight: 700,
                                        textTransform: 'uppercase',
                                        color: '#111',
                                      }}
                                    >
                                      {sectionTitle}
                                    </Typography>
                                  )}
                                  {(!field.renderOwnLabel || (!editing && field.render && field.type === 'select')) && (
                                    <Typography
                                      noWrap
                                      title={fieldLabel}
                                      sx={{
                                        mb: '6px',
                                        fontFamily: d365.fontFamily,
                                        fontSize: legalEntity ? 12 : d365.labelFontSize,
                                        lineHeight: 1.2,
                                        color: d365.text,
                                      }}
                                    >
                                      {field.type === 'select' || lookupMasterRoute(field.name, field.masterRoute)
                                        ? <LookupTitleMenu name={field.name} label={fieldLabel} masterRoute={field.masterRoute} />
                                        : fieldLabel}
                                    </Typography>
                                  )}
                                  {!editing && field.render && field.type === 'select' ? (
                                    <EditableViewField
                                      value={field.formatValue?.(value) ?? field.options?.find((option) => option.value === String(value))?.label ?? String(value ?? '')}
                                      lookup
                                      label={fieldLabel}
                                      onEdit={editField}
                                      disabled={field.disabled}
                                    />
                                  ) : field.render ? (
                                    <FieldViewModeProvider viewMode={!editing}>
                                    <Box sx={{
                                      position: 'relative',
                                      '&:hover .field-edit-action, &:focus-within .field-edit-action': { opacity: 1 },
                                    }}>
                                    {field.render({
                                      value,
                                      editing,
                                      disabled: !editable,
                                      onChange: (nextValue) => onChange(field.name, nextValue),
                                    })
                                    }
                                    {editField && (
                                      <IconButton
                                        className="field-edit-action"
                                        size="small"
                                        aria-label={`Edit ${fieldLabel}`}
                                        onClick={editField}
                                        sx={{ position: 'absolute', insetInlineEnd: 0, bottom: 0, opacity: 0, color: d365.primary, p: 0.25 }}
                                      >
                                        <EditOutlinedIcon sx={{ fontSize: 13 }} />
                                      </IconButton>
                                    )}
                                    </Box>
                                    </FieldViewModeProvider>
                                  ) : field.type === 'boolean' ? (
                                    !editing ? <EditableViewField value={value ? yesLabel : noLabel} label={fieldLabel} onEdit={editField} disabled={field.disabled} /> : <Box
                                      sx={{
                                        display: 'flex',
                                        alignItems: 'center',
                                        height: d365.controlHeight,
                                      }}
                                    >
                                      <Switch
                                        size="small"
                                        checked={Boolean(value)}
                                        disabled={!editable}
                                        onChange={(_, checked) => onChange(field.name, checked)}
                                        sx={legalEntity ? legalEntitySwitchSx : d365SwitchSx}
                                      />
                                      <Typography
                                        sx={{
                                          marginInlineStart: legalEntity ? '6px' : 0,
                                          fontFamily: d365.fontFamily,
                                          fontSize: d365.fontSize,
                                        }}
                                      >
                                        {value ? yesLabel : noLabel}
                                      </Typography>
                                    </Box>
                                  ) : editable && field.type === 'select' ? (
                                    <Box sx={legalEntity ? legalEntityFieldSx : editFieldSx}>
                                      <LookupField
                                        name={field.name}
                                        label={fieldLabel}
                                        externalLabel
                                        value={String(value ?? '')}
                                        variant={legalEntity ? 'standard' : 'outlined'}
                                        options={(field.options ?? []).map((option) => ({ id: option.value, code: option.value, name: option.label }))}
                                        queryKey={['list-details-lookup', field.name, field.options]}
                                        fetchPage={async ({ pageNumber, pageSize, search }) => {
                                          const matches = (field.options ?? []).filter((option) =>
                                            `${option.value} ${option.label}`.toLocaleLowerCase().includes(search.trim().toLocaleLowerCase())
                                          );
                                          return {
                                            data: matches.slice((pageNumber - 1) * pageSize, pageNumber * pageSize)
                                              .map((option) => ({ id: option.value, code: option.value, name: option.label })),
                                            pageNumber,
                                            totalPages: Math.max(1, Math.ceil(matches.length / pageSize)),
                                            totalRecords: matches.length,
                                          };
                                        }}
                                        sideMode="server"
                                        searchable
                                        lazyLoading
                                        pageSize={50}
                                        onChange={(selected) => onChange(field.name, String(selected ?? ''))}
                                      />
                                    </Box>
                                  ) : editable ? (
                                    <TextField
                                      variant={legalEntity ? 'standard' : 'outlined'}
                                      type={
                                        field.type === 'number'
                                          ? 'number'
                                          : field.type === 'date'
                                            ? 'date'
                                            : 'text'
                                      }
                                      multiline={field.multiline}
                                      rows={field.multiline ? (field.rows ?? 4) : undefined}
                                      value={value ?? ''}
                                      onChange={(event) =>
                                        onChange(
                                          field.name,
                                          field.type === 'number'
                                            ? Number(event.target.value)
                                            : event.target.value
                                        )
                                      }
                                      sx={
                                        legalEntity
                                          ? field.multiline
                                            ? legalEntityMultilineSx
                                            : legalEntityFieldSx
                                          : field.multiline
                                            ? multilineFieldSx
                                            : editFieldSx
                                      }
                                    />
                                  ) : (
                                    <EditableViewField
                                      value={
                                        field.type === 'select'
                                          ? (field.options?.find(
                                              (option) => option.value === String(value)
                                            )?.label ?? value)
                                          : value
                                      }
                                      numeric={field.type === 'number'}
                                      disabled={field.disabled}
                                      lookup={field.type === 'select' || Boolean(lookupMasterRoute(field.name, field.masterRoute)) || Boolean(field.linkStyle) || (legalEntity && field.name === 'languageId')}
                                      label={fieldLabel}
                                      onEdit={editField}
                                    />
                                  )}
                                </Box>
                              );
                            })}
                          </Box>
                        </Box>
                      ))}
                    </Box>
                  )}
                </AccordionDetails>
              </Accordion>
            );
          })}
        </Box>
      </Box>
    </Box>
  );
}

const editFieldSx = {
  width: '100%',
  '& .MuiInputBase-root': {
    height: d365.controlHeight,
    borderRadius: d365.radius,
    fontSize: d365.fontSize,
  },
  '& .MuiInputBase-input': { px: 0.75, py: 0.25 },
};
const multilineFieldSx = {
  width: '100%',
  '& .MuiInputBase-root': {
    minHeight: 119,
    alignItems: 'flex-start',
    borderRadius: d365.radius,
    fontSize: d365.fontSize,
    p: 0,
  },
  '& textarea': { p: '5px 7px' },
};
const legalEntityFieldSx = {
  width: '100%',
  '& .MuiInputBase-root': {
    height: d365.controlHeight,
    fontFamily: d365.fontFamily,
    fontSize: d365.fontSize,
  },
  '& .MuiInputBase-input': { px: 0.5, py: 0.25 },
  '& .MuiInput-underline:before': { borderBottomColor: d365.darkBorder },
  '& .MuiInput-underline:after': { borderBottomColor: d365.primary },
};
const legalEntityMultilineSx = {
  ...legalEntityFieldSx,
  '& .MuiInputBase-root': {
    minHeight: 119,
    alignItems: 'flex-start',
    fontFamily: d365.fontFamily,
    fontSize: d365.fontSize,
  },
  '& textarea': { p: '5px 7px' },
};
const d365SwitchSx = {
  width: 35,
  height: 20,
  p: '3px',
  marginInlineEnd: 0.25,
  transform: 'translateY(-5px)',
  '& .MuiSwitch-switchBase': { p: '5px', '&.Mui-checked': { transform: 'translateX(14px)' } },
  '& .MuiSwitch-thumb': { width: 10, height: 10 },
  '& .MuiSwitch-track': {
    borderRadius: 8,
    border: `1px solid ${d365.darkBorder}`,
    bgcolor: '#fff',
    opacity: 1,
  },
  '& .Mui-checked + .MuiSwitch-track': { bgcolor: '#605e5c !important', opacity: '1 !important' },
};
const legalEntitySwitchSx = {
  width: 36,
  height: 20,
  p: 0,
  '& .MuiSwitch-switchBase': {
    p: '3px',
    '&.Mui-checked': { transform: 'translateX(16px)', color: '#fff' },
  },
  '& .MuiSwitch-thumb': { width: 14, height: 14 },
  '& .MuiSwitch-track': { borderRadius: '10px', bgcolor: '#605e5c', opacity: '1 !important' },
  '& .Mui-disabled + .MuiSwitch-track': { opacity: '1 !important' },
};
