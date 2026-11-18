import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { LegacyTaxPageRedirect } from '@modules/finance/tax/routes/LegacyTaxPageRedirect';
import { LEGACY_TAX_ROUTE_PATHS, TAX_ROUTE_PATHS } from '@modules/finance/tax/routes/taxRoutePaths';

function Destination() {
  const location = useLocation();
  return (
    <output>
      {JSON.stringify({
        url: location.pathname + location.search + location.hash,
        state: location.state,
      })}
    </output>
  );
}

const cases = Object.keys(LEGACY_TAX_ROUTE_PATHS).flatMap((name) => {
  const key = name as keyof typeof LEGACY_TAX_ROUTE_PATHS;
  return [
    { path: `${LEGACY_TAX_ROUTE_PATHS[key]}/`, target: TAX_ROUTE_PATHS[key] },
    { path: `${LEGACY_TAX_ROUTE_PATHS[key].toUpperCase()}/`, target: TAX_ROUTE_PATHS[key] },
  ];
});

describe('legacy Tax link compatibility', () => {
  it.each(cases)('redirects $path without losing the return state', async ({ path, target }) => {
    const state = { returnTo: '/accounts-receivable/customers/1' };
    render(
      <MemoryRouter
        initialEntries={[{ pathname: path, search: '?company=dat', hash: '#details', state }]}
      >
        <Routes>
          {Object.values(LEGACY_TAX_ROUTE_PATHS).map((legacy) => (
            <Route key={legacy} path={legacy} element={<LegacyTaxPageRedirect />} />
          ))}
          <Route path={target} element={<Destination />} />
        </Routes>
      </MemoryRouter>
    );
    expect(await screen.findByRole('status')).toHaveTextContent(
      JSON.stringify({ url: `${target}?company=dat#details`, state })
    );
  });
});
