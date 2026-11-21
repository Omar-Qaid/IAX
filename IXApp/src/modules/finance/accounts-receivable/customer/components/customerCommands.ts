import type { NavigateFunction } from 'react-router-dom';
import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';
import { PERMISSIONS } from '@core/permissions/permissions';
import { accountContacts } from '@modules/finance/foundation/contacts/routes/contactRoutes';
import { documentTableIds } from '@shared/components/documents/recordTableIds';
import { PARTY_BALANCE_ROUTE_PATHS } from '@modules/finance/party-balances/routes/partyBalanceRoutePaths';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import type { CustomerRecord } from '../api/customerQuickCreateApi';
import { buildCustomerRibbonGroups } from './customerRibbon';
import { buildCustomerSellRibbonGroups } from './customerSellRibbon';
import { buildCustomerInvoiceRibbonGroups } from './customerInvoiceRibbon';
import { buildCustomerCollectRibbonGroups } from './customerCollectRibbon';

interface CustomerCommand {
  id: string;
  label: string;
  requiresSelection: boolean;
  disabled?: boolean;
  ribbonPersistenceKey: string;
  groups: ActionPaneRibbonGroup[];
}

/**
 * Customer action pane tabs (Customer, Sell, Invoice, Collect), shared by the customer
 * list and the customer details page. Optional actions are hidden when not passed.
 */
export function buildCustomerCommands(
  record: CustomerRecord | null,
  {
    t,
    hasPermission,
    navigate,
    onContacts,
    onNewSalesOrder,
    onReturnOrder,
    onSettleTransactions,
    onPaymentJournal,
  }: {
    t: (key: string, fallback?: string) => string;
    hasPermission: (permission: string) => boolean;
    navigate: NavigateFunction;
    onContacts: (record: CustomerRecord | null) => void;
    onNewSalesOrder: (record: CustomerRecord) => void;
    onReturnOrder?: (record: CustomerRecord) => void;
    onSettleTransactions?: (record: CustomerRecord) => void;
    onPaymentJournal?: (record: CustomerRecord) => void;
  }
): CustomerCommand[] {
  const commands: CustomerCommand[] = [
    {
      id: 'customer',
      label: t('customerCommands.customer'),
      requiresSelection: true,
      ribbonPersistenceKey: 'accounts-receivable.customers.customer-ribbon-pinned',
      groups: buildCustomerRibbonGroups((key, fallback) => t(key, fallback), {
        saved: Boolean(record && record.recId > 0),
        hasParty: Boolean(record?.party),
        canReadBalances: hasPermission(PERMISSIONS.CUSTOMER_TRANSACTION_VIEW),
        onContacts: () => onContacts(record),
        onViewContacts: hasPermission(PERMISSIONS.CONTACT_VIEW)
          ? () => record && navigate(accountContacts('customer', record.accountNumber))
          : undefined,
        onNewContact:
          hasPermission(PERMISSIONS.CONTACT_VIEW) && hasPermission(PERMISSIONS.CONTACT_CREATE)
            ? () => record && navigate(accountContacts('customer', record.accountNumber, 'new'))
            : undefined,
        onTransactions: () => {
          if (record) navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.custTrans(record.accountNumber));
        },
        onOpenTransactions: () => {
          if (record && record.recId > 0)
            navigate(
              ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customerOpenTransactions(record.accountNumber)
            );
        },
        onBalance: () => {
          if (record && record.recId > 0)
            navigate(PARTY_BALANCE_ROUTE_PATHS.customerBalance(record.accountNumber));
        },
        onAttachments: () => {
          if (record && record.recId > 0)
            navigate(
              '/documents/docu-view?refTableId=' +
                documentTableIds.custTable +
                '&refRecId=' +
                record.recId
            );
        },
      }),
    },
    ...(() => {
      const saved = Boolean(record && record.recId > 0);
      const groups = buildCustomerSellRibbonGroups((key, fallback) => t(key, fallback), {
        onNewSalesOrder: hasPermission(PERMISSIONS.SALES_ORDER_CREATE)
          ? () => {
              if (saved && record) onNewSalesOrder(record);
            }
          : undefined,
        onReturnOrder:
          onReturnOrder && hasPermission(PERMISSIONS.RETURN_ORDER_CREATE)
            ? () => {
                if (saved && record) onReturnOrder?.(record);
              }
            : undefined,
        onSalesOrders: hasPermission(PERMISSIONS.SALES_ORDER_VIEW)
          ? () => {
              if (saved && record)
                navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customerSalesOrders(record.accountNumber));
            }
          : undefined,
        onOrderLines: hasPermission(PERMISSIONS.SALES_ORDER_VIEW)
          ? () => {
              if (saved && record)
                navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.custSalesLines(record.accountNumber));
            }
          : undefined,
        onPackingSlips: hasPermission(PERMISSIONS.SALES_ORDER_VIEW)
          ? () => {
              if (saved && record)
                navigate(
                  ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customerPackingSlips(record.accountNumber)
                );
            }
          : undefined,
        onTradeAgreementJournals: hasPermission(PERMISSIONS.TRADE_AGREEMENT_JOURNAL_VIEW)
          ? () => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.TRADE_AGREEMENT_JOURNALS)
          : undefined,
        onPriceDiscountGroups: hasPermission(PERMISSIONS.PRICE_DISCOUNT_GROUP_VIEW)
          ? () => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.PRICE_DISCOUNT_GROUPS)
          : undefined,
        onJournalNames: hasPermission(PERMISSIONS.TRADE_AGREEMENT_JOURNAL_NAME_VIEW)
          ? () => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.TRADE_AGREEMENT_JOURNAL_NAMES)
          : undefined,
      });
      return groups.length
        ? [
            {
              id: 'sell',
              label: t('customerCommands.sell'),
              requiresSelection: true,
              disabled: !saved,
              ribbonPersistenceKey: 'accounts-receivable.customers.sell-ribbon-pinned',
              groups,
            },
          ]
        : [];
    })(),
    ...(hasPermission(PERMISSIONS.FREE_TEXT_INVOICE_VIEW)
      ? [
          {
            id: 'invoice',
            label: t('customerCommands.invoice'),
            requiresSelection: true,
            disabled: !record || record.recId <= 0,
            ribbonPersistenceKey: 'accounts-receivable.customers.invoice-ribbon-pinned',
            groups: buildCustomerInvoiceRibbonGroups((key, fallback) => t(key, fallback), {
              onNewFreeTextInvoice: hasPermission(PERMISSIONS.FREE_TEXT_INVOICE_CREATE)
                ? () => {
                    if (record && record.recId > 0)
                      navigate(
                        ACCOUNTS_RECEIVABLE_ROUTE_PATHS.newFreeTextInvoice(record.accountNumber)
                      );
                  }
                : undefined,
              onFreeTextInvoices: () => {
                if (record)
                  navigate(
                    ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customerFreeTextInvoices(record.accountNumber)
                  );
              },
              onInvoiceJournal: hasPermission(PERMISSIONS.SALES_ORDER_VIEW)
                ? () => {
                    if (record && record.recId > 0)
                      navigate(
                        ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customerInvoiceJournal(record.accountNumber)
                      );
                  }
                : undefined,
            }),
          },
        ]
      : []),
    {
      id: 'collect',
      label: t('customerCollectRibbon.title', 'Collect'),
      requiresSelection: true,
      disabled: !record || record.recId <= 0,
      ribbonPersistenceKey: 'accounts-receivable.customers.collect-ribbon-pinned',
      groups: buildCustomerCollectRibbonGroups((key, fallback) => t(key, fallback), {
        onSettleTransactions:
          onSettleTransactions && hasPermission(PERMISSIONS.CUSTOMER_PAYMENT_VIEW)
            ? () => {
                if (record && record.recId > 0) onSettleTransactions?.(record);
              }
            : undefined,
        onPaymentJournal:
          onPaymentJournal && hasPermission(PERMISSIONS.CUSTOMER_PAYMENT_POST)
            ? () => {
                if (record && record.recId > 0) onPaymentJournal?.(record);
              }
            : undefined,
      }),
    },
  ];
  return commands;
}
