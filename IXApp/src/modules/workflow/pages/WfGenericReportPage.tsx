import React from 'react';
import {
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import PlayArrowOutlined from '@mui/icons-material/PlayArrowOutlined';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import { wfProcessApi, type WfProcessRecord } from '../api/wfProcessApi';
import { fetchProcessPage, processLookupColumns } from '../lookups/processLookup';

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
  '& .MuiInputBase-root': { height: 32, borderRadius: 0.5 },
  '& .MuiInputBase-input': { fontSize: 12, py: 0.5 },
};

function ReportParametersDialog({
  open,
  onCancel,
  onRun,
}: {
  open: boolean;
  onCancel: () => void;
  onRun: () => void;
}): React.ReactElement {
  const [fromDate, setFromDate] = React.useState('2026-08-28');
  const [toDate, setToDate] = React.useState('2026-08-29');
  const [processId, setProcessId] = React.useState<number | null>(null);
  return (
    <Dialog
      open={open}
      onClose={onCancel}
      maxWidth="sm"
      fullWidth
      slotProps={{ paper: { sx: { borderRadius: 0, minHeight: 560 } } }}
    >
      <DialogTitle
        sx={{
          px: 3,
          py: 2,
          borderBottom: '1px solid',
          borderColor: 'divider',
          fontSize: 18,
          fontWeight: 600,
        }}
      >
        WfGenericReport
      </DialogTitle>
      <DialogContent sx={{ px: 3, py: 2.25 }}>
        <Typography sx={{ fontSize: 16, fontWeight: 700 }}>Report Builder</Typography>
        <Typography
          color="text.secondary"
          sx={{ fontSize: 11, pb: 1.5, borderBottom: '1px solid', borderColor: 'divider' }}
        >
          Configure parameters to generate a custom operational report.
        </Typography>
        <Box sx={{ mt: 2 }}>
          <AppLookupGridField<WfProcessRecord>
            name="processId"
            label="Process"
            value={processId}
            onChange={(value) => setProcessId(value == null ? null : Number(value))}
            placeholder="Select workflow process"
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
        <Box sx={{ mt: 2, display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
          <Box>
            <Typography sx={{ mb: 0.5, fontSize: 10, fontWeight: 700 }}>DATE FROM</Typography>
            <TextField
              fullWidth
              type="date"
              value={fromDate}
              onChange={(event) => setFromDate(event.target.value)}
              sx={compactFieldSx}
            />
          </Box>
          <Box>
            <Typography sx={{ mb: 0.5, fontSize: 10, fontWeight: 700 }}>DATE TO</Typography>
            <TextField
              fullWidth
              type="date"
              value={toDate}
              onChange={(event) => setToDate(event.target.value)}
              sx={compactFieldSx}
            />
          </Box>
        </Box>
        <Typography
          sx={{
            mt: 2,
            pb: 0.5,
            borderBottom: '1px solid',
            borderColor: 'divider',
            fontSize: 10,
            fontWeight: 700,
          }}
        >
          FILTERS
        </Typography>
        <Box sx={{ mt: 1, display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 1.25 }}>
          {['Showroom', 'Seller', 'Payment method'].map((label) => (
            <Box key={label}>
              <Typography sx={{ mb: 0.35, fontSize: 10 }}>{label}</Typography>
              <Select
                fullWidth
                size="small"
                defaultValue="all"
                sx={{ height: 30, borderRadius: 0.5, fontSize: 11 }}
              >
                <MenuItem value="all">All</MenuItem>
              </Select>
            </Box>
          ))}
        </Box>
        <Box sx={{ mt: 2.25, display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 3 }}>
          <Box>
            <Typography
              sx={{
                pb: 0.5,
                borderBottom: '1px solid',
                borderColor: 'divider',
                fontSize: 10,
                fontWeight: 700,
              }}
            >
              GROUP BY
            </Typography>
            <Stack sx={{ mt: 0.5 }}>
              {['Showroom', 'Payment method', 'Seller', 'Date'].map((label, index) => (
                <FormControlLabel
                  key={label}
                  control={<Checkbox size="small" defaultChecked={index !== 2} />}
                  label={label}
                  sx={{ height: 25, '& .MuiTypography-root': { fontSize: 11 } }}
                />
              ))}
            </Stack>
          </Box>
          <Box>
            <Typography
              sx={{
                pb: 0.5,
                borderBottom: '1px solid',
                borderColor: 'divider',
                fontSize: 10,
                fontWeight: 700,
              }}
            >
              MEASURES
            </Typography>
            <Stack sx={{ mt: 0.5 }}>
              {['SUM (Amount)', 'AVG (Amount)', 'COUNT (Requests)'].map((label, index) => (
                <FormControlLabel
                  key={label}
                  control={<Checkbox size="small" defaultChecked={index === 0} />}
                  label={label}
                  sx={{ height: 25, '& .MuiTypography-root': { fontSize: 11 } }}
                />
              ))}
            </Stack>
          </Box>
        </Box>
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 1.5, borderTop: '1px solid', borderColor: 'divider' }}>
        <Button size="small" onClick={onCancel}>
          Cancel
        </Button>
        <Button size="small" variant="contained" startIcon={<PlayArrowOutlined />} onClick={onRun}>
          OK
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function ProcessingDialog({
  open,
  onCancel,
}: {
  open: boolean;
  onCancel: () => void;
}): React.ReactElement {
  return (
    <Dialog
      open={open}
      onClose={onCancel}
      maxWidth={false}
      slotProps={{
        paper: {
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
        }}
      >
        Processing operation - WfGenericReport
      </DialogTitle>
      <DialogContent sx={{ px: 3, py: 2.5 }}>
        <Typography color="text.secondary" sx={{ fontSize: 12 }}>
          Please wait while the report is being prepared.
        </Typography>
        <Typography sx={{ mt: 2.25, mb: 0.75, fontSize: 11, fontWeight: 600 }}>
          Operation elapsed time: 00:00:01
        </Typography>
        <LinearProgress
          aria-label="Report processing progress"
          sx={{ width: '100%', height: 6, borderRadius: 3 }}
        />
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 1.5, borderTop: 1, borderColor: 'divider' }}>
        <Button variant="outlined" size="small" onClick={onCancel}>
          Cancel
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export function WfGenericReportPage(): React.ReactElement {
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
    }, 1200);
  };
  const cancelProcessing = () => {
    if (timer.current != null) window.clearTimeout(timer.current);
    setProcessing(false);
    setParametersOpen(true);
  };
  const columns = React.useMemo<ColumnDef<GenericReportRow>[]>(
    () => [
      { field: 'date', headerName: 'Date', width: 125 },
      { field: 'showroom', headerName: 'Showroom', width: 150 },
      { field: 'seller', headerName: 'Seller', width: 180 },
      { field: 'paymentMethod', headerName: 'Payment method', width: 160 },
      { field: 'requests', headerName: 'Requests', width: 110, type: 'number' },
      {
        field: 'amount',
        headerName: 'Amount',
        width: 140,
        type: 'number',
        renderCell: ({ value }) =>
          new Intl.NumberFormat('en-SA', { style: 'currency', currency: 'SAR' }).format(
            Number(value)
          ),
      },
    ],
    []
  );
  const config: EnterpriseListConfig<GenericReportRow> = {
    readOnly: true,
    contextLabel: 'Workflow reports',
    viewLabel: 'WfGenericReport',
    filterLabel: 'Filter',
    informationLabel: 'Information',
    searchMode: 'quick',
    searchFields: [
      { field: 'showroom', label: 'Showroom' },
      { field: 'seller', label: 'Seller' },
      { field: 'paymentMethod', label: 'Payment method' },
    ],
    crud: { editLabel: 'Edit', newLabel: 'New', deleteLabel: 'Delete' },
    commands: [{ id: 'parameters', label: 'Parameters', onClick: () => setParametersOpen(true) }],
    utilities: {
      personalizeLabel: 'Personalize',
      guideLabel: 'Guide',
      notificationsLabel: 'Notifications',
      refreshLabel: 'Refresh',
      openWindowLabel: 'Open in new window',
    },
    initialSelection: 'none',
    showSearchCommand: true,
  };
  return (
    <SimpleListPage<GenericReportRow>
      variant="enterprise"
      title="WfGenericReport"
      subtitle="Generated operational report"
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
