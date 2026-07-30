import type { ColumnDef } from './Types';

export const downloadFile = (content: string, fileName: string, contentType: string) => {
    const a = document.createElement('a');
    const file = new Blob([content], { type: contentType });
    a.href = URL.createObjectURL(file);
    a.download = fileName;
    a.click();
};

export function computeFlexWidths<T>(columns: ColumnDef<T>[], containerWidth: number): ColumnDef<T>[] {
    let totalFlex = 0;
    let fixedWidth = 0;
    let pinnedWidth = 0;
    for (const col of columns) {
        if (col.hidden) continue;
        if (col.pinned) {
            pinnedWidth += col.width || 150;
        } else if (col.flex) {
            totalFlex += col.flex;
        } else {
            fixedWidth += col.width || 150;
        }
    }
    if (totalFlex === 0) return columns;

    const available = Math.max(0, containerWidth - fixedWidth - pinnedWidth);

    let changed = false;
    const next = columns.map(col => {
        if (!col.flex || col.hidden || col.pinned) return col;
        const newWidth = Math.floor(Math.max(col.minWidth || 80, (col.flex / totalFlex) * available));
        if (col.width === newWidth) return col;
        changed = true;
        return { ...col, width: newWidth };
    });
    return changed ? next : columns;
}

export function generateCSV<T>(rows: T[], columns: ColumnDef<T>[]): string {
    const visibleCols = columns.filter(c => !c.hidden);
    const header = visibleCols.map(c => `"${c.headerName}"`).join(',');
    const csvRows = rows.map(row => {
        return visibleCols.map(col => {
            const val = col.valueGetter ? col.valueGetter({ row }) : (row as any)[col.field as string];
            const str = val != null ? String(val) : '';
            return `"${str.replace(/"/g, '""')}"`;
        }).join(',');
    });
    return [header, ...csvRows].join('\n');
}

export function getNestedValue(obj: any, path: string): any {
    if (!obj || !path) return undefined;
    if (!path.includes('.')) return obj[path];
    return path.split('.').reduce((o, i) => o?.[i], obj);
}
