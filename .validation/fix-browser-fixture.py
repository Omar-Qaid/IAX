from pathlib import Path
p=Path('IXApp/e2e/sales-order-grid.pw.ts');s=p.read_text();s=s.replace("/SalesTable/1'","/SalesTable/1/details'")
with p.open('r+',newline='') as f:f.write(s);f.truncate()
