import React from 'react';
import { Badge, IconButton, SvgIcon, Tooltip, type SvgIconProps } from '@mui/material';

const PersonalizeIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="m12 3 3 3-3 3-3-3 3-3Zm-6 6 3 3-3 3-3-3 3-3Zm12 0 3 3-3 3-3-3 3-3Zm-6 6 3 3-3 3-3-3 3-3Z" fill="none" stroke="currentColor" strokeWidth="1.4" /></SvgIcon>;
const GuideIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M5 5.5c3-1 5-.5 7 1.2v12c-2-1.7-4-2.2-7-1.2v-12Zm14 0c-3-1-5-.5-7 1.2v12c2-1.7 4-2.2 7-1.2v-12Z" fill="none" stroke="currentColor" strokeWidth="1.4" /></SvgIcon>;
const NotificationIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M18 16v-5a6 6 0 0 0-12 0v5l-2 2h16l-2-2Zm-8 4h4" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round" /></SvgIcon>;
const RefreshIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M19 7V3l-2 2a8 8 0 1 0 2.1 8" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" /></SvgIcon>;
const OpenWindowIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M14 5h5v5m0-5-8 8M18 13v6H5V6h6" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round" /></SvgIcon>;

export interface EnterpriseCommandUtilitiesProps {
  personalizeLabel: string;
  guideLabel: string;
  notificationsLabel: string;
  refreshLabel: string;
  openWindowLabel: string;
  notificationCount?: number;
  onRefresh?: () => void;
}

export const EnterpriseCommandUtilities: React.FC<EnterpriseCommandUtilitiesProps> = ({ personalizeLabel, guideLabel, notificationsLabel, refreshLabel, openWindowLabel, notificationCount = 0, onRefresh }) => {
  const sx = { p: 0.5, color: 'primary.main', borderRadius: 0.5, '&:hover': { bgcolor: 'action.hover' } };
  return <>
    <Tooltip title={personalizeLabel}><IconButton size="small" aria-label={personalizeLabel} sx={sx}><PersonalizeIcon sx={{ fontSize: 17 }} /></IconButton></Tooltip>
    <Tooltip title={guideLabel}><IconButton size="small" aria-label={guideLabel} sx={sx}><GuideIcon sx={{ fontSize: 17 }} /></IconButton></Tooltip>
    <Tooltip title={notificationsLabel}><IconButton size="small" aria-label={notificationsLabel} sx={sx}><Badge badgeContent={notificationCount} color="primary" sx={{ '& .MuiBadge-badge': { fontSize: 9, minWidth: 15, height: 15 } }}><NotificationIcon sx={{ fontSize: 17 }} /></Badge></IconButton></Tooltip>
    <Tooltip title={refreshLabel}><IconButton size="small" aria-label={refreshLabel} onClick={onRefresh} sx={sx}><RefreshIcon sx={{ fontSize: 17 }} /></IconButton></Tooltip>
    <Tooltip title={openWindowLabel}><IconButton size="small" aria-label={openWindowLabel} sx={{ ...sx, color: 'text.disabled' }}><OpenWindowIcon sx={{ fontSize: 17 }} /></IconButton></Tooltip>
  </>;
};
