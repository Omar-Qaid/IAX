import React from 'react';

export interface PrintoutPaginationValue {
  currentPage: number;
  totalPages: number;
}

const ReportViewerPaginationContext = React.createContext<PrintoutPaginationValue | null>(null);

export const ReportViewerPaginationProvider = ReportViewerPaginationContext.Provider;

export const useReportViewerPagination = (): PrintoutPaginationValue | null =>
  React.useContext(ReportViewerPaginationContext);
