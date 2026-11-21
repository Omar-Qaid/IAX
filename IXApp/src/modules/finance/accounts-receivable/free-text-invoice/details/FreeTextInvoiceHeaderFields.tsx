import React from 'react';
import { getRecordPreview } from '@shared/components/record-preview';
import { TextField } from '@mui/material';
import { FieldPresentationProvider } from '@shared/components/fields/FieldPresentationContext';
import { LookupField } from '@shared/components/lookups/LookupField';
import type { LookupOption } from '@shared/components/lookups/types';
import type { DetailFieldConfig, DetailValues } from '@patterns/list-details/types';
import { ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS as AR_PREVIEWS } from '../../previews/accountsReceivableRecordPreviews';
import { FOUNDATION_RECORD_PREVIEWS as FOUNDATION_PREVIEWS } from '../../../foundation/previews/foundationRecordPreviews';
import type { Invoice } from './freeTextInvoiceModel';
import type { FreeTextInvoiceDocument } from './useFreeTextInvoiceDocument';

export function buildFreeTextInvoiceHeaderFields(scope: FreeTextInvoiceDocument) {
  const { t, display, customerName, editing, busy, update, customers, customerOptions, lookups } =
    scope;
  const readOnlyFields = new Set(['customerName', 'invoiceId', 'dueDate', 'posted']);
  const values: DetailValues = {
    customerAccount: display.customerAccount,
    customerName,
    invoiceAccount: display.invoiceAccount,
    description: display.description,
    invoiceId: display.invoiceId || t('freeTextInvoices.notPosted'),
    invoiceDate: display.invoiceDate.slice(0, 10),
    dueDate: (display.dueDate ?? '').slice(0, 10),
    currencyCode: display.currencyCode,
    posted: display.posted,
    taxGroup: display.taxGroup,
    paymentTerms: display.paymentTerms,
    paymentMethod: display.paymentMethod,
    postingProfile: display.postingProfile,
  };
  const textField = (
    name: keyof Invoice,
    label: string,
    type: 'text' | 'date' = 'text'
  ): DetailFieldConfig =>
    editing && !readOnlyFields.has(name)
      ? {
          name,
          label,
          render: () => (
            <TextField
              size="small"
              fullWidth
              type={type}
              value={String(display[name] ?? '').slice(0, type === 'date' ? 10 : undefined)}
              disabled={busy}
              slotProps={{ htmlInput: { 'aria-label': label } }}
              onChange={(event) => update({ [name]: event.target.value })}
            />
          ),
        }
      : { name, label, type: readOnlyFields.has(name) ? 'display' : 'text' };
  const displayField = (
    name: string,
    label: string,
    type: 'display' | 'boolean' = 'display'
  ): DetailFieldConfig =>
    type === 'boolean' && !editing
      ? { name, label, type: 'boolean' }
      : { name, label, type: 'display' };
  const lookupField = (
    name: keyof Invoice,
    label: string,
    options: LookupOption[],
    previewEntity: string,
    onChange?: (value: string) => void
  ): DetailFieldConfig =>
    editing
      ? {
          name,
          label,
          renderOwnLabel: true,
          render: () => (
            <FieldPresentationProvider viewMode={false}>
              <LookupField
                name={name}
                masterRoute={getRecordPreview(previewEntity)?.masterRoute}
                previewEntity={previewEntity}
                label={label}
                value={String(display[name] ?? '')}
                options={options}
                displayMode="select"
                searchable
                disabled={busy}
                onChange={(value) =>
                  onChange ? onChange(String(value ?? '')) : update({ [name]: String(value ?? '') })
                }
              />
            </FieldPresentationProvider>
          ),
        }
      : { name, label, previewEntity, masterRoute: getRecordPreview(previewEntity)?.masterRoute };
  const optionRows = (rows?: { value: string; label: string }[]) =>
    (rows ?? []).map((row) => ({ id: row.value, code: row.value, name: row.label }));
  const chooseCustomer = (account: string) => {
    const row = customers.data?.find((item) => item.accountNumber === account);
    update({
      customerAccount: account,
      invoiceAccount: row?.invoiceAccount || account,
      currencyCode: row?.currencyCode ?? '',
      paymentTerms: row?.paymTermId ?? '',
      paymentMethod: row?.paymModeId ?? '',
      taxGroup: row?.taxGroupId ?? '',
    });
  };

  // Header fields, shared by the compact header (Lines tab) and the Header tab FastTabs.
  const fields = {
    customerAccount: lookupField(
      'customerAccount',
      t('fields.customerAccount'),
      customerOptions,
      AR_PREVIEWS.CUSTOMER,
      chooseCustomer
    ),
    customerName: displayField('customerName', t('fields.name')),
    invoiceAccount: lookupField(
      'invoiceAccount',
      t('fields.invoiceAccount'),
      customerOptions,
      AR_PREVIEWS.CUSTOMER
    ),
    description: textField('description', t('fields.description')),
    invoiceId: displayField('invoiceId', t('freeTextInvoices.invoice')),
    invoiceDate: textField('invoiceDate', t('freeTextInvoices.invoiceDate'), 'date'),
    dueDate: displayField('dueDate', t('freeTextInvoices.dueDate')),
    posted: displayField('posted', t('freeTextInvoices.posted'), 'boolean'),
    currencyCode: lookupField(
      'currencyCode',
      t('fields.currency'),
      optionRows(lookups.data?.currencies),
      FOUNDATION_PREVIEWS.CURRENCY
    ),
    taxGroup: lookupField(
      'taxGroup',
      t('fields.salesTaxGroup'),
      optionRows(lookups.data?.salesTaxGroups),
      FOUNDATION_PREVIEWS.SALES_TAX_GROUP
    ),
    paymentTerms: lookupField(
      'paymentTerms',
      t('freeTextInvoices.paymentTerms'),
      optionRows(lookups.data?.paymentTerms),
      AR_PREVIEWS.PAYMENT_TERMS
    ),
    paymentMethod: lookupField(
      'paymentMethod',
      t('freeTextInvoices.paymentMethod'),
      optionRows(lookups.data?.paymentMethods),
      AR_PREVIEWS.PAYMENT_METHOD
    ),
    postingProfile: {
      ...textField('postingProfile', t('freeTextInvoices.postingProfile')),
      masterRoute: getRecordPreview(AR_PREVIEWS.POSTING_PROFILE)?.masterRoute,
      previewEntity: AR_PREVIEWS.POSTING_PROFILE,
    },
  };
  return { readOnlyFields, values, fields };
}
