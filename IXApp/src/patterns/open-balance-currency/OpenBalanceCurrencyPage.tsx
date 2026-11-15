import React from 'react';
import { Alert, Box, Radio, Tab, Tabs, Typography, useMediaQuery, useTheme } from '@mui/material';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { EditableViewField } from '@shared/components/fields/EditableViewField';
import { documentTableIds } from '@shared/components/documents/recordTableIds';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { localizedName } from '@shared/utilities/localizedName';
import type { BalancePartyKind, PartyOpenBalanceCurrency } from './types';

type BalanceRecord = PartyOpenBalanceCurrency & { id: string };

interface BalanceRow {
  id: string;
  description: string;
  currency: string;
  amountCur?: number | null;
  amountMst?: number | null;
  amountReporting?: number | null;
}

export interface OpenBalanceCurrencyPageProps {
  kind: BalancePartyKind;
  balance: PartyOpenBalanceCurrency;
  relatedBalance: number | null;
  relatedUnavailable?: boolean;
  refreshing?: boolean;
  onBack: () => void;
  onRefresh: () => void;
  onViewContacts?: () => void;
  onOpenTransactions?: () => void;
}

/** Shared read-only CustOpenBalanceCurrency / VendOpenBalanceCurrency layout. */
export function OpenBalanceCurrencyPage({
  kind,
  balance,
  relatedBalance,
  relatedUnavailable,
  refreshing,
  onBack,
  onRefresh,
  onViewContacts,
  onOpenTransactions,
}: OpenBalanceCurrencyPageProps): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const compactViewport = useMediaQuery(useTheme().breakpoints.down('md'));
  const tr = (key: string) => t(`partyBalance.${key}`);
  const format = (value: number | null | undefined) =>
    value === undefined
      ? ''
      : value === null
        ? '—'
        : new Intl.NumberFormat(isRtl ? 'ar-SA' : 'en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          }).format(value);
  const name = localizedName(balance, isRtl);
  const gridRef = React.useRef<DataGridHandle>(null);
  const [filterVisible, setFilterVisible] = React.useState(false);
  const record = React.useMemo<BalanceRecord>(
    () => ({ ...balance, id: String(balance.recId) }),
    [balance]
  );
  const records = React.useMemo(() => [record], [record]);
  const [selectedId, setSelectedId] = React.useState<string | null>(null);
  const partyLabel = tr(kind);
  const otherKind = kind === 'customer' ? 'vendor' : 'customer';
  const rows: BalanceRow[] = balance.currencies.map((currency) => ({
    id: `currency-${currency.currencyCode}`,
    description: tr('balanceInCurrency'),
    currency: currency.currencyCode,
    amountCur: currency.amountCur,
    amountMst: currency.amountMst,
    amountReporting: balance.reportingCurrency ? currency.amountReporting : undefined,
  }));
  rows.push(
    {
      id: 'accounting',
      description: tr('accountingBalance'),
      currency: '',
      amountMst: balance.balanceMst,
    },
    {
      id: 'reporting',
      description: tr('reportingBalance'),
      currency: '',
      amountReporting: balance.reportingCurrency ? balance.balanceReporting : undefined,
    }
  );
  if (kind === 'customer')
    rows.push(
      { id: 'credit', description: tr('creditLimit'), currency: '', amountMst: balance.creditMax },
      {
        id: 'remaining',
        description: tr('creditRemaining'),
        currency: '',
        amountMst: balance.creditMax - balance.balanceMst,
      }
    );
  const activeRow = rows.some((row) => row.id === selectedId) ? selectedId : rows[0]?.id;
  const columns: ColumnDef<BalanceRow>[] = [
    {
      field: 'selection',
      headerName: '○',
      width: 40,
      minWidth: 40,
      sortable: false,
      filterable: false,
      renderCell: ({ row }) => (
        <Radio
          checked={row.id === activeRow}
          tabIndex={-1}
          aria-hidden
          size="small"
          sx={{ p: 0, pointerEvents: 'none', '& .MuiSvgIcon-root': { fontSize: 16 } }}
        />
      ),
    },
    {
      field: 'description',
      headerName: tr('description'),
      width: 150,
      renderCell: ({ row }) => (
        <Box
          component="span"
          sx={{
            color: 'text.primary',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
          }}
        >
          {row.description}
        </Box>
      ),
    },
    { field: 'currency', headerName: tr('currency'), width: 95 },
    {
      field: 'amountCur',
      headerName: tr('balanceCurrency'),
      width: 130,
      align: 'right',
      renderCell: ({ row }) => (
        <Box component="span" sx={{ color: 'text.primary' }}>
          {format(row.amountCur)}
        </Box>
      ),
    },
    {
      field: 'amountMst',
      headerName: tr('balance'),
      width: 95,
      align: 'right',
      renderCell: ({ row }) => (
        <Box component="span" sx={{ color: 'text.primary' }}>
          {format(row.amountMst)}
        </Box>
      ),
    },
    {
      field: 'amountReporting',
      headerName: `${tr('balanceReporting')}${balance.reportingCurrency ? ` (${balance.reportingCurrency})` : ''}`,
      width: 110,
      align: 'right',
      renderCell: ({ row }) => (
        <Box component="span" sx={{ color: 'text.primary' }}>
          {format(row.amountReporting)}
        </Box>
      ),
    },
  ];
  const content = (
    <Box sx={{ px: 3, pt: 2.5, pb: 1 }}>
      <Typography sx={{ fontSize: 12, mb: 0.75 }}>
        {balance.accountNum} : {name} | {t('common.standardView', 'Standard view')}
      </Typography>
      <Typography component="h1" sx={{ fontSize: 21, fontWeight: 600, mb: 1.5 }}>
        {tr(`${kind}Balance`)}
      </Typography>
      {!balance.reconciled && (
        <Alert severity="warning" sx={{ mb: 1 }}>
          {tr('notReconciled')}
        </Alert>
      )}
      {relatedUnavailable && (
        <Alert severity="info" sx={{ mb: 1 }}>
          {tr('relatedUnavailable')}
        </Alert>
      )}
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2.5, alignItems: 'flex-start', mb: 1.5 }}>
        <Summary title={partyLabel}>
          <Field label={tr('accountNumber')} value={balance.accountNum} width={150} />
          <Field label={tr('name')} value={name} />
          <Field label={tr('currency')} value={balance.currencyCode} narrow />
        </Summary>
        <Summary title={tr(`${kind}Balance`)}>
          <Field label={tr('currency')} value={balance.accountingCurrency} narrow />
          <Field label={tr('balance')} value={format(balance.balanceMst)} numeric narrow />
        </Summary>
        <Summary title={tr(`${otherKind}Balance`)}>
          <Field label={tr('currency')} value={balance.accountingCurrency} narrow />
          <Field label={tr('balance')} value={format(relatedBalance)} numeric narrow />
        </Summary>
      </Box>
      <Tabs value="currencies" sx={{ minHeight: 32, mb: 1.5 }}>
        <Tab
          value="currencies"
          label={tr('currencies')}
          sx={{ minHeight: 32, p: 0, minWidth: 70, textTransform: 'none', fontSize: 12 }}
        />
      </Tabs>
      <Box sx={{ height: { xs: 420, md: 'max(310px, calc(100dvh - 365px))' } }}>
        <DataGrid<BalanceRow>
          ref={gridRef}
          mobilePresentation="table"
          rows={rows}
          columns={columns}
          getRowId={(row) => row.id}
          height="100%"
          showColumnBorders={false}
          showCellBorders={false}
          hideColumnMenu
          selectedIds={activeRow ? [activeRow] : []}
          onSelectionChange={(ids) => setSelectedId(ids.length ? String(ids[0]) : null)}
          rowHeight={31}
          headerHeight={29}
          hideToolbar
          hideFooter
          hideSidebar
          hideFilterRow={!filterVisible}
          selectionMode="single"
          storageKey={`${kind}.open-balance-currency.reference-v2`}
        />
      </Box>
    </Box>
  );
  const config: EnterpriseListDetailsConfig<BalanceRecord> = {
    readOnly: true,
    dataSource: {
      type: 'controlled',
      records,
      onRecordsChange: () => undefined,
      refresh: onRefresh,
    },
    initialSelectedId: record.id,
    createRecord: () => record,
    getPrimaryText: (row) => row.accountNum,
    getSecondaryText: (row) => localizedName(row, isRtl),
    getValues: () => ({}),
    setValues: (row) => row,
    headerFields: [],
    detailHeader: <></>,
    sections: [{ id: 'balance', title: tr(`${kind}Balance`), hideHeader: true, content }],
    recordTableName: kind === 'customer' ? 'CustTable' : 'VendTable',
    getAuditRecordId: (row) => row.recId,
    attachments: {
      refTableId: kind === 'customer' ? documentTableIds.custTable : documentTableIds.vendTable,
      getRefRecId: (row) => row.recId,
    },
    actionPaneAfterListContent: (
      <EnterpriseCrudActions
        editLabel={t('actions.edit')}
        newLabel={t('actions.new')}
        deleteLabel={t('actions.delete')}
        canEdit={false}
        canNew={false}
        canDelete={false}
      />
    ),
    commands: onOpenTransactions || onViewContacts
      ? [
          {
            id: kind,
            label: partyLabel,
            groups: [
              ...(onViewContacts ? [{ id: 'accounts', label: t('customerRibbon.groups.accounts', 'Accounts'), actions: [{ id: 'view-contacts', label: t('contactPerson.view', 'View contacts'), onClick: onViewContacts }] }] : []),
              ...(onOpenTransactions ? [{
                id: 'transactions',
                label: t('customerRibbon.groups.transactions', 'Transactions'),
                actions: [
                  {
                    id: 'open-transactions',
                    label: tr('openTransactions'),
                    onClick: onOpenTransactions,
                  },
                ],
              }] : []),
            ],
          },
        ]
      : [],
    backCommand: { onClick: onBack },
    onSearch: () => {
      setFilterVisible((visible) => !visible);
    },
    interactionLocked: refreshing,
    presentation: {
      mode: 'list',
      listInitiallyVisible: !compactViewport,
      listWidth: 232,
      listMinWidth: 200,
      listMaxWidth: 360,
      detailEndPadding: 0,
    },
  };
  return (
    <ListDetailsPage
      key={`${kind}:${balance.accountNum}`}
      variant="enterprise"
      title={tr(`${kind}Balance`)}
      config={config}
    />
  );
}

function Summary({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}): React.ReactElement {
  return (
    <Box sx={{ minWidth: 100 }}>
      <Typography sx={{ fontSize: 11, fontWeight: 600, textTransform: 'uppercase', mb: 1 }}>
        {title}
      </Typography>
      {children}
    </Box>
  );
}

function Field({
  label,
  value,
  narrow,
  width,
  numeric,
}: {
  label: string;
  value: string;
  narrow?: boolean;
  width?: number;
  numeric?: boolean;
}): React.ReactElement {
  return (
    <Box sx={{ width: width ?? (narrow ? 100 : 210), mb: 1, maxWidth: '100%' }}>
      <Typography sx={{ fontSize: 11, mb: 0.2 }}>{label}</Typography>
      <Box
        component="output"
        aria-label={label}
        sx={{ display: 'block', color: numeric ? 'primary.main' : 'text.primary' }}
      >
        <EditableViewField label={label} value={value || '—'} numeric={numeric} />
      </Box>
    </Box>
  );
}
