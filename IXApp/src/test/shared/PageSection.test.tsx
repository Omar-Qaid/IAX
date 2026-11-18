import { describe, expect, it } from 'vitest';
import { render, screen } from '@test/testUtils';
import { PageSection } from '@shared/components/page/PageSection';

describe('PageSection style composition', () => {
  it('applies callback and array overrides without changing sibling defaults', () => {
    render(
      <>
        <PageSection data-testid="default">Default</PageSection>
        <PageSection data-testid="callback" sx={() => ({ marginBottom: '19px' })}>
          Callback
        </PageSection>
        <PageSection
          data-testid="array"
          sx={[{ marginBottom: '11px' }, () => ({ marginBottom: '23px' })]}
        >
          Array
        </PageSection>
      </>
    );
    expect(screen.getByTestId('callback')).toHaveStyle({ marginBottom: '19px' });
    expect(screen.getByTestId('array')).toHaveStyle({ marginBottom: '23px' });
    expect(screen.getByTestId('default')).not.toHaveStyle({ marginBottom: '23px' });
  });
});
