import React from 'react';
import { SvgIcon, type SvgIconProps } from '@mui/material';
import { ActionPaneGroup } from './ActionPaneGroup';
import { ActionPaneButton } from './ActionPaneButton';

const EditIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25Zm17.71-10.04a1 1 0 0 0 0-1.42l-2.5-2.5a1 1 0 0 0-1.42 0l-1.96 1.96 3.75 3.75 2.13-1.79Z" /></SvgIcon>;
const AddIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M11 5h2v14h-2zM5 11h14v2H5z" /></SvgIcon>;
const DeleteIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12Zm3.5-9h1v8h-1v-8Zm4 0h1v8h-1v-8ZM15.5 4l-1-1h-5l-1 1H5v2h14V4z" /></SvgIcon>;
const SaveIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M5 3h12l3 3v15H4V3h1Zm2 2v5h9V5H7Zm0 9v5h10v-5H7Z" /></SvgIcon>;
const CancelIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="m6.4 5 5.6 5.6L17.6 5 19 6.4 13.4 12l5.6 5.6-1.4 1.4-5.6-5.6L6.4 19 5 17.6l5.6-5.6L5 6.4 6.4 5Z" /></SvgIcon>;

export interface EnterpriseCrudActionsProps {
  editLabel: string;
  newLabel: string;
  deleteLabel: string;
  canEdit: boolean;
  canDelete: boolean;
  onEdit?: () => void;
  onNew?: () => void;
  onDelete?: () => void;
  editPermission?: string;
  newPermission?: string;
  deletePermission?: string;
  editing?: boolean;
  saveLabel?: string;
  cancelLabel?: string;
  onSave?: () => void;
  onCancel?: () => void;
}

export const EnterpriseCrudActions: React.FC<EnterpriseCrudActionsProps> = ({ editLabel, newLabel, deleteLabel, canEdit, canDelete, onEdit, onNew, onDelete, editPermission, newPermission, deletePermission, editing = false, saveLabel = 'Save', cancelLabel = 'Cancel', onSave, onCancel }) => (
  <ActionPaneGroup>
    {editing ? <>
      <ActionPaneButton label={saveLabel} icon={<SaveIcon sx={{ fontSize: 16 }} />} onClick={onSave} permission={editPermission} />
      <ActionPaneButton label={cancelLabel} icon={<CancelIcon sx={{ fontSize: 16 }} />} onClick={onCancel} permission={editPermission} />
    </> : <>
      <ActionPaneButton label={editLabel} icon={<EditIcon sx={{ fontSize: 16 }} />} disabled={!canEdit} onClick={onEdit} permission={editPermission} />
      <ActionPaneButton label={newLabel} icon={<AddIcon sx={{ fontSize: 16 }} />} onClick={onNew} permission={newPermission} />
      <ActionPaneButton label={deleteLabel} icon={<DeleteIcon sx={{ fontSize: 16 }} />} disabled={!canDelete} onClick={onDelete} permission={deletePermission} />
    </>}
  </ActionPaneGroup>
);
