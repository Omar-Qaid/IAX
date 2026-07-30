export interface PaginationState {
  pageIndex: number;
  pageSize: number;
  totalCount: number;
}

export interface SortState {
  field: string;
  direction: 'asc' | 'desc';
}
