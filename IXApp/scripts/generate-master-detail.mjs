import { existsSync, mkdirSync, realpathSync, writeFileSync } from 'node:fs';
import { dirname, isAbsolute, relative, resolve, sep } from 'node:path';
import { pathToFileURL } from 'node:url';

export function masterDetailFiles(name) {
  if (!/^[A-Z][A-Za-z0-9]*$/.test(name)) throw new Error('--name must be a PascalCase identifier.');
  const camel = name[0].toLowerCase() + name.slice(1);
  return {
    [`pages/${name}DetailsPage.tsx`]: `import React from 'react';
import { ${name}Details, type ${name}DetailsProps } from '../details/${name}Details';

export function ${name}DetailsPage(props: ${name}DetailsProps): React.ReactElement {
  return <${name}Details key={JSON.stringify([props.company, props.documentId])} {...props} />;
}
`,
    [`details/${camel}Model.ts`]: `import type { MasterDetailDocumentRecord } from '@patterns/master-detail';

// Extend these UI models with the feature's existing API fields and mappings.
export interface ${name}Line { id: string }
export interface ${name}Document extends MasterDetailDocumentRecord<${name}Line> {}
`,
    [`api/${camel}Api.ts`]: `import type { MasterDetailDocumentSource } from '@patterns/master-detail';
import type { ${name}Document } from '../details/${camel}Model';

// Implement this adapter using existing endpoints and business validation.
// No endpoint, permission or posting behavior is inferred by the generator.
export type ${name}Api = MasterDetailDocumentSource<${name}Document>;
`,
    [`details/use${name}Document.ts`]: `import { useMasterDetailDocument } from '@patterns/master-detail';
import type { ${name}Api } from '../api/${camel}Api';
import type { ${name}Document, ${name}Line } from './${camel}Model';

export interface ${name}DocumentOptions {
  company: string;
  documentId: string;
  source: ${name}Api;
  canEdit: boolean;
}
export function use${name}Document(options: ${name}DocumentOptions) {
  return useMasterDetailDocument<${name}Line, ${name}Document>(options);
}
`,
    [`details/${name}HeaderSections.ts`]: `import type { DetailSectionConfig } from '@patterns/list-details/types';

export function build${name}HeaderSections(t: (key: string, fallback: string) => string): DetailSectionConfig[] {
  return [{
    id: 'general', title: t('common.general', 'General'), visualVariant: 'legalEntity',
    groups: [{ id: 'general', fields: [{ name: 'title', label: t('common.title', 'Title') }] }],
  }];
}
`,
    [`lines/${name}LineColumns.ts`]: `import type { ColumnDef } from '@shared/components/data-grid/types';
import type { ${name}Line } from '../details/${camel}Model';

export function build${name}LineColumns(t: (key: string, fallback: string) => string): ColumnDef<${name}Line>[] {
  // Add the feature's real line fields, lookups and editors here.
  return [{ field: 'id', headerName: t('common.id', 'ID'), editable: false, minWidth: 120 }];
}
`,
    [`details/${name}Details.tsx`]: `import React from 'react';
import { Alert } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { MasterDetailPage, MasterDetailLines, MasterDetailLinesGrid } from '@patterns/master-detail';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { use${name}Document, type ${name}DocumentOptions } from './use${name}Document';
import { build${name}HeaderSections } from './${name}HeaderSections';
import { build${name}LineColumns } from '../lines/${name}LineColumns';
import type { ${name}Line } from './${camel}Model';

export interface ${name}DetailsProps extends ${name}DocumentOptions {
  listPath: string;
  onSelect: (id: string) => void;
  onBack: () => void;
}
export function ${name}Details(props: ${name}DetailsProps): React.ReactElement {
  const { t } = useAppTranslation();
  const state = use${name}Document(props);
  const document = state.document;
  const header = build${name}HeaderSections(t);
  const columns = build${name}LineColumns(t);
  return <MasterDetailPage
    records={state.records} selectedId={props.documentId}
    onSelect={id => { if (!state.editing && !state.busy) props.onSelect(id); }}
    title={document?.title ?? t('common.loading', 'Loading')}
    subtitle={document?.subtitle} breadcrumb={{ listPath: props.listPath }}
    loading={state.loading} error={state.loadError?.message}
    onRetry={state.refresh} onRefresh={state.refresh}
    filterLabel={t('actions.filter', 'Filter')} emptyLabel={t('messages.noRecords', 'No records')}
    editing={state.editing || state.busy}
    backAction={<ActionPaneBackButton label={t('actions.back', 'Back')} onClick={props.onBack} disabled={state.editing || state.busy} />}
    actionPane={<EnterpriseCrudActions
      editLabel={t('actions.edit', 'Edit')} newLabel={t('actions.new', 'New')} deleteLabel={t('actions.delete', 'Delete')}
      saveLabel={t('actions.save', 'Save')} cancelLabel={t('actions.cancel', 'Cancel')}
      canEdit={props.canEdit && Boolean(document)} canNew={false} canDelete={false}
      editing={state.editing} saving={state.busy}
      onEdit={state.beginEdit} onSave={() => void state.save()} onCancel={state.cancel}
    />}
    viewTabs={{ value: state.tab, onChange: value => void state.switchTab(value) }}
    onSearch={() => void state.onSearch()}
    values={{ title: document?.title ?? '' }}
    onFieldChange={(field, value) => state.update(current => field === 'title' ? { ...current, title: String(value) } : current)}
    onFieldEdit={state.beginEdit} canEditField={() => props.canEdit && !state.busy}
    sections={[
      ...(state.error ? [{ id: 'save-error', title: '', hideHeader: true, content: <Alert severity="error">{state.error}</Alert> }] : []),
      ...header,
      ...(state.tab === 'lines' ? [{ id: 'lines', title: t('common.lines', 'Lines'), hideHeader: true,
        content: <MasterDetailLines toolbarLabel={t('common.lines', 'Lines')} saving={state.busy}
          grid={<MasterDetailLinesGrid<${name}Line>
            ref={state.grid} rows={document?.lines ?? []} columns={columns}
            storageKey={JSON.stringify(props.source.queryKey) + '.lines'}
            editing={state.editing} busy={state.busy} canEdit={props.canEdit} onEdit={state.beginEdit}
            hideFilterRow={!state.filterVisible}
            onRowSave={row => { state.update(current => ({ ...current, lines: current.lines.map(line => line.id === row.id ? { ...line, ...row } : line) })); }}
          />} />,
      }] : []),
    ]}
  />;
}
`,
    'README.md': `# ${name} master-details feature

Generated from the standard master-details structure. The page is not registered automatically.

1. Extend details/${camel}Model.ts with the existing document and line fields.
2. Implement the ${name}Api adapter using existing get/list/save APIs, company scoping and validation. Provide a stable, feature-specific queryKey.
3. Configure details/${name}HeaderSections.ts and lines/${name}LineColumns.ts using shared fields, lookups and grid editors.
4. Register a route entry that passes source, company, documentId, canEdit, listPath, onSelect and onBack to ${name}DetailsPage. Derive canEdit from permissions and document status. Protect the route using the existing route registry.
5. Add feature-specific actions, new/delete/posting operations and dialogs only when supported by the actual API. The default scaffold edits existing documents; it does not invent these operations.

The pattern owns layout, styling, tabs, Search, Save/Cancel coordination and grid commit behavior. The keyed route entry resets document state when company or document ID changes. Business calculations, API mapping and validation remain in the feature.
`,
  };
}

export function generateMasterDetail({
  feature,
  name,
  dryRun = false,
  projectRoot = resolve(import.meta.dirname, '..'),
}) {
  if (
    !feature ||
    !feature.split('/').every((part) => /^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$/.test(part))
  ) {
    throw new Error('--feature must be a relative kebab-case path under src/modules.');
  }
  const root = resolve(projectRoot, 'src/modules');
  const destination = resolve(root, feature);
  const within = relative(root, destination);
  if (!within || within.startsWith(`..${sep}`) || isAbsolute(within))
    throw new Error('Invalid feature path.');
  const files = masterDetailFiles(name);
  if (existsSync(destination))
    throw new Error('Feature folder already exists; existing work will not be overwritten.');
  let ancestor = dirname(destination);
  while (!existsSync(ancestor)) ancestor = dirname(ancestor);
  const actualParent = relative(realpathSync(root), realpathSync(ancestor));
  if (actualParent === '..' || actualParent.startsWith(`..${sep}`) || isAbsolute(actualParent))
    throw new Error('Feature parent resolves outside src/modules.');
  if (!dryRun) {
    for (const [file, source] of Object.entries(files)) {
      const target = resolve(destination, file);
      mkdirSync(dirname(target), { recursive: true });
      writeFileSync(target, source, { flag: 'wx' });
    }
  }
  return Object.keys(files).map((file) => `${feature}/${file}`);
}

if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  const args = process.argv.slice(2);
  const value = (flag) => args[args.indexOf(flag) + 1];
  try {
    if (!args.includes('--feature') || !args.includes('--name'))
      throw new Error(
        'Usage: npm run generate:master-detail -- --feature finance/module/feature --name FeatureName [--dry-run]'
      );
    const files = generateMasterDetail({
      feature: value('--feature'),
      name: value('--name'),
      dryRun: args.includes('--dry-run'),
    });
    console.log(
      `${args.includes('--dry-run') ? 'Would generate' : 'Generated'} ${files.length} files:\n${files.join('\n')}`
    );
  } catch (error) {
    console.error(error.message);
    process.exitCode = 1;
  }
}
