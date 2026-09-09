import React, { useState } from 'react';
import {
  Box,
  IconButton,
  InputAdornment,
  List,
  ListItemButton,
  TextField,
  Typography,
} from '@mui/material';
import { PageContainer } from '@shared/components/page/PageContainer';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import MenuIcon from '@mui/icons-material/Menu';
import SearchIcon from '@mui/icons-material/Search';
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
  backAction,
  endActions,
  viewLabel,
  navigationLabel = filterLabel,
  tabs,
}: MasterDetailPageProps): React.ReactElement {
  const [navigationOpen, setNavigationOpen] = useState(true);
  const [filter, setFilter] = useState('');
  const [navigationWidth, setNavigationWidth] = useState(280);
  const visible = records.filter((record) =>
    `${record.title} ${record.subtitle ?? ''} ${record.description ?? ''}`
      .toLocaleLowerCase()
      .includes(filter.trim().toLocaleLowerCase())
  );
  return (
    <PageContainer sx={{ gap: 0, height: '100%', minHeight: 0 }}>
      <ActionPane endActions={endActions}>
        {backAction}
        <IconButton
          aria-label={navigationLabel}
          aria-expanded={navigationOpen}
          onClick={() => setNavigationOpen((open) => !open)}
          sx={{
            bgcolor: navigationOpen ? 'primary.main' : undefined,
            color: navigationOpen ? 'primary.contrastText' : 'primary.main',
            borderRadius: 0.5,
            width: 31,
            height: 31,
            mx: 0.5,
            '&:hover': { bgcolor: navigationOpen ? 'primary.dark' : 'action.hover' },
          }}
        >
          <MenuIcon fontSize="small" />
        </IconButton>
        {actionPane}
      </ActionPane>
      <Box
        sx={{
          display: 'flex',
          gap: 1,
          alignItems: 'stretch',
          minHeight: 0,
          flex: 1,
          height: 'calc(100dvh - 180px)',
        }}
      >
        <Box
          component="aside"
          aria-label={navigationLabel}
          sx={{
            display: navigationOpen ? 'flex' : 'none',
            flexDirection: 'column',
            overflow: 'hidden',
            width: { xs: 170, md: navigationWidth },
            flexShrink: 0,
            border: 1,
            borderColor: 'divider',
            bgcolor: 'background.paper',
            borderRadius: 2,
            boxShadow: '0 2px 6px rgba(0,0,0,0.12)',
          }}
        >
          <Box sx={{ p: 1 }}>
            <TextField
              fullWidth
              size="small"
              placeholder={filterLabel}
              value={filter}
              onChange={(event) => setFilter(event.target.value)}
              slotProps={{
                htmlInput: { 'aria-label': filterLabel },
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon sx={{ fontSize: 16 }} />
                    </InputAdornment>
                  ),
                },
              }}
              sx={{ '& .MuiInputBase-root': { height: 30, fontSize: 12 } }}
            />
          </Box>
          <List disablePadding sx={{ flex: 1, minHeight: 0, overflow: 'auto' }}>
            {visible.map((record) => (
              <ListItemButton
                key={record.id}
                selected={selectedId === record.id}
                onClick={() => onSelect(record.id)}
                sx={{
                  display: 'block',
                  py: 1,
                  px: 1.25,
                  minHeight: 88,
                  '&.Mui-selected': { bgcolor: 'action.selected', color: 'primary.main' },
                  borderBottom: 1,
                  borderColor: 'divider',
                  borderInlineStart: '4px solid',
                  borderInlineStartColor: selectedId === record.id ? 'primary.main' : 'transparent',
                }}
              >
                <Typography noWrap sx={{ fontWeight: 500, fontSize: 18 }}>
                  {record.title}
                </Typography>
                <Typography
                  variant="caption"
                  noWrap
                  sx={{ display: 'block', color: 'primary.main', fontSize: 12 }}
                >
                  {record.subtitle}
                </Typography>
                <Typography variant="caption" noWrap sx={{ display: 'block', fontSize: 12 }}>
                  {record.description}
                </Typography>
              </ListItemButton>
            ))}
            {!visible.length && (
              <Typography sx={{ p: 1.5 }} variant="body2">
                {emptyLabel}
              </Typography>
            )}
          </List>
        </Box>
        <Box
          role="separator"
          tabIndex={0}
          aria-label={filterLabel}
          aria-orientation="vertical"
          aria-valuemin={180}
          aria-valuemax={420}
          aria-valuenow={navigationWidth}
          onKeyDown={(event) => {
            if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
              event.preventDefault();
              setNavigationWidth((width) =>
                Math.max(180, Math.min(420, width + (event.key === 'ArrowRight' ? 20 : -20)))
              );
            }
          }}
          sx={{
            width: 5,
            flexShrink: 0,
            bgcolor: 'transparent',
            cursor: 'col-resize',
            display: { xs: 'none', md: navigationOpen ? 'block' : 'none' },
            '&:focus': { bgcolor: 'primary.main' },
          }}
        />
        <Box component="main" sx={{ flex: 1, minWidth: 0, overflowY: 'auto', px: 1, pb: 2 }}>
          {subtitle && (
            <Typography variant="body2" color="primary" sx={{ mb: 1 }}>
              {subtitle}
              {viewLabel && (
                <Box
                  component="span"
                  sx={{
                    color: 'text.primary',
                    borderInlineStart: 1,
                    borderColor: 'divider',
                    marginInlineStart: 1.5,
                    paddingInlineStart: 1.5,
                  }}
                >
                  {viewLabel}
                </Box>
              )}
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
            <Typography variant="h5" noWrap sx={{ fontWeight: 500, fontSize: 18 }}>
              {title}
            </Typography>
            {status}
          </Box>
          {tabs}
          {children}
        </Box>
      </Box>
    </PageContainer>
  );
}
