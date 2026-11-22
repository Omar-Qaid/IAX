import React from 'react';
import {
  Alert,
  Box,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Button,
  Stack,
  Tab,
  Tabs,
  Typography,
} from '@mui/material';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useNotifications } from '@shared/hooks/useNotifications';
import {
  ledgerJournalCustPaymApi as api,
  type PaymentJournal,
} from '../api/ledgerJournalCustPaymApi';

export const paymentJournalKeys = (company: string) =>
  ['customer-payment-journals', company] as const;
export const paymentJournalSx = {
  '& .MuiTab-root': { minHeight: 32, minWidth: 0, px: 1.2, fontSize: 11, textTransform: 'none' },
  minHeight: 32,
};
export function JournalTabs({
  tabs,
  value,
  onChange,
  disabled,
}: {
  tabs: string[];
  value: string;
  onChange: (tab: string) => void;
  disabled?: boolean;
}) {
  return (
    <Tabs
      value={value}
      onChange={(_, next: string) => onChange(next)}
      sx={paymentJournalSx}
      variant="scrollable"
    >
      {tabs.map((tab) => (
        <Tab key={tab} value={tab} label={tab} disabled={disabled} />
      ))}
    </Tabs>
  );
}
export function JournalDetails({ values }: { values: Record<string, React.ReactNode> }) {
  return (
    <Box
      sx={{
        p: 2,
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
        gap: 2,
        alignContent: 'start',
      }}
    >
      {Object.entries(values).map(([label, value]) => (
        <Box key={label}>
          <Typography variant="caption" color="text.secondary">
            {label}
          </Typography>
          <Typography sx={{ fontSize: 12, mt: 0.5 }}>{value || '—'}</Typography>
        </Box>
      ))}
    </Box>
  );
}
export function usePaymentJournalActions() {
  const company = useCompanyStore((state) => state.currentCompany);
  const cache = useQueryClient();
  const { notifyError, notifySuccess } = useNotifications();
  const [report, setReport] = React.useState<{
    title: string;
    messages: string[];
    /** Messages shown as errors (after the summary line). */
    errors?: string[];
    success: boolean;
  } | null>(null);
  const [pendingPost, setPendingPost] = React.useState<PaymentJournal | null>(null);
  const refresh = () => cache.invalidateQueries({ queryKey: paymentJournalKeys(company) });
  const operation = useMutation({
    mutationFn: async ({
      journal,
      action,
    }: {
      journal: PaymentJournal;
      action: 'validate' | 'simulate' | 'post';
    }) => {
      if (action === 'validate') {
        const result = await api.validate(journal.recId);
        setReport({
          title: 'Validate journal',
          success: result.valid,
          messages: [
            result.valid ? 'Static validation passed.' : 'Validation failed.',
            ...result.unverifiedChecks,
            result.scope,
          ],
          errors: result.errors,
        });
      } else if (action === 'simulate') {
        const result = await api.simulate(journal.recId);
        setReport({
          title: 'Simulate posting',
          success: result.wouldPost,
          messages: [
            result.wouldPost ? 'Simulation succeeded.' : result.refusal || 'Simulation failed.',
            result.scope,
          ],
        });
      } else {
        await api.post(journal.recId);
        notifySuccess(`Journal ${journal.journalNum} posted.`);
        await refresh();
      }
    },
    onError: (error: Error) => notifyError(error.message),
  });
  const run = (journal: PaymentJournal | null, action: 'validate' | 'simulate' | 'post') => {
    if (!journal || operation.isPending) return;
    if (action === 'post') setPendingPost(journal);
    else operation.mutate({ journal, action });
  };
  const dialogs = (
    <>
      <Dialog open={!!report} onClose={() => setReport(null)} fullWidth maxWidth="sm">
        <DialogTitle>{report?.title}</DialogTitle>
        <DialogContent>
          <Stack spacing={1}>
            {report?.messages.filter(Boolean).map((message, i) => (
              <React.Fragment key={i}>
                <Alert severity={i === 0 ? (report.success ? 'success' : 'error') : 'info'}>
                  {message}
                </Alert>
                {/* Validation errors right below the summary, as errors (not info). */}
                {i === 0 &&
                  report.errors?.map((error, e) => (
                    <Alert key={`error-${e}`} severity="error">
                      {error}
                    </Alert>
                  ))}
              </React.Fragment>
            ))}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setReport(null)}>Close</Button>
        </DialogActions>
      </Dialog>
      <Dialog open={!!pendingPost} onClose={() => setPendingPost(null)}>
        <DialogTitle>Post customer payment journal</DialogTitle>
        <DialogContent>
          Post journal {pendingPost?.journalNum}? This creates financial transactions and locks the
          journal against editing.
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPendingPost(null)}>Cancel</Button>
          <Button
            onClick={() => {
              if (pendingPost) operation.mutate({ journal: pendingPost, action: 'post' });
              setPendingPost(null);
            }}
          >
            Post
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
  return { run, dialogs, busy: operation.isPending, refresh };
}
