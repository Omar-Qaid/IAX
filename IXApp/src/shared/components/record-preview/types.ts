/** One labelled value in a record preview card. */
export interface RecordPreviewField<TRecord = Record<string, unknown>> {
  /** Translation key for the label (falls back to `defaultLabel`). */
  labelKey: string;
  defaultLabel: string;
  /** Property to show, or a function for computed / localized values. */
  value: keyof TRecord | ((record: TRecord, context: RecordPreviewValueContext) => unknown);
  /** The value references another master record: shown as a link, like D365. */
  lookup?: boolean;
}

/** Passed to computed field values for localized text, dates and numbers. */
export interface RecordPreviewValueContext {
  isRtl: boolean;
  /** Current UI language (BCP 47), for `toLocaleString` and friends. */
  language: string;
}

/** Loads one record by the key users see (account number, group ID, code). */
export type RecordPreviewLoader<TRecord> = (
  key: string,
  signal?: AbortSignal
) => Promise<TRecord | null>;

/** The lookup option a field already holds for the value (code + display name). */
export interface RecordPreviewOption {
  code: string;
  name?: string | null;
}

/** How to preview one master-data entity. */
export interface RecordPreviewDefinition<TRecord = Record<string, unknown>> {
  /**
   * Fetches the record. Optional for entities a page always previews from data it
   * already holds (pass `record` to `RecordPreview`), e.g. an order's addresses.
   */
  load?: RecordPreviewLoader<TRecord>;
  fields: RecordPreviewField<TRecord>[];
  /** Cache lifetime of a loaded record. Default: `RECORD_PREVIEW_STALE_TIME_MS`. */
  staleTimeMs?: number;
  /**
   * Route of the entity's master form. Fields and lookups that already declare
   * this `masterRoute` get the preview automatically, with no per-field setup.
   */
  masterRoute?: string;
  /**
   * Builds the card record from the lookup option a field already holds, so the
   * card shows at once, without a request (and without needing access to the
   * master's own endpoint). Used when the option carries a name.
   */
  fromOption?: (option: RecordPreviewOption) => TRecord;
}

/** A resolved card row, ready to render. */
export interface RecordPreviewRow {
  label: string;
  value: unknown;
  lookup?: boolean;
}
