import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { AVAILABLE_MODULE_NAV_CONFIGS } from '@app/configuration/navigation';
import { getPageDefinition } from '@app/routes/pageRegistry';
import { LEGACY_TAX_ROUTE_PATHS, TAX_ROUTE_PATHS } from '@modules/finance/tax/routes/taxRoutePaths';
import { LegacyTaxPageRedirect } from '@modules/finance/tax/routes/LegacyTaxPageRedirect';

function Destination() {
  const location = useLocation();
  return (
    <div>
      {location.pathname}
      {location.search}
      {location.hash}
    </div>
  );
}

describe('Tax module routing', () => {
  it('owns its registered pages and removes them from General ledger setup', () => {
    const tax = AVAILABLE_MODULE_NAV_CONFIGS['mod-Tax'];
    expect(tax.defaultPath).toBe(TAX_ROUTE_PATHS.TAX_CODES);
    expect(tax.matchPath).toBe('/tax');
    const taxPaths = tax.sections.flatMap((section) => section.links.map((link) => link.path));
    expect(taxPaths).toHaveLength(5);
    for (const path of taxPaths) expect(getPageDefinition(path!)?.permission).toMatch(/^Tax\./);
    const ledgerPaths = AVAILABLE_MODULE_NAV_CONFIGS['mod-GeneralLedger'].sections.flatMap(
      (section) => section.links.map((link) => link.path)
    );
    expect(ledgerPaths.some((path) => path?.startsWith('/tax/'))).toBe(false);
  });

  it.each(Object.keys(LEGACY_TAX_ROUTE_PATHS) as (keyof typeof LEGACY_TAX_ROUTE_PATHS)[])(
    'keeps saved %s links working with their query and hash',
    async (key) => {
      expect(getPageDefinition(LEGACY_TAX_ROUTE_PATHS[key])?.permission).toBe(
        getPageDefinition(TAX_ROUTE_PATHS[key])?.permission
      );
      render(
        <MemoryRouter initialEntries={[`${LEGACY_TAX_ROUTE_PATHS[key]}?company=dat#details`]}>
          <Routes>
            <Route path={LEGACY_TAX_ROUTE_PATHS[key]} element={<LegacyTaxPageRedirect />} />
            <Route path={TAX_ROUTE_PATHS[key]} element={<Destination />} />
          </Routes>
        </MemoryRouter>
      );
      expect(
        await screen.findByText(`${TAX_ROUTE_PATHS[key]}?company=dat#details`)
      ).toBeInTheDocument();
    }
  );
});
