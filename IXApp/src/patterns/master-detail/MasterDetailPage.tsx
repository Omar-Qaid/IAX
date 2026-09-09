import React, { useState } from 'react';
import { Box, List, ListItemButton, TextField, Typography } from '@mui/material';
import { PageContainer } from '@shared/components/page/PageContainer';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import type { MasterDetailPageProps } from './types';

export function MasterDetailPage({
  records,
  selectedId,
  onSelect,
  title,
  subtitle,
  status,
  filterLabel,
  emptyLabel,
  actionPane,
  children,
}: MasterDetailPageProps): React.ReactElement {
  const [filter, setFilter] = useState('');
  const [navigationWidth, setNavigationWidth] = useState(280);
  const visible = records.filter((record) =>
    `${record.title} ${record.subtitle ?? ''} ${record.description ?? ''}`
      .toLocaleLowerCase()
      .includes(filter.trim().toLocaleLowerCase())
  );
  return (
    <PageContainer>
      <ActionPane>{actionPane}</ActionPane>
      <Box sx={{ display: 'flex', gap: 2, alignItems: 'stretch', minHeight: 560 }}>
        <Box
          component="aside"
          sx={{
            width: { xs: 170, md: navigationWidth },
            flexShrink: 0,
            border: 1,
            borderColor: 'divider',
            bgcolor: 'background.paper',
            borderRadius: 0.5,
          }}
        >
          <Box sx={{ p: 1 }}>
            <TextField
              fullWidth
              size="small"
              placeholder={filterLabel}
              value={filter}
              onChange={(event) => setFilter(event.target.value)}
              slotProps={{ htmlInput: { 'aria-label': filterLabel } }}
            />
          </Box>
          <List disablePadding sx={{ maxHeight: 'calc(100vh - 260px)', overflow: 'auto' }}>
            {visible.map((record) => (
              <ListItemButton
                key={record.id}
                selected={selectedId === record.id}
                onClick={() => onSelect(record.id)}
                sx={{
                  display: 'block',
                  py: 1.25,
                  borderBottom: 1,
                  borderColor: 'divider',
                  borderInlineStart: '4px solid',
                  borderInlineStartColor: selectedId === record.id ? 'primary.main' : 'transparent',
                }}
              >
                <Typography sx={{ fontWeight: 600 }}>{record.title}</Typography>
                <Typography variant="caption" sx={{ display: 'block' }}>
                  {record.subtitle}
                </Typography>
                <Typography variant="caption">{record.description}</Typography>
              </ListItemButton>
            ))}
            {!visible.length && (
              <Typography sx={{ p: 1.5 }} variant="body2">
                {emptyLabel}
              </Typography>
            )}
          </List>
        </Box>
        <Box role="separator" tabIndex={0} aria-label={filterLabel} aria-orientation="vertical" aria-valuemin={180} aria-valuemax={420} aria-valuenow={navigationWidth}
          onKeyDown={(event) => { if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') { event.preventDefault(); setNavigationWidth((width) => Math.max(180, Math.min(420, width + (event.key === 'ArrowRight' ? 20 : -20)))); } }}
          sx={{ width: 5, flexShrink: 0, bgcolor: 'divider', cursor: 'col-resize', display: { xs: 'none', md: 'block' }, '&:focus': { bgcolor: 'primary.main' } }} />
        <Box component="main" sx={{ flex: 1, minWidth: 0 }}>
          {subtitle && (
            <Typography variant="body2" color="primary" sx={{ mb: 1 }}>
              {subtitle}
            </Typography>
          )}
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: 2,
              mb: 1,
            }}
          >
            <Typography variant="h5" sx={{ fontWeight: 600 }}>
              {title}
            </Typography>
            {status}
          </Box>
          {children}
        </Box>
      </Box>
    </PageContainer>
  );
}
