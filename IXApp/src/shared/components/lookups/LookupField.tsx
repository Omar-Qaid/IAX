import { localizedName } from '@shared/utilities/localizedName';
import React, { useMemo, useState } from 'react';
import { Autocomplete, Box, TextField, InputAdornment, IconButton } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import { LookupDialog } from './LookupDialog';
import type { LookupFieldProps, LookupOption } from './types';
import { useLookupGridField } from '@shared/hooks/useLookupGridField';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { FieldValues } from 'react-hook-form';
import { LookupTitleMenu } from './LookupTitleMenu';
import { lookupValueSx } from './lookupValueStyle';
import { AppDisplayField } from '@shared/components/fields/AppDisplayField';
import { useFieldViewMode } from '@shared/components/fields/FieldViewModeContext';

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
  name,
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
  searchable = true,
  sideMode = 'server',
  lazyLoading = true,
  fetchPage,
  onFetchOptions,
  queryKey,
  pageSize = 50,
  searchDebounceMs = 350,
  showAllNamesInOptions = false,
}: LookupFieldProps) {
  const { isRtl } = useAppTranslation();
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
    enabled: usesServerDataSource && (open || scalarValue != null),
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
  const selected =
    options.find((option) => String(option.id) === String(scalarValue)) ??
    lookup.rows.find((option) => String(option.id) === String(scalarValue)) ??
    (String(selectedCache?.id) === String(scalarValue) ? selectedCache : null);
  const handleScroll = (event: React.UIEvent<HTMLUListElement>) => {
    if (!lazyLoading || !lookup.hasNextPage || lookup.isFetchingNextPage) return;
    const list = event.currentTarget;
    if (list.scrollHeight - list.scrollTop - list.clientHeight <= 80) void lookup.fetchNextPage();
  };

  if (viewMode) return <AppDisplayField label={externalLabel ? '' : label} value={selected ? localizedName(selected, isRtl) : scalarValue == null || scalarValue === 0 ? '' : String(scalarValue)} lookup />;

  return (
    <Autocomplete<LookupOption, false, boolean, false>
      sx={{ ...standardLookupSx, ...(variant === 'outlined' ? compactOutlinedLookupSx : {}), ...lookupValueSx(scalarValue) }}
      open={open}
      onOpen={() => setOpen(true)}
      onClose={() => {
        setOpen(false);
        setSearch('');
      }}
      value={selected}
      options={visibleOptions}
      inputValue={open && searchable ? search : localizedName(selected, isRtl)}
      onInputChange={(_, nextInput, reason) => {
        if (searchable && reason === 'input') setSearch(nextInput);
      }}
      onChange={(_, option) => {
        setSelectedCache(option);
        setOpen(false);
        setSearch('');
        onChange?.(option?.id ?? null, option ?? undefined);
      }}
      getOptionLabel={(option) => localizedName(option, isRtl)}
      renderOption={showAllNamesInOptions ? (optionProps, option) => (
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
      disableClearable={required}
      slotProps={{ listbox: { onScroll: handleScroll } }}
      renderInput={(params) => (
        <TextField
          {...params}
          variant={variant}
          label={externalLabel ? undefined : <LookupTitleMenu name={name} label={label} masterRoute={masterRoute} />}
          required={required}
          error={error}
          helperText={helperText}
          placeholder={placeholder}
          size="small"
          slotProps={{
            ...params.slotProps,
            inputLabel: { ...params.slotProps.inputLabel, shrink: true },
            htmlInput: { ...params.slotProps.htmlInput, readOnly: !searchable,
              ...(externalLabel ? { 'aria-label': label } : {}) },
          }}
        />
      )}
    />
  );
}

export function LookupField<TFieldValues extends FieldValues = FieldValues>({
  name,
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
}: LookupFieldProps<TFieldValues>): React.ReactElement {
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

  if (viewMode && displayMode !== 'select') return <AppDisplayField label={externalLabel ? '' : label} value={displayValue || (value == null || value === 0 ? '' : String(value))} lookup />;

  if (displayMode === 'select') {
    return (
      <SelectLookup
        name={name}
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
        searchable={searchable}
        sideMode={sideMode}
        lazyLoading={lazyLoading}
        fetchPage={fetchPage}
        onFetchOptions={onFetchOptions}
        queryKey={queryKey}
        pageSize={pageSize}
        searchDebounceMs={searchDebounceMs}
        showAllNamesInOptions={showAllNamesInOptions}
      />
    );
  }

  return (
    <>
      <TextField
        variant={variant}
        label={externalLabel ? undefined : <LookupTitleMenu name={name} label={label} masterRoute={masterRoute} />}
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

      {usesServerDataSource ? (
        <ServerLookupDialog
          open={dialogOpen}
          onClose={() => setDialogOpen(false)}
          title={t('lookups.selectField', { field: label })}
          selectedId={value}
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
          selectedId={value}
          onSelect={handleSelectOption}
          loading={loading}
          searchable={searchable}
          sideMode="client"
        />
      )}
    </>
  );
}
