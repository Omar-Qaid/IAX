import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAppStore } from '@app/store/useAppStore';
import { ReportViewer, type ReportExportFormat } from '@patterns/report-viewer/ReportViewer';
import { PrintoutDocument } from '@shared/components/printout/PrintoutDocument';
import { legalEntityService } from '@modules/organization/services/legalEntityService';
import { wfRequestApi, type WfRequestRecord } from '../api/wfRequestApi';
import { WorkflowMailPrintoutBody, toPrintoutCompany } from '../components/WorkflowMailPrintout';

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
  const requestId = request?.recId ?? 0;
  const currentCompany = useAppStore((state) => state.currentCompany);
  const reportContainerRef = React.useRef<HTMLDivElement | null>(null);
  const details = useQuery({ queryKey: ['workflow', 'mail', 'printout-details', requestId], queryFn: ({ signal }) => wfRequestApi.mailDetails(requestId, signal), enabled: open && requestId > 0 });
  const companies = useQuery({ queryKey: ['legal-entities', 'workflow-mail-printout'], queryFn: ({ signal }) => legalEntityService.list(signal), staleTime: 60_000, enabled: open });
  const companyCode = request?.dataAreaId || currentCompany;
  const entity = companies.data?.find((item) => item.dataArea.localeCompare(companyCode, undefined, { sensitivity: 'accent' }) === 0) ?? companies.data?.find((item) => item.dataArea.localeCompare(currentCompany, undefined, { sensitivity: 'accent' }) === 0);
  const company = toPrintoutCompany(entity, companyCode);
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
    const html = `<!doctype html><html><head><meta charset="utf-8"><title>${baseName}</title></head><body>${reportContainerRef.current?.innerHTML ?? ''}</body></html>`;
    downloadText(html, `${baseName}.${format === 'Word' ? 'doc' : 'mhtml'}`, format === 'Word' ? 'application/msword' : 'multipart/related');
  };

  const loading = details.isLoading || companies.isLoading;
  const error = details.isError || companies.isError ? 'Unable to load the workflow mail printout.' : null;
  const report = request && details.data ? <div ref={reportContainerRef}><PrintoutDocument company={company} title="Workflow Mail" reference={request.code || `Request ${request.recId}`}><WorkflowMailPrintoutBody request={request} details={details.data} /></PrintoutDocument></div> : undefined;

  return <ReportViewer open={open} title={`Workflow Mail · ${baseName}`} loading={loading} error={error} emptyMessage="Select a workflow request to print." onClose={onClose} onReload={() => void Promise.all([details.refetch(), companies.refetch()])} onPrint={print} onExport={exportReport}>{report}</ReportViewer>;
}
