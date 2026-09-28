import React, { useMemo } from 'react';
import { Box } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { loadWfPerformerSqlSchema } from '../api/wfPerformerApi';

interface Props {
  sqlTable: string;
  sqlField: string;
  disabled: boolean;
  onChange: (values: { sqlTable: string; sqlField: string }) => void;
}

export function WfPerformerDatabaseQueryFields({
  sqlTable,
  sqlField,
  disabled,
  onChange,
}: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const schema = useQuery({
    queryKey: ['workflow', 'performer-sql-schema'],
    queryFn: ({ signal }) => loadWfPerformerSqlSchema(signal),
  });
  const tableOptions = useMemo(
    () =>
      Object.keys(schema.data ?? {})
        .sort((left, right) => left.localeCompare(right))
        .map((table) => ({ id: table, code: table, name: table })),
    [schema.data]
  );
  const fieldOptions = useMemo(
    () =>
      (schema.data?.[sqlTable] ?? []).map((field) => ({
        id: field,
        code: field,
        name: field,
      })),
    [schema.data, sqlTable]
  );

  return (
    <Box sx={{ display: 'grid', gap: 1 }}>
      <AppLookupField
        name="sqlTable"
        label={t('wfPerformers.fields.sqlTable')}
        value={sqlTable}
        onChange={(value) => onChange({ sqlTable: String(value ?? ''), sqlField: '' })}
        options={tableOptions}
        disabled={disabled || schema.isLoading}
        displayMode="select"
        searchable
        lazyLoading
        pageSize={25}
        required
      />
      <AppLookupField
        name="sqlField"
        label={t('wfPerformers.fields.sqlField')}
        value={sqlField}
        onChange={(value) => onChange({ sqlTable, sqlField: String(value ?? '') })}
        options={fieldOptions}
        disabled={disabled || schema.isLoading || !sqlTable}
        displayMode="select"
        searchable
        lazyLoading
        pageSize={25}
        required
      />
    </Box>
  );
}
