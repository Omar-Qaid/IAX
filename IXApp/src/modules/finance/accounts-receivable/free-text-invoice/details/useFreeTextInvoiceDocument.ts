import React from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@core/auth/useAuth';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { freeTextInvoiceQueryKeys } from '../../queries/accountsReceivableQueryKeys';
import type { DataGridHandle } from '@shared/components/data-grid/types';
import { useGridFilterRow } from '@shared/components/data-grid/hooks/useGridFilterRow';
import type { LookupOption } from '@shared/components/lookups/types';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { useNotifications } from '@shared/hooks/useNotifications';
import { localizedName } from '@shared/utilities/localizedName';
import { customerQuickCreateApi } from '../../customer/api/customerQuickCreateApi';
import { freeTextInvoiceApi, type FreeTextInvoiceInput } from '../api/freeTextInvoiceApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS as paths } from '../../routes/accountsReceivableRoutePaths';
import type { Invoice } from './freeTextInvoiceModel';
import { isValidInvoiceLine } from '../lines/freeTextInvoiceLine';
export interface FreeTextInvoiceDetailsProps {
  company: string;
  invoice: Invoice;
  isNew: boolean;
  startEditing: boolean;
}

export function useFreeTextInvoiceDocument({
  company,
  invoice,
  isNew,
  startEditing,
}: FreeTextInvoiceDetailsProps) {
  const { t, isRtl, currentLanguage } = useAppTranslation();
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const cache = useQueryClient();
  const { notifySuccess } = useNotifications();
  const allowedEdit =
    hasPermission(
      isNew ? PERMISSIONS.FREE_TEXT_INVOICE_CREATE : PERMISSIONS.FREE_TEXT_INVOICE_EDIT
    ) && !invoice.posted;
  const [draft, setDraft] = React.useState<Invoice | null>(() =>
    allowedEdit && (isNew || startEditing) ? structuredClone(invoice) : null
  );
  const draftRef = React.useRef(draft);
  const [busy, setBusy] = React.useState(false);
  const operationLock = React.useRef(false);
  const [error, setError] = React.useState('');
  const [confirmation, setConfirmation] = React.useState<'post' | 'delete' | null>(null);
  const [totalsOpen, setTotalsOpen] = React.useState(false);
  const [tab, setTab] = React.useState('lines');
  const [lineTab, setLineTab] = React.useState('general');
  const [selectedLineId, setSelectedLineId] = React.useState<string | undefined>(
    invoice.lines[0]?.id
  );
  const [filterVisible, setFilterVisible] = useGridFilterRow(
    'accounts-receivable.free-invoice-lines'
  );
  const [ledgerLabels, setLedgerLabels] = React.useState<Record<number, string>>({});
  const grid = React.useRef<DataGridHandle>(null);
  const editing = Boolean(draft);
  const display = draft ?? invoice;
  const selectedLine = display.lines.find((line) => line.id === selectedLineId);
  useUnsavedChanges(editing);
  const changeDraft = (next: Invoice | null) => {
    draftRef.current = next;
    setDraft(next);
  };
  const update = (change: Partial<Invoice>) => {
    setDraft((current) => {
      if (!current) return current;
      const next = { ...current, ...change };
      draftRef.current = next;
      return next;
    });
  };
  const customers = useQuery({
    queryKey: freeTextInvoiceQueryKeys.customers(company),
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
    enabled: hasPermission(PERMISSIONS.CUSTOMER_VIEW),
  });
  const lookups = useQuery({
    queryKey: freeTextInvoiceQueryKeys.lookup(company, 'options'),
    queryFn: ({ signal }) => freeTextInvoiceApi.headerLookups(signal),
    enabled: editing,
  });
  const taxes = useQuery({
    queryKey: freeTextInvoiceQueryKeys.lookup(company, 'tax-groups'),
    queryFn: ({ signal }) => freeTextInvoiceApi.taxGroups(signal),
    enabled: editing,
  });
  const ledgerReferences = useQuery({
    queryKey: freeTextInvoiceQueryKeys.lookup(company, 'ledger-references'),
    queryFn: ({ signal }) =>
      freeTextInvoiceApi.ledgerDimensions({ pageNumber: 1, pageSize: 100, search: '', signal }),
  });
  const ledgerCodes = React.useMemo(() => {
    const codes = new Map<string, string | undefined>();
    ledgerReferences.data?.data.forEach((option) => codes.set(String(option.id), option.code));
    return codes;
  }, [ledgerReferences.data]);
  const ledgerText = React.useCallback(
    (id: number) => ledgerLabels[id] || ledgerCodes.get(String(id)),
    [ledgerLabels, ledgerCodes]
  );
  const invoices = useQuery({
    queryKey: freeTextInvoiceQueryKeys.list(company),
    queryFn: ({ signal }) => freeTextInvoiceApi.list(signal),
    enabled: !isNew,
  });
  const customer = customers.data?.find((row) => row.accountNumber === display.customerAccount);
  const customerName = customer
    ? localizedName({ name: customer.name, nameAlias: customer.nameAr }, isRtl)
    : display.customerAccount;
  const customerOptions: LookupOption[] = (customers.data ?? []).map((row) => ({
    id: row.accountNumber,
    code: row.accountNumber,
    name: row.name,
    nameAlias: row.nameAr,
  }));
  React.useEffect(() => {
    const current = draftRef.current;
    if (!isNew || !current || current.currencyCode || current.invoiceAccount) return;
    const row = customers.data?.find((item) => item.accountNumber === current.customerAccount);
    if (row)
      changeDraft({
        ...current,
        invoiceAccount: row.invoiceAccount || row.accountNumber,
        currencyCode: row.currencyCode,
        paymentTerms: row.paymTermId,
        paymentMethod: row.paymModeId,
        taxGroup: row.taxGroupId,
      });
  }, [customers.data, isNew]);
  const amountFormat = React.useMemo(
    () =>
      new Intl.NumberFormat(currentLanguage.code, {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
      }),
    [currentLanguage.code]
  );
  const amount = React.useCallback((value: number) => amountFormat.format(value), [amountFormat]);
  const beginEdit = () => {
    if (allowedEdit && !busy) {
      setError('');
      changeDraft(structuredClone(invoice));
    }
  };
  const switchTab = async (value: string) => {
    if (busy || (grid.current && !(await grid.current.saveEdit()))) return;
    setTab(value);
  };
  const invalidate = async () => {
    await Promise.all([
      cache.invalidateQueries({ queryKey: freeTextInvoiceQueryKeys.list(company) }),
      cache.invalidateQueries({
        queryKey: freeTextInvoiceQueryKeys.invoice(company, String(invoice.recId)),
      }),
    ]);
  };
  const save = async () => {
    if (!allowedEdit || !draftRef.current || operationLock.current) return;
    operationLock.current = true;
    setBusy(true);
    setError('');
    try {
      if (grid.current && !(await grid.current.saveEdit())) return;
      const record = draftRef.current!;
      if (
        !record.customerAccount.trim() ||
        !record.invoiceDate ||
        !record.lines.length ||
        record.lines.some((line) => !isValidInvoiceLine(line))
      ) {
        throw new Error(
          t(
            'freeTextInvoices.requiredLines',
            'Select a customer and invoice date. Add at least one line with a description, ledger account, positive quantity and non-negative unit price.'
          )
        );
      }
      const input: FreeTextInvoiceInput = {
        customerAccount: record.customerAccount,
        invoiceAccount: record.invoiceAccount || undefined,
        invoiceDate: record.invoiceDate.slice(0, 10),
        currencyCode: record.currencyCode || undefined,
        paymentTerms: record.paymentTerms || undefined,
        paymentMethod: record.paymentMethod || undefined,
        postingProfile: record.postingProfile || undefined,
        taxGroup: record.taxGroup || undefined,
        description: record.description,
        defaultDimension: isNew ? record.defaultDimension || undefined : record.defaultDimension,
        lines: record.lines.map((line) => ({
          description: line.description,
          quantity: line.quantity,
          unitPrice: line.unitPrice,
          ledgerDimension: line.ledgerDimension,
          defaultDimension: line.recId ? line.defaultDimension : undefined,
          taxGroup: line.taxGroup || undefined,
          taxItemGroup: line.taxItemGroup || undefined,
        })),
      };
      const saved = await freeTextInvoiceApi.save(input, isNew ? undefined : invoice.recId);
      cache.setQueryData(
        ['accounts-receivable', 'free-text-invoice', company, String(saved.recId)],
        saved
      );
      changeDraft(null);
      navigate(paths.freeTextInvoice(saved.recId), { replace: true });
      await invalidate();
      setSelectedLineId(saved.lines[0] ? String(saved.lines[0].recId) : undefined);
      notifySuccess(t('messages.savedSuccessfully', 'Saved successfully.'));
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : t('errors.generic'));
    } finally {
      operationLock.current = false;
      setBusy(false);
    }
  };
  const runConfirmed = async () => {
    if (operationLock.current || editing || isNew || invoice.posted || !confirmation) return;
    if (
      !hasPermission(
        confirmation === 'post'
          ? PERMISSIONS.FREE_TEXT_INVOICE_POST
          : PERMISSIONS.FREE_TEXT_INVOICE_DELETE
      )
    )
      return;
    operationLock.current = true;
    setBusy(true);
    setError('');
    try {
      if (confirmation === 'post') {
        const posted = await freeTextInvoiceApi.post(invoice.recId);
        cache.setQueryData(freeTextInvoiceQueryKeys.invoice(company, String(invoice.recId)), {
          ...posted,
          id: String(posted.recId),
        });
        notifySuccess(t('freeTextInvoices.postedSuccess', 'Invoice posted.'));
      } else {
        await freeTextInvoiceApi.remove(invoice.recId);
        navigate(paths.FREE_TEXT_INVOICES);
      }
      setConfirmation(null);
      await invalidate();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : t('errors.generic'));
      setConfirmation(null);
    } finally {
      operationLock.current = false;
      setBusy(false);
    }
  };
  return {
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
  };
}

export type FreeTextInvoiceDocument = ReturnType<typeof useFreeTextInvoiceDocument>;
