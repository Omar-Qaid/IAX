import React from 'react';
import { Box, Typography } from '@mui/material';
export { toReportCompany } from '@shared/components/report-viewer/reportCompany';
import type { MailRequestDetailsDto, WfRequestRecord } from '../../api/wfRequestApi';
import { normalizeDynamicControlType } from '../../components/DynamicControlRenderer';
import { MailFieldValue } from '../../components/MailFieldValue';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { TFunction } from 'i18next';

export const getLocalizedMailStatus = (t: TFunction, status: string): string => {
  const normalized = status
    .trim()
    .toLowerCase()
    .replace(/[\s_-]+/g, '');
  if (normalized === 'completed') return t('mail.statuses.completed');
  if (normalized === 'inprogress') return t('mail.statuses.inProgress');
  if (normalized === 'processing') return t('mail.statuses.processing');
  if (normalized === 'stopped') return t('mail.statuses.stopped');
  return status;
};

const formatDateTime = (value: string | null, locale: string): string => {
  if (!value) return '—';
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? value
    : new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(date);
};

function Field({
  label,
  value,
  fullWidth = false,
}: {
  label: string;
  value: React.ReactNode;
  fullWidth?: boolean;
}): React.ReactElement {
  return (
    <Box
      className="printout-field"
      sx={{
        gridColumn: fullWidth ? '1 / -1' : 'auto',
        border: '1px solid #d9e2ec',
        borderRadius: 0.5,
        overflow: 'hidden',
      }}
    >
      <Typography
        className="printout-color"
        sx={{
          px: 1,
          py: 0.55,
          bgcolor: '#eef4f8',
          borderBottom: '1px solid #d9e2ec',
          color: '#334e68',
          fontSize: 9,
          fontWeight: 700,
        }}
      >
        {label}
      </Typography>
      <Box sx={{ minHeight: 27, px: 1, py: 0.75, color: '#102a43', fontSize: 10.5 }}>{value}</Box>
    </Box>
  );
}

function Section({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}): React.ReactElement {
  return (
    <Box className="printout-section">
      <Typography
        className="printout-color"
        sx={{
          mb: 0.75,
          px: 1,
          py: 0.65,
          bgcolor: '#174f82',
          color: '#fff',
          fontSize: 11,
          fontWeight: 700,
        }}
      >
        {title}
      </Typography>
      {children}
    </Box>
  );
}

export function WorkflowMailReportViewerBody({
  request,
  details,
  processName,
  showNotes = true,
}: {
  request: WfRequestRecord;
  details: MailRequestDetailsDto;
  processName?: string;
  showNotes?: boolean;
}): React.ReactElement {
  const { t, currentLanguage, isRtl } = useAppTranslation();
  const direction = isRtl ? 'rtl' : 'ltr';
  return (
    <Box dir={direction} sx={{ display: 'grid', gap: 1.5, textAlign: 'start' }}>
      <Section title={t('mail.print.requestInformation')}>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: 1 }}>
          <Field label={t('mail.fields.request')} value={request.code || `#${request.recId}`} />
          <Field label={t('mail.fields.process')} value={processName || details.processName} />
          <Field
            label={t('mail.fields.status')}
            value={getLocalizedMailStatus(t, details.status)}
          />
          <Field
            label={t('mail.fields.requestDate')}
            value={formatDateTime(details.requestDate, currentLanguage.code)}
          />
          <Field label={t('mail.print.employeeName')} value={details.employeeName} />
          <Field label={t('mail.print.employeeNumber')} value={details.employeeNumber} />
          <Field label={t('mail.print.transactionType')} value={details.transactionType} />
          <Field
            label={t('mail.print.responsibleEmployee')}
            value={details.responsibleEmployee || '—'}
          />
          <Field
            label={t('mail.print.transactionTime')}
            value={formatDateTime(details.transactionTime, currentLanguage.code)}
          />
          <Field
            label={t('mail.print.transactionEndTime')}
            value={formatDateTime(details.transactionEndTime, currentLanguage.code)}
          />
        </Box>
      </Section>
      <Section title={t('mail.print.requestData')}>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: 1 }}>
          {[...details.fields]
            .sort((left, right) => left.controlOrder - right.controlOrder)
            .map((field) => {
              const type = normalizeDynamicControlType(field.controlType);
              const fullWidth = ['signature', 'longtext', 'file', 'table', 'label'].includes(type);
              const label = isRtl ? field.labelAr || field.label : field.label || field.labelAr;
              return (
                <Field
                  key={`${field.detailId}-${field.controlDataId ?? field.controlId ?? field.controlOrder}`}
                  label={label}
                  fullWidth={fullWidth}
                  value={
                    <Box dir="auto">
                      <MailFieldValue field={field} />
                    </Box>
                  }
                />
              );
            })}
        </Box>
      </Section>
      {showNotes && request.notes ? (
        <Section title={t('mail.print.notes')}>
          <Typography sx={{ px: 1, whiteSpace: 'pre-wrap', fontSize: 10.5 }}>
            {request.notes}
          </Typography>
        </Section>
      ) : null}
    </Box>
  );
}
