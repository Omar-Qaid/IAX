import { existsSync, readdirSync, readFileSync } from 'node:fs';
import { dirname, join, relative, resolve } from 'node:path';

const projectRoot = resolve(import.meta.dirname, '..');
const modules = join(projectRoot, 'src/modules');
const walk = (directory) =>
  readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const path = join(directory, entry.name);
    return entry.isDirectory() ? walk(path) : [path];
  });
const files = walk(modules);
const compositions = files.filter(
  (file) => file.endsWith('.tsx') && /<MasterDetailPage(?:\s|>|<)/.test(readFileSync(file, 'utf8'))
);
const problems = [];
for (const file of compositions) {
  const feature = dirname(dirname(file));
  const label = relative(modules, file);
  if (dirname(file) !== join(feature, 'details'))
    problems.push(`${label}: put the composition in details/.`);
  for (const directory of ['pages', 'details', 'lines', 'api']) {
    if (!existsSync(join(feature, directory))) problems.push(`${label}: missing ${directory}/.`);
  }
  const details = files.filter((path) => dirname(path) === join(feature, 'details'));
  if (!details.some((path) => /use[A-Z].*Document\.tsx?$/.test(path)))
    problems.push(`${label}: missing details/use<Feature>Document.ts.`);
  if (!details.some((path) => /HeaderSections\.tsx?$/.test(path)))
    problems.push(`${label}: missing details/<Feature>HeaderSections.ts[x].`);
}
if (problems.length) {
  console.error(problems.join('\n'));
  process.exitCode = 1;
} else {
  console.log(`Master-details structure passed for ${compositions.length} feature compositions.`);
}
