import type { ReactNode } from 'react';
import type { RecordPreviewOption } from '@shared/components/record-preview';
import type { RelatedInformationSection } from '@shared/components/page/RelatedInformationPanel';
import type { EnterpriseCommandUtilitiesProps } from '@shared/components/action-pane/EnterpriseCommandUtilities';
import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';
import type { DetailSectionConfig, DetailValue, DetailValues } from '@patterns/list-details/types';
import type { PageBreadcrumbConfig } from '@shared/services/pageBreadcrumbStore';

/** One record in the left navigation pane (D365 "list" of a details page). */
export interface MasterDetailRecord {
  id: string;
  /** Primary line, e.g. the order number. */
  title: string;
  /** Secondary line, e.g. "CUST-100 - Consulting Customer". */
  subtitle?: string;
  /** Extra searchable text that is not shown. */
  description?: string;
}

/** Advanced filter (Filter pane, D365 "Filter" on the right rail). */
export interface MasterDetailAdvancedFilter<T> {
  title?: string;
  /** Fields users can filter on, with how to read each from a record. */
  fields: Array<{ id: string; label: string; getValue: (record: T) => unknown }>;
}

/** Action pane tab that opens a ribbon of grouped actions (D365 "Sales order", "Sell"). */
export interface MasterDetailRibbon {
  id: string;
  label: string;
  groups: readonly ActionPaneRibbonGroup[];
  disabled?: boolean;
  /** Remembers whether the user pinned this ribbon open. */
  persistenceKey?: string;
}

/** Plain action pane command after the ribbons (D365 tabs not built yet are disabled). */
export interface MasterDetailCommand {
  id: string;
  label: string;
  icon?: ReactNode;
  disabled?: boolean;
  onClick?: () => void;
}

export interface MasterDetailPageProps<T extends MasterDetailRecord = MasterDetailRecord> {
  /** Shell breadcrumb hierarchy; inferred from the parent list route when omitted. */
  breadcrumb?: PageBreadcrumbConfig | false;
  /** Records of the navigation pane. */
  records: T[];
  selectedId?: string;
  onSelect: (id: string) => void;
  /** Page title, e.g. "SO-2026-000001 : Consulting Customer". */
  title: string;
  /** Document description above the title, e.g. "Sales order details". */
  subtitle?: string;
  /** Shown after the document description, e.g. "Standard view". */
  viewLabel?: string;
  /** Right side of the title row, e.g. the document status. */
  status?: ReactNode;
  filterLabel: string;
  emptyLabel: string;
  /** Action pane content (Edit/Save/Cancel, ribbon triggers, commands). */
  actionPane: ReactNode;
  backAction?: ReactNode;
  /** Ribbon tabs after `actionPane`; locked while editing. */
  ribbons?: readonly MasterDetailRibbon[];
  /** Commands after the ribbons; locked while editing. */
  commands?: readonly MasterDetailCommand[];
  /** Extra items at the end of the action pane, before the utilities. */
  endActions?: ReactNode;
  navigationLabel?: string;
  /** Tabs under the title (see `MasterDetailTabs`). */
  tabs?: ReactNode;
  /** Standard document views; the page retains ownership of tab state and validation. */
  viewTabs?: { value: string; onChange: (value: string) => void; ariaLabel?: string };
  /** Standard Search command. Pages decide how to reveal their line filters. */
  onSearch?: () => void;
  /** Edit mode: navigation, filter, options and utilities are locked, like D365. */
  editing?: boolean;
  loading?: boolean;
  error?: string | null;
  onRetry?: () => void;

  // ----- navigation pane -------------------------------------------------
  /** Show the navigation pane initially (it can always be toggled). */
  defaultNavigationOpen?: boolean;
  /** Persist the pane width per page (user-resizable). */
  navigationWidthStorageKey?: string;
  /** Custom quick-filter match (default: title, subtitle, description). */
  matchesSearch?: (record: T, query: string) => boolean;

  // ----- advanced filter + related information (right rail) ---------------
  advancedFilter?: MasterDetailAdvancedFilter<T>;
  /** FactBox-style related information for the open record. */
  relatedInformation?: { title?: string; sections: RelatedInformationSection[] };

  // ----- record audit, attachments, utilities ------------------------------
  /** Table name for record info / audit (Options menu), e.g. "SalesTable". */
  recordTableName?: string;
  /** Record ID for audit (defaults to the record's `id`). */
  getAuditRecordId?: (record: T) => string | number;
  /** Attachments: table ID and RecId of the open record (omit to hide). */
  attachments?: { refTableId: number; refRecId: number | null };
  showOptionsMenu?: boolean;
  /** Labels/handlers for the standard utility icons (refresh, personalize…). */
  utilities?: Partial<EnterpriseCommandUtilitiesProps>;
  onRefresh?: () => void;

  // ----- field-config sections (optional) ---------------------------------
  /**
   * Header/detail sections built from field configs (same engine as
   * list-details): groups of fields, view/edit, master routes and previews.
   * Rendered before `children`.
   */
  sections?: DetailSectionConfig[];
  values?: DetailValues;
  onFieldChange?: (name: string, value: DetailValue) => void;
  onFieldEdit?: (name: string) => void;
  canEditField?: (name: string) => boolean;

  children?: ReactNode;
}

/** One header field: label + value (view) or the editor the page supplies (edit). */
export interface MasterDetailFieldProps {
  label: string;
  /** Text shown in view mode. */
  value?: ReactNode;
  /** Editor rendered in edit mode (the page owns its state and saving). */
  editor?: ReactNode;
  editing?: boolean;
  /** Numeric values align to the end, like D365. */
  numeric?: boolean;
  /** Pencil on the view value: switches the page to edit mode. */
  onEdit?: () => void;
  /** Master data: open-details icon on the label and blue link style on the value. */
  masterRoute?: string;
  /** Registered record preview entity (hover card); resolved from `masterRoute` when omitted. */
  previewEntity?: string;
  /** Key of the referenced record (defaults to the value). */
  previewKey?: string | number | null;
  /** The loaded lookup option: the hover card shows it without a request. */
  previewOption?: RecordPreviewOption | null;
  /** Mark a value as a reference even without a master route (blue text). */
  lookup?: boolean;
}
