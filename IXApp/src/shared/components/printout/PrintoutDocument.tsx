import React from 'react';
import { Box, Stack, Typography } from '@mui/material';
import { usePrintoutPagination } from './PrintoutPaginationContext';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { APP_FONT_FAMILY } from '@shared/constants/fontFamilies';

export interface PrintoutCompany {
  name: string;
  secondaryName?: string | null;
  companyCode?: string | null;
  logoSource?: string | null;
  addressLines?: string[];
  contactLines?: string[];
  registrationLines?: string[];
}

export interface PrintoutMetadataItem {
  label: string;
  value: React.ReactNode;
}

export interface PrintoutHeaderConfig {
  subtitle?: React.ReactNode;
  period?: React.ReactNode;
  metadata?: PrintoutMetadataItem[];
  pageNumber?: number;
  totalPages?: number;
}

export interface PrintoutFooterConfig {
  confidentialityText?: React.ReactNode;
  notes?: React.ReactNode;
  generatedBy?: string | null;
  generatedAt?: string | Date | null;
  pageNumber?: number;
  totalPages?: number;
}

export interface PrintoutPageSettings {
  paperSize?: 'A4' | 'Letter';
  orientation?: 'portrait' | 'landscape';
  margin?: 'normal' | 'narrow' | 'wide';
  direction?: 'ltr' | 'rtl' | 'auto';
}

export interface PrintoutDocumentProps {
  company: PrintoutCompany;
  title: string;
  reference?: string | null;
  reportDate?: string | Date | null;
  status?: string | null;
  generatedBy?: string | null;
  generatedAt?: string | Date | null;
  criteria?: React.ReactNode;
  headerConfig?: PrintoutHeaderConfig;
  footerConfig?: PrintoutFooterConfig;
  pageSettings?: PrintoutPageSettings;
  header?: React.ReactNode;
  children: React.ReactNode;
  footer?: React.ReactNode;
  paperSize?: 'A4' | 'Letter';
  orientation?: 'portrait' | 'landscape';
  margin?: 'normal' | 'narrow' | 'wide';
  showHeader?: boolean;
  showBody?: boolean;
  showFooter?: boolean;
  showPageNumber?: boolean;
}

const documentStyles = `
  .printout-print-page-position { display: none; }
  @media print {
    html, body { background: #fff !important; }
    body * { visibility: hidden !important; }
    .printout-document, .printout-document * { visibility: visible !important; }
    .printout-document {
      position: absolute !important;
      inset: 0 auto auto 0 !important;
      width: 100% !important;
      min-height: auto !important;
      margin: 0 !important;
      padding: 0 !important;
      box-shadow: none !important;
      border: 0 !important;
    }
    .printout-screen-only { display: none !important; }
    .printout-preview-scale { zoom: 1 !important; }
    .printout-document thead { display: table-header-group; }
    .printout-document tfoot { display: table-footer-group; }
    .printout-section, .printout-field, .printout-keep-together, tr, img { break-inside: avoid; page-break-inside: avoid; }
    .printout-table thead { display: table-header-group; }
    .printout-table tfoot { display: table-footer-group; }
    .printout-page-number::after { content: counter(page); }
    .printout-page-count::after { content: counter(pages); }
    .printout-screen-page-position { display: none !important; }
    .printout-print-page-position { display: inline !important; }
    .printout-color { print-color-adjust: exact; -webkit-print-color-adjust: exact; }
  }
`;

const formatReportDate = (value: string | Date | null | undefined, locale: string): string => {
  if (!value) return '';
  const date = value instanceof Date ? value : new Date(value);
  return Number.isNaN(date.getTime()) ? String(value) : new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(date);
};

function PrintoutPagePosition({ pageNumber, totalPages }: { pageNumber: number; totalPages: number }): React.ReactElement {
  const { t } = useAppTranslation();
  return <>
    <Box component="span" className="printout-screen-page-position">{t('printout.pageOf', { page: pageNumber, total: totalPages })}</Box>
    <Box component="span" className="printout-print-page-position">{t('printout.page')} <Box component="span" className="printout-page-number" /> {t('printout.of')} <Box component="span" className="printout-page-count" /></Box>
  </>;
}

export function PrintoutHeader({ company, title, reference, reportDate, status, generatedBy, criteria, config }: { company: PrintoutCompany; title: string; reference?: string | null; reportDate?: string | Date | null; status?: string | null; generatedBy?: string | null; criteria?: React.ReactNode; config?: PrintoutHeaderConfig }): React.ReactElement {
  const { t, currentLanguage, isRtl } = useAppTranslation();
  const pagination = usePrintoutPagination();
  const pageNumber = config?.pageNumber ?? pagination?.currentPage ?? 1;
  const totalPages = config?.totalPages ?? pagination?.totalPages ?? 1;
  const primaryCompanyName = isRtl && company.secondaryName ? company.secondaryName : company.name;
  const secondaryCompanyName = isRtl && company.secondaryName ? company.name : company.secondaryName;
  return (
    <Box className="printout-header">
      <Box sx={{ display: 'grid', gridTemplateColumns: '31mm minmax(0, 1fr) 58mm', alignItems: 'center', gap: 2, minHeight: '29mm', pb: 1.5, borderBottom: '2px solid #174f82' }}>
        <Box sx={{ width: '29mm', height: '22mm', display: 'grid', placeItems: 'center' }}>
          {company.logoSource ? <Box component="img" src={company.logoSource} alt={t('printout.logoAlt', { company: primaryCompanyName })} sx={{ display: 'block', maxWidth: '100%', maxHeight: '100%', objectFit: 'contain' }} /> : null}
        </Box>
        <Box sx={{ minWidth: 0, textAlign: 'center' }}>
          <Typography dir="auto" sx={{ fontSize: 17, lineHeight: 1.25, fontWeight: 750, color: '#102a43' }}>{primaryCompanyName}</Typography>
          {secondaryCompanyName ? <Typography dir="auto" sx={{ mt: 0.25, fontSize: 13, lineHeight: 1.3, color: '#334e68' }}>{secondaryCompanyName}</Typography> : null}
          <Typography sx={{ mt: 0.75, fontSize: 15, lineHeight: 1.25, fontWeight: 700, color: '#174f82' }}>{title}</Typography>
          {config?.subtitle ? <Typography sx={{ mt: 0.25, fontSize: 9.5, color: '#334e68' }}>{config.subtitle}</Typography> : null}
          {config?.period ? <Typography sx={{ mt: 0.25, fontSize: 9, color: '#486581' }}>{t('printout.period')}: {config.period}</Typography> : null}
          {company.addressLines?.map((line) => <Typography key={line} sx={{ mt: 0.15, fontSize: 8.25, lineHeight: 1.25, color: '#486581' }}>{line}</Typography>)}
          {company.contactLines?.map((line) => <Typography key={line} sx={{ fontSize: 8.25, lineHeight: 1.25, color: '#486581' }}>{line}</Typography>)}
        </Box>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'max-content minmax(0, 1fr)', columnGap: 0.75, rowGap: 0.4, textAlign: 'start', color: '#486581' }}>
          {company.companyCode ? <Typography sx={{ fontSize: 9.5, fontWeight: 700 }}>{t('printout.company')}: <Box component="span" dir="auto">{company.companyCode}</Box></Typography> : null}
          <Box />
          {reference ? <><Typography sx={{ fontSize: 8.5, fontWeight: 700 }}>{t('printout.reportNo')}:</Typography><Typography dir="auto" sx={{ fontSize: 8.5 }}>{reference}</Typography></> : null}
          {reportDate ? <><Typography sx={{ fontSize: 8.5, fontWeight: 700 }}>{t('printout.date')}:</Typography><Typography sx={{ fontSize: 8.5 }}>{formatReportDate(reportDate, currentLanguage.code)}</Typography></> : null}
          {status ? <><Typography sx={{ fontSize: 8.5, fontWeight: 700 }}>{t('printout.status')}:</Typography><Typography dir="auto" sx={{ fontSize: 8.5 }}>{status}</Typography></> : null}
          {generatedBy ? <><Typography sx={{ fontSize: 8.5, fontWeight: 700 }}>{t('printout.generatedBy')}:</Typography><Typography dir="auto" sx={{ fontSize: 8.5 }}>{generatedBy}</Typography></> : null}
          {config?.metadata?.map((item) => <React.Fragment key={item.label}><Typography sx={{ fontSize: 8.5, fontWeight: 700 }}>{item.label}:</Typography><Box sx={{ minWidth: 0, fontSize: 8.5, overflowWrap: 'anywhere' }}>{item.value}</Box></React.Fragment>)}
          <Typography sx={{ gridColumn: '1 / -1', mt: 0.25, fontSize: 8.5, fontWeight: 700 }}><PrintoutPagePosition pageNumber={pageNumber} totalPages={totalPages} /></Typography>
          {company.registrationLines?.map((line) => <Typography key={line} sx={{ gridColumn: '1 / -1', fontSize: 8.5, lineHeight: 1.35 }}>{line}</Typography>)}
        </Box>
      </Box>
      {criteria ? <Box sx={{ px: 1, py: 0.65, borderBottom: '1px solid #d9e2ec', color: '#486581', fontSize: 8.5 }}><Box component="span" sx={{ fontWeight: 700 }}>{t('printout.criteria')}: </Box>{criteria}</Box> : null}
    </Box>
  );
}

export function PrintoutFooter({ company, reference, generatedBy, generatedAt, config, showPageNumber = true }: { company: PrintoutCompany; reference?: string | null; generatedBy?: string | null; generatedAt?: string | Date | null; config?: PrintoutFooterConfig; showPageNumber?: boolean }): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const pagination = usePrintoutPagination();
  const pageNumber = config?.pageNumber ?? pagination?.currentPage ?? 1;
  const totalPages = config?.totalPages ?? pagination?.totalPages ?? 1;
  return (
    <Box className="printout-footer">
      <Box sx={{ pt: 1.25 }}>
        <Box sx={{ borderTop: '1px solid #9fb3c8', pt: 0.75, display: 'grid', gridTemplateColumns: '1fr auto 1fr', gap: 1.5, alignItems: 'end', color: '#627d98' }}>
          <Box>
            <Typography sx={{ fontSize: 8.5, fontWeight: 700 }}>{config?.confidentialityText ?? t('printout.confidential')}</Typography>
            {config?.notes ? <Typography sx={{ fontSize: 8, lineHeight: 1.3 }}>{config.notes}</Typography> : null}
            {company.contactLines?.map((line) => <Typography key={line} sx={{ fontSize: 8, lineHeight: 1.3 }}>{line}</Typography>)}
          </Box>
          {showPageNumber ? <Typography sx={{ fontSize: 8.5, whiteSpace: 'nowrap' }}><PrintoutPagePosition pageNumber={pageNumber} totalPages={totalPages} /></Typography> : null}
          <Box sx={{ textAlign: 'end' }}>
            {config?.generatedAt ?? generatedAt ? <Typography sx={{ fontSize: 8 }}>{t('printout.generated')}: {formatReportDate(config?.generatedAt ?? generatedAt, currentLanguage.code)}</Typography> : null}
            {config?.generatedBy ?? generatedBy ? <Typography sx={{ fontSize: 8 }}>{t('printout.printedBy')}: {config?.generatedBy ?? generatedBy}</Typography> : null}
            {reference ? <Typography sx={{ fontSize: 8 }}>{reference}</Typography> : null}
          </Box>
        </Box>
      </Box>
    </Box>
  );
}

export function PrintoutDocument({ company, title, reference, reportDate, status, generatedBy, generatedAt, criteria, headerConfig, footerConfig, pageSettings, header, children, footer, paperSize = 'A4', orientation = 'portrait', margin = 'normal', showHeader = true, showBody = true, showFooter = true, showPageNumber = true }: PrintoutDocumentProps): React.ReactElement {
  const { isRtl } = useAppTranslation();
  paperSize = pageSettings?.paperSize ?? paperSize;
  orientation = pageSettings?.orientation ?? orientation;
  margin = pageSettings?.margin ?? margin;
  const portrait = orientation === 'portrait';
  const size = paperSize === 'A4' ? { width: 210, height: 297 } : { width: 216, height: 279 };
  const previewWidth = portrait ? size.width : size.height;
  const previewHeight = portrait ? size.height : size.width;
  const padding = margin === 'narrow' ? 8 : margin === 'wide' ? 20 : 14;
  const pageMargin = margin === 'narrow' ? '10mm' : margin === 'wide' ? '22mm' : '16mm';
  return (
    <>
      <style>{documentStyles}</style>
      <style>{`@page { size: ${paperSize} ${orientation}; margin: ${pageMargin}; }`}</style>
      <Box className="printout-document printout-color" dir={pageSettings?.direction ?? (isRtl ? 'rtl' : 'ltr')} sx={{ width: `${previewWidth}mm`, minHeight: `${previewHeight}mm`, mx: 'auto', p: `${padding}mm`, boxSizing: 'border-box', bgcolor: '#fff', color: '#102a43', fontFamily: APP_FONT_FAMILY, textAlign: 'start', boxShadow: '0 4px 24px rgba(15,23,42,.16)' }}>
        <Box component="table" sx={{ width: '100%', borderCollapse: 'collapse' }}>
          {showHeader ? <Box component="thead"><Box component="tr"><Box component="td" sx={{ p: 0 }}>{header ?? <PrintoutHeader company={company} title={title} reference={reference} reportDate={reportDate} status={status} generatedBy={generatedBy} criteria={criteria} config={headerConfig} />}</Box></Box></Box> : null}
          {showBody ? <Box component="tbody"><Box component="tr"><Box component="td" sx={{ p: 0, verticalAlign: 'top' }}><Stack spacing={1.5} sx={{ py: 2 }}>{children}</Stack></Box></Box></Box> : null}
          {showFooter ? <Box component="tfoot"><Box component="tr"><Box component="td" sx={{ p: 0 }}>{footer ?? <PrintoutFooter company={company} reference={reference} generatedBy={generatedBy} generatedAt={generatedAt} config={footerConfig} showPageNumber={showPageNumber} />}</Box></Box></Box> : null}
        </Box>
      </Box>
    </>
  );
}
