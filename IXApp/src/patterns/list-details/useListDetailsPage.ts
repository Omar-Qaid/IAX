import { useCallback, useDeferredValue, useEffect, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import type { DetailValue, EnterpriseListDetailsConfig, ListDetailRecord } from './types';
import { createEnterpriseFilterCondition, matchesEnterpriseFilter, type EnterpriseFilterCondition } from '@shared/components/data-grid/EnterpriseFilterPanel';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface NumberSequenceMetadata {
  sequenceKey: string;
  mode: 'automatic' | 'manual';
  manual: boolean;
  available: boolean;
  blocked: boolean;
  previewCode: string | null;
  scope: string | null;
  message: string | null;
}

export function useListDetailsPage<T extends ListDetailRecord>(config: EnterpriseListDetailsConfig<T>) {
  const source = config.dataSource;
  const controlledRecords = source.type === 'controlled' ? source.records : null;
  const controlledLoading = source.type === 'controlled' ? Boolean(source.loading) : false;
  const controlledError = source.type === 'controlled' ? (source.error ?? null) : null;
  const remoteSourceKey = source.type === 'remote' ? source.key : '';
  const [localRecords, setLocalRecords] = useState<T[]>(source.type === 'remote' ? (source.initialRecords ?? []) : source.records);
  const [loading, setLoading] = useState(source.type === 'remote' || (source.type === 'controlled' && Boolean(source.loading)));
  const [error, setError] = useState<string | null>(source.type === 'controlled' ? (source.error ?? null) : null);
  const remoteQuery = useQuery({
    queryKey: ['list-details', remoteSourceKey],
    queryFn: ({ signal }) => source.type === 'remote' ? source.load(signal) : Promise.resolve([] as T[]),
    enabled: source.type === 'remote',
    initialData: source.type === 'remote' ? source.initialRecords : undefined,
  });
  const sequenceKey = config.numberSequence?.key ?? '';
  const numberSequenceQuery = useQuery({
    queryKey: ['number-sequence', sequenceKey],
    queryFn: async ({ signal }) => {
      const response = await apiClient.get<ApiResponse<NumberSequenceMetadata>>(
        `/v1/${sequenceKey}/number-sequence`,
        { signal }
      );
      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.message || `Number sequence ${sequenceKey} is unavailable.`);
      }
      return response.data.data;
    },
    enabled: Boolean(sequenceKey),
    staleTime: 0,
  });
  const records = controlledRecords ?? localRecords;
  const [selectedId, setSelectedId] = useState<string | null>(records[0]?.id ?? null);
  const [query, setQuery] = useState(config.initialQuery ?? '');
  const [filterVisible, setFilterVisible] = useState(true);
  const [filterPanelOpen, setFilterPanelOpen] = useState(config.advancedFilterOpenOnLoad ?? false);
  const [informationPanelOpen, setInformationPanelOpen] = useState(config.informationOpenOnLoad ?? false);
  const defaultAdvancedField = config.advancedFilter?.fields?.[0]?.id ?? 'default';
  const [draftAdvancedFilters, setDraftAdvancedFilters] = useState<EnterpriseFilterCondition[]>([createEnterpriseFilterCondition(defaultAdvancedField)]);
  const [advancedFilters, setAdvancedFilters] = useState<EnterpriseFilterCondition[]>([]);
  const [editing, setEditing] = useState(false);
  useUnsavedChanges(editing);
  const [isNew, setIsNew] = useState(false);
  const [saving, setSaving] = useState(false);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});
  const selected = records.find((record) => record.id === selectedId) ?? null;
  const [draft, setDraft] = useState<T | null>(selected);

  useEffect(() => {
    setQuery(config.initialQuery ?? '');
  }, [config.initialQuery]);

  useEffect(() => {
    if (source.type !== 'remote') return;
    setLoading(remoteQuery.isLoading || remoteQuery.isFetching);
    setError(remoteQuery.error instanceof Error ? remoteQuery.error.message : remoteQuery.error ? String(remoteQuery.error) : null);
    if (!remoteQuery.data) return;
    const loaded = remoteQuery.data;
    setLocalRecords(loaded);
    setSelectedId((current) => loaded.some((record) => record.id === current) ? current : (loaded[0]?.id ?? null));
    setDraft((current) => loaded.find((record) => record.id === current?.id) ?? loaded[0] ?? null);
  }, [remoteQuery.data, remoteQuery.error, remoteQuery.isFetching, remoteQuery.isLoading, source.type]);
  useEffect(() => {
    if (source.type !== 'controlled') return;
    setLoading(controlledLoading);
    setError(controlledError);
    if (editing) return;
    setSelectedId((current) =>
      controlledRecords?.some((record) => record.id === current) ? current : (controlledRecords?.[0]?.id ?? null)
    );
    setDraft((current) =>
      controlledRecords?.find((record) => record.id === current?.id) ?? controlledRecords?.[0] ?? null
    );
  }, [controlledError, controlledLoading, controlledRecords, editing, source.type]);

  const deferredQuery = useDeferredValue(query);
  const visibleRecords = useMemo(() => {
    const normalized = deferredQuery.trim();
    return records.filter((record) => {
      const matchesQuery = !normalized || (config.matchesSearch ? config.matchesSearch(record, normalized) : config.getPrimaryText(record).toLocaleLowerCase().includes(normalized.toLocaleLowerCase()));
      const matchesAdvanced = !config.advancedFilter || advancedFilters.every((condition) => {
        const field = config.advancedFilter?.fields?.find((candidate) => candidate.id === condition.field);
        const value = field?.getValue(record) ?? config.advancedFilter?.getValue?.(record);
        return value !== undefined ? matchesEnterpriseFilter(value, condition) : config.advancedFilter!.matches(record, condition.value);
      });
      return matchesQuery && matchesAdvanced;
    });
  }, [advancedFilters, config, deferredQuery, records]);
  const replaceRecords = (next: T[]) => { if (source.type === 'controlled') source.onRecordsChange(next); else setLocalRecords(next); };
  const refresh = useCallback(() => { if (source.type === 'controlled') void source.refresh?.(); else if (source.type === 'remote') void remoteQuery.refetch(); }, [remoteQuery, source]);
  const choose = (record: T) => { if (editing) return; setSelectedId(record.id); setDraft(record); setValidationErrors({}); };
  const startEdit = () => { if (!selected) return; setDraft(selected); setIsNew(false); setEditing(true); setValidationErrors({}); };
  const startNew = async () => {
    let record = config.createRecord();
    if (config.numberSequence) {
      setError(null);
      const result = await numberSequenceQuery.refetch();
      const metadata = result.data;
      if (!metadata?.available) {
        setError(metadata?.message || result.error?.message || 'Number sequence is unavailable.');
        return;
      }
      record = {
        ...record,
        [config.numberSequence.field]: metadata.manual ? '' : (metadata.previewCode ?? ''),
      };
    }
    setDraft(record); setSelectedId(record.id); setIsNew(true); setEditing(true); setValidationErrors({});
  };
  const save = async () => {
    if (!draft) return;
    const errors = await config.validate?.(draft) ?? {};
    if (isNew && config.numberSequence && numberSequenceQuery.data?.manual) {
      const code = draft[config.numberSequence.field];
      if (typeof code !== 'string' || !code.trim())
        errors[String(config.numberSequence.field)] = 'Code is required for a manual number sequence.';
    }
    setValidationErrors(errors); if (Object.keys(errors).length) return;
    setSaving(true); setError(null);
    try {
      const createPayload = isNew && config.numberSequence && !numberSequenceQuery.data?.manual
        ? { ...draft, [config.numberSequence.field]: null }
        : draft;
      const result = source.type === 'remote' ? await (isNew ? source.create(createPayload) : source.update(draft)) : draft;
      const created = isNew && Array.isArray(result) ? result : [result as T];
      if (created.length === 0) throw new Error('The create operation did not return any records.');
      const persisted = created[0];
      replaceRecords(isNew ? [...created, ...records] : records.map((record) => record.id === draft.id ? persisted : record));
      setDraft(persisted); setSelectedId(persisted.id); setEditing(false); setIsNew(false);
      if (isNew && config.numberSequence) await numberSequenceQuery.refetch();
    } catch (reason: unknown) { setError(reason instanceof Error ? reason.message : String(reason)); } finally { setSaving(false); }
  };
  const cancel = () => { if (isNew) setSelectedId(records[0]?.id ?? null); setDraft(isNew ? (records[0] ?? null) : selected); setEditing(false); setIsNew(false); setValidationErrors({}); };
  const remove = async () => {
    if (!selected) return; setSaving(true); setError(null);
    try { if (source.type === 'remote') await source.delete(selected); const remaining = records.filter((record) => record.id !== selected.id); replaceRecords(remaining); setSelectedId(remaining[0]?.id ?? null); setDraft(remaining[0] ?? null); }
    catch (reason: unknown) { setError(reason instanceof Error ? reason.message : String(reason)); } finally { setSaving(false); }
  };
  const changeValue = (name: string, value: DetailValue) => { setValidationErrors((current) => { const next = { ...current }; delete next[name]; return next; }); setDraft((current) => current ? config.setValues(current, { ...config.getValues(current), [name]: value }) : current); };
  const changeHeader = (fieldId: string, value: DetailValue) => { setValidationErrors((current) => { const next = { ...current }; delete next[fieldId]; return next; }); setDraft((current) => { const field = config.headerFields.find((candidate) => candidate.id === fieldId); return current && field ? field.setValue(current, value) : current; }); };
  return { records, selectedId, selected, draft, query, filterVisible, filterPanelOpen, informationPanelOpen, draftAdvancedFilters, editing, isNew, saving, loading, error, validationErrors, visibleRecords, numberSequenceMetadata: numberSequenceQuery.data ?? null, numberSequenceLoading: numberSequenceQuery.isFetching, setQuery, setDraftAdvancedFilters, applyAdvancedFilter: () => setAdvancedFilters(draftAdvancedFilters.filter((condition) => condition.value.trim())), resetAdvancedFilter: () => { setDraftAdvancedFilters([createEnterpriseFilterCondition(defaultAdvancedField)]); setAdvancedFilters([]); }, toggleFilter: () => config.advancedFilter ? setFilterPanelOpen((open) => !open) : setFilterVisible((visible) => !visible), toggleInformation: () => setInformationPanelOpen((open) => !open), choose, startEdit, startNew, save, cancel, remove, refresh, changeValue, changeHeader, changeRecord: setDraft };
}
