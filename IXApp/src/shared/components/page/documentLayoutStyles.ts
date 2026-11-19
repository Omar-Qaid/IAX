import { d365 } from '@shared/constants/enterpriseUiTokens';

/** Shared sales-order document page measurements and font inheritance. */
export const documentTypographySx = {
  fontFamily: d365.fontFamily,
  '& .MuiTypography-root, & .MuiButton-root, & .MuiInputBase-root, & input, & textarea': {
    fontFamily: `${d365.fontFamily} !important`,
  },
} as const;

export const documentBodySx = {
  display: 'flex',
  flex: 1,
  height: '100%',
  minHeight: 0,
  gap: '23px',
  marginInlineStart: '1px',
  overflow: 'hidden',
  position: 'relative',
  alignItems: 'stretch',
} as const;
