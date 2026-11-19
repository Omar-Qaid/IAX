import React, { useRef, useState } from 'react';
import { EnterpriseCrudActions, type EnterpriseCrudActionsProps } from './EnterpriseCrudActions';

export interface CommitAwareCrudActionsProps extends Omit<EnterpriseCrudActionsProps, 'onSave'> {
  beforeSave?: () => boolean | Promise<boolean>;
  onSave?: () => void | Promise<void>;
  onCancelDraft?: () => void;
}

/** Coordinates an optional nested editor commit with page Save and Cancel. */
export function CommitAwareCrudActions({
  beforeSave,
  onCancelDraft,
  ...props
}: CommitAwareCrudActionsProps) {
  const [committing, setCommitting] = useState(false);
  const locked = useRef(false);
  return (
    <EnterpriseCrudActions
      {...props}
      saving={props.saving || committing}
      onSave={async () => {
        if (locked.current || props.saving) return;
        locked.current = true;
        setCommitting(true);
        try {
          if (beforeSave && !(await beforeSave())) return;
          await props.onSave?.();
        } finally {
          locked.current = false;
          setCommitting(false);
        }
      }}
      onCancel={() => {
        if (locked.current || props.saving) return;
        onCancelDraft?.();
        props.onCancel?.();
      }}
    />
  );
}
