import React from 'react';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { DynamicForm, type DynamicFormHandle } from '@modules/workflow/components/DynamicForm';
import { dynamicRequestFormApi, type DynamicRequestFormDefinition } from '@modules/workflow/api/dynamicRequestFormApi';
import { inputMaskValid, supportedSubmissionRule } from '@modules/workflow/components/submissionRules';
import type { PrintTemplateDocument } from '@shared/components/report-designer';
import { toReportCompany } from '@shared/components/report-viewer/reportCompany';

vi.mock('@shared/components/report-viewer', () => ({
  requestControlTemplateBindings: (template: { bound?: boolean }) => template.bound ? [{ requestControlId: 10 }] : [],
  ReportTemplateRenderer: ({ template, renderRequestControl }: { template: { bound?: boolean }; renderRequestControl: (binding: { requestControlId: number }) => React.ReactNode }) =>
    <div>{template.bound && renderRequestControl({ requestControlId: 10 })}</div>,
}));

const definition = (): DynamicRequestFormDefinition => ({
  processId: 1, processName: 'Test', processDescription: null,
  controls: [{
    requestControlId: 10, controlId: 1, code: 'reason', label: 'Reason', labelAr: null, labelColor: null,
    controlType: 'text', sortOrder: 1, score: 0, required: true, readOnly: false, uniqueKey: false,
    usedAsCriteria: false, defaultValue: 'Original', visibilityCondition: null, options: [], validations: [],
  }],
});

afterEach(() => vi.restoreAllMocks());

function mount(props: React.ComponentProps<typeof DynamicForm> = { processId: 1 }) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  const ref = React.createRef<DynamicFormHandle>();
  const view = render(<QueryClientProvider client={client}><DynamicForm ref={ref} {...props} /></QueryClientProvider>);
  return { ...view, client, ref };
}

describe('Submission failure recovery', () => {
  it('sends files with the submission and permits retry after failure without losing answers', async () => {
    vi.spyOn(dynamicRequestFormApi, 'getDefinition').mockResolvedValue(definition());
    const submit = vi.spyOn(dynamicRequestFormApi, 'submit').mockRejectedValueOnce(new Error('File storage unavailable'))
      .mockResolvedValue({ requestId: 42, code: 'REQ-42', score: 0, attachmentOwners: [] });
    const file = new File(['proof'], 'proof.pdf', { type: 'application/pdf' });
    const { ref } = mount({ processId: 1, requestFiles: [file] });
    fireEvent.change(await screen.findByLabelText(/Reason/), { target: { value: 'Keep this answer' } });
    await act(async () => { ref.current!.submit(); });
    await screen.findByText('File storage unavailable');
    expect(screen.getByLabelText(/Reason/)).toHaveValue('Keep this answer');
    expect(submit).toHaveBeenLastCalledWith(expect.objectContaining({ values: [{ requestControlId: 10, value: 'Keep this answer' }] }),
      [{ file, requestControlId: null, optionId: null }]);
    await act(async () => { ref.current!.submit(); });
    await waitFor(() => expect(screen.getByRole('button', { name: /^Submit Request$/i })).toBeDisabled());
    await act(async () => { ref.current!.submit(); });
    expect(submit).toHaveBeenCalledTimes(2);
  });

  it('ignores simultaneous submit calls and does not reset edits on definition refresh', async () => {
    vi.spyOn(dynamicRequestFormApi, 'getDefinition').mockResolvedValue(definition());
    let finish!: (value: { requestId: number; code: null; score: number; attachmentOwners: [] }) => void;
    const submit = vi.spyOn(dynamicRequestFormApi, 'submit').mockImplementation(() => new Promise(resolve => { finish = resolve; }));
    const { client, ref } = mount();
    fireEvent.change(await screen.findByLabelText(/Reason/), { target: { value: 'Keep me' } });
    act(() => client.setQueryData(['workflow', 'dynamic-request-form', 1], { ...definition(), processName: 'Updated' }));
    expect(screen.getByLabelText(/Reason/)).toHaveValue('Keep me');
    act(() => { ref.current!.submit(); ref.current!.submit(); });
    expect(submit).toHaveBeenCalledTimes(1);
    await act(async () => finish({ requestId: 1, code: null, score: 0, attachmentOwners: [] }));
  });
});

describe('Print mode attachments', () => {
  it.each([true, false])('renders required option uploads when the parent is template-bound: %s', async (bound) => {
    const form = definition();
    form.controls[0] = { ...form.controls[0], controlType: 'radio', defaultValue: 'yes', options: [{
      optionId: 5, value: 'yes', label: 'Yes', score: 0, sortOrder: 1,
      featureConfiguration: { requireFileUpload: true, showOtherControls: false, visibleControlIds: [], sendAlertMessage: false, alertMessage: '', performerIds: [] },
    }] };
    vi.spyOn(dynamicRequestFormApi, 'getDefinition').mockResolvedValue(form);
    mount({ processId: 1, displayMode: 'printTemplate', printTemplate: { bound } as unknown as PrintTemplateDocument, printCompany: toReportCompany(undefined, 'dat') });
    expect(await screen.findAllByRole('button', { name: 'Attach Supporting document' })).toHaveLength(1);
  });
});

describe('Input masks', () => {
  it.each([
    ['123-AB', '000-AA', true], ['12x-AB', '000-AA', false], ['123-ABx', '000-AA', false],
    ['A9', '\\A0', true], ['a7', '**', true], ['', '', false],
  ] as const)('validates %s against %s', (value, mask, expected) => expect(inputMaskValid(value, mask)).toBe(expected));

  it('recognizes database uniqueness but rejects unknown rules', () => {
    expect(supportedSubmissionRule('uniqueglobal')).toBe(true);
    expect(supportedSubmissionRule('misspelled')).toBe(false);
  });
});
