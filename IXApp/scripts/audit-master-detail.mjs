import { existsSync, readdirSync, readFileSync } from 'node:fs';
import { dirname, join, relative, resolve, sep } from 'node:path';
import { pathToFileURL } from 'node:url';
import { masterDetailFeatureName, masterDetailStructure } from './master-detail-structure.mjs';

// Existing complex documents keep their feature-owned workflow helpers.
const existingFeatures = new Map([
  ['finance/accounts-receivable/sales-order', { name: 'SalesOrder', hasLines: true }],
  ['finance/accounts-receivable/free-text-invoice', { name: 'FreeTextInvoice', hasLines: true }],
  ['finance/foundation/contacts', { name: 'ContactPerson', hasLines: false }],
]);

export function auditMasterDetail(projectRoot = resolve(import.meta.dirname, '..')) {
  const modules = join(projectRoot, 'src/modules');
  const walk = (directory) =>
    readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
      const path = join(directory, entry.name);
      return entry.isDirectory() ? walk(path) : [path];
    });
  const files = walk(modules);
  const compositions = files.filter(
    (file) =>
      file.endsWith('.tsx') && /<MasterDetailPage(?:\s|>|<)/.test(readFileSync(file, 'utf8'))
  );
  const problems = [];
  for (const [path, { name, hasLines }] of existingFeatures) {
    const feature = join(modules, path);
    if (!existsSync(feature)) continue;
    const camel = name[0].toLowerCase() + name.slice(1);
    const roles = [
      `details/${name}Details.tsx`,
      `details/use${name}Document.ts`,
      `details/${name}HeaderFields.ts`,
      `details/${name}HeaderSections.ts`,
      ...(hasLines
        ? [
            `details/${camel}Totals.tsx`,
            `details/${name}DetailsDialogs.tsx`,
            `lines/${name}LinesGrid.tsx`,
            `lines/${name}LineFields.tsx`,
            `lines/${name}LineTabs.tsx`,
            `lines/use${name}LineColumns.ts`,
          ]
        : []),
    ];
    for (const role of roles) {
      if (
        !existsSync(join(feature, role)) &&
        !(role.endsWith('.ts') && existsSync(join(feature, `${role}x`)))
      ) {
        problems.push(`${path}: missing common details/lines role ${role}.`);
      }
    }
    for (const file of files.filter((file) => dirname(file) === join(feature, 'details'))) {
      if (/(LineFields|LineTabs|LinesGrid|LineColumns)\.tsx?$/.test(file))
        problems.push(`${relative(modules, file)}: line code belongs in lines/.`);
    }
  }
  const standardFeatures = new Map();
  for (const file of files.filter((path) => path.endsWith(`${sep}README.md`))) {
    const name = masterDetailFeatureName(readFileSync(file, 'utf8'));
    if (name) standardFeatures.set(dirname(file), name);
  }
  for (const file of compositions) {
    const feature = dirname(dirname(file));
    const label = relative(modules, file);
    if (dirname(file) !== join(feature, 'details'))
      problems.push(`${label}: put the composition in details/.`);
    const profile = existingFeatures.get(relative(modules, feature).split(sep).join('/'));
    const directories = [
      'pages',
      'details',
      'api',
      ...(profile?.hasLines === false ? [] : ['lines']),
    ];
    for (const directory of directories) {
      if (!existsSync(join(feature, directory))) problems.push(`${label}: missing ${directory}/.`);
    }
    const details = files.filter((path) => dirname(path) === join(feature, 'details'));
    if (!details.some((path) => /use[A-Z].*Document\.tsx?$/.test(path)))
      problems.push(`${label}: missing details/use<Feature>Document.ts.`);
    if (!details.some((path) => /HeaderSections\.tsx?$/.test(path)))
      problems.push(`${label}: missing details/<Feature>HeaderSections.ts[x].`);
    if (
      !existingFeatures.has(relative(modules, feature).split(sep).join('/')) &&
      !standardFeatures.has(feature)
    ) {
      const inferred = /([A-Z][A-Za-z0-9]*)Details\.tsx$/.exec(file)?.[1];
      problems.push(
        `${label}: generate the feature using generate:master-detail and keep its README structure marker.`
      );
      if (inferred) standardFeatures.set(feature, inferred);
    }
  }
  for (const [feature, name] of standardFeatures) {
    for (const [role, path] of Object.entries(masterDetailStructure(name))) {
      const exact = join(feature, path);
      // Header definitions and columns may contain feature-specific JSX editors.
      const jsx = ['header', 'columns'].includes(role) ? `${exact}x` : null;
      if (!existsSync(exact) && !(jsx && existsSync(jsx)))
        problems.push(`${relative(modules, feature)}: missing standard ${role} file ${path}.`);
    }
  }
  return { problems, compositions: compositions.length, standardFeatures: standardFeatures.size };
}

if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  const result = auditMasterDetail();
  if (result.problems.length) {
    console.error(result.problems.join('\n'));
    process.exitCode = 1;
  } else {
    console.log(
      `Master-details structure passed for ${result.compositions} feature compositions (${result.standardFeatures} generated features).`
    );
  }
}
