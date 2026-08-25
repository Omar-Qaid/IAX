import React from 'react';
import { Box, Typography } from '@mui/material';
export { toPrintoutCompany } from '@shared/components/printout/reportCompany';
import type { MailRequestDetailsDto, WfRequestRecord } from '../api/wfRequestApi';
import { normalizeDynamicControlType } from './DynamicControlRenderer';
import { MailFieldValue } from './MailFieldValue';

const formatDateTime = (value: string | null): string => {
  if (!value) return '—';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(date);
};

function Field({ label, value, fullWidth = false }: { label: string; value: React.ReactNode; fullWidth?: boolean }): React.ReactElement {
  return (
    <Box className="printout-field" sx={{ gridColumn: fullWidth ? '1 / -1' : 'auto', border: '1px solid #d9e2ec', borderRadius: 0.5, overflow: 'hidden' }}>
      <Typography className="printout-color" sx={{ px: 1, py: 0.55, bgcolor: '#eef4f8', borderBottom: '1px solid #d9e2ec', color: '#334e68', fontSize: 9, fontWeight: 700 }}>{label}</Typography>
      <Box sx={{ minHeight: 27, px: 1, py: 0.75, color: '#102a43', fontSize: 10.5 }}>{value}</Box>
    </Box>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }): React.ReactElement {
  return <Box className="printout-section"><Typography className="printout-color" sx={{ mb: 0.75, px: 1, py: 0.65, bgcolor: '#174f82', color: '#fff', fontSize: 11, fontWeight: 700 }}>{title}</Typography>{children}</Box>;
}

export function WorkflowMailPrintoutBody({ request, details, showNotes = true }: { request: WfRequestRecord; details: MailRequestDetailsDto; showNotes?: boolean }): React.ReactElement {
  return (
    <>
      <Section title="Request information">
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: 1 }}>
          <Field label="Request" value={request.code || `#${request.recId}`} />
          <Field label="Process" value={details.processName} />
          <Field label="Status" value={details.status} />
          <Field label="Request date" value={formatDateTime(details.requestDate)} />
          <Field label="Employee name" value={details.employeeName} />
          <Field label="Employee number" value={details.employeeNumber} />
          <Field label="Transaction type" value={details.transactionType} />
          <Field label="Responsible employee" value={details.responsibleEmployee || '—'} />
          <Field label="Transaction time" value={formatDateTime(details.transactionTime)} />
          <Field label="Transaction end time" value={formatDateTime(details.transactionEndTime)} />
        </Box>
      </Section>
      <Section title="Request data">
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: 1 }}>
          {[...details.fields].sort((left, right) => left.controlOrder - right.controlOrder).map((field) => {
            const type = normalizeDynamicControlType(field.controlType);
            const fullWidth = ['signature', 'longtext', 'file', 'table', 'label'].includes(type);
            return <Field key={`${field.detailId}-${field.controlDataId ?? field.controlId ?? field.controlOrder}`} label={field.labelAr || field.label} fullWidth={fullWidth} value={<Box dir={field.labelAr ? 'rtl' : 'ltr'}><MailFieldValue field={field} /></Box>} />;
          })}
        </Box>
      </Section>
      {showNotes && request.notes ? <Section title="Notes"><Typography sx={{ px: 1, whiteSpace: 'pre-wrap', fontSize: 10.5 }}>{request.notes}</Typography></Section> : null}
    </>
  );
}
