import { readFileSync, readdirSync } from 'node:fs';
import { extname, join, relative, resolve, sep } from 'node:path';

const projectRoot = resolve(import.meta.dirname, '..');
const sourceRoot = join(projectRoot, 'src');
const extensions = new Set(['.ts', '.tsx']);
const walk = (directory) =>
  readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const path = join(directory, entry.name);
    return entry.isDirectory() ? walk(path) : extensions.has(extname(entry.name)) ? [path] : [];
  });

const files = walk(sourceRoot);
const contents = new Map(files.map((file) => [file, readFileSync(file, 'utf8')]));
const exportPattern =
  /export\s+(?:declare\s+)?(?:abstract\s+)?(?:const|let|function|class|interface|type|enum)\s+([A-Za-z_$][\w$]*)/g;
const candidates = [];

for (const [file, content] of contents) {
  for (const match of content.matchAll(exportPattern)) {
    const symbol = match[1];
    const symbolPattern = new RegExp(`\\b${symbol}\\b`);
    const consumers = [...contents].filter(
      ([candidateFile, candidateContent]) =>
        candidateFile !== file && symbolPattern.test(candidateContent)
    );
    if (!consumers.length) {
      const line = content.slice(0, match.index).split(/\r?\n/).length;
      candidates.push({
        path: relative(projectRoot, file).split(sep).join('/'),
        line,
        symbol,
      });
    }
  }
}

console.log(
  `Possible unused exports: ${candidates.length}. Review before removal; this report is intentionally non-blocking.`
);
candidates
  .sort((left, right) => left.path.localeCompare(right.path) || left.line - right.line)
  .forEach(({ path, line, symbol }) => console.log(`- ${path}:${line} ${symbol}`));
