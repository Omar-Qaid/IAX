import React from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';

export const SortableItem: React.FC<{
    id: string;
    children: (handleProps: React.HTMLAttributes<HTMLElement>) => React.ReactNode;
}> = React.memo(({ id, children }) => {
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
        useSortable({ id });
    const style: React.CSSProperties = {
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.5 : 1,
    };
    return (
        <div ref={setNodeRef} style={style} {...attributes}>
            {children(listeners ?? {})}
        </div>
    );
});

SortableItem.displayName = 'SortableItem';
