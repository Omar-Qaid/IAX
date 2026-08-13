const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1880, height: 827 } });
  await page.goto('http://127.0.0.1:4173/process-builder/new', { waitUntil: 'networkidle' });
  await page.getByRole('tab', { name: 'Palette' }).click();
  await page.screenshot({ path: 'C:/Users/Omar.Qaid/AppData/Local/Temp/ixapp-pb-palette-before.png' });
  await browser.close();
})();
