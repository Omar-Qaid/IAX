import React from 'react';
import { Box, Popper } from '@mui/material';
import { useRecordPreview } from './hooks';
import { getRecordPreview, resolveRecordPreviewEntity } from './recordPreviewRegistry';
import { RecordPreviewCard } from './RecordPreviewCard';
import type { RecordPreviewOption } from './types';

export interface RecordPreviewProps {
  /** Registered master-data entity, e.g. "customer" or "customerGroup". */
  entity?: string;
  /** Alternatively the field's master form route; resolves the registered entity. */
  masterRoute?: string;
  /** The key users see (account number, group ID). No preview when empty. */
  recordKey?: string | number | null;
  /** Record the page already holds: shown directly, without loading. */
  record?: unknown;
  /** Lookup option the field holds; entities with `fromOption` show it without loading. */
  option?: RecordPreviewOption | null;
  children: React.ReactNode;
  /** Wrap the content inline instead of filling the cell/field width. */
  inline?: boolean;
}

/**
 * Generic D365 record preview for master data, in grids and forms. Renders its
 * children unchanged when no registered entity applies or the key is empty, so
 * it can wrap any value safely.
 */
export function RecordPreview({
  entity,
  masterRoute,
  recordKey,
  record,
  option,
  children,
  inline = false,
}: RecordPreviewProps) {
  const resolved = resolveRecordPreviewEntity(entity, masterRoute);
  const key = recordKey == null ? '' : String(recordKey).trim();
  const definition = resolved ? getRecordPreview(resolved) : undefined;
  // Prefer data already on the page: the option's name makes the card instant.
  const optionRecord =
    record === undefined && option?.name && definition?.fromOption
      ? definition.fromOption(option)
      : undefined;
  record = record ?? optionRecord;
  // Nothing to show: no entity, no key, or an entity that cannot load and got no record.
  const loadable = resolved !== undefined && (record !== undefined || definition?.load !== undefined);
  if (!resolved || key === '' || !loadable) return <>{children}</>;
  return (
    <RecordPreviewTrigger entity={resolved} recordKey={key} record={record} inline={inline}>
      {children}
    </RecordPreviewTrigger>
  );
}

function RecordPreviewTrigger({
  entity,
  recordKey,
  record,
  inline,
  children,
}: {
  entity: string;
  recordKey: string;
  record?: unknown;
  inline: boolean;
  children: React.ReactNode;
}) {
  const preview = useRecordPreview(entity, recordKey, record);
  return (
    <Box
      component="span"
      {...preview.triggerProps}
      sx={{
        display: inline ? 'inline-flex' : 'flex',
        alignItems: 'center',
        minWidth: 0,
        maxWidth: '100%',
        width: inline ? undefined : '100%',
        // Fill the field/cell: a wrapped editor or label block must not shrink.
        ...(inline ? {} : { '& > :not(.MuiPopper-root)': { flex: '1 1 auto', minWidth: 0 } }),
      }}
    >
      {children}
      <Popper
        id={preview.id}
        open={preview.open}
        anchorEl={preview.anchor}
        placement="top-start"
        sx={{ zIndex: 1500 }}
        modifiers={[{ name: 'offset', options: { offset: [0, 10] } }]}
      >
        <RecordPreviewCard
          rows={preview.rows}
          loading={preview.loading}
          error={preview.error}
          emptyText={preview.emptyText}
          {...preview.cardProps}
        />
      </Popper>
    </Box>
  );
}
