import React, { useEffect, useMemo, useRef } from 'react';
import { ActionPaneButton } from './ActionPaneButton';
import { ActionPaneRibbon, type ActionPaneRibbonGroup } from './ActionPaneRibbon';
import { useActionPaneRibbon, type ActionPaneRibbonDescriptor } from './ActionPane';

export interface ActionPaneRibbonTriggerProps {
  id: string;
  label: string;
  groups: readonly ActionPaneRibbonGroup[];
  disabled?: boolean;
  persistenceKey?: string;
  controlsId?: string;
}

export function ActionPaneRibbonTrigger({
  id,
  label,
  groups,
  disabled,
  persistenceKey,
  controlsId = `${id}-action-ribbon`,
}: ActionPaneRibbonTriggerProps): React.ReactElement {
  const pane = useActionPaneRibbon();
  const restored = useRef(false);
  const descriptor = useMemo<ActionPaneRibbonDescriptor>(
    () => ({
      id,
      persistenceKey,
      render: (pinned, onPinnedChange) => (
        <div id={controlsId}>
          <ActionPaneRibbon
            groups={groups}
            pinned={pinned}
            onPinnedChange={onPinnedChange}
            unpinLabel="Collapse action pane"
          />
        </div>
      ),
    }),
    [controlsId, groups, id, persistenceKey]
  );

  useEffect(() => {
    if (!pane || !persistenceKey || restored.current) return;
    restored.current = true;
    try {
      if (window.localStorage.getItem(persistenceKey) === 'true') pane.restoreRibbon(descriptor);
    } catch {
      // Storage can be unavailable in restricted browser contexts.
    }
  }, [descriptor, pane, persistenceKey]);

  return (
    <ActionPaneButton
      label={label}
      disabled={disabled || !pane}
      selected={pane?.activeId === id}
      controlsId={controlsId}
      onClick={() => pane?.toggleRibbon(descriptor)}
    />
  );
}
