import React from 'react';
import FunctionsIcon from '@mui/icons-material/Functions';
import { useQuery } from '@tanstack/react-query';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsTreePage } from '@patterns/list-details-tree';
import type { ListDetailsTreePageConfig } from '@patterns/list-details-tree';
import type { DetailValue, DetailValues } from '@patterns/list-details/types';
import { mainAccountApi, type MainAccountRecord } from '../api/mainAccountApi';

const text = (value: DetailValue | undefined) => String(value ?? '');
const int = (value: DetailValue | undefined) => Number(value) || 0;
/** The server models a flag as the NoYes enum, where 1 is yes. */
const noYes = (value: DetailValue | undefined) => (value ? 1 : 0);
/** The form edits a date as yyyy-MM-dd; an empty value clears the limit. */
const day = (value: string | null | undefined) => value?.slice(0, 10) ?? '';
const dayOrNull = (value: DetailValue | undefined) => {
  const date = text(value).trim();
  return date ? date : null;
};

export function MainAccountDetails(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return <MainAccountDetailsContent key={company} company={company} />;
}

function MainAccountDetailsContent({ company }: { company: string }): React.ReactElement {
  const { t } = useAppTranslation();
  const label = (key: string) => t(`mainAccount.${key}`);
  const [selected, setSelected] = React.useState<MainAccountRecord | null>(null);
  // The form is titled by the chart of accounts the selected account belongs to.
  const charts = useQuery({
    queryKey: ['ledger-chart-of-accounts', company],
    queryFn: ({ signal }) => mainAccountApi.charts(signal),
  });
  const chart =
    (charts.data ?? []).find((row) => row.recId === selected?.ledgerChartOfAccounts) ??
    (charts.data ?? [])[0];
  const title = chart ? `${label('title')}: ${chart.name}` : label('title');

  const options = (prefix: string, count: number, from = 0) =>
    Array.from({ length: count }, (_, index) => ({
      value: String(index + from),
      label: label(`${prefix}.${index + from}`),
    }));
  /** FieldControl: Optional, Fill in, Table, List. */
  const fieldControlOptions = options('fieldControl', 4);
  const debitCreditOptions = options('debitCredit', 3);

  const config: ListDetailsTreePageConfig<MainAccountRecord> = {
    recordTableName: 'MainAccount',
    filterStorageKey: 'main-accounts',
    dataSource: {
      type: 'remote',
      key: `main-accounts-${company}`,
      load: (signal) => mainAccountApi.list(signal),
      create: (record) => mainAccountApi.create(record),
      update: async (record) => mainAccountApi.update(record.id, record),
      delete: (record) => mainAccountApi.remove(record.id),
    },
    createRecord: (): MainAccountRecord => ({
      id: `new-${crypto.randomUUID()}`,
      recId: 0,
      mainAccountId: '',
      name: '',
      type: 1,
      currencyCode: '',
      accountCategoryRef: 0,
      ledgerChartOfAccounts: 0,
      offsetLedgerDimension: 0,
      openingAccount: 0,
      postingType: 0,
      debitCreditProposal: 0,
      debitCreditCheck: 0,
      debitCreditBalanceDemand: 0,
      mandatoryPaymentReference: 0,
      validateCurrency: 0,
      validatePosting: 0,
      validateUser: 0,
      userId: '',
      activeFrom: null,
      activeTo: null,
      suspended: 0,
      closeType: 0,
      closing: 0,
      monetary: 0,
      exchangeAdjustmentRateType: 0,
      reportingExchangeAdjustmentRateType: 0,
      reportingAccountType: 0,
      consolidationMainAccount: '',
      groupLevel01: '',
      groupLevel02: '',
      groupLevel03: '',
    }),
    // The list reads "account, name", as the chart of accounts does.
    getPrimaryText: (record) => `${record.mainAccountId}, ${record.name}`,
    getSecondaryText: () => '',
    matchesSearch: (record, query) =>
      `${record.mainAccountId} ${record.name}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      mainAccountId: record.mainAccountId,
      name: record.name,
      type: record.type,
      accountCategoryRef: record.accountCategoryRef,
      debitCreditProposal: record.debitCreditProposal,
      debitCreditCheck: record.debitCreditCheck,
      debitCreditBalanceDemand: record.debitCreditBalanceDemand,
      offsetLedgerDimension: record.offsetLedgerDimension,
      openingAccount: record.openingAccount,
      closing: Boolean(record.closing),
      monetary: Boolean(record.monetary),
      closeType: record.closeType,
      consolidationMainAccount: record.consolidationMainAccount,
      currencyCode: record.currencyCode,
      validateCurrency: record.validateCurrency,
      exchangeAdjustmentRateType: record.exchangeAdjustmentRateType,
      reportingExchangeAdjustmentRateType: record.reportingExchangeAdjustmentRateType,
      reportingAccountType: record.reportingAccountType,
      groupLevel01: record.groupLevel01,
      groupLevel02: record.groupLevel02,
      groupLevel03: record.groupLevel03,
      validateUser: record.validateUser,
      userId: record.userId,
      activeFrom: day(record.activeFrom),
      activeTo: day(record.activeTo),
      suspended: Boolean(record.suspended),
      postingType: record.postingType,
      validatePosting: record.validatePosting,
    }),
    // The reference form carries no header fields: Main account and Name live in General.
    headerFields: [],
    setValues: (record, values): MainAccountRecord => ({
      ...record,
      mainAccountId: text(values.mainAccountId).trim(),
      name: text(values.name).trim(),
      type: int(values.type),
      accountCategoryRef: int(values.accountCategoryRef),
      debitCreditProposal: int(values.debitCreditProposal),
      debitCreditCheck: int(values.debitCreditCheck),
      debitCreditBalanceDemand: int(values.debitCreditBalanceDemand),
      offsetLedgerDimension: int(values.offsetLedgerDimension),
      openingAccount: int(values.openingAccount),
      closing: noYes(values.closing),
      monetary: noYes(values.monetary),
      closeType: int(values.closeType),
      consolidationMainAccount: text(values.consolidationMainAccount).trim(),
      currencyCode: text(values.currencyCode).trim(),
      validateCurrency: int(values.validateCurrency),
      exchangeAdjustmentRateType: int(values.exchangeAdjustmentRateType),
      reportingExchangeAdjustmentRateType: int(values.reportingExchangeAdjustmentRateType),
      reportingAccountType: int(values.reportingAccountType),
      groupLevel01: text(values.groupLevel01).trim(),
      groupLevel02: text(values.groupLevel02).trim(),
      groupLevel03: text(values.groupLevel03).trim(),
      validateUser: int(values.validateUser),
      userId: text(values.userId).trim(),
      activeFrom: dayOrNull(values.activeFrom),
      activeTo: dayOrNull(values.activeTo),
      suspended: noYes(values.suspended),
      postingType: int(values.postingType),
      validatePosting: int(values.validatePosting),
    }),
    validate: (record) => {
      const errors: Record<string, string> = {};
      if (!record.mainAccountId.trim()) errors.mainAccountId = label('validation.accountRequired');
      if (!record.name.trim()) errors.name = label('validation.nameRequired');
      return errors;
    },
    sections: () => [
      {
        id: 'general',
        title: label('sections.general'),
        defaultExpanded: true,
        columns: 6,
        commands: [
          { id: 'totals', label: label('commands.totals'), icon: <FunctionsIcon fontSize="small" /> },
          {
            id: 'assign-template',
            label: label('commands.assignTemplate'),
            items: [
              { id: 'assign', label: label('commands.assignTemplateItem') },
              { id: 'remove', label: label('commands.removeTemplate') },
            ],
          },
          {
            id: 'additional-consolidation-accounts',
            label: label('commands.additionalConsolidationAccounts'),
          },
          { id: 'currency-code-validation', label: label('commands.currencyCodeValidation') },
        ],
        groups: [
          {
            id: 'identification',
            title: label('groups.identification'),
            fields: [
              { name: 'mainAccountId', label: label('fields.mainAccount') },
              { name: 'name', label: label('fields.name') },
              { name: 'blank-identification', label: '', type: 'display', disabled: true },
              {
                name: 'type',
                label: label('fields.mainAccountType'),
                type: 'select',
                options: options('types', 5, 1),
                sectionTitle: label('groups.ledger'),
              },
              {
                name: 'reportingAccountType',
                label: label('fields.reportingType'),
                type: 'number',
              },
              { name: 'accountCategoryRef', label: label('fields.mainAccountCategory'), type: 'number' },
            ],
          },
          {
            id: 'debit-credit',
            fields: [
              {
                name: 'debitCreditProposal',
                label: label('fields.debitCreditDefault'),
                type: 'select',
                options: debitCreditOptions,
              },
              {
                name: 'debitCreditCheck',
                label: label('fields.debitCreditRequirement'),
                type: 'select',
                options: debitCreditOptions,
              },
              {
                name: 'debitCreditBalanceDemand',
                label: label('fields.balanceControl'),
                type: 'select',
                options: debitCreditOptions,
              },
              {
                name: 'offsetLedgerDimension',
                label: label('fields.offsetAccount'),
                type: 'number',
                sectionTitle: label('groups.relatedAccounts'),
              },
              { name: 'openingAccount', label: label('fields.openingAccount'), type: 'number' },
            ],
          },
          {
            id: 'administration',
            title: label('groups.administration'),
            fields: [
              { name: 'activeFrom', label: label('fields.activeFrom'), type: 'date' },
              { name: 'activeTo', label: label('fields.activeTo'), type: 'date' },
              {
                name: 'suspended',
                label: label('fields.suspended'),
                type: 'boolean',
                booleanVariant: 'checkbox',
              },
              {
                name: 'closing',
                label: label('fields.closing'),
                type: 'boolean',
                booleanVariant: 'checkbox',
              },
              {
                name: 'closeType',
                label: label('fields.closeType'),
                type: 'select',
                options: options('closeType', 3),
              },
            ],
          },
          {
            id: 'consolidation',
            title: label('groups.consolidation'),
            fields: [
              {
                name: 'consolidationMainAccount',
                label: label('fields.defaultConsolidationAccount'),
              },
              {
                name: 'currencyCode',
                label: label('fields.defaultCurrency'),
                sectionTitle: label('groups.currency'),
              },
              {
                name: 'validateCurrency',
                label: label('fields.validateCurrencyCode'),
                type: 'select',
                options: fieldControlOptions,
              },
              {
                // D365 drives foreign currency revaluation from MainAccount.Monetary.
                name: 'monetary',
                label: label('fields.foreignCurrencyRevaluation'),
                type: 'boolean',
              },
            ],
          },
          {
            id: 'exchange-rates',
            fields: [
              {
                name: 'exchangeAdjustmentRateType',
                label: label('fields.exchangeRateType'),
                type: 'number',
              },
              {
                name: 'reportingExchangeAdjustmentRateType',
                label: label('fields.reportingCurrencyExchangeRateType'),
                type: 'number',
              },
              {
                name: 'groupLevel01',
                label: label('fields.level1'),
                sectionTitle: label('groups.reporting'),
              },
              { name: 'groupLevel02', label: label('fields.level2') },
              { name: 'groupLevel03', label: label('fields.level3') },
            ],
          },
        ],
      },
      {
        id: 'legal-entity-overrides',
        title: label('sections.legalEntityOverrides'),
        content: <>{label('hints.legalEntityOverrides')}</>,
      },
      {
        id: 'posting-validation',
        title: label('sections.postingValidation'),
        defaultExpanded: true,
        columns: 2,
        commands: [
          { id: 'user-validation', label: label('commands.userValidation') },
          { id: 'posting-validation', label: label('commands.postingValidation') },
        ],
        groups: [
          {
            id: 'user',
            title: label('groups.user'),
            fields: [
              { name: 'userId', label: label('fields.userId') },
              {
                name: 'validateUser',
                label: label('fields.validateUser'),
                type: 'select',
                options: fieldControlOptions,
              },
            ],
          },
          {
            id: 'posting-type',
            title: label('groups.postingType'),
            fields: [
              { name: 'postingType', label: label('fields.postingType'), type: 'number' },
              {
                name: 'validatePosting',
                label: label('fields.postingTypeValidation'),
                type: 'select',
                options: fieldControlOptions,
              },
            ],
          },
        ],
      },
      {
        id: 'financial-reporting',
        title: label('sections.financialReporting'),
        content: <>{label('hints.financialReporting')}</>,
      },
      {
        id: 'advanced-rules',
        title: label('sections.advancedRules'),
        content: <>{label('hints.advancedRules')}</>,
      },
    ],
    onSelectionChange: setSelected,
    commands: [
      {
        id: 'new-from-template',
        menuLabel: label('commands.newFromTemplate'),
        label: label('commands.newFromTemplateItem'),
      },
      {
        id: 'remove-template',
        menuLabel: label('commands.newFromTemplate'),
        label: label('commands.removeTemplate'),
      },
      { id: 'transactions', label: label('commands.transactions'), requiresSelection: true },
      { id: 'accounting-source-explorer', label: label('commands.accountingSourceExplorer') },
      { id: 'name-translations', label: label('commands.nameTranslations') },
      { id: 'account-statement', label: label('commands.accountStatement') },
      { id: 'total-accounts', label: label('commands.totalAccounts') },
    ],
    // Main accounts are a flat chart, so every node is a root.
    tree: {
      getParentId: () => null,
      getLabel: (record) => `${record.mainAccountId}, ${record.name}`,
      compare: (left, right) => left.mainAccountId.localeCompare(right.mainAccountId),
      ariaLabel: label('title'),
    },
    listPager: {},
    presentation: {
      mode: 'list',
      listWidth: 330,
      listMinWidth: 240,
      listMaxWidth: 520,
      listWidthStorageKey: 'main-accounts-tree',
    },
    advancedFilter: {
      fieldLabel: label('fields.name'),
      getValue: (record) => record.name,
      matches: (record, value) =>
        `${record.mainAccountId} ${record.name}`
          .toLocaleLowerCase()
          .includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsTreePage title={title} config={config} />;
}
