import type { FieldValues } from 'react-hook-form';
import { LookupField } from './LookupField';
import type { LookupFieldProps } from './types';

/** Lookup control preset for data-grid cells: compact and labeled by the column header. */
export function DataGridLookupField<TFieldValues extends FieldValues = FieldValues>(
  props: Omit<LookupFieldProps<TFieldValues>, 'displayMode'>
): React.ReactElement {
  return <LookupField {...props} displayMode="select" masterRoute={undefined} externalLabel compact />;
}
