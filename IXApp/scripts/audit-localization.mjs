import fs from 'node:fs';
import path from 'node:path';
import ts from 'typescript';

const sourceRoot = path.resolve('src');
// Mock/business records are data, not interface copy. Their localization belongs to the
// backing data source, while this audit targets strings owned by React UI code.
const ignoredSegments = new Set(['test', 'tests', '__tests__', 'mocks']);
const userFacingAttributes = new Set([
  'alt',
  'aria-description',
  'aria-label',
  'helperText',
  'label',
  'placeholder',
  'title',
]);
const userFacingProperties = new Set([
  'ariaDescription',
  'ariaLabel',
  'description',
  'emptyText',
  'errorMessage',
  'headerName',
  'helperText',
  'label',
  'loadingText',
  'message',
  'noOptionsText',
  'placeholder',
  'subtitle',
  'successMessage',
  'title',
  'tooltip',
  'warningMessage',
]);
const visibleText = /[A-Za-z\u0600-\u06ff]{2}/u;
const translationKey = /^[a-z][A-Za-z0-9]*(?:\.[A-Za-z0-9_-]+)+$/u;
const intentionallyLiteralFiles = new Set([
  'src/modules/workflow/pages/mailTemporaryData.ts', // Unused legacy sample records.
]);
const intentionalTechnicalText = new Set([
  'esc',
  'Aa',
  'Segoe UI',
  'Inter',
  'Roboto',
  'Outfit',
  'Plus Jakarta Sans',
  'SEPA',
]);

const isIntentionalLiteral = (file, kind, text) => {
  if (intentionallyLiteralFiles.has(file)) return true;
  if (kind === 'default-parameter') return true; // Internal API/default values are not rendered copy.
  if (intentionalTechnicalText.has(text)) return true;
  if (file === 'src/core/localization/languages.ts') return true; // Language metadata/native names.
  if (file === 'src/modules/process-builder/components/ProcessBuilderPalette.tsx') return true; // Compatibility labels; rendering uses translation keys.
  if (file === 'src/modules/workflow/components/DynamicSpecialControls.tsx') {
    return [
      'PDF',
      'ZIP',
      'Audio',
      'Video',
      'Image',
      'Word',
      'Excel',
      'PowerPoint',
      'TXT',
      'PS',
      'Ps',
      'AI',
      'Ai',
      '© OpenStreetMap',
    ].includes(text);
  }
  return false;
};

const files = [];
const visitDirectory = (directory) => {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    if (entry.isDirectory() && ignoredSegments.has(entry.name)) continue;
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) visitDirectory(fullPath);
    else if (/\.tsx?$/u.test(entry.name) && !entry.name.endsWith('.d.ts')) files.push(fullPath);
  }
};
visitDirectory(sourceRoot);

const findings = [];
const addFinding = (sourceFile, node, kind, value) => {
  const text = value.trim().replace(/\s+/gu, ' ');
  if (!visibleText.test(text) || translationKey.test(text)) return;
  const file = path.relative(process.cwd(), sourceFile.fileName).replaceAll('\\', '/');
  if (isIntentionalLiteral(file, kind, text)) return;
  const { line, character } = sourceFile.getLineAndCharacterOfPosition(node.getStart(sourceFile));
  findings.push({
    file,
    line: line + 1,
    column: character + 1,
    kind,
    text,
  });
};

for (const file of files) {
  const source = fs.readFileSync(file, 'utf8');
  const sourceFile = ts.createSourceFile(
    file,
    source,
    ts.ScriptTarget.Latest,
    true,
    file.endsWith('.tsx') ? ts.ScriptKind.TSX : ts.ScriptKind.TS
  );
  const walk = (node) => {
    if (ts.isJsxText(node)) addFinding(sourceFile, node, 'jsx-text', node.text);

    if (ts.isJsxAttribute(node) && userFacingAttributes.has(node.name.text)) {
      if (node.initializer && ts.isStringLiteral(node.initializer)) {
        addFinding(
          sourceFile,
          node.initializer,
          `attribute:${node.name.text}`,
          node.initializer.text
        );
      } else if (
        node.initializer &&
        ts.isJsxExpression(node.initializer) &&
        node.initializer.expression &&
        (ts.isStringLiteral(node.initializer.expression) ||
          ts.isNoSubstitutionTemplateLiteral(node.initializer.expression))
      ) {
        addFinding(
          sourceFile,
          node.initializer.expression,
          `attribute:${node.name.text}`,
          node.initializer.expression.text
        );
      }
    }

    if (ts.isPropertyAssignment(node)) {
      const name =
        ts.isIdentifier(node.name) || ts.isStringLiteral(node.name) ? node.name.text : '';
      if (
        userFacingProperties.has(name) &&
        (ts.isStringLiteral(node.initializer) ||
          ts.isNoSubstitutionTemplateLiteral(node.initializer))
      ) {
        addFinding(sourceFile, node.initializer, `property:${name}`, node.initializer.text);
      }
    }

    if (
      ts.isParameter(node) &&
      node.initializer &&
      (ts.isStringLiteral(node.initializer) || ts.isNoSubstitutionTemplateLiteral(node.initializer))
    ) {
      addFinding(sourceFile, node.initializer, 'default-parameter', node.initializer.text);
    }

    if (
      ts.isCallExpression(node) &&
      ts.isIdentifier(node.expression) &&
      ['alert', 'confirm', 'prompt'].includes(node.expression.text)
    ) {
      const argument = node.arguments[0];
      if (
        argument &&
        (ts.isStringLiteral(argument) || ts.isNoSubstitutionTemplateLiteral(argument))
      ) {
        addFinding(sourceFile, argument, `call:${node.expression.text}`, argument.text);
      }
    }

    ts.forEachChild(node, walk);
  };
  walk(sourceFile);
}

findings.sort((a, b) => a.file.localeCompare(b.file) || a.line - b.line || a.column - b.column);
for (const finding of findings) {
  console.log(
    `${finding.file}:${finding.line}:${finding.column} [${finding.kind}] ${finding.text}`
  );
}
console.log(`Localization audit: ${findings.length} literal user-facing string candidate(s).`);
process.exitCode = findings.length === 0 ? 0 : 1;
