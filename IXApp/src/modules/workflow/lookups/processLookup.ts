import { wfProcessApi } from '../api/wfProcessApi';

export const processLookupColumns = [
  { field: 'code', header: 'wfProcess.fields.code', width: 120 },
  { field: 'name', header: 'wfProcess.fields.name', flex: 1 },
  { field: 'nameAR', header: 'wfProcess.fields.nameAR', flex: 1 },
] as const;

export const fetchProcessPage = async ({
  pageNumber,
  pageSize,
  search,
  signal,
}: {
  pageNumber: number;
  pageSize: number;
  search: string;
  signal?: AbortSignal;
}) => {
  const processes = await wfProcessApi.list(signal);
  const query = search.trim().toLocaleLowerCase();
  const filtered = query
    ? processes.filter((process) =>
        `${process.code ?? ''} ${process.name ?? ''} ${process.nameAR ?? ''}`
          .toLocaleLowerCase()
          .includes(query)
      )
    : processes;
  const start = (pageNumber - 1) * pageSize;
  return {
    data: filtered.slice(start, start + pageSize),
    pageNumber,
    totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
    totalRecords: filtered.length,
  };
};
