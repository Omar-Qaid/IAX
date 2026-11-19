import { mkdtempSync, mkdirSync, readFileSync, existsSync, rmSync, realpathSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve, basename } from 'node:path';
import { test } from 'node:test';
import assert from 'node:assert/strict';
import ts from 'typescript';
import { generateMasterDetail, masterDetailFiles } from './generate-master-detail.mjs';

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
