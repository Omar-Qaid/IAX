import { spawn } from 'node:child_process';
import { resolve } from 'node:path';
import { createServer } from 'vite';

const host = '127.0.0.1';
const port = 4173;
const playwrightCli = resolve('node_modules', '@playwright', 'test', 'cli.js');

const server = await createServer({
  mode: 'test',
  logLevel: 'error',
  server: {
    host,
    port,
    strictPort: true,
  },
});

let exitCode = 1;

try {
  await server.listen();

  const playwright = spawn(process.execPath, [playwrightCli, 'test', ...process.argv.slice(2)], {
    env: {
      ...process.env,
      PLAYWRIGHT_EXTERNAL_SERVER: 'true',
    },
    stdio: 'inherit',
  });

  exitCode = await new Promise((resolveExitCode, reject) => {
    playwright.once('error', reject);
    playwright.once('exit', (code, signal) => {
      if (signal) {
        reject(new Error(`Playwright terminated by signal ${signal}.`));
        return;
      }
      resolveExitCode(code ?? 1);
    });
  });
} finally {
  await server.close();
}

process.exitCode = exitCode;
