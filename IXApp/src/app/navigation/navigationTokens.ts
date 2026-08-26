import { APP_FONT_FAMILY } from '@shared/constants/fontFamilies';

export const navigationTokens = {
  expandedWidth: 249,
  collapsedWidth: 48,
  headerHeight: 42,
  itemHeight: 42,
  iconSize: 17,
  chevronSize: 15,
  horizontalPadding: 14,
  iconTextGap: 12,
  fontFamily: APP_FONT_FAMILY,
  fontSize: 13,
  fontWeight: 400,
  background: '#f7f7f7',
  text: '#1b1a19',
  icon: '#201f1e',
  mutedIcon: '#605e5c',
  border: '#edebe9',
  hover: '#edebe9',
  pressed: '#e1dfdd',
  selected: '#e5efff',
  selectedBar: '#2b67f6',
} as const;
