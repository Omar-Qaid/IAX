import React from 'react';
import { Box, IconButton, InputAdornment, TextField, Typography } from '@mui/material';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import SearchIcon from '@mui/icons-material/Search';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import type { ListDetailRecord, ListDetailsNavigationContext } from '@patterns/list-details/types';
import type { ListDetailsTreeConfig } from './types';

interface TreeNavigationPaneProps<
  T extends ListDetailRecord,
> extends ListDetailsNavigationContext<T> {
  tree: ListDetailsTreeConfig<T>;
  getPrimaryText: (record: T) => string;
  getSecondaryText?: (record: T) => string;
}

export function TreeNavigationPane<T extends ListDetailRecord>({
  records,
  visibleRecords,
  selectedId,
  editing,
  loading,
  query,
  filterVisible,
  filterLabel,
  onQueryChange,
  onSelect,
  tree,
  getPrimaryText,
  getSecondaryText,
}: TreeNavigationPaneProps<T>): React.ReactElement {
  const recordsById = React.useMemo(
    () => new Map(records.map((record) => [record.id, record])),
    [records]
  );
  const parentIds = React.useMemo(() => {
    const ids = new Set<string>();
    records.forEach((record) => {
      const parentId = tree.getParentId(record);
      if (parentId) ids.add(parentId);
    });
    return ids;
  }, [records, tree]);
  const [expandedIds, setExpandedIds] = React.useState<Set<string>>(
    () =>
      new Set(
        tree.initiallyExpanded === 'all'
          ? parentIds
          : Array.isArray(tree.initiallyExpanded)
            ? tree.initiallyExpanded
            : []
      )
  );
  React.useEffect(() => {
    if (tree.initiallyExpanded !== 'all') return;
    setExpandedIds((current) => new Set([...current, ...parentIds]));
  }, [parentIds, tree.initiallyExpanded]);
  const includedIds = React.useMemo(() => {
    const included = new Set(visibleRecords.map((record) => record.id));
    for (const record of visibleRecords) {
      let parentId = tree.getParentId(record);
      const visited = new Set<string>();
      while (parentId && recordsById.has(parentId) && !visited.has(parentId)) {
        visited.add(parentId);
        included.add(parentId);
        parentId = tree.getParentId(recordsById.get(parentId)!);
      }
    }
    return included;
  }, [recordsById, tree, visibleRecords]);
  const childrenByParent = React.useMemo(() => {
    const children = new Map<string | null, T[]>();
    records.forEach((record) => {
      if (!includedIds.has(record.id)) return;
      const declaredParent = tree.getParentId(record);
      const parentId = declaredParent && includedIds.has(declaredParent) ? declaredParent : null;
      const siblings = children.get(parentId) ?? [];
      siblings.push(record);
      children.set(parentId, siblings);
    });
    if (tree.compare) children.forEach((children) => children.sort(tree.compare));
    return children;
  }, [includedIds, records, tree]);
  const searchExpandedIds = React.useMemo(() => {
    const ids = new Set<string>();
    if (!query.trim()) return ids;
    includedIds.forEach((id) => {
      let parentId = tree.getParentId(recordsById.get(id)!);
      while (parentId && recordsById.has(parentId) && !ids.has(parentId)) {
        ids.add(parentId);
        parentId = tree.getParentId(recordsById.get(parentId)!);
      }
    });
    return ids;
  }, [includedIds, query, recordsById, tree]);
  const isExpanded = (id: string) => expandedIds.has(id) || searchExpandedIds.has(id);
  const toggle = (id: string) =>
    setExpandedIds((current) => {
      const next = new Set(current);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  const renderNodes = (parentId: string | null, depth: number, ancestors: Set<string>) =>
    (childrenByParent.get(parentId) ?? []).map((record) => {
      if (ancestors.has(record.id)) return null;
      const children = childrenByParent.get(record.id) ?? [];
      const expanded = children.length > 0 && isExpanded(record.id);
      const label = tree.getLabel?.(record) ?? getPrimaryText(record);
      const secondary = tree.getSecondaryText?.(record) ?? getSecondaryText?.(record);
      const nextAncestors = new Set(ancestors).add(record.id);
      return (
        <React.Fragment key={record.id}>
          <Box
            role="treeitem"
            aria-selected={selectedId === record.id}
            aria-expanded={children.length ? expanded : undefined}
            aria-level={depth + 1}
            tabIndex={selectedId === record.id ? 0 : -1}
            onClick={() => !editing && onSelect(record)}
            onKeyDown={(event) => {
              if (editing) return;
              if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                onSelect(record);
              } else if (children.length && event.key === 'ArrowRight' && !expanded) {
                event.preventDefault();
                toggle(record.id);
              } else if (children.length && event.key === 'ArrowLeft' && expanded) {
                event.preventDefault();
                toggle(record.id);
              }
            }}
            sx={{
              minHeight: tree.rowHeight ?? 36,
              display: 'flex',
              alignItems: 'center',
              gap: 0.25,
              paddingInlineStart: `${8 + depth * (tree.indent ?? 18)}px`,
              paddingInlineEnd: 1,
              cursor: editing ? 'default' : 'pointer',
              bgcolor: selectedId === record.id ? '#d9e7ff' : 'transparent',
              color: selectedId === record.id ? d365.primary : d365.text,
              '&:hover': { bgcolor: selectedId === record.id ? '#d9e7ff' : '#f3f2f1' },
            }}
          >
            <Box sx={{ width: 24, flexShrink: 0 }}>
              {children.length > 0 && (
                <IconButton
                  size="small"
                  aria-label={`${expanded ? (tree.collapseLabel ?? 'Collapse') : (tree.expandLabel ?? 'Expand')} ${label}`}
                  onClick={(event) => {
                    event.stopPropagation();
                    toggle(record.id);
                  }}
                  sx={{ width: 24, height: 24 }}
                >
                  {expanded ? (
                    <ExpandMoreIcon fontSize="small" />
                  ) : (
                    <ChevronRightIcon fontSize="small" />
                  )}
                </IconButton>
              )}
            </Box>
            {tree.renderIcon?.(record)}
            <Box sx={{ minWidth: 0, flex: 1 }}>
              <Typography
                noWrap
                sx={{ fontSize: d365.fontSize, fontWeight: children.length ? 600 : 400 }}
              >
                {label}
              </Typography>
              {secondary && (
                <Typography noWrap sx={{ fontSize: 10, color: 'text.secondary' }}>
                  {secondary}
                </Typography>
              )}
            </Box>
          </Box>
          {expanded && renderNodes(record.id, depth + 1, nextAncestors)}
        </React.Fragment>
      );
    });

  return (
    <Box sx={{ height: '100%', minHeight: 0, display: 'flex', flexDirection: 'column' }}>
      {filterVisible && (
        <Box sx={{ p: 1 }}>
          <TextField
            fullWidth
            size="small"
            placeholder={filterLabel}
            value={query}
            disabled={editing}
            onChange={(event) => onQueryChange(event.target.value)}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon sx={{ fontSize: 16 }} />
                  </InputAdornment>
                ),
              },
            }}
            sx={{ '& .MuiInputBase-root': { height: 34, fontSize: d365.fontSize } }}
          />
        </Box>
      )}
      <Box
        role="tree"
        aria-label={tree.ariaLabel}
        aria-busy={loading || undefined}
        sx={{ flex: 1, minHeight: 0, overflowY: 'auto', py: 0.5 }}
      >
        {includedIds.size > 0 ? (
          renderNodes(null, 0, new Set())
        ) : (
          <Typography sx={{ p: 2, color: 'text.secondary', fontSize: d365.fontSize }}>
            {tree.emptyLabel ?? 'No records found.'}
          </Typography>
        )}
      </Box>
    </Box>
  );
}
