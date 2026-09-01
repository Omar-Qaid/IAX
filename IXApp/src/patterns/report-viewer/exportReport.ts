import type { ReportExportFormat } from './types';

export interface ExportReportOptions {
  element: HTMLElement;
  format: ReportExportFormat;
  fileName: string;
  title?: string;
  language?: string;
  direction?: 'ltr' | 'rtl';
}

const mimeTypes: Record<Exclude<ReportExportFormat, 'PDF' | 'Excel' | 'TIFF'>, string> = {
  Word: 'application/msword;charset=utf-8',
  CSV: 'text/csv;charset=utf-8',
  XML: 'application/xml;charset=utf-8',
  MHTML: 'multipart/related;charset=utf-8',
};

const safeFileName = (value: string): string =>
  [...value.trim()]
    .map((character) =>
      character.charCodeAt(0) < 32 || '<>:"/\\|?*'.includes(character) ? '-' : character
    )
    .join('')
    .replace(/[. ]+$/g, '')
    .slice(0, 160) || 'report';

const downloadBlob = (blob: Blob, fileName: string): void => {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = fileName;
  anchor.style.display = 'none';
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  window.setTimeout(() => URL.revokeObjectURL(url), 1_000);
};

const cellText = (element: Element | undefined): string =>
  (element?.textContent ?? '').replace(/\s+/g, ' ').trim();

export const extractReportRows = (element: HTMLElement): string[][] => {
  const rows: string[][] = [];
  const fields = [...element.querySelectorAll<HTMLElement>('.printout-field')];
  for (const field of fields) {
    const children = [...field.children];
    if (children.length >= 2) rows.push([cellText(children[0]), cellText(children[1])]);
  }

  for (const table of element.querySelectorAll('table')) {
    if (rows.length > 0 && rows.at(-1)?.some(Boolean)) rows.push([]);
    for (const row of table.querySelectorAll('tr')) {
      rows.push(
        [...row.querySelectorAll(':scope > th, :scope > td')].map((cell) => cellText(cell))
      );
    }
  }

  if (rows.some((row) => row.some(Boolean))) return rows;
  return (element.innerText || element.textContent || '')
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean)
    .map((line) => [line]);
};

const csvCell = (value: string): string => `"${value.replaceAll('"', '""')}"`;

const createCsv = (rows: string[][]): string =>
  `\uFEFF${rows.map((row) => row.map(csvCell).join(',')).join('\r\n')}`;

const xmlEscape = (value: string): string =>
  value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&apos;');

const createXml = (rows: string[][], title: string): string =>
  `<?xml version="1.0" encoding="UTF-8"?>\n<report title="${xmlEscape(title)}">\n${rows
    .map(
      (row, rowIndex) =>
        `  <row index="${rowIndex + 1}">${row
          .map((cell, cellIndex) => `<cell index="${cellIndex + 1}">${xmlEscape(cell)}</cell>`)
          .join('')}</row>`
    )
    .join('\n')}\n</report>`;

const documentStyles = (): string => {
  const rules: string[] = [];
  for (const sheet of document.styleSheets) {
    try {
      rules.push(...[...sheet.cssRules].map((rule) => rule.cssText));
    } catch {
      // Cross-origin style sheets cannot be read; the report's local application styles remain.
    }
  }
  return rules.join('\n');
};

const createHtml = (
  element: HTMLElement,
  title: string,
  language: string,
  direction: 'ltr' | 'rtl'
): string => `<!doctype html>
<html lang="${language}" dir="${direction}">
<head>
  <meta charset="utf-8">
  <title>${xmlEscape(title)}</title>
  <style>
    ${documentStyles()}
    html, body { margin: 0; background: #fff; direction: ${direction}; }
    body { padding: 12mm; }
    .printout-screen-only { display: none !important; }
  </style>
</head>
<body>${element.outerHTML}</body>
</html>`;

const waitForReportAssets = async (element: HTMLElement): Promise<void> => {
  await document.fonts?.ready;
  await Promise.all(
    [...element.querySelectorAll('img')].map((image) => {
      if (image.complete) return image.decode?.().catch(() => undefined) ?? Promise.resolve();
      return new Promise<void>((resolve) => {
        image.addEventListener('load', () => resolve(), { once: true });
        image.addEventListener('error', () => resolve(), { once: true });
      });
    })
  );
};

const captureReport = async (element: HTMLElement): Promise<HTMLCanvasElement> => {
  await waitForReportAssets(element);
  const { default: html2canvas } = await import('html2canvas');
  const previewScale = element.closest<HTMLElement>('.printout-preview-scale');
  const previousZoom = previewScale?.style.zoom ?? '';
  if (previewScale) previewScale.style.zoom = '1';
  try {
    await new Promise<void>((resolve) => requestAnimationFrame(() => resolve()));
    return await html2canvas(element, {
      backgroundColor: '#ffffff',
      scale: 2,
      useCORS: true,
      logging: false,
      windowWidth: Math.max(document.documentElement.clientWidth, element.scrollWidth),
      windowHeight: Math.max(document.documentElement.clientHeight, element.scrollHeight),
    });
  } finally {
    if (previewScale) previewScale.style.zoom = previousZoom;
  }
};

export interface PdfPageLayout {
  pageCount: number;
  renderedImageHeight: number;
}

export const calculatePdfPageLayout = (
  imageHeight: number,
  pageHeight: number,
  overflowTolerance = 1
): PdfPageLayout => {
  const safePageHeight = Math.max(1, pageHeight);
  const safeImageHeight = Math.max(0, imageHeight);
  const safeTolerance = Math.max(0, overflowTolerance);
  const pageCount = Math.max(
    1,
    Math.ceil(Math.max(0, safeImageHeight - safeTolerance) / safePageHeight)
  );

  return {
    pageCount,
    renderedImageHeight: Math.min(safeImageHeight, pageCount * safePageHeight),
  };
};

const exportPdf = async (element: HTMLElement, fileName: string): Promise<void> => {
  const canvas = await captureReport(element);
  const { jsPDF } = await import('jspdf');
  const orientation = canvas.width > canvas.height ? 'landscape' : 'portrait';
  const pdf = new jsPDF({ orientation, unit: 'mm', format: 'a4', compress: true });
  const pageWidth = pdf.internal.pageSize.getWidth();
  const pageHeight = pdf.internal.pageSize.getHeight();
  const imageHeight = (canvas.height * pageWidth) / canvas.width;
  const { pageCount, renderedImageHeight } = calculatePdfPageLayout(imageHeight, pageHeight);
  const image = canvas.toDataURL('image/jpeg', 0.96);

  for (let page = 0; page < pageCount; page += 1) {
    if (page > 0) pdf.addPage('a4', orientation);
    pdf.addImage(
      image,
      'JPEG',
      0,
      -(page * pageHeight),
      pageWidth,
      renderedImageHeight,
      undefined,
      'FAST'
    );
  }
  pdf.save(`${fileName}.pdf`);
};

const exportExcel = async (
  rows: string[][],
  fileName: string,
  title: string,
  direction: 'ltr' | 'rtl'
): Promise<void> => {
  const ExcelJS = await import('exceljs');
  const workbook = new ExcelJS.Workbook();
  workbook.creator = 'IAX';
  workbook.created = new Date();
  const worksheet = workbook.addWorksheet(title.slice(0, 31) || 'Report', {
    views: [{ rightToLeft: direction === 'rtl' }],
  });
  rows.forEach((row) => worksheet.addRow(row));
  const columnCount = Math.max(1, ...rows.map((row) => row.length));
  worksheet.columns = Array.from({ length: columnCount }, (_, columnIndex) => ({
    width: Math.min(60, Math.max(12, ...rows.map((row) => (row[columnIndex]?.length ?? 0) + 2))),
  }));
  worksheet.eachRow((row, rowNumber) => {
    row.alignment = {
      vertical: 'top',
      horizontal: direction === 'rtl' ? 'right' : 'left',
      wrapText: true,
    };
    if (rowNumber === 1) row.font = { bold: true };
  });
  const buffer = await workbook.xlsx.writeBuffer();
  downloadBlob(
    new Blob([buffer], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    }),
    `${fileName}.xlsx`
  );
};

const exportTiff = async (element: HTMLElement, fileName: string): Promise<void> => {
  const canvas = await captureReport(element);
  const context = canvas.getContext('2d', { willReadFrequently: true });
  if (!context) throw new Error('The browser could not create the TIFF image.');
  const rgba = context.getImageData(0, 0, canvas.width, canvas.height).data;
  const { default: UTIF } = await import('utif');
  const encoded = UTIF.encodeImage(
    rgba.buffer.slice(rgba.byteOffset, rgba.byteOffset + rgba.byteLength) as ArrayBuffer,
    canvas.width,
    canvas.height
  );
  downloadBlob(new Blob([encoded], { type: 'image/tiff' }), `${fileName}.tiff`);
};

export const exportReportElement = async ({
  element,
  format,
  fileName,
  title = fileName,
  language = document.documentElement.lang || 'en',
  direction = document.documentElement.dir === 'rtl' ? 'rtl' : 'ltr',
}: ExportReportOptions): Promise<void> => {
  const normalizedName = safeFileName(fileName);
  const rows = extractReportRows(element);

  if (format === 'PDF') return exportPdf(element, normalizedName);
  if (format === 'Excel') return exportExcel(rows, normalizedName, title, direction);
  if (format === 'TIFF') return exportTiff(element, normalizedName);
  if (format === 'CSV') {
    return downloadBlob(
      new Blob([createCsv(rows)], { type: mimeTypes.CSV }),
      `${normalizedName}.csv`
    );
  }
  if (format === 'XML') {
    return downloadBlob(
      new Blob([createXml(rows, title)], { type: mimeTypes.XML }),
      `${normalizedName}.xml`
    );
  }

  const html = createHtml(element, title, language, direction);
  if (format === 'Word') {
    return downloadBlob(
      new Blob(['\uFEFF', html], { type: mimeTypes.Word }),
      `${normalizedName}.doc`
    );
  }

  const boundary = `----IAXReport${Date.now().toString(16)}`;
  const mhtml = [
    'MIME-Version: 1.0',
    `Content-Type: multipart/related; boundary="${boundary}"; type="text/html"`,
    '',
    `--${boundary}`,
    'Content-Type: text/html; charset="utf-8"',
    'Content-Transfer-Encoding: 8bit',
    `Content-Location: ${normalizedName}.html`,
    '',
    html,
    `--${boundary}--`,
    '',
  ].join('\r\n');
  downloadBlob(new Blob([mhtml], { type: mimeTypes.MHTML }), `${normalizedName}.mhtml`);
};
