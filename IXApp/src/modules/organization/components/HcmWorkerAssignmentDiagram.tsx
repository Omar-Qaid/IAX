import React from 'react';
import { Alert, Box, CircularProgress, Tooltip, Typography } from '@mui/material';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import BadgeOutlinedIcon from '@mui/icons-material/BadgeOutlined';
import PersonOutlineIcon from '@mui/icons-material/PersonOutlined';
import StorefrontOutlinedIcon from '@mui/icons-material/StorefrontOutlined';
import { useQuery } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import { localizedName } from '@shared/utilities/localizedName';
import { hcmWorkerApi } from '../api/hcmWorkerApi';

interface Props {
  workerId: number;
  company: string;
}

export function HcmWorkerAssignmentDiagram({ workerId, company }: Props): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const query = useQuery({
    queryKey: ['hcm-worker-assignment-chain', company, workerId],
    queryFn: ({ signal }) => hcmWorkerApi.assignmentChain(workerId, signal),
    enabled: workerId > 0,
  });

  if (query.isLoading) {
    return (
      <Box sx={{ minHeight: 120, display: 'grid', placeItems: 'center' }}>
        <CircularProgress size={22} />
      </Box>
    );
  }

  if (query.isError) {
    const message = query.error instanceof Error ? query.error.message : String(query.error);
    return (
      <Alert severity="error" sx={{ m: 1 }}>
        {message}
      </Alert>
    );
  }

  const nodes = query.data ?? [];
  if (!nodes.length) {
    return (
      <Typography sx={{ p: 2, color: 'text.secondary' }}>
        {t('hcmWorkers.assignments.noHierarchy')}
      </Typography>
    );
  }

  return (
    <Box
      sx={{
        overflow: 'hidden',
        px: 2,
        py: 2.5,
        minWidth: 0,
        background: 'linear-gradient(135deg, #f8faff 0%, #ffffff 48%, #f6fbf7 100%)',
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'stretch', width: '100%', minWidth: 0 }}>
        {nodes.map((node, index) => {
          const isShowroom = node.type === 'showroom';
          const isWorker = node.type === 'worker';
          const accent = isShowroom ? '#107c10' : isWorker ? '#7c3aed' : d365.primary;
          const paleAccent = isShowroom ? '#eaf6ea' : isWorker ? '#f1ebff' : '#eaf0ff';
          const displayName = localizedName(node, isRtl);
          const title =
            node.type === 'showroom'
              ? t('hcmWorkers.assignments.showroomNode')
              : localizedName({ name: node.title, nameAlias: node.titleAlias }, isRtl) ||
                (node.type === 'worker'
                  ? t('hcmWorkers.assignments.workerNode')
                  : t('hcmWorkers.assignments.manager'));
          return (
            <React.Fragment key={`${node.type}-${node.recId}`}>
              {index > 0 && (
                <Box
                  sx={{
                    width: 36,
                    flex: '0 1 36px',
                    minWidth: 18,
                    display: 'flex',
                    alignItems: 'center',
                  }}
                >
                  <Box sx={{ height: 2, flex: 1, bgcolor: '#c7d2eb' }} />
                  <Box
                    sx={{
                      width: 24,
                      height: 24,
                      mx: -0.25,
                      borderRadius: '50%',
                      bgcolor: '#fff',
                      border: '1px solid #c7d2eb',
                      display: 'grid',
                      placeItems: 'center',
                    }}
                  >
                    <ArrowForwardIcon
                      sx={{
                        fontSize: 15,
                        color: d365.primary,
                        transform: isRtl ? 'rotate(180deg)' : undefined,
                      }}
                    />
                  </Box>
                  <Box sx={{ height: 2, flex: 1, bgcolor: '#c7d2eb' }} />
                </Box>
              )}
              <Box
                sx={{
                  position: 'relative',
                  width: 0,
                  minHeight: 112,
                  minWidth: 0,
                  flex: '1 1 0',
                  overflow: 'hidden',
                  border: '1px solid #d9e0ec',
                  borderRadius: 2,
                  bgcolor: '#fff',
                  px: 1.75,
                  py: 1.5,
                  display: 'flex',
                  alignItems: 'center',
                  gap: 1.25,
                  boxShadow: isWorker
                    ? `0 6px 18px ${accent}24`
                    : '0 3px 10px rgba(24, 42, 70, 0.08)',
                  transition: 'transform 160ms ease, box-shadow 160ms ease',
                  '&:hover': { transform: 'translateY(-2px)', boxShadow: `0 8px 20px ${accent}22` },
                  '&::before': {
                    content: '""',
                    position: 'absolute',
                    insetBlock: 0,
                    insetInlineStart: 0,
                    width: 5,
                    bgcolor: accent,
                  },
                }}
              >
                <Box
                  sx={{
                    width: 42,
                    height: 42,
                    flex: '0 0 42px',
                    borderRadius: '50%',
                    bgcolor: paleAccent,
                    color: accent,
                    display: 'grid',
                    placeItems: 'center',
                  }}
                >
                  {isShowroom ? (
                    <StorefrontOutlinedIcon />
                  ) : isWorker ? (
                    <BadgeOutlinedIcon />
                  ) : (
                    <PersonOutlineIcon />
                  )}
                </Box>
                <Box sx={{ minWidth: 0 }}>
                  <Typography
                    sx={{
                      mb: 0.5,
                      color: accent,
                      fontSize: 11,
                      fontWeight: 700,
                      letterSpacing: 0.15,
                    }}
                    noWrap
                  >
                    {title}
                  </Typography>
                  <Tooltip title={displayName} arrow placement="top">
                    <Typography
                      sx={{ color: '#172033', fontSize: 14, fontWeight: 700, lineHeight: 1.45 }}
                      noWrap
                    >
                      {displayName}
                    </Typography>
                  </Tooltip>
                  {!isShowroom && (
                    <Typography
                      sx={{
                        mt: 0.35,
                        fontSize: 11,
                        color: 'text.secondary',
                        direction: 'ltr',
                        textAlign: isRtl ? 'right' : 'left',
                      }}
                    >
                      {node.code}
                    </Typography>
                  )}
                </Box>
              </Box>
            </React.Fragment>
          );
        })}
      </Box>
    </Box>
  );
}
