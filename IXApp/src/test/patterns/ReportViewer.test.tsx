import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@test/testUtils';
import { calculateReportPageCount, ReportViewer } from '@patterns/report-viewer/ReportViewer';
import { PrintoutDocument } from '@shared/components/printout/PrintoutDocument';

describe('ReportViewer', () => {
  it('does not create a blank page for small preview-versus-print height differences', () => {
    expect(calculateReportPageCount(1_225, 1_123)).toBe(1);
    expect(calculateReportPageCount(1_400, 1_123)).toBe(2);
  });

  it('delegates controlled page changes without rendering the complete dataset', () => {
    const onPageChange = vi.fn();
    render(
      <ReportViewer
        open
        title="Paged report"
        pagination={{ currentPage: 3, totalPages: 250, onPageChange }}
        onClose={vi.fn()}
        onPrint={vi.fn()}
        onExport={vi.fn()}
      >
        <PrintoutDocument company={{ name: 'Example company' }} title="Paged report">
          <div>Only the loaded server page</div>
        </PrintoutDocument>
      </ReportViewer>
    );

    expect(screen.getByText('of 250')).toBeInTheDocument();
    expect(screen.getByText('Only the loaded server page')).toBeInTheDocument();
    expect(screen.getAllByText('Page 3 of 250')).toHaveLength(2);
    fireEvent.click(screen.getByRole('button', { name: 'Next page' }));
    expect(onPageChange).toHaveBeenCalledWith(4);
  });
});
