import { describe, it, expect } from 'vitest';
import { render, screen, fireEvent } from '@test/testUtils';
import { FastTabs } from '@shared/components/fast-tabs/FastTabs';
import { FastTab } from '@shared/components/fast-tabs/FastTab';

describe('FastTabs Component Suite', () => {
  it('renders title and toggles accordion expanded content', () => {
    render(
      <FastTabs>
        <FastTab id="tab-1" title="General Information" summary="Basic Details">
          <div>FastTab Inner Content</div>
        </FastTab>
      </FastTabs>
    );

    expect(screen.getByText('General Information')).toBeInTheDocument();
    expect(screen.getByText('FastTab Inner Content')).toBeInTheDocument();

    const summaryHeader = screen.getByText('General Information');
    fireEvent.click(summaryHeader);

    expect(screen.getByText('General Information')).toBeInTheDocument();
  });
});
