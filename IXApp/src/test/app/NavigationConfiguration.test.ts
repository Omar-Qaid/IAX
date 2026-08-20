import { describe, expect, it } from 'vitest';
import {
  AVAILABLE_MODULE_NAV_CONFIGS,
  filterModuleNavigation,
  getModuleNavLinkPermission,
  type ModuleNavConfig,
} from '@app/configuration/navigation';
import { getPageDefinition } from '@app/routes/pageRegistry';
import { findPageDefinitionForPath } from '@app/routes/pageRegistry';
import { COMMAND_PALETTE_PAGES } from '@app/shell/AppCommandPalette';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { PERMISSIONS } from '@core/permissions/permissions';

describe('module navigation configuration', () => {
  it('exposes only module defaults and links backed by registered pages', () => {
    for (const config of Object.values(AVAILABLE_MODULE_NAV_CONFIGS)) {
      expect(getPageDefinition(config.defaultPath)).toBeDefined();
      for (const section of config.sections) {
        for (const link of section.links) {
          if (link.path) expect(getPageDefinition(link.path)).toBeDefined();
        }
      }
    }
  });

  it('removes unsupported links and modules with unsupported default routes', () => {
    const configs: Record<string, ModuleNavConfig> = {
      supported: {
        moduleId: 'supported',
        label: 'nav.accountsReceivable',
        icon: 'receipt',
        defaultPath: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS,
        matchPath: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.ROOT,
        sections: [
          {
            id: 'customers',
            title: 'nav.customers',
            links: [
              { label: 'nav.allCustomers', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS },
              { label: 'nav.unsupported', path: '/unsupported' },
            ],
          },
        ],
      },
      unsupported: {
        moduleId: 'unsupported',
        label: 'nav.unsupported',
        icon: 'default',
        defaultPath: '/unsupported',
        matchPath: '/unsupported',
        sections: [],
      },
    };

    const filtered = filterModuleNavigation(configs);

    expect(Object.keys(filtered)).toEqual(['supported']);
    expect(filtered.supported.sections[0].links).toHaveLength(1);
  });

  it('derives omitted link permissions from the registered page', () => {
    expect(
      getModuleNavLinkPermission({
        label: 'nav.settings',
        path: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS,
      })
    ).toBe(PERMISSIONS.SETTINGS_VIEW);
    expect(getPageDefinition(ROUTE_PATHS.SYSTEM_ADMINISTRATION.NUMBER_SEQUENCES)?.permission).toBe(
      PERMISSIONS.NUMBER_SEQUENCE_VIEW
    );
  });

  it('builds command-palette entries only from registered, non-parameterized pages', () => {
    expect(COMMAND_PALETTE_PAGES.length).toBeGreaterThan(0);
    for (const page of COMMAND_PALETTE_PAGES) {
      expect(getPageDefinition(page.path)).toBeDefined();
      expect(page.path).not.toContain(':');
    }
    expect(COMMAND_PALETTE_PAGES.some((page) => page.path === '/pointOfSale')).toBe(false);
  });

  it('matches concrete detail URLs without exposing unregistered paths', () => {
    expect(findPageDefinitionForPath('/accounts-receivable/sales-orders/SO-1001')?.id).toBe(
      'sales-order-details'
    );
    expect(findPageDefinitionForPath('/unsupported')).toBeUndefined();
  });

  it('registers the backend-backed workflow processes page', () => {
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.REQUEST_SUBMISSION)).toBeDefined();
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.REQUEST_FROM)).toBeDefined();
    expect(findPageDefinitionForPath('/workflow/request-from/10/100')?.id).toBe('request-from');
    expect(
      AVAILABLE_MODULE_NAV_CONFIGS['mod-Workflow']?.sections.find(
        (section) => section.id === 'requests'
      )?.links.some((link) => link.path === ROUTE_PATHS.WORKFLOW.REQUEST_SUBMISSION)
    ).toBe(true);
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.PROCESSES)?.permission).toBe(
      PERMISSIONS.WF_PROCESS_VIEW
    );
    expect(getPageDefinition(ROUTE_PATHS.PROCESS_BUILDER)?.permission).toBeUndefined();
    expect(getPageDefinition(ROUTE_PATHS.PROCESS_BUILDER_NEW)).toBeDefined();
    expect(
      AVAILABLE_MODULE_NAV_CONFIGS['mod-Workflow']?.sections
        .flatMap((section) => section.links)
        .some((link) => link.path === ROUTE_PATHS.PROCESS_BUILDER_NEW)
    ).toBe(true);
    expect(AVAILABLE_MODULE_NAV_CONFIGS['mod-Workflow']?.defaultPath).toBe(
      ROUTE_PATHS.WORKFLOW.PROCESSES
    );
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.CATEGORIES)?.permission).toBe(
      PERMISSIONS.WF_CATEGORY_VIEW
    );
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.ACTIVITY_TYPES)?.permission).toBe(
      PERMISSIONS.WF_ACTIVITY_TYPE_VIEW
    );
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.DATA_TYPES)?.permission).toBeUndefined();
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.CONTROLS)?.permission).toBe(
      PERMISSIONS.WF_CONTROL_VIEW
    );
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.PRIORITIES)?.permission).toBe(
      PERMISSIONS.WF_PRIORITY_VIEW
    );
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.VARIABLES)?.permission).toBe(
      PERMISSIONS.WF_VARIABLE_VIEW
    );
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.STEPS)?.permission).toBe(
      PERMISSIONS.WF_STEP_VIEW
    );
    expect(getPageDefinition(ROUTE_PATHS.WORKFLOW.ACTIVITIES)?.permission).toBe(
      PERMISSIONS.WF_ACTIVITY_VIEW
    );
  });
});
