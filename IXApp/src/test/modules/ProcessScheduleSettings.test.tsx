import { beforeEach, expect, it, vi } from 'vitest';
import userEvent from '@testing-library/user-event';
import { render, screen } from '@test/testUtils';
import { ProcessScheduleSettings } from '@modules/process-builder/components/ProcessScheduleSettings';
import { waitFor } from '@testing-library/react';

const schedules = vi.hoisted(() => ({ load: vi.fn(), save: vi.fn() }));
vi.mock('@modules/process-builder/api/processBuilderSchedules', () => ({ processBuilderSchedules: schedules }));
beforeEach(() => { localStorage.clear(); schedules.load.mockReset().mockResolvedValue(null); schedules.save.mockReset(); });

it('recovers schedule choices for the same process without enabling other processes', async () => {
  const user = userEvent.setup();
  const view = render(<ProcessScheduleSettings processId="101" />);
  await screen.findByRole('button', { name: 'Save' });
  expect(screen.queryByRole('combobox', { name: 'Recurrence' })).toBeNull();
  await user.click(screen.getByRole('switch', { name: 'Process Scheduled' }));
  await user.click(screen.getByRole('combobox', { name: 'Recurrence' }));
  for (const name of ['Daily', 'Weekly', 'Monthly', 'Yearly'])
    expect(screen.getByRole('option', { name })).toBeDefined();
  await user.click(screen.getByRole('option', { name: 'Monthly' }));
  view.unmount();

  const recovered = render(<ProcessScheduleSettings processId="101" />);
  expect(screen.getByRole('switch', { name: 'Process Scheduled' })).toBeChecked();
  expect(screen.getByRole('combobox', { name: 'Recurrence' })).toHaveTextContent('Monthly');
  expect(screen.getByText(/save this schedule to activate background request creation/)).toBeDefined();
  recovered.unmount();

  render(<ProcessScheduleSettings processId="102" />);
  expect(screen.getByRole('switch', { name: 'Process Scheduled' })).not.toBeChecked();
});

it('loads server configuration and saves activation with its concurrency version', async () => {
  const configuration = {
    enabled: true, frequency: 'weekly', startsAt: '2026-10-01T09:00:00', timeZone: 'UTC',
    sourceType: 'employee', sourceTable: '', sourceRecord: '42', mappings: [], version: 'first',
  };
  schedules.load.mockResolvedValue(configuration);
  schedules.save.mockResolvedValue({ ...configuration, enabled: false, version: 'second' });
  const user = userEvent.setup();
  render(<ProcessScheduleSettings processId="101" />);
  await waitFor(() => expect(screen.getByRole('switch', { name: 'Process Scheduled' })).toBeChecked());
  await user.click(screen.getByRole('switch', { name: 'Process Scheduled' }));
  await user.click(screen.getByRole('button', { name: 'Save' }));
  await waitFor(() => expect(schedules.save).toHaveBeenCalledWith('101', { ...configuration, enabled: false }));
  expect(await screen.findByRole('status')).toHaveTextContent('Schedule saved on the server.');
});

it('prevents saving when server configuration cannot be loaded', async () => {
  schedules.load.mockRejectedValue(new Error('Schedule access denied'));
  render(<ProcessScheduleSettings processId="101" />);
  expect(await screen.findByRole('alert')).toHaveTextContent('Schedule access denied');
  expect(screen.getByRole('button', { name: 'Save' })).toBeDisabled();
});
