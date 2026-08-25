import React from 'react';
import { Box, Stack, Typography } from '@mui/material';

export interface PrintoutCompany {
  name: string;
  secondaryName?: string | null;
  companyCode?: string | null;
  logoSource?: string | null;
  addressLines?: string[];
  contactLines?: string[];
  registrationLines?: string[];
}

interface PrintoutDocumentProps {
  company: PrintoutCompany;
  title: string;
  reference?: string | null;
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
    .printout-section, .printout-field { break-inside: avoid; page-break-inside: avoid; }
    .printout-page-number::after { content: counter(page); }
    .printout-color { print-color-adjust: exact; -webkit-print-color-adjust: exact; }
  }
`;

export function PrintoutHeader({ company, title }: { company: PrintoutCompany; title: string }): React.ReactElement {
  return (
    <Box className="printout-header">
      <Box sx={{ display: 'grid', gridTemplateColumns: '31mm minmax(0, 1fr) 55mm', alignItems: 'center', gap: 2, minHeight: '29mm', pb: 1.5, borderBottom: '2px solid #174f82' }}>
        <Box sx={{ width: '29mm', height: '22mm', display: 'grid', placeItems: 'center' }}>
          {company.logoSource ? <Box component="img" src={company.logoSource} alt={`${company.name} logo`} sx={{ display: 'block', maxWidth: '100%', maxHeight: '100%', objectFit: 'contain' }} /> : null}
        </Box>
        <Box sx={{ minWidth: 0, textAlign: 'center' }}>
          <Typography sx={{ fontSize: 17, lineHeight: 1.25, fontWeight: 750, color: '#102a43' }}>{company.name}</Typography>
          {company.secondaryName ? <Typography dir="rtl" sx={{ mt: 0.25, fontSize: 13, lineHeight: 1.3, color: '#334e68' }}>{company.secondaryName}</Typography> : null}
          <Typography sx={{ mt: 0.75, fontSize: 15, lineHeight: 1.25, fontWeight: 700, color: '#174f82' }}>{title}</Typography>
        </Box>
        <Box sx={{ textAlign: 'right', color: '#486581' }}>
          {company.companyCode ? <Typography sx={{ fontSize: 9.5, fontWeight: 700 }}>Company: {company.companyCode}</Typography> : null}
          {company.addressLines?.map((line) => <Typography key={line} sx={{ fontSize: 8.5, lineHeight: 1.35 }}>{line}</Typography>)}
          {company.contactLines?.map((line) => <Typography key={line} sx={{ fontSize: 8.5, lineHeight: 1.35 }}>{line}</Typography>)}
          {company.registrationLines?.map((line) => <Typography key={line} sx={{ fontSize: 8.5, lineHeight: 1.35 }}>{line}</Typography>)}
        </Box>
      </Box>
    </Box>
  );
}

export function PrintoutFooter({ company, reference, children, showPageNumber = true }: { company: PrintoutCompany; reference?: string | null; children?: React.ReactNode; showPageNumber?: boolean }): React.ReactElement {
  return (
    <Box className="printout-footer">
      <Box sx={{ pt: 1.25 }}>
        <Box sx={{ borderTop: '1px solid #9fb3c8', pt: 0.75, display: 'grid', gridTemplateColumns: '1fr auto 1fr', gap: 1, color: '#627d98' }}>
          <Typography sx={{ fontSize: 8.5 }}>{company.name}</Typography>
          <Typography sx={{ fontSize: 8.5 }}>{children ?? 'Workflow mail printout'}{showPageNumber ? ' · Page ' : ''}<Box component="span" className="printout-page-number" /></Typography>
          <Typography sx={{ fontSize: 8.5, textAlign: 'right' }}>{reference || ''}</Typography>
        </Box>
      </Box>
    </Box>
  );
}

export function PrintoutDocument({ company, title, reference, children, footer, paperSize = 'A4', orientation = 'portrait', margin = 'normal', showHeader = true, showBody = true, showFooter = true, showPageNumber = true }: PrintoutDocumentProps): React.ReactElement {
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
      <Box className="printout-document printout-color" sx={{ width: `${previewWidth}mm`, minHeight: `${previewHeight}mm`, mx: 'auto', p: `${padding}mm`, boxSizing: 'border-box', bgcolor: '#fff', color: '#102a43', fontFamily: 'Arial, "Segoe UI", sans-serif', boxShadow: '0 4px 24px rgba(15,23,42,.16)' }}>
        <Box component="table" sx={{ width: '100%', borderCollapse: 'collapse' }}>
          {showHeader ? <Box component="thead"><Box component="tr"><Box component="td" sx={{ p: 0 }}><PrintoutHeader company={company} title={title} /></Box></Box></Box> : null}
          {showBody ? <Box component="tbody"><Box component="tr"><Box component="td" sx={{ p: 0, verticalAlign: 'top' }}><Stack spacing={1.5} sx={{ py: 2 }}>{children}</Stack></Box></Box></Box> : null}
          {showFooter ? <Box component="tfoot"><Box component="tr"><Box component="td" sx={{ p: 0 }}><PrintoutFooter company={company} reference={reference} showPageNumber={showPageNumber}>{footer}</PrintoutFooter></Box></Box></Box> : null}
        </Box>
      </Box>
    </>
  );
}
