import React from 'react';
import { Box, Typography } from '@mui/material';
import {
  PrintoutDocument,
  type PrintoutCompany,
} from '@shared/components/printout/PrintoutDocument';
import type {
  PrintTemplateDocument,
  PrintTemplateElement,
  PrintValueFormat,
} from '../types/printTemplate.types';
import { resolveRuntimeBinding, type RuntimePrintData } from './runtimePrintData';

interface Props {
  template: PrintTemplateDocument;
  data: RuntimePrintData;
  company: PrintoutCompany;
}

const empty = (value: unknown) =>
  value == null || value === '' || (Array.isArray(value) && value.length === 0);

const pad = (value: number): string => String(value).padStart(2, '0');

const formatDatePattern = (date: Date, pattern: string, locale: string): string => {
  const day = pad(date.getDate());
  const month = pad(date.getMonth() + 1);
  const year = String(date.getFullYear());
  const shortMonth = new Intl.DateTimeFormat(locale, { month: 'short' }).format(date);
  return pattern
    .replace('yyyy', year)
    .replace('MMM', shortMonth)
    .replace('MM', month)
    .replace('dd', day)
    .replace('HH', pad(date.getHours()))
    .replace('mm', pad(date.getMinutes()));
};

export const formatPrintValue = (
  value: unknown,
  format: PrintValueFormat | null | undefined,
  locale: string
): string => {
  if (empty(value)) return '';
  if (!format || format.type === 'text') return String(value);
  if (format.type === 'date' || format.type === 'dateTime') {
    const date = new Date(String(value));
    if (Number.isNaN(date.getTime())) return String(value);
    if (format.pattern) {
      const formattedDate = formatDatePattern(date, format.pattern, locale);
      return format.type === 'dateTime'
        ? `${formattedDate} ${pad(date.getHours())}:${pad(date.getMinutes())}`
        : formattedDate;
    }
    return new Intl.DateTimeFormat(
      locale,
      format.type === 'date' ? { dateStyle: 'medium' } : { dateStyle: 'medium', timeStyle: 'short' }
    ).format(date);
  }
  if (format.type === 'boolean')
    return value === true || value === 1 || value === 'true'
      ? format.trueText || 'Yes'
      : format.falseText || 'No';
  const number = Number(value);
  if (!Number.isFinite(number)) return String(value);
  const decimalPlaces = format.decimalPlaces ?? (format.type === 'percentage' ? 0 : 2);
  const options: Intl.NumberFormatOptions = {
    minimumFractionDigits: decimalPlaces,
    maximumFractionDigits: decimalPlaces,
    useGrouping: format.useGrouping ?? true,
  };
  if (format.type === 'currency') {
    options.style = 'currency';
    options.currency = format.currency || 'SAR';
  } else if (format.type === 'percentage') {
    options.style = 'percent';
  }
  const formatted = new Intl.NumberFormat(locale, options).format(Math.abs(number));
  if (number >= 0) return formatted;
  if (format.negativeFormat === 'parentheses') return `(${formatted})`;
  if (format.negativeFormat === 'trailingMinus') return `${formatted}-`;
  return `-${formatted}`;
};

const ReportPageValue = ({ source }: { source?: string | null }): React.ReactElement | null => {
  if (source === 'pageNumber') return <Box component="span" className="printout-page-number" />;
  if (source === 'totalPages') return <Box component="span" className="printout-page-count" />;
  if (source === 'pageNumberOfTotal') {
    return (
      <>
        <Box component="span" className="printout-page-number" /> /{' '}
        <Box component="span" className="printout-page-count" />
      </>
    );
  }
  return null;
};

const isVisible = (element: PrintTemplateElement, data: RuntimePrintData): boolean => {
  const condition = element.visibleWhen;
  if (!condition) return true;
  const actual = resolveRuntimeBinding(data, condition.field);
  const expected = condition.value;
  switch (condition.operator) {
    case 'isEmpty':
      return empty(actual);
    case 'isNotEmpty':
      return !empty(actual);
    case '=':
      return String(actual ?? '') === String(expected ?? '');
    case '!=':
      return String(actual ?? '') !== String(expected ?? '');
    case '>':
      return Number(actual) > Number(expected);
    case '>=':
      return Number(actual) >= Number(expected);
    case '<':
      return Number(actual) < Number(expected);
    case '<=':
      return Number(actual) <= Number(expected);
    case 'contains':
      return String(actual ?? '').includes(String(expected ?? ''));
    case 'notContains':
      return !String(actual ?? '').includes(String(expected ?? ''));
    case 'in':
      return Array.isArray(expected) && expected.map(String).includes(String(actual));
    case 'notIn':
      return Array.isArray(expected) && !expected.map(String).includes(String(actual));
  }
};

function RuntimeElement({
  element,
  data,
  template,
}: {
  element: PrintTemplateElement;
  data: RuntimePrintData;
  template: PrintTemplateDocument;
}): React.ReactElement | null {
  if (!isVisible(element, data)) return null;
  const style = element.style;
  const sx = {
    width: style?.width ? `${style.width}%` : undefined,
    fontSize: style?.fontSize,
    fontWeight: style?.fontWeight,
    textAlign: style?.alignment,
    color: style?.color,
    backgroundColor: style?.backgroundColor,
    p: style?.padding != null ? `${style.padding}px` : undefined,
    mb: style?.marginBottom != null ? `${style.marginBottom}px` : undefined,
    borderWidth: style?.borderWidth != null ? `${style.borderWidth}px` : undefined,
    borderStyle: style?.borderWidth ? 'solid' : undefined,
    borderColor: style?.borderColor,
    borderRadius: style?.borderRadius != null ? `${style.borderRadius}px` : undefined,
    breakInside: style?.keepTogether ? 'avoid' : undefined,
    whiteSpace: 'pre-wrap',
    overflowWrap: 'anywhere',
    boxSizing: 'border-box',
  } as const;
  const children = (items: PrintTemplateElement[]) =>
    items.map((child) => (
      <RuntimeElement key={child.id} element={child} data={data} template={template} />
    ));
  if (element.type === 'text') return <Typography sx={sx}>{element.value}</Typography>;
  if (element.type === 'field') {
    const raw = resolveRuntimeBinding(data, element.binding);
    const missing = empty(raw);
    const reportPageValue =
      element.binding.sourceType === 'report' ? (
        <ReportPageValue source={element.binding.source} />
      ) : null;
    const value =
      reportPageValue ??
      (missing
        ? element.fallback ||
          (template.missingFieldBehavior === 'na'
            ? 'N/A'
            : template.missingFieldBehavior === 'placeholder'
              ? `{{${element.label}}}`
              : '')
        : formatPrintValue(raw, element.format, template.language));
    return (
      <Box
        className="printout-field"
        sx={{
          ...sx,
          display: 'grid',
          gridTemplateColumns: element.label ? 'minmax(30mm, .45fr) 1fr' : '1fr',
          border: '1px solid #d9e2ec',
          minHeight: 28,
        }}
      >
        <Box sx={{ px: 0.75, py: 0.5, fontWeight: 700, bgcolor: '#f3f6f9' }}>{element.label}</Box>
        <Box dir="auto" sx={{ px: 0.75, py: 0.5 }}>
          {value}
        </Box>
      </Box>
    );
  }
  if (element.type === 'section')
    return (
      <Box className="printout-section" sx={sx}>
        {element.title ? (
          <Typography
            sx={{ mb: 0.75, px: 0.75, py: 0.5, bgcolor: '#174f82', color: '#fff', fontWeight: 700 }}
          >
            {element.title}
          </Typography>
        ) : null}
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: `repeat(${Math.max(1, element.columns)}, minmax(0, 1fr))`,
            gap: 0.75,
          }}
        >
          {children(element.elements)}
        </Box>
      </Box>
    );
  if (element.type === 'row')
    return (
      <Box sx={{ ...sx, display: 'flex', gap: 0.75, alignItems: 'stretch' }}>
        {children(element.elements)}
      </Box>
    );
  if (element.type === 'column')
    return (
      <Box sx={{ ...sx, flex: Math.max(1, element.span), display: 'grid', gap: 0.75 }}>
        {children(element.elements)}
      </Box>
    );
  if (element.type === 'divider')
    return <Box sx={{ ...sx, borderTop: '1px solid #9fb3c8', my: 0.75 }} />;
  if (element.type === 'image') {
    const source = element.binding
      ? resolveRuntimeBinding(data, element.binding)
      : element.sourceType === 'companyLogo'
        ? data.company.logoSource
        : element.source;
    return source ? (
      <Box
        component="img"
        src={String(source)}
        alt={element.altText || ''}
        sx={{
          ...sx,
          display: 'block',
          height: style?.height != null ? `${style.height}px` : undefined,
          maxWidth: '100%',
          maxHeight: style?.height != null ? undefined : '35mm',
          objectFit: style?.objectFit ?? 'contain',
          mx: style?.alignment === 'center' ? 'auto' : undefined,
          marginInlineStart: style?.alignment === 'end' ? 'auto' : undefined,
        }}
      />
    ) : null;
  }
  if (element.type === 'table') {
    const resolved = resolveRuntimeBinding(data, element.dataSource);
    const rows = Array.isArray(resolved) ? resolved : [];
    return (
      <Box component="table" sx={{ ...sx, width: '100%', borderCollapse: 'collapse' }}>
        <Box component="thead" sx={{ display: 'table-header-group' }}>
          <Box component="tr">
            {element.columns.map((column) => (
              <Box
                component="th"
                key={column.id}
                sx={{
                  border: '1px solid #b8c7d9',
                  px: 0.75,
                  py: 0.5,
                  bgcolor: '#eef3f7',
                  textAlign: 'start',
                }}
              >
                {column.label}
              </Box>
            ))}
          </Box>
        </Box>
        <Box component="tbody">
          {rows.map((row, rowIndex) => (
            <Box component="tr" key={rowIndex}>
              {element.columns.map((column) => {
                const value =
                  row && typeof row === 'object'
                    ? (row as Record<string, unknown>)[column.field]
                    : undefined;
                return (
                  <Box
                    component="td"
                    key={column.id}
                    sx={{ border: '1px solid #d9e2ec', px: 0.75, py: 0.5 }}
                  >
                    {formatPrintValue(value, column.format, template.language)}
                  </Box>
                );
              })}
            </Box>
          ))}
        </Box>
      </Box>
    );
  }
  if (element.type === 'barcode' || element.type === 'qrCode') {
    const value = String(resolveRuntimeBinding(data, element.binding) ?? '');
    return (
      <Box
        sx={{
          ...sx,
          display: 'grid',
          placeItems: 'center',
          minHeight: element.type === 'qrCode' ? 92 : 58,
          border: '1px solid #172b4d',
          p: 1,
        }}
      >
        <Typography
          aria-label={element.type}
          sx={{
            fontFamily: 'monospace',
            fontSize: element.type === 'qrCode' ? 12 : 22,
            letterSpacing: element.type === 'barcode' ? 3 : 0,
          }}
        >
          {element.type === 'barcode' ? `|||| ${value} ||||` : `▦ ${value}`}
        </Typography>
      </Box>
    );
  }
  if (element.type === 'signature') {
    const value = resolveRuntimeBinding(data, element.binding);
    const image = typeof value === 'string' && value.startsWith('data:image/');
    return (
      <Box sx={{ ...sx, minHeight: 62, borderBottom: '1px solid #172b4d', p: 0.75 }}>
        {element.label ? (
          <Typography sx={{ fontWeight: 700, fontSize: 11 }}>{element.label}</Typography>
        ) : null}
        {image ? (
          <Box
            component="img"
            src={value}
            alt={element.label || ''}
            sx={{ display: 'block', maxHeight: 48, maxWidth: '100%' }}
          />
        ) : (
          <Typography dir="auto">{String(value ?? '')}</Typography>
        )}
      </Box>
    );
  }
  if (element.type === 'pageNumber')
    return (
      <Typography sx={sx}>
        <Box component="span" className="printout-page-number" />
      </Typography>
    );
  if (element.type === 'printDate')
    return (
      <Typography sx={sx}>
        {formatPrintValue(data.system.printDate, { type: 'dateTime' }, template.language)}
      </Typography>
    );
  if (element.type === 'spacer') return <Box sx={{ height: element.height }} />;
  if (element.type === 'pageBreak')
    return <Box sx={{ breakAfter: 'page', pageBreakAfter: 'always' }} />;
  return null;
}

export function RuntimePrintTemplate({ template, data, company }: Props): React.ReactElement {
  const render = (items: PrintTemplateElement[]) => (
    <Box sx={{ display: 'grid', gap: 0.75 }}>
      {items.map((element) => (
        <RuntimeElement key={element.id} element={element} data={data} template={template} />
      ))}
    </Box>
  );
  return (
    <PrintoutDocument
      company={company}
      title=""
      pageSettings={{
        paperSize: template.page.size,
        orientation: template.page.orientation,
        direction: template.direction,
        margins: template.page.margins,
      }}
      header={render(template.header)}
      footer={render(template.footer)}
      showHeader={template.header.length > 0}
      showFooter={template.footer.length > 0}
      showPageNumber={false}
    >
      {render(template.sections)}
    </PrintoutDocument>
  );
}
