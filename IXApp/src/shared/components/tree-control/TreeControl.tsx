import React, { useMemo, useState } from 'react';
import { Box, Button, ButtonBase, Collapse, Typography } from '@mui/material';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowRightIcon from '@mui/icons-material/KeyboardArrowRight';
import type { TreeControlProps } from './types';

export function TreeControl<T>({ nodes, config }: TreeControlProps<T>): React.ReactElement {
  const [expandedIds, setExpandedIds] = useState<Set<string>>(
    () => new Set(config.initialExpandedIds ?? [])
  );

  const branchIds = useMemo(() => {
    const ids: string[] = [];
    const visit = (items: readonly T[]) => {
      items.forEach((node) => {
        const children = config.getChildren(node);
        const branch = config.isBranch?.(node) ?? children.length > 0;
        if (branch) ids.push(config.getId(node));
        if (children.length > 0) visit(children);
      });
    };
    visit(nodes);
    return ids;
  }, [config, nodes]);

  const allExpanded = branchIds.length > 0 && branchIds.every((id) => expandedIds.has(id));
  const toggleAll = () => setExpandedIds(allExpanded ? new Set() : new Set(branchIds));
  const toggleNode = (id: string) =>
    setExpandedIds((current) => {
      const next = new Set(current);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  const renderNodes = (items: readonly T[], depth: number): React.ReactNode =>
    items.map((node) => {
      const id = config.getId(node);
      const children = config.getChildren(node);
      const branch = config.isBranch?.(node) ?? children.length > 0;
      const expanded = branch && expandedIds.has(id);
      const selected = id === config.selectedId;
      const contentId = `tree-control-${id}`;

      return (
        <Box key={id} role="treeitem" aria-expanded={branch ? expanded : undefined}>
          {branch ? (
            <ButtonBase
              aria-controls={contentId}
              onClick={() => toggleNode(id)}
              sx={{
                display: 'flex',
                justifyContent: 'flex-start',
                width: '100%',
                minHeight: 28,
                gap: 0.75,
                marginInlineStart: depth ? `${depth * 10}px` : 0,
                px: 0.5,
                bgcolor: selected ? '#dbe7fb' : undefined,
                textAlign: 'start',
                '&:hover': { bgcolor: selected ? '#dbe7fb' : 'action.hover' },
              }}
            >
              {expanded ? (
                <KeyboardArrowDownIcon sx={{ fontSize: 16 }} />
              ) : (
                <KeyboardArrowRightIcon sx={{ fontSize: 16 }} />
              )}
              <Typography component="span" variant="body2" sx={{ fontSize: 13 }}>
                {config.getLabel(node)}
              </Typography>
            </ButtonBase>
          ) : (
            <Box
              sx={{
                minHeight: 28,
                display: 'flex',
                alignItems: 'center',
                marginInlineStart: `${depth * 10}px`,
                paddingInlineStart: `${depth * 34}px`,
                fontSize: 13,
                cursor: 'default',
                bgcolor: selected ? '#dbe7fb' : undefined,
                '&:hover': { bgcolor: selected ? '#dbe7fb' : 'action.hover' },
              }}
            >
              {config.getLabel(node)}
            </Box>
          )}
          {branch && (
            <Collapse in={expanded} timeout="auto" unmountOnExit>
              <Box id={contentId} role="group">
                {children.length > 0
                  ? renderNodes(children, depth + 1)
                  : config.emptyChildrenLabel && (
                      <Typography
                        color="text.secondary"
                        variant="body2"
                        sx={{
                          minHeight: 28,
                          display: 'flex',
                          alignItems: 'center',
                          marginInlineStart: `${(depth + 1) * 10}px`,
                          paddingInlineStart: `${(depth + 1) * 34}px`,
                          fontSize: 13,
                        }}
                      >
                        {config.emptyChildrenLabel}
                      </Typography>
                    )}
              </Box>
            </Collapse>
          )}
        </Box>
      );
    });

  return (
    <Box sx={{ display: 'grid', gap: 0.25 }}>
      <Box>
        <Button
          onClick={toggleAll}
          size="small"
          sx={{ minWidth: 0, px: 0.5, py: 0.25, textTransform: 'none', fontSize: 12 }}
        >
          {allExpanded
            ? (config.collapseAllLabel ?? 'Collapse')
            : (config.expandAllLabel ?? 'Expand')}
        </Button>
      </Box>
      <Box
        aria-label={config.ariaLabel}
        role="tree"
        sx={{ maxHeight: config.maxHeight ?? 198, overflowY: 'auto', pe: 0.5 }}
      >
        {renderNodes(nodes, 0)}
      </Box>
    </Box>
  );
}
