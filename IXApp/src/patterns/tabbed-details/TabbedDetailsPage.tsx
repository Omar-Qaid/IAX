import { useEffect, useState, type ReactNode, type SyntheticEvent } from 'react';
import { Box, Tab, Tabs } from '@mui/material';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';

export interface DetailsTab { id: string; label: string; content: ReactNode; disabled?: boolean }
export interface TabbedDetailsPageProps { title: string; subtitle?: string; tabs: DetailsTab[]; initialTabId?: string; actions?: ReactNode; onTabChange?: (id: string) => void }
export function TabbedDetailsPage({ title, subtitle, tabs, initialTabId, actions, onTabChange }: TabbedDetailsPageProps) {
  const firstEnabled = tabs.find(tab => !tab.disabled)?.id ?? '';
  const [activeId, setActiveId] = useState(initialTabId ?? firstEnabled);
  useEffect(() => { if (!tabs.some(tab => tab.id === activeId && !tab.disabled)) setActiveId(firstEnabled); }, [activeId, firstEnabled, tabs]);
  const handleChange = (_: SyntheticEvent, id: string) => { setActiveId(id); onTabChange?.(id); };
  const activeTab = tabs.find(tab => tab.id === activeId);
  return <PageContainer>
    <PageHeader title={title} subtitle={subtitle} actions={actions} />
    <Tabs value={activeId} onChange={handleChange} variant="scrollable" scrollButtons="auto" aria-label={title}>
      {tabs.map(tab => <Tab key={tab.id} value={tab.id} label={tab.label} disabled={tab.disabled} id={`tab-${tab.id}`} aria-controls={`panel-${tab.id}`} />)}
    </Tabs>
    <Box role="tabpanel" id={`panel-${activeId}`} aria-labelledby={`tab-${activeId}`} sx={{ pt: 2 }}>{activeTab?.content}</Box>
  </PageContainer>;
}
