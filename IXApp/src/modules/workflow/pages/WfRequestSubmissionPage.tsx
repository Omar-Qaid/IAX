import React from 'react';
import { Box, Button, CircularProgress, Stack, Typography } from '@mui/material';
import DescriptionOutlined from '@mui/icons-material/DescriptionOutlined';
import CalendarMonthOutlined from '@mui/icons-material/CalendarMonthOutlined';
import StarBorderOutlined from '@mui/icons-material/StarBorderOutlined';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { localizedName } from '@shared/utilities/localizedName';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailSectionConfig, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { wfCategoryApi, type WfCategoryRecord } from '../api/wfCategoryApi';
import { wfProcessApi } from '../api/wfProcessApi';
import { WORKFLOW_ROUTE_PATHS } from '../routes/workflowRoutePaths';

const emptyCategory = (): WfCategoryRecord => ({
  id: 'empty-category', recId: 0, code: null, name: '', description: null, sysField: false,
  sortOrder: 0, isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
});

const processIcon = (index: number) => {
  const Icon = [DescriptionOutlined, CalendarMonthOutlined, StarBorderOutlined][index % 3];
  return <Icon sx={{ fontSize: 30 }} />;
};

function CategoryRequestForm({ category }: { category: WfCategoryRecord }) {
  const navigate = useNavigate();
  const { t, isRtl } = useAppTranslation();
  const processes = useQuery({
    queryKey: ['workflow', 'request-submission-processes', category.recId],
    queryFn: async ({ signal }) =>
      (await wfProcessApi.list(signal))
        .filter((process) => process.isActive && process.categoryId === category.recId)
        .sort((left, right) => left.sortOrder - right.sortOrder),
  });

  return (
    <Stack spacing={2}>
      <Box>
        <Typography sx={{ fontWeight: 700, mb: 1 }}>{t('pages.requestSubmission.chooseType')}</Typography>
        {processes.isLoading ? <CircularProgress size={22} aria-label={t('pages.requestSubmission.loadingTypes')} /> : (
          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(180px, 1fr))', gap: 1.25 }}>
            {(processes.data ?? []).map((process, index) => {
              return (
                <Button
                  key={process.id}
                  variant="outlined"
                  onClick={() => navigate(WORKFLOW_ROUTE_PATHS.requestFrom(category.recId, process.recId))}
                  sx={{ minHeight: 112, p: 1.5, display: 'flex', flexDirection: 'column', gap: 0.75,
                    borderColor: 'divider', bgcolor: '#fff', color: 'text.primary', textTransform: 'none',
                    '&:hover': { borderColor: 'primary.main', bgcolor: 'rgba(99, 91, 255, 0.08)', color: 'primary.main' } }}
                >
                  {processIcon(index)}
                  <Typography dir="auto" sx={{ fontWeight: 700, textAlign: 'center' }}>{localizedName(process, isRtl) || process.code}</Typography>
                  {process.description && <Typography dir="auto" variant="caption" color="text.secondary" sx={{ textAlign: 'center' }}>{process.description}</Typography>}
                </Button>
              );
            })}
          </Box>
        )}
        {!processes.isLoading && (processes.data ?? []).length === 0 && (
          <Typography color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>{t('pages.requestSubmission.noActiveTypes')}</Typography>
        )}
      </Box>
    </Stack>
  );
}

export function RequestSubmissionPage(): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const categories = useQuery({
    queryKey: ['workflow', 'request-submission-categories'],
    queryFn: ({ signal }) => wfCategoryApi.list(signal),
  });
  const records = React.useMemo(
    () => [...(categories.data ?? [])].sort((left, right) => left.sortOrder - right.sortOrder),
    [categories.data]
  );
  const requestDate = React.useMemo(() => new Date().toISOString().slice(0, 10), []);

  const config: EnterpriseListDetailsConfig<WfCategoryRecord> = {
    readOnly: true,
    dataSource: { type: 'controlled', records, onRecordsChange: () => undefined,
      loading: categories.isLoading, error: categories.error instanceof Error ? categories.error.message : null,
      refresh: async () => { await categories.refetch(); } },
    createRecord: emptyCategory,
    getPrimaryText: (category) => localizedName(category, isRtl) || category.code || t('pages.requestSubmission.unnamedCategory'),
    getSecondaryText: (category) => category.description || category.code || '',
    matchesSearch: (category, query) => `${category.code ?? ''} ${category.name ?? ''} ${category.nameAlias ?? ''} ${category.description ?? ''}`
      .toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: () => ({}),
    setValues: (category) => category,
    headerFields: [
      { id: 'code', label: t('pages.requestSubmission.categoryCode'), disabled: true, getValue: (category) => category.code ?? '', setValue: (category) => category },
      { id: 'name', label: t('pages.requestSubmission.category'), disabled: true, getValue: (category) => localizedName(category, isRtl), setValue: (category) => category },
      { id: 'requestDate', label: t('pages.requestSubmission.requestDate'), disabled: true, getValue: () => requestDate, setValue: (category) => category },
    ],
    sections: ({ record }): DetailSectionConfig[] => [{ id: 'request-submission', title: t('pages.requestSubmission.title'),
      defaultExpanded: true, content: <CategoryRequestForm key={record.id} category={record} /> }],
    advancedFilter: { fieldLabel: t('pages.requestSubmission.category'), getValue: (category) => localizedName(category, isRtl),
      matches: (category, value) => localizedName(category, isRtl).toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()) },
    showAttachmentAction: false,
  };
  return <ListDetailsPage variant="enterprise" title={t('pages.requestSubmission.title')} config={config} />;
}
