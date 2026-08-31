import React from 'react';
import { Box, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { MailRequestFieldDto } from '../api/wfRequestApi';
import { normalizeDynamicControlType, readTableRows } from './DynamicControlRenderer';
import { SignatureControl } from './DynamicSpecialControls';

interface LocationValue {
  address?: string;
  latitude?: number | string;
  longitude?: number | string;
}

const tableLabels: Record<string, { ar: string; en: string }> = {
  sequence: { ar: 'م', en: 'No.' },
  beneficiary: { ar: 'اسم المستفيد', en: 'Beneficiary' },
  invoice_number: { ar: 'رقم الفاتورة', en: 'Invoice number' },
  invoice_amount: { ar: 'قيمة الفاتورة', en: 'Invoice amount' },
  vat: { ar: 'الضريبة', en: 'VAT' },
  total: { ar: 'الإجمالي', en: 'Total' },
  payment_statement: { ar: 'بيان التحويل / الصرف', en: 'Transfer / payment statement' },
};

const readableColumnLabel = (key: string, rtl: boolean): string => {
  const known = tableLabels[key];
  if (known) return rtl ? known.ar : known.en;
  return key.replace(/_/g, ' ').replace(/\b\w/g, (letter) => letter.toLocaleUpperCase());
};

function LocationFieldValue({ value }: { value: string }): React.ReactElement {
  let location: LocationValue | null = null;
  try {
    const parsed = JSON.parse(value) as unknown;
    if (parsed && typeof parsed === 'object' && !Array.isArray(parsed))
      location = parsed as LocationValue;
  } catch {
    // Older workflow requests may contain a plain-text location.
  }

  if (!location) return <Box dir="auto">{value || '—'}</Box>;
  const hasCoordinates = location.latitude !== undefined && location.longitude !== undefined;
  return (
    <Box dir="auto">
      <Typography sx={{ fontSize: 'inherit', whiteSpace: 'pre-wrap' }}>
        {location.address || '—'}
      </Typography>
      {hasCoordinates && (
        <Typography color="text.secondary" sx={{ mt: 0.25, fontSize: 10.5 }}>
          {location.latitude}, {location.longitude}
        </Typography>
      )}
    </Box>
  );
}

function TableFieldValue({ value, rtl }: { value: string; rtl: boolean }): React.ReactElement {
  const rows = readTableRows(value);
  if (rows.length === 0) return <Box>—</Box>;
  const columns = [...new Set(rows.flatMap((row) => Object.keys(row)))];
  if (columns.length === 0) return <Box>—</Box>;

  return (
    <Box sx={{ width: '100%', overflowX: 'auto', border: '1px solid #b8b8b8' }}>
      <Box
        component="table"
        dir={rtl ? 'rtl' : 'ltr'}
        sx={{
          width: '100%',
          borderCollapse: 'collapse',
          tableLayout: 'fixed',
        }}
      >
        <Box component="thead" sx={{ bgcolor: '#e7e7e7' }}>
          <Box component="tr">
            {columns.map((column) => (
              <Box
                component="th"
                key={column}
                sx={{
                  width: column === 'sequence' ? 42 : column === 'payment_statement' ? 120 : 'auto',
                  px: 0.75,
                  py: 0.6,
                  border: '1px solid #aaa',
                  fontSize: 9.5,
                  fontWeight: 700,
                  lineHeight: 1.2,
                  overflowWrap: 'anywhere',
                }}
              >
                {readableColumnLabel(column, rtl)}
              </Box>
            ))}
          </Box>
        </Box>
        <Box component="tbody">
          {rows.map((row, rowIndex) => (
            <Box component="tr" key={rowIndex}>
              {columns.map((column) => (
                <Box
                  component="td"
                  key={column}
                  dir="auto"
                  sx={{
                    px: 0.75,
                    py: 0.65,
                    border: '1px solid #bbb',
                    fontSize: 10,
                    textAlign: 'center',
                    overflowWrap: 'anywhere',
                  }}
                >
                  {row[column] || '—'}
                </Box>
              ))}
            </Box>
          ))}
        </Box>
      </Box>
    </Box>
  );
}

export function MailFieldValue({ field }: { field: MailRequestFieldDto }): React.ReactElement {
  const { isRtl } = useAppTranslation();
  const label = isRtl ? field.labelAr || field.label : field.label || field.labelAr;
  const value = isRtl
    ? field.valueAr || field.value || field.valueEn
    : field.valueEn || field.value || field.valueAr;
  const controlType = normalizeDynamicControlType(field.controlType);

  if (controlType === 'signature') {
    return (
      <SignatureControl
        control={{ label, hideLabel: true, controlType: 'signature', readOnly: true }}
        value={value}
        onChange={() => undefined}
        preview
      />
    );
  }
  if (controlType === 'location') return <LocationFieldValue value={value} />;
  if (controlType === 'table') return <TableFieldValue value={value} rtl={isRtl} />;
  return (
    <Box dir="auto" sx={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' }}>
      {value || '—'}
    </Box>
  );
}
