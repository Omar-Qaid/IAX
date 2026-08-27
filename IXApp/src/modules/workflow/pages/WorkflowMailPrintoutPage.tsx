import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { ReportViewer, type ReportExportFormat } from '@patterns/report-viewer/ReportViewer';
import { PrintoutDocument } from '@shared/components/printout/PrintoutDocument';
import { fetchPrintoutCompany, toPrintoutCompany } from '@shared/components/printout/reportCompany';
import { wfRequestApi, type WfRequestRecord } from '../api/wfRequestApi';
import { getLocalizedMailStatus, WorkflowMailPrintoutBody } from '../components/WorkflowMailPrintout';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface WorkflowMailPrintoutViewerProps {
  open: boolean;
  request: WfRequestRecord | null;
  onClose: () => void;
}

const downloadText = (contents: string, fileName: string, type: string) => {
  const url = URL.createObjectURL(new Blob([contents], { type }));
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = fileName;
  anchor.click();
  URL.revokeObjectURL(url);
};

export function WorkflowMailPrintoutViewer({ open, request, onClose }: WorkflowMailPrintoutViewerProps): React.ReactElement {
  const { t, currentLanguage, isRtl } = useAppTranslation();
  const direction = isRtl ? 'rtl' : 'ltr';
  const requestId = request?.recId ?? 0;
  const currentCompany = useCompanyStore((state) => state.currentCompany);
  const { user } = useAuth();
  const reportContainerRef = React.useRef<HTMLDivElement | null>(null);
  const [generatedAt, setGeneratedAt] = React.useState(() => new Date());
  React.useEffect(() => {
    if (open && requestId > 0) setGeneratedAt(new Date());
  }, [open, requestId]);
  const details = useQuery({ queryKey: ['workflow', 'mail', 'printout-details', requestId], queryFn: ({ signal }) => wfRequestApi.mailDetails(requestId, signal), enabled: open && requestId > 0 });
  const companyCode = request?.dataAreaId || currentCompany || '';
  const reportCompany = useQuery({ queryKey: ['report-company', companyCode], queryFn: ({ signal }) => fetchPrintoutCompany(companyCode, signal), staleTime: 60_000, enabled: open && Boolean(companyCode) });
  const company = reportCompany.data ?? toPrintoutCompany(undefined, companyCode);
  const baseName = request?.code || `workflow-request-${requestId}`;

  const print = () => {
    const previousTitle = document.title;
    document.title = baseName;
    window.print();
    document.title = previousTitle;
  };
  const exportReport = (format: ReportExportFormat) => {
    if (format === 'PDF' || format === 'TIFF') return print();
    const fields = details.data?.fields ?? [];
    if (format === 'CSV' || format === 'Excel') {
      const csv = ['Label,Value', ...fields.map((field) => `"${field.label.replaceAll('"', '""')}","${field.value.replaceAll('"', '""')}"`)].join('\r\n');
      return downloadText(csv, `${baseName}.${format === 'Excel' ? 'xls' : 'csv'}`, format === 'Excel' ? 'application/vnd.ms-excel' : 'text/csv');
    }
    if (format === 'XML') {
      const xml = `<?xml version="1.0" encoding="UTF-8"?><workflow-mail request="${requestId}">${fields.map((field) => `<field label="${field.label.replaceAll('&', '&amp;').replaceAll('"', '&quot;')}">${field.value.replaceAll('&', '&amp;').replaceAll('<', '&lt;')}</field>`).join('')}</workflow-mail>`;
      return downloadText(xml, `${baseName}.xml`, 'application/xml');
    }
    const html = `<!doctype html><html lang="${currentLanguage.code}" dir="${direction}"><head><meta charset="utf-8"><title>${baseName}</title></head><body>${reportContainerRef.current?.innerHTML ?? ''}</body></html>`;
    downloadText(html, `${baseName}.${format === 'Word' ? 'doc' : 'mhtml'}`, format === 'Word' ? 'application/msword' : 'multipart/related');
  };

  const loading = details.isLoading || reportCompany.isLoading;
  const error = details.isError || reportCompany.isError ? t('mail.print.loadError') : null;
  const report = request && details.data ? <div ref={reportContainerRef}><PrintoutDocument company={company} title={t('mail.print.title')} reference={request.code || t('mail.requestFallback', { id: request.recId })} reportDate={details.data.requestDate} status={getLocalizedMailStatus(t, details.data.status)} generatedBy={user?.displayName || user?.username} headerConfig={{ subtitle: details.data.processName }} footerConfig={{ generatedBy: user?.displayName || user?.username, generatedAt }} pageSettings={{ paperSize: 'A4', orientation: 'portrait', margin: 'normal', direction }}><WorkflowMailPrintoutBody request={request} details={details.data} /></PrintoutDocument></div> : undefined;

  return <ReportViewer open={open} title={t('mail.print.viewerTitle', { name: baseName })} loading={loading} error={error} emptyMessage={t('mail.print.selectRequest')} viewerOptions={{ initialZoomMode: 'Automatic Zoom', direction }} onClose={onClose} onReload={() => void Promise.all([details.refetch(), reportCompany.refetch()])} onPrint={print} onExport={exportReport}>{report}</ReportViewer>;
}
