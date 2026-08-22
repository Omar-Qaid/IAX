import type { BuilderControl } from '../types/processBuilderTypes';
import { DynamicControlRenderer } from '@modules/workflow/components/DynamicControlRenderer';
export function ControlPreview({ control }: { control: BuilderControl }) {
  return <DynamicControlRenderer preview control={{
    label: control.label, controlType: control.type, required: control.required,
    readOnly: control.readOnly, defaultValue: control.defaultValue,
    options: (control.options.length ? control.options : ['Option']).map((option) => ({ value: option, label: option })),
    validations: control.validations.filter((validation) => validation.active).map((validation) => ({
      type: validation.type,
      expression: validation.secondaryValue,
      value: validation.value,
      errorMessage: validation.message,
    })),
  }} value={control.defaultValue} onChange={() => undefined} />;
}
