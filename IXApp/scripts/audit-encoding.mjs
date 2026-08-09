import { readFileSync, readdirSync } from 'node:fs';
import { extname, join, relative, resolve } from 'node:path';

const projectRoot = resolve(import.meta.dirname, '..');
const includedRoots = ['src', 'docs', 'public'];
const includedExtensions = new Set(['.ts', '.tsx', '.json', '.md', '.css', '.html']);
const mojibakeMarkers = ['\uFFFD', 'Ã', 'Â', 'â€', 'â€”', 'â”', 'Ø', 'Ù'];
const findings = [];

const walk = (directory) =>
  readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const path = join(directory, entry.name);
    return entry.isDirectory()
      ? walk(path)
      : includedExtensions.has(extname(entry.name))
        ? [path]
        : [];
  });

for (const root of includedRoots) {
  for (const file of walk(join(projectRoot, root))) {
    const lines = readFileSync(file, 'utf8').split(/\r?\n/);
    lines.forEach((line, index) => {
      if (mojibakeMarkers.some((marker) => line.includes(marker))) {
        findings.push(`${relative(projectRoot, file)}:${index + 1}`);
      }
    });
  }
}

for (const file of ['ARCHITECTURE.md', 'README.md', 'index.html']) {
  const path = join(projectRoot, file);
  const lines = readFileSync(path, 'utf8').split(/\r?\n/);
  lines.forEach((line, index) => {
    if (mojibakeMarkers.some((marker) => line.includes(marker))) {
      findings.push(`${file}:${index + 1}`);
    }
  });
}

if (findings.length) {
  console.error('Possible UTF-8 mojibake detected:');
  findings.forEach((finding) => console.error(`- ${finding}`));
  process.exitCode = 1;
} else {
  console.log('UTF-8 mojibake check passed.');
}
