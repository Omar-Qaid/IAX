from pathlib import Path
root=Path('IXApp/src/modules/finance/accounts-receivable')
for file,names in [('customer/components/CustomerQuickCreate.tsx',['country']),('customer/pages/CustTablePage.tsx',['countryRegionId','salesPoolId']),('sales-order/components/SalesOrderQuickCreate.tsx',['addressCountryRegionId','addressState','addressCity','addressZipCode'])]:
 p=root/file;s=p.read_text()
 for name in names:s=s.replace("name: '"+name+"',","name: '"+name+"', previewEntity: 'referenceOption',")
 with p.open('r+',newline='') as f:f.write(s);f.truncate()
p=root/'payment/components/CustPaymentJournalDialog.tsx';s=p.read_text().replace("import React,", "import { RecordPreview } from '@shared/components/record-preview';\nimport React,",1);s=s.replace('<FormControl size="small" sx={{ minWidth: 260 }}>','''<RecordPreview entity="referenceOption" recordKey={bankAccountId}
            option={{ code: bankAccountId, name: bankQuery.data?.find((bank) => bank.accountId === bankAccountId)?.name }}>
          <FormControl size="small" sx={{ minWidth: 260 }}>''',1);s=s.replace('</FormControl>','</FormControl>\n          </RecordPreview>',1)
with p.open('r+',newline='') as f:f.write(s);f.truncate()
