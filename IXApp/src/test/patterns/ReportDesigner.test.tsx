import React from 'react';
import { render, screen } from '@test/testUtils';
import { ReportDesigner } from '@patterns/report-designer';

describe('ReportDesigner', () => {
  it('renders an accessible designer workspace with unconstrained child layout', () => {
    render(
      <ReportDesigner ariaLabel="Invoice report designer">
        <div>Feature toolbar</div>
        <div>Feature canvas</div>
      </ReportDesigner>
    );

    const designer = screen.getByRole('region', { name: 'Invoice report designer' });
    expect(designer).toContainElement(screen.getByText('Feature toolbar'));
    expect(designer).toContainElement(screen.getByText('Feature canvas'));
  });

  it('renders with slot-based modular architecture (toolbar, sidebar, properties, footer)', () => {
    render(
      <ReportDesigner
        ariaLabel="Modular report designer"
        toolbar={<div>Toolbar Slot</div>}
        sidebar={<div>Sidebar Slot</div>}
        properties={<div>Properties Slot</div>}
        footer={<div>Footer Slot</div>}
      >
        <div>Canvas Slot Content</div>
      </ReportDesigner>
    );

    const designer = screen.getByRole('region', { name: 'Modular report designer' });
    expect(designer).toContainElement(screen.getByText('Toolbar Slot'));
    expect(designer).toContainElement(screen.getByText('Sidebar Slot'));
    expect(designer).toContainElement(screen.getByText('Canvas Slot Content'));
    expect(designer).toContainElement(screen.getByText('Properties Slot'));
    expect(designer).toContainElement(screen.getByText('Footer Slot'));
  });

  it('renders loading overlay when isLoading is true', () => {
    render(
      <ReportDesigner
        ariaLabel="Designer loading test"
        isLoading
        loadingMessage="Loading canvas..."
      >
        <div>Canvas content</div>
      </ReportDesigner>
    );

    expect(screen.getByText('Loading canvas...')).toBeInTheDocument();
  });
});
