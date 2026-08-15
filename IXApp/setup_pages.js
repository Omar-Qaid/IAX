const fs = require('fs');
const path = require('path');

const pages = [
  { path: 'auth/pages/LoginPage.tsx', name: 'LoginPage', isDefault: true },
  { path: 'dashboard/pages/DashboardPage.tsx', name: 'DashboardPage', isDefault: false },
  { path: 'accounts-receivable/customers/pages/CustomersPage.tsx', name: 'CustomersPage', isDefault: false },
  { path: 'accounts-receivable/customer-groups/pages/CustomerGroupsPage.tsx', name: 'CustomerGroupsPage', isDefault: false },
  { path: 'accounts-receivable/sales-orders/pages/SalesOrdersPage.tsx', name: 'SalesOrdersPage', isDefault: false },
  { path: 'accounts-receivable/sales-orders/pages/SalesOrderPage.tsx', name: 'SalesOrderPage', isDefault: false },
  { path: 'foundation/currencies/pages/CurrenciesPage.tsx', name: 'CurrenciesPage', isDefault: false },
  { path: 'system-administration/settings/pages/ApplicationSettingsPage.tsx', name: 'ApplicationSettingsPage', isDefault: false }
];

pages.forEach(p => {
  const fullPath = path.join(__dirname, 'src/modules', p.path);
  fs.mkdirSync(path.dirname(fullPath), { recursive: true });
  
  let content = `import React from 'react';\n\n`;
  if (p.isDefault) {
    content += `const ${p.name}: React.FC = () => {\n  return (\n    <div>\n      <h1>${p.name}</h1>\n    </div>\n  );\n};\n\nexport default ${p.name};\n`;
  } else {
    content += `export const ${p.name}: React.FC = () => {\n  return (\n    <div>\n      <h1>${p.name}</h1>\n    </div>\n  );\n};\n`;
  }
  
  if (!fs.existsSync(fullPath)) {
    fs.writeFileSync(fullPath, content);
    console.log(`Created ${fullPath}`);
  }
});
