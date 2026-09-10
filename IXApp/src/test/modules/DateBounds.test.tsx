import { describe, expect, it } from 'vitest';
import { render, screen } from '@test/testUtils';
import { dateBoundValid, resolveDateBound } from '@modules/workflow/components/dateBounds';
import { DynamicControlRenderer } from '@modules/workflow/components/DynamicControlRenderer';

describe('configurable date bounds', () => {
  const today = new Date('2024-02-29T22:00:00Z');
  it.each([
    ['today', '2024-02-29'], ['today+1y', '2025-02-28'], ['today-30d', '2024-01-30'],
    ['today+6m', '2024-08-29'], ['2026-12-31', '2026-12-31'], ['2025-02-29', null], ['yesterday', null],
  ])('resolves %s as %s', (expression, expected) => expect(resolveDateBound(expression, today)).toBe(expected));
  it('clamps month offsets and includes the entire maximum date', () => {
    expect(resolveDateBound('today+1m', new Date('2024-01-31T12:00:00Z'))).toBe('2024-02-29');
    expect(dateBoundValid('maxDate', '2025-02-28T23:59:59', 'today+1y', today)).toBe(true);
    expect(dateBoundValid('maxDate', '2025-03-01', 'today+1y', today)).toBe(false);
    expect(dateBoundValid('minDate', '2024-02-28', 'today', today)).toBe(false);
    expect(dateBoundValid('minDate', '2024-02-29T99:99', 'today', today)).toBe(false);
  });
  it('sets picker limits per control and reports manually entered invalid dates', () => {
    render(<DynamicControlRenderer control={{ label: 'Visit date', controlType: 'date', validations: [
      { type: 'minDate', value: '2026-01-01', errorMessage: 'Too early' },
      { type: 'maxDate', value: '2026-12-31', errorMessage: 'Too late' },
    ] }} value="2025-12-31" onChange={() => undefined} />);
    const input = screen.getByLabelText('Visit date');
    expect(input).toHaveAttribute('min', '2026-01-01');
    expect(input).toHaveAttribute('max', '2026-12-31');
    expect(input).toHaveAttribute('aria-invalid', 'true');
    expect(screen.getByText('Too early')).toBeDefined();
  });
});
