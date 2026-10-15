import React, { useMemo } from 'react';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { inventModelGroupApi, type InventModelGroupRecord } from '../api/inventModelGroupApi';

const bool = (value: DetailValue | undefined): boolean => value === true;
const num = (value: DetailValue | undefined): number => Number(value) || 0;
const str = (value: DetailValue | undefined): string => String(value ?? '');
const option = (value: number, label: string) => ({ value: String(value), label });

const emptyGroup = (): InventModelGroupRecord => ({
  id: `new-${crypto.randomUUID()}`,
  modelGroupId: '',
  name: '',
  inventModel: 0,
  stockedProduct: true,
  postOnHandPhysical: false,
  postOnHandFinancial: false,
  postPhysicalRevenue: false,
  postPhysicalPurchase: false,
  standardCost: false,
  mandatoryRegister: false,
  mandatoryReceive: false,
  mandatoryPick: false,
  mandatoryDeduct: false,
  mandatoryWmsOrder: false,
  negativePhysical: false,
  negativeFinancial: false,
  quarantineControl: false,
  inclPhysicalValueInCost: false,
  includePhysicalValueForAverage: false,
  itemProdReservationActive: false,
  itemProdReservation: 0,
  reserveByDate: false,
  reserveReversed: false,
  mcrReservation: false,
  pdsDispositionCode: '',
  pdsCheck: false,
  pdsConsReq: false,
  pdsReqVendBatchDetail: false,
  pdsSameLot: false,
  pdsVendorCheckItem: false,
  pdsPickCriteria: 0,
  pickingListBatchExpirationDateValidationRule: 0,
});

const valueKeys: Array<keyof InventModelGroupRecord> = [
  'inventModel',
  'stockedProduct',
  'postOnHandPhysical',
  'postOnHandFinancial',
  'postPhysicalRevenue',
  'postPhysicalPurchase',
  'standardCost',
  'mandatoryRegister',
  'mandatoryReceive',
  'mandatoryPick',
  'mandatoryDeduct',
  'mandatoryWmsOrder',
  'negativePhysical',
  'negativeFinancial',
  'quarantineControl',
  'inclPhysicalValueInCost',
  'includePhysicalValueForAverage',
  'itemProdReservationActive',
  'itemProdReservation',
  'reserveByDate',
  'reserveReversed',
  'mcrReservation',
  'pdsDispositionCode',
  'pdsCheck',
  'pdsConsReq',
  'pdsReqVendBatchDetail',
  'pdsSameLot',
  'pdsVendorCheckItem',
  'pdsPickCriteria',
  'pickingListBatchExpirationDateValidationRule',
];

export function InventModelGroupPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const config = useMemo<EnterpriseListDetailsConfig<InventModelGroupRecord>>(() => {
    const modelOptions = [
      option(0, 'FIFO'),
      option(1, 'LIFO'),
      option(2, 'LIFO date'),
      option(3, 'Weighted average'),
      option(4, 'Weighted avg. date'),
      option(5, 'Standard cost'),
      option(6, 'Moving average'),
    ];
    const getValues = (record: InventModelGroupRecord): DetailValues => {
      const values: DetailValues = {};
      for (const key of valueKeys) values[key] = record[key] as DetailValue;
      values.inventModel = String(record.inventModel);
      values.itemProdReservation = String(record.itemProdReservation);
      values.pdsPickCriteria = String(record.pdsPickCriteria);
      values.pickingListBatchExpirationDateValidationRule = String(
        record.pickingListBatchExpirationDateValidationRule
      );
      return values;
    };
    const setValues = (
      record: InventModelGroupRecord,
      values: DetailValues
    ): InventModelGroupRecord => ({
      ...record,
      inventModel: num(values.inventModel),
      stockedProduct: bool(values.stockedProduct),
      postOnHandPhysical: bool(values.postOnHandPhysical),
      postOnHandFinancial: bool(values.postOnHandFinancial),
      postPhysicalRevenue: bool(values.postPhysicalRevenue),
      postPhysicalPurchase: bool(values.postPhysicalPurchase),
      standardCost: bool(values.standardCost),
      mandatoryRegister: bool(values.mandatoryRegister),
      mandatoryReceive: bool(values.mandatoryReceive),
      mandatoryPick: bool(values.mandatoryPick),
      mandatoryDeduct: bool(values.mandatoryDeduct),
      mandatoryWmsOrder: bool(values.mandatoryWmsOrder),
      negativePhysical: bool(values.negativePhysical),
      negativeFinancial: bool(values.negativeFinancial),
      quarantineControl: bool(values.quarantineControl),
      inclPhysicalValueInCost: bool(values.inclPhysicalValueInCost),
      includePhysicalValueForAverage: bool(values.includePhysicalValueForAverage),
      itemProdReservationActive: bool(values.itemProdReservationActive),
      itemProdReservation: num(values.itemProdReservation),
      reserveByDate: bool(values.reserveByDate),
      reserveReversed: bool(values.reserveReversed),
      mcrReservation: bool(values.mcrReservation),
      pdsDispositionCode: str(values.pdsDispositionCode),
      pdsCheck: bool(values.pdsCheck),
      pdsConsReq: bool(values.pdsConsReq),
      pdsReqVendBatchDetail: bool(values.pdsReqVendBatchDetail),
      pdsSameLot: bool(values.pdsSameLot),
      pdsVendorCheckItem: bool(values.pdsVendorCheckItem),
      pdsPickCriteria: num(values.pdsPickCriteria),
      pickingListBatchExpirationDateValidationRule: num(
        values.pickingListBatchExpirationDateValidationRule
      ),
    });
    return {
      recordTableName: 'InventModelGroup',
      dataSource: {
        type: 'remote',
        key: 'inventory-model-groups',
        load: inventModelGroupApi.list,
        create: inventModelGroupApi.create,
        update: inventModelGroupApi.update,
        delete: inventModelGroupApi.delete,
      },
      createRecord: emptyGroup,
      getPrimaryText: (record) => record.modelGroupId,
      getSecondaryText: (record) => record.name,
      matchesSearch: (record, query) =>
        `${record.modelGroupId} ${record.name}`
          .toLocaleLowerCase()
          .includes(query.trim().toLocaleLowerCase()),
      getValues,
      setValues,
      headerFields: [
        {
          id: 'modelGroupId',
          label: t('inventModelGroup.fields.id', 'Item model group'),
          width: 180,
          getValue: (record) => record.modelGroupId,
          setValue: (record, value) => ({ ...record, modelGroupId: str(value) }),
        },
        {
          id: 'name',
          label: t('inventModelGroup.fields.name', 'Name'),
          width: 'minmax(320px, 520px)',
          getValue: (record) => record.name,
          setValue: (record, value) => ({ ...record, name: str(value) }),
        },
      ],
      sections: [
        {
          id: 'costing',
          title: t('inventModelGroup.sections.costing', 'Costing method & cost recognition'),
          defaultExpanded: true,
          columns: 4,
          groups: [
            {
              id: 'inventory-policy',
              title: t('inventModelGroup.groups.inventoryPolicy', 'INVENTORY POLICY'),
              fields: [
                {
                  name: 'stockedProduct',
                  label: t('inventModelGroup.fields.stockedProduct', 'Stocked product'),
                  type: 'boolean',
                },
                {
                  name: 'inventModel',
                  label: t('inventModelGroup.fields.inventModel', 'Inventory model'),
                  type: 'select',
                  options: modelOptions,
                },
              ],
            },
            {
              id: 'cost-price',
              title: t('inventModelGroup.groups.costPrice', 'COST PRICE'),
              fields: [
                {
                  name: 'inclPhysicalValueInCost',
                  label: t(
                    'inventModelGroup.fields.includePhysicalValue',
                    'Include physical value'
                  ),
                  type: 'boolean',
                },
                {
                  name: 'standardCost',
                  label: t('inventModelGroup.fields.fixedReceiptPrice', 'Fixed receipt price'),
                  type: 'boolean',
                },
                {
                  name: 'includePhysicalValueForAverage',
                  label: t(
                    'inventModelGroup.fields.includeAveragePhysical',
                    'Include physical value in weighted average recalculation'
                  ),
                  type: 'boolean',
                },
              ],
            },
            {
              id: 'ledger',
              title: t('inventModelGroup.groups.ledger', 'LEDGER INTEGRATION'),
              fields: [
                {
                  name: 'postOnHandPhysical',
                  label: t(
                    'inventModelGroup.fields.postPhysicalInventory',
                    'Post physical inventory'
                  ),
                  type: 'boolean',
                },
                {
                  name: 'postOnHandFinancial',
                  label: t(
                    'inventModelGroup.fields.postFinancialInventory',
                    'Post financial inventory'
                  ),
                  type: 'boolean',
                },
              ],
            },
            {
              id: 'recognition',
              title: t('inventModelGroup.groups.recognition', 'RECOGNITION OF COST'),
              fields: [
                {
                  name: 'postPhysicalRevenue',
                  label: t(
                    'inventModelGroup.fields.deferredRevenue',
                    'Post to Deferred Revenue Account on Sales Delivery'
                  ),
                  type: 'boolean',
                },
                {
                  name: 'postPhysicalPurchase',
                  label: t(
                    'inventModelGroup.fields.accrueLiability',
                    'Accrue liability on product receipt'
                  ),
                  type: 'boolean',
                },
              ],
            },
          ],
        },
        {
          id: 'policies',
          title: t('inventModelGroup.sections.policies', 'Inventory policies'),
          defaultExpanded: true,
          columns: 5,
          groups: [
            {
              id: 'negative',
              title: t('inventModelGroup.groups.negative', 'NEGATIVE INVENTORY'),
              fields: [
                {
                  name: 'negativePhysical',
                  label: t(
                    'inventModelGroup.fields.negativePhysical',
                    'Physical negative inventory'
                  ),
                  type: 'boolean',
                },
                {
                  name: 'negativeFinancial',
                  label: t(
                    'inventModelGroup.fields.negativeFinancial',
                    'Financial negative inventory'
                  ),
                  type: 'boolean',
                },
                {
                  name: 'quarantineControl',
                  label: t('inventModelGroup.fields.quarantine', 'Quarantine management'),
                  type: 'boolean',
                  sectionTitle: t('inventModelGroup.groups.warehouse', 'WAREHOUSE MANAGEMENT'),
                },
              ],
            },
            {
              id: 'physical',
              title: t('inventModelGroup.groups.physical', 'PHYSICAL UPDATE'),
              fields: [
                {
                  name: 'mandatoryRegister',
                  label: t('inventModelGroup.fields.registration', 'Registration requirements'),
                  type: 'boolean',
                },
                {
                  name: 'mandatoryReceive',
                  label: t('inventModelGroup.fields.receiving', 'Receiving requirements'),
                  type: 'boolean',
                },
                {
                  name: 'mandatoryPick',
                  label: t('inventModelGroup.fields.picking', 'Picking requirements'),
                  type: 'boolean',
                },
                {
                  name: 'mandatoryDeduct',
                  label: t('inventModelGroup.fields.deduction', 'Deduction requirements'),
                  type: 'boolean',
                },
                {
                  name: 'reserveByDate',
                  label: t('inventModelGroup.fields.fifoDate', 'FIFO date-controlled'),
                  type: 'boolean',
                  sectionTitle: t('inventModelGroup.groups.reservation', 'RESERVATION'),
                },
              ],
            },
            {
              id: 'reservation',
              fields: [
                {
                  name: 'reserveReversed',
                  label: t('inventModelGroup.fields.backwardShipDate', 'Backward from ship date'),
                  type: 'boolean',
                },
                {
                  name: 'pdsSameLot',
                  label: t('inventModelGroup.fields.sameBatch', 'Same batch selection'),
                  type: 'boolean',
                },
                {
                  name: 'pdsConsReq',
                  label: t('inventModelGroup.fields.consolidate', 'Consolidate requirement'),
                  type: 'boolean',
                },
                {
                  name: 'pdsCheck',
                  label: t('inventModelGroup.fields.fefo', 'FEFO date-controlled'),
                  type: 'boolean',
                },
              ],
            },
            {
              id: 'batch',
              fields: [
                {
                  name: 'pdsPickCriteria',
                  label: t('inventModelGroup.fields.pickCriteria', 'Pick criteria'),
                  type: 'select',
                  options: [option(0, 'None'), option(1, 'Expiration date')],
                },
                {
                  name: 'pickingListBatchExpirationDateValidationRule',
                  label: t(
                    'inventModelGroup.fields.expirationValidation',
                    'Validate batch expiration on'
                  ),
                  type: 'select',
                  options: [option(0, 'None'), option(1, 'Production scheduled end date')],
                },
                {
                  name: 'pdsDispositionCode',
                  label: t('inventModelGroup.fields.dispositionCode', 'Batch disposition code'),
                },
              ],
            },
            {
              id: 'vendor',
              title: t('inventModelGroup.groups.vendorBatch', 'VENDOR BATCH'),
              fields: [
                {
                  name: 'pdsReqVendBatchDetail',
                  label: t('inventModelGroup.fields.purchaseRegistration', 'Purchase registration'),
                  type: 'boolean',
                },
                {
                  name: 'pdsVendorCheckItem',
                  label: t('inventModelGroup.fields.vendorCheck', 'Approved vendor check'),
                  type: 'boolean',
                  sectionTitle: t('inventModelGroup.groups.vendorList', 'APPROVED VENDOR LIST'),
                },
                {
                  name: 'itemProdReservationActive',
                  label: t(
                    'inventModelGroup.fields.overrideReservation',
                    'Override item production reservation'
                  ),
                  type: 'boolean',
                  sectionTitle: t(
                    'inventModelGroup.groups.productionReservation',
                    'OVERRIDE ITEM PRODUCTION RESERVATION'
                  ),
                },
                {
                  name: 'itemProdReservation',
                  label: t('inventModelGroup.fields.reservation', 'Reservation'),
                  type: 'select',
                  options: [option(0, 'None'), option(1, 'Automatic')],
                },
              ],
            },
          ],
        },
      ],
      permissions: {
        view: PERMISSIONS.INVENTORY_TRANSACTION_VIEW,
        create: PERMISSIONS.INVENTORY_TRANSACTION_CREATE,
        edit: PERMISSIONS.INVENTORY_TRANSACTION_EDIT,
        delete: PERMISSIONS.INVENTORY_TRANSACTION_DELETE,
      },
      validate: (record) => ({
        ...(!record.modelGroupId.trim()
          ? {
              modelGroupId: t('validation.required', {
                field: t('inventModelGroup.fields.id', 'Item model group'),
              }),
            }
          : {}),
        ...(!record.name.trim()
          ? { name: t('validation.required', { field: t('inventModelGroup.fields.name', 'Name') }) }
          : {}),
      }),
      presentation: { mode: 'list', listWidth: 275, listResizable: true },
    };
  }, [t]);

  return (
    <ListDetailsPage
      variant="enterprise"
      title={t('inventModelGroup.title', 'Item model groups')}
      config={config}
    />
  );
}
