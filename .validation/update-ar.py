from pathlib import Path
base=Path('IXApp/src/modules/finance/accounts-receivable')
def edit(file, fn):
 p=base/file
 s=p.read_text(encoding='utf-8'); n=fn(s)
 if s!=n:
  with p.open('r+',encoding='utf-8',newline='') as f:f.write(n);f.truncate()
def field(s,name,prop):
 return s.replace("name: '"+name+"',", "name: '"+name+"', "+prop)
edit('customer/pages/CustTablePage.tsx', lambda s: field(s,'dlvModeId',"previewEntity: 'dlvMode',"))
edit('customer/components/CustomerQuickCreate.tsx',lambda s:field(s,'deliveryMode',"previewEntity: 'dlvMode',"))
edit('sales-order/components/SalesOrderQuickCreate.tsx',lambda s:field(field(s,'deliveryMode',"previewEntity: 'dlvMode',"),'deliveryTerms',"previewEntity: 'dlvTerm',"))
edit('setup/pages/CustParametersPage.tsx', lambda s: field(s,'oneTimeCustomerAccount',"masterRoute: '/accounts-receivable/customers', previewEntity: 'customer',"))
edit('price-discount/components/PriceDiscSetupListPage.tsx',lambda s:s.replace("field: 'journalName',", "field: 'journalName', preview: { entity: 'tradeAgreementJournalName' },",1))
edit('free-text-invoice/lines/useFreeTextInvoiceLineColumns.tsx',lambda s:s.replace("field: 'ledgerDimension',", "field: 'ledgerDimension', preview: { entity: 'customerLedgerDimension' },").replace('name="ledgerDimension"','name="ledgerDimension" previewEntity="customerLedgerDimension"').replace('name={field}',"name={field} previewEntity={field === 'taxGroup' ? FOUNDATION_PREVIEWS.SALES_TAX_GROUP : FOUNDATION_PREVIEWS.ITEM_SALES_TAX_GROUP}"))
edit('free-text-invoice/lines/FreeTextInvoiceLineFields.tsx',lambda s:s.replace('previewEntity={FOUNDATION_PREVIEWS.SALES_TAX_GROUP}', 'masterRoute="/tax/tax-groups" previewEntity={FOUNDATION_PREVIEWS.SALES_TAX_GROUP}').replace('previewEntity={FOUNDATION_PREVIEWS.ITEM_SALES_TAX_GROUP}', 'masterRoute="/tax/tax-item-groups" previewEntity={FOUNDATION_PREVIEWS.ITEM_SALES_TAX_GROUP}').replace('value={ledgerOf(line)}','value={ledgerOf(line)} masterRoute="/general-ledger/main-accounts" previewEntity="customerLedgerDimension" previewKey={line.ledgerDimension || null}'))
