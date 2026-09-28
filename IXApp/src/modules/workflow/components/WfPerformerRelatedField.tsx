import React, { useEffect, useMemo, useState } from 'react';
import { Box } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { localizedName } from '@shared/utilities/localizedName';
import { wfActivityControlApi } from '../api/wfActivityControlApi';
import { wfProcessApi } from '../api/wfProcessApi';
import { wfRequestControlApi } from '../api/wfRequestControlApi';

interface Props {
  performerTypeId: 2 | 3;
  value: number;
  disabled: boolean;
  onChange: (value: number) => void;
}

interface PerformerControlOption {
  recId: number;
  processId: number;
  code: string | null;
  name: string | null;
  nameAlias?: string | null;
}

export function WfPerformerRelatedField({
  performerTypeId,
  value,
  disabled,
  onChange,
}: Props): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const [processId, setProcessId] = useState(0);
  const processes = useQuery({
    queryKey: ['workflow', 'performer-processes'],
    queryFn: ({ signal }) => wfProcessApi.list(signal),
  });
  const controls = useQuery<PerformerControlOption[]>({
    queryKey: ['workflow', 'performer-controls', performerTypeId],
    queryFn: async ({ signal }) => {
      const rows =
        performerTypeId === 2
          ? await wfRequestControlApi.list(signal)
          : await wfActivityControlApi.list(signal);
      return rows.map((control) => ({
        recId: control.recId,
        processId: control.processId,
        code: control.code,
        name: control.name,
        nameAlias: control.nameAlias,
      }));
    },
  });

  useEffect(() => {
    if (!value) return;
    const selected = controls.data?.find((control) => control.recId === value);
    if (selected && selected.processId !== processId) setProcessId(selected.processId);
  }, [controls.data, processId, value]);

  const processOptions = useMemo(
    () =>
      (processes.data ?? []).map((process) => ({
        id: process.recId,
        code: process.code ?? '',
        name: localizedName(process, isRtl),
      })),
    [isRtl, processes.data]
  );
  const controlOptions = useMemo(
    () =>
      (controls.data ?? [])
        .filter((control) => control.processId === processId)
        .map((control) => ({
          id: control.recId,
          code: control.code ?? '',
          name: localizedName(control, isRtl),
        })),
    [controls.data, isRtl, processId]
  );

  return (
    <Box sx={{ display: 'grid', gap: 1 }}>
      <AppLookupField
        name="performerProcessId"
        label={t('wfPerformers.fields.process')}
        value={processId}
        onChange={(next) => {
          setProcessId(Number(next) || 0);
          onChange(0);
        }}
        options={processOptions}
        disabled={disabled || processes.isLoading}
        displayMode="select"
        searchable
        lazyLoading
        pageSize={25}
        required
      />
      <AppLookupField
        name="relatedField"
        label={
          performerTypeId === 2
            ? t('wfPerformers.fields.requestControl')
            : t('wfPerformers.fields.activityControl')
        }
        value={value}
        onChange={(next) => onChange(Number(next) || 0)}
        options={controlOptions}
        disabled={disabled || controls.isLoading || processId <= 0}
        displayMode="select"
        searchable
        lazyLoading
        pageSize={25}
        required
      />
    </Box>
  );
}
