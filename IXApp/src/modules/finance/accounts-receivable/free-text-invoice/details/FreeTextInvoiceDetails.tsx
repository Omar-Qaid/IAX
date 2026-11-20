import { FreeTextInvoiceDetailsDialogs } from './FreeTextInvoiceDetailsDialogs';
import { buildFreeTextInvoiceTotals } from './freeTextInvoiceTotals';
import { buildFreeTextInvoiceHeaderFields } from './FreeTextInvoiceHeaderFields';
import { FreeTextInvoiceLinesGrid } from '../lines/FreeTextInvoiceLinesGrid';
import { buildFreeTextInvoiceLineTabs } from '../lines/FreeTextInvoiceLineTabs';
import { buildFreeTextInvoiceHeaderSections } from './FreeTextInvoiceHeaderSections';
import {
  useFreeTextInvoiceDocument,
  type FreeTextInvoiceDetailsProps,
} from './useFreeTextInvoiceDocument';
import React from 'react';
import { Alert, Box, Typography } from '@mui/material';
import { PERMISSIONS } from '@core/permissions/permissions';
import { MasterDetailPage } from '@patterns/master-detail';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { recordTableId } from '@shared/components/documents';
import type { DetailSectionConfig } from '@patterns/list-details/types';
import { CommitAwareCrudActions } from '@shared/components/action-pane/CommitAwareCrudActions';
import { localizedName } from '@shared/utilities/localizedName';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS as paths } from '../../routes/accountsReceivableRoutePaths';

import { asInvoice } from './freeTextInvoiceModel';

export function FreeTextInvoiceDetails({
  company,
  invoice,
  isNew,
  startEditing,
}: FreeTextInvoiceDetailsProps) {
  const scope = useFreeTextInvoiceDocument({ company, invoice, isNew, startEditing });
  const {
    t,
    isRtl,
    hasPermission,
    navigate,
    allowedEdit,
    busy,
    error,
    setError,
    setConfirmation,
    setTotalsOpen,
    tab,
    setTab,
    setFilterVisible,
    grid,
    editing,
    display,
    changeDraft,
    customers,
    lookups,
    taxes,
    invoices,
    customerName,
    beginEdit,
    switchTab,
    invalidate,
    save,
  } = scope;
  // View mode: plain field configs, rendered by the shared engine from `values`
  // (hover cards for master data, pencil to start editing). Edit mode: editors.
  const { readOnlyFields, values, fields } = buildFreeTextInvoiceHeaderFields(scope);
  const fastTab = (id: string, title: string, content: React.ReactNode): DetailSectionConfig => ({
    id,
    title,
    visualVariant: 'legalEntity',
    content: <Box sx={{ gridColumn: '1 / -1', minWidth: 0 }}>{content}</Box>,
  });

  const { compactHeader, headerSections } = buildFreeTextInvoiceHeaderSections({
    t,
    fields,
    values,
    display,
    customerName,
  });
  const { totalsContent, totalsSections } = buildFreeTextInvoiceTotals(scope, invoice);
  const totalsSection = fastTab('totals', t('freeTextInvoices.totals'), totalsContent);
  const { lineDetails } = buildFreeTextInvoiceLineTabs(scope);
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
            <FreeTextInvoiceLinesGrid scope={scope} company={company} />
          ),
          fastTab('line-details', t('freeTextInvoices.lineDetails'), lineDetails),
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
            <CommitAwareCrudActions
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
              onSave={() => save()}
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
                        disabled: busy || editing || isNew || invoice.posted,
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
                    disabled: busy || editing || isNew,
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
      <FreeTextInvoiceDetailsDialogs scope={scope} totalsSections={totalsSections} />
    </>
  );
}
