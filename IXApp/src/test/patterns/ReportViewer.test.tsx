import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@test/testUtils';
import {
  calculateReportPageCount,
  REPORT_EXPORT_FORMATS,
  ReportViewer,
} from '@patterns/report-viewer/ReportViewer';
import { ReportViewerDocument } from '@shared/components/report-viewer/ReportViewerDocument';
import { calculatePdfPageLayout } from '@patterns/report-viewer/exportReport';

describe('ReportViewer', () => {
  it('does not create a blank page for small preview-versus-print height differences', () => {
    expect(calculateReportPageCount(1_225, 1_123)).toBe(1);
    expect(calculateReportPageCount(1_400, 1_123)).toBe(2);
  });

  it('keeps a pixel-rounded A4 canvas on one exported PDF page', () => {
    const roundedA4Height = (1_123 * 210) / 794;
    expect(roundedA4Height).toBeGreaterThan(297);
    expect(calculatePdfPageLayout(roundedA4Height, 297)).toEqual({
      pageCount: 1,
      renderedImageHeight: 297,
    });
    expect(calculatePdfPageLayout(300, 297).pageCount).toBe(2);
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
        <ReportViewerDocument company={{ name: 'Example company' }} title="Paged report">
          <div>Only the loaded server page</div>
        </ReportViewerDocument>
      </ReportViewer>
    );

    expect(screen.getByText('of 250')).toBeInTheDocument();
    expect(screen.getByText('Only the loaded server page')).toBeInTheDocument();
    expect(screen.getAllByText('Page 3 of 250')).toHaveLength(2);
    fireEvent.click(screen.getByRole('button', { name: 'Next page' }));
    expect(onPageChange).toHaveBeenCalledWith(4);
  });

  it('offers and delegates every supported export format', async () => {
    const onExport = vi.fn();
    render(
      <ReportViewer
        open
        title="Exportable report"
        onClose={vi.fn()}
        onPrint={vi.fn()}
        onExport={onExport}
      >
        <ReportViewerDocument company={{ name: 'Example company' }} title="Exportable report">
          <div>Report contents</div>
        </ReportViewerDocument>
      </ReportViewer>
    );

    for (const format of REPORT_EXPORT_FORMATS) {
      const exportButton = await screen.findByRole('button', { name: 'Export' });
      await waitFor(() => expect(exportButton).toBeEnabled());
      fireEvent.click(exportButton);
      fireEvent.click(screen.getByRole('menuitem', { name: format }));
      await waitFor(() => expect(onExport).toHaveBeenLastCalledWith(format));
      await screen.findByRole('button', { name: 'Export' });
    }
    expect(onExport).toHaveBeenCalledTimes(REPORT_EXPORT_FORMATS.length);
  });
});
