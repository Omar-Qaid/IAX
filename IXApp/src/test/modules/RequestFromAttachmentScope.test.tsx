import React from 'react';
import { fireEvent, render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { expect, it, vi } from 'vitest';
import { RequestFromPage } from '@modules/workflow/pages/WfRequestFromPage';
import type { WfProcessRecord } from '@modules/workflow/api/wfProcessApi';

vi.mock('react-router-dom', () => ({ useParams: () => ({ categoryId: '1', processId: '1' }) }));
vi.mock('@modules/workflow/api/wfProcessApi', () => ({ wfProcessApi: { list: async () => [
  { id: '1', recId: 1, categoryId: 1, name: 'Process A', sortOrder: 1 },
  { id: '2', recId: 2, categoryId: 1, name: 'Process B', sortOrder: 2 },
] } }));
vi.mock('@modules/workflow/report-designer/api/reportDesignerApi', () => ({ reportDesignerApi: { listPublished: async () => [] } }));
vi.mock('@modules/workflow/components/DynamicForm', () => ({
  DynamicForm: React.forwardRef(function Form({ requestFiles }: { requestFiles: File[] }, _ref) {
    return <div data-testid="queued-files">{requestFiles.map(file => file.name).join(',')}</div>;
  }),
}));
vi.mock('@patterns/list-details/ListDetailsPage', () => ({
  ListDetailsPage: ({ config }: { config: {
    dataSource: { records: WfProcessRecord[] };
    onSelectionChange: (record: WfProcessRecord) => void;
    sections: (context: { record: WfProcessRecord }) => Array<{ content: React.ReactNode }>;
    actionPaneEndContent: React.ReactNode;
  } }) => {
    const [selected, setSelected] = React.useState(1);
    const record = config.dataSource.records.find(item => item.recId === selected);
    return <>
      {config.actionPaneEndContent}
      {config.dataSource.records.map(item => <button key={item.id} onClick={() => {
        setSelected(item.recId); config.onSelectionChange(item);
      }}>{item.name}</button>)}
      {record && config.sections({ record }).map((section, index) => <React.Fragment key={index}>{section.content}</React.Fragment>)}
    </>;
  },
}));

it('clears queued request files when switching processes', async () => {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  const { container } = render(<QueryClientProvider client={client}><RequestFromPage /></QueryClientProvider>);
  await screen.findByRole('button', { name: 'Process B' });
  const file = new File(['private proof'], 'process-a.pdf');
  fireEvent.change(container.querySelector('input[type="file"]')!, { target: { files: [file] } });
  expect(screen.getByTestId('queued-files')).toHaveTextContent('process-a.pdf');
  fireEvent.click(screen.getByRole('button', { name: 'Process B' }));
  expect(screen.getByTestId('queued-files')).toBeEmptyDOMElement();
  fireEvent.click(screen.getByRole('button', { name: 'Process A' }));
  expect(screen.getByTestId('queued-files')).toBeEmptyDOMElement();
});
