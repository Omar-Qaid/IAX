import React, { useMemo } from 'react';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { unitOfMeasureApi, type UnitOfMeasureRecord } from '../api/unitOfMeasureApi';
import { UnitConversionEditor } from '../components/UnitConversionEditor';

const empty = (): UnitOfMeasureRecord => ({ id: `new-${crypto.randomUUID()}`, recId: 0, symbol: '', unitOfMeasureClass: 0, systemOfUnits: 0, decimalPrecision: 0, conversions: [] });

export function UnitOfMeasurePage(): React.ReactElement {
  const { t } = useAppTranslation();
  const config = useMemo<EnterpriseListDetailsConfig<UnitOfMeasureRecord>>(() => ({
    recordTableName: 'UnitOfMeasure',
    dataSource: { type: 'remote', key: 'units-of-measure', load: unitOfMeasureApi.list, create: unitOfMeasureApi.create, update: unitOfMeasureApi.update, delete: unitOfMeasureApi.delete },
    createRecord: empty,
    getPrimaryText: (x) => x.symbol,
    getSecondaryText: (x) => ['Quantity', 'Length', 'Area', 'Volume', 'Mass', 'Time', 'Temperature', 'Electricity'][x.unitOfMeasureClass] ?? '',
    matchesSearch: (x, q) => x.symbol.toLowerCase().includes(q.toLowerCase()),
    getValues: (x) => ({ unitOfMeasureClass: x.unitOfMeasureClass, systemOfUnits: x.systemOfUnits, decimalPrecision: x.decimalPrecision }) as DetailValues,
    setValues: (x, v) => ({ ...x, unitOfMeasureClass: Number(v.unitOfMeasureClass), systemOfUnits: Number(v.systemOfUnits), decimalPrecision: Number(v.decimalPrecision) }),
    headerFields: [{ id: 'symbol', label: t('unitOfMeasure.symbol', 'Symbol'), getValue: (x) => x.symbol, setValue: (x, v) => ({ ...x, symbol: String(v).trim() }) }],
    sections: ({ record }) => [
      { id: 'general', title: t('common.general', 'General'), defaultExpanded: true, groups: [{ id: 'classification', fields: [
        { name: 'unitOfMeasureClass', label: t('unitOfMeasure.unitClass', 'Unit class'), type: 'select', options: ['Quantity', 'Length', 'Area', 'Volume', 'Mass', 'Time', 'Temperature', 'Electricity'].map((label, value) => ({ value: String(value), label })) },
        { name: 'systemOfUnits', label: t('unitOfMeasure.system', 'System of units'), type: 'select', options: [{ value: '0', label: 'Metric' }, { value: '1', label: 'Imperial' }] },
        { name: 'decimalPrecision', label: t('unitOfMeasure.precision', 'Decimal precision'), type: 'number' },
      ] }] },
      { id: 'conversions', title: t('unitOfMeasure.conversions', 'Unit conversions'), defaultExpanded: true, content: <UnitConversionEditor unit={record} /> },
    ],
    permissions: { view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW, create: PERMISSIONS.INVENTORY_TRANSACTION_CREATE, edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT, delete: PERMISSIONS.INVENTORY_TRANSACTION_DELETE },
    validate: (x) => ({ ...(!x.symbol.trim() ? { symbol: t('unitOfMeasure.symbolRequired', 'Symbol is required.') } : {}), ...(x.decimalPrecision < 0 || x.decimalPrecision > 6 ? { decimalPrecision: t('unitOfMeasure.precisionRange', 'Precision must be between 0 and 6.') } : {}) }),
    advancedFilter: { fieldLabel: t('unitOfMeasure.symbol', 'Symbol'), getValue: (x) => x.symbol, matches: (x, v) => x.symbol.toLowerCase().includes(v.toLowerCase()) },
    presentation: { mode: 'list', listWidth: 270, listResizable: true },
  }), [t]);
  return <ListDetailsPage variant="enterprise" title={t('unitOfMeasure.title', 'Units of measure')} config={config} />;
}
