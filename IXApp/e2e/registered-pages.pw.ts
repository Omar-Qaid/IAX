import { expect, test } from '@playwright/test';
import { writeFile } from 'node:fs/promises';

// This is deliberately an unavailable-API check, not proof that a business
// operation succeeds. Specific CRUD/posting tests supply their own API fixtures.
test('every registered screen handles an unavailable API without crashing', async ({
  page,
}, testInfo) => {
  test.setTimeout(600_000);
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  await page.route('http://localhost:33319/api/**', (route) =>
    route.fulfill({
      status: 503,
      json: { success: false, message: 'Frontend audit: API unavailable' },
    })
  );
  await page.goto('/dashboard');
  const definitions = await page.evaluate(async () => {
    // Vite resolves the same registry and aliases used by the application.
    const modulePath = '/src/app/routes/pageRegistry.ts';
    const registry = await import(/* @vite-ignore */ modulePath);
    return registry.APP_PAGE_DEFINITIONS.map((entry: { id: string; path: string }) => ({
      id: entry.id,
      path: entry.path,
    })) as { id: string; path: string }[];
  });
  const results: { id: string; path: string; buttons: string[]; errors: string[] }[] = [];
  let errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  for (const definition of definitions) {
    errors = [];
    const path = definition.path.replace(/:([\w]+)\??/g, (_, name: string) =>
      name === 'kind' ? 'customer' : name.toLowerCase().includes('account') ? 'AUDIT' : '1'
    );
    let buttons: string[] = [];
    try {
      await page.goto(path);
      await expect(page.locator('main')).toBeVisible({ timeout: 30_000 });
      // Let lazy routes resolve before inspecting the main content.
      await expect(page.locator('main').getByText(/Loading page/i)).toHaveCount(0, {
        timeout: 30_000,
      });
      buttons = await page
        .locator('main button')
        .evaluateAll((elements) =>
          elements.map(
            (element) =>
              element.getAttribute('aria-label') ||
              element.textContent?.trim() ||
              element.getAttribute('title') ||
              '(unlabelled button)'
          )
        );
      const content = await page.locator('main').innerText();
      if (
        /Something went wrong|Unexpected Application Error|Cannot read properties|is not a function/.test(
          content
        )
      ) {
        errors.push(content.slice(0, 1000));
      }
    } catch (error) {
      errors.push(String(error));
    }
    results.push({ id: definition.id, path, buttons, errors: [...errors] });
    console.log(
      `Screen ${results.length}/${definitions.length}: ${definition.id}: ${errors.length ? 'FAILED' : 'opened'}`
    );
    expect.soft(errors, `${definition.id}: ${path}`).toEqual([]);
  }
  const inventoryPath = testInfo.outputPath('registered-screen-error-state-audit.json');
  await writeFile(
    inventoryPath,
    JSON.stringify(
      { mode: 'unavailable-api-smoke-only', actionBehaviorVerified: false, screens: results },
      null,
      2
    )
  );
  await testInfo.attach('registered-screen-error-state-audit', {
    path: inventoryPath,
    contentType: 'application/json',
  });
});
