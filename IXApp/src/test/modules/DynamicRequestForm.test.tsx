import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { DynamicControlRenderer } from '@modules/workflow/components/DynamicControlRenderer';
import { DynamicForm } from '@modules/workflow/components/DynamicForm';
import { dynamicRequestFormApi } from '@modules/workflow/api/dynamicRequestFormApi';

describe('DynamicControlRenderer', () => {
  it('renders a configured label control as display-only content', () => {
    render(<DynamicControlRenderer control={{ label: 'Applicant information', labelColor: '#2457a7', controlType: 'label' }} value="" onChange={() => undefined} />);
    expect(screen.getByRole('note')).toHaveTextContent('Applicant information');
    expect(screen.getByText('Applicant information')).toHaveStyle({ color: '#2457a7', fontWeight: '800' });
    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
  });

  it('renders database-provided select options and reports the configured value', async () => {
    const onChange = vi.fn();
    render(<DynamicControlRenderer control={{
      label: 'Travel class', controlType: 'dropdown-manual',
      options: [{ value: 'economy', label: 'Economy' }, { value: 'business', label: 'Business' }],
    }} value="" onChange={onChange} />);

    await userEvent.click(screen.getByRole('combobox', { name: 'Travel class' }));
    await userEvent.click(screen.getByRole('option', { name: 'Business' }));
    expect(onChange).toHaveBeenCalledWith('business');
  });

  it('shows compact metadata indicators on configured options', () => {
    render(<DynamicControlRenderer control={{
      label: 'Action', controlType: 'radiobuttonlist',
      options: [{
        value: 'deduction', label: 'Salary deduction', sendsNotification: true,
        requiresAttachment: true, revealsControls: true,
      }],
    }} value="" onChange={() => undefined} />);

    expect(screen.getByRole('radio', { name: /Salary deduction/ })).toBeInTheDocument();
    expect(screen.getByTestId('NotificationsNoneOutlinedIcon')).toBeVisible();
    expect(screen.getByTestId('AttachFileOutlinedIcon')).toBeVisible();
    expect(screen.getByTestId('SubdirectoryArrowRightOutlinedIcon')).toBeVisible();
  });

  it('reveals dependent controls directly beneath the selected option', async () => {
    vi.spyOn(dynamicRequestFormApi, 'getDefinition').mockResolvedValue({
      processId: 591,
      processName: 'Disciplinary Process',
      processDescription: null,
      controls: [
        {
          requestControlId: 10, controlId: 1, code: 'action', label: 'Action', labelAr: null,
          labelColor: null, controlType: 'radiobuttonlist', sortOrder: 10, score: 0,
          required: false, readOnly: false, uniqueKey: false, usedAsCriteria: false,
          defaultValue: '', visibilityCondition: null, validations: [],
          options: [{
            optionId: 100, value: 'deduction', label: 'Salary deduction', score: 20, sortOrder: 10,
            featureConfiguration: {
              requireFileUpload: true, sendAlertMessage: false, alertMessage: '', performerIds: [],
              showOtherControls: true, visibleControlIds: [20],
            },
          }, {
            optionId: 101, value: 'termination', label: 'Termination', score: 30, sortOrder: 20,
            featureConfiguration: {
              requireFileUpload: false, sendAlertMessage: false, alertMessage: '', performerIds: [],
              showOtherControls: true, visibleControlIds: [20, 21],
            },
          }],
        },
        {
          requestControlId: 20, controlId: 2, code: 'amount', label: 'Amount', labelAr: null,
          labelColor: null, controlType: 'digits', sortOrder: 20, score: 0,
          required: true, readOnly: false, uniqueKey: false, usedAsCriteria: false,
          defaultValue: '', visibilityCondition: null, validations: [], options: [],
        },
        {
          requestControlId: 21, controlId: 5, code: 'reason', label: 'Reason', labelAr: null,
          labelColor: null, controlType: 'text', sortOrder: 21, score: 0,
          required: true, readOnly: false, uniqueKey: false, usedAsCriteria: false,
          defaultValue: '', visibilityCondition: null, validations: [], options: [],
        },
        {
          requestControlId: 30, controlId: 3, code: 'employee', label: 'Employee', labelAr: null,
          labelColor: null, controlType: 'text', sortOrder: 30, score: 0,
          required: false, readOnly: false, uniqueKey: false, usedAsCriteria: false,
          defaultValue: '', visibilityCondition: null, validations: [], options: [],
        },
        {
          requestControlId: 40, controlId: 4, code: 'date', label: 'Date', labelAr: null,
          labelColor: null, controlType: 'date', sortOrder: 40, score: 0,
          required: false, readOnly: false, uniqueKey: false, usedAsCriteria: false,
          defaultValue: '', visibilityCondition: null, validations: [], options: [],
        },
      ],
    });
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    render(<QueryClientProvider client={queryClient}><DynamicForm processId={591} showActions={false} /></QueryClientProvider>);

    await screen.findByRole('radio', { name: /Salary deduction/ });
    expect(Array.from(screen.getByTestId('dynamic-form-grid').querySelectorAll('[data-control-id]')).map((element) => element.getAttribute('data-control-id'))).toEqual(['10', '30', '40']);
    expect(screen.queryByLabelText('Amount')).not.toBeInTheDocument();
    await userEvent.click(screen.getByRole('radio', { name: /Salary deduction/ }));

    const dependencies = await screen.findByRole('group', { name: 'Salary deduction additional fields' });
    await waitFor(() => expect(within(dependencies).getByLabelText('Amount')).toBeVisible());
    expect(within(dependencies).getByRole('button', { name: 'Attach Supporting document' })).toBeVisible();

    await userEvent.click(screen.getByRole('radio', { name: /Termination/ }));
    const fullRowDependencies = await screen.findByRole('group', { name: 'Termination additional fields' });
    expect(within(fullRowDependencies).getByLabelText('Amount')).toBeVisible();
    expect(within(fullRowDependencies).getByLabelText('Reason')).toBeVisible();
    expect(screen.getAllByTestId('dynamic-form-row')[0]).toContainElement(fullRowDependencies);
  });

  it('serializes checkbox-list selections for metadata submission', async () => {
    const onChange = vi.fn();
    render(<DynamicControlRenderer control={{
      label: 'Services', controlType: 'checkboxlist',
      options: [{ value: 'laptop', label: 'Laptop' }, { value: 'vpn', label: 'VPN' }],
    }} value="[]" onChange={onChange} />);

    await userEvent.click(screen.getByRole('checkbox', { name: 'VPN' }));
    expect(onChange).toHaveBeenCalledWith('["vpn"]');
  });

  it('supports canonical radio and calendar metadata types', () => {
    const { rerender } = render(<DynamicControlRenderer control={{
      label: 'Decision', controlType: 'radio',
      options: [{ value: 'yes', label: 'Yes' }, { value: 'no', label: 'No' }],
    }} value="yes" onChange={() => undefined} />);
    expect(screen.getByRole('radio', { name: 'Yes' })).toBeChecked();

    rerender(<DynamicControlRenderer control={{ label: 'Travel date', controlType: 'calendar' }}
      value="2026-08-20" onChange={() => undefined} />);
    expect(screen.getByLabelText('Travel date')).toHaveAttribute('type', 'date');
  });

  it('renders a metadata-validated drag and drop file control', () => {
    const onChange = vi.fn();
    const fileControl = {
      label: 'Documents', controlType: 'file',
      validations: [
        { type: 'fileExtensions', value: 'pdf', errorMessage: 'PDF files only.' },
        { type: 'fileSize', value: '2 MB', errorMessage: 'File is too large.' },
        { type: 'maxFiles', value: '2', errorMessage: 'Only two files.' },
      ],
    };
    const { container, rerender } = render(<DynamicControlRenderer control={fileControl} value="" onChange={onChange} />);

    expect(screen.getByRole('button', { name: 'Upload Documents' })).toHaveTextContent('Drop documents here or Browse');
    expect(screen.getByRole('button', { name: 'Upload Documents' })).toHaveStyle({ height: '30px' });
    expect(screen.getByRole('button', { name: 'Upload Files' })).toBeVisible();
    const input = container.querySelector('input[type="file"]');
    expect(input).toHaveAttribute('multiple');
    fireEvent.change(input!, { target: { files: [new File(['content'], 'document.pdf', { type: 'application/pdf' })] } });
    expect(onChange).toHaveBeenCalledWith(expect.stringContaining('document.pdf'));

    rerender(<DynamicControlRenderer control={fileControl} value={'[{"n":"document.pdf","s":7,"t":"application/pdf"}]'} onChange={onChange} />);
    expect(screen.getByRole('button', { name: 'Upload More Files' })).toBeVisible();
    fireEvent.click(screen.getByRole('button', { name: 'Remove All Files' }));
    expect(onChange).toHaveBeenLastCalledWith('[]');
  });

  it('renders compact attachments as wrapping chips with preview, remove, and add actions', async () => {
    const onChange = vi.fn();
    render(<DynamicControlRenderer
      control={{ label: 'Supporting documents', controlType: 'file', compact: true, hideLabel: true }}
      value={'[{"n":"1001.jpg","s":1024,"t":"image/jpeg"},{"n":"a-very-long-supporting-document-name.pdf","s":2048,"t":"application/pdf"}]'}
      onChange={onChange}
    />);

    const attachments = screen.getByTestId('compact-attachments');
    expect(attachments).toHaveStyle({ display: 'flex', flexWrap: 'wrap' });
    expect(within(attachments).getByRole('button', { name: 'Preview 1001.jpg' })).toBeVisible();
    expect(within(attachments).getByRole('button', { name: 'Preview a-very-long-supporting-document-name.pdf' })).toHaveStyle({ maxWidth: '160px' });
    expect(within(attachments).getByRole('button', { name: 'Add another file' })).toBeVisible();

    fireEvent.click(within(attachments).getByLabelText('Remove 1001.jpg'));
    expect(onChange).toHaveBeenLastCalledWith(expect.not.stringContaining('1001.jpg'));
    expect(onChange).toHaveBeenLastCalledWith(expect.stringContaining('a-very-long-supporting-document-name.pdf'));
  });

  it('shows image thumbnails and opens a larger preview popup', async () => {
    const createDescriptor = Object.getOwnPropertyDescriptor(URL, 'createObjectURL');
    const revokeDescriptor = Object.getOwnPropertyDescriptor(URL, 'revokeObjectURL');
    Object.defineProperty(URL, 'createObjectURL', { configurable: true, value: vi.fn(() => 'blob:photo-preview') });
    Object.defineProperty(URL, 'revokeObjectURL', { configurable: true, value: vi.fn() });
    const onChange = vi.fn();
    const control = { label: 'Evidence', controlType: 'file' };
    const { container, rerender, unmount } = render(<DynamicControlRenderer control={control} value="" onChange={onChange} />);

    fireEvent.change(container.querySelector('input[type="file"]')!, {
      target: { files: [new File(['image'], 'photo.png', { type: 'image/png' })] },
    });
    rerender(<DynamicControlRenderer control={control} value={'[{"n":"photo.png","s":5,"t":"image/png"}]'} onChange={onChange} />);

    expect(screen.getByRole('button', { name: 'Preview photo.png' }).querySelector('img')).toHaveAttribute('src', 'blob:photo-preview');
    await userEvent.click(screen.getByRole('button', { name: 'Preview photo.png' }));
    expect(screen.getByRole('dialog', { name: /Preview photo.png/ })).toBeVisible();
    expect(screen.getByAltText('Preview of photo.png')).toHaveAttribute('src', 'blob:photo-preview');
    await userEvent.click(screen.getByRole('button', { name: 'Close file preview' }));
    expect(screen.queryByRole('dialog', { name: /Preview photo.png/ })).not.toBeInTheDocument();

    unmount();
    if (createDescriptor) Object.defineProperty(URL, 'createObjectURL', createDescriptor); else Reflect.deleteProperty(URL, 'createObjectURL');
    if (revokeDescriptor) Object.defineProperty(URL, 'revokeObjectURL', revokeDescriptor); else Reflect.deleteProperty(URL, 'revokeObjectURL');
  });

  it('renders signature and location controls from canonical metadata types', async () => {
    const canvasContext = {
      clearRect: vi.fn(), beginPath: vi.fn(), moveTo: vi.fn(), lineTo: vi.fn(), stroke: vi.fn(),
      lineWidth: 1, lineCap: 'round', lineJoin: 'round', strokeStyle: '#000',
    } as unknown as CanvasRenderingContext2D;
    const contextSpy = vi.spyOn(HTMLCanvasElement.prototype, 'getContext').mockReturnValue(canvasContext);
    const { rerender } = render(<DynamicControlRenderer control={{ label: 'E-Signature', controlType: 'signature' }} value="" onChange={() => undefined} />);
    expect(screen.queryByLabelText('Signature drawing area')).not.toBeInTheDocument();
    expect(screen.getByLabelText('E-Signature signature preview')).toHaveStyle({ height: '30px' });
    await userEvent.click(screen.getByRole('button', { name: 'Sign' }));
    expect(screen.getByLabelText('Signature drawing area')).toBeVisible();
    expect(screen.getByRole('toolbar', { name: 'Signature actions' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Clear Signature' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel Signature' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Save Signature' })).toBeDisabled();

    rerender(<DynamicControlRenderer control={{ label: 'Work location', controlType: 'location' }} value="" onChange={() => undefined} />);
    expect(screen.queryByRole('application', { name: 'Location map' })).not.toBeInTheDocument();
    expect(screen.getByLabelText('Work location location preview')).toHaveStyle({ height: '30px' });
    await userEvent.click(screen.getByRole('button', { name: 'Open Map' }));
    expect(screen.getByRole('application', { name: 'Location map' })).toBeVisible();
    expect(screen.getByRole('dialog', { name: 'Work location' })).toBeVisible();
    expect(screen.queryByRole('textbox', { name: 'Work location' })).not.toBeInTheDocument();
    contextSpy.mockRestore();
  });

  it('keeps signature inline and opens only the location editor in a dialog', async () => {
    const canvasContext = {
      clearRect: vi.fn(), beginPath: vi.fn(), moveTo: vi.fn(), lineTo: vi.fn(), stroke: vi.fn(),
      lineWidth: 1, lineCap: 'round', lineJoin: 'round', strokeStyle: '#000',
    } as unknown as CanvasRenderingContext2D;
    const contextSpy = vi.spyOn(HTMLCanvasElement.prototype, 'getContext').mockReturnValue(canvasContext);
    const { rerender } = render(<DynamicControlRenderer control={{ label: 'E-Signature', controlType: 'signature' }} value="" onChange={() => undefined} />);
    expect(screen.queryByLabelText('Signature drawing area')).not.toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Sign' }));
    expect(screen.getByLabelText('Signature drawing area')).toBeVisible();
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

    rerender(<DynamicControlRenderer control={{ label: 'Work location', controlType: 'location' }} value="" onChange={() => undefined} />);
    expect(screen.queryByRole('application', { name: 'Location map' })).not.toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Open Map' }));
    expect(screen.getByRole('application', { name: 'Location map' })).toBeVisible();
    expect(screen.getByRole('dialog', { name: 'Work location' })).toBeVisible();
    contextSpy.mockRestore();
  });

  it('opens signature and location editors with one click on their preview areas', () => {
    const canvasContext = {
      clearRect: vi.fn(), beginPath: vi.fn(), moveTo: vi.fn(), lineTo: vi.fn(), stroke: vi.fn(),
      lineWidth: 1, lineCap: 'round', lineJoin: 'round', strokeStyle: '#000',
    } as unknown as CanvasRenderingContext2D;
    const contextSpy = vi.spyOn(HTMLCanvasElement.prototype, 'getContext').mockReturnValue(canvasContext);
    const { rerender } = render(<DynamicControlRenderer control={{ label: 'E-Signature', controlType: 'signature' }} value="sig:0,0;10,10" onChange={() => undefined} />);

    fireEvent.click(screen.getByLabelText('E-Signature signature preview'));
    expect(screen.getByLabelText('Signature drawing area')).toBeVisible();

    rerender(<DynamicControlRenderer control={{ label: 'Work location', controlType: 'location' }} value="" onChange={() => undefined} />);
    fireEvent.click(screen.getByLabelText('Work location location preview'));
    expect(screen.getByRole('dialog', { name: 'Work location' })).toBeVisible();
    contextSpy.mockRestore();
  });

  it('offers edit and remove actions over a saved location', () => {
    const onChange = vi.fn();
    render(<DynamicControlRenderer control={{ label: 'Work location', controlType: 'location' }}
      value={'{"address":"Jeddah","latitude":21.5433,"longitude":39.1728}'} onChange={onChange} />);

    expect(screen.getByLabelText('Work location location preview')).toHaveStyle({ height: '30px' });
    expect(screen.getByRole('toolbar', { name: 'Location actions' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Edit Location' })).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Remove Location' }));
    expect(onChange).toHaveBeenCalledWith('');
  });

  it('offers overlay actions and saves a signature when clicking outside', () => {
    const canvasContext = {
      clearRect: vi.fn(), beginPath: vi.fn(), moveTo: vi.fn(), lineTo: vi.fn(), stroke: vi.fn(),
      lineWidth: 1, lineCap: 'round', lineJoin: 'round', strokeStyle: '#000',
    } as unknown as CanvasRenderingContext2D;
    const contextSpy = vi.spyOn(HTMLCanvasElement.prototype, 'getContext').mockReturnValue(canvasContext);
    const onChange = vi.fn();
    render(<><DynamicControlRenderer control={{ label: 'E-Signature', controlType: 'signature' }}
      value="sig:0,0;10,10" onChange={onChange} /><button>Outside</button></>);

    expect(screen.getByRole('toolbar', { name: 'Saved signature actions' })).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Edit Signature' }));
    fireEvent.pointerDown(screen.getByRole('button', { name: 'Outside' }));
    expect(onChange).toHaveBeenCalledWith('sig:0,0;10,10');
    contextSpy.mockRestore();
  });

  it('shows a compact warning when browser location permission is blocked', async () => {
    const originalGeolocation = Object.getOwnPropertyDescriptor(navigator, 'geolocation');
    const getCurrentPosition = vi.fn((_success: PositionCallback, error: PositionErrorCallback) => error({ code: 1, message: 'Permission denied' } as GeolocationPositionError));
    Object.defineProperty(navigator, 'geolocation', { configurable: true, value: { getCurrentPosition } });
    render(<DynamicControlRenderer control={{ label: 'Work location', controlType: 'location' }} value="" onChange={() => undefined} />);

    await userEvent.click(screen.getByRole('button', { name: 'Open Map' }));
    expect(screen.getByRole('alert')).toHaveTextContent('Location access is blocked');
    expect(getCurrentPosition).toHaveBeenCalledTimes(1);

    if (originalGeolocation) Object.defineProperty(navigator, 'geolocation', originalGeolocation);
    else Reflect.deleteProperty(navigator, 'geolocation');
  });
});
