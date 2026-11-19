import React from 'react';
import { Box, IconButton, Stack, TextField, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { type SalesOrderDeliveryAddressOption } from '../api/salesOrderLinesApi';
import {
  POSTAL_ADDRESS_RECORD_PREVIEW,
  type PostalAddressPreview,
} from '@shared/components/logistics/postalAddressRecordPreview';
import { RecordPreview } from '@shared/components/record-preview';
import { type SalesOrderHeaderInput, type SalesOrderListRecord } from '../api/salesOrderListApi';
import { LookupField } from '@shared/components/lookups/LookupField';
import { LookupDisplayField } from '@shared/components/lookups/LookupDisplayField';
import { DocumentFieldLabel } from '@shared/components/fields/DocumentFieldLabel';
import { EditableViewField } from '@shared/components/fields/EditableViewField';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import {
  headerMasterRoutes,
  headerPreviewEntities,
  toAddressPreview,
} from './salesOrderDetailsConfig';
import type { SalesOrderDocumentView } from './useSalesOrderDocument';
import type { buildSalesOrderTotals } from './salesOrderTotals';

type Scope = SalesOrderDocumentView & ReturnType<typeof buildSalesOrderTotals>;

export function buildSalesOrderHeaderPanel(scope: Scope) {
  const {
    activeHeader,
    canEditHeader,
    startHeaderEdit,
    headerInput,
    headerDimensionInput,
    openDeliveryAddressDrawer,
    setHeaderDraft,
    savingHeader,
    t,
    order,
    deliveryAddressesQuery,
    dimensionsQuery,
    taxGroupsQuery,
    amount,
    number,
    totalDiscount,
  } = scope;
  const field = (label: string, value?: React.ReactNode, editable = false, lookup = false) => (
    <Box key={label}>
      <DocumentFieldLabel>{label}</DocumentFieldLabel>
      <EditableViewField
        label={label}
        value={value == null ? '' : String(value)}
        lookup={lookup}
        onEdit={editable && canEditHeader && !activeHeader ? startHeaderEdit : undefined}
      />
    </Box>
  );
  const headerPanelField = (
    name: keyof SalesOrderHeaderInput,
    label: string,
    displayValue: React.ReactNode,
    options?: { value: string; label: string }[]
  ) =>
    // Edit mode: the lookup editor (plain label, its own preview).
    activeHeader ? (
      <React.Fragment key={name}>
        {headerPanelFieldContent(name, label, displayValue, options)}
      </React.Fragment>
    ) : headerMasterRoutes[name] || headerPreviewEntities[name as keyof SalesOrderListRecord] ? (
      // View mode master value: open-details icon + record card from the loaded option.
      <LookupDisplayField
        key={name}
        label={label}
        value={displayValue == null ? '' : String(displayValue)}
        masterRoute={headerMasterRoutes[name]}
        previewEntity={headerPreviewEntities[name as keyof SalesOrderListRecord]}
        previewKey={String(order[name as keyof SalesOrderListRecord] ?? '')}
        previewOption={headerMasterOption(name, options)}
        onEdit={canEditHeader ? startHeaderEdit : undefined}
      />
    ) : (
      <React.Fragment key={name}>
        {headerPanelFieldContent(name, label, displayValue, options)}
      </React.Fragment>
    );
  // Code + name of a header master value, from lists the page already loads.
  const headerMasterOption = (
    name: keyof SalesOrderHeaderInput,
    options?: { value: string; label: string }[]
  ) => {
    const code = String(order[name as keyof SalesOrderListRecord] ?? '');
    if (!code) return null;
    const list: { code: string; name: string }[] =
      name === 'inventSiteId'
        ? (dimensionsQuery.data?.sites ?? [])
        : name === 'inventLocationId'
          ? (dimensionsQuery.data?.warehouses ?? [])
          : name === 'taxGroupId'
            ? (taxGroupsQuery.data?.salesTaxGroups ?? [])
            : (options ?? []).map((option) => ({ code: option.value, name: option.label }));
    const match = list.find((option) => option.code === code);
    return match ? { code: match.code, name: match.name } : null;
  };
  const headerPanelFieldContent = (
    name: keyof SalesOrderHeaderInput,
    label: string,
    displayValue: React.ReactNode,
    options?: { value: string; label: string }[]
  ) =>
    activeHeader ? (
      name === 'inventSiteId' || name === 'inventLocationId' ? (
        headerDimensionInput(name, label)
      ) : name === 'notes' ? (
        <TextField
          fullWidth
          size="small"
          label={label}
          multiline
          minRows={3}
          value={activeHeader.notes}
          disabled={savingHeader}
          onChange={(event) =>
            setHeaderDraft((draft) => (draft ? { ...draft, notes: event.target.value } : draft))
          }
        />
      ) : (
        headerInput(name, label, options)
      )
    ) : (
      field(
        label,
        displayValue,
        true,
        Boolean(options) || name === 'inventSiteId' || name === 'inventLocationId'
      )
    );
  const selectedDeliveryAddress = deliveryAddressesQuery.data?.find(
    (address) =>
      address.value === String(activeHeader?.deliveryPostalAddress ?? order.deliveryPostalAddress)
  );
  // Card record for the order's delivery address: the loaded option, else the
  // name and address the order itself carries.
  const orderAddressPreview = (
    option: SalesOrderDeliveryAddressOption | undefined
  ): PostalAddressPreview | undefined =>
    toAddressPreview(option) ??
    (order.deliveryName || order.deliveryAddress
      ? { location: order.deliveryName ?? '', address: order.deliveryAddress ?? '' }
      : undefined);
  const orderAddressKey = (option: SalesOrderDeliveryAddressOption | undefined) =>
    option?.value || String(order.deliveryPostalAddress || '') || order.deliveryName || '';
  const deliveryAddressPanel = (
    <Box sx={{ minWidth: 0 }}>
      <Typography variant="caption" color="text.secondary">
        {t('salesOrder.ui.deliveryAddress', 'Delivery address')}
      </Typography>
      <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
        <RecordPreview
          entity={POSTAL_ADDRESS_RECORD_PREVIEW}
          recordKey={orderAddressKey(selectedDeliveryAddress)}
          record={orderAddressPreview(selectedDeliveryAddress)}
        >
          {activeHeader ? (
            <Box sx={{ flex: 1, minWidth: 0 }}>
              <LookupField
                name="deliveryPostalAddress"
                label={t('salesOrder.ui.deliveryAddress', 'Delivery address')}
                value={activeHeader.deliveryPostalAddress}
                options={(deliveryAddressesQuery.data ?? []).map((address) => ({
                  id: address.value,
                  code: address.label,
                  name: `${address.label}${address.isPrimary ? ' (Primary)' : ''}`,
                }))}
                displayMode="select"
                searchable
                lazyLoading={false}
                disabled={savingHeader || deliveryAddressesQuery.isLoading}
                onChange={(value) =>
                  setHeaderDraft((draft) =>
                    draft ? { ...draft, deliveryPostalAddress: String(value ?? '') } : draft
                  )
                }
              />
            </Box>
          ) : (
            <Box sx={{ flex: 1, minWidth: 0 }}>
              <EditableViewField
                label={t('salesOrder.ui.deliveryAddress', 'Delivery address')}
                value={selectedDeliveryAddress?.label ?? order.deliveryName ?? ''}
                lookup
                onEdit={canEditHeader ? startHeaderEdit : undefined}
              />
            </Box>
          )}
        </RecordPreview>
        <IconButton
          size="small"
          aria-label={
            selectedDeliveryAddress ? 'Add or update delivery address' : 'Add delivery address'
          }
          disabled={!canEditHeader || savingHeader}
          onClick={() => openDeliveryAddressDrawer('header')}
          sx={{ border: 1, borderColor: 'divider', borderRadius: 1, flex: '0 0 auto' }}
        >
          <AddIcon fontSize="small" />
        </IconButton>
      </Stack>
      <TextField
        fullWidth
        size="small"
        variant="outlined"
        label={t('salesOrder.ui.address', 'Address')}
        value={selectedDeliveryAddress?.address ?? order.deliveryAddress ?? ''}
        multiline
        minRows={3}
        slotProps={{ input: { readOnly: true }, inputLabel: { shrink: true } }}
      />
    </Box>
  );
  const headerGroup = (title: string, children: React.ReactNode) => (
    <Box sx={{ minWidth: 0, display: 'grid', alignContent: 'start', gap: 1.25 }}>
      <Typography
        sx={{
          fontSize: d365.labelFontSize,
          fontWeight: 700,
          lineHeight: 1.5,
          textTransform: 'uppercase',
        }}
      >
        {title}
      </Typography>
      {children}
    </Box>
  );
  const header = (
    <Box
      sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: 3 }}
    >
      {headerGroup(
        t('salesOrder.headerGroups.deliveryAddress', 'Delivery address'),
        <>
          {headerPanelField('deliveryName', 'Name', order.deliveryName)}
          {deliveryAddressPanel}
        </>
      )}
      {headerGroup(
        t('salesOrder.headerGroups.deliveryDate', 'Delivery date'),
        <>
          {headerPanelField(
            'shippingDateRequested',
            'Requested ship date',
            order.shippingDateRequested?.slice(0, 10) ?? ''
          )}
          {headerPanelField(
            'deliveryDate',
            'Requested receipt date',
            order.deliveryDate?.slice(0, 10) ?? ''
          )}
          {headerPanelField(
            'shippingDateConfirmed',
            'Confirmed ship date',
            order.shippingDateConfirmed?.slice(0, 10) ?? ''
          )}
          {headerPanelField(
            'receiptDateConfirmed',
            'Confirmed receipt date',
            order.receiptDateConfirmed?.slice(0, 10) ?? ''
          )}
        </>
      )}
      {headerGroup(
        t('salesOrder.headerGroups.references', 'References'),
        <>
          {headerPanelField('customerReference', 'Customer reference', order.customerReference)}
          {headerPanelField(
            'customerRequisitionNumber',
            'Customer requisition',
            order.customerRequisitionNumber
          )}
        </>
      )}
      {headerGroup(
        t('salesOrder.headerGroups.discounts', 'Discounts'),
        <>
          {field('Total discount', amount(totalDiscount))}
          {headerPanelField(
            'totalDiscountPercent',
            'Total discount %',
            number(order.totalDiscountPercent),
            undefined
          )}
        </>
      )}
      {headerGroup(
        t('salesOrder.headerGroups.warehouse', 'Warehouse'),
        <>
          {field('Release status', order.releaseStatus)}
          {headerPanelField('inventSiteId', 'Site', order.inventSiteId)}
          {headerPanelField('inventLocationId', 'Warehouse', order.inventLocationId)}
        </>
      )}
      {headerGroup(
        t('salesOrder.headerGroups.transportation', 'Transportation'),
        <>
          {headerPanelField('freightZone', 'Routes', order.freightZone)}
          {headerPanelField(
            'carrierCustomerAccount',
            'Carrier customer account number',
            order.carrierCustomerAccount
          )}
        </>
      )}
      {headerGroup(
        t('salesOrder.headerGroups.notes', 'Notes'),
        headerPanelField('notes', 'Notes', order.notes)
      )}
    </Box>
  );
  return {
    field,
    headerPanelField,
    headerMasterOption,
    headerPanelFieldContent,
    selectedDeliveryAddress,
    orderAddressPreview,
    orderAddressKey,
    deliveryAddressPanel,
    headerGroup,
    header,
  };
}
