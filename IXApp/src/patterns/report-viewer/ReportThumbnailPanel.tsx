import React from 'react';
import { Box, IconButton, Paper, Stack, Typography } from '@mui/material';
import AttachFileOutlined from '@mui/icons-material/AttachFileOutlined';
import GridViewOutlined from '@mui/icons-material/GridViewOutlined';
import LayersOutlined from '@mui/icons-material/LayersOutlined';
import ListOutlined from '@mui/icons-material/ListOutlined';
import { useVirtualizer } from '@tanstack/react-virtual';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface ReportThumbnailPanelProps {
  open: boolean;
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  renderThumbnail?: (page: number, selected: boolean) => React.ReactNode;
}

export function ReportThumbnailPanel({ open, currentPage, totalPages, onPageChange, renderThumbnail }: ReportThumbnailPanelProps): React.ReactElement {
  const { t } = useAppTranslation();
  const scrollRef = React.useRef<HTMLDivElement | null>(null);
  // TanStack Virtual maintains mutable measurements outside React state.
  // eslint-disable-next-line react-hooks/incompatible-library
  const virtualizer = useVirtualizer({
    count: totalPages,
    getScrollElement: () => scrollRef.current,
    estimateSize: () => 190,
    overscan: 3,
  });

  React.useEffect(() => {
    if (open && totalPages > 0) virtualizer.scrollToIndex(currentPage - 1, { align: 'auto' });
  }, [currentPage, open, totalPages, virtualizer]);

  return (
    <Paper
      square
      className="printout-screen-only"
      sx={{ minWidth: 0, display: 'grid', gridTemplateRows: '31px minmax(0, 1fr)', overflow: 'hidden', bgcolor: '#d7d8dc', borderInlineEnd: '1px solid #adb2ba' }}
    >
      {open ? <>
        <Stack direction="row" spacing={0.1} sx={{ alignItems: 'center', px: 0.5, bgcolor: '#eeeeef', borderBottom: '1px solid #b8bdc4' }}>
          <IconButton size="small" color="primary" aria-label={t('reportViewer.thumbnails.pages')}><GridViewOutlined sx={{ fontSize: 17 }} /></IconButton>
          <IconButton size="small" aria-label={t('reportViewer.thumbnails.outline')}><ListOutlined sx={{ fontSize: 17 }} /></IconButton>
          <IconButton size="small" aria-label={t('reportViewer.thumbnails.attachments')}><AttachFileOutlined sx={{ fontSize: 17 }} /></IconButton>
          <IconButton size="small" aria-label={t('reportViewer.thumbnails.layers')}><LayersOutlined sx={{ fontSize: 17 }} /></IconButton>
        </Stack>
        <Box ref={scrollRef} sx={{ overflowY: 'auto', overflowX: 'hidden' }}>
          <Box sx={{ position: 'relative', height: virtualizer.getTotalSize() }}>
            {virtualizer.getVirtualItems().map((item) => {
              const page = item.index + 1;
              const selected = currentPage === page;
              return <Box key={page} onClick={() => onPageChange(page)} sx={{ position: 'absolute', top: 0, insetInlineStart: 0, width: '100%', height: item.size, transform: `translateY(${item.start}px)`, cursor: 'pointer', pt: 1.5 }}>
                {renderThumbnail?.(page, selected) ?? <Box sx={{ width: { xs: 72, md: 108 }, aspectRatio: '210 / 297', mx: 'auto', bgcolor: '#fff', border: '2px solid', borderColor: selected ? '#315fa8' : '#9ba2ab', boxShadow: selected ? '0 0 0 2px rgba(49,95,168,.2)' : '0 2px 5px rgba(0,0,0,.18)', p: 0.75 }}>
                  <Box sx={{ height: '18%', borderBottom: '1px solid #9aa4ae' }} />
                  <Stack spacing={0.5} sx={{ mt: 0.75 }}>
                    {[70, 92, 82, 95, 76, 88].map((width, line) => <Box key={line} sx={{ width: `${width}%`, height: 2, bgcolor: '#c2c7ce' }} />)}
                  </Stack>
                </Box>}
                <Typography sx={{ mt: 0.5, textAlign: 'center', fontSize: 11, fontWeight: selected ? 700 : 500 }}>{page}</Typography>
              </Box>;
            })}
          </Box>
        </Box>
      </> : null}
    </Paper>
  );
}
