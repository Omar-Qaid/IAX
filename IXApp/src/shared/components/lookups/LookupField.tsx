import { localizedName } from '@shared/utilities/localizedName';
import React, { useEffect, useMemo, useRef, useState } from 'react';
import {
  Autocomplete,
  Box,
  TextField,
  InputAdornment,
  IconButton,
  Popper,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import ClearIcon from '@mui/icons-material/Clear';
import { LookupDialog } from './LookupDialog';
import type { LookupFieldProps, LookupOption } from './types';
import { useLookupGridField } from '@shared/hooks/useLookupGridField';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { FieldValues } from 'react-hook-form';
import { hasLookupValue, lookupValueSx } from './lookupValueStyle';
import { LookupDisplayField } from './LookupDisplayField';
import { useFieldViewMode } from '@shared/components/fields/FieldPresentationContext';
import { RecordPreview, RecordPreviewCard } from '@shared/components/record-preview';

const standardLookupSx = {
  minWidth: 0,
  '& .MuiInput-root': {
    minHeight: 32,
    boxSizing: 'border-box',
  },
  '& .MuiAutocomplete-inputRoot.MuiInput-root': {
    paddingBlock: '0 !important',
  },
  '& .MuiAutocomplete-inputRoot.MuiInput-root .MuiAutocomplete-input': {
    boxSizing: 'border-box',
    height: 31,
    paddingBlock: '6px !important',
    paddingInlineStart: '0 !important',
    // A selected value can render both the clear and popup buttons.
    paddingInlineEnd: '60px !important',
  },
  '& .MuiAutocomplete-endAdornment': {
    insetInlineEnd: 0,
    top: '50%',
    transform: 'translateY(-50%)',
  },
} as const;

const compactOutlinedLookupSx = {
  '& .MuiAutocomplete-inputRoot.MuiOutlinedInput-root': {
    minHeight: 28,
    height: 28,
    paddingBlock: '0 !important',
  },
  '& .MuiAutocomplete-inputRoot.MuiOutlinedInput-root .MuiAutocomplete-input': {
    boxSizing: 'border-box',
    height: 28,
    paddingBlock: '4px !important',
  },
} as const;

const compactGridLookupSx = {
  width: '100%',
  minWidth: 0,
  '& .MuiAutocomplete-inputRoot': {
    boxSizing: 'border-box',
    minWidth: 0,
    height: 28,
    minHeight: 28,
    paddingInlineEnd: '42px !important',
  },
  '& .MuiAutocomplete-inputRoot.MuiOutlinedInput-root': {
    height: 28,
    minHeight: 28,
    paddingBlock: '0 !important',
  },
  '& .MuiAutocomplete-inputRoot.MuiOutlinedInput-root .MuiAutocomplete-input': {
    boxSizing: 'border-box',
    height: '28px !important',
    minWidth: 0,
    padding: '4px 3px !important',
    paddingInlineEnd: '0 !important',
  },
  '& .MuiAutocomplete-input': {
    minWidth: 0,
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
  },
  '& .MuiAutocomplete-endAdornment': {
    display: 'flex',
    alignItems: 'center',
    insetInlineEnd: 2,
    top: '50%',
    transform: 'translateY(-50%)',
  },
  '& .MuiAutocomplete-clearIndicator': {
    marginInlineEnd: 0,
    padding: '2px',
  },
  '& .MuiAutocomplete-popupIndicator': {
    padding: '2px',
  },
} as const;

/** D365 shows the record preview after the pointer/focus rests on the value briefly. */
const PREVIEW_DELAY_MS = 400;

/** Client-side lists render this many options, growing as the user scrolls. */
const CLIENT_RENDER_CHUNK = 100;

/** Grid-cell list: code and name side by side, like the D365 lookup grid. */
const gridOptionSx = {
  display: 'grid !important',
  gridTemplateColumns: 'minmax(64px, max-content) minmax(0, 1fr)',
  columnGap: 1.5,
  alignItems: 'center',
  minHeight: 30,
  py: '3px !important',
  fontSize: 13,
} as const;

/**
 * Grid cells: borderless standard input with only the chevron at the end (no
 * clear button). Overrides the form rule that reserves 60px for clear + popup
 * buttons, which in a ~100px column left no room for the value at all.
 */
const gridCellLookupSx = {
  '& .MuiInput-root': { minHeight: 28 },
  '& .MuiAutocomplete-inputRoot': {
    boxSizing: 'border-box',
    minWidth: 0,
    height: 28,
    minHeight: 28,
    paddingInlineEnd: '22px !important',
  },
  '& .MuiAutocomplete-inputRoot.MuiInput-root': { paddingBlock: '0 !important' },
  '& .MuiAutocomplete-inputRoot.MuiInput-root .MuiAutocomplete-input': {
    boxSizing: 'border-box',
    height: 28,
    minWidth: 0,
    padding: '4px 2px !important',
  },
} as const;

type ServerLookupDialogProps = Pick<
  LookupFieldProps,
  | 'fetchPage'
  | 'onFetchOptions'
  | 'queryKey'
  | 'pageSize'
  | 'searchDebounceMs'
  | 'searchable'
  | 'lazyLoading'
> & {
  open: boolean;
  title: string;
  selectedId?: string | number | (string | number)[];
  onClose: () => void;
  onSelect: (option: LookupOption) => void;
};

function ServerLookupDialog({
  open,
  title,
  selectedId,
  onClose,
  onSelect,
  fetchPage,
  onFetchOptions,
  queryKey = ['lookup-field', title],
  pageSize = 50,
  searchDebounceMs = 350,
  searchable = true,
  lazyLoading = false,
}: ServerLookupDialogProps) {
  const [search, setSearch] = useState('');
  const resolvedFetchPage = useMemo(
    () =>
      fetchPage ??
      (async ({ search: searchValue }) => {
        const data = onFetchOptions ? await onFetchOptions(searchValue) : [];
        return { data, pageNumber: 1, totalPages: 1, totalRecords: data.length };
      }),
    [fetchPage, onFetchOptions]
  );
  const lookup = useLookupGridField<LookupOption>({
    queryKey,
    fetchPage: resolvedFetchPage,
    enabled: open,
    pageSize,
    search: searchable ? search : '',
    debounceMs: searchDebounceMs,
  });

  return (
    <LookupDialog
      open={open}
      onClose={onClose}
      title={title}
      options={lookup.rows}
      selectedId={selectedId}
      onSelect={onSelect}
      loading={lookup.isLoading || (lookup.isFetching && !lookup.isFetchingNextPage)}
      loadingMore={lookup.isFetchingNextPage}
      searchable={searchable}
      sideMode="server"
      searchValue={search}
      onSearchChange={setSearch}
      lazyLoading={lazyLoading}
      hasMore={lazyLoading && lookup.hasNextPage}
      onLoadMore={() => void lookup.fetchNextPage()}
    />
  );
}

function SelectLookup({
  label,
  masterRoute,
  variant = 'outlined',
  externalLabel = false,
  value,
  onChange,
  options = [],
  disabled = false,
  readOnly = false,
  required = false,
  error = false,
  helperText,
  placeholder,
  fullWidth = true,
  compact = false,
  searchable = true,
  sideMode = 'server',
  lazyLoading = true,
  fetchPage,
  onFetchOptions,
  queryKey,
  pageSize = 50,
  searchDebounceMs = 350,
  showAllNamesInOptions = false,
  autoFocus = false,
  previewFields,
  previewEntity,
  gridCell = false,
}: LookupFieldProps & { gridCell?: boolean }) {
  const { t, isRtl } = useAppTranslation();
  const viewMode = useFieldViewMode();
  const usesServerDataSource = sideMode === 'server' && Boolean(fetchPage || onFetchOptions);
  const resolvedFetchPage = useMemo(
    () =>
      usesServerDataSource
        ? (fetchPage ??
          (async ({ search }) => {
            const data = onFetchOptions ? await onFetchOptions(search) : [];
            return { data, pageNumber: 1, totalPages: 1, totalRecords: data.length };
          }))
        : async ({ pageNumber, pageSize: requestedPageSize, search }) => {
            const term = searchable ? search.trim().toLowerCase() : '';
            const filtered = term
              ? options.filter(
                  (option) =>
                    option.code.toLowerCase().includes(term) ||
                    option.name.toLowerCase().includes(term) ||
                    (option.nameAlias ?? '').toLowerCase().includes(term) ||
                    option.description?.toLowerCase().includes(term)
                )
              : options;
            const effectivePage = lazyLoading ? pageNumber : 1;
            const effectivePageSize = lazyLoading
              ? requestedPageSize
              : Math.max(filtered.length, 1);
            const start = (effectivePage - 1) * effectivePageSize;
            return {
              data: filtered.slice(start, start + effectivePageSize),
              pageNumber: effectivePage,
              totalPages: Math.max(1, Math.ceil(filtered.length / effectivePageSize)),
              totalRecords: filtered.length,
            };
          },
    [fetchPage, lazyLoading, onFetchOptions, options, searchable, usesServerDataSource]
  );
  const scalarValue = Array.isArray(value) ? value[0] : value;
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');
  const [selectedCache, setSelectedCache] = useState<LookupOption | null>(null);
  const lookup = useLookupGridField<LookupOption>({
    queryKey: queryKey ?? ['lookup-field', label, 'server'],
    fetchPage: resolvedFetchPage,
    enabled: usesServerDataSource && (open || hasLookupValue(scalarValue)),
    pageSize,
    search: searchable ? search : '',
    debounceMs: searchDebounceMs,
  });
  const visibleOptions = useMemo(() => {
    if (usesServerDataSource) return lookup.rows;
    const term = searchable ? search.trim().toLowerCase() : '';
    if (!term) return options;
    return options.filter(
      (option) =>
        option.code.toLowerCase().includes(term) ||
        option.name.toLowerCase().includes(term) ||
        (option.nameAlias ?? '').toLowerCase().includes(term) ||
        option.description?.toLowerCase().includes(term)
    );
  }, [lookup.rows, options, search, searchable, usesServerDataSource]);
  const resolvedSelected =
    options.find((option) => String(option.id) === String(scalarValue)) ??
    lookup.rows.find((option) => String(option.id) === String(scalarValue)) ??
    (String(selectedCache?.id) === String(scalarValue) ? selectedCache : null);
  // With large lists the stored value is often not on the first page: in a grid
  // cell show a text code instead of an empty field (never a raw numeric record ID).
  const selected = useMemo<LookupOption | null>(
    () =>
      resolvedSelected ??
      (gridCell && typeof scalarValue === 'string' && scalarValue.trim() !== ''
        ? { id: scalarValue as string | number, code: String(scalarValue), name: String(scalarValue) }
        : null),
    [resolvedSelected, scalarValue, gridCell]
  );
  // Client lists are rendered in chunks so thousands of options stay responsive.
  const [renderLimit, setRenderLimit] = useState(CLIENT_RENDER_CHUNK);
  const renderedOptions = usesServerDataSource
    ? visibleOptions
    : visibleOptions.slice(0, renderLimit);
  // The selected option feeds the record preview card (no extra request).
  const selectedRecordOption = selected ? { code: selected.code, name: localizedName(selected, isRtl) } : null;
  const highlightedRef = useRef<LookupOption | null>(null);
  const [previewAnchor, setPreviewAnchor] = useState<HTMLElement | null>(null);
  const previewTimerRef = useRef<number | undefined>(undefined);
  useEffect(() => () => window.clearTimeout(previewTimerRef.current), []);
  // Grid cells have no clear button (D365): emptying the text and leaving clears.
  // True once the user types while the list is open. Until then the field keeps
  // showing (and selecting) the current value, so opening never blanks it.
  const [typed, setTyped] = useState(false);
  const handleScroll = (event: React.UIEvent<HTMLUListElement>) => {
    const list = event.currentTarget;
    const nearEnd = list.scrollHeight - list.scrollTop - list.clientHeight <= 80;
    if (!nearEnd) return;
    if (!usesServerDataSource) {
      if (renderLimit < visibleOptions.length) setRenderLimit((limit) => limit + CLIENT_RENDER_CHUNK);
      return;
    }
    if (!lazyLoading || !lookup.hasNextPage || lookup.isFetchingNextPage) return;
    void lookup.fetchNextPage();
  };
  const choose = (option: LookupOption | null) => {
    setSelectedCache(option);
    setOpen(false);
    setTyped(false);
    setSearch('');
    setRenderLimit(CLIENT_RENDER_CHUNK);
    onChange?.(option?.id ?? null, option ?? undefined);
  };
  // Grid cells show the code, matching the read-only cell (D365 shows the key).
  const optionText = (option: LookupOption | null) =>
    option ? (gridCell ? option.code || localizedName(option, isRtl) : localizedName(option, isRtl)) : '';

  if (viewMode) return <LookupDisplayField label={externalLabel ? '' : label} value={selected ? localizedName(selected, isRtl) : scalarValue == null || scalarValue === 0 ? '' : String(scalarValue)} masterRoute={masterRoute} previewEntity={previewEntity} previewKey={scalarValue} previewOption={selectedRecordOption} />;

  // -- Record preview card (grid cells, D365) ---------------------------------
  const previewOption = gridCell ? resolvedSelected : null;
  const previewRows = previewOption
    ? (previewFields
        ? previewFields.map((field) => ({
            label: field.label,
            value: field.field === 'name' ? localizedName(previewOption, isRtl) : previewOption[field.field],
          }))
        : [
            { label, value: previewOption.code },
            { label: t('fields.name', 'Name'), value: localizedName(previewOption, isRtl) },
            { label: t('fields.description', 'Description'), value: previewOption.description },
          ]
      ).filter(
        (row, index, rows) =>
          row.value != null &&
          String(row.value).trim() !== '' &&
          // Skip a name that only repeats the code.
          rows.findIndex((other) => String(other.value) === String(row.value)) === index
      )
    : [];
  // A registered master-data preview (previewEntity) replaces the option card.
  const showPreview =
    !previewEntity && previewAnchor !== null && !open && !typed && previewRows.length > 0;
  const previewId = `lookup-preview-${String(label).replace(/\W+/g, '-')}`;
  const startPreview = (anchor: HTMLElement) => {
    if (!gridCell) return;
    window.clearTimeout(previewTimerRef.current);
    previewTimerRef.current = window.setTimeout(() => setPreviewAnchor(anchor), PREVIEW_DELAY_MS);
  };
  const stopPreview = () => {
    window.clearTimeout(previewTimerRef.current);
    setPreviewAnchor(null);
  };

  return (
    <RecordPreview entity={previewEntity} masterRoute={masterRoute} recordKey={scalarValue as string | number | null} option={selectedRecordOption}>
    <Autocomplete<LookupOption, false, boolean, false>
      onFocus={(event) => startPreview(event.currentTarget)}
      sx={{
        ...standardLookupSx,
        ...(variant === 'outlined' && !gridCell ? compactOutlinedLookupSx : {}),
        ...(compact ? compactGridLookupSx : {}),
        ...(gridCell ? gridCellLookupSx : {}),
        // Lookup values read as links, in forms and in grid cells alike (D365).
        ...lookupValueSx(scalarValue),
      }}
      // D365 grid cells show a thin chevron and no clear button.
      {...(gridCell ? { popupIcon: <KeyboardArrowDownIcon sx={{ fontSize: 18 }} /> } : {})}
      open={open}
      onOpen={() => {
        setTyped(false);
        setOpen(true);
      }}
      onClose={() => {
        if (gridCell && !required && typed && search.trim() === '' && hasLookupValue(scalarValue))
          choose(null);
        setOpen(false);
        setSearch('');
        setRenderLimit(CLIENT_RENDER_CHUNK);
        highlightedRef.current = null;
      }}
      value={selected}
      options={renderedOptions}
      inputValue={searchable && typed ? search : optionText(selected)}
      // Focus selects the current text, so typing replaces it (D365).
      selectOnFocus
      onInputChange={(_, nextInput, reason) => {
        if (searchable && reason === 'input') {
          setTyped(true);
          setSearch(nextInput);
          setRenderLimit(CLIENT_RENDER_CHUNK);
          highlightedRef.current = null;
        }
      }}
      onChange={(_, option) => choose(option)}
      onBlur={() => {
        stopPreview();
        // Leaving the field: an emptied optional grid value is cleared (D365);
        // any other unfinished text reverts to the current value.
        if (gridCell && !required && typed && search.trim() === '' && hasLookupValue(scalarValue))
          choose(null);
        setTyped(false);
        setSearch('');
      }}
      // Typing then Enter picks the first match without arrowing to it.
      autoHighlight
      onHighlightChange={(_, option) => {
        highlightedRef.current = option;
      }}
      onKeyDown={(event) => {
        if (event.key === 'Escape') stopPreview();
        // D365 type-ahead: typing part of a code and pressing Tab takes the match
        // and moves on. Only when something was typed, so tabbing through an
        // untouched field never changes its value.
        if (event.key !== 'Tab' || !open || !search.trim()) return;
        // The auto-highlighted first match is not always reported; fall back to it.
        const match = highlightedRef.current ?? renderedOptions[0];
        if (match) choose(match);
      }}
      getOptionLabel={(option) => optionText(option)}
      renderOption={gridCell ? (optionProps, option) => {
        const { key, ...liProps } = optionProps as typeof optionProps & { key: React.Key };
        const name = localizedName(option, isRtl);
        return (
          <Box component="li" key={key} {...liProps} sx={gridOptionSx}>
            <Box component="span" sx={{ fontWeight: 600, whiteSpace: 'nowrap' }}>{option.code}</Box>
            <Box component="span" sx={{ color: 'text.secondary', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
              {name !== option.code ? name : ''}
            </Box>
          </Box>
        );
      } : showAllNamesInOptions ? (optionProps, option) => (
        <Box component="li" {...optionProps} aria-label={localizedName(option, isRtl)} sx={{ display: 'grid !important', gap: 0.2 }}>
          <Box component="span" sx={{ fontSize: 12, fontWeight: 800 }}>{option.code}</Box>
          <Box component="span" sx={{ fontSize: 13 }}>{option.name}</Box>
          {option.nameAlias?.trim() && option.nameAlias.trim() !== option.name.trim() ? (
            <Box component="span" dir="rtl" sx={{ fontSize: 12, color: 'text.secondary', textAlign: 'start' }}>
              {option.nameAlias}
            </Box>
          ) : null}
        </Box>
      ) : undefined}
      isOptionEqualToValue={(option, selectedOption) =>
        String(option.id) === String(selectedOption.id)
      }
      filterOptions={(availableOptions) => availableOptions}
      loading={
        usesServerDataSource &&
        (lookup.isLoading || (lookup.isFetching && !lookup.isFetchingNextPage))
      }
      disabled={disabled || readOnly}
      fullWidth={fullWidth}
      disableClearable={required || gridCell}
      slotProps={{
        listbox: { onScroll: handleScroll },
        // Grid columns are narrow: let the list be wide enough for code and name.
        ...(gridCell ? { popper: { style: { minWidth: 280 }, placement: 'bottom-start' as const } } : {}),
      }}
      renderInput={(params) => (
        <TextField
          {...params}
          // In a grid cell the cell draws the only border: no outline, no underline.
          variant={gridCell ? 'standard' : variant}
          label={externalLabel ? undefined : label}
          required={required}
          error={error}
          helperText={helperText}
          placeholder={placeholder}
          size="small"
          autoFocus={autoFocus}
          slotProps={{
            ...params.slotProps,
            ...(gridCell
              ? { input: { ...params.slotProps.input, disableUnderline: true } }
              : {}),
            inputLabel: { ...params.slotProps.inputLabel, shrink: true },
            htmlInput: { ...params.slotProps.htmlInput, readOnly: !searchable,
              ...(externalLabel ? { 'aria-label': label } : {}),
              ...(showPreview ? { 'aria-describedby': previewId } : {}) },
          }}
        />
      )}
    />
    {gridCell && (
      <Popper
        // MUI's Popper already carries role="tooltip"; the input points to it.
        id={previewId}
        open={showPreview}
        anchorEl={previewAnchor}
        placement="top-start"
        sx={{ zIndex: 1500 }}
        modifiers={[{ name: 'offset', options: { offset: [0, 10] } }]}
      >
        {/* Same card as master-data record previews: one D365 design everywhere. */}
        <RecordPreviewCard rows={previewRows} />
      </Popper>
    )}
    </RecordPreview>
  );
}

export function LookupField<TFieldValues extends FieldValues = FieldValues>({
  // Required by the shared props type because the form-bound wrapper LookupGridField binds
  // it to its Controller. This unbound component holds no form state, so it does not read it.
  name: _name,
  label,
  masterRoute,
  variant = 'outlined',
  externalLabel = false,
  value,
  onChange,
  options = [],
  disabled = false,
  readOnly = false,
  required = false,
  error = false,
  helperText,
  loading = false,
  placeholder,
  fullWidth = true,
  compact = false,
  displayMode = 'select',
  searchable = true,
  sideMode = 'server',
  lazyLoading = true,
  fetchPage,
  onFetchOptions,
  queryKey,
  pageSize,
  searchDebounceMs,
  showAllNamesInOptions = false,
  autoFocus = false,
  previewFields,
  previewEntity,
  gridCell = false,
}: LookupFieldProps<TFieldValues> & { gridCell?: boolean }): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const viewMode = useFieldViewMode();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedOptionCache, setSelectedOptionCache] = useState<LookupOption | undefined>();
  const usesServerDataSource = sideMode === 'server' && Boolean(fetchPage || onFetchOptions);

  const selectedOption =
    options.find((opt) => String(opt.id) === String(value)) ??
    (String(selectedOptionCache?.id) === String(value) ? selectedOptionCache : undefined);
  const displayValue = selectedOption
    ? `${selectedOption.code} - ${localizedName(selectedOption, isRtl)}`
    : '';
  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    onChange?.(null, undefined);
  };

  const handleSelectOption = (option: LookupOption) => {
    setSelectedOptionCache(option);
    onChange?.(option.id, option);
  };

  if (viewMode && displayMode !== 'select') return <LookupDisplayField label={externalLabel ? '' : label} value={displayValue || (value == null || value === 0 ? '' : String(value))} masterRoute={masterRoute} previewEntity={previewEntity} previewKey={Array.isArray(value) ? undefined : value} />;

  if (displayMode === 'select') {
    return (
      <SelectLookup
        label={label}
        masterRoute={masterRoute}
        variant={variant}
        externalLabel={externalLabel}
        value={value}
        onChange={onChange}
        options={options}
        disabled={disabled}
        readOnly={readOnly}
        required={required}
        error={error}
        helperText={helperText}
        placeholder={placeholder ?? t('lookups.selectPlaceholder')}
        fullWidth={fullWidth}
        compact={compact}
        searchable={searchable}
        sideMode={sideMode}
        lazyLoading={lazyLoading}
        fetchPage={fetchPage}
        onFetchOptions={onFetchOptions}
        queryKey={queryKey}
        pageSize={pageSize}
        searchDebounceMs={searchDebounceMs}
        showAllNamesInOptions={showAllNamesInOptions}
        autoFocus={autoFocus}
        previewFields={previewFields}
        previewEntity={previewEntity}
        gridCell={gridCell}
      />
    );
  }

  return (
    <>
      <RecordPreview entity={previewEntity} masterRoute={masterRoute} recordKey={Array.isArray(value) ? null : (value as string | number | null)}>
      <TextField
        variant={variant}
        label={externalLabel ? undefined : label}
        value={displayValue}
        required={required}
        disabled={disabled}
        error={error}
        helperText={helperText}
        placeholder={placeholder ?? t('lookups.selectPlaceholder')}
        fullWidth={fullWidth}
        size="small"
        onClick={() => !disabled && !readOnly && setDialogOpen(true)}
        slotProps={{
          inputLabel: { shrink: true },
          input: {
            readOnly: true,
            endAdornment: (
              <InputAdornment position="end">
                {value && !disabled && !readOnly ? (
                  <IconButton size="small" aria-label={t('actions.clear')} onClick={handleClear}>
                    <ClearIcon fontSize="small" />
                  </IconButton>
                ) : null}
                <IconButton
                  size="small"
                  aria-label={t('actions.search')}
                  disabled={disabled || readOnly}
                  onClick={() => setDialogOpen(true)}
                >
                  <SearchIcon fontSize="small" />
                </IconButton>
              </InputAdornment>
            ),
          },
        }}
        sx={{
          ...standardLookupSx,
          ...lookupValueSx(value),
          cursor: disabled || readOnly ? 'default' : 'pointer',
        }}
      />
      </RecordPreview>

      {usesServerDataSource ? (
        <ServerLookupDialog
          open={dialogOpen}
          onClose={() => setDialogOpen(false)}
          title={t('lookups.selectField', { field: label })}
          selectedId={value ?? undefined}
          onSelect={handleSelectOption}
          fetchPage={fetchPage}
          onFetchOptions={onFetchOptions}
          queryKey={queryKey}
          pageSize={pageSize}
          searchDebounceMs={searchDebounceMs}
          searchable={searchable}
          lazyLoading={lazyLoading}
        />
      ) : (
        <LookupDialog
          open={dialogOpen}
          onClose={() => setDialogOpen(false)}
          title={t('lookups.selectField', { field: label })}
          options={options}
          selectedId={value ?? undefined}
          onSelect={handleSelectOption}
          loading={loading}
          searchable={searchable}
          sideMode="client"
        />
      )}
    </>
  );
}
