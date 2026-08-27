import React from 'react';
import { Box, Typography } from '@mui/material';
import { PrintoutDocument, type PrintoutCompany } from '@shared/components/printout/PrintoutDocument';
import type { PrintTemplateDocument, PrintTemplateElement, PrintValueFormat } from '../types/printTemplate.types';
import { resolveRuntimeBinding, type RuntimePrintData } from './runtimePrintData';

interface Props { template: PrintTemplateDocument; data: RuntimePrintData; company: PrintoutCompany }

const empty = (value: unknown) => value == null || value === '' || (Array.isArray(value) && value.length === 0);

const formatValue = (value: unknown, format: PrintValueFormat | null | undefined, locale: string): string => {
  if (empty(value)) return '';
  if (!format || format.type === 'text') return String(value);
  if (format.type === 'date' || format.type === 'dateTime') {
    const date = new Date(String(value));
    if (Number.isNaN(date.getTime())) return String(value);
    return new Intl.DateTimeFormat(locale, format.type === 'date' ? { dateStyle: 'medium' } : { dateStyle: 'medium', timeStyle: 'short' }).format(date);
  }
  if (format.type === 'boolean') return value === true || value === 1 || value === 'true' ? format.trueText || 'Yes' : format.falseText || 'No';
  const number = Number(value);
  if (!Number.isFinite(number)) return String(value);
  if (format.type === 'currency') return new Intl.NumberFormat(locale, { style: 'currency', currency: format.currency || 'USD' }).format(number);
  if (format.type === 'percentage') return new Intl.NumberFormat(locale, { style: 'percent' }).format(number);
  return new Intl.NumberFormat(locale).format(number);
};

const isVisible = (element: PrintTemplateElement, data: RuntimePrintData): boolean => {
  const condition = element.visibleWhen;
  if (!condition) return true;
  const actual = resolveRuntimeBinding(data, condition.field);
  const expected = condition.value;
  switch (condition.operator) {
    case 'isEmpty': return empty(actual);
    case 'isNotEmpty': return !empty(actual);
    case '=': return String(actual ?? '') === String(expected ?? '');
    case '!=': return String(actual ?? '') !== String(expected ?? '');
    case '>': return Number(actual) > Number(expected);
    case '>=': return Number(actual) >= Number(expected);
    case '<': return Number(actual) < Number(expected);
    case '<=': return Number(actual) <= Number(expected);
    case 'contains': return String(actual ?? '').includes(String(expected ?? ''));
    case 'notContains': return !String(actual ?? '').includes(String(expected ?? ''));
    case 'in': return Array.isArray(expected) && expected.map(String).includes(String(actual));
    case 'notIn': return Array.isArray(expected) && !expected.map(String).includes(String(actual));
  }
};

function RuntimeElement({ element, data, template }: { element: PrintTemplateElement; data: RuntimePrintData; template: PrintTemplateDocument }): React.ReactElement | null {
  if (!isVisible(element, data)) return null;
  const style = element.style;
  const sx = { width: style?.width ? `${style.width}%` : undefined, fontSize: style?.fontSize, fontWeight: style?.fontWeight, textAlign: style?.alignment, color: style?.color, backgroundColor: style?.backgroundColor, breakInside: style?.keepTogether ? 'avoid' : undefined, whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' } as const;
  const children = (items: PrintTemplateElement[]) => items.map((child) => <RuntimeElement key={child.id} element={child} data={data} template={template} />);
  if (element.type === 'text') return <Typography sx={sx}>{element.value}</Typography>;
  if (element.type === 'field') {
    const raw = resolveRuntimeBinding(data, element.binding);
    const missing = empty(raw);
    const value = missing ? element.fallback || (template.missingFieldBehavior === 'na' ? 'N/A' : template.missingFieldBehavior === 'placeholder' ? `{{${element.label}}}` : '') : formatValue(raw, element.format, template.language);
    return <Box className="printout-field" sx={{ ...sx, display: 'grid', gridTemplateColumns: element.label ? 'minmax(30mm, .45fr) 1fr' : '1fr', border: '1px solid #d9e2ec', minHeight: 28 }}><Box sx={{ px: .75, py: .5, fontWeight: 700, bgcolor: '#f3f6f9' }}>{element.label}</Box><Box dir="auto" sx={{ px: .75, py: .5 }}>{value}</Box></Box>;
  }
  if (element.type === 'section') return <Box className="printout-section" sx={sx}>{element.title ? <Typography sx={{ mb: .75, px: .75, py: .5, bgcolor: '#174f82', color: '#fff', fontWeight: 700 }}>{element.title}</Typography> : null}<Box sx={{ display: 'grid', gridTemplateColumns: `repeat(${Math.max(1, element.columns)}, minmax(0, 1fr))`, gap: .75 }}>{children(element.elements)}</Box></Box>;
  if (element.type === 'row') return <Box sx={{ ...sx, display: 'flex', gap: .75, alignItems: 'stretch' }}>{children(element.elements)}</Box>;
  if (element.type === 'column') return <Box sx={{ ...sx, flex: Math.max(1, element.span), display: 'grid', gap: .75 }}>{children(element.elements)}</Box>;
  if (element.type === 'divider') return <Box sx={{ ...sx, borderTop: '1px solid #9fb3c8', my: .75 }} />;
  if (element.type === 'image') {
    const source = element.binding ? resolveRuntimeBinding(data, element.binding) : element.sourceType === 'companyLogo' ? data.company.logoSource : element.source;
    return source ? <Box component="img" src={String(source)} alt={element.altText || ''} sx={{ ...sx, display: 'block', maxWidth: '100%', maxHeight: '35mm', objectFit: 'contain', mx: style?.alignment === 'center' ? 'auto' : undefined }} /> : null;
  }
  if (element.type === 'pageNumber') return <Typography sx={sx}><Box component="span" className="printout-page-number" /></Typography>;
  if (element.type === 'printDate') return <Typography sx={sx}>{formatValue(data.system.printDate, { type: 'dateTime' }, template.language)}</Typography>;
  if (element.type === 'spacer') return <Box sx={{ height: element.height }} />;
  if (element.type === 'pageBreak') return <Box sx={{ breakAfter: 'page', pageBreakAfter: 'always' }} />;
  return null;
}

export function RuntimePrintTemplate({ template, data, company }: Props): React.ReactElement {
  const render = (items: PrintTemplateElement[]) => <Box sx={{ display: 'grid', gap: .75 }}>{items.map((element) => <RuntimeElement key={element.id} element={element} data={data} template={template} />)}</Box>;
  return <PrintoutDocument company={company} title="" pageSettings={{ paperSize: template.page.size, orientation: template.page.orientation, direction: template.direction, margins: template.page.margins }} header={render(template.header)} footer={render(template.footer)} showHeader={template.header.length > 0} showFooter={template.footer.length > 0} showPageNumber={false}>{render(template.sections)}</PrintoutDocument>;
}
