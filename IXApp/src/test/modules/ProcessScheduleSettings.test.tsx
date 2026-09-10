import { beforeEach, expect, it } from 'vitest';
import userEvent from '@testing-library/user-event';
import { render, screen } from '@test/testUtils';
import { ProcessScheduleSettings } from '@modules/process-builder/components/ProcessScheduleSettings';

beforeEach(() => localStorage.clear());

it('recovers schedule choices for the same process without enabling other processes', async () => {
  const user = userEvent.setup();
  const view = render(<ProcessScheduleSettings processId="101" />);
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
  expect(screen.getByText(/Automatic request creation is not connected yet/)).toBeDefined();
  recovered.unmount();

  render(<ProcessScheduleSettings processId="102" />);
  expect(screen.getByRole('switch', { name: 'Process Scheduled' })).not.toBeChecked();
});
