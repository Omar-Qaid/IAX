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
}

const placeholderValue = (element: Extract<PrintTemplateElement, { type: 'field' }>): string => {
  if (element.binding.sourceType === 'requestControl') {
    return element.binding.requestControlId
      ? `{{request.controls.${element.binding.requestControlId}}}`
      : '{{request field}}';
  }
  return `{{${element.binding.sourceType}.${element.binding.source ?? 'field'}}}`;
};

export function TemplateElementPreview({
  element,
  region,
  selectedId,
  onSelect,
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
        p: element.type === 'divider' ? 0.5 : 0.75,
        border: selected ? '1px solid #1976d2' : '1px solid transparent',
        bgcolor: selected ? 'rgba(25,118,210,.035)' : 'transparent',
        color: style?.color,
        fontSize: style?.fontSize,
        textAlign: style?.alignment ?? 'start',
        breakInside: style?.keepTogether ? 'avoid' : undefined,
        cursor: 'pointer',
        boxSizing: 'border-box',
        '&:hover': { borderColor: selected ? '#1976d2' : '#b8c7d9' },
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
            {placeholderValue(element)}
          </Typography>
        </Box>
      ) : null}
      {element.type === 'image' ? (
        <Box
          sx={{
            height: 64,
            display: 'grid',
            placeItems: 'center',
            border: '1px dashed #aaa',
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
