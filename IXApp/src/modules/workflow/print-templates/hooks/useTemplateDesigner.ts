import React from 'react';
import type { PrintTemplateDocument, PrintTemplateElement } from '../types/printTemplate.types';

export type TemplateRegion = 'header' | 'sections' | 'footer';
export type DesignerComponentType =
  | 'text'
  | 'richText'
  | 'field'
  | 'labelValue'
  | 'image'
  | 'companyLogo'
  | 'barcode'
  | 'qrCode'
  | 'section'
  | 'row'
  | 'column'
  | 'container'
  | 'spacer'
  | 'divider'
  | 'table'
  | 'dynamicTable'
  | 'repeatingSection'
  | 'keyValueTable'
  | 'checkbox'
  | 'signature'
  | 'dateTime'
  | 'pageNumber'
  | 'pageNumberOfTotal'
  | 'pageBreak';

const elementChildren = (element: PrintTemplateElement): PrintTemplateElement[] | null => {
  if (element.type === 'section' || element.type === 'row' || element.type === 'column') {
    return element.elements;
  }
  return null;
};

const replaceElement = (
  elements: PrintTemplateElement[],
  id: string,
  transform: (element: PrintTemplateElement) => PrintTemplateElement | null
): PrintTemplateElement[] =>
  elements.flatMap((element) => {
    if (element.id === id) {
      const replacement = transform(element);
      return replacement ? [replacement] : [];
    }
    const children = elementChildren(element);
    if (!children) return [element];
    return [{ ...element, elements: replaceElement(children, id, transform) }];
  });

const findElement = (elements: PrintTemplateElement[], id: string): PrintTemplateElement | null => {
  for (const element of elements) {
    if (element.id === id) return element;
    const children = elementChildren(element);
    if (children) {
      const match = findElement(children, id);
      if (match) return match;
    }
  }
  return null;
};

const moveElement = (
  elements: PrintTemplateElement[],
  id: string,
  offset: -1 | 1
): PrintTemplateElement[] => {
  const index = elements.findIndex((element) => element.id === id);
  if (index >= 0) {
    const nextIndex = index + offset;
    if (nextIndex < 0 || nextIndex >= elements.length) return elements;
    const next = [...elements];
    [next[index], next[nextIndex]] = [next[nextIndex], next[index]];
    return next;
  }
  return elements.map((element) => {
    const children = elementChildren(element);
    return children ? { ...element, elements: moveElement(children, id, offset) } : element;
  });
};

const makeId = (): string =>
  typeof crypto !== 'undefined' && 'randomUUID' in crypto
    ? crypto.randomUUID().replaceAll('-', '')
    : `element${Date.now()}${Math.random().toString(16).slice(2)}`;

export const createDesignerElement = (type: DesignerComponentType): PrintTemplateElement => {
  const id = makeId();
  switch (type) {
    case 'text':
      return { type, id, value: 'Text', style: { fontSize: 12, alignment: 'start' } };
    case 'richText':
      return { type: 'text', id, value: 'Rich text', style: { fontSize: 12, alignment: 'start' } };
    case 'field':
      return {
        type: 'field',
        id,
        label: '',
        binding: { sourceType: 'system', source: 'requestNumber' },
      };
    case 'labelValue':
      return {
        type: 'field',
        id,
        label: 'Label',
        binding: { sourceType: 'system', source: 'requestNumber' },
      };
    case 'checkbox':
      return {
        type: 'field',
        id,
        label: 'Checkbox',
        binding: { sourceType: 'system', source: 'requestStatus' },
        format: { type: 'boolean', trueText: '☑', falseText: '☐' },
      };
    case 'section':
      return { type, id, title: '', columns: 1, elements: [] };
    case 'repeatingSection':
      return { type: 'section', id, title: 'Repeating section', columns: 1, elements: [] };
    case 'row':
      return { type, id, elements: [] };
    case 'column':
      return { type, id, span: 1, elements: [] };
    case 'container':
      return { type: 'column', id, span: 1, elements: [] };
    case 'image':
      return { type: 'image', id, sourceType: 'url', altText: 'Image' };
    case 'companyLogo':
      return { type: 'image', id, sourceType: 'companyLogo', altText: 'Company logo' };
    case 'divider':
      return { type, id };
    case 'spacer':
      return { type, id, height: 8 };
    case 'table':
    case 'dynamicTable':
      return {
        type: 'table',
        id,
        dataSource: { sourceType: 'repeating', source: 'items' },
        columns: [{ id: `${id}value`, label: 'Value', field: 'value' }],
        repeatHeader: true,
      };
    case 'keyValueTable':
      return {
        type: 'table',
        id,
        dataSource: { sourceType: 'repeating', source: 'items' },
        columns: [
          { id: `${id}key`, label: 'Key', field: 'key' },
          { id: `${id}value`, label: 'Value', field: 'value' },
        ],
        repeatHeader: true,
      };
    case 'barcode':
      return {
        type,
        id,
        binding: { sourceType: 'system', source: 'requestNumber' },
        format: 'code128',
      };
    case 'qrCode':
      return { type, id, binding: { sourceType: 'system', source: 'requestNumber' } };
    case 'signature':
      return {
        type,
        id,
        label: 'Signature',
        binding: { sourceType: 'system', source: 'currentUser' },
      };
    case 'dateTime':
      return { type: 'printDate', id };
    case 'pageNumber':
      return { type, id };
    case 'pageNumberOfTotal':
      return {
        type: 'field',
        id,
        label: '',
        binding: { sourceType: 'report', source: 'pageNumberOfTotal' },
      };
    case 'pageBreak':
      return { type, id };
  }
};

export function useTemplateDesigner(
  document: PrintTemplateDocument,
  onChange: (document: PrintTemplateDocument) => void
) {
  const [region, setRegion] = React.useState<TemplateRegion>('sections');
  const [selectedId, setSelectedId] = React.useState<string | null>(null);

  const selectedElement = React.useMemo(() => {
    if (!selectedId) return null;
    return (
      findElement(document.header, selectedId) ??
      findElement(document.sections, selectedId) ??
      findElement(document.footer, selectedId)
    );
  }, [document, selectedId]);

  const updateRegion = React.useCallback(
    (target: TemplateRegion, elements: PrintTemplateElement[]) =>
      onChange({ ...document, [target]: elements }),
    [document, onChange]
  );

  const addElement = React.useCallback(
    (type: DesignerComponentType) => {
      const element = createDesignerElement(type);
      if (selectedElement) {
        const children = elementChildren(selectedElement);
        if (children) {
          const next = replaceElement(document[region], selectedElement.id, (value) => ({
            ...value,
            elements: [...(elementChildren(value) ?? []), element],
          }));
          updateRegion(region, next);
          setSelectedId(element.id);
          return;
        }
      }
      updateRegion(region, [...document[region], element]);
      setSelectedId(element.id);
    },
    [document, region, selectedElement, updateRegion]
  );

  const updateSelected = React.useCallback(
    (transform: (element: PrintTemplateElement) => PrintTemplateElement) => {
      if (!selectedId) return;
      updateRegion(region, replaceElement(document[region], selectedId, transform));
    },
    [document, region, selectedId, updateRegion]
  );

  const removeSelected = React.useCallback(() => {
    if (!selectedId) return;
    updateRegion(
      region,
      replaceElement(document[region], selectedId, () => null)
    );
    setSelectedId(null);
  }, [document, region, selectedId, updateRegion]);

  const moveSelected = React.useCallback(
    (offset: -1 | 1) => {
      if (!selectedId) return;
      updateRegion(region, moveElement(document[region], selectedId, offset));
    },
    [document, region, selectedId, updateRegion]
  );

  const select = React.useCallback((targetRegion: TemplateRegion, id: string) => {
    setRegion(targetRegion);
    setSelectedId(id);
  }, []);

  return {
    region,
    setRegion,
    selectedId,
    selectedElement,
    select,
    addElement,
    updateSelected,
    removeSelected,
    moveSelected,
  };
}
