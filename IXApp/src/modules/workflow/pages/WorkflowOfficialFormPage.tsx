import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ReportViewer } from '@patterns/report-viewer/ReportViewer';
import { fetchPrintoutCompany, toPrintoutCompany } from '@shared/components/printout/reportCompany';
import { printTemplateApi } from '../print-templates/api/printTemplateApi';
import { RuntimePrintTemplate } from '../print-templates/runtime/RuntimePrintTemplate';
import { createRuntimePrintData } from '../print-templates/runtime/runtimePrintData';
import { wfRequestApi, type WfRequestRecord } from '../api/wfRequestApi';

interface Props {
  open: boolean;
  request: WfRequestRecord | null;
  templateId: number;
  onClose: () => void;
}

export function WorkflowOfficialFormViewer({ open, request, templateId, onClose }: Props): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const { user } = useAuth();
  const currentCompany = useCompanyStore((state) => state.currentCompany);
  const requestId = request?.recId ?? 0;
  const companyCode = request?.dataAreaId || currentCompany || '';
  const [printedAt, setPrintedAt] = React.useState(() => new Date());
  React.useEffect(() => {
    if (open && requestId > 0) setPrintedAt(new Date());
  }, [open, requestId, templateId]);

  const publishedTemplate = useQuery({ queryKey: ['workflow', 'official-form', requestId, templateId], queryFn: ({ signal }) => printTemplateApi.getPublishedForRequest(requestId, templateId, signal), enabled: open && requestId > 0 && templateId > 0 });
  const details = useQuery({ queryKey: ['workflow', 'mail', 'printout-details', requestId], queryFn: ({ signal }) => wfRequestApi.mailDetails(requestId, signal), enabled: open && requestId > 0 });
  const reportCompany = useQuery({ queryKey: ['report-company', companyCode], queryFn: ({ signal }) => fetchPrintoutCompany(companyCode, signal), staleTime: 60_000, enabled: open && Boolean(companyCode) });
  const company = reportCompany.data ?? toPrintoutCompany(undefined, companyCode);
  const requestName = request?.code || (request ? t('mail.requestFallback', { id: request.recId }) : '');
  const templateLanguage = publishedTemplate.data?.document.language;
  const runtimeData = React.useMemo(() => request && details.data ? createRuntimePrintData(request, details.data, company, user, printedAt, templateLanguage) : null, [company, details.data, printedAt, request, templateLanguage, user]);
  const print = () => {
    const previousTitle = document.title;
    document.title = `${requestName}-${publishedTemplate.data?.code || 'official-form'}`;
    window.print();
    document.title = previousTitle;
  };
  const report = publishedTemplate.data && runtimeData ? <RuntimePrintTemplate template={publishedTemplate.data.document} data={runtimeData} company={company} /> : undefined;
  const loading = publishedTemplate.isLoading || details.isLoading || reportCompany.isLoading;
  const error = publishedTemplate.isError || details.isError || reportCompany.isError ? t('mail.print.loadError') : null;
  return (
    <ReportViewer
      open={open}
      title={publishedTemplate.data?.name || t('mail.print.officialViewerTitle', { name: requestName })}
      loading={loading}
      error={error}
      emptyMessage={t('mail.print.selectRequest')}
      exportFormats={[]}
      viewerOptions={{ initialZoomMode: 'Automatic Zoom', direction: publishedTemplate.data?.document.direction || (isRtl ? 'rtl' : 'ltr') }}
      onClose={onClose}
      onReload={() => void Promise.all([publishedTemplate.refetch(), details.refetch(), reportCompany.refetch()])}
      onPrint={print}
      onExport={() => undefined}
    >
      {report}
    </ReportViewer>
  );
}
