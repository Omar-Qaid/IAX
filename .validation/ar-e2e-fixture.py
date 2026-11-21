from pathlib import Path
p=Path('IXApp/e2e/sales-order-grid.pw.ts');s=p.read_text();needle="      else if (path.endsWith('/units'))";s=s.replace(needle,"""      else if (path.endsWith('/SalesTable/1'))
        data = { recId: 1, salesId: 'SO-GRID-001', customerAccount: 'C-001', customerName: 'Grid test customer', invoiceAccount: 'C-001', currencyCode: 'SAR', customerGroup: 'RETAIL', salesStatus: 'Backorder', documentStatus: 'None', deliveryDate: '2026-09-09', paymentTerms: 'Net 30', deliveryMode: 'Road', orderTotal: 15000 };
"""+needle,1)
with p.open('r+',newline='') as f:f.write(s);f.truncate()
