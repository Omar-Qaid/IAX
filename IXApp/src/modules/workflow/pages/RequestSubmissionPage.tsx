import React from 'react';
import { Box, Button, CircularProgress, Stack, Typography } from '@mui/material';
import DescriptionOutlined from '@mui/icons-material/DescriptionOutlined';
import CalendarMonthOutlined from '@mui/icons-material/CalendarMonthOutlined';
import StarBorderOutlined from '@mui/icons-material/StarBorderOutlined';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
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
  const [selectedProcessId, setSelectedProcessId] = React.useState(0);
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
        <Typography sx={{ fontWeight: 700, mb: 1 }}>Choose a request type</Typography>
        {processes.isLoading ? <CircularProgress size={22} aria-label="Loading request types" /> : (
          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(180px, 1fr))', gap: 1.25 }}>
            {(processes.data ?? []).map((process, index) => {
              const selected = selectedProcessId === process.recId;
              return (
                <Button
                  key={process.id}
                  variant="outlined"
                  aria-pressed={selected}
                  onClick={() => {
                    setSelectedProcessId(process.recId);
                    navigate(WORKFLOW_ROUTE_PATHS.requestFrom(category.recId, process.recId));
                  }}
                  sx={{ minHeight: 112, p: 1.5, display: 'flex', flexDirection: 'column', gap: 0.75,
                    borderColor: selected ? 'primary.main' : 'divider',
                    bgcolor: selected ? 'rgba(99, 91, 255, 0.08)' : '#fff',
                    color: selected ? 'primary.main' : 'text.primary', textTransform: 'none' }}
                >
                  {processIcon(index)}
                  <Typography sx={{ fontWeight: 700, textAlign: 'center' }}>{process.name || process.code}</Typography>
                  {process.description && <Typography variant="caption" color="text.secondary" sx={{ textAlign: 'center' }}>{process.description}</Typography>}
                </Button>
              );
            })}
          </Box>
        )}
        {!processes.isLoading && (processes.data ?? []).length === 0 && (
          <Typography color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>No active request types are available in this category.</Typography>
        )}
      </Box>
      {/* Submission is intentionally handled outside this category-selection page.
        {saving ? 'Submitting…' : 'Submit request'}
      */}
    </Stack>
  );
}

export function RequestSubmissionPage(): React.ReactElement {
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
    getPrimaryText: (category) => category.name || category.code || 'Unnamed category',
    getSecondaryText: (category) => category.description || category.code || '',
    matchesSearch: (category, query) => `${category.code ?? ''} ${category.name ?? ''} ${category.description ?? ''}`
      .toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: () => ({}),
    setValues: (category) => category,
    headerFields: [
      { id: 'code', label: 'Category code', disabled: true, getValue: (category) => category.code ?? '', setValue: (category) => category },
      { id: 'name', label: 'Category', disabled: true, getValue: (category) => category.name ?? '', setValue: (category) => category },
      { id: 'requestDate', label: 'Request date', disabled: true, getValue: () => requestDate, setValue: (category) => category },
    ],
    sections: ({ record }): DetailSectionConfig[] => [{ id: 'request-submission', title: 'Request Submission',
      defaultExpanded: true, content: <CategoryRequestForm key={record.id} category={record} /> }],
    advancedFilter: { fieldLabel: 'Category', getValue: (category) => category.name,
      matches: (category, value) => (category.name ?? '').toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()) },
  };
  return <ListDetailsPage variant="enterprise" title="Request Submission" config={config} />;
}
