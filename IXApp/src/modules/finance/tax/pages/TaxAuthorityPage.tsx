import React, { useMemo } from 'react';
import { Box, Button, TextField, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { useQuery } from '@tanstack/react-query';
import { usePermission } from '@core/permissions/usePermission';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { ListDetailsListGridPage } from '@patterns/list-details-listgrid/ListDetailsListGridPage';
import { listGridLabelSx } from '@patterns/list-details-listgrid/fieldStyles';
import { PageEditSessionContext } from '@shared/components/data-grid/PageEditSession';
import type {
  DetailSectionConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { taxAuthorityApi, type TaxAuthorityRecord } from '../api/taxAuthorityApi';

const emptyTaxAuthority = (): TaxAuthorityRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  taxAuthority: '',
  name: '',
  taxAuthorityId: '',
  accountNum: '',
  phone: '',
  mobile: '',
  fax: '',
  sms: '',
  telex: '',
  extension: '',
  pager: '',
  email: '',
  url: '',
  address: '',
  roundOff: 0,
  roundOffType: 0,
  taxReportLayout: 0,
  useDefaultLayout: 0,
  separateTaxSummary: 0,
  printBlankPage: 0,
});
const textValue = (value: DetailValue): string => String(value ?? '');
const numberValue = (value: DetailValue): number => Number(value) || 0;

function AuthorityAddress({ value, onChange, editing }: { value: string; onChange: (value: string) => void; editing: boolean }) {
  const { t } = useAppTranslation();
  const session = React.useContext(PageEditSessionContext);
  const input = React.useRef<HTMLTextAreaElement>(null);
  const [focusRequested, setFocusRequested] = React.useState(false);
  React.useEffect(() => {
    if (editing && focusRequested) { input.current?.focus(); setFocusRequested(false); }
  }, [editing, focusRequested]);
  const label = t('taxAuthority.fields.address', 'Address');
  return <Box>
    <Button startIcon={<AddIcon />} size="small" disabled={session?.saving || (!editing && !session?.requestEdit)}
      onClick={() => { if (!editing) session?.requestEdit?.(); setFocusRequested(true); }}
      sx={{ mb: '16px', px: '4px', minWidth: 0, fontSize: 12 }}>
      {t('actions.new', 'New')}
    </Button>
    <Box sx={{ width: 220, maxWidth: '100%' }}>
      <Typography component="label" htmlFor="tax-authority-address" sx={listGridLabelSx}>{label}</Typography>
      <TextField id="tax-authority-address" inputRef={input} fullWidth multiline rows={5} size="small"
        value={value} onChange={(event) => onChange(event.target.value)}
        disabled={session?.saving} slotProps={{ input: { readOnly: !editing }, htmlInput: { 'aria-label': label } }}
        sx={{ '& .MuiInputBase-root': { fontSize: 12, borderRadius: '3px', minHeight: 118, alignItems: 'start', bgcolor: editing ? 'background.paper' : 'action.hover' } }} />
    </Box>
  </Box>;
}

export function TaxAuthorityPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const canViewVendors = usePermission('AccountsPayable.Vendors.View').hasPermission;
  const vendors = useQuery({ queryKey: ['tax-authority-vendors', company],
    queryFn: ({ signal }) => taxAuthorityApi.vendorOptions(signal), enabled: canViewVendors });
  const reportLayoutOptions = useMemo(
    () =>
      [
        'Default',
        'German',
        'English',
        'US',
        'Nordic',
        'Spanish',
        'Italian',
        'French',
        'Belgian',
        'Dutch',
        'Austrian',
      ].map((label, value) => ({ value: String(value), label })),
    []
  );
  const roundOffOptions = useMemo(
    () => [
      { value: '0', label: t('taxAuthority.options.normal', 'Normal') },
      { value: '1', label: t('taxAuthority.options.roundDown', 'Round down') },
      { value: '2', label: t('taxAuthority.options.roundUp', 'Round up') },
    ],
    [t]
  );
  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'general',
        title: t('common.general', 'General'),
        defaultExpanded: true,
        columns: 4,
        gridTemplateColumns: 'repeat(4, minmax(0, 220px))',
        columnGap: '50px',
        detailsPadding: '10px',
        groups: [
          {
            id: 'vendor',
            fields: [
              {
                name: 'accountNum',
                label: t('taxAuthority.fields.vendorAccount', 'Vendor account'),
                width: 163,
                type: 'select',
                options: vendors.data ?? [],
              },
            ],
          },
          {
            id: 'layout',
            fields: [
              {
                name: 'taxReportLayout',
                label: t('taxAuthority.fields.reportLayout', 'Report layout'),
                type: 'select',
                width: 220,
                options: reportLayoutOptions,
              },
            ],
          },
          {
            id: 'roundingForm',
            fields: [
              {
                name: 'roundOffType',
                label: t('taxAuthority.fields.roundingForm', 'Rounding form'),
                type: 'select',
                options: roundOffOptions,
                width: 163,
              },
            ],
          },
          {
            id: 'roundOff',
            fields: [
              {
                name: 'roundOff',
                label: t('taxAuthority.fields.roundOff', 'Round-off'),
                type: 'number',
                width: 105,
              },
            ],
          },
        ],
      },
      {
        id: 'address',
        title: t('taxAuthority.sections.address', 'Address'),
        defaultExpanded: true,
        detailsPadding: '8px 10px 10px',
        groups: [
          {
            id: 'addressValue',
            fields: [
              {
                name: 'address',
                label: t('taxAuthority.fields.address', 'Address'),
                multiline: true,
              },
            ],
          },
        ],
      },
      {
        id: 'contactInformation',
        title: t('taxAuthority.sections.contactInformation', 'Contact information'),
        defaultExpanded: true,
        columns: 5,
        gridTemplateColumns: 'repeat(5, minmax(0, 220px))',
        columnGap: '50px',
        detailsPadding: '10px',
        groups: [
          {
            id: 'telephone',
            fields: [
              { name: 'phone', label: t('taxAuthority.fields.telephone', 'Telephone'), width: 163 },
              { name: 'extension', label: t('taxAuthority.fields.extension', 'Extension'), width: 105 },
            ],
          },
          {
            id: 'mobile',
            fields: [
              { name: 'mobile', label: t('taxAuthority.fields.mobile', 'Mobile phone'), width: 163 },
              { name: 'pager', label: t('taxAuthority.fields.pager', 'Pager'), width: 163 },
            ],
          },
          {
            id: 'fax',
            fields: [
              { name: 'fax', label: t('taxAuthority.fields.fax', 'Fax'), width: 163 },
              { name: 'email', label: t('taxAuthority.fields.email', 'Email'), width: 220 },
            ],
          },
          {
            id: 'sms',
            fields: [
              { name: 'sms', label: t('taxAuthority.fields.sms', 'SMS') },
              { name: 'url', label: t('taxAuthority.fields.internetAddress', 'Internet address') },
            ],
          },
          {
            id: 'telex',
            fields: [{ name: 'telex', label: t('taxAuthority.fields.telex', 'Telex number'), width: 163 }],
          },
        ],
      },
      {
        id: 'reportSettings',
        title: t('taxAuthority.sections.reportSettings', 'Report settings'),
        groups: [
          {
            id: 'settings',
            fields: [
              {
                name: 'useDefaultLayout',
                label: t('taxAuthority.fields.useDefaultLayout', 'Use default layout'),
                type: 'boolean',
              },
              {
                name: 'separateTaxSummary',
                label: t('taxAuthority.fields.separateTaxSummary', 'Separate tax summary'),
                type: 'boolean',
              },
              {
                name: 'printBlankPage',
                label: t('taxAuthority.fields.printBlankPage', 'Print blank page'),
                type: 'boolean',
              },
            ],
          },
        ],
      },
    ],
    [reportLayoutOptions, roundOffOptions, vendors.data, t]
  );

  const config = useMemo<EnterpriseListDetailsConfig<TaxAuthorityRecord>>(
    () => ({
      recordTableName: 'TaxAuthorityAddress',
      dataSource: {
        type: 'remote',
        key: `tax-authorities:${company}`,
        load: taxAuthorityApi.list,
        create: taxAuthorityApi.create,
        update: taxAuthorityApi.update,
        delete: taxAuthorityApi.delete,
      },
      createRecord: emptyTaxAuthority,
      getPrimaryText: (record) => record.taxAuthority,
      getSecondaryText: (record) => record.name,
      matchesSearch: (record, query) =>
        `${record.taxAuthority} ${record.name} ${record.taxAuthorityId}`
          .toLowerCase()
          .includes(query.toLowerCase()),
      getValues: (record): DetailValues => ({
        accountNum: record.accountNum,
        taxReportLayout: String(record.taxReportLayout),
        roundOffType: String(record.roundOffType),
        roundOff: record.roundOff,
        address: record.address,
        phone: record.phone,
        extension: record.extension,
        mobile: record.mobile,
        pager: record.pager,
        fax: record.fax,
        email: record.email,
        sms: record.sms,
        url: record.url,
        telex: record.telex,
        useDefaultLayout: record.useDefaultLayout === 1,
        separateTaxSummary: record.separateTaxSummary === 1,
        printBlankPage: record.printBlankPage === 1,
      }),
      setValues: (record, values) => ({
        ...record,
        accountNum: textValue(values.accountNum),
        taxReportLayout: numberValue(values.taxReportLayout),
        roundOffType: numberValue(values.roundOffType),
        roundOff: numberValue(values.roundOff),
        address: textValue(values.address),
        phone: textValue(values.phone),
        extension: textValue(values.extension),
        mobile: textValue(values.mobile),
        pager: textValue(values.pager),
        fax: textValue(values.fax),
        email: textValue(values.email),
        sms: textValue(values.sms),
        url: textValue(values.url),
        telex: textValue(values.telex),
        useDefaultLayout: values.useDefaultLayout ? 1 : 0,
        separateTaxSummary: values.separateTaxSummary ? 1 : 0,
        printBlankPage: values.printBlankPage ? 1 : 0,
      }),
      headerFields: [
        {
          id: 'taxAuthority',
          width: 105,
          label: t('taxAuthority.fields.authority', 'Authority'),
          getValue: (x) => x.taxAuthority,
          setValue: (x, value) => ({ ...x, taxAuthority: String(value).toUpperCase() }),
        },
        {
          id: 'name',
          width: 220,
          label: t('taxAuthority.fields.name', 'Name'),
          getValue: (x) => x.name,
          setValue: (x, value) => ({ ...x, name: String(value) }),
        },
        {
          id: 'taxAuthorityId',
          width: 150,
          label: t('taxAuthority.fields.authorityIdentification', 'Authority identification'),
          getValue: (x) => x.taxAuthorityId,
          setValue: (x, value) => ({ ...x, taxAuthorityId: String(value).toUpperCase() }),
        },
      ],
      sections: ({ record, editing, onRecordChange }) => sections.map((section) => section.id === 'address'
        ? { ...section, groups: undefined, content: <AuthorityAddress editing={editing} value={record.address}
            onChange={(address) => onRecordChange({ ...record, address })} /> }
        : section),
      permissions: {
        view: PERMISSIONS.TAX_AUTHORITY_VIEW,
        create: PERMISSIONS.TAX_AUTHORITY_CREATE,
        edit: PERMISSIONS.TAX_AUTHORITY_EDIT,
        delete: PERMISSIONS.TAX_AUTHORITY_DELETE,
      },
      validate: (record) => ({
        ...(!record.taxAuthority.trim()
          ? {
              taxAuthority: t('validation.required', {
                field: t('taxAuthority.fields.authority', 'Authority'),
              }),
            }
          : {}),
        ...(!record.name.trim()
          ? { name: t('validation.required', { field: t('taxAuthority.fields.name', 'Name') }) }
          : {}),
      }),
      advancedFilter: {
        fieldLabel: t('taxAuthority.fields.authority', 'Authority'),
        getValue: (record) => record.taxAuthority,
        matches: (record, value) =>
          `${record.taxAuthority} ${record.name}`
            .toLowerCase()
            .includes(value.trim().toLowerCase()),
      },
      presentation: { mode: 'list', listWidth: 264, listResizable: true, headerMaxWidth: 500, recordHeaderMinHeight: 124 },
    }),
    [sections, t, company]
  );

  return (
    <ListDetailsListGridPage
      title={t('taxAuthority.title', 'Sales tax authorities')}
      config={config}
    />
  );
}
