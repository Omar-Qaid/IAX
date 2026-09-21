import React from 'react';
import { OrganizationUnitsPage } from './OrganizationUnitsPage';

/** @deprecated Kept so existing /organizations bookmarks use the list-details Organization Units page. */
export function OrganizationPage(): React.ReactElement {
  return <OrganizationUnitsPage />;
}
