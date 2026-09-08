import React from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Drawer,
  FormControlLabel,
  IconButton,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import PlayArrowOutlined from '@mui/icons-material/PlayArrowOutlined';
import CloseOutlined from '@mui/icons-material/CloseOutlined';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import { wfProcessApi, type WfProcessRecord } from '../api/wfProcessApi';
import { fetchProcessPage, processLookupColumns } from '../lookups/processLookup';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { wfRequestControlApi, type WfRequestControlRecord } from '../api/wfRequestControlApi';
import { localizedName } from '@shared/utilities/localizedName';

interface GenericReportRow {
  id: string;
  date: string;
  showroom: string;
  seller: string;
  paymentMethod: string;
  requests: number;
  amount: number;
}

const MOCK_ROWS: GenericReportRow[] = [
  {
    id: '1',
    date: '2026-08-28',
    showroom: 'Riyadh',
    seller: 'Omar Ali',
    paymentMethod: 'Card',
    requests: 18,
    amount: 42850,
  },
  {
    id: '2',
    date: '2026-08-28',
    showroom: 'Jeddah',
    seller: 'Sara Ahmed',
    paymentMethod: 'Cash',
    requests: 12,
    amount: 28740,
  },
  {
    id: '3',
    date: '2026-08-28',
    showroom: 'Dammam',
    seller: 'Khalid Saleh',
    paymentMethod: 'Bank transfer',
    requests: 9,
    amount: 21600,
  },
  {
    id: '4',
    date: '2026-08-29',
    showroom: 'Riyadh',
    seller: 'Noura Hassan',
    paymentMethod: 'Card',
    requests: 15,
    amount: 36420,
  },
  {
    id: '5',
    date: '2026-08-29',
    showroom: 'Jeddah',
    seller: 'Mohammed Sami',
    paymentMethod: 'Cash',
    requests: 11,
    amount: 25490,
  },
];

const compactFieldSx = {
  '& .MuiInputBase-root': { height: 30, borderRadius: 0, bgcolor: '#f6f8fa' },
  '& .MuiOutlinedInput-notchedOutline': { border: 0 },
  '& .MuiInputBase-input': { fontSize: 12, py: 0.5, textAlign: 'start' },
};

const sectionHeadingSx = {
  pb: 0.5,
  borderBottom: '1px solid',
  borderColor: 'divider',
  fontSize: 10,
  fontWeight: 700,
  textTransform: 'uppercase',
};

const isDimension = (control: WfRequestControlRecord): boolean =>
  control.fieldRole === 'Dimension' || control.fieldRole === 'Both';
const isMeasure = (control: WfRequestControlRecord): boolean =>
  control.fieldRole === 'Measure' || control.fieldRole === 'Both';

function ReportParametersDialog({
  open,
  onCancel,
  onRun,
}: {
  open: boolean;
  onCancel: () => void;
  onRun: () => void;
}): React.ReactElement {
  const { t, currentLanguage, isRtl } = useAppTranslation();
  const direction = currentLanguage.dir;
  const [fromDate, setFromDate] = React.useState('2026-08-28');
  const [toDate, setToDate] = React.useState('2026-08-29');
  const [processId, setProcessId] = React.useState<number | null>(null);
  const [sortValue, setSortValue] = React.useState('');
  const metadata = useQuery({
    queryKey: ['workflow', 'generic-report', 'metadata', processId],
    queryFn: ({ signal }) => wfRequestControlApi.list(signal),
    enabled: processId != null && processId > 0,
    select: (controls) =>
      controls
        .filter((control) => control.processId === processId && control.isActive !== false)
        .sort((left, right) => left.sortOrder - right.sortOrder),
  });
  const controls = metadata.data ?? [];
  const filterControls = controls.filter((control) => control.canFilter);
  const groupControls = controls.filter((control) => control.canGroup && isDimension(control));
  const measureControls = controls.filter(isMeasure);
  const sortControls = controls.filter((control) => control.canSort);
  const labelFor = (control: WfRequestControlRecord) =>
    localizedName(control, isRtl) || control.code || String(control.recId);
  const measureOptions = measureControls.flatMap((control) => {
    const numeric = control.dataType === 'Decimal' || control.dataType === 'Integer';
    const aggregations = numeric
      ? (['SUM', 'AVG'] as const)
      : control.defaultAggregation !== 'NONE'
        ? [control.defaultAggregation]
        : [];
    return aggregations.map((aggregation) => ({
      id: `${control.recId}:${aggregation}`,
      label: `${aggregation} (${labelFor(control)})`,
      defaultChecked: control.defaultAggregation === aggregation,
    }));
  });
  return (
    <Drawer
      open={open}
      onClose={onCancel}
      anchor={isRtl ? 'left' : 'right'}
      sx={{ zIndex: (theme) => theme.zIndex.modal + 1 }}
      slotProps={{
        paper: {
          dir: direction,
          role: 'dialog',
          'aria-label': t('genericReport.title'),
          sx: {
            width: { xs: '100vw', sm: 620, md: 720 },
            maxWidth: '100vw',
            height: '100dvh',
            borderRadius: 0,
            display: 'flex',
            flexDirection: 'column',
          },
        },
      }}
    >
      <DialogTitle
        dir={direction}
        sx={{
          minHeight: 52,
          px: 2.25,
          py: 1.25,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
          borderBottom: '1px solid',
          borderColor: 'divider',
          fontSize: 15,
          fontWeight: 700,
          textAlign: 'start',
        }}
      >
        <Box component="span" sx={{ textAlign: 'start' }}>
          {t('genericReport.builder')}
        </Box>
        <IconButton
          size="small"
          onClick={onCancel}
          aria-label={t('actions.close')}
          sx={{ flexShrink: 0 }}
        >
          <CloseOutlined fontSize="small" />
        </IconButton>
      </DialogTitle>
      <DialogContent
        dir={direction}
        sx={{
          px: 2.25,
          pt: 1.5,
          pb: 2,
          flex: 1,
          overflowY: 'auto',
          textAlign: 'start',
          '& .MuiTypography-root': { textAlign: 'start' },
          '& .MuiInputBase-input, & .MuiSelect-select': { textAlign: 'start' },
          '& .MuiInputLabel-root': {
            insetInlineStart: 0,
            insetInlineEnd: 'auto',
            transformOrigin: 'top start',
          },
          '& .MuiFormControlLabel-root': {
            mx: 0,
            justifyContent: 'flex-start',
          },
          '& .MuiFormControlLabel-label': { textAlign: 'start' },
        }}
      >
        <Typography
          color="text.secondary"
          sx={{ fontSize: 9, pb: 1.25, borderBottom: '1px solid', borderColor: 'divider' }}
        >
          {t('genericReport.description')}
        </Typography>
        <Box
          sx={{
            mt: 2,
            '& .MuiInputBase-root': { bgcolor: '#f6f8fa', borderRadius: 0 },
            '& .MuiOutlinedInput-notchedOutline': { border: 0 },
          }}
        >
          <AppLookupGridField<WfProcessRecord>
            name="processId"
            label={t('genericReport.process')}
            value={processId}
            onChange={(value) => setProcessId(value == null ? null : Number(value))}
            placeholder={t('genericReport.selectProcess')}
            fullWidth
            size="small"
            columns={[...processLookupColumns]}
            queryKey={['workflow', 'generic-report', 'process-lookup']}
            fetchPage={fetchProcessPage}
            fetchById={(value) => wfProcessApi.getById(Number(value)).catch(() => null)}
            valueField="recId"
            labelField="name"
            labelFieldAr="nameAlias"
            pageSize={25}
          />
        </Box>
        <Box
          dir={direction}
          sx={{ mt: 2, display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}
        >
          <Box>
            <Typography sx={{ mb: 0.5, fontSize: 10, fontWeight: 700 }}>
              {t('genericReport.dateFrom')}
            </Typography>
            <TextField
              fullWidth
              type="date"
              value={fromDate}
              onChange={(event) => setFromDate(event.target.value)}
              sx={compactFieldSx}
            />
          </Box>
          <Box>
            <Typography sx={{ mb: 0.5, fontSize: 10, fontWeight: 700 }}>
              {t('genericReport.dateTo')}
            </Typography>
            <TextField
              fullWidth
              type="date"
              value={toDate}
              onChange={(event) => setToDate(event.target.value)}
              sx={compactFieldSx}
            />
          </Box>
        </Box>
        <Typography sx={{ ...sectionHeadingSx, mt: 2 }}>{t('genericReport.filters')}</Typography>
        <Box
          dir={direction}
          sx={{
            mt: 1,
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', sm: 'repeat(3, minmax(0, 1fr))' },
            gap: 1.25,
          }}
        >
          {filterControls.map((control) => (
            <Box key={control.recId}>
              <Typography sx={{ mb: 0.35, fontSize: 10 }}>{labelFor(control)}</Typography>
              {control.referenceType || control.dataType === 'Boolean' ? (
                <Select
                  fullWidth
                  size="small"
                  defaultValue="all"
                  sx={{
                    height: 30,
                    borderRadius: 0,
                    bgcolor: '#f6f8fa',
                    fontSize: 11,
                    '& .MuiOutlinedInput-notchedOutline': { border: 0 },
                  }}
                >
                  <MenuItem value="all">{t('genericReport.all')}</MenuItem>
                  {control.dataType === 'Boolean' && (
                    <MenuItem value="true">{t('common.yes')}</MenuItem>
                  )}
                  {control.dataType === 'Boolean' && (
                    <MenuItem value="false">{t('common.no')}</MenuItem>
                  )}
                </Select>
              ) : (
                <TextField
                  fullWidth
                  size="small"
                  type={
                    control.dataType === 'Date'
                      ? 'date'
                      : control.dataType === 'Time'
                        ? 'time'
                        : control.dataType === 'Decimal' || control.dataType === 'Integer'
                          ? 'number'
                          : 'text'
                  }
                  sx={compactFieldSx}
                />
              )}
            </Box>
          ))}
        </Box>
        <Box
          dir={direction}
          sx={{
            mt: 2.25,
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
            gap: 3,
          }}
        >
          <Box>
            <Typography sx={sectionHeadingSx}>{t('genericReport.groupBy')}</Typography>
            <Stack sx={{ mt: 0.5 }}>
              {groupControls.map((control) => (
                <FormControlLabel
                  key={control.recId}
                  control={<Checkbox size="small" defaultChecked />}
                  label={labelFor(control)}
                  sx={{ height: 25, '& .MuiTypography-root': { fontSize: 11 } }}
                />
              ))}
            </Stack>
          </Box>
          <Box>
            <Typography sx={sectionHeadingSx}>{t('genericReport.measures')}</Typography>
            <Stack sx={{ mt: 0.5 }}>
              {measureOptions.map((option) => (
                <FormControlLabel
                  key={option.id}
                  control={<Checkbox size="small" defaultChecked={option.defaultChecked} />}
                  label={option.label}
                  sx={{ height: 25, '& .MuiTypography-root': { fontSize: 11 } }}
                />
              ))}
              <FormControlLabel
                control={<Checkbox size="small" />}
                label={t('genericReport.countRequests')}
                sx={{ height: 25, '& .MuiTypography-root': { fontSize: 11 } }}
              />
            </Stack>
          </Box>
          <Box sx={{ gridColumn: { sm: '1 / 2' } }}>
            <Typography sx={sectionHeadingSx}>{t('genericReport.sortBy')}</Typography>
            <Select
              fullWidth
              size="small"
              displayEmpty
              value={sortValue}
              onChange={(event) => setSortValue(event.target.value)}
              sx={{
                mt: 1,
                height: 30,
                borderRadius: 0,
                bgcolor: '#f6f8fa',
                fontSize: 11,
                '& .MuiOutlinedInput-notchedOutline': { border: 0 },
              }}
            >
              <MenuItem value="">{t('common.none')}</MenuItem>
              {sortControls.flatMap((control) => [
                <MenuItem key={`${control.recId}-asc`} value={`${control.recId}:asc`}>
                  {labelFor(control)} ASC
                </MenuItem>,
                <MenuItem key={`${control.recId}-desc`} value={`${control.recId}:desc`}>
                  {labelFor(control)} DESC
                </MenuItem>,
              ])}
            </Select>
          </Box>
        </Box>
      </DialogContent>
      <DialogActions
        dir={direction}
        sx={{
          mx: 2.25,
          px: 0,
          py: 2,
          borderTop: '1px solid',
          borderColor: 'divider',
          justifyContent: 'flex-end',
        }}
      >
        <Button
          size="small"
          variant="contained"
          startIcon={<PlayArrowOutlined />}
          onClick={onRun}
          disabled={!processId || metadata.isLoading}
          sx={{
            minWidth: 96,
            borderRadius: 0,
            bgcolor: '#050505',
            fontSize: 10,
            fontWeight: 700,
            textTransform: 'none',
            '&:hover': { bgcolor: '#222' },
          }}
        >
          {t('genericReport.runReport')}
        </Button>
      </DialogActions>
    </Drawer>
  );
}

function ProcessingDialog({
  open,
  onCancel,
}: {
  open: boolean;
  onCancel: () => void;
}): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  return (
    <Dialog
      open={open}
      onClose={onCancel}
      maxWidth={false}
      slotProps={{
        paper: {
          dir: currentLanguage.dir,
          sx: {
            width: { xs: 'calc(100vw - 32px)', sm: 440 },
            maxWidth: 440,
            m: 2,
            borderRadius: 2,
            overflow: 'hidden',
          },
        },
      }}
    >
      <DialogTitle
        sx={{
          px: 3,
          py: 2,
          borderBottom: 1,
          borderColor: 'divider',
          fontSize: 17,
          fontWeight: 650,
          textAlign: 'start',
        }}
      >
        {t('genericReport.processingTitle')}
      </DialogTitle>
      <DialogContent dir={currentLanguage.dir} sx={{ px: 3, py: 2.5, textAlign: 'start' }}>
        <Typography color="text.secondary" sx={{ fontSize: 12 }}>
          {t('genericReport.processingDescription')}
        </Typography>
        <Typography sx={{ mt: 2.25, mb: 0.75, fontSize: 11, fontWeight: 600 }}>
          {t('genericReport.elapsedTime', { time: '00:00:01' })}
        </Typography>
        <LinearProgress
          aria-label={t('genericReport.processingProgress')}
          sx={{ width: '100%', height: 6, borderRadius: 3 }}
        />
      </DialogContent>
      <DialogActions
        dir={currentLanguage.dir}
        sx={{ px: 3, py: 1.5, borderTop: 1, borderColor: 'divider' }}
      >
        <Button variant="outlined" size="small" onClick={onCancel}>
          {t('common.cancel')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export function WfGenericReportPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const [parametersOpen, setParametersOpen] = React.useState(true);
  const [processing, setProcessing] = React.useState(false);
  const [rows, setRows] = React.useState<GenericReportRow[]>([]);
  const timer = React.useRef<number | null>(null);
  React.useEffect(
    () => () => {
      if (timer.current != null) window.clearTimeout(timer.current);
    },
    []
  );
  const run = () => {
    setParametersOpen(false);
    setProcessing(true);
    timer.current = window.setTimeout(() => {
      setProcessing(false);
      setRows(MOCK_ROWS);
      timer.current = null;
    }, 50);
  };
  const cancelProcessing = () => {
    if (timer.current != null) window.clearTimeout(timer.current);
    timer.current = null;
    setProcessing(false);
    setParametersOpen(true);
  };
  const columns = React.useMemo<ColumnDef<GenericReportRow>[]>(
    () => [
      { field: 'date', headerName: t('genericReport.date'), width: 125 },
      { field: 'showroom', headerName: t('genericReport.showroom'), width: 150 },
      { field: 'seller', headerName: t('genericReport.seller'), width: 180 },
      { field: 'paymentMethod', headerName: t('genericReport.paymentMethod'), width: 160 },
      { field: 'requests', headerName: t('genericReport.requests'), width: 110, type: 'number' },
      {
        field: 'amount',
        headerName: t('genericReport.amount'),
        width: 140,
        type: 'number',
        renderCell: ({ value }) =>
          new Intl.NumberFormat(currentLanguage.code, {
            style: 'currency',
            currency: 'SAR',
          }).format(Number(value)),
      },
    ],
    [currentLanguage.code, t]
  );
  const config: EnterpriseListConfig<GenericReportRow> = {
    readOnly: true,
    contextLabel: t('genericReport.workflowReports'),
    viewLabel: t('genericReport.title'),
    filterLabel: t('genericReport.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    searchFields: [
      { field: 'showroom', label: t('genericReport.showroom') },
      { field: 'seller', label: t('genericReport.seller') },
      { field: 'paymentMethod', label: t('genericReport.paymentMethod') },
    ],
    crud: {
      editLabel: t('common.edit'),
      newLabel: t('genericReport.new'),
      deleteLabel: t('common.delete'),
    },
    commands: [
      {
        id: 'parameters',
        label: t('genericReport.parameters'),
        onClick: () => setParametersOpen(true),
      },
    ],
    utilities: {
      personalizeLabel: t('genericReport.personalize'),
      guideLabel: t('genericReport.guide'),
      notificationsLabel: t('common.notifications'),
      refreshLabel: t('common.refresh'),
      openWindowLabel: t('genericReport.openInNewWindow'),
    },
    initialSelection: 'none',
    showSearchCommand: true,
  };
  return (
    <SimpleListPage<GenericReportRow>
      variant="enterprise"
      title={t('genericReport.title')}
      subtitle={t('genericReport.subtitle')}
      enterpriseConfig={config}
      dataSource={{ type: 'static', rows }}
      columns={columns}
      dataGridProps={{
        storageKey: 'workflow.wf-generic-report',
        selectionMode: 'single',
        hideAddRowButton: true,
        hideInlineEditActions: true,
        hideSidebar: false,
      }}
      dialogs={
        <>
          <ReportParametersDialog
            open={parametersOpen}
            onCancel={() => setParametersOpen(false)}
            onRun={run}
          />
          <ProcessingDialog open={processing} onCancel={cancelProcessing} />
        </>
      }
      contentMinHeight={420}
    />
  );
}
