import React from 'react';
import { Box, IconButton, Stack, TextField } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import type { DetailFieldConfig, DetailSectionConfig } from '@patterns/list-details/types';
import { POSTAL_ADDRESS_RECORD_PREVIEW } from '@shared/components/logistics/postalAddressRecordPreview';
import { RecordPreview } from '@shared/components/record-preview';
import { type SalesOrderHeaderInput, type SalesOrderListRecord } from '../api/salesOrderListApi';
import { LookupField } from '@shared/components/lookups/LookupField';
import { EditableViewField } from '@shared/components/fields/EditableViewField';
import { SalesOrderLinesGrid } from '../lines/SalesOrderLinesGrid';
import { headerMasterRoutes, headerPreviewEntities } from './salesOrderDetailsConfig';
import type { SalesOrderDocumentView } from './useSalesOrderDocument';
import type { buildSalesOrderTotals } from './salesOrderTotals';
import type { buildSalesOrderHeaderFields } from './SalesOrderHeaderFields';
import type { buildSalesOrderLineFields } from '../lines/SalesOrderLineFields';
import type { buildSalesOrderLineTabs } from '../lines/SalesOrderLineTabs';

type Scope = SalesOrderDocumentView &
  ReturnType<typeof buildSalesOrderTotals> &
  ReturnType<typeof buildSalesOrderHeaderFields> &
  ReturnType<typeof buildSalesOrderLineFields> &
  ReturnType<typeof buildSalesOrderLineTabs>;

export function buildSalesOrderHeaderSections(scope: Scope) {
  const {
    totalsContent,
    activeHeader,
    canEditHeader,
    startHeaderEdit,
    headerInput,
    headerDimensionInput,
    openDeliveryAddressDrawer,
    setHeaderDraft,
    savingHeader,
    lineFilterVisible,
    setLineFilterVisible,
    tab,
    selectedLineId,
    setSelectedLineId,
    t,
    reservationOptions,
    order,
    refetchOrder,
    customersQuery,
    lookupsQuery,
    deliveryAddressesQuery,
    taxGroupsQuery,
    orderAddressPreview,
    orderAddressKey,
    header,
    lineDetails,
  } = scope;
  const section = (
    title: string,
    content: React.ReactNode,
    expanded = true
  ): DetailSectionConfig => ({
    id: `${tab}-${title}`,
    title,
    content,
    defaultExpanded: expanded,
    visualVariant: 'legalEntity',
  });
  const headerLookupFields = new Set([
    'customerAccount',
    'invoiceAccount',
    'customerGroup',
    'currencyCode',
    'inventSiteId',
    'inventLocationId',
    'taxGroupId',
    'salesGroup',
    'deliveryPostalAddress',
    'deliveryMode',
    'deliveryTerms',
    'salesPool',
    'paymentTerms',
    'paymentMethod',
    'paymentSchedule',
    'paymentSpecification',
    'cashDiscountCode',
    'priceGroup',
    'lineDiscountGroup',
    'multiLineDiscountGroup',
    'totalDiscountGroup',
    'chargesGroup',
    'customerRebateGroup',
    'customerTmaGroup',
    'reservation',
  ]);
  const readOnlyHeaderFields = new Set([
    'salesId',
    'customerName',
    'customerAccount',
    'customerGroup',
    'salesStatus',
    'documentStatus',
    'deliveryAddress',
    'reportingCurrencyFixedExchangeRate',
    'releaseStatus',
  ]);
  const headerField = (name: string, label: string, sectionTitle?: string): DetailFieldConfig => ({
    name,
    masterRoute: headerMasterRoutes[name as keyof SalesOrderHeaderInput],
    previewEntity: headerPreviewEntities[name as keyof SalesOrderListRecord],
    label: t(`salesOrder.headerFields.${name}`, label),
    type: readOnlyHeaderFields.has(name)
      ? 'display'
      : headerLookupFields.has(name)
        ? 'select'
        : 'text',
    sectionTitle,
    ...((activeHeader && (name in activeHeader || name === 'deliveryAddress') && name !== 'id') ||
    name === 'deliveryPostalAddress' ||
    name === 'deliveryAddress'
      ? {
          renderOwnLabel: true,
          render: () => {
            if (name === 'inventSiteId' || name === 'inventLocationId')
              return headerDimensionInput(name, t(`salesOrder.headerFields.${name}`, label));
            if (name === 'deliveryAddress') {
              const selectedAddress = deliveryAddressesQuery.data?.find(
                (address) =>
                  address.value ===
                  String(activeHeader?.deliveryPostalAddress ?? order.deliveryPostalAddress)
              );
              return (
                <TextField
                  fullWidth
                  size="small"
                  variant="outlined"
                  label={t(`salesOrder.headerFields.${name}`, label)}
                  value={selectedAddress?.address ?? order.deliveryAddress ?? ''}
                  multiline
                  minRows={3}
                  slotProps={{ input: { readOnly: true }, inputLabel: { shrink: true } }}
                />
              );
            }
            if (name === 'deliveryPostalAddress') {
              const addressOptions = deliveryAddressesQuery.data ?? [];
              const selected = addressOptions.find(
                (address) =>
                  address.value ===
                  String(activeHeader?.deliveryPostalAddress ?? order.deliveryPostalAddress)
              );
              return (
                <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                  {activeHeader ? (
                    <Box sx={{ flex: 1, minWidth: 0 }}>
                      <LookupField
                        name="deliveryPostalAddress"
                        label={t(`salesOrder.headerFields.${name}`, label)}
                        value={activeHeader.deliveryPostalAddress}
                        options={addressOptions.map((address) => ({
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
                      <RecordPreview
                        entity={POSTAL_ADDRESS_RECORD_PREVIEW}
                        recordKey={orderAddressKey(selected)}
                        record={orderAddressPreview(selected)}
                      >
                        <EditableViewField
                          label={t('salesOrder.ui.deliveryAddress', 'Delivery address')}
                          value={selected?.label ?? order.deliveryName ?? ''}
                          lookup
                          onEdit={canEditHeader ? startHeaderEdit : undefined}
                        />
                      </RecordPreview>
                    </Box>
                  )}
                  <IconButton
                    size="small"
                    aria-label={
                      selected ? 'Add or update delivery address' : 'Add delivery address'
                    }
                    disabled={!canEditHeader || savingHeader}
                    onClick={() => openDeliveryAddressDrawer('header')}
                    sx={{ border: 1, borderColor: 'divider', borderRadius: 1, flex: '0 0 auto' }}
                  >
                    <AddIcon fontSize="small" />
                  </IconButton>
                </Stack>
              );
            }
            let options: { value: string; label: string }[] | undefined = undefined;
            if (name === 'invoiceAccount') {
              options = customersQuery.data?.map((c) => ({
                value: c.accountNumber,
                label: `${c.accountNumber} - ${c.name}`,
              }));
            } else if (name === 'currencyCode') {
              options = lookupsQuery.data?.currencies;
            } else if (name === 'paymentTerms') {
              options = lookupsQuery.data?.paymentTerms;
            } else if (name === 'paymentMethod') {
              options = lookupsQuery.data?.paymentMethods;
            } else if (name === 'reservation') {
              options = reservationOptions.map((option) => ({
                value: String(option.value),
                label: option.label,
              }));
            } else if (name === 'intercompanyOrigin') {
              options = [
                { value: '0', label: t('salesOrder.ui.none', 'None') },
                { value: '1', label: t('salesOrder.ui.outbound', 'Outbound') },
                { value: '2', label: t('salesOrder.ui.inbound', 'Inbound') },
              ];
            } else if (name === 'deliveryMode') {
              options = lookupsQuery.data?.deliveryModes;
            } else if (name === 'deliveryTerms') {
              options = lookupsQuery.data?.deliveryTerms;
            } else if (name === 'salesPool') {
              options = lookupsQuery.data?.salesPools;
            } else if (name === 'paymentSchedule') {
              options = lookupsQuery.data?.paymentSchedules;
            } else if (name === 'taxGroupId') {
              options = (taxGroupsQuery.data?.salesTaxGroups ?? []).map((group) => ({
                value: group.code,
                label: `${group.code} - ${group.name}`,
              }));
            } else if (name === 'salesType') {
              options = [
                { value: '3', label: t('salesOrder.ui.salesOrder', 'Sales order') },
                { value: '4', label: t('salesOrder.ui.returnedOrder', 'Returned order') },
              ];
            } else if (name === 'oneTimeCustomer' || name === 'pricesIncludeSalesTax') {
              options = [
                { value: 'false', label: t('common.no', 'No') },
                { value: 'true', label: t('common.yes', 'Yes') },
              ];
            } else if (
              name === 'blindShipment' ||
              name === 'residentialDestination' ||
              name === 'excludeFromMasterPlanning' ||
              name === 'intercompanyAutoCreateOrders' ||
              name === 'intercompanyDirectDelivery' ||
              name === 'intercompanyAllowIndirectCreation'
            ) {
              options = [
                { value: 'false', label: t('common.no', 'No') },
                { value: 'true', label: t('common.yes', 'Yes') },
              ];
            } else if (name === 'deliveryDateControlType') {
              options = [
                { value: '0', label: t('salesOrder.ui.none', 'None') },
                { value: '1', label: t('salesOrder.ui.salesLeadTime', 'Sales lead time') },
                { value: '2', label: t('salesOrder.ui.atp', 'ATP') },
                { value: '3', label: t('salesOrder.ui.ctp', 'CTP') },
              ];
            } else if (name === 'mpsFullRunCtpStatus') {
              options = [
                { value: '0', label: t('salesOrder.ui.none', 'None') },
                { value: '1', label: t('salesOrder.ui.complete', 'Complete') },
                { value: '2', label: t('salesOrder.ui.failed', 'Failed') },
              ];
            } else if (name === 'carrierService') {
              options = [
                { value: '0', label: t('salesOrder.ui.none', 'None') },
                { value: '1', label: t('salesOrder.ui.ground', 'Ground') },
                { value: '2', label: t('salesOrder.ui.air', 'Air') },
                { value: '3', label: t('salesOrder.ui.ocean', 'Ocean') },
                { value: '4', label: t('salesOrder.ui.other', 'Other') },
              ];
            }
            return headerInput(
              name as keyof SalesOrderHeaderInput,
              t(`salesOrder.headerFields.${name}`, label),
              options
            );
          },
        }
      : {}),
  });
  const headerSections: DetailSectionConfig[] = [
    {
      summaryItems: [
        { id: 'salesId', label: t('fields.salesOrderNumber'), value: order.salesId, accent: true },
        { id: 'customerName', label: t('fields.customerName'), value: order.customerName },
        {
          id: 'customerAccount',
          label: t('fields.customerAccount'),
          value: order.customerAccount,
          accent: true,
        },
        {
          id: 'invoiceAccount',
          label: t('fields.invoiceAccount'),
          value: order.invoiceAccount,
          accent: true,
        },
        {
          id: 'site',
          label: t('salesOrderQuickCreate.site', 'Site'),
          value: order.inventSiteId,
          accent: true,
        },
        {
          id: 'warehouse',
          label: t('salesOrderQuickCreate.warehouse', 'Warehouse'),
          value: order.inventLocationId,
          accent: true,
        },
      ],
      id: 'header-general',
      title: t('salesOrder.general', 'General'),
      visualVariant: 'legalEntity',
      columns: 5,
      columnGap: 40,
      minHeight: 480,
      groups: [
        {
          id: 'order',
          title: t('salesOrder.salesOrder', 'Sales order'),
          fields: [
            headerField('salesId', 'Sales order'),
            headerField('customerName', 'Customer name'),
            headerField('salesNameAlias', 'Arabic name'),
          ],
        },
        {
          id: 'customer',
          fields: [
            headerField('salesType', 'Order type'),
            headerField('customerAccount', 'Customer account', t('fields.customer')),
            headerField('oneTimeCustomer', 'One-time customer'),
            headerField('invoiceAccount', 'Invoice account'),
          ],
        },
        {
          id: 'contact',
          title: t('salesOrder.contactInformation', 'Contact information'),
          fields: [
            headerField('email', 'Email'),
            headerField('phone', 'Telephone'),
            headerField('salesStatus', 'Status', t('common.status')),
            headerField('deadline', 'Deadline'),
          ],
        },
        {
          id: 'storage',
          fields: [
            headerField('documentStatus', 'Document status'),
            headerField(
              'inventSiteId',
              'Site',
              t('salesOrder.storageDimensions', 'Storage dimensions')
            ),
            headerField('inventLocationId', 'Warehouse'),
            headerField('campaignId', 'Campaign ID'),
          ],
        },
        {
          id: 'references',
          title: t('salesOrder.references', 'References'),
          fields: [
            headerField('customerRequisitionNumber', 'Customer requisition'),
            headerField('customerReference', 'Customer reference'),
          ],
        },
      ],
    },
    {
      summaryItems: [
        {
          id: 'customerGroup',
          label: t('fields.customerGroup'),
          value: order.customerGroup,
          accent: true,
        },
        { id: 'currency', label: t('fields.currency'), value: order.currencyCode, accent: true },
        {
          id: 'paymentTerms',
          label: t('fields.paymentTerms'),
          value: order.paymentTerms,
          accent: true,
        },
      ],
      id: 'header-setup',
      title: t('salesOrder.setup', 'Setup'),
      visualVariant: 'legalEntity',
      columns: 5,
      columnGap: 40,
      groups: [
        {
          id: 'tax',
          title: t('salesOrder.salesTax', 'Sales tax'),
          fields: [
            headerField('taxGroupId', 'Sales tax group'),
            headerField('pricesIncludeSalesTax', 'Prices include sales tax'),
          ],
        },
        {
          id: 'posting',
          title: t('salesOrder.posting', 'Posting'),
          fields: [
            headerField('customerGroup', 'Customer group'),
            headerField('currencyCode', 'Currency'),
          ],
        },
        {
          id: 'commission',
          title: t('salesOrder.commission', 'Commission'),
          fields: [headerField('salesGroup', 'Sales group')],
        },
        {
          id: 'reservation',
          fields: [
            headerField('deliveryMode', 'Delivery mode'),
            headerField('deliveryDate', 'Requested delivery'),
            headerField('salesPool', 'Pool'),
            headerField('reservation', 'Reservation'),
          ],
        },
        {
          id: 'language',
          fields: [
            headerField('languageId', 'Language'),
            headerField('paymentTerms', 'Payment terms'),
          ],
        },
      ],
    },
    {
      id: 'header-address',
      title: 'Address',
      visualVariant: 'legalEntity',
      columns: 5,
      groups: [
        {
          id: 'delivery-address',
          title: 'Delivery address',
          fields: [
            headerField('deliveryName', 'Name'),
            headerField('deliveryPostalAddress', 'Delivery address'),
            headerField('deliveryAddress', 'Address'),
          ],
        },
      ],
    },
    {
      id: 'header-delivery',
      title: 'Delivery',
      visualVariant: 'legalEntity',
      columns: 6,
      columnGap: 40,
      groups: [
        {
          id: 'delivery-dates',
          title: 'DELIVERY DATE',
          fields: [
            headerField('shippingDateRequested', 'Requested ship date'),
            headerField('deliveryDate', 'Requested receipt date'),
          ],
        },
        {
          id: 'confirmed-dates',
          title: 'CONFIRMED DATES',
          fields: [
            headerField('shippingDateConfirmed', 'Confirmed ship date'),
            headerField('receiptDateConfirmed', 'Confirmed receipt date'),
            headerField('deliveryDateControlType', 'Delivery date control'),
          ],
        },
        {
          id: 'planning',
          title: 'PLANNING',
          fields: [headerField('mpsFullRunCtpStatus', 'Batch CTP status')],
        },
        {
          id: 'delivery-details',
          title: 'DETAILS',
          fields: [
            headerField('blindShipment', 'Blind shipment'),
            headerField('deliveryReason', 'Delivery reason'),
            headerField('exportReason', 'Reason for export'),
          ],
        },
        {
          id: 'delivery-mode',
          title: 'MISC. DELIVERY INFO',
          fields: [
            headerField('deliveryMode', 'Mode of delivery'),
            headerField('deliveryTerms', 'Delivery terms'),
          ],
        },
        {
          id: 'carrier',
          title: 'CARRIER INFORMATION',
          fields: [
            headerField('residentialDestination', 'Residential destination'),
            headerField('shippingCarrier', 'Shipping carrier'),
            headerField('carrierId', 'Carrier ID'),
            headerField('carrierService', 'Carrier service'),
            headerField('brokerId', 'Broker ID'),
            headerField('transportMode', 'Mode'),
          ],
        },
      ],
    },
    {
      id: 'header-price-discount',
      title: 'Price and discount',
      visualVariant: 'legalEntity',
      columns: 6,
      columnGap: 40,
      groups: [
        {
          id: 'header-currency',
          title: 'CURRENCY',
          fields: [
            headerField('currencyCode', 'Currency'),
            headerField('fixedExchangeRate', 'Fixed exchange rate'),
            headerField(
              'reportingCurrencyFixedExchangeRate',
              'Reporting currency fixed exchange rate'
            ),
          ],
        },
        {
          id: 'header-payment',
          title: 'PAYMENT',
          fields: [
            headerField('paymentTerms', 'Payment'),
            headerField('fixedDueDate', 'Due date'),
            headerField('paymentMethod', 'Method of payment'),
            headerField('paymentSpecification', 'Payment specification'),
          ],
        },
        {
          id: 'header-payment-details',
          fields: [
            headerField('paymentSchedule', 'Payment schedule'),
            headerField('paymentTermsBaseDate', 'Payment terms base date'),
            headerField('cashDiscountCode', 'Cash discount'),
            headerField('discountPercent', 'Discount percentage'),
          ],
        },
        {
          id: 'header-discount-charges',
          title: 'DISCOUNT OR CHARGES',
          fields: [
            headerField('priceGroup', 'Price group'),
            headerField('lineDiscountGroup', 'Line discount group'),
          ],
        },
        {
          id: 'header-discount-groups',
          fields: [
            headerField('multiLineDiscountGroup', 'Multiline discount group'),
            headerField('totalDiscountGroup', 'Total discount group'),
            headerField('chargesGroup', 'Charges group'),
            headerField('customerRebateGroup', 'Customer rebate group'),
          ],
        },
        {
          id: 'header-rebates',
          title: 'REBATES',
          fields: [
            headerField('customerTmaGroup', 'Customer TMA group'),
            headerField('rebateReference', 'Rebate reference'),
            headerField('totalDiscountPercent', 'Total discount %'),
          ],
        },
      ],
    },
    {
      id: 'header-intercompany',
      title: 'Intercompany settings',
      visualVariant: 'legalEntity',
      columns: 4,
      groups: [
        {
          id: 'intercompany',
          fields: [
            headerField('intercompanyAutoCreateOrders', 'Autocreate intercompany orders'),
            headerField('intercompanyDirectDelivery', 'Direct delivery'),
            headerField('intercompanyOrigin', 'Origin (intercompany orders)'),
            headerField('intercompanyAllowIndirectCreation', 'Allow indirect creation'),
          ],
        },
      ],
    },
    {
      id: 'header-warehouse',
      title: 'Warehouse',
      visualVariant: 'legalEntity',
      columns: 4,
      groups: [
        {
          id: 'warehouse-status',
          title: 'STATUS',
          fields: [headerField('releaseStatus', 'Release status')],
        },
      ],
    },
    {
      id: 'header-transportation',
      title: 'Transportation',
      visualVariant: 'legalEntity',
      columns: 4,
      groups: [
        {
          id: 'transport-routes',
          title: 'ROUTES',
          fields: [
            headerField('carrierCustomerAccount', 'Carrier customer account number'),
            headerField('freightZone', 'Freight zone'),
          ],
        },
      ],
    },
  ];
  const sections = [
    ...(tab === 'header'
      ? // The page owns header edit state (Edit/Save/Cancel in its action pane).
        headerSections.map((headerSection) => ({
          ...headerSection,
          editing: Boolean(activeHeader),
        }))
      : [section(t('salesOrder.header', 'Sales order header'), header, true)]),
    tab === 'lines' &&
      section(
        t('salesOrder.lines', 'Sales order lines'),
        <SalesOrderLinesGrid
          key={order.id}
          order={order}
          editing={Boolean(activeHeader)}
          selectedLineId={selectedLineId}
          setSelectedLineId={setSelectedLineId}
          lineFilterVisible={lineFilterVisible}
          setLineFilterVisible={setLineFilterVisible}
          refreshOrder={() => refetchOrder()}
          onRequestEdit={canEditHeader ? startHeaderEdit : undefined}
        />
      ),
    tab === 'lines' && section(t('salesOrder.lineDetails', 'Line details'), lineDetails),
    section(t('fields.totals', 'Totals'), totalsContent),
  ].filter((value): value is DetailSectionConfig => Boolean(value));
  return {
    section,
    headerLookupFields,
    readOnlyHeaderFields,
    headerField,
    headerSections,
    sections,
  };
}
