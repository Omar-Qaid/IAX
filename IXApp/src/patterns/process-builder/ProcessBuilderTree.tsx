import React from 'react';
import { Box, ButtonBase, Collapse, Typography } from '@mui/material';
import AccountTreeOutlined from '@mui/icons-material/AccountTreeOutlined';
import DescriptionOutlined from '@mui/icons-material/DescriptionOutlined';
import FormatListNumberedOutlined from '@mui/icons-material/FormatListNumberedOutlined';
import TaskAltOutlined from '@mui/icons-material/TaskAltOutlined';
import type { ProcessBuilderNode } from './types';

const icons = {
  process: <AccountTreeOutlined fontSize="small" />,
  variable: <DescriptionOutlined fontSize="small" />,
  step: <FormatListNumberedOutlined fontSize="small" />,
  activity: <TaskAltOutlined fontSize="small" />,
};

export function ProcessBuilderTree({ nodes, selectedId, onSelect, depth = 0 }: {
  nodes: ProcessBuilderNode[];
  selectedId: string | null;
  onSelect: (id: string) => void;
  depth?: number;
}): React.ReactElement {
  return <Box role={depth === 0 ? 'tree' : 'group'} aria-label={depth === 0 ? 'Process structure' : undefined}>
    {nodes.map((node) => <Box key={node.id}>
      <ButtonBase
        role="treeitem"
        aria-selected={node.id === selectedId}
        onClick={() => onSelect(node.id)}
        sx={{ width: '100%', justifyContent: 'flex-start', gap: 1, minHeight: 34, px: 1, pl: 1 + depth * 2.5, borderLeft: '3px solid', borderLeftColor: node.id === selectedId ? 'primary.main' : 'transparent', bgcolor: node.id === selectedId ? 'action.selected' : 'transparent', '&:hover': { bgcolor: 'action.hover' } }}
      >
        <Box sx={{ display: 'flex', color: node.id === selectedId ? 'primary.main' : 'text.secondary' }}>{icons[node.kind]}</Box>
        <Box sx={{ minWidth: 0, textAlign: 'start' }}>
          <Typography noWrap sx={{ fontSize: '0.75rem', fontWeight: node.id === selectedId ? 700 : 500 }}>{node.label}</Typography>
          {node.secondaryText && <Typography noWrap color="text.secondary" sx={{ fontSize: '0.625rem' }}>{node.secondaryText}</Typography>}
        </Box>
      </ButtonBase>
      {node.children?.length ? <Collapse in timeout={0}><ProcessBuilderTree nodes={node.children} selectedId={selectedId} onSelect={onSelect} depth={depth + 1} /></Collapse> : null}
    </Box>)}
  </Box>;
}
