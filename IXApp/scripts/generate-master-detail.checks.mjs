import {
  mkdtempSync,
  mkdirSync,
  readFileSync,
  existsSync,
  rmSync,
  realpathSync,
  renameSync,
  writeFileSync,
} from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve, basename } from 'node:path';
import { test } from 'node:test';
import assert from 'node:assert/strict';
import ts from 'typescript';
import { generateMasterDetail, masterDetailFiles } from './generate-master-detail.mjs';
import { masterDetailStructure } from './master-detail-structure.mjs';
import { auditMasterDetail } from './audit-master-detail.mjs';

function fixture(run) {
  const root = realpathSync(mkdtempSync(join(tmpdir(), 'ixapp-master-detail-')));
  mkdirSync(join(root, 'src/modules'), { recursive: true });
  try {
    run(root);
  } finally {
    if (
      !basename(root).startsWith('ixapp-master-detail-') ||
      !resolve(root).startsWith(realpathSync(tmpdir()))
    )
      throw new Error('Unsafe fixture cleanup path.');
    rmSync(root, { recursive: true });
  }
}

test('generates the same eight-file structure and never overwrites a feature', () =>
  fixture((projectRoot) => {
    const options = {
      projectRoot,
      feature: 'finance/sample/purchase-order',
      name: 'PurchaseOrder',
    };
    const paths = generateMasterDetail(options);
    assert.equal(paths.length, 8);
    for (const path of paths) assert.ok(existsSync(join(projectRoot, 'src/modules', path)));
    const page = readFileSync(join(projectRoot, 'src/modules', paths[0]), 'utf8');
    assert.match(page, /key=\{JSON\.stringify\(\[props\.company, props\.documentId\]\)\}/);
    assert.throws(() => generateMasterDetail(options), /already exists/);
    assert.equal(readFileSync(join(projectRoot, 'src/modules', paths[0]), 'utf8'), page);
  }));

test('dry-run creates nothing and rejects invalid names and paths', () =>
  fixture((projectRoot) => {
    generateMasterDetail({
      projectRoot,
      feature: 'finance/example',
      name: 'Example',
      dryRun: true,
    });
    assert.equal(existsSync(join(projectRoot, 'src/modules/finance')), false);
    for (const feature of [
      '../outside',
      '/outside',
      'finance/../../outside',
      'finance\\outside',
      'finance//outside',
    ]) {
      assert.throws(() => generateMasterDetail({ projectRoot, feature, name: 'Example' }));
    }
    assert.throws(() => masterDetailFiles('../Example'));
  }));

test('generated TypeScript parses for different feature names', () => {
  for (const name of ['PurchaseOrder', 'TransferOrder', 'GeneralJournal']) {
    for (const [fileName, source] of Object.entries(masterDetailFiles(name))) {
      if (!/\.tsx?$/.test(fileName)) continue;
      const output = ts.transpileModule(source, {
        fileName,
        reportDiagnostics: true,
        compilerOptions: {
          jsx: ts.JsxEmit.ReactJSX,
          target: ts.ScriptTarget.ES2022,
          module: ts.ModuleKind.ESNext,
        },
      });
      assert.deepEqual(
        output.diagnostics?.filter(
          (diagnostic) => diagnostic.category === ts.DiagnosticCategory.Error
        ),
        [],
        fileName
      );
    }
  }
});

test('twelve future features use exactly the same file map and pass the structure audit', () =>
  fixture((projectRoot) => {
    const names = [
      'PurchaseOrder',
      'TransferOrder',
      'GeneralJournal',
      'PaymentJournal',
      'InventoryJournal',
      'ReturnOrder',
      'VendorInvoice',
      'CustomerInvoice',
      'ServiceOrder',
      'ReceiptJournal',
      'ShipmentOrder',
      'AdjustmentJournal',
    ];
    for (const name of names) {
      const files = masterDetailFiles(name);
      assert.deepEqual(Object.keys(files), Object.values(masterDetailStructure(name)));
      generateMasterDetail({ projectRoot, feature: `finance/example/${name.toLowerCase()}`, name });
    }
    const result = auditMasterDetail(projectRoot);
    assert.deepEqual(result.problems, []);
    assert.equal(result.compositions, 12);
    assert.equal(result.standardFeatures, 12);
  }));

test('audit catches renamed standard files and a removed structure marker', () =>
  fixture((projectRoot) => {
    const feature = 'finance/example/sample-order';
    generateMasterDetail({ projectRoot, feature, name: 'SampleOrder' });
    const root = join(projectRoot, 'src/modules', feature);
    const columns = join(root, 'lines/SampleOrderLineColumns.ts');
    const renamed = join(root, 'lines/DifferentColumns.ts');
    renameSync(columns, renamed);
    assert.ok(
      auditMasterDetail(projectRoot).problems.some((problem) =>
        problem.includes('missing standard columns file')
      )
    );
    renameSync(renamed, columns);
    renameSync(columns, `${columns}x`);
    assert.deepEqual(auditMasterDetail(projectRoot).problems, []);
    const readme = join(root, 'README.md');
    writeFileSync(
      readme,
      readFileSync(readme, 'utf8').replace('<!-- master-detail-feature: SampleOrder -->', '')
    );
    assert.ok(
      auditMasterDetail(projectRoot).problems.some((problem) =>
        problem.includes('structure marker')
      )
    );
  }));
