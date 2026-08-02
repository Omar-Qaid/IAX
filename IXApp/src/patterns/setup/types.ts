import type { ReactNode } from 'react';

export type SetupValue = string | number | boolean;
export type SetupValues = Record<string, SetupValue>;

export interface SetupNavigationItem {
  id: string;
  label: string;
}

export interface SetupFieldOption {
  value: string;
  label: string;
}

export interface SetupFieldConfig {
  name: string;
  label: string;
  type: 'boolean' | 'select' | 'number' | 'text';
  options?: SetupFieldOption[];
  disabled?: boolean;
  min?: number;
  max?: number;
  width?: number;
}

export interface SetupSectionConfig {
  id: string;
  title: string;
  fields: SetupFieldConfig[];
  defaultExpanded?: boolean;
}

export interface SetupPageProps {
  title: string;
  viewLabel: string;
  navigationItems: SetupNavigationItem[];
  sections: SetupSectionConfig[];
  initialValues: SetupValues;
  saveLabel: string;
  optionsLabel: string;
  yesLabel: string;
  noLabel: string;
  savedMessage?: string;
  headerContent?: ReactNode;
  onSave?: (values: SetupValues) => void | Promise<void>;
}
