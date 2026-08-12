import { logger } from '../../../../../utils/logger';
import type { DropdownOptionX } from '../types';

const xmlEscape = (s: string) =>
    String(s ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');

export const buildDropdownXml = (items: DropdownOptionX[]): string =>
    `<Data>${items.map((o) =>
        `<Item><ar>${xmlEscape(o.ar)}</ar><en>${xmlEscape(o.en)}</en><value>${xmlEscape(o.value)}</value></Item>`
    ).join('')}</Data>`;

export const buildGridXml = (rows: string[][]): string => {
    const colCount = rows.reduce((m, r) => Math.max(m, r.length), 1);
    return `<Root>${rows.map((r) => {
        const cells = Array.from({ length: colCount }, (_, i) => r[i] ?? '');
        return `<row>${cells.map((c, i) =>
            c ? `<col${i + 1}>${xmlEscape(c)}</col${i + 1}>` : `<col${i + 1} />`
        ).join('')}</row>`;
    }).join('')}</Root>`;
};

export const parseDropdownXml = (xml: string): DropdownOptionX[] => {
    try {
        if (!xml || !xml.trim()) return [];
        const doc = new DOMParser().parseFromString(xml, 'application/xml');
        if (doc.getElementsByTagName('parsererror').length > 0) {
            logger.warn('XML parse error in parseDropdownXml');
            return [];
        }
        return Array.from(doc.querySelectorAll('Data > Item')).map((el) => ({
            ar: el.querySelector('ar')?.textContent ?? '',
            en: el.querySelector('en')?.textContent ?? '',
            value: el.querySelector('value')?.textContent ?? '',
        }));
    } catch { return []; }
};

export const parseGridXml = (xml: string): string[][] => {
    try {
        if (!xml || !xml.trim()) return [];
        const doc = new DOMParser().parseFromString(xml, 'application/xml');
        if (doc.getElementsByTagName('parsererror').length > 0) {
            logger.warn('XML parse error in parseGridXml');
            return [];
        }
        return Array.from(doc.querySelectorAll('Root > row')).map((row) =>
            Array.from(row.children).map((c) => c.textContent ?? '')
        );
    } catch { return []; }
};
