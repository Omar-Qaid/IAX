import React from 'react';
import { Accordion, AccordionDetails, AccordionSummary, Box, MenuItem, Switch, TextField, Typography } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import type { DetailSectionConfig, DetailValue, DetailValues } from './types';

export interface ListDetailsLayoutProps {
  listPane: React.ReactNode;
  header: React.ReactNode;
  sections: DetailSectionConfig[];
  values: DetailValues;
  editing: boolean;
  yesLabel: string;
  noLabel: string;
  onChange: (name: string, value: DetailValue) => void;
  listWidth?: number;
}

export function ListDetailsLayout({ listPane, header, sections, values, editing, yesLabel, noLabel, onChange, listWidth = 264 }: ListDetailsLayoutProps): React.ReactElement {
  return <Box sx={{ minHeight: 0, flex: 1, display: 'flex', gap: 2, overflow: 'hidden' }}>
    <Box sx={{ width: { xs: '100%', md: listWidth }, flexShrink: 0, display: { xs: 'none', md: 'block' }, bgcolor: 'background.paper', border: 1, borderColor: 'divider', borderRadius: 1, overflow: 'hidden', boxShadow: '0 1px 4px rgba(0,0,0,.12)' }}>{listPane}</Box>
    <Box sx={{ minWidth: 0, flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
      {header}
      <Box sx={{ minHeight: 0, flex: 1, overflowY: 'auto', pr: 0.5 }}>
        {sections.map((section) => <Accordion key={section.id} defaultExpanded={section.defaultExpanded ?? true} disableGutters elevation={0} sx={{ mb: 0.5, border: 1, borderColor: 'divider', borderRadius: '7px !important', '&::before': { display: 'none' } }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon sx={{ fontSize: 17 }} />} sx={{ px: 1.25, minHeight: 42, borderBottom: 1, borderColor: 'divider', '&.Mui-expanded': { minHeight: 42 }, '& .MuiAccordionSummary-content, & .MuiAccordionSummary-content.Mui-expanded': { my: 0.75 }, '& .MuiAccordionSummary-expandIconWrapper': { border: 1, borderColor: 'divider', borderRadius: 0.5, p: 0.2 } }}><Typography sx={{ fontSize: '0.875rem', fontWeight: 600 }}>{section.title}</Typography></AccordionSummary>
          <AccordionDetails sx={{ p: 1.25 }}>
            {section.link && <Box sx={{ mb: 1 }}>{section.link}</Box>}
            <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2,1fr)', lg: `repeat(${Math.min(5, section.groups.length)}, minmax(150px,1fr))` }, gap: 3 }}>
              {section.groups.map((group) => <Box key={group.id} sx={{ minWidth: 0 }}>
                {group.title && <Typography sx={{ mb: 1, fontSize: '0.6875rem', fontWeight: 700, textTransform: 'uppercase' }}>{group.title}</Typography>}
                <Box sx={{ display: 'grid', gap: 1.1 }}>{group.fields.map((field) => {
                  const value = values[field.name]; const disabled = !editing || field.disabled || field.type === 'display';
                  return <Box key={field.name} sx={{ minWidth: 0 }}><Typography noWrap title={field.label} sx={{ mb: 0.25, fontSize: '0.6875rem', color: 'text.secondary' }}>{field.label}</Typography>
                    {field.type === 'boolean' ? <Box sx={{ display: 'flex', alignItems: 'center', height: 28 }}><Switch size="small" checked={Boolean(value)} disabled={disabled} onChange={(_, checked) => onChange(field.name, checked)} sx={{ ml: -0.75, mr: 0.25, '& .MuiSwitch-thumb': { width: 13, height: 13 } }} /><Typography sx={{ fontSize: '0.75rem' }}>{value ? yesLabel : noLabel}</Typography></Box>
                    : field.type === 'select' ? <TextField select value={value ?? ''} disabled={disabled} onChange={(event) => onChange(field.name, event.target.value)} sx={fieldSx}>{(field.options ?? []).map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}</TextField>
                    : <TextField type={field.type === 'number' ? 'number' : 'text'} value={value ?? ''} disabled={disabled} onChange={(event) => onChange(field.name, field.type === 'number' ? Number(event.target.value) : event.target.value)} sx={fieldSx} />}
                  </Box>;
                })}</Box>
              </Box>)}
            </Box>
          </AccordionDetails>
        </Accordion>)}
      </Box>
    </Box>
  </Box>;
}

const fieldSx = { width: '100%', '& .MuiInputBase-root': { height: 29, borderRadius: 0.5, fontSize: '0.75rem' }, '& .MuiInputBase-input': { px: 0.75, py: 0.5 }, '& .MuiInputBase-root.Mui-disabled': { bgcolor: 'transparent' }, '& .MuiInputBase-input.Mui-disabled': { WebkitTextFillColor: 'currentColor' } };

