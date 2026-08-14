import React, { useMemo } from 'react';
import { Box, Button, MenuItem, Select, TextField, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { sysNumberSequenceApi, type SysNumberSequenceRecord } from '../api/sysNumberSequenceApi';

const nullableNumber = (value: DetailValue | undefined): number | null =>
  value === '' || value == null ? null : Number(value);
const flagValue = (value: DetailValue | undefined): number => (value ? 1 : 0);

type SegmentType = 'Company' | 'Constant' | 'Alphanumeric';
interface NumberSequenceSegment {
  id: string;
  type: SegmentType;
  value: string;
}

const parseSegments = (format: string): NumberSequenceSegment[] => {
  const parts = format.match(/#+|[A-Za-z0-9]+|[^A-Za-z0-9#]+/g) ?? [];
  return parts.map((value, index) => ({
    id: `${index}-${value}`,
    type: /^#+$/.test(value)
      ? 'Alphanumeric'
      : /^[A-Za-z0-9]+$/.test(value)
        ? 'Company'
        : 'Constant',
    value,
  }));
};

function NumberSequenceSegments({
  record,
  editing,
  onRecordChange,
  t,
}: {
  record: SysNumberSequenceRecord;
  editing: boolean;
  onRecordChange: (record: SysNumberSequenceRecord) => void;
  t: (key: string, options?: Record<string, unknown>) => string;
}): React.ReactElement {
  const segments = useMemo(() => parseSegments(record.format), [record.format]);
  const [selectedIndex, setSelectedIndex] = React.useState(0);

  React.useEffect(() => setSelectedIndex(0), [record.id]);

  const updateSegments = (next: NumberSequenceSegment[]) => {
    onRecordChange({ ...record, format: next.map((segment) => segment.value).join('') });
  };
  const updateSegment = (index: number, changes: Partial<NumberSequenceSegment>) => {
    updateSegments(
      segments.map((segment, position) =>
        position === index ? { ...segment, ...changes } : segment
      )
    );
  };
  const add = () => {
    const next = [...segments, { id: crypto.randomUUID(), type: 'Constant' as const, value: '-' }];
    updateSegments(next);
    setSelectedIndex(next.length - 1);
  };
  const remove = () => {
    if (!segments.length) return;
    const next = segments.filter((_, index) => index !== selectedIndex);
    updateSegments(next);
    setSelectedIndex(Math.max(0, Math.min(selectedIndex, next.length - 1)));
  };
  const move = (offset: number) => {
    const target = selectedIndex + offset;
    if (target < 0 || target >= segments.length) return;
    const next = [...segments];
    [next[selectedIndex], next[target]] = [next[target], next[selectedIndex]];
    updateSegments(next);
    setSelectedIndex(target);
  };

  return (
    <Box sx={{ maxWidth: 680 }}>
      <Box sx={{ display: 'flex', height: 28, alignItems: 'center', mb: 0.75 }}>
        <Button
          size="small"
          startIcon={<AddIcon />}
          disabled={!editing}
          onClick={add}
          sx={segmentActionSx}
        >
          {t('actions.add')}
        </Button>
        <Button
          size="small"
          startIcon={<DeleteOutlineIcon />}
          disabled={!editing || !segments.length}
          onClick={remove}
          sx={segmentActionSx}
        >
          {t('actions.remove')}
        </Button>
        <Button
          size="small"
          startIcon={<ArrowUpwardIcon />}
          disabled={!editing || selectedIndex <= 0}
          onClick={() => move(-1)}
          sx={segmentActionSx}
        >
          {t('sysNumberSequence.actions.moveUp')}
        </Button>
        <Button
          size="small"
          startIcon={<ArrowDownwardIcon />}
          disabled={!editing || selectedIndex >= segments.length - 1}
          onClick={() => move(1)}
          sx={segmentActionSx}
        >
          {t('sysNumberSequence.actions.moveDown')}
        </Button>
      </Box>
      <Box sx={{ display: 'grid', gridTemplateColumns: '178px 190px 186px 126px', fontSize: 12 }}>
        <Box />
        <Box sx={segmentHeaderSx}>{t('sysNumberSequence.fields.segment')}</Box>
        <Box sx={segmentHeaderSx}>{t('sysNumberSequence.fields.value')}</Box>
        <Box sx={segmentHeaderSx}>{t('sysNumberSequence.fields.length')}</Box>
        {segments.map((segment, index) => (
          <React.Fragment key={segment.id}>
            <Box
              onClick={() => setSelectedIndex(index)}
              sx={{
                ...segmentCellSx,
                bgcolor: selectedIndex === index ? '#dbe7ff' : 'transparent',
              }}
            />
            <Box
              onClick={() => setSelectedIndex(index)}
              sx={{
                ...segmentCellSx,
                bgcolor: selectedIndex === index ? '#dbe7ff' : 'transparent',
              }}
            >
              {editing ? (
                <Select
                  variant="standard"
                  fullWidth
                  disableUnderline
                  value={segment.type}
                  onChange={(event) =>
                    updateSegment(index, { type: event.target.value as SegmentType })
                  }
                  sx={segmentInputSx}
                >
                  {(['Company', 'Constant', 'Alphanumeric'] as const).map((type) => (
                    <MenuItem key={type} value={type}>
                      {t(`sysNumberSequence.segmentTypes.${type.toLowerCase()}`)}
                    </MenuItem>
                  ))}
                </Select>
              ) : (
                t(`sysNumberSequence.segmentTypes.${segment.type.toLowerCase()}`)
              )}
            </Box>
            <Box
              onClick={() => setSelectedIndex(index)}
              sx={{
                ...segmentCellSx,
                bgcolor: selectedIndex === index ? '#dbe7ff' : 'transparent',
              }}
            >
              {editing ? (
                <TextField
                  variant="standard"
                  fullWidth
                  value={segment.value}
                  onChange={(event) => updateSegment(index, { value: event.target.value })}
                  sx={segmentTextFieldSx}
                />
              ) : (
                segment.value
              )}
            </Box>
            <Box
              onClick={() => setSelectedIndex(index)}
              sx={{
                ...segmentCellSx,
                justifyContent: 'flex-end',
                bgcolor: selectedIndex === index ? '#dbe7ff' : 'transparent',
              }}
            >
              {segment.value.length}
            </Box>
          </React.Fragment>
        ))}
      </Box>
      <Box sx={{ mt: 2.5, width: 163 }}>
        <Typography sx={{ mb: 0.5, fontSize: 12 }}>
          {t('sysNumberSequence.fields.format')}
        </Typography>
        <TextField
          variant="outlined"
          value={record.format}
          disabled
          sx={{
            '& .MuiInputBase-root': { height: 32, fontSize: 12 },
            '& input': { px: 0.75, py: 0.5 },
          }}
        />
      </Box>
    </Box>
  );
}

const segmentActionSx = {
  minWidth: 0,
  height: 27,
  px: 0.75,
  fontSize: 12,
  fontWeight: 400,
  '& .MuiButton-startIcon': { mr: 0.25 },
  '& svg': { fontSize: 17 },
};
const segmentHeaderSx = { px: 0.75, py: 0.5, borderBottom: '1px solid #c8c6c4' };
const segmentCellSx = {
  minHeight: 31,
  px: 0.75,
  display: 'flex',
  alignItems: 'center',
  borderBottom: '1px solid #c8c6c4',
  borderInlineEnd: '1px solid #c8c6c4',
  fontSize: 12,
  cursor: 'default',
};
const segmentInputSx = { width: '100%', fontSize: 12, '& .MuiSelect-select': { py: 0 } };
const segmentTextFieldSx = {
  '& .MuiInputBase-root': { height: 26, fontSize: 12 },
  '& input': { p: 0 },
};

const emptyNumberSequence = (): SysNumberSequenceRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  numberSequence: '',
  txt: '',
  latestCleanDateTime: null,
  latestCleanDateTimeTzId: null,
  lowest: 0,
  highest: 999999,
  nextRec: 1,
  blocked: 0,
  format: '######',
  continuous: 0,
  cyclic: 0,
  annotatedFormat: '{SEQ}',
  cleanAtAccess: 0,
  inUse: 1,
  noIncrement: 0,
  numberSequenceScope: null,
  cleanInterval: null,
  allowChangeUp: 0,
  allowChangeDown: 0,
  manual: 0,
  fetchAheadQty: null,
  fetchAhead: 0,
  modifiedTransactionId: null,
  partition: null,
  isActive: true,
  rowVersion: null,
  isDeleted: false,
  recVersion: 1,
  dataAreaId: 'dat',
});

export function SysNumberSequencePage(): React.ReactElement {
  const { t } = useAppTranslation();

  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'scope',
        title: t('sysNumberSequence.sections.scopeParameters'),
        visualVariant: 'legalEntity',
        gridTemplateColumns: '220px',
        groups: [
          {
            id: 'scope',
            fields: [
              {
                name: 'numberSequenceScope',
                label: t('sysNumberSequence.fields.scope'),
                type: 'number',
              },
            ],
          },
        ],
      },
      {
        id: 'segments',
        title: t('sysNumberSequence.sections.segments'),
        visualVariant: 'legalEntity',
      },
      {
        id: 'general',
        title: t('sysNumberSequence.sections.general'),
        visualVariant: 'legalEntity',
        gridTemplateColumns: 'minmax(420px, 520px) minmax(260px, 340px)',
        groups: [
          {
            id: 'setup',
            title: t('sysNumberSequence.groups.setup'),
            columns: 2,
            fields: [
              {
                name: 'inUse',
                label: t('sysNumberSequence.fields.inUse'),
                type: 'boolean',
              },
              { name: 'manual', label: t('sysNumberSequence.fields.manual'), type: 'boolean' },
              {
                name: 'blocked',
                label: t('sysNumberSequence.fields.stopped'),
                type: 'boolean',
              },
              {
                name: 'continuous',
                label: t('sysNumberSequence.fields.continuous'),
                type: 'boolean',
              },
              {
                name: 'lowest',
                label: t('sysNumberSequence.fields.smallest'),
                type: 'number',
                sectionTitle: t('sysNumberSequence.groups.numberAllocation'),
              },
              {
                name: 'highest',
                label: t('sysNumberSequence.fields.largest'),
                type: 'number',
                sectionTitle: t('sysNumberSequence.groups.numberAllocation'),
              },
              { name: 'nextRec', label: t('sysNumberSequence.fields.nextRec'), type: 'number' },
            ],
          },
          {
            id: 'changes',
            title: t('sysNumberSequence.groups.allowUserChanges'),
            fields: [
              {
                name: 'allowChangeDown',
                label: t('sysNumberSequence.fields.toLowerNumber'),
                type: 'boolean',
              },
              {
                name: 'allowChangeUp',
                label: t('sysNumberSequence.fields.toHigherNumber'),
                type: 'boolean',
              },
              { name: 'cyclic', label: t('sysNumberSequence.fields.cyclic'), type: 'boolean' },
              {
                name: 'noIncrement',
                label: t('sysNumberSequence.fields.noIncrement'),
                type: 'boolean',
              },
            ],
          },
        ],
      },
      {
        id: 'cleanup',
        title: t('sysNumberSequence.sections.automaticCleanup'),
        visualVariant: 'legalEntity',
        link: (
          <Typography sx={{ maxWidth: 1050, fontSize: 12, lineHeight: 1.45 }}>
            {t('sysNumberSequence.help.cleanup')}
          </Typography>
        ),
        gridTemplateColumns: 'minmax(280px, 330px) minmax(260px, 320px)',
        groups: [
          {
            id: 'activate-cleanup',
            title: t('sysNumberSequence.groups.activateCleanup'),
            columns: 2,
            fields: [
              {
                name: 'cleanAtAccess',
                label: t('sysNumberSequence.fields.cleanUp'),
                type: 'boolean',
              },
              {
                name: 'cleanInterval',
                label: t('sysNumberSequence.fields.interval'),
                type: 'number',
              },
            ],
          },
          {
            id: 'latest-cleanup',
            title: t('sysNumberSequence.groups.latestCleanup'),
            fields: [
              {
                name: 'latestCleanDateTime',
                label: t('sysNumberSequence.fields.dateTime'),
                type: 'display',
                disabled: true,
              },
              {
                name: 'latestCleanDateTimeTzId',
                label: t('sysNumberSequence.fields.timeZoneId'),
                type: 'number',
              },
            ],
          },
        ],
      },
      {
        id: 'performance',
        title: t('sysNumberSequence.sections.performance'),
        visualVariant: 'legalEntity',
        link: (
          <Typography sx={{ maxWidth: 1050, fontSize: 12, lineHeight: 1.45 }}>
            {t('sysNumberSequence.help.performance')}
          </Typography>
        ),
        gridTemplateColumns: 'minmax(320px, 350px) minmax(340px, 420px)',
        groups: [
          {
            id: 'preallocation',
            title: t('sysNumberSequence.groups.activatePreallocation'),
            columns: 2,
            fields: [
              {
                name: 'fetchAhead',
                label: t('sysNumberSequence.fields.preallocation'),
                type: 'boolean',
              },
              {
                name: 'fetchAheadQty',
                label: t('sysNumberSequence.fields.quantityOfNumbers'),
                type: 'number',
              },
            ],
          },
          {
            id: 'allocated',
            title: t('sysNumberSequence.groups.allocatedNumbers'),
            columns: 2,
            fields: [
              {
                name: 'allocatedNext',
                label: t('sysNumberSequence.fields.nextRec'),
                type: 'number',
                disabled: true,
              },
              {
                name: 'allocatedLargest',
                label: t('sysNumberSequence.fields.largest'),
                type: 'number',
                disabled: true,
              },
            ],
          },
        ],
      },
    ],
    [t]
  );

  const config: EnterpriseListDetailsConfig<SysNumberSequenceRecord> = {
    dataSource: {
      type: 'remote',
      key: 'system-number-sequences',
      load: (signal) => sysNumberSequenceApi.list(signal),
      create: sysNumberSequenceApi.create,
      update: sysNumberSequenceApi.update,
      delete: sysNumberSequenceApi.delete,
    },
    createRecord: emptyNumberSequence,
    getPrimaryText: (record) => record.numberSequence,
    getSecondaryText: (record) => record.txt || record.format,
    matchesSearch: (record, query) =>
      `${record.numberSequence} ${record.txt} ${record.format} ${record.annotatedFormat}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      numberSequence: record.numberSequence,
      txt: record.txt,
      lowest: record.lowest ?? '',
      highest: record.highest ?? '',
      nextRec: record.nextRec ?? '',
      numberSequenceScope: record.numberSequenceScope ?? '',
      annotatedFormat: record.annotatedFormat,
      format: record.format,
      segmentType: t('sysNumberSequence.segmentTypes.alphanumeric'),
      segmentLength: record.format.length,
      latestCleanDateTime: record.latestCleanDateTime ?? '',
      allocatedNext: record.nextRec ?? '',
      allocatedLargest: record.highest ?? '',
      cleanInterval: record.cleanInterval ?? '',
      latestCleanDateTimeTzId: record.latestCleanDateTimeTzId ?? '',
      fetchAheadQty: record.fetchAheadQty ?? '',
      continuous: Boolean(record.continuous),
      cyclic: Boolean(record.cyclic),
      manual: Boolean(record.manual),
      blocked: Boolean(record.blocked),
      inUse: Boolean(record.inUse),
      noIncrement: Boolean(record.noIncrement),
      cleanAtAccess: Boolean(record.cleanAtAccess),
      allowChangeUp: Boolean(record.allowChangeUp),
      allowChangeDown: Boolean(record.allowChangeDown),
      fetchAhead: Boolean(record.fetchAhead),
    }),
    setValues: (record, values) => ({
      ...record,
      numberSequence: String(values.numberSequence ?? ''),
      txt: String(values.txt ?? ''),
      lowest: nullableNumber(values.lowest),
      highest: nullableNumber(values.highest),
      nextRec: nullableNumber(values.nextRec),
      numberSequenceScope: nullableNumber(values.numberSequenceScope),
      annotatedFormat: String(values.annotatedFormat ?? ''),
      format: String(values.format ?? ''),
      cleanInterval: nullableNumber(values.cleanInterval),
      latestCleanDateTimeTzId: nullableNumber(values.latestCleanDateTimeTzId),
      fetchAheadQty: nullableNumber(values.fetchAheadQty),
      continuous: flagValue(values.continuous),
      cyclic: flagValue(values.cyclic),
      manual: flagValue(values.manual),
      blocked: flagValue(values.blocked),
      inUse: flagValue(values.inUse),
      noIncrement: flagValue(values.noIncrement),
      cleanAtAccess: flagValue(values.cleanAtAccess),
      allowChangeUp: flagValue(values.allowChangeUp),
      allowChangeDown: flagValue(values.allowChangeDown),
      fetchAhead: flagValue(values.fetchAhead),
    }),
    headerFields: [
      {
        id: 'numberSequence',
        label: t('sysNumberSequence.fields.numberSequenceCode'),
        linkStyle: true,
        getValue: (record) => record.numberSequence,
        setValue: (record, value) => ({ ...record, numberSequence: String(value) }),
      },
      {
        id: 'txt',
        label: t('sysNumberSequence.fields.name'),
        getValue: (record) => record.txt,
        setValue: (record, value) => ({ ...record, txt: String(value) }),
      },
    ],
    sections: ({ record, editing, onRecordChange }) =>
      sections.map((section) =>
        section.id === 'segments'
          ? {
              ...section,
              content: (
                <NumberSequenceSegments
                  record={record}
                  editing={editing}
                  onRecordChange={onRecordChange}
                  t={t}
                />
              ),
            }
          : section
      ),
    permissions: {
      view: 'System.NumberSequences.View',
      create: 'System.NumberSequences.Create',
      edit: 'System.NumberSequences.Edit',
      delete: 'System.NumberSequences.Delete',
    },
    validate: (record) => ({
      ...(!record.numberSequence.trim()
        ? {
            numberSequence: t('validation.required', {
              field: t('sysNumberSequence.fields.numberSequence'),
            }),
          }
        : {}),
      ...(!record.txt.trim()
        ? { txt: t('validation.required', { field: t('sysNumberSequence.fields.description') }) }
        : {}),
      ...(!record.format.trim()
        ? { format: t('validation.required', { field: t('sysNumberSequence.fields.format') }) }
        : {}),
      ...(!record.annotatedFormat.trim()
        ? {
            annotatedFormat: t('validation.required', {
              field: t('sysNumberSequence.fields.annotatedFormat'),
            }),
          }
        : {}),
      ...(record.lowest != null && record.lowest < 0
        ? { lowest: t('sysNumberSequence.validation.lowest') }
        : {}),
      ...(record.lowest != null && record.highest != null && record.highest <= record.lowest
        ? { highest: t('sysNumberSequence.validation.highest') }
        : {}),
    }),
    advancedFilter: {
      fieldLabel: t('sysNumberSequence.fields.numberSequence'),
      getValue: (record) => record.numberSequence,
      matches: (record, value) =>
        record.numberSequence.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
    commands: [
      { id: 'number-sequence', label: t('pages.sysNumberSequences.command') },
      { id: 'options', label: t('customerCommands.options') },
    ],
  };

  return (
    <ListDetailsPage
      variant="enterprise"
      title={t('pages.sysNumberSequences.title')}
      config={config}
    />
  );
}
