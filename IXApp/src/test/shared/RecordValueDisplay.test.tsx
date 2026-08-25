import React from 'react';
import { describe, expect, it } from 'vitest';
import { render, screen } from '@test/testUtils';
import { RecordValueDisplay } from '@shared/components/dialogs/RecordValueDisplay';

describe('RecordValueDisplay', () => {
  it('renders workflow request XML as readable label and value rows', () => {
    const xml = '<Details><Control><ControlLabel>Total sales</ControlLabel><ControlLabelAR>إجمالي المبيعات</ControlLabelAR><ControlValue>5239</ControlValue></Control><Control><ControlLabel>cash</ControlLabel><ControlValue>2206</ControlValue></Control></Details>';

    render(<RecordValueDisplay value={xml} />);

    expect(screen.getByText('إجمالي المبيعات')).toBeDefined();
    expect(screen.getByText('5239')).toBeDefined();
    expect(screen.getByText('cash')).toBeDefined();
    expect(screen.getByText('2206')).toBeDefined();
    expect(screen.queryByText(/<ControlLabel>/)).toBeNull();
  });

  it('extracts readable fields from incomplete legacy workflow XML', () => {
    const xml = '<Details><Control><ControlLabel>Total sales</ControlLabel><ControlValue>5239</ControlValue></Control><Control><ControlLabel>cash</ControlLabel>';

    render(<RecordValueDisplay value={xml} />);

    expect(screen.getByText('Total sales')).toBeDefined();
    expect(screen.getByText('5239')).toBeDefined();
    expect(screen.queryByText(/<Details>/)).toBeNull();
  });
});
