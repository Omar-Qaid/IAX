import type { SxProps, Theme } from '@mui/material/styles';

export interface ReportDesignerProps {
  /** Main layout content or child slots */
  children?: React.ReactNode;
  /** Accessible label for the region */
  ariaLabel?: string;
  className?: string;
  /** Overall container minimum height */
  minHeight?: number | string;
  /** Overall container height */
  height?: number | string;
  /** Height for the top toolbar header */
  toolbarHeight?: number | string;
  /** Width for left sidebar (palette/components) when using slots */
  sidebarWidth?: number | string;
  /** Width for right properties panel when using slots */
  propertiesWidth?: number | string;
  /** Optional toolbar slot */
  toolbar?: React.ReactNode;
  /** Optional sidebar / component palette slot */
  sidebar?: React.ReactNode;
  /** Optional property inspector panel slot */
  properties?: React.ReactNode;
  /** Optional footer / status bar slot */
  footer?: React.ReactNode;
  /** Global loading overlay flag */
  isLoading?: boolean;
  /** Message displayed on loading overlay */
  loadingMessage?: string;
  /** Custom style overrides */
  sx?: SxProps<Theme>;
}
