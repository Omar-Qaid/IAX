import { memo } from 'react';
import { Tab, Tabs } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import { ProcessBuilderPalette } from './ProcessBuilderPalette';
import { ProcessBuilderTreePanel } from './ProcessBuilderTreePanel';
import { processBuilderTokens as tokens } from './processBuilderTokens';

export const ProcessBuilderNavigationPanel = memo(function ProcessBuilderNavigationPanel() {
  const { t } = useAppTranslation();
  const leftTab = useProcessBuilderStore((state) => state.leftTab);
  const setLeftTab = useProcessBuilderStore((state) => state.setLeftTab);
  return (
    <>
      <Tabs
        value={leftTab}
        onChange={(_, value: number) => setLeftTab(value)}
        variant="fullWidth"
        aria-label={t('wfProcessBuilder.navigation.label')}
        sx={{
          minHeight: 40,
          '& .MuiTab-root': {
            minHeight: 40,
            fontSize: tokens.fontSize.secondary,
            fontWeight: 600,
            color: tokens.textMuted,
          },
          '& .Mui-selected': { color: `${tokens.accent} !important` },
          '& .MuiTabs-indicator': { bgcolor: tokens.accent, height: 2 },
        }}
      >
        <Tab label={t('wfProcessBuilder.navigation.tree')} />
        <Tab label={t('wfProcessBuilder.navigation.palette')} />
      </Tabs>
      {leftTab === 0 ? <ProcessBuilderTreePanel /> : <ProcessBuilderPalette />}
    </>
  );
});
