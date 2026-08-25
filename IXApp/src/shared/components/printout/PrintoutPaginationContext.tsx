import React from 'react';

export interface PrintoutPaginationValue {
  currentPage: number;
  totalPages: number;
}

const PrintoutPaginationContext = React.createContext<PrintoutPaginationValue | null>(null);

export const PrintoutPaginationProvider = PrintoutPaginationContext.Provider;

export const usePrintoutPagination = (): PrintoutPaginationValue | null =>
  React.useContext(PrintoutPaginationContext);
