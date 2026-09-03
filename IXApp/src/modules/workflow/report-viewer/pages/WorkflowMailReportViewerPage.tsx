import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { ReportViewer, type ReportExportFormat } from '@patterns/report-viewer/ReportViewer';
import { exportReportElement } from '@patterns/report-viewer/exportReport';
import { ReportViewerDocument } from '@shared/components/report-viewer/ReportViewerDocument';
import { fetchreportCompany, toReportCompany } from '@shared/components/report-viewer/reportCompany';
import { wfRequestApi, type WfRequestRecord } from '../api/wfRequestApi';
import { getLocalizedMailStatus, WorkflowMailReportViewerBody } from '../components/WorkflowMailReportViewer';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { recordTableId } from '@shared/components/documents';
import { useNotifications } from '@shared/hooks/useNotifications';
import { wfProcessApi } from '../api/wfProcessApi';
import { localizedName } from '@shared/utilities/localizedName';

interface WorkflowMailReportViewerViewerProps {
  open: boolean;
  request: WfRequestRecord | null;
  onClose: () => void;
}

export function WorkflowMailReportViewerViewer({ open, request, onClose }: WorkflowMailReportViewerViewerProps): React.ReactElement {
  const { t, currentLanguage, isRtl } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
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
  const process = useQuery({ queryKey: ['workflow', 'mail', 'printout-process', request?.processId], queryFn: () => wfProcessApi.getById(Number(request?.processId)), enabled: open && Number(request?.processId) > 0 });
  const companyCode = request?.dataAreaId || currentCompany || '';
  const reportCompany = useQuery({ queryKey: ['report-company', companyCode], queryFn: ({ signal }) => fetchreportCompany(companyCode, signal), staleTime: 60_000, enabled: open && Boolean(companyCode) });
  const company = reportCompany.data ?? toReportCompany(undefined, companyCode);
  const baseName = request?.code || `workflow-request-${requestId}`;
  const processDisplayName = localizedName(process.data, isRtl) || details.data?.processName || '';

  const print = () => {
    const previousTitle = document.title;
    document.title = baseName;
    window.print();
    document.title = previousTitle;
  };
  const exportReport = async (format: ReportExportFormat) => {
    if (!reportContainerRef.current) return;
    try {
      await exportReportElement({
        element: reportContainerRef.current,
        format,
        fileName: baseName,
        title: processDisplayName || baseName,
        language: currentLanguage.code,
        direction,
      });
      notifySuccess(t('reportViewer.export.success', { format }));
    } catch {
      notifyError(t('reportViewer.export.failed', { format }));
    }
  };

  const loading = details.isLoading || reportCompany.isLoading || process.isLoading;
  const error = details.isError || reportCompany.isError || process.isError ? t('mail.print.loadError') : null;
  const report = request && details.data ? <div ref={reportContainerRef}><ReportViewerDocument company={company} title={t('mail.print.title')} reference={request.code || t('mail.requestFallback', { id: request.recId })} reportDate={details.data.requestDate} status={getLocalizedMailStatus(t, details.data.status)} generatedBy={user?.displayName || user?.username} headerConfig={{ subtitle: processDisplayName }} footerConfig={{ generatedBy: user?.displayName || user?.username, generatedAt }} pageSettings={{ paperSize: 'A4', orientation: 'portrait', margin: 'normal', direction }}><WorkflowMailReportViewerBody request={request} details={details.data} processName={processDisplayName} /></ReportViewerDocument></div> : undefined;

  return <ReportViewer open={open} title={t('mail.print.viewerTitle', { name: baseName })} loading={loading} error={error} emptyMessage={t('mail.print.selectRequest')} viewerOptions={{ initialZoomMode: 'Automatic Zoom', direction }} onClose={onClose} onReload={() => void Promise.all([details.refetch(), reportCompany.refetch(), process.refetch()])} onPrint={print} onExport={exportReport}>{report}</ReportViewer>;
}
