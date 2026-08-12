import { useEffect, useState } from 'react';
import type { ProcessBuilderNode } from './types';

export const useProcessBuilderPage = (nodes: ProcessBuilderNode[]) => {
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState(0);

  useEffect(() => {
    if (!selectedId && nodes[0]) setSelectedId(nodes[0].id);
  }, [nodes, selectedId]);

  const findNode = (items: ProcessBuilderNode[]): ProcessBuilderNode | null => {
    for (const item of items) {
      if (item.id === selectedId) return item;
      const nested = item.children ? findNode(item.children) : null;
      if (nested) return nested;
    }
    return null;
  };

  return { selectedId, selectedNode: findNode(nodes), activeTab, setSelectedId, setActiveTab };
};
