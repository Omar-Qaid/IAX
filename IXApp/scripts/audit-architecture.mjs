import { existsSync, readFileSync, readdirSync } from 'node:fs';
import { dirname, extname, join, relative, resolve, sep } from 'node:path';

const projectRoot = resolve(import.meta.dirname, '..');
const sourceRoot = join(projectRoot, 'src');
const sourceExtensions = new Set(['.ts', '.tsx']);
const aliases = new Map([
  ['@app', 'app'],
  ['@core', 'core'],
  ['@shared', 'shared'],
  ['@patterns', 'patterns'],
  ['@modules', 'modules'],
  ['@mocks', 'mocks'],
  ['@test', 'test'],
]);

const allowedLayers = {
  app: new Set(['app', 'modules', 'patterns', 'shared', 'core', 'mocks']),
  modules: new Set(['modules', 'patterns', 'shared', 'core', 'mocks']),
  patterns: new Set(['patterns', 'shared', 'core']),
  shared: new Set(['shared', 'core']),
  core: new Set(['core']),
  mocks: new Set(['mocks', 'core', 'shared']),
  test: new Set(['test', 'app', 'core', 'shared', 'patterns', 'modules', 'mocks']),
};

// Existing debt is explicit so the gate blocks regressions while later phases remove it.
const layerDebtBaseline = new Set([]);

const iconBarrelBaseline = new Set([
  'app/navigation/NavItem.tsx',
  'app/navigation/NavSection.tsx',
  'app/shell/AppSidebar.tsx',
  'app/shell/AppTopBar.tsx',
  'app/shell/ModuleNavPanel.tsx',
  'shared/components/data-grid/DataGridMobileBody.tsx',
  'shared/components/data-grid/GridSidebar.tsx',
  'shared/components/data-grid/body/GridRow.tsx',
  'shared/components/data-grid/body/RowContextMenu.tsx',
  'shared/components/data-grid/header/FilterInput.tsx',
  'shared/components/data-grid/header/FilterPopover.tsx',
  'shared/components/data-grid/header/HeaderMenu.tsx',
  'shared/components/data-grid/header/PinnedHeaderCell.tsx',
  'shared/components/data-grid/header/SortableHeader.tsx',
  'shared/components/data-grid/sidebar/ColumnsPanel.tsx',
  'shared/components/data-grid/sidebar/FeaturesPanel.tsx',
  'shared/components/data-grid/sidebar/FiltersPanel.tsx',
  'shared/components/dialogs/AppHistoryDrawer.tsx',
]);

const walk = (directory) =>
  readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const path = join(directory, entry.name);
    return entry.isDirectory()
      ? walk(path)
      : sourceExtensions.has(extname(entry.name))
        ? [path]
        : [];
  });

const files = walk(sourceRoot);
const normalizedRelative = (path) => relative(sourceRoot, path).split(sep).join('/');
const layerOfFile = (path) => normalizedRelative(path).split('/')[0];
const moduleOfFile = (path) => {
  const segments = normalizedRelative(path).split('/');
  return segments[0] === 'modules' && segments.length > 2 ? segments[1] : null;
};

const aliasPath = (specifier) => {
  for (const [alias, layer] of aliases) {
    if (specifier === alias) return join(sourceRoot, layer);
    if (specifier.startsWith(`${alias}/`)) {
      return join(sourceRoot, layer, specifier.slice(alias.length + 1));
    }
  }
  return null;
};

const resolveSourceFile = (file, specifier) => {
  const base =
    aliasPath(specifier) ?? (specifier.startsWith('.') ? resolve(dirname(file), specifier) : null);
  if (!base) return null;
  const candidates = [
    base,
    `${base}.ts`,
    `${base}.tsx`,
    join(base, 'index.ts'),
    join(base, 'index.tsx'),
  ];
  return (
    candidates.find(
      (candidate) => existsSync(candidate) && sourceExtensions.has(extname(candidate))
    ) ?? null
  );
};

const isUnresolvedInternalImport = (file, specifier) => {
  const base =
    aliasPath(specifier) ?? (specifier.startsWith('.') ? resolve(dirname(file), specifier) : null);
  if (!base) return false;
  const relativePath = relative(sourceRoot, base);
  if (relativePath.startsWith('..')) return false;
  const candidates = [
    base,
    `${base}.ts`,
    `${base}.tsx`,
    `${base}.css`,
    `${base}.json`,
    join(base, 'index.ts'),
    join(base, 'index.tsx'),
  ];
  return !candidates.some(existsSync);
};

const importPattern = /(?:from\s*|import\s*\(\s*|import\s*)["']([^"']+)["']/g;
const graph = new Map(files.map((file) => [file, new Set()]));
const layerViolations = [];
const crossModuleViolations = [];
const iconBarrelImports = [];
const unresolvedInternalImports = [];

for (const file of files) {
  const sourceLayer = layerOfFile(file);
  const sourceModule = moduleOfFile(file);
  const allowed = allowedLayers[sourceLayer];
  const content = readFileSync(file, 'utf8');

  for (const match of content.matchAll(importPattern)) {
    const specifier = match[1];
    const targetFile = resolveSourceFile(file, specifier);
    if (!targetFile && isUnresolvedInternalImport(file, specifier)) {
      unresolvedInternalImports.push(`${normalizedRelative(file)} -> ${specifier}`);
    }
    if (targetFile) graph.get(file).add(targetFile);

    if (specifier === '@mui/icons-material') {
      iconBarrelImports.push(normalizedRelative(file));
    }

    if (!targetFile || !allowed) continue;
    const targetLayer = layerOfFile(targetFile);
    if (!allowed.has(targetLayer)) {
      layerViolations.push(`${normalizedRelative(file)} -> ${targetLayer}`);
    }

    const targetModule = moduleOfFile(targetFile);
    if (sourceModule && targetModule && sourceModule !== targetModule) {
      crossModuleViolations.push(
        `${normalizedRelative(file)} -> ${normalizedRelative(targetFile)}`
      );
    }
  }
}

const findCycles = () => {
  const state = new Map();
  const stack = [];
  const cycles = new Set();

  const visit = (file) => {
    state.set(file, 'visiting');
    stack.push(file);
    for (const dependency of graph.get(file) ?? []) {
      if (!graph.has(dependency)) continue;
      if (!state.has(dependency)) visit(dependency);
      else if (state.get(dependency) === 'visiting') {
        const start = stack.indexOf(dependency);
        const cycle = [...stack.slice(start), dependency].map(normalizedRelative);
        const nodes = cycle.slice(0, -1);
        const canonicalStart = nodes.reduce(
          (best, value, index) => (value < nodes[best] ? index : best),
          0
        );
        const canonical = [...nodes.slice(canonicalStart), ...nodes.slice(0, canonicalStart)];
        cycles.add([...canonical, canonical[0]].join(' -> '));
      }
    }
    stack.pop();
    state.set(file, 'visited');
  };

  files.forEach((file) => {
    if (!state.has(file)) visit(file);
  });
  return [...cycles].sort();
};

const uniqueLayerViolations = [...new Set(layerViolations)].sort();
const newLayerViolations = uniqueLayerViolations.filter(
  (violation) => !layerDebtBaseline.has(violation)
);
const resolvedLayerDebt = [...layerDebtBaseline].filter(
  (violation) => !uniqueLayerViolations.includes(violation)
);
const uniqueCrossModuleViolations = [...new Set(crossModuleViolations)].sort();
const uniqueIconBarrels = [...new Set(iconBarrelImports)].sort();
const newIconBarrels = uniqueIconBarrels.filter((file) => !iconBarrelBaseline.has(file));
const resolvedIconDebt = [...iconBarrelBaseline].filter(
  (file) => !uniqueIconBarrels.includes(file)
);
const cycles = findCycles();
const uniqueUnresolvedImports = [...new Set(unresolvedInternalImports)].sort();

console.log(`Known layer debt: ${uniqueLayerViolations.length} edge(s).`);
console.log(`Known icon-barrel debt: ${uniqueIconBarrels.length} file(s).`);
if (resolvedLayerDebt.length) {
  console.log(`Resolved layer baseline entries ready for removal: ${resolvedLayerDebt.length}.`);
}
if (resolvedIconDebt.length) {
  console.log(`Resolved icon baseline entries ready for removal: ${resolvedIconDebt.length}.`);
}

if (newLayerViolations.length) {
  console.error('New forbidden dependency edges:');
  newLayerViolations.forEach((violation) => console.error(`- ${violation}`));
}
if (uniqueCrossModuleViolations.length) {
  console.error('Cross-module imports are forbidden:');
  uniqueCrossModuleViolations.forEach((violation) => console.error(`- ${violation}`));
}
if (newIconBarrels.length) {
  console.error('New MUI icon barrel imports are forbidden:');
  newIconBarrels.forEach((file) => console.error(`- ${file}`));
}
if (cycles.length) {
  console.error('Circular source dependencies detected:');
  cycles.forEach((cycle) => console.error(`- ${cycle}`));
}
if (uniqueUnresolvedImports.length) {
  console.error('Unresolved internal imports detected:');
  uniqueUnresolvedImports.forEach((violation) => console.error(`- ${violation}`));
}

if (
  newLayerViolations.length ||
  uniqueCrossModuleViolations.length ||
  newIconBarrels.length ||
  cycles.length ||
  uniqueUnresolvedImports.length
) {
  process.exitCode = 1;
} else {
  console.log('Architecture regression check passed.');
}
