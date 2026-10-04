import React, { useState } from 'react';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppRecordAuditDrawer } from '@shared/components/dialogs/AppRecordAuditDrawer';
import { AppRecordInfoDrawer } from '@shared/components/dialogs/AppRecordInfoDrawer';
import { ActionPaneRibbonTrigger } from './ActionPaneRibbonTrigger';

export interface OptionsMenuProps<T> {
  record: T | null;
  tableName: string;
  getRecordId?: (record: T) => string | number;
  title?: string;
  disabled?: boolean;
}

export function OptionsMenu<T>({
  record,
  tableName,
  getRecordId,
  title,
  disabled,
}: OptionsMenuProps<T>): React.ReactElement {
  const { t } = useAppTranslation();
  const [infoOpen, setInfoOpen] = useState(false);
  const [auditOpen, setAuditOpen] = useState(false);
  const recordId = record
    ? (getRecordId?.(record) ?? (record as { id?: string | number }).id ?? null)
    : null;

  return (
    <>
      <ActionPaneRibbonTrigger
        id="options"
        label={t('common.options')}
        disabled={disabled || !record}
        groups={[
          {
            id: 'record',
            label: t('common.record', 'Record'),
            actions: [
              {
                id: 'record-info',
                label: t('common.recordInfo'),
                onClick: () => setInfoOpen(true),
              },
              {
                id: 'record-audit',
                label: t('common.recordAudit'),
                onClick: () => setAuditOpen(true),
              },
            ],
          },
        ]}
      />
      <AppRecordInfoDrawer
        open={infoOpen}
        onClose={() => setInfoOpen(false)}
        record={record}
        title={title}
      />
      <AppRecordAuditDrawer
        open={auditOpen}
        onClose={() => setAuditOpen(false)}
        tableName={tableName}
        recordId={recordId}
      />
    </>
  );
}
