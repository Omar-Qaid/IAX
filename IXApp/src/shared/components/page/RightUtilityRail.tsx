import React from 'react';
import { Box, IconButton, SvgIcon, Tooltip, type SvgIconProps } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';

export const RIGHT_UTILITY_RAIL_WIDTH = d365.utilityRailWidth;

const FilterRailIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M3 5h18l-7 8v5l-4 2v-7L3 5Z" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinejoin="round" /></SvgIcon>;
const InfoRailIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18Zm0-10v6m0-9.25v.5" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" /></SvgIcon>;

export interface RightUtilityRailProps {
  filterLabel: string;
  informationLabel: string;
  onFilter?: () => void;
  onInformation?: () => void;
  filterActive?: boolean;
  informationActive?: boolean;
  showInformation?: boolean;
  disabled?: boolean;
}

export const RightUtilityRail: React.FC<RightUtilityRailProps> = ({ filterLabel, informationLabel, onFilter, onInformation, filterActive = false, informationActive = false, showInformation = true, disabled = false }) => (
  <Box
    component="aside"
    aria-label={informationLabel}
    sx={{ position: 'absolute', insetInlineEnd: 0, top: 0, bottom: 0, width: RIGHT_UTILITY_RAIL_WIDTH, minHeight: 0, boxSizing: 'border-box', overflow: 'hidden', borderInlineStart: `1px solid ${d365.border}`, bgcolor: d365.surface, display: { xs: 'none', lg: 'flex' }, alignItems: 'center', flexDirection: 'column', pt: 0.25, gap: 0.5, zIndex: 3 }}
  >
    <Tooltip title={filterLabel} placement="left"><span><IconButton disabled={disabled} size="small" aria-label={filterLabel} aria-pressed={filterActive} onClick={onFilter} sx={{ border: '1px solid', borderColor: filterActive ? 'primary.main' : 'transparent', color: filterActive ? 'primary.main' : 'text.primary', borderRadius: 0.5 }}><FilterRailIcon sx={{ fontSize: 18 }} /></IconButton></span></Tooltip>
    {showInformation && <Tooltip title={informationLabel} placement="left"><span><IconButton disabled={disabled} size="small" aria-label={informationLabel} aria-pressed={informationActive} onClick={onInformation} sx={{ border: '1px solid', borderColor: informationActive ? 'primary.main' : 'transparent', color: informationActive ? 'primary.main' : 'text.primary', borderRadius: 0.5 }}><InfoRailIcon sx={{ fontSize: 18 }} /></IconButton></span></Tooltip>}
  </Box>
);
