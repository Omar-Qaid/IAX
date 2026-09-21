import React, { useMemo, useState } from 'react';
import { Autocomplete, TextField, InputAdornment, IconButton } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import { LookupDialog } from './LookupDialog';
import type { LookupFieldProps, LookupOption } from './types';
import { useLookupGridField } from '@shared/hooks/useLookupGridField';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { FieldValues } from 'react-hook-form';

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
}: LookupFieldProps) {
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
    queryKey: queryKey ?? ['lookup-field', label],
    fetchPage: resolvedFetchPage,
    enabled: open,
    pageSize,
    search: searchable ? search : '',
    debounceMs: searchDebounceMs,
  });
  const selected =
    options.find((option) => String(option.id) === String(scalarValue)) ??
    (String(selectedCache?.id) === String(scalarValue) ? selectedCache : null);
  const handleScroll = (event: React.UIEvent<HTMLUListElement>) => {
    if (!lazyLoading || !lookup.hasNextPage || lookup.isFetchingNextPage) return;
    const list = event.currentTarget;
    if (list.scrollHeight - list.scrollTop - list.clientHeight <= 80) void lookup.fetchNextPage();
  };

  return (
    <Autocomplete<LookupOption, false, boolean, false>
      open={open}
      onOpen={() => setOpen(true)}
      onClose={() => {
        setOpen(false);
        setSearch('');
      }}
      value={selected}
      options={lookup.rows}
      inputValue={open && searchable ? search : selected?.name || ''}
      onInputChange={(_, nextInput, reason) => {
        if (searchable && reason === 'input') setSearch(nextInput);
      }}
      onChange={(_, option) => {
        setSelectedCache(option);
        setOpen(false);
        setSearch('');
        onChange?.(option?.id ?? null, option ?? undefined);
      }}
      getOptionLabel={(option) => option.name}
      isOptionEqualToValue={(option, selectedOption) =>
        String(option.id) === String(selectedOption.id)
      }
      filterOptions={(availableOptions) => availableOptions}
      loading={lookup.isLoading || (lookup.isFetching && !lookup.isFetchingNextPage)}
      disabled={disabled || readOnly}
      fullWidth={fullWidth}
      disableClearable={required}
      slotProps={{ listbox: { onScroll: handleScroll } }}
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          required={required}
          error={error}
          helperText={helperText}
          placeholder={placeholder}
          size="small"
          slotProps={{
            ...params.slotProps,
            htmlInput: { ...params.slotProps.htmlInput, readOnly: !searchable },
          }}
        />
      )}
    />
  );
}

export function LookupField<TFieldValues extends FieldValues = FieldValues>({
  name,
  label,
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
}: LookupFieldProps<TFieldValues>): React.ReactElement {
  const { t } = useAppTranslation();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedOptionCache, setSelectedOptionCache] = useState<LookupOption | undefined>();
  const usesServerDataSource = sideMode === 'server' && Boolean(fetchPage || onFetchOptions);

  const selectedOption =
    options.find((opt) => String(opt.id) === String(value)) ??
    (String(selectedOptionCache?.id) === String(value) ? selectedOptionCache : undefined);
  const displayValue = selectedOption ? `${selectedOption.code} - ${selectedOption.name}` : '';
  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    onChange?.(null, undefined);
  };

  const handleSelectOption = (option: LookupOption) => {
    setSelectedOptionCache(option);
    onChange?.(option.id, option);
  };

  if (displayMode === 'select') {
    return (
      <SelectLookup
        name={name}
        label={label}
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
      />
    );
  }

  return (
    <>
      <TextField
        label={label}
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
        sx={{ cursor: disabled || readOnly ? 'default' : 'pointer' }}
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
