import { BufferedValueField } from '@shared/components/fields/BufferedValueField';
import React from 'react';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { DataGridLookupField } from '@shared/components/lookups/DataGridLookupField';
import { freeTextInvoiceQueryKeys } from '../../queries/accountsReceivableQueryKeys';
import { FOUNDATION_RECORD_PREVIEWS as FOUNDATION_PREVIEWS } from '../../../foundation/previews/foundationRecordPreviews';
import { freeTextInvoiceApi } from '../api/freeTextInvoiceApi';
import type { Line } from './freeTextInvoiceLine';

/** Invoice-specific columns composed from the generic document grid and lookup editors. */
export function useFreeTextInvoiceLineColumns({
  company,
  editing,
  amount,
  ledgerText,
  setLedgerLabels,
  taxes,
}: {
  company: string;
  editing: boolean;
  amount: (value: number) => string;
  ledgerText: (value: number) => string | undefined;
  setLedgerLabels: React.Dispatch<React.SetStateAction<Record<number, string>>>;
  taxes: { data?: Awaited<ReturnType<typeof freeTextInvoiceApi.taxGroups>>; isLoading: boolean };
}): ColumnDef<Line>[] {
  const { t } = useAppTranslation();
  return React.useMemo(
    (): ColumnDef<Line>[] => [
      { field: 'description', headerName: t('fields.description'), width: 250, editable: true },
      {
        field: 'ledgerDimension',
        headerName: t('freeTextInvoices.ledgerAccount'),
        width: 210,
        editable: true,
        renderCell: ({ row }) =>
          ledgerText(row.ledgerDimension) ||
          row.ledgerDisplay ||
          (row.ledgerDimension ? String(row.ledgerDimension) : ''),
        renderEditCell: ({ row, value, onChange, disabled }) => (
          <>
            <DataGridLookupField
              name="ledgerDimension"
              label={t('freeTextInvoices.ledgerAccount')}
              value={Number(value) || null}
              options={
                value
                  ? [
                      {
                        id: Number(value),
                        code: ledgerText(Number(value)) || row.ledgerDisplay || String(value),
                        name: '',
                      },
                    ]
                  : []
              }
              queryKey={freeTextInvoiceQueryKeys.lookup(company, 'ledger-dimensions')}
              fetchPage={freeTextInvoiceApi.ledgerDimensions}
              searchable
              lazyLoading
              sideMode="server"
              disabled={disabled}
              onChange={(next, option) => {
                const selected = Array.isArray(option) ? option[0] : option;
                if (selected)
                  setLedgerLabels((current) => ({
                    ...current,
                    [Number(next)]: selected.code || selected.name,
                  }));
                onChange(Number(next) || 0);
              }}
            />
          </>
        ),
      },
      ...(['taxGroup', 'taxItemGroup'] as const).map((field): ColumnDef<Line> => ({
        field,
        headerName:
          field === 'taxGroup' ? t('fields.salesTaxGroup') : t('freeTextInvoices.itemTaxGroup'),
        width: 170,
        editable: true,
        preview: {
          entity:
            field === 'taxGroup'
              ? FOUNDATION_PREVIEWS.SALES_TAX_GROUP
              : FOUNDATION_PREVIEWS.ITEM_SALES_TAX_GROUP,
        },
        renderEditCell: ({ value, onChange, disabled }) => (
          <>
            <DataGridLookupField
              name={field}
              label={
                field === 'taxGroup'
                  ? t('fields.salesTaxGroup')
                  : t('freeTextInvoices.itemTaxGroup')
              }
              value={String(value ?? '') || null}
              options={(field === 'taxGroup'
                ? taxes.data?.salesTaxGroups
                : taxes.data?.itemSalesTaxGroups
              )?.map((option) => ({ id: option.id, code: String(option.id), name: option.name }))}
              searchable
              lazyLoading={false}
              disabled={disabled || taxes.isLoading}
              onChange={(next) => onChange(String(next ?? ''))}
            />
          </>
        ),
      })),
      {
        field: 'quantity',
        headerName: t('fields.quantity'),
        width: 110,
        type: 'number',
        editable: true,
        renderEditCell: ({ value, onChange, disabled }) => (
          <BufferedValueField
            fullWidth
            size="small"
            variant="standard"
            type="number"
            value={value ?? ''}
            disabled={disabled}
            slotProps={{
              htmlInput: { 'aria-label': t('fields.quantity'), style: { textAlign: 'right' } },
            }}
            onChange={(event) => onChange(Number(event.target.value))}
          />
        ),
      },
      {
        field: 'unitPrice',
        headerName: t('fields.unitPrice'),
        width: 120,
        type: 'number',
        editable: true,
        renderEditCell: ({ value, onChange, disabled }) => (
          <BufferedValueField
            fullWidth
            size="small"
            variant="standard"
            type="number"
            value={value ?? ''}
            disabled={disabled}
            slotProps={{
              htmlInput: { 'aria-label': t('fields.unitPrice'), style: { textAlign: 'right' } },
            }}
            onChange={(event) => onChange(Number(event.target.value))}
          />
        ),
      },
      {
        field: 'netAmount',
        headerName: t('freeTextInvoices.amount'),
        width: 135,
        type: 'number',
        renderCell: ({ row }) => (editing ? '—' : amount(row.netAmount)),
      },
    ],
    [t, editing, amount, ledgerText, company, taxes.data, taxes.isLoading, setLedgerLabels]
  );
}
