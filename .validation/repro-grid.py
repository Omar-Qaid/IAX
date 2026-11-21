from pathlib import Path
p=Path('IXApp/e2e/sales-order-grid.pw.ts');s=p.read_text();needle="      else if (path.endsWith('/units'))";s=s.replace(needle,"""      else if (path.endsWith('/1/details'))
        data = { recId: 1, salesId: 'SO-GRID-001', customerAccount: 'C-001', customerName: 'Grid test customer', invoiceAccount: 'C-001', currencyCode: 'SAR', customerGroup: 'RETAIL', salesStatus: 'Backorder', documentStatus: 'None', deliveryDate: '2026-09-09', paymentTerms: 'Net 30', deliveryMode: 'Road', orderTotal: 15000 };
"""+needle,1)
s=s.replace("    await expect(grid.locator('[data-row-id=\"1\"]')).toBeVisible();", "    await expect(grid.locator('[data-row-id=\"1\"]')).toBeVisible();\n    await expect(grid.locator('a')).toHaveCount(0);",1)
with p.open('r+',newline='') as f:f.write(s);f.truncate()
