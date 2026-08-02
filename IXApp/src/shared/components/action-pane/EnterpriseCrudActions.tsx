import React from 'react';
import { SvgIcon, type SvgIconProps } from '@mui/material';
import { ActionPaneGroup } from './ActionPaneGroup';
import { ActionPaneButton } from './ActionPaneButton';

const EditIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25Zm17.71-10.04a1 1 0 0 0 0-1.42l-2.5-2.5a1 1 0 0 0-1.42 0l-1.96 1.96 3.75 3.75 2.13-1.79Z" /></SvgIcon>;
const AddIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M11 5h2v14h-2zM5 11h14v2H5z" /></SvgIcon>;
const DeleteIcon = (props: SvgIconProps) => <SvgIcon {...props}><path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12Zm3.5-9h1v8h-1v-8Zm4 0h1v8h-1v-8ZM15.5 4l-1-1h-5l-1 1H5v2h14V4z" /></SvgIcon>;

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
}

export const EnterpriseCrudActions: React.FC<EnterpriseCrudActionsProps> = ({ editLabel, newLabel, deleteLabel, canEdit, canDelete, onEdit, onNew, onDelete, editPermission, newPermission, deletePermission }) => (
  <ActionPaneGroup>
    <ActionPaneButton label={editLabel} icon={<EditIcon sx={{ fontSize: 16 }} />} disabled={!canEdit} onClick={onEdit} permission={editPermission} />
    <ActionPaneButton label={newLabel} icon={<AddIcon sx={{ fontSize: 16 }} />} onClick={onNew} permission={newPermission} />
    <ActionPaneButton label={deleteLabel} icon={<DeleteIcon sx={{ fontSize: 16 }} />} disabled={!canDelete} onClick={onDelete} permission={deletePermission} />
  </ActionPaneGroup>
);
