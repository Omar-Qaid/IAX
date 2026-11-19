import React from 'react';
import { Box, IconButton, Stack, Switch, TextField, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { salesOrderLinesApi } from '../api/salesOrderLinesApi';
import { fetchSalesOrderUnitOptions } from '../api/salesOrderLookupPages';
import { POSTAL_ADDRESS_RECORD_PREVIEW } from '@shared/components/logistics/postalAddressRecordPreview';
import { RecordPreview } from '@shared/components/record-preview';
import { LookupField } from '@shared/components/lookups/LookupField';
import { LookupDisplayField } from '@shared/components/lookups/LookupDisplayField';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import { synchronizeSalesLineDiscount } from '../lines/salesLineDiscount';
import { salesOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';
import { lineMasters, toAddressPreview, type DetailLine } from './salesOrderDetailsConfig';
import type { SalesOrderDocumentView } from './useSalesOrderDocument';
import type { buildSalesOrderTotals } from './salesOrderTotals';
import type { buildSalesOrderHeaderPanel } from './SalesOrderHeaderPanel';

type Scope = SalesOrderDocumentView &
  ReturnType<typeof buildSalesOrderTotals> &
  ReturnType<typeof buildSalesOrderHeaderPanel>;

export function buildSalesOrderLineFields(scope: Scope) {
  const {
    company,
    activeHeader,
    selectedLine,
    saveLineDetail,
    openDeliveryAddressDrawer,
    lineDetailDraft,
    setLineDetailDraft,
    savingLineDetail,
    t,
    lookupsQuery,
    deliveryAddressesQuery,
    dimensionsQuery,
    taxGroupsQuery,
    field,
  } = scope;
  const displayedLine = lineDetailDraft ?? selectedLine;
  const lineDetailField = (
    name: keyof DetailLine,
    label: string,
    type: 'text' | 'number' | 'date' = 'text'
  ) => {
    if (!displayedLine) return null;
    if (!activeHeader || name === 'itemNumber')
      return field(label, String(displayedLine[name] ?? ''), name !== 'itemNumber');
    return (
      <TextField
        key={name}
        fullWidth
        size="small"
        variant="outlined"
        label={label}
        type={type}
        value={lineDetailDraft?.[name] ?? ''}
        disabled={savingLineDetail}
        slotProps={{ inputLabel: { shrink: true } }}
        onChange={(event) => {
          const value = type === 'number' ? Number(event.target.value) : event.target.value;
          setLineDetailDraft((draft) => {
            if (!draft) return draft;
            if (
              typeof value === 'number' &&
              (name === 'quantity' ||
                name === 'unitPrice' ||
                name === 'priceUnit' ||
                name === 'lineDiscount' ||
                name === 'lineDiscountPercent')
            ) {
              const updated = synchronizeSalesLineDiscount(draft, name, value);
              return name === 'unitPrice' || name === 'priceUnit'
                ? { ...updated, usePriceAgreement: false }
                : updated;
            }
            return { ...draft, [name]: value };
          });
        }}
        onBlur={() => void saveLineDetail()}
      />
    );
  };
  const lineMultilineField = (name: 'description' | 'deliveryName', label: string) => {
    if (!displayedLine) return null;
    if (!activeHeader) return field(label, displayedLine[name], true);
    return (
      <TextField
        fullWidth
        size="small"
        variant="outlined"
        label={label}
        multiline
        minRows={4}
        value={lineDetailDraft?.[name] ?? ''}
        disabled={savingLineDetail}
        onChange={(event) => {
          const value = event.target.value;
          setLineDetailDraft((draft) => (draft ? { ...draft, [name]: value } : draft));
        }}
        onBlur={() => void saveLineDetail()}
      />
    );
  };
  const dimensionField = (name: 'site' | 'warehouse', label: string) => {
    if (!displayedLine) return null;
    if (!activeHeader) return lineMasterValue(name, label, displayedLine[name]);
    const options =
      name === 'site'
        ? (dimensionsQuery.data?.sites ?? [])
        : (dimensionsQuery.data?.warehouses ?? []).filter(
            (warehouse) => !lineDetailDraft?.site || warehouse.siteId === lineDetailDraft.site
          );
    return lineLookupShell(
      <LookupField
        key={name}
        name={name}
        masterRoute={lineMasters[name]?.masterRoute}
        label={label}
        value={lineDetailDraft?.[name] ?? ''}
        options={options}
        displayMode="select"
        searchable
        lazyLoading={false}
        disabled={savingLineDetail || dimensionsQuery.isLoading}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = {
            ...lineDetailDraft,
            [name]: String(value ?? ''),
            ...(name === 'site' ? { warehouse: '' } : {}),
          };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  // Read-only line master value with its D365 record preview.
  // View mode uses the shared lookup display: open-details icon on the label (only
  // in view mode, like D365) and the record card built from the loaded option.
  const lineMasterOptions = (name: keyof DetailLine): { code: string; name: string }[] => {
    switch (name) {
      case 'site':
        return dimensionsQuery.data?.sites ?? [];
      case 'warehouse':
        return dimensionsQuery.data?.warehouses ?? [];
      case 'taxGroup':
        return taxGroupsQuery.data?.salesTaxGroups ?? [];
      case 'taxItemGroup':
        return taxGroupsQuery.data?.itemSalesTaxGroups ?? [];
      case 'deliveryMode':
        return (lookupsQuery.data?.deliveryModes ?? []).map((o) => ({
          code: o.value,
          name: o.label,
        }));
      case 'deliveryTerms':
        return (lookupsQuery.data?.deliveryTerms ?? []).map((o) => ({
          code: o.value,
          name: o.label,
        }));
      default:
        return [];
    }
  };
  const lineMasterValue = (name: keyof DetailLine, label: string, value?: unknown) => {
    const text = value == null ? '' : String(value);
    const option = lineMasterOptions(name).find((o) => o.code === text);
    return (
      <LookupDisplayField
        key={name}
        label={label}
        value={text}
        masterRoute={lineMasters[name]?.masterRoute}
        previewEntity={lineMasters[name]?.entity}
        previewOption={option ? { code: option.code, name: option.name } : null}
      />
    );
  };
  const lineLookupShell = (content: React.ReactNode) => <Box sx={{ minWidth: 0 }}>{content}</Box>;
  const deliveryAddressField = () => {
    if (!displayedLine) return null;
    const options = deliveryAddressesQuery.data ?? [];
    const selected = options.find(
      (option) => option.value === String(displayedLine.deliveryPostalAddress ?? '')
    );
    const preview = (content: React.ReactNode) => (
      <RecordPreview
        entity={POSTAL_ADDRESS_RECORD_PREVIEW}
        recordKey={selected?.value}
        record={toAddressPreview(selected)}
      >
        {content}
      </RecordPreview>
    );
    if (!activeHeader)
      return (
        <>
          {preview(field('Delivery address', selected?.label ?? '', true, true))}
          {field('Address', selected?.address ?? '', true)}
        </>
      );
    return (
      <>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          {preview(
            <Box sx={{ flex: 1, minWidth: 0 }}>
              <LookupField
                name="deliveryPostalAddress"
                label={t('salesOrder.ui.deliveryAddress', 'Delivery address')}
                value={lineDetailDraft?.deliveryPostalAddress ?? ''}
                options={options.map((option) => ({
                  id: option.value,
                  code: option.label,
                  name: `${option.label}${option.isPrimary ? ' (Primary)' : ''}`,
                }))}
                displayMode="select"
                searchable
                lazyLoading={false}
                disabled={savingLineDetail || deliveryAddressesQuery.isLoading}
                onChange={(value) => {
                  if (!lineDetailDraft) return;
                  const next = { ...lineDetailDraft, deliveryPostalAddress: String(value ?? '') };
                  setLineDetailDraft(next);
                  void saveLineDetail(next);
                }}
              />
            </Box>
          )}
          <IconButton
            size="small"
            aria-label={t('salesOrder.ui.addDeliveryAddress', 'Add delivery address')}
            onClick={() => openDeliveryAddressDrawer('line')}
          >
            <AddIcon fontSize="small" />
          </IconButton>
        </Stack>
        {field('Address', selected?.address ?? '')}
      </>
    );
  };
  const unitField = () => {
    const label = t('fields.unit');
    if (!displayedLine) return null;
    if (!activeHeader) return lineMasterValue('unit', label, displayedLine.unit);
    return lineLookupShell(
      <LookupField
        name="unit"
        masterRoute={lineMasters.unit?.masterRoute}
        label={label}
        value={lineDetailDraft?.unit ?? ''}
        // Units load page by page from the server as the user searches/scrolls.
        options={
          lineDetailDraft?.unit
            ? [{ id: lineDetailDraft.unit, code: lineDetailDraft.unit, name: lineDetailDraft.unit }]
            : []
        }
        queryKey={salesOrderQueryKeys.lookup(company, 'units')}
        fetchPage={fetchSalesOrderUnitOptions}
        displayMode="select"
        searchable
        lazyLoading
        sideMode="server"
        disabled={savingLineDetail}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, unit: String(value ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const packingUnitField = () => {
    const label = t('salesOrder.packingUnit', 'وحدة التعبئة');
    if (!displayedLine) return null;
    if (!activeHeader) return lineMasterValue('packingUnit', label, displayedLine.packingUnit);
    return lineLookupShell(
      <LookupField
        name="packingUnit"
        masterRoute={lineMasters.packingUnit?.masterRoute}
        label={label}
        value={lineDetailDraft?.packingUnit ?? ''}
        // Units load page by page from the server as the user searches/scrolls.
        options={
          lineDetailDraft?.packingUnit
            ? [
                {
                  id: lineDetailDraft.packingUnit,
                  code: lineDetailDraft.packingUnit,
                  name: lineDetailDraft.packingUnit,
                },
              ]
            : []
        }
        queryKey={salesOrderQueryKeys.lookup(company, 'units')}
        fetchPage={fetchSalesOrderUnitOptions}
        displayMode="select"
        searchable
        lazyLoading
        sideMode="server"
        disabled={savingLineDetail}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, packingUnit: String(value ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const lineValue = (
    name: keyof DetailLine,
    label: string,
    date = false,
    editable = false,
    lookup = false
  ) => {
    const value = displayedLine?.[name];
    const text = value == null || value === '' ? undefined : String(value);
    return field(label, date ? text?.slice(0, 10) : text, editable, lookup);
  };
  const lineLookupField = (
    name: 'deliveryMode' | 'deliveryTerms',
    label: string,
    options: { value: string; label: string }[] = []
  ) => {
    if (!displayedLine) return null;
    if (!activeHeader) return lineMasterValue(name, label, displayedLine[name]);
    return lineLookupShell(
      <LookupField
        name={name}
        previewEntity={lineMasters[name]?.entity}
        label={label}
        value={lineDetailDraft?.[name] ?? ''}
        options={options.map((option) => ({
          id: option.value,
          code: option.value,
          name: option.label,
        }))}
        displayMode="select"
        searchable
        lazyLoading={false}
        disabled={savingLineDetail || lookupsQuery.isLoading}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, [name]: String(value ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const lineEnumField = (
    name:
      | 'lineType'
      | 'deliveryType'
      | 'lineDeliveryType'
      | 'sourcingOrigin'
      | 'deliveryDateControlType'
      | 'mpsFullRunCtpStatus'
      | 'shipCarrierDlvType'
      | 'reservation'
      | 'intercompanyOrigin'
      | 'itemReferenceType',
    label: string,
    options: { value: number; label: string }[]
  ) => {
    if (!displayedLine) return null;
    const value = Number(displayedLine[name] ?? 0);
    if (!activeHeader)
      return field(
        label,
        options.find((option) => option.value === value)?.label ?? String(value),
        true,
        true
      );
    return lineLookupShell(
      <LookupField
        name={name}
        label={label}
        value={value}
        options={options.map((option) => ({
          id: option.value,
          code: String(option.value),
          name: option.label,
        }))}
        displayMode="select"
        searchable={false}
        lazyLoading={false}
        disabled={savingLineDetail}
        onChange={(nextValue) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, [name]: Number(nextValue) };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const taxGroupField = (name: 'taxGroup' | 'taxItemGroup', label: string) => {
    if (!displayedLine) return null;
    if (!activeHeader) return lineMasterValue(name, label, displayedLine[name]);
    const options =
      name === 'taxGroup'
        ? (taxGroupsQuery.data?.salesTaxGroups ?? [])
        : (taxGroupsQuery.data?.itemSalesTaxGroups ?? []);
    return lineLookupShell(
      <LookupField
        name={name}
        masterRoute={lineMasters[name]?.masterRoute}
        label={label}
        value={lineDetailDraft?.[name] ?? ''}
        options={options}
        displayMode="select"
        searchable
        lazyLoading={false}
        disabled={savingLineDetail || taxGroupsQuery.isLoading}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, [name]: String(value ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const lineBooleanField = (
    name:
      | 'autoBatchReservation'
      | 'sameBatchSelection'
      | 'scrap'
      | 'stopped'
      | 'preventPartialDelivery'
      | 'directDelivery'
      | 'excludeFromMasterPlanning'
      | 'excludeFromRebate'
      | 'excludeFromRebateManagement',
    label: string
  ) => {
    if (!displayedLine) return null;
    const checked =
      name === 'directDelivery'
        ? Number(displayedLine.lineDeliveryType ?? 0) === 2
        : name === 'excludeFromMasterPlanning'
          ? Boolean(displayedLine.excludeFromMasterPlanning)
          : name === 'excludeFromRebate' || name === 'excludeFromRebateManagement'
            ? Boolean(displayedLine[name])
            : Boolean(displayedLine[name]);
    if (!activeHeader)
      return field(label, checked ? t('common.yes', 'Yes') : t('common.no', 'No'), true);
    return (
      <Box sx={{ minWidth: 0, minHeight: 48 }}>
        <Typography
          sx={{
            mb: '6px',
            fontFamily: d365.fontFamily,
            fontSize: d365.labelFontSize,
            lineHeight: 1.2,
          }}
        >
          {label}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', height: d365.fieldControlHeight }}>
          <Switch
            size="small"
            checked={checked}
            disabled={savingLineDetail}
            onChange={(_, nextValue) => {
              if (!lineDetailDraft) return;
              const next =
                name === 'directDelivery'
                  ? {
                      ...lineDetailDraft,
                      directDelivery: nextValue,
                      lineDeliveryType: nextValue ? 2 : 0,
                    }
                  : name === 'excludeFromMasterPlanning' ||
                      name === 'excludeFromRebate' ||
                      name === 'excludeFromRebateManagement'
                    ? { ...lineDetailDraft, [name]: nextValue }
                    : { ...lineDetailDraft, [name]: nextValue };
              setLineDetailDraft(next);
              void saveLineDetail(next);
            }}
          />
          <Typography sx={{ fontFamily: d365.fontFamily, fontSize: d365.fontSize }}>
            {checked ? t('common.yes', 'Yes') : t('common.no', 'No')}
          </Typography>
        </Box>
      </Box>
    );
  };
  const returnLotField = () => {
    if (!displayedLine) return null;
    const label = t('salesOrder.returnLotId', 'Return lot ID');
    if (!activeHeader) return field(label, displayedLine.returnLotId, true, true);
    return lineLookupShell(
      <LookupField
        name="returnLotId"
        label={label}
        value={lineDetailDraft?.returnLotId ?? ''}
        options={
          lineDetailDraft?.returnLotId
            ? [
                {
                  id: lineDetailDraft.returnLotId,
                  code: lineDetailDraft.returnLotId,
                  name: lineDetailDraft.returnLotId,
                },
              ]
            : []
        }
        queryKey={salesOrderQueryKeys.lookup(company, 'return-lots', displayedLine.itemNumber)}
        fetchPage={({ pageNumber, pageSize, search, signal }) =>
          salesOrderLinesApi.returnLots({
            itemNumber: displayedLine.itemNumber,
            pageNumber,
            pageSize,
            search,
            signal,
          })
        }
        displayMode="select"
        searchable
        lazyLoading
        sideMode="server"
        disabled={savingLineDetail}
        onChange={(value) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, returnLotId: String(value ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const inventoryTrackingField = (name: 'batchNumber' | 'serialNumber', label: string) => {
    if (!displayedLine) return null;
    const fetchPage =
      name === 'batchNumber'
        ? ({
            pageNumber,
            pageSize,
            search,
            signal,
          }: {
            pageNumber: number;
            pageSize: number;
            search: string;
            signal?: AbortSignal;
          }) =>
            salesOrderLinesApi.batchNumbers({
              itemNumber: displayedLine.itemNumber,
              pageNumber,
              pageSize,
              search,
              signal,
            })
        : ({
            pageNumber,
            pageSize,
            search,
            signal,
          }: {
            pageNumber: number;
            pageSize: number;
            search: string;
            signal?: AbortSignal;
          }) =>
            salesOrderLinesApi.serialNumbers({
              itemNumber: displayedLine.itemNumber,
              pageNumber,
              pageSize,
              search,
              signal,
            });
    const value = displayedLine[name] ?? '';
    if (!activeHeader) return field(label, value, true, true);
    return lineLookupShell(
      <LookupField
        name={name}
        label={label}
        value={value}
        options={value ? [{ id: value, code: value, name: value }] : []}
        queryKey={salesOrderQueryKeys.lookup(
          company,
          'inventory-tracking',
          name,
          displayedLine.itemNumber
        )}
        fetchPage={fetchPage}
        displayMode="select"
        searchable
        lazyLoading
        sideMode="server"
        disabled={savingLineDetail}
        onChange={(nextValue) => {
          if (!lineDetailDraft) return;
          const next = { ...lineDetailDraft, [name]: String(nextValue ?? '') };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  const mainAccountField = () => {
    if (!displayedLine) return null;
    const label = t('salesOrder.mainAccount', 'Main account');
    if (!activeHeader) return field(label, displayedLine.ledgerDimensionDisplay ?? '', true, true);
    return lineLookupShell(
      <LookupField
        name="ledgerDimension"
        label={label}
        value={displayedLine.ledgerDimension ?? 0}
        options={
          displayedLine.ledgerDimension
            ? [
                {
                  id: displayedLine.ledgerDimension,
                  code:
                    displayedLine.ledgerDimensionDisplay ?? String(displayedLine.ledgerDimension),
                  name:
                    displayedLine.ledgerDimensionDisplay ?? String(displayedLine.ledgerDimension),
                },
              ]
            : []
        }
        queryKey={salesOrderQueryKeys.lookup(company, 'ledger-dimensions')}
        fetchPage={salesOrderLinesApi.ledgerDimensions}
        displayMode="select"
        searchable
        lazyLoading
        sideMode="server"
        disabled={savingLineDetail}
        onChange={(value, option) => {
          if (!lineDetailDraft) return;
          const next = {
            ...lineDetailDraft,
            ledgerDimension: Number(value ?? 0),
            ledgerDimensionDisplay: Array.isArray(option)
              ? (option[0]?.name ?? '')
              : (option?.name ?? ''),
          };
          setLineDetailDraft(next);
          void saveLineDetail(next);
        }}
      />
    );
  };
  return {
    displayedLine,
    lineDetailField,
    lineMultilineField,
    dimensionField,
    lineMasterOptions,
    lineMasterValue,
    lineLookupShell,
    deliveryAddressField,
    unitField,
    packingUnitField,
    lineValue,
    lineLookupField,
    lineEnumField,
    taxGroupField,
    lineBooleanField,
    returnLotField,
    inventoryTrackingField,
    mainAccountField,
  };
}
