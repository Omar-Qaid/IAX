import React from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Button,
  ButtonGroup,
  Divider,
  FormControlLabel,
  IconButton,
  MenuItem,
  Paper,
  Stack,
  Switch,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Tooltip,
  Typography,
} from '@mui/material';
import TextFieldsOutlined from '@mui/icons-material/TextFieldsOutlined';
import DataObjectOutlined from '@mui/icons-material/DataObjectOutlined';
import ViewAgendaOutlined from '@mui/icons-material/ViewAgendaOutlined';
import TableRowsOutlined from '@mui/icons-material/TableRowsOutlined';
import ViewColumnOutlined from '@mui/icons-material/ViewColumnOutlined';
import ImageOutlined from '@mui/icons-material/ImageOutlined';
import HorizontalRuleOutlined from '@mui/icons-material/HorizontalRuleOutlined';
import DeleteOutline from '@mui/icons-material/DeleteOutlined';
import ArrowUpwardOutlined from '@mui/icons-material/ArrowUpwardOutlined';
import ArrowDownwardOutlined from '@mui/icons-material/ArrowDownwardOutlined';
import ExpandMoreOutlined from '@mui/icons-material/ExpandMoreOutlined';
import { useQuery } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { dynamicRequestFormApi } from '../../api/dynamicRequestFormApi';
import { TemplateElementPreview } from './TemplateElementPreview';
import {
  useTemplateDesigner,
  type DesignerComponentType,
  type TemplateRegion,
} from '../hooks/useTemplateDesigner';
import type {
  PrintFieldElement,
  PrintTemplateDocument,
  PrintTemplateElement,
  PrintElementStyle,
} from '../types/printTemplate.types';

interface Props {
  processId: number;
  document: PrintTemplateDocument;
  onChange: (document: PrintTemplateDocument) => void;
  isDefault?: boolean;
  onDefaultChange?: (isDefault: boolean) => void;
}

interface RequestControlLookupRow {
  requestControlId: number;
  code: string;
  name: string;
  nameAr: string;
  displayName: string;
  displayNameAr: string;
}

const requestControlLookupColumns = [
  { field: 'code', header: 'printTemplates.fields.code', width: 120 },
  { field: 'name', header: 'printTemplates.fields.name', flex: 1, showInLtr: true },
  { field: 'nameAr', header: 'printTemplates.fields.name', flex: 1, showInRtl: true },
] as const;

const loadRequestControls = async (processId: number, signal?: AbortSignal) => {
  const definition = await dynamicRequestFormApi.getDefinition(processId, signal);
  return definition.controls.map((control): RequestControlLookupRow => {
    const nameAr = control.labelAr || control.label;
    return {
      requestControlId: control.requestControlId,
      code: control.code,
      name: control.label,
      nameAr,
      displayName: `${control.code} - ${control.label}`,
      displayNameAr: `${control.code} - ${nameAr}`,
    };
  });
};

const palette: Array<{ type: DesignerComponentType; icon: React.ReactElement }> = [
  { type: 'text', icon: <TextFieldsOutlined /> },
  { type: 'richText', icon: <TextFieldsOutlined /> },
  { type: 'field', icon: <DataObjectOutlined /> },
  { type: 'labelValue', icon: <DataObjectOutlined /> },
  { type: 'image', icon: <ImageOutlined /> },
  { type: 'companyLogo', icon: <ImageOutlined /> },
  { type: 'barcode', icon: <DataObjectOutlined /> },
  { type: 'qrCode', icon: <DataObjectOutlined /> },
  { type: 'section', icon: <ViewAgendaOutlined /> },
  { type: 'row', icon: <TableRowsOutlined /> },
  { type: 'column', icon: <ViewColumnOutlined /> },
  { type: 'container', icon: <ViewAgendaOutlined /> },
  { type: 'spacer', icon: <HorizontalRuleOutlined /> },
  { type: 'divider', icon: <HorizontalRuleOutlined /> },
  { type: 'table', icon: <TableRowsOutlined /> },
  { type: 'dynamicTable', icon: <TableRowsOutlined /> },
  { type: 'repeatingSection', icon: <ViewAgendaOutlined /> },
  { type: 'keyValueTable', icon: <TableRowsOutlined /> },
  { type: 'checkbox', icon: <DataObjectOutlined /> },
  { type: 'signature', icon: <TextFieldsOutlined /> },
  { type: 'dateTime', icon: <TextFieldsOutlined /> },
  { type: 'pageNumber', icon: <TextFieldsOutlined /> },
  { type: 'pageNumberOfTotal', icon: <TextFieldsOutlined /> },
  { type: 'pageBreak', icon: <HorizontalRuleOutlined /> },
];

const systemFields = [
  'requestNumber',
  'requestDate',
  'requestStatus',
  'processName',
  'processCode',
  'createdBy',
  'createdDate',
  'submittedBy',
  'submissionDate',
  'currentUser',
  'printDate',
] as const;

const reportFields = [
  'pageNumber',
  'totalPages',
  'pageNumberOfTotal',
  'currentDate',
  'currentTime',
  'printedDate',
  'printedBy',
] as const;

function PropertyAccordion({
  title,
  children,
  defaultExpanded = false,
}: {
  title: string;
  children: React.ReactNode;
  defaultExpanded?: boolean;
}): React.ReactElement {
  return (
    <Accordion
      defaultExpanded={defaultExpanded}
      disableGutters
      elevation={0}
      square
      sx={{ '&::before': { display: 'none' }, borderTop: 1, borderColor: 'divider' }}
    >
      <AccordionSummary
        expandIcon={<ExpandMoreOutlined sx={{ fontSize: 17 }} />}
        sx={{
          minHeight: 36,
          px: 1,
          '&.Mui-expanded': { minHeight: 36 },
          '& .MuiAccordionSummary-content, & .MuiAccordionSummary-content.Mui-expanded': {
            my: 0.5,
          },
        }}
      >
        <Typography sx={{ fontSize: 11, fontWeight: 700 }}>{title}</Typography>
      </AccordionSummary>
      <AccordionDetails sx={{ px: 1, pt: 0.5, pb: 1.25 }}>
        <Stack spacing={1.25}>{children}</Stack>
      </AccordionDetails>
    </Accordion>
  );
}

function ColorPropertyField({
  label,
  pickerLabel,
  value,
  fallback,
  onChange,
}: {
  label: string;
  pickerLabel: string;
  value: string | null | undefined;
  fallback: string;
  onChange: (value: string | null) => void;
}): React.ReactElement {
  const pickerValue = /^#[0-9a-f]{6}$/i.test(value ?? '') ? value! : fallback;
  return (
    <Stack direction="row" spacing={0.75} alignItems="center">
      <TextField
        size="small"
        fullWidth
        label={label}
        value={value ?? ''}
        placeholder={fallback}
        onChange={(event) => onChange(event.target.value || null)}
      />
      <Box
        component="input"
        type="color"
        aria-label={pickerLabel}
        value={pickerValue}
        onChange={(event: React.ChangeEvent<HTMLInputElement>) => onChange(event.target.value)}
        sx={{ width: 42, height: 38, p: 0.25, border: '1px solid', borderColor: 'divider', borderRadius: 1, bgcolor: 'background.paper', cursor: 'pointer' }}
      />
    </Stack>
  );
}

function ElementProperties({
  element,
  update,
  processId,
}: {
  element: PrintTemplateElement | null;
  update: (transform: (element: PrintTemplateElement) => PrintTemplateElement) => void;
  processId: number;
}): React.ReactElement {
  const { t } = useAppTranslation();
  if (!element) {
    return (
      <Typography color="text.secondary" sx={{ p: 1, fontSize: 11 }}>
        {t('printTemplates.designer.selectElement')}
      </Typography>
    );
  }

  const updateStyle = (key: keyof PrintElementStyle, value: string | number | boolean | null) =>
    update((current) => ({ ...current, style: { ...current.style, [key]: value } }));

  return (
    <Stack spacing={0} sx={{ pb: 1 }}>
      <Typography sx={{ p: 1, fontSize: 12, fontWeight: 700 }}>
        {t(`printTemplates.designer.components.${element.type}`)}
      </Typography>
      <PropertyAccordion title={t('printTemplates.designer.groups.content')} defaultExpanded>
        {element.type === 'text' ? (
          <TextField
            size="small"
            multiline
            minRows={2}
            label={t('printTemplates.designer.properties.text')}
            value={element.value}
            onChange={(event) =>
              update((current) =>
                current.type === 'text' ? { ...current, value: event.target.value } : current
              )
            }
          />
        ) : null}
        {element.type === 'field' ? (
          <FieldProperties element={element} update={update} processId={processId} />
        ) : null}
        {element.type === 'section' ? (
          <>
            <TextField
              size="small"
              label={t('printTemplates.designer.properties.title')}
              value={element.title ?? ''}
              onChange={(event) =>
                update((current) =>
                  current.type === 'section' ? { ...current, title: event.target.value } : current
                )
              }
            />
            <TextField
              size="small"
              type="number"
              label={t('printTemplates.designer.properties.columns')}
              value={element.columns}
              slotProps={{ htmlInput: { min: 1, max: 4 } }}
              onChange={(event) =>
                update((current) =>
                  current.type === 'section'
                    ? {
                        ...current,
                        columns: Math.max(1, Math.min(4, Number(event.target.value) || 1)),
                      }
                    : current
                )
              }
            />
          </>
        ) : null}
        {element.type === 'column' ? (
          <TextField
            size="small"
            type="number"
            label={t('printTemplates.designer.properties.span')}
            value={element.span}
            slotProps={{ htmlInput: { min: 1, max: 12 } }}
            onChange={(event) =>
              update((current) =>
                current.type === 'column'
                  ? { ...current, span: Math.max(1, Math.min(12, Number(event.target.value) || 1)) }
                  : current
              )
            }
          />
        ) : null}
        {element.type === 'image' ? (
          <TextField
            select
            size="small"
            label={t('printTemplates.designer.properties.imageSource')}
            value={element.sourceType}
            onChange={(event) =>
              update((current) =>
                current.type === 'image' ? { ...current, sourceType: event.target.value } : current
              )
            }
          >
            <MenuItem value="companyLogo">
              {t('printTemplates.designer.imageSources.companyLogo')}
            </MenuItem>
            <MenuItem value="url">{t('printTemplates.designer.imageSources.url')}</MenuItem>
          </TextField>
        ) : null}
        {element.type === 'image' && element.sourceType === 'url' ? (
          <TextField
            size="small"
            label={t('printTemplates.designer.properties.url')}
            value={element.source ?? ''}
            onChange={(event) =>
              update((current) =>
                current.type === 'image' ? { ...current, source: event.target.value } : current
              )
            }
          />
        ) : null}
        {element.type === 'image' ? (
          <TextField
            size="small"
            label={t('printTemplates.designer.properties.altText')}
            value={element.altText ?? ''}
            onChange={(event) =>
              update((current) =>
                current.type === 'image' ? { ...current, altText: event.target.value } : current
              )
            }
          />
        ) : null}
        {element.type === 'spacer' ? (
          <TextField
            size="small"
            type="number"
            label={t('printTemplates.designer.properties.height')}
            value={element.height}
            slotProps={{ htmlInput: { min: 1, max: 100 } }}
            onChange={(event) =>
              update((current) =>
                current.type === 'spacer'
                  ? {
                      ...current,
                      height: Math.max(1, Math.min(100, Number(event.target.value) || 1)),
                    }
                  : current
              )
            }
          />
        ) : null}
        {element.type === 'table' ? (
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={element.repeatHeader}
                onChange={(_, checked) =>
                  update((current) =>
                    current.type === 'table' ? { ...current, repeatHeader: checked } : current
                  )
                }
              />
            }
            label={
              <Typography sx={{ fontSize: 11 }}>
                {t('printTemplates.designer.properties.repeatHeader')}
              </Typography>
            }
          />
        ) : null}
        {element.type === 'barcode' ? (
          <TextField
            select
            size="small"
            label={t('printTemplates.designer.properties.barcodeFormat')}
            value={element.format}
            onChange={(event) =>
              update((current) =>
                current.type === 'barcode' ? { ...current, format: event.target.value } : current
              )
            }
          >
            <MenuItem value="code128">Code 128</MenuItem>
            <MenuItem value="code39">Code 39</MenuItem>
            <MenuItem value="ean13">EAN-13</MenuItem>
          </TextField>
        ) : null}
        {element.type === 'signature' ? (
          <TextField
            size="small"
            label={t('printTemplates.designer.properties.label')}
            value={element.label ?? ''}
            onChange={(event) =>
              update((current) =>
                current.type === 'signature' ? { ...current, label: event.target.value } : current
              )
            }
          />
        ) : null}
      </PropertyAccordion>
      {element.type === 'field' ? (
        <PropertyAccordion title={t('printTemplates.designer.groups.format')}>
          <FieldFormatProperties element={element} update={update} />
        </PropertyAccordion>
      ) : null}
      {element.type === 'text' || element.type === 'field' ? (
        <PropertyAccordion title={t('printTemplates.designer.groups.typography')}>
          <TextField
            size="small"
            type="number"
            label={t('printTemplates.designer.properties.fontSize')}
            value={element.style?.fontSize ?? 12}
            onChange={(event) => updateStyle('fontSize', Number(event.target.value) || 12)}
          />
          <TextField
            select
            size="small"
            label={t('printTemplates.designer.properties.fontWeight')}
            value={element.style?.fontWeight ?? 400}
            onChange={(event) => updateStyle('fontWeight', Number(event.target.value))}
          >
            <MenuItem value={400}>{t('printTemplates.designer.fontWeights.normal')}</MenuItem>
            <MenuItem value={500}>{t('printTemplates.designer.fontWeights.medium')}</MenuItem>
            <MenuItem value={700}>{t('printTemplates.designer.fontWeights.bold')}</MenuItem>
          </TextField>
        </PropertyAccordion>
      ) : null}
      <PropertyAccordion title={t('printTemplates.designer.groups.layout')}>
        <TextField
          select
          size="small"
          label={t('printTemplates.designer.properties.alignment')}
          value={element.style?.alignment ?? 'start'}
          onChange={(event) => updateStyle('alignment', event.target.value)}
        >
          <MenuItem value="start">{t('printTemplates.designer.alignments.start')}</MenuItem>
          <MenuItem value="center">{t('printTemplates.designer.alignments.center')}</MenuItem>
          <MenuItem value="end">{t('printTemplates.designer.alignments.end')}</MenuItem>
        </TextField>
        <TextField
          size="small"
          type="number"
          label={t('printTemplates.designer.properties.width')}
          value={element.style?.width ?? 100}
          slotProps={{ htmlInput: { min: 10, max: 100 } }}
          onChange={(event) =>
            updateStyle('width', Math.max(10, Math.min(100, Number(event.target.value) || 100)))
          }
        />
        {element.type === 'image' ? (
          <>
            <TextField
              size="small"
              type="number"
              label={t('printTemplates.designer.properties.height')}
              value={element.style?.height ?? 80}
              slotProps={{ htmlInput: { min: 20, max: 500 } }}
              onChange={(event) =>
                updateStyle('height', Math.max(20, Math.min(500, Number(event.target.value) || 80)))
              }
            />
            <TextField
              select
              size="small"
              label={t('printTemplates.designer.properties.imageFit')}
              value={element.style?.objectFit ?? 'contain'}
              onChange={(event) => updateStyle('objectFit', event.target.value)}
            >
              <MenuItem value="contain">{t('printTemplates.designer.imageFits.contain')}</MenuItem>
              <MenuItem value="cover">{t('printTemplates.designer.imageFits.cover')}</MenuItem>
              <MenuItem value="fill">{t('printTemplates.designer.imageFits.fill')}</MenuItem>
            </TextField>
          </>
        ) : null}
        <TextField
          size="small"
          type="number"
          label={t('printTemplates.designer.properties.padding')}
          value={element.style?.padding ?? 0}
          slotProps={{ htmlInput: { min: 0, max: 60 } }}
          onChange={(event) =>
            updateStyle('padding', Math.max(0, Math.min(60, Number(event.target.value) || 0)))
          }
        />
        <TextField
          size="small"
          type="number"
          label={t('printTemplates.designer.properties.marginBottom')}
          value={element.style?.marginBottom ?? 0}
          slotProps={{ htmlInput: { min: 0, max: 100 } }}
          onChange={(event) =>
            updateStyle('marginBottom', Math.max(0, Math.min(100, Number(event.target.value) || 0)))
          }
        />
      </PropertyAccordion>
      <PropertyAccordion title={t('printTemplates.designer.groups.appearance')}>
        {element.type === 'text' || element.type === 'field' ? (
          <ColorPropertyField
            label={t('printTemplates.designer.properties.textColor')}
            value={element.style?.color ?? ''}
            fallback="#000000"
            pickerLabel={t('printTemplates.designer.selectColor', {
              field: t('printTemplates.designer.properties.textColor'),
            })}
            onChange={(value) => updateStyle('color', value)}
          />
        ) : null}
        <ColorPropertyField
          label={t('printTemplates.designer.properties.backgroundColor')}
          value={element.style?.backgroundColor ?? ''}
          fallback="#ffffff"
          pickerLabel={t('printTemplates.designer.selectColor', {
            field: t('printTemplates.designer.properties.backgroundColor'),
          })}
          onChange={(value) => updateStyle('backgroundColor', value)}
        />
        <TextField
          size="small"
          type="number"
          label={t('printTemplates.designer.properties.borderWidth')}
          value={element.style?.borderWidth ?? 0}
          slotProps={{ htmlInput: { min: 0, max: 10 } }}
          onChange={(event) =>
            updateStyle('borderWidth', Math.max(0, Math.min(10, Number(event.target.value) || 0)))
          }
        />
        <ColorPropertyField
          label={t('printTemplates.designer.properties.borderColor')}
          value={element.style?.borderColor ?? ''}
          fallback="#000000"
          pickerLabel={t('printTemplates.designer.selectColor', {
            field: t('printTemplates.designer.properties.borderColor'),
          })}
          onChange={(value) => updateStyle('borderColor', value)}
        />
        <TextField
          size="small"
          type="number"
          label={t('printTemplates.designer.properties.borderRadius')}
          value={element.style?.borderRadius ?? 0}
          slotProps={{ htmlInput: { min: 0, max: 100 } }}
          onChange={(event) =>
            updateStyle('borderRadius', Math.max(0, Math.min(100, Number(event.target.value) || 0)))
          }
        />
      </PropertyAccordion>
      <PropertyAccordion title={t('printTemplates.designer.groups.print')}>
        <FormControlLabel
          control={
            <Switch
              size="small"
              checked={element.style?.keepTogether ?? false}
              onChange={(_, checked) => updateStyle('keepTogether', checked)}
            />
          }
          label={
            <Typography sx={{ fontSize: 11 }}>
              {t('printTemplates.designer.properties.keepTogether')}
            </Typography>
          }
        />
      </PropertyAccordion>
    </Stack>
  );
}

function FieldProperties({
  element,
  update,
  processId,
}: {
  element: PrintFieldElement;
  update: (transform: (element: PrintTemplateElement) => PrintTemplateElement) => void;
  processId: number;
}) {
  const { t } = useAppTranslation();
  const setField = (transform: (field: PrintFieldElement) => PrintFieldElement) =>
    update((current) => (current.type === 'field' ? transform(current) : current));
  return (
    <>
      <TextField
        size="small"
        label={t('printTemplates.designer.properties.label')}
        value={element.label}
        onChange={(event) => setField((field) => ({ ...field, label: event.target.value }))}
      />
      <TextField
        select
        size="small"
        label={t('printTemplates.designer.properties.sourceType')}
        value={element.binding.sourceType}
        onChange={(event) =>
          setField((field) => ({
            ...field,
            binding: {
              sourceType: event.target.value as PrintFieldElement['binding']['sourceType'],
              source:
                event.target.value === 'system'
                  ? 'requestNumber'
                  : event.target.value === 'company'
                    ? 'name'
                    : event.target.value === 'report'
                      ? 'pageNumber'
                      : null,
            },
          }))
        }
      >
        <MenuItem value="system">{t('printTemplates.designer.sources.system')}</MenuItem>
        <MenuItem value="company">{t('printTemplates.designer.sources.company')}</MenuItem>
        <MenuItem value="report">{t('printTemplates.designer.sources.report')}</MenuItem>
        <MenuItem value="requestControl">
          {t('printTemplates.designer.sources.requestControl')}
        </MenuItem>
      </TextField>
      {element.binding.sourceType === 'system' ? (
        <TextField
          select
          size="small"
          label={t('printTemplates.designer.properties.field')}
          value={element.binding.source ?? 'requestNumber'}
          onChange={(event) =>
            setField((field) => ({
              ...field,
              binding: { ...field.binding, source: event.target.value },
            }))
          }
        >
          {systemFields.map((field) => (
            <MenuItem key={field} value={field}>
              {t(`printTemplates.designer.systemFields.${field}`)}
            </MenuItem>
          ))}
        </TextField>
      ) : null}
      {element.binding.sourceType === 'company' ? (
        <TextField
          select
          size="small"
          label={t('printTemplates.designer.properties.field')}
          value={element.binding.source ?? 'name'}
          onChange={(event) =>
            setField((field) => ({
              ...field,
              binding: { ...field.binding, source: event.target.value },
            }))
          }
        >
          {[
            'name',
            'arabicName',
            'address',
            'vatNumber',
            'commercialRegistration',
            'phone',
            'email',
          ].map((field) => (
            <MenuItem key={field} value={field}>
              {t(`printTemplates.designer.companyFields.${field}`)}
            </MenuItem>
          ))}
        </TextField>
      ) : null}
      {element.binding.sourceType === 'report' ? (
        <TextField
          select
          size="small"
          label={t('printTemplates.designer.properties.field')}
          value={element.binding.source ?? 'pageNumber'}
          onChange={(event) =>
            setField((field) => ({
              ...field,
              binding: { ...field.binding, source: event.target.value },
              format:
                event.target.value === 'currentTime'
                  ? { type: 'date', pattern: 'HH:mm' }
                  : event.target.value === 'currentDate' || event.target.value === 'printedDate'
                    ? { type: 'date', pattern: 'dd/MM/yyyy' }
                    : field.format,
            }))
          }
        >
          {reportFields.map((field) => (
            <MenuItem key={field} value={field}>
              {t(`printTemplates.designer.reportFields.${field}`)}
            </MenuItem>
          ))}
        </TextField>
      ) : null}
      {element.binding.sourceType === 'requestControl' ? (
        <AppLookupGridField<RequestControlLookupRow>
          name="requestControlId"
          label={t('printTemplates.designer.properties.requestControlId')}
          value={element.binding.requestControlId ?? ''}
          onChange={(value) =>
            setField((field) => ({
              ...field,
              binding: { ...field.binding, requestControlId: Number(value) || null },
            }))
          }
          disabled={processId <= 0}
          columns={[...requestControlLookupColumns]}
          queryKey={['workflow', 'print-template-request-controls', processId]}
          fetchPage={async ({ pageNumber, pageSize, search, signal }) => {
            const controls = await loadRequestControls(processId, signal);
            const query = search.trim().toLocaleLowerCase();
            const filtered = query
              ? controls.filter((control) =>
                  `${control.code} ${control.name} ${control.nameAr}`
                    .toLocaleLowerCase()
                    .includes(query)
                )
              : controls;
            const start = (pageNumber - 1) * pageSize;
            return {
              data: filtered.slice(start, start + pageSize),
              pageNumber,
              totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
              totalRecords: filtered.length,
            };
          }}
          fetchById={async (value) => {
            const controls = await loadRequestControls(processId);
            return controls.find((control) => control.requestControlId === Number(value)) ?? null;
          }}
          valueField="requestControlId"
          labelField="displayName"
          labelFieldAr="displayNameAr"
          pageSize={25}
        />
      ) : null}
    </>
  );
}

function FieldFormatProperties({
  element,
  update,
}: {
  element: PrintFieldElement;
  update: (transform: (element: PrintTemplateElement) => PrintTemplateElement) => void;
}): React.ReactElement {
  const { t } = useAppTranslation();
  const format = element.format ?? { type: 'text' as const };
  const updateFormat = (values: Partial<NonNullable<PrintFieldElement['format']>>) =>
    update((current) =>
      current.type === 'field'
        ? { ...current, format: { ...(current.format ?? { type: 'text' }), ...values } }
        : current
    );
  const numeric = ['number', 'currency', 'percentage'].includes(format.type);
  const date = format.type === 'date' || format.type === 'dateTime';

  return (
    <>
      <TextField
        select
        size="small"
        label={t('printTemplates.designer.properties.format')}
        value={format.type}
        onChange={(event) =>
          updateFormat({
            type: event.target.value as NonNullable<PrintFieldElement['format']>['type'],
          })
        }
      >
        {['text', 'number', 'currency', 'percentage', 'date', 'dateTime', 'boolean'].map((type) => (
          <MenuItem key={type} value={type}>
            {t(`printTemplates.designer.formats.${type}`)}
          </MenuItem>
        ))}
      </TextField>
      {numeric ? (
        <>
          <TextField
            type="number"
            size="small"
            label={t('printTemplates.designer.properties.decimalPlaces')}
            value={format.decimalPlaces ?? 2}
            slotProps={{ htmlInput: { min: 0, max: 8 } }}
            onChange={(event) =>
              updateFormat({
                decimalPlaces: Math.max(0, Math.min(8, Number(event.target.value) || 0)),
              })
            }
          />
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={format.useGrouping ?? true}
                onChange={(_, checked) => updateFormat({ useGrouping: checked })}
              />
            }
            label={
              <Typography sx={{ fontSize: 11 }}>
                {t('printTemplates.designer.properties.thousandSeparator')}
              </Typography>
            }
          />
          {format.type === 'currency' ? (
            <TextField
              size="small"
              label={t('printTemplates.designer.properties.currency')}
              value={format.currency ?? 'SAR'}
              onChange={(event) => updateFormat({ currency: event.target.value.toUpperCase() })}
              slotProps={{ htmlInput: { maxLength: 3 } }}
            />
          ) : null}
          <TextField
            select
            size="small"
            label={t('printTemplates.designer.properties.negativeFormat')}
            value={format.negativeFormat ?? 'minus'}
            onChange={(event) =>
              updateFormat({
                negativeFormat: event.target.value as NonNullable<typeof format.negativeFormat>,
              })
            }
          >
            <MenuItem value="minus">-1,234.00</MenuItem>
            <MenuItem value="parentheses">(1,234.00)</MenuItem>
            <MenuItem value="trailingMinus">1,234.00-</MenuItem>
          </TextField>
        </>
      ) : null}
      {date ? (
        <TextField
          select
          size="small"
          label={t('printTemplates.designer.properties.datePattern')}
          value={format.pattern ?? 'dd/MM/yyyy'}
          onChange={(event) => updateFormat({ pattern: event.target.value })}
        >
          {['dd/MM/yyyy', 'dd-MM-yyyy', 'MM/dd/yyyy', 'dd MMM yyyy', 'yyyy-MM-dd'].map(
            (pattern) => (
              <MenuItem key={pattern} value={pattern}>
                {pattern}
              </MenuItem>
            )
          )}
        </TextField>
      ) : null}
    </>
  );
}

export function TemplateDesigner({
  processId,
  document,
  onChange,
  isDefault = false,
  onDefaultChange,
}: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const designer = useTemplateDesigner(document, onChange);
  const requestControls = useQuery({
    queryKey: ['workflow', 'print-template-request-control-labels', processId],
    queryFn: ({ signal }) => loadRequestControls(processId, signal),
    enabled: processId > 0,
    staleTime: 2 * 60 * 1000,
  });
  const requestControlNames = React.useMemo(
    () =>
      new Map(
        (requestControls.data ?? []).map((control) => [
          control.requestControlId,
          document.language === 'ar' ? control.nameAr : control.name,
        ])
      ),
    [document.language, requestControls.data]
  );
  const pageWidth = document.page.orientation === 'portrait' ? 595 : 842;
  const pageHeight = document.page.orientation === 'portrait' ? 842 : 595;

  const changePage = (key: 'orientation' | 'size', value: string) =>
    onChange({
      ...document,
      page: { ...document.page, [key]: value } as PrintTemplateDocument['page'],
    });

  return (
    <Box
      sx={{
        height: 'min(78vh, 820px)',
        minHeight: 560,
        display: 'grid',
        gridTemplateRows: '54px minmax(0, 1fr)',
        border: '1px solid #d7dce2',
        bgcolor: '#eef1f4',
      }}
    >
      <Stack
        direction="row"
        spacing={1}
        sx={{ px: 1, alignItems: 'center', flexWrap: 'nowrap', overflowX: 'auto', borderBottom: '1px solid #d7dce2', bgcolor: '#fff' }}
      >
        <Typography sx={{ fontSize: 12, fontWeight: 700 }}>
          {t('printTemplates.designer.title')}
        </Typography>
        <Divider orientation="vertical" flexItem />
        {onDefaultChange ? (
          <FormControlLabel
            sx={{ m: 0, flexShrink: 0 }}
            control={
              <Switch
                size="small"
                checked={isDefault}
                onChange={(_, checked) => onDefaultChange(checked)}
              />
            }
            label={
              <Typography sx={{ fontSize: 11, whiteSpace: 'nowrap' }}>
                {t('printTemplates.fields.default')}
              </Typography>
            }
          />
        ) : null}
        <TextField
          select
          size="small"
          value={document.page.size}
          onChange={(event) => changePage('size', event.target.value)}
          sx={{ width: 90 }}
        >
          <MenuItem value="A4">A4</MenuItem>
          <MenuItem value="Letter">{t('printTemplates.designer.pageSizes.letter')}</MenuItem>
        </TextField>
        <TextField
          select
          size="small"
          label={t('printTemplates.fields.orientation')}
          value={document.page.orientation}
          onChange={(event) => changePage('orientation', event.target.value)}
          sx={{ width: 125 }}
        >
          <MenuItem value="portrait">{t('printTemplates.orientation.portrait')}</MenuItem>
          <MenuItem value="landscape">{t('printTemplates.orientation.landscape')}</MenuItem>
        </TextField>
        <TextField
          select
          size="small"
          label={t('printTemplates.fields.language')}
          value={document.language}
          onChange={(event) => {
            const language = event.target.value as PrintTemplateDocument['language'];
            onChange({
              ...document,
              language,
              direction: language === 'ar' ? 'rtl' : 'ltr',
            });
          }}
          sx={{ width: 150, flexShrink: 0 }}
        >
          <MenuItem value="en">{t('printTemplates.languages.english')}</MenuItem>
          <MenuItem value="ar">{t('printTemplates.languages.arabic')}</MenuItem>
        </TextField>
        <Box sx={{ flex: 1 }} />
        <ButtonGroup size="small">
          <Tooltip title={t('printTemplates.designer.moveUp')}>
            <span>
              <IconButton
                disabled={!designer.selectedElement}
                onClick={() => designer.moveSelected(-1)}
              >
                <ArrowUpwardOutlined fontSize="small" />
              </IconButton>
            </span>
          </Tooltip>
          <Tooltip title={t('printTemplates.designer.moveDown')}>
            <span>
              <IconButton
                disabled={!designer.selectedElement}
                onClick={() => designer.moveSelected(1)}
              >
                <ArrowDownwardOutlined fontSize="small" />
              </IconButton>
            </span>
          </Tooltip>
          <Tooltip title={t('actions.delete')}>
            <span>
              <IconButton
                disabled={!designer.selectedElement}
                color="error"
                onClick={designer.removeSelected}
              >
                <DeleteOutline fontSize="small" />
              </IconButton>
            </span>
          </Tooltip>
        </ButtonGroup>
      </Stack>
      <Box
        sx={{
          minHeight: 0,
          display: 'grid',
          gridTemplateColumns: '190px minmax(420px, 1fr) 250px',
        }}
      >
        <Paper
          square
          variant="outlined"
          sx={{ minWidth: 0, overflow: 'auto', borderWidth: 0, borderInlineEndWidth: 1 }}
        >
          <Typography sx={{ p: 1, fontSize: 11, fontWeight: 700 }}>
            {t('printTemplates.designer.componentsTitle')}
          </Typography>
          <Stack spacing={0.5} sx={{ px: 0.75 }}>
            {palette.map((item) => (
              <Button
                key={item.type}
                size="small"
                variant="text"
                startIcon={item.icon}
                onClick={() => designer.addElement(item.type)}
                sx={{
                  justifyContent: 'flex-start',
                  minHeight: 34,
                  color: 'text.primary',
                  fontSize: 11,
                }}
              >
                {t(`printTemplates.designer.components.${item.type}`)}
              </Button>
            ))}
          </Stack>
          <Divider sx={{ my: 1 }} />
          <Typography color="text.secondary" sx={{ px: 1, fontSize: 10, lineHeight: 1.5 }}>
            {t('printTemplates.designer.containerHint')}
          </Typography>
        </Paper>
        <Box sx={{ minWidth: 0, overflow: 'auto', p: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'center', minWidth: pageWidth + 32 }}>
            <Paper
              className="print-template-page"
              dir={document.direction}
              elevation={2}
              sx={{
                width: pageWidth,
                minHeight: pageHeight,
                bgcolor: '#fff',
                p: `${document.page.margins.top}px ${document.page.margins.right}px ${document.page.margins.bottom}px ${document.page.margins.left}px`,
                boxSizing: 'border-box',
              }}
            >
              {(['header', 'sections', 'footer'] as TemplateRegion[]).map((target) => (
                <Box
                  key={target}
                  onClick={() => designer.setRegion(target)}
                  sx={{
                    minHeight: target === 'sections' ? 520 : 70,
                    mb: target === 'footer' ? 0 : 1,
                    border: designer.region === target ? '1px dashed #8bb7e8' : '1px dashed #ddd',
                    p: 0.5,
                  }}
                >
                  <Typography sx={{ color: '#999', fontSize: 9, textTransform: 'uppercase' }}>
                    {t(`printTemplates.designer.regions.${target}`)}
                  </Typography>
                  {document[target].map((element) => (
                    <TemplateElementPreview
                      key={element.id}
                      element={element}
                      region={target}
                      selectedId={designer.selectedId}
                      onSelect={designer.select}
                      requestControlNames={requestControlNames}
                    />
                  ))}
                </Box>
              ))}
            </Paper>
          </Box>
        </Box>
        <Paper
          square
          variant="outlined"
          sx={{ minWidth: 0, overflow: 'auto', borderWidth: 0, borderInlineStartWidth: 1 }}
        >
          <Typography sx={{ p: 1, fontSize: 11, fontWeight: 700 }}>
            {t('printTemplates.designer.propertiesTitle')}
          </Typography>
          <ToggleButtonGroup
            exclusive
            fullWidth
            size="small"
            value={designer.region}
            onChange={(_, value: TemplateRegion | null) => value && designer.setRegion(value)}
            sx={{ px: 0.75 }}
          >
            <ToggleButton value="header">
              {t('printTemplates.designer.regions.header')}
            </ToggleButton>
            <ToggleButton value="sections">
              {t('printTemplates.designer.regions.sections')}
            </ToggleButton>
            <ToggleButton value="footer">
              {t('printTemplates.designer.regions.footer')}
            </ToggleButton>
          </ToggleButtonGroup>
          <ElementProperties
            element={designer.selectedElement}
            update={designer.updateSelected}
            processId={processId}
          />
        </Paper>
      </Box>
    </Box>
  );
}
