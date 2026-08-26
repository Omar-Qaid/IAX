export interface RenderableOption {
  value: string;
  label: string;
  sendsNotification?: boolean;
  requiresAttachment?: boolean;
  revealsControls?: boolean;
}

export interface RenderableValidation {
  type: string;
  expression?: string | null;
  value?: string | null;
  errorMessage?: string;
}

export interface RenderableControl {
  label: string;
  hideLabel?: boolean;
  compact?: boolean;
  controlType: string;
  labelColor?: string | null;
  required?: boolean;
  readOnly?: boolean;
  defaultValue?: string | null;
  options?: RenderableOption[];
  validations?: RenderableValidation[];
}
