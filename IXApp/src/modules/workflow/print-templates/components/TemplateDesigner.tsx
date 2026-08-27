import React from 'react';
import {
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
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { dynamicRequestFormApi } from '../../api/dynamicRequestFormApi';
import { TemplateElementPreview } from './TemplateElementPreview';
import {
  useTemplateDesigner,
  type PhaseTwoElementType,
  type TemplateRegion,
} from '../hooks/useTemplateDesigner';
import type {
  PrintFieldElement,
  PrintTemplateDocument,
  PrintTemplateElement,
} from '../types/printTemplate.types';

interface Props {
  processId: number;
  document: PrintTemplateDocument;
  onChange: (document: PrintTemplateDocument) => void;
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

const palette: Array<{ type: PhaseTwoElementType; icon: React.ReactElement }> = [
  { type: 'text', icon: <TextFieldsOutlined /> },
  { type: 'field', icon: <DataObjectOutlined /> },
  { type: 'section', icon: <ViewAgendaOutlined /> },
  { type: 'row', icon: <TableRowsOutlined /> },
  { type: 'column', icon: <ViewColumnOutlined /> },
  { type: 'image', icon: <ImageOutlined /> },
  { type: 'divider', icon: <HorizontalRuleOutlined /> },
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

  const updateStyle = (
    key: 'fontSize' | 'fontWeight' | 'alignment' | 'width' | 'keepTogether',
    value: string | number | boolean | null
  ) => update((current) => ({ ...current, style: { ...current.style, [key]: value } }));

  return (
    <Stack spacing={1.25} sx={{ p: 1 }}>
      <Typography sx={{ fontSize: 12, fontWeight: 700 }}>
        {t(`printTemplates.designer.components.${element.type}`)}
      </Typography>
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
      {element.type === 'text' || element.type === 'field' ? (
        <>
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
            label={t('printTemplates.designer.properties.alignment')}
            value={element.style?.alignment ?? 'start'}
            onChange={(event) => updateStyle('alignment', event.target.value)}
          >
            <MenuItem value="start">{t('printTemplates.designer.alignments.start')}</MenuItem>
            <MenuItem value="center">{t('printTemplates.designer.alignments.center')}</MenuItem>
            <MenuItem value="end">{t('printTemplates.designer.alignments.end')}</MenuItem>
          </TextField>
        </>
      ) : null}
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
              source: event.target.value === 'system' ? 'requestNumber' : null,
            },
          }))
        }
      >
        <MenuItem value="system">{t('printTemplates.designer.sources.system')}</MenuItem>
        <MenuItem value="company">{t('printTemplates.designer.sources.company')}</MenuItem>
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

export function TemplateDesigner({ processId, document, onChange }: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const designer = useTemplateDesigner(document, onChange);
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
        gridTemplateRows: '42px minmax(0, 1fr)',
        border: '1px solid #d7dce2',
        bgcolor: '#eef1f4',
      }}
    >
      <Stack
        direction="row"
        spacing={1}
        sx={{ px: 1, alignItems: 'center', borderBottom: '1px solid #d7dce2', bgcolor: '#fff' }}
      >
        <Typography sx={{ fontSize: 12, fontWeight: 700 }}>
          {t('printTemplates.designer.title')}
        </Typography>
        <Divider orientation="vertical" flexItem />
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
          value={document.page.orientation}
          onChange={(event) => changePage('orientation', event.target.value)}
          sx={{ width: 125 }}
        >
          <MenuItem value="portrait">{t('printTemplates.orientation.portrait')}</MenuItem>
          <MenuItem value="landscape">{t('printTemplates.orientation.landscape')}</MenuItem>
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
