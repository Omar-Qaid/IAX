import {
  useFreeTextInvoiceDocument,
  type FreeTextInvoiceDetailsProps,
} from './useFreeTextInvoiceDocument';
import { useFreeTextInvoiceLineColumns } from '../lines/useFreeTextInvoiceLineColumns';
import React from 'react';
import { Alert, Box, TextField, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import { PERMISSIONS } from '@core/permissions/permissions';
import {
  MasterDetailLinesGrid,
  MasterDetailField,
  MasterDetailLineDetails,
  MasterDetailLines,
  MasterDetailPage,
  MasterDetailTotals,
} from '@patterns/master-detail';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { recordTableId } from '@shared/components/documents';
import type {
  DetailFieldConfig,
  DetailSectionConfig,
  DetailValues,
} from '@patterns/list-details/types';
import { DocumentTotalsDrawer } from '@patterns/document/DocumentTotalsDrawer';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import { FieldPresentationProvider } from '@shared/components/fields/FieldPresentationContext';
import { LookupField } from '@shared/components/lookups/LookupField';
import type { LookupOption } from '@shared/components/lookups/types';
import { localizedName } from '@shared/utilities/localizedName';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS as paths } from '../../routes/accountsReceivableRoutePaths';
import { ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS as AR_PREVIEWS } from '../../previews/accountsReceivableRecordPreviews';
import { FOUNDATION_RECORD_PREVIEWS as FOUNDATION_PREVIEWS } from '../../../foundation/previews/foundationRecordPreviews';

import { asInvoice } from './freeTextInvoiceModel';
import { isValidInvoiceLine, newLine, type Line } from '../lines/freeTextInvoiceLine';

export function FreeTextInvoiceDetails({
  company,
  invoice,
  isNew,
  startEditing,
}: FreeTextInvoiceDetailsProps) {
  const {
    t,
    isRtl,
    hasPermission,
    navigate,
    allowedEdit,
    draftRef,
    busy,
    error,
    setError,
    confirmation,
    setConfirmation,
    totalsOpen,
    setTotalsOpen,
    tab,
    setTab,
    lineTab,
    setLineTab,
    selectedLineId,
    setSelectedLineId,
    filterVisible,
    setFilterVisible,
    setLedgerLabels,
    grid,
    editing,
    display,
    selectedLine,
    changeDraft,
    update,
    customers,
    lookups,
    taxes,
    ledgerText,
    invoices,
    customerName,
    customerOptions,
    amount,
    beginEdit,
    switchTab,
    invalidate,
    save,
    runConfirmed,
  } = useFreeTextInvoiceDocument({ company, invoice, isNew, startEditing });
  // View mode: plain field configs, rendered by the shared engine from `values`
  // (hover cards for master data, pencil to start editing). Edit mode: editors.
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
      : { name, label, previewEntity };
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
    postingProfile: textField('postingProfile', t('freeTextInvoices.postingProfile')),
  };
  const group = (id: string, title: string, groupFields: DetailFieldConfig[]) => ({
    id,
    title: title.toUpperCase(),
    fields: groupFields,
  });
  const fastTab = (id: string, title: string, content: React.ReactNode): DetailSectionConfig => ({
    id,
    title,
    visualVariant: 'legalEntity',
    content: <Box sx={{ gridColumn: '1 / -1', minWidth: 0 }}>{content}</Box>,
  });

  // Lines tab: compact header FastTab (one row of field groups), like sales order.
  const compactHeader: DetailSectionConfig = {
    id: 'invoice-header',
    title: t('freeTextInvoices.header'),
    visualVariant: 'legalEntity',
    columns: 5,
    groups: [
      group('customer', t('fields.customer'), [fields.customerAccount, fields.customerName]),
      group('invoice', t('freeTextInvoices.invoice'), [fields.invoiceDate, fields.dueDate]),
      group('references', t('fields.description'), [fields.invoiceAccount, fields.description]),
      group('sales-tax', t('freeTextInvoices.salesTax'), [fields.currencyCode, fields.taxGroup]),
      group('payment', t('freeTextInvoices.payment'), [fields.paymentTerms, fields.paymentMethod]),
    ],
  };
  // Header tab: the full header as FastTabs with key values in the heading.
  const headerSections: DetailSectionConfig[] = [
    {
      id: 'header-general',
      title: t('common.general'),
      visualVariant: 'legalEntity',
      columns: 4,
      summaryItems: [
        {
          id: 'invoiceId',
          label: t('freeTextInvoices.invoice'),
          value: values.invoiceId,
          accent: true,
        },
        {
          id: 'customerAccount',
          label: t('fields.customerAccount'),
          value: display.customerAccount,
          accent: true,
        },
        { id: 'customerName', label: t('fields.customerName'), value: customerName },
      ],
      groups: [
        group('customer', t('fields.customer'), [
          fields.customerAccount,
          fields.customerName,
          fields.invoiceAccount,
        ]),
        group('invoice', t('freeTextInvoices.invoice'), [
          fields.invoiceId,
          fields.invoiceDate,
          fields.dueDate,
        ]),
        group('description', t('fields.description'), [fields.description]),
        group('status', t('common.status'), [fields.posted]),
      ],
    },
    {
      id: 'header-setup',
      title: t('freeTextInvoices.setup'),
      visualVariant: 'legalEntity',
      columns: 4,
      summaryItems: [
        {
          id: 'currencyCode',
          label: t('fields.currency'),
          value: display.currencyCode,
          accent: true,
        },
        { id: 'taxGroup', label: t('fields.salesTaxGroup'), value: display.taxGroup, accent: true },
      ],
      groups: [
        group('currency', t('fields.currency'), [fields.currencyCode]),
        group('sales-tax', t('freeTextInvoices.salesTax'), [fields.taxGroup]),
        group('posting', t('freeTextInvoices.posting'), [fields.postingProfile]),
      ],
    },
    {
      id: 'header-payment',
      title: t('freeTextInvoices.payment'),
      visualVariant: 'legalEntity',
      columns: 4,
      summaryItems: [
        {
          id: 'paymentTerms',
          label: t('freeTextInvoices.paymentTerms'),
          value: display.paymentTerms,
          accent: true,
        },
      ],
      groups: [
        group('payment', t('freeTextInvoices.payment'), [
          fields.paymentTerms,
          fields.paymentMethod,
        ]),
      ],
    },
  ];
  const lineColumns = useFreeTextInvoiceLineColumns({
    company,
    editing,
    amount,
    ledgerText,
    setLedgerLabels,
    taxes,
  });
  const selectedLineIds = React.useMemo(
    () => (selectedLineId ? [selectedLineId] : []),
    [selectedLineId]
  );
  const addLine = async () => {
    // View mode: Add line switches the page to edit mode first (D365).
    if (!editing) beginEdit();
    if (!draftRef.current || busy || !(await grid.current?.saveEdit())) return;
    const line = {
      ...newLine(),
      lineNum: draftRef.current.lines.length + 1,
      taxGroup: draftRef.current.taxGroup,
    };
    update({ lines: [...draftRef.current.lines, line] });
    setSelectedLineId(line.id);
  };
  const removeLine = () => {
    grid.current?.cancelEdit();
    update({ lines: display.lines.filter((line) => line.id !== selectedLineId) });
    setSelectedLineId(undefined);
  };
  const ledgerOf = (line: Line) =>
    ledgerText(line.ledgerDimension) ||
    line.ledgerDisplay ||
    (line.ledgerDimension ? String(line.ledgerDimension) : '');
  const lineDetailFields = (line: Line) =>
    lineTab === 'general' ? (
      <>
        <MasterDetailField label={t('freeTextInvoices.invoiceText')} value={line.description} />
        <MasterDetailField label={t('fields.quantity')} value={line.quantity} numeric />
        <MasterDetailField label={t('fields.unitPrice')} value={amount(line.unitPrice)} numeric />
        <MasterDetailField
          label={t('freeTextInvoices.amount')}
          value={editing ? '—' : amount(line.netAmount)}
          numeric
        />
      </>
    ) : lineTab === 'setup' ? (
      <>
        <MasterDetailField
          label={t('fields.salesTaxGroup')}
          value={line.taxGroup}
          previewEntity={FOUNDATION_PREVIEWS.SALES_TAX_GROUP}
        />
        <MasterDetailField
          label={t('freeTextInvoices.itemTaxGroup')}
          value={line.taxItemGroup}
          previewEntity={FOUNDATION_PREVIEWS.ITEM_SALES_TAX_GROUP}
        />
      </>
    ) : (
      <>
        <MasterDetailField
          label={t('freeTextInvoices.ledgerAccount')}
          value={ledgerOf(line)}
          lookup
        />
        <MasterDetailField
          label={t('freeTextInvoices.defaultDimension')}
          value={line.defaultDimension || ''}
        />
      </>
    );
  const totalsSection = fastTab(
    'totals',
    t('freeTextInvoices.totals'),
    <MasterDetailTotals
      items={[
        { id: 'net', label: t('freeTextInvoices.netAmount'), value: amount(display.netAmount) },
        { id: 'tax', label: t('freeTextInvoices.salesTax'), value: amount(display.taxAmount) },
        {
          id: 'total',
          label: t('freeTextInvoices.invoiceAmount'),
          value: `${amount(display.invoiceAmount)} ${display.currencyCode}`,
          emphasis: true,
        },
      ]}
    />
  );
  const problem = error || lookups.error?.message || taxes.error?.message;
  const sections: DetailSectionConfig[] = [
    ...(problem
      ? [fastTab('error', t('common.information'), <Alert severity="error">{problem}</Alert>)]
      : []),
    ...(tab === 'header'
      ? headerSections
      : [
          compactHeader,
          fastTab(
            'invoice-lines',
            t('freeTextInvoices.lines'),
            <MasterDetailLines
              toolbarLabel={t('freeTextInvoices.lines')}
              actions={[
                {
                  id: 'add',
                  label: t('freeTextInvoices.addLine'),
                  icon: <AddIcon />,
                  disabled: (!editing && !allowedEdit) || busy,
                  onClick: () => void addLine(),
                },
                {
                  id: 'remove',
                  label: t('actions.remove'),
                  icon: <DeleteOutlineIcon />,
                  disabled: !editing || busy || !selectedLine,
                  onClick: removeLine,
                },
              ]}
              saving={busy}
              grid={
                <>
                  <MasterDetailLinesGrid<Line>
                    editing={editing}
                    busy={busy}
                    canEdit={allowedEdit}
                    onEdit={beginEdit}
                    ref={grid}
                    rows={display.lines}
                    columns={lineColumns}
                    storageKey="accounts-receivable.free-invoice-lines"
                    hideFilterRow={!filterVisible}
                    selectionMode="single"
                    selectedIds={selectedLineIds}
                    onRowClick={(line) => setSelectedLineId(line.id)}
                    onSelectionChange={(ids) =>
                      setSelectedLineId(ids[0] ? String(ids[0]) : undefined)
                    }
                    onRowSave={(row) => {
                      const current = draftRef.current;
                      if (!current || !row.id) return;
                      const original = current.lines.find((line) => line.id === row.id);
                      if (!original) return;
                      const updated = {
                        ...original,
                        ...row,
                        quantity: Number(row.quantity ?? original.quantity),
                        unitPrice: Number(row.unitPrice ?? original.unitPrice),
                        ledgerDimension: Number(row.ledgerDimension ?? original.ledgerDimension),
                      };
                      if (!isValidInvoiceLine(updated)) {
                        throw new Error(
                          t(
                            'freeTextInvoices.requiredLines',
                            'Select a customer and invoice date. Add at least one line with a description, ledger account, positive quantity and non-negative unit price.'
                          )
                        );
                      }
                      changeDraft({
                        ...current,
                        lines: current.lines.map((line) => (line.id === row.id ? updated : line)),
                      });
                    }}
                  />
                  {editing && (
                    <Typography
                      variant="caption"
                      color="text.secondary"
                      sx={{ display: 'block', mt: 0.5 }}
                    >
                      {t('freeTextInvoices.serverAmounts')}
                    </Typography>
                  )}
                </>
              }
            />
          ),
          fastTab(
            'line-details',
            t('freeTextInvoices.lineDetails'),
            <MasterDetailLineDetails
              ariaLabel={t('freeTextInvoices.lineDetails')}
              tabs={[
                { value: 'general', label: t('common.general') },
                { value: 'setup', label: t('freeTextInvoices.setup') },
                { value: 'dimensions', label: t('freeTextInvoices.financialDimensions') },
              ]}
              value={lineTab}
              onChange={setLineTab}
              hasLine={Boolean(selectedLine)}
              emptyLabel={t('freeTextInvoices.selectLine')}
            >
              {selectedLine && lineDetailFields(selectedLine)}
            </MasterDetailLineDetails>
          ),
        ]),
    totalsSection,
  ];
  const records = React.useMemo(
    () => (isNew ? [invoice] : (invoices.data ?? [invoice]).map(asInvoice)),
    [isNew, invoice, invoices.data]
  );
  // Navigation pane rows: invoice number (or "not posted") and customer account.
  const nameOf = (account: string) => {
    const row = customers.data?.find((item) => item.accountNumber === account);
    return row ? localizedName({ name: row.name, nameAlias: row.nameAr }, isRtl) : '';
  };
  const navigationRecords = records.map((record) => ({
    id: record.id,
    title: record.invoiceId || t('freeTextInvoices.notPosted'),
    subtitle: [record.customerAccount, nameOf(record.customerAccount)].filter(Boolean).join(' - '),
    recId: record.recId,
  }));
  // "INV-001 : Customer name", like "SO-001 : Customer name".
  const title = isNew
    ? t('freeTextInvoices.newInvoice', 'New free text invoice')
    : `${invoice.invoiceId || t('freeTextInvoices.notPosted')} : ${customerName}`;
  return (
    <>
      <MasterDetailPage
        breadcrumb={{
          listPath: paths.FREE_TEXT_INVOICES,
          labelKey: isNew ? 'freeTextInvoices.newInvoice' : 'freeTextInvoices.freeTextInvoice',
        }}
        records={navigationRecords}
        selectedId={invoice.id}
        onSelect={(id) => {
          const record = navigationRecords.find((row) => row.id === id);
          if (record && id !== invoice.id && !editing && !busy)
            navigate(paths.freeTextInvoice(record.recId));
        }}
        title={title}
        subtitle={t('freeTextInvoices.details')}
        viewLabel={t('common.standardView')}
        status={
          isNew
            ? undefined
            : invoice.posted
              ? t('freeTextInvoices.posted')
              : t('freeTextInvoices.notPosted')
        }
        filterLabel={t('actions.filter')}
        emptyLabel={t('messages.noRecords', 'No records')}
        editing={editing || busy}
        navigationWidthStorageKey="free-invoice"
        backAction={
          <ActionPaneBackButton
            label={t('actions.back')}
            onClick={() => navigate(paths.FREE_TEXT_INVOICES)}
            disabled={editing || busy}
          />
        }
        actionPane={
          <>
            <EnterpriseCrudActions
              editLabel={t('actions.edit')}
              newLabel={t('actions.new')}
              deleteLabel={t('actions.delete')}
              saveLabel={t('actions.save')}
              cancelLabel={t('actions.cancel')}
              editPermission={PERMISSIONS.FREE_TEXT_INVOICE_EDIT}
              newPermission={PERMISSIONS.FREE_TEXT_INVOICE_CREATE}
              deletePermission={PERMISSIONS.FREE_TEXT_INVOICE_DELETE}
              savePermission={
                isNew ? PERMISSIONS.FREE_TEXT_INVOICE_CREATE : PERMISSIONS.FREE_TEXT_INVOICE_EDIT
              }
              canEdit={allowedEdit && !isNew && !busy}
              canNew={!busy}
              canDelete={!isNew && !invoice.posted && !busy}
              editing={editing}
              saving={busy}
              onEdit={beginEdit}
              onNew={() => navigate(paths.newFreeTextInvoice(invoice.customerAccount))}
              onDelete={() => setConfirmation('delete')}
              onSave={() => void save()}
              onCancel={() => {
                grid.current?.cancelEdit();
                changeDraft(null);
                setError('');
                if (isNew) navigate(paths.FREE_TEXT_INVOICES);
                else navigate(paths.freeTextInvoice(invoice.recId), { replace: true });
              }}
            />
          </>
        }
        ribbons={[
          {
            id: 'free-text-invoice',
            label: t('freeTextInvoices.freeTextInvoice'),
            persistenceKey: 'accounts-receivable.free-text-invoice.ribbon-pinned',
            groups: [
              {
                id: 'post',
                label: t('freeTextInvoices.post'),
                actions: hasPermission(PERMISSIONS.FREE_TEXT_INVOICE_POST)
                  ? [
                      {
                        id: 'post',
                        label: t('freeTextInvoices.post'),
                        disabled: busy || isNew || invoice.posted,
                        onClick: () => setConfirmation('post'),
                      },
                    ]
                  : [],
              },
              {
                id: 'view',
                label: t('freeTextInvoices.view'),
                actions: [
                  {
                    id: 'totals',
                    label: t('freeTextInvoices.totals'),
                    disabled: busy || isNew,
                    onClick: () => setTotalsOpen(true),
                  },
                ],
              },
            ],
          },
        ]}
        commands={[{ id: 'general', label: t('common.general'), disabled: true }]}
        onSearch={() => {
          setTab('lines');
          setFilterVisible((value) => !value);
        }}
        values={values}
        onFieldEdit={beginEdit}
        canEditField={(name) => allowedEdit && !busy && !readOnlyFields.has(name)}
        viewTabs={{
          value: tab,
          onChange: (value) => void switchTab(value),
          ariaLabel: t('freeTextInvoices.title'),
        }}
        sections={sections}
        recordTableName={isNew ? undefined : 'CustInvoiceTable'}
        getAuditRecordId={(record) => record.recId}
        attachments={
          !isNew && !editing
            ? { refTableId: recordTableId('CustInvoiceTable'), refRecId: invoice.recId }
            : undefined
        }
        onRefresh={() => {
          if (!editing && !busy) void invalidate();
        }}
        relatedInformation={{
          title: t('relatedInformation.title'),
          sections: [
            {
              id: 'customer',
              label: t('fields.customer'),
              defaultExpanded: true,
              content: (
                <Typography sx={{ fontSize: '0.75rem', whiteSpace: 'pre-line' }}>
                  {`${invoice.customerAccount}\n${customerName}`}
                </Typography>
              ),
            },
          ],
        }}
      />
      <ConfirmationDialog
        open={Boolean(confirmation)}
        title={confirmation === 'post' ? t('freeTextInvoices.post') : t('actions.delete')}
        message={
          confirmation === 'post'
            ? t('freeTextInvoices.confirmPost')
            : t('freeTextInvoices.confirmDelete', 'Delete this unposted free text invoice?')
        }
        confirmLabel={confirmation === 'post' ? t('freeTextInvoices.post') : t('actions.delete')}
        loading={busy}
        onClose={() => {
          if (!busy) setConfirmation(null);
        }}
        onConfirm={() => void runConfirmed()}
      />
      <DocumentTotalsDrawer
        open={totalsOpen}
        onClose={() => setTotalsOpen(false)}
        title={t('freeTextInvoices.totals')}
        viewLabel={t('common.standardView')}
        sections={[
          {
            id: 'totals',
            title: invoice.currencyCode,
            columns: [
              {
                id: 'amounts',
                fields: [
                  {
                    id: 'net',
                    label: t('freeTextInvoices.netAmount'),
                    value: amount(invoice.netAmount),
                  },
                  {
                    id: 'tax',
                    label: t('freeTextInvoices.salesTax'),
                    value: amount(invoice.taxAmount),
                  },
                  {
                    id: 'total',
                    label: t('freeTextInvoices.invoiceAmount'),
                    value: amount(invoice.invoiceAmount),
                    emphasized: true,
                  },
                ],
              },
            ],
          },
        ]}
      />
    </>
  );
}
