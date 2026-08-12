/* eslint-disable react-hooks/refs -- dnd-kit exposes callback refs and reactive transform values for render. */
import React from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';

export function SortableBuilderItem({ id, children }: { id: string; children: (attributes: ReturnType<typeof useSortable>['attributes'], listeners: ReturnType<typeof useSortable>['listeners']) => React.ReactNode }) {
  const sortable = useSortable({ id });
  return <div ref={sortable.setNodeRef} style={{ transform: CSS.Transform.toString(sortable.transform), transition: sortable.transition, opacity: sortable.isDragging ? 0.55 : 1 }}>
    {children(sortable.attributes, sortable.listeners)}
  </div>;
}
