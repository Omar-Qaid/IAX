import React from 'react';
import { Badge, IconButton, Tooltip } from '@mui/material';
import AttachFileOutlined from '@mui/icons-material/AttachFileOutlined';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { documentApi } from './documentApi';

export function RecordAttachmentsButton({ refTableId, refRecId, disabled = false }: { refTableId: number; refRecId: number | null; disabled?: boolean }): React.ReactElement {
  const navigate = useNavigate(); const enabled = !disabled && Boolean(refTableId > 0 && refRecId && refRecId > 0);
  const documents = useQuery({ queryKey: ['documents', refTableId, refRecId], queryFn: ({ signal }) => documentApi.list(refTableId, refRecId!, signal), enabled });
  return <Tooltip title={enabled ? 'Attachments' : 'Select a saved record'}><span><IconButton size="small" aria-label="Attachments" disabled={!enabled} onClick={() => navigate(`/documents/docu-view?refTableId=${refTableId}&refRecId=${refRecId}`)} sx={{ width: 42, height: 31, p: 0, color: 'primary.main', bgcolor: 'transparent', borderRadius: 0, '&:hover': { bgcolor: 'action.hover' }, '&.Mui-disabled': { color: 'primary.main', bgcolor: 'transparent', opacity: 0.55 } }}><Badge badgeContent={documents.data?.totalCount ?? 0} color="primary" showZero max={99} overlap="circular" anchorOrigin={{ vertical: 'top', horizontal: 'right' }} sx={{ '& .MuiBadge-badge': { minWidth: 18, height: 18, px: 0.4, fontSize: 10, fontWeight: 600, top: -1, right: 0 } }}><AttachFileOutlined sx={{ fontSize: 20 }} /></Badge></IconButton></span></Tooltip>;
}
