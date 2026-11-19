import type { FieldValues } from 'react-hook-form';
import { LookupField } from './LookupField';
import type { LookupFieldProps } from './types';

/**
 * Lookup control preset for data-grid cells: compact, labeled by the column header,
 * showing the code like the read-only cell, with a code/name list, type-ahead
 * (Enter or Tab takes the first match) and paging for large lists.
 */
export function DataGridLookupField<TFieldValues extends FieldValues = FieldValues>(
  props: Omit<LookupFieldProps<TFieldValues>, 'displayMode'>
): React.ReactElement {
  return (
    <LookupField
      {...props}
      displayMode="select"
      masterRoute={undefined}
      externalLabel
      compact
      gridCell
    />
  );
}
