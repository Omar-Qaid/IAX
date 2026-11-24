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
  Switch,
  Tab,
  Tabs,
  Typography,
} from '@mui/material';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useNotifications } from '@shared/hooks/useNotifications';
import {
  ledgerJournalApiFor,
  type LedgerJournalTypeId,
  type PaymentJournal,
} from '../api/ledgerJournalApi';

/** Query keys of one journal type in one company (types never share cached rows). */
export const paymentJournalKeys = (company: string, journalType: LedgerJournalTypeId = 7) =>
  ['ledger-journals', journalType, company] as const;
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
/** One read-only field of a header form: text in a box, or a Yes/No switch. */
export interface JournalField {
  label: string;
  value?: React.ReactNode;
  /** Shows a disabled Yes/No switch instead of a text box. */
  toggle?: boolean;
  /** Group title shown above this field (a second group in the same column). */
  heading?: string;
  /** D365 field this journal does not store yet: shown empty, with a tooltip. */
  unavailable?: boolean;
  /** Editor shown instead of the read-only value (a line being edited). */
  editor?: React.ReactNode;
}
/** A titled column of fields (D365 field group). Untitled groups continue the previous column. */
export interface JournalFieldGroup {
  title?: string;
  fields: JournalField[];
}

/** D365 header tab layout: field groups side by side, each a column of labelled fields. */
export function JournalFieldGroups({ groups }: { groups: JournalFieldGroup[] }) {
  return (
    <Box
      sx={{
        p: 2,
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fill, minmax(190px, 1fr))',
        columnGap: 2,
        rowGap: 1,
        alignItems: 'start',
      }}
    >
      {groups.map((group, index) => (
        <Stack key={group.title ?? index} spacing={1} sx={{ minWidth: 0 }}>
          <Typography
            variant="caption"
            sx={{ fontWeight: 700, textTransform: 'uppercase', minHeight: 18 }}
          >
            {group.title ?? ''}
          </Typography>
          {group.fields.map((field) => (
            <Box
              key={field.label}
              title={field.unavailable ? 'Not stored for this journal yet.' : undefined}
            >
              {field.heading && (
                <Typography
                  variant="caption"
                  sx={{ display: 'block', fontWeight: 700, textTransform: 'uppercase', mb: 1 }}
                >
                  {field.heading}
                </Typography>
              )}
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                {field.label}
              </Typography>
              {field.editor ? (
                field.editor
              ) : field.toggle ? (
                <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
                  <Switch
                    size="small"
                    checked={!field.unavailable && Boolean(field.value)}
                    disabled
                    slotProps={{ input: { 'aria-label': field.label } }}
                  />
                  <Typography sx={{ fontSize: 12 }}>{field.value ? 'Yes' : 'No'}</Typography>
                </Stack>
              ) : (
                <Box
                  sx={{
                    mt: 0.25,
                    minHeight: 28,
                    px: 0.75,
                    display: 'flex',
                    alignItems: 'center',
                    fontSize: 12,
                    border: 1,
                    borderColor: 'divider',
                    borderRadius: 0.5,
                    bgcolor: 'action.hover',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                  }}
                >
                  {field.value === undefined || field.value === null || field.value === ''
                    ? ''
                    : field.value}
                </Box>
              )}
            </Box>
          ))}
        </Stack>
      ))}
    </Box>
  );
}

/** Validate / simulate / post for journals of one type, with their report and confirmation dialogs. */
export function usePaymentJournalActions(
  journalType: LedgerJournalTypeId = 7,
  title = 'customer payment journal'
) {
  const api = ledgerJournalApiFor(journalType);
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
  const refresh = () =>
    cache.invalidateQueries({ queryKey: paymentJournalKeys(company, journalType) });
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
        <DialogTitle>Post {title}</DialogTitle>
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
