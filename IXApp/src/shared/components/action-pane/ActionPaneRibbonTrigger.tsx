import React, { useEffect, useMemo, useRef } from 'react';
import { useAppTranslation } from '@core/localization/useAppTranslation';
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
  const { t } = useAppTranslation();
  const pane = useActionPaneRibbon();
  const updateRibbon = pane?.updateRibbon;
  const restored = useRef(false);
  // Both pin labels are passed: they are screen-reader names, so leaving either to the
  // ribbon's English default would keep it English in Arabic.
  const pinLabel = t('actions.pinActionPane');
  const unpinLabel = t('actions.unpinActionPane');
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
            pinLabel={pinLabel}
            unpinLabel={unpinLabel}
          />
        </div>
      ),
    }),
    [controlsId, groups, id, persistenceKey, pinLabel, unpinLabel]
  );

  useEffect(() => {
    updateRibbon?.(descriptor);
  }, [descriptor, updateRibbon]);

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
