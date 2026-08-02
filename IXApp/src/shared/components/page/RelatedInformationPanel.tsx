import React, { useState } from 'react';
import { Box, Collapse, IconButton, Stack, Typography } from '@mui/material';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';

export interface RelatedInformationSection {
  id: string;
  label: string;
  content?: React.ReactNode;
  defaultExpanded?: boolean;
}

export interface RelatedInformationPanelProps {
  title: string;
  sections: RelatedInformationSection[];
}

export const RelatedInformationPanel: React.FC<RelatedInformationPanelProps> = ({ title, sections }) => {
  const [expanded, setExpanded] = useState(() => new Set(sections.filter((section) => section.defaultExpanded).map((section) => section.id)));
  const toggle = (id: string) => setExpanded((current) => {
    const next = new Set(current);
    if (next.has(id)) next.delete(id); else next.add(id);
    return next;
  });

  return (
    <Box sx={{ width: 245, height: '100%', minHeight: 0, boxSizing: 'border-box', overflowY: 'auto', flexShrink: 0, bgcolor: 'background.paper', border: (theme) => `1px solid ${theme.palette.divider}`, borderRadius: 1, boxShadow: 2, p: 1 }}>
      <Typography component="h2" sx={{ px: 0.25, pb: 0.75, fontSize: '1rem', fontWeight: 600, borderBottom: (theme) => `1px solid ${theme.palette.divider}` }}>{title}</Typography>
      {sections.map((section) => {
        const open = expanded.has(section.id);
        return (
          <Box key={section.id} sx={{ borderBottom: (theme) => `1px solid ${theme.palette.divider}` }}>
            <Stack direction="row" sx={{ minHeight: 40, alignItems: 'center', justifyContent: 'space-between' }}>
              <Typography sx={{ pl: 0.25, fontSize: '0.8125rem', fontWeight: open ? 600 : 500 }}>{section.label}</Typography>
              <IconButton size="small" aria-label={section.label} aria-expanded={open} onClick={() => toggle(section.id)} sx={{ border: '1px solid', borderColor: 'divider', borderRadius: 0.5, p: 0.25 }}>
                {open ? <KeyboardArrowUpIcon sx={{ fontSize: 17 }} /> : <KeyboardArrowDownIcon sx={{ fontSize: 17 }} />}
              </IconButton>
            </Stack>
            <Collapse in={open} unmountOnExit>
              <Box sx={{ px: 0.75, pb: 2.5, pt: 0.5, minHeight: section.content ? 95 : 48 }}>
                {section.content}
              </Box>
            </Collapse>
          </Box>
        );
      })}
    </Box>
  );
};
