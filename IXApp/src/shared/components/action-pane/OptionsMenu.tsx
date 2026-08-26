import React, { useState } from 'react';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import HistoryIcon from '@mui/icons-material/History';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppRecordAuditDrawer } from '@shared/components/dialogs/AppRecordAuditDrawer';
import { AppRecordInfoDrawer } from '@shared/components/dialogs/AppRecordInfoDrawer';
import { ActionPaneMenu } from './ActionPaneMenu';

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
      <ActionPaneMenu
        label={t('common.options')}
        icon={<SettingsOutlinedIcon sx={{ fontSize: 17 }} />}
        disabled={disabled || !record}
        actions={[
          {
            id: 'record-info',
            label: t('common.recordInfo'),
            icon: <InfoOutlinedIcon fontSize="small" />,
            onClick: () => setInfoOpen(true),
          },
          {
            id: 'record-audit',
            label: t('common.recordAudit'),
            icon: <HistoryIcon fontSize="small" />,
            onClick: () => setAuditOpen(true),
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
