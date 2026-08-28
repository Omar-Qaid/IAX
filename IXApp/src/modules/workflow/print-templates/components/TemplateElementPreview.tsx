import React from 'react';
import { Box, Divider, Typography } from '@mui/material';
import ImageOutlined from '@mui/icons-material/ImageOutlined';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { PrintTemplateElement } from '../types/printTemplate.types';
import type { TemplateRegion } from '../hooks/useTemplateDesigner';

interface Props {
  element: PrintTemplateElement;
  region: TemplateRegion;
  selectedId: string | null;
  onSelect: (region: TemplateRegion, id: string) => void;
  requestControlNames?: ReadonlyMap<number, string>;
}

const placeholderValue = (
  element: Extract<PrintTemplateElement, { type: 'field' }>,
  requestControlNames?: ReadonlyMap<number, string>
): string => {
  if (element.binding.sourceType === 'requestControl') {
    const requestControlId = element.binding.requestControlId;
    if (!requestControlId) return '{{request field}}';
    return requestControlNames?.get(requestControlId) ?? `{{request.controls.${requestControlId}}}`;
  }
  return `{{${element.binding.sourceType}.${element.binding.source ?? 'field'}}}`;
};

export function TemplateElementPreview({
  element,
  region,
  selectedId,
  onSelect,
  requestControlNames,
}: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const selected = selectedId === element.id;
  const style = element.style;
  const children =
    element.type === 'section' || element.type === 'row' || element.type === 'column'
      ? element.elements
      : [];

  return (
    <Box
      data-testid={`template-element-${element.type}`}
      onClick={(event) => {
        event.stopPropagation();
        onSelect(region, element.id);
      }}
      sx={{
        position: 'relative',
        width: style?.width ? `${style.width}%` : '100%',
        minHeight: element.type === 'divider' ? 14 : 26,
        p: style?.padding != null ? `${style.padding}px` : element.type === 'divider' ? 0.5 : 0.75,
        mb: style?.marginBottom != null ? `${style.marginBottom}px` : undefined,
        borderWidth: style?.borderWidth != null ? `${style.borderWidth}px` : 1,
        borderStyle: 'solid',
        borderColor: style?.borderWidth ? style.borderColor || '#000' : 'transparent',
        borderRadius: style?.borderRadius != null ? `${style.borderRadius}px` : undefined,
        outline: selected ? '1px solid #1976d2' : 'none',
        bgcolor: style?.backgroundColor || (selected ? 'rgba(25,118,210,.035)' : 'transparent'),
        color: style?.color,
        fontSize: style?.fontSize,
        textAlign: style?.alignment ?? 'start',
        breakInside: style?.keepTogether ? 'avoid' : undefined,
        cursor: 'pointer',
        boxSizing: 'border-box',
        '&:hover': { outline: selected ? '1px solid #1976d2' : '1px solid #b8c7d9' },
      }}
    >
      {element.type === 'text' ? (
        <Typography
          sx={{
            fontSize: style?.fontSize ?? 12,
            fontWeight: style?.fontWeight,
            textAlign: style?.alignment ?? 'start',
          }}
        >
          {element.value || 'Text'}
        </Typography>
      ) : null}
      {element.type === 'field' ? (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: 'minmax(90px, 35%) 1fr',
            border: '1px solid #ddd',
          }}
        >
          <Typography sx={{ p: 0.65, bgcolor: '#f6f6f6', fontSize: 11, fontWeight: 600 }}>
            {element.label || t('printTemplates.designer.components.field')}
          </Typography>
          <Typography sx={{ p: 0.65, fontSize: 11, color: 'text.secondary' }}>
            {placeholderValue(element, requestControlNames)}
          </Typography>
        </Box>
      ) : null}
      {element.type === 'image' ? (
        <Box
          sx={{
            height: style?.height ?? 64,
            display: 'grid',
            placeItems: 'center',
            border: '1px dashed #aaa',
            borderRadius: style?.borderRadius != null ? `${style.borderRadius}px` : undefined,
            color: 'text.secondary',
          }}
        >
          <ImageOutlined fontSize="small" />
          <Typography sx={{ fontSize: 10 }}>
            {element.sourceType === 'companyLogo' ? 'Company logo' : 'Image'}
          </Typography>
        </Box>
      ) : null}
      {element.type === 'divider' ? <Divider /> : null}
      {element.type === 'spacer' ? (
        <Box sx={{ height: element.height, borderBlock: '1px dashed #d6dde5' }} />
      ) : null}
      {element.type === 'table' ? (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: `repeat(${Math.max(1, element.columns.length)}, minmax(0, 1fr))`,
            borderTop: '1px solid #b8c7d9',
            borderInlineStart: '1px solid #b8c7d9',
          }}
        >
          {element.columns.map((column) => (
            <Typography
              key={column.id}
              sx={{
                p: 0.65,
                borderInlineEnd: '1px solid #b8c7d9',
                borderBottom: '1px solid #b8c7d9',
                bgcolor: '#eef3f7',
                fontSize: 11,
                fontWeight: 700,
              }}
            >
              {column.label}
            </Typography>
          ))}
          {element.columns.map((column) => (
            <Typography
              key={`${column.id}-value`}
              sx={{
                p: 0.65,
                borderInlineEnd: '1px solid #d9e2ec',
                borderBottom: '1px solid #d9e2ec',
                color: 'text.secondary',
                fontSize: 10,
              }}
            >
              {`{{${column.field}}}`}
            </Typography>
          ))}
        </Box>
      ) : null}
      {element.type === 'barcode' || element.type === 'qrCode' ? (
        <Box
          sx={{
            minHeight: element.type === 'qrCode' ? 74 : 46,
            display: 'grid',
            placeItems: 'center',
            border: '1px dashed #78899b',
          }}
        >
          <Typography
            sx={{
              fontFamily: 'monospace',
              fontSize: element.type === 'qrCode' ? 12 : 17,
              letterSpacing: element.type === 'barcode' ? 2 : 0,
            }}
          >
            {element.type === 'barcode' ? '|||| 123456 ||||' : '▦ QR CODE'}
          </Typography>
        </Box>
      ) : null}
      {element.type === 'signature' ? (
        <Box
          sx={{
            minHeight: 54,
            borderBottom: '1px solid #78899b',
            display: 'flex',
            alignItems: 'flex-end',
            p: 0.5,
          }}
        >
          <Typography sx={{ fontSize: 11 }}>
            {element.label || t('printTemplates.designer.components.signature')}
          </Typography>
        </Box>
      ) : null}
      {element.type === 'printDate' ? (
        <Typography sx={{ fontSize: 11, color: 'text.secondary' }}>
          {t('printTemplates.designer.components.dateTime')}: 28/08/2026 10:30
        </Typography>
      ) : null}
      {element.type === 'pageNumber' ? (
        <Typography sx={{ fontSize: 11 }}>
          {t('printTemplates.designer.reportFields.pageNumber')}: 1
        </Typography>
      ) : null}
      {element.type === 'pageBreak' ? (
        <Box
          sx={{
            borderTop: '2px dashed #d14343',
            color: '#a61b1b',
            textAlign: 'center',
            fontSize: 10,
          }}
        >
          {t('printTemplates.designer.components.pageBreak')}
        </Box>
      ) : null}
      {element.type === 'section' ? (
        <Typography sx={{ mb: 0.5, fontSize: 12, fontWeight: 700 }}>
          {element.title || t('printTemplates.designer.components.section')}
        </Typography>
      ) : null}
      {children.length > 0 ? (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns:
              element.type === 'section'
                ? `repeat(${Math.max(1, element.columns)}, minmax(0, 1fr))`
                : element.type === 'row'
                  ? `repeat(${children.length}, minmax(0, 1fr))`
                  : '1fr',
            gap: 0.75,
          }}
        >
          {children.map((child) => (
            <TemplateElementPreview
              key={child.id}
              element={child}
              region={region}
              selectedId={selectedId}
              onSelect={onSelect}
              requestControlNames={requestControlNames}
            />
          ))}
        </Box>
      ) : element.type === 'section' || element.type === 'row' || element.type === 'column' ? (
        <Box
          sx={{
            minHeight: 34,
            display: 'grid',
            placeItems: 'center',
            border: '1px dashed #ccd3da',
            color: 'text.secondary',
            fontSize: 10,
          }}
        >
          {t('printTemplates.designer.emptyContainer')}
        </Box>
      ) : null}
    </Box>
  );
}
