import React, { useMemo, useState } from 'react';
import { Box, MenuItem, TextField, Typography } from '@mui/material';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface ExchangeRatePair {
  id: string;
  exchangeRateType: string;
  fromCurrency: string;
  toCurrency: string;
  conversionFactor: number;
  values: DetailValues;
}

interface ExchangeRateLine {
  id: string;
  startDate: string;
  exchangeRate: number;
  exchangeValue: string;
}

const INITIAL_PAIRS: ExchangeRatePair[] = ['AED', 'EUR', 'GBP', 'QAR', 'USD'].map((fromCurrency) => ({
  id: `pair-${fromCurrency.toLowerCase()}`,
  exchangeRateType: 'default',
  fromCurrency,
  toCurrency: 'SAR',
  conversionFactor: 1,
  values: {},
}));

const INITIAL_LINES: ExchangeRateLine[] = [{
  id: 'rate-aed-sar-2024-07-01',
  startDate: '2024-07-01',
  exchangeRate: 1.02339,
  exchangeValue: '1.02339 SAR for 1 AED',
}];

export function ExchangeRatePage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const [records, setRecords] = useState(INITIAL_PAIRS);
  const [exchangeRateType, setExchangeRateType] = useState('default');
  const [rateLines, setRateLines] = useState(INITIAL_LINES);
  const [selectedRateIds, setSelectedRateIds] = useState<(string | number)[]>([INITIAL_LINES[0].id]);
  const [fromDate, setFromDate] = useState('2026-07-03');
  const [toDate, setToDate] = useState('2026-09-01');

  const pairColumns = useMemo<ColumnDef<ExchangeRatePair>[]>(() => [
    { field: 'fromCurrency', headerName: 'exchangeRates.fields.fromCurrency', width: 122, pinned: 'left' },
    { field: 'toCurrency', headerName: 'exchangeRates.fields.toCurrency', width: 94 },
    { field: 'conversionFactor', headerName: 'exchangeRates.fields.conversionFactor', minWidth: 132, flex: 1 },
  ], [currentLanguage.code]);

  const lineColumns = useMemo<ColumnDef<ExchangeRateLine>[]>(() => [
    { field: 'startDate', headerName: 'exchangeRates.fields.startDate', width: 185, renderCell: ({ value }) => formatDate(String(value), currentLanguage.code) },
    { field: 'exchangeRate', headerName: 'exchangeRates.fields.exchangeRate', width: 115 },
    { field: 'exchangeValue', headerName: 'exchangeRates.fields.exchangeValue', minWidth: 300, flex: 1 },
  ], [currentLanguage.code]);

  const headerField = (id: 'fromCurrency' | 'toCurrency') => ({
    id,
    label: t(`exchangeRates.fields.${id}`),
    getValue: (record: ExchangeRatePair) => record[id],
    setValue: (record: ExchangeRatePair, value: string | number | boolean) => ({ ...record, [id]: String(value) }),
  });

  const config: EnterpriseListDetailsConfig<ExchangeRatePair> = {
    dataSource: { type: 'controlled', records, onRecordsChange: setRecords },
    createRecord: () => ({ id: `pair-${Date.now()}`, exchangeRateType, fromCurrency: '', toCurrency: 'SAR', conversionFactor: 1, values: {} }),
    getPrimaryText: (record) => record.fromCurrency,
    getSecondaryText: (record) => record.toCurrency,
    matchesSearch: (record, query) => `${record.fromCurrency} ${record.toCurrency}`.toLocaleLowerCase(currentLanguage.code).includes(query.toLocaleLowerCase(currentLanguage.code)),
    getValues: (record) => record.values,
    setValues: (record, values) => ({ ...record, values }),
    headerFields: [
      headerField('fromCurrency'),
      headerField('toCurrency'),
      { id: 'conversionFactor', label: t('exchangeRates.fields.conversionFactor'), type: 'number', getValue: (record) => record.conversionFactor, setValue: (record, value) => ({ ...record, conversionFactor: Number(value) }) },
    ],
    sections: [{
      id: 'exchangeRates',
      title: t('exchangeRates.sections.addRemove'),
      content: <TabularDetailPanel
        rows={rateLines}
        columns={lineColumns}
        addLabel={t('actions.add')}
        removeLabel={t('actions.remove')}
        selectedIds={selectedRateIds}
        onSelectionChange={setSelectedRateIds}
        onAdd={() => {
          const id = `rate-${Date.now()}`;
          setRateLines((current) => [...current, { id, startDate: fromDate, exchangeRate: 1, exchangeValue: t('exchangeRates.defaultValue') }]);
          setSelectedRateIds([id]);
        }}
        onRemove={() => {
          setRateLines((current) => current.filter((line) => !selectedRateIds.includes(line.id)));
          setSelectedRateIds([]);
        }}
        storageKey="foundation.exchange-rates.lines"
        filterContent={<Box>
          <Typography sx={{ mb: 0.75, fontSize: '0.72rem' }}>{t('exchangeRates.displayDateRange')}</Typography>
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            <DateField label={t('exchangeRates.fields.fromDate')} value={fromDate} onChange={setFromDate} />
            <DateField label={t('exchangeRates.fields.toDate')} value={toDate} onChange={setToDate} />
          </Box>
        </Box>}
      />,
    }],
    presentation: {
      mode: 'grid',
      columns: pairColumns,
      storageKey: 'foundation.exchange-rates.pairs',
      listWidth: 370,
      headerMaxWidth: 370,
      masterRowHeight: 31,
      masterHeaderHeight: 31,
      headerContent: <Box sx={{ px: 1, pb: 1 }}>
        <Typography sx={{ mb: 0.25, fontSize: '0.6875rem', color: 'text.secondary' }}>{t('exchangeRates.fields.type')}</Typography>
        <TextField select size="small" value={exchangeRateType} onChange={(event) => setExchangeRateType(event.target.value)} sx={{ width: 153, '& .MuiInputBase-root': { height: 29, fontSize: '0.75rem' } }}>
          <MenuItem value="default">{t('exchangeRates.options.default')}</MenuItem>
        </TextField>
      </Box>,
    },
    permissions: { view: 'currency.view', create: 'currency.manage', edit: 'currency.manage', delete: 'currency.manage' },
    validate: (record) => ({
      ...(!record.fromCurrency.trim() ? { fromCurrency: t('validation.required', { field: t('exchangeRates.fields.fromCurrency') }) } : {}),
      ...(!record.toCurrency.trim() ? { toCurrency: t('validation.required', { field: t('exchangeRates.fields.toCurrency') }) } : {}),
    }),
    advancedFilter: { fieldLabel: t('exchangeRates.fields.fromCurrency'), getValue: (record) => record.fromCurrency, matches: (record, value) => record.fromCurrency.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)) },
    commands: [{ id: 'options', label: t('customerCommands.options') }],
  };

  return <ListDetailsPage variant="enterprise" title={t('pages.exchangeRates.title')} config={config} />;
}

function DateField({ label, value, onChange }: { label: string; value: string; onChange: (value: string) => void }): React.ReactElement {
  return <Box><Typography sx={{ mb: 0.2, fontSize: '0.6875rem', color: 'text.secondary' }}>{label}</Typography><TextField type="date" size="small" value={value} onChange={(event) => onChange(event.target.value)} sx={{ width: 154, '& .MuiInputBase-root': { height: 30, fontSize: '0.75rem' } }} /></Box>;
}

function formatDate(value: string, locale: string): string {
  const [year, month, day] = value.split('-').map(Number);
  return year && month && day ? new Intl.DateTimeFormat(locale, { year: 'numeric', month: 'numeric', day: 'numeric', timeZone: 'UTC' }).format(new Date(Date.UTC(year, month - 1, day))) : value;
}
