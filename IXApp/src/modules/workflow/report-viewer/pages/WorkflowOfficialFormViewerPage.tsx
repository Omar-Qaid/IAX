import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ReportViewer, type ReportExportFormat } from '@patterns/report-viewer/ReportViewer';
import { exportReportElement } from '@patterns/report-viewer/exportReport';
import { fetchreportCompany, toReportCompany } from '@shared/components/report-viewer/reportCompany';
import { recordTableId } from '@shared/components/documents';
import { useNotifications } from '@shared/hooks/useNotifications';
import { localizedName } from '@shared/utilities/localizedName';
import { wfRequestApi, type WfRequestRecord } from '../../api/wfRequestApi';
import { reportDesignerApi } from '@shared/components/report-designer';
import { ReportTemplateRenderer as PrintTemplateViewer } from '@shared/components/report-viewer';
import { createruntimeReportData } from '../utils/runtimeReportData';

interface Props {
  open: boolean;
  request: WfRequestRecord | null;
  templateId: number;
  onClose: () => void;
}

export function WorkflowOfficialFormViewer({ open, request, templateId, onClose }: Props): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const { user } = useAuth();
  const currentCompany = useCompanyStore((state) => state.currentCompany);
  const reportContainerRef = React.useRef<HTMLDivElement | null>(null);
  const requestId = request?.recId ?? 0;
  const companyCode = request?.dataAreaId || currentCompany || '';
  const [printedAt, setPrintedAt] = React.useState(() => new Date());
  
  React.useEffect(() => {
    if (open && requestId > 0) setPrintedAt(new Date());
  }, [open, requestId, templateId]);

  const publishedTemplate = useQuery({ 
    queryKey: ['workflow', 'official-form', requestId, templateId], 
    queryFn: ({ signal }) => reportDesignerApi.getPublishedForRecord(recordTableId('WfRequests'), requestId, templateId, signal), 
    enabled: open && requestId > 0 && templateId > 0 
  });
  
  const details = useQuery({ 
    queryKey: ['workflow', 'mail', 'printout-details', requestId], 
    queryFn: ({ signal }) => wfRequestApi.mailDetails(requestId, signal), 
    enabled: open && requestId > 0 
  });
  
  const reportCompany = useQuery({ 
    queryKey: ['report-company', companyCode], 
    queryFn: ({ signal }) => fetchreportCompany(companyCode, signal), 
    staleTime: 60_000, 
    enabled: open && Boolean(companyCode) 
  });
  
  const company = reportCompany.data ?? toReportCompany(undefined, companyCode);
  const requestName = request?.code || (request ? t('mail.requestFallback', { id: request.recId }) : '');
  const templateDisplayName = localizedName(publishedTemplate.data, isRtl);
  const templateLanguage = publishedTemplate.data?.document.language;
  const runtimeData = React.useMemo(() => request && details.data ? createruntimeReportData(request, details.data, company, user, printedAt, templateLanguage) : null, [company, details.data, printedAt, request, templateLanguage, user]);
  
  const print = () => {
    const previousTitle = document.title;
    document.title = `${requestName}-${publishedTemplate.data?.code || 'official-form'}`;
    window.print();
    document.title = previousTitle;
  };
  
  const exportReport = async (format: ReportExportFormat) => {
    if (!reportContainerRef.current || !publishedTemplate.data) return;
    try {
      await exportReportElement({
        element: reportContainerRef.current,
        format,
        fileName: `${requestName}-${publishedTemplate.data.code || 'official-form'}`,
        title: templateDisplayName,
        language: publishedTemplate.data.document.language,
        direction: publishedTemplate.data.document.direction,
      });
      notifySuccess(t('reportViewer.export.success', { format }));
    } catch {
      notifyError(t('reportViewer.export.failed', { format }));
    }
  };
  
  const report = publishedTemplate.data && runtimeData ? (
    <div ref={reportContainerRef}>
      <PrintTemplateViewer template={publishedTemplate.data.document} data={runtimeData} company={company} />
    </div>
  ) : undefined;
  
  const loading = publishedTemplate.isLoading || details.isLoading || reportCompany.isLoading;
  const error = publishedTemplate.isError || details.isError || reportCompany.isError ? t('mail.print.loadError') : null;
  
  return (
    <ReportViewer
      open={open}
      title={templateDisplayName || t('mail.print.officialViewerTitle', { name: requestName })}
      loading={loading}
      error={error}
      emptyMessage={t('mail.print.selectRequest')}
      viewerOptions={{ initialZoomMode: 'Automatic Zoom', direction: publishedTemplate.data?.document.direction || (isRtl ? 'rtl' : 'ltr') }}
      onClose={onClose}
      onReload={() => void Promise.all([publishedTemplate.refetch(), details.refetch(), reportCompany.refetch()])}
      onPrint={print}
      onExport={exportReport}
    >
      {report}
    </ReportViewer>
  );
}
