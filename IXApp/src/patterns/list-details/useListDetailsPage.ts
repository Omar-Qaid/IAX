import { useCallback, useEffect, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import type { DetailValue, EnterpriseListDetailsConfig, ListDetailRecord } from './types';
import { createEnterpriseFilterCondition, matchesEnterpriseFilter, type EnterpriseFilterCondition } from '@shared/components/data-grid/EnterpriseFilterPanel';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';

export function useListDetailsPage<T extends ListDetailRecord>(config: EnterpriseListDetailsConfig<T>) {
  const source = config.dataSource;
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
  const records = source.type === 'controlled' ? source.records : localRecords;
  const [selectedId, setSelectedId] = useState<string | null>(records[0]?.id ?? null);
  const [query, setQuery] = useState('');
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
    if (source.type !== 'remote') return;
    setLoading(remoteQuery.isLoading || remoteQuery.isFetching);
    setError(remoteQuery.error instanceof Error ? remoteQuery.error.message : remoteQuery.error ? String(remoteQuery.error) : null);
    if (!remoteQuery.data) return;
    const loaded = remoteQuery.data;
    setLocalRecords(loaded);
    setSelectedId((current) => loaded.some((record) => record.id === current) ? current : (loaded[0]?.id ?? null));
    setDraft((current) => loaded.find((record) => record.id === current?.id) ?? loaded[0] ?? null);
  }, [remoteQuery.data, remoteQuery.error, remoteQuery.isFetching, remoteQuery.isLoading, source.type]);
  useEffect(() => { if (source.type === 'controlled') { setLoading(Boolean(source.loading)); setError(source.error ?? null); } }, [source]);

  const visibleRecords = useMemo(() => {
    const normalized = query.trim();
    return records.filter((record) => {
      const matchesQuery = !normalized || (config.matchesSearch ? config.matchesSearch(record, normalized) : config.getPrimaryText(record).toLocaleLowerCase().includes(normalized.toLocaleLowerCase()));
      const matchesAdvanced = !config.advancedFilter || advancedFilters.every((condition) => {
        const field = config.advancedFilter?.fields?.find((candidate) => candidate.id === condition.field);
        const value = field?.getValue(record) ?? config.advancedFilter?.getValue?.(record);
        return value !== undefined ? matchesEnterpriseFilter(value, condition) : config.advancedFilter!.matches(record, condition.value);
      });
      return matchesQuery && matchesAdvanced;
    });
  }, [advancedFilters, config, query, records]);
  const replaceRecords = (next: T[]) => { if (source.type === 'controlled') source.onRecordsChange(next); else setLocalRecords(next); };
  const refresh = useCallback(() => { if (source.type === 'controlled') void source.refresh?.(); else if (source.type === 'remote') void remoteQuery.refetch(); }, [remoteQuery, source]);
  const choose = (record: T) => { if (editing) return; setSelectedId(record.id); setDraft(record); setValidationErrors({}); };
  const startEdit = () => { if (!selected) return; setDraft(selected); setIsNew(false); setEditing(true); setValidationErrors({}); };
  const startNew = () => { const record = config.createRecord(); setDraft(record); setSelectedId(record.id); setIsNew(true); setEditing(true); setValidationErrors({}); };
  const save = async () => {
    if (!draft) return;
    const errors = await config.validate?.(draft) ?? {}; setValidationErrors(errors); if (Object.keys(errors).length) return;
    setSaving(true); setError(null);
    try {
      const persisted = source.type === 'remote' ? await (isNew ? source.create(draft) : source.update(draft)) : draft;
      replaceRecords(isNew ? [persisted, ...records] : records.map((record) => record.id === draft.id ? persisted : record));
      setDraft(persisted); setSelectedId(persisted.id); setEditing(false); setIsNew(false);
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
  return { records, selectedId, selected, draft, query, filterVisible, filterPanelOpen, informationPanelOpen, draftAdvancedFilters, editing, isNew, saving, loading, error, validationErrors, visibleRecords, setQuery, setDraftAdvancedFilters, applyAdvancedFilter: () => setAdvancedFilters(draftAdvancedFilters.filter((condition) => condition.value.trim())), resetAdvancedFilter: () => { setDraftAdvancedFilters([createEnterpriseFilterCondition(defaultAdvancedField)]); setAdvancedFilters([]); }, toggleFilter: () => config.advancedFilter ? setFilterPanelOpen((open) => !open) : setFilterVisible((visible) => !visible), toggleInformation: () => setInformationPanelOpen((open) => !open), choose, startEdit, startNew, save, cancel, remove, refresh, changeValue, changeHeader, changeRecord: setDraft };
}
