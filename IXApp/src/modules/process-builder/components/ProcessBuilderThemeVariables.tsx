import React from 'react';
import { GlobalStyles } from '@mui/material';
import { processBuilderColorVariables } from './processBuilderTokens';

/**
 * Publishes the Process Builder colors on :root for the active theme. Global,
 * not on the page box, so dialogs, menus and drawers (rendered in portals)
 * get the same colors.
 */
export function ProcessBuilderThemeVariables(): React.ReactElement {
  return <GlobalStyles styles={(theme) => ({ ':root': processBuilderColorVariables(theme) })} />;
}
