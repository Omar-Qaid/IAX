import React, { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import MapOutlinedIcon from '@mui/icons-material/MapOutlined';
import { Alert, Box, Button, Chip, Stack, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { LogisticsPostalAddressDrawer } from '@shared/components/logistics/LogisticsPostalAddressDrawer';
import { LogisticsElectronicAddressDrawer } from '@shared/components/logistics/LogisticsElectronicAddressDrawer';
import type { ElectronicAddressType, LogisticsElectronicAddress, LogisticsPostalAddress } from '@shared/types/logistics';
import { legalEntityService } from '../services/legalEntityService';
import type { LegalEntityAddress, LegalEntityContact, LegalEntityRecord } from '../types/legalEntityTypes';
import { documentApi } from '@shared/components/documents/documentApi';
import {
  LEGAL_ENTITY_DASHBOARD_IMAGE,
  LEGAL_ENTITY_REPORT_LOGO,
  LEGAL_ENTITY_TABLE_ID,
} from '../api/legalEntityImageAttachments';

const emptyEntity = (): LegalEntityRecord => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, party: 0, dataArea: '', name: '', languageId: null,
  currencyCode: null, taxLicenseNum: null, federalTaxId: null, bankAccount: null, calendar: null,
  timeZone: null, memo: null, arabicName: null, localizedRegion: null, logo: null, reportLogo: null,
  addresses: [], contacts: [],
  inHierarchy: true, useForFinancialConsolidation: false, useForFinancialElimination: false, fullName: '',
});

const value = (input: string | null): string => input ?? '';

export function LegalEntityPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();

  const config = useMemo<EnterpriseListDetailsConfig<LegalEntityRecord>>(() => ({
    recordTableName: 'CompanyInfo',
    attachments: { refTableId: LEGAL_ENTITY_TABLE_ID, getRefRecId: (record) => record.recId },
    dataSource: {
      type: 'remote', key: 'organization-legal-entities', load: async (signal) => (await legalEntityService.list(signal)).sort((left, right) => left.name.localeCompare(right.name)),
      create: (record) => legalEntityService.create(record), update: (record) => legalEntityService.update(record),
      delete: (record) => legalEntityService.delete(record),
    },
    createRecord: emptyEntity,
    getPrimaryText: (record) => record.name,
    getSecondaryText: (record) => record.dataArea,
    matchesSearch: (record, query) => `${record.name} ${record.dataArea}`.toLocaleLowerCase(currentLanguage.code).includes(query.toLocaleLowerCase(currentLanguage.code)),
    getValues: (record): DetailValues => ({
      memo: value(record.memo), arabicName: value(record.arabicName), localizedRegion: value(record.localizedRegion),
      taxLicenseNum: value(record.taxLicenseNum), federalTaxId: value(record.federalTaxId),
      languageId: value(record.languageId), timeZone: value(record.timeZone), currencyCode: value(record.currencyCode),
      bankAccount: value(record.bankAccount), calendar: record.calendar ?? '',
      inHierarchy: record.inHierarchy ?? true,
      useForFinancialConsolidation: record.useForFinancialConsolidation ?? false,
      useForFinancialElimination: record.useForFinancialElimination ?? false,
      fullName: record.fullName ?? '',
    }),
    setValues: (record, values) => ({
      ...record,
      memo: nullable(values.memo), arabicName: nullable(values.arabicName), localizedRegion: nullable(values.localizedRegion),
      taxLicenseNum: nullable(values.taxLicenseNum), federalTaxId: nullable(values.federalTaxId),
      languageId: nullable(values.languageId), timeZone: nullable(values.timeZone), currencyCode: nullable(values.currencyCode),
      bankAccount: nullable(values.bankAccount), calendar: values.calendar === '' ? null : Number(values.calendar),
      inHierarchy: Boolean(values.inHierarchy),
      useForFinancialConsolidation: Boolean(values.useForFinancialConsolidation),
      useForFinancialElimination: Boolean(values.useForFinancialElimination),
      fullName: String(values.fullName ?? ''),
    }),
    headerFields: [
      { id: 'name', label: t('legalEntities.fields.name'), getValue: (record) => record.name, setValue: (record, next) => ({ ...record, name: String(next) }) },
      { id: 'dataArea', label: t('legalEntities.fields.company'), getValue: (record) => record.dataArea, setValue: (record, next) => ({ ...record, dataArea: String(next) }) },
    ],
    sections: ({ record, editing, onRecordChange }) => [
      {
        id: 'general', title: t('legalEntities.sections.general'), visualVariant: 'legalEntity', defaultExpanded: false, gridTemplateColumns: '270px 270px 270px 270px 220px', columnGap: 0, minHeight: 199,
        groups: [
          { id: 'memo', width: 220, fields: [{ name: 'memo', label: t('legalEntities.fields.memo'), multiline: true, rows: 5 }] },
          { id: 'arabic', width: 220, fields: [{ name: 'arabicName', label: 'Arabic Name' }, { name: 'inHierarchy', label: 'In hierarchy', type: 'boolean' }, { name: 'useForFinancialConsolidation', label: 'Use for financial consolidation process', type: 'boolean' }] },
          { id: 'middle', width: 220, fields: [{ name: 'useForFinancialElimination', label: 'Use for financial elimination process', type: 'boolean' }, { name: 'fullName', label: 'Full name', disabled: true }, { name: 'localizedRegion', label: t('legalEntities.fields.localizedRegion') }] },
          { id: 'registrations', width: 164, fields: [{ name: 'taxLicenseNum', label: 'Tax registration number' }, { name: 'federalTaxId', label: 'CR Number' }] },
          { id: 'locale', width: 220, title: 'LANGUAGE', fields: [{ name: 'languageId', label: 'Language', type: 'select', options: [{ value: 'en-US', label: 'en-US' }, { value: 'ar-SA', label: 'ar-SA' }] }, { name: 'timeZone', label: 'TIME ZONE   ·   Time zone', type: 'select', options: [{ value: '(GMT+03:00) Kuwait, Riyadh', label: '(GMT+03:00) Kuwait, Riyadh' }, { value: 'Arab Standard Time', label: '(GMT+03:00) Kuwait, Riyadh' }] }] },
        ],
      },
      { id: 'addresses', title: 'Addresses', visualVariant: 'legalEntity', defaultExpanded: false, detailsPadding: '2px 10px 10px 12px', content: <AddressPanel key={`addresses-${record.id}`} record={record} editing={editing} onChange={onRecordChange} /> },
      { id: 'contacts', title: 'Contact information', visualVariant: 'legalEntity', defaultExpanded: false, content: <ContactPanel key={`contacts-${record.id}`} record={record} editing={editing} onChange={onRecordChange} /> },
      {
        id: 'statutory', title: t('legalEntities.sections.statutory'), visualVariant: 'legalEntity', defaultExpanded: false, columns: 3,
        groups: [
          { id: 'currency', title: 'GENERAL', fields: [{ name: 'currencyCode', label: 'Currency' }] },
          { id: 'bankAccount', fields: [{ name: 'bankAccount', label: 'Bank account' }] },
          { id: 'calendar', fields: [{ name: 'calendar', label: 'Fiscal calendar', type: 'number' }] },
        ],
      },
      { id: 'dashboard-image', title: 'Dashboard image', visualVariant: 'legalEntity', defaultExpanded: true, content: <CompanyImagePanel key={`dashboard-image-${record.id}`} label="Dashboard company image" attachmentName={LEGAL_ENTITY_DASHBOARD_IMAGE} refRecId={record.recId} value={record.logo} pendingFile={record.logoFile} mode="banner" editing={editing} onChange={(logo, logoFile) => onRecordChange({ ...record, logo, logoFile })} /> },
      { id: 'report-logo', title: 'Report company logo image', visualVariant: 'legalEntity', defaultExpanded: true, content: <CompanyImagePanel key={`report-logo-${record.id}`} label="Report company logo" attachmentName={LEGAL_ENTITY_REPORT_LOGO} refRecId={record.recId} value={record.reportLogo} pendingFile={record.reportLogoFile} mode="logo" editing={editing} onChange={(reportLogo, reportLogoFile) => onRecordChange({ ...record, reportLogo, reportLogoFile })} /> },
    ],
    presentation: { mode: 'list', listWidth: 281, listWidthStorageKey: 'organization.legal-entities.reference-v1', headerMaxWidth: 520 },
    permissions: { view: 'legalEntity.view', create: 'legalEntity.manage', edit: 'legalEntity.manage', delete: 'legalEntity.manage' },
    validate: (record) => ({
      ...(!record.name.trim() ? { name: 'Name is required' } : {}),
      ...(!record.dataArea.trim() ? { dataArea: 'Company is required' } : {}),
    }),
    advancedFilter: {
      fieldLabel: t('legalEntities.fields.name'),
      fields: [
        { id: 'name', label: t('legalEntities.fields.name'), getValue: (record) => record.name },
        { id: 'dataArea', label: t('legalEntities.fields.company'), getValue: (record) => record.dataArea },
      ],
      matches: (record, query) => record.name.toLocaleLowerCase().includes(query.trim().toLocaleLowerCase()),
    },
  }), [currentLanguage.code, t]);

  return <ListDetailsPage variant="enterprise" title={t('pages.legalEntities.title')} config={config} />;
}

interface CollectionPanelProps { record: LegalEntityRecord; editing: boolean; onChange: (record: LegalEntityRecord) => void }

interface CompanyImagePanelProps {
  label: string;
  attachmentName: string;
  refRecId: number;
  value: string | null;
  pendingFile?: File | null;
  mode: 'banner' | 'logo';
  editing: boolean;
  onChange: (value: string | null, file: File | null) => void;
}

const imageSource = (value: string | null): string | null => {
  const image = value?.trim();
  if (!image) return null;
  if (image.startsWith('data:image/')) return image;
  if (image.startsWith('/9j/')) return `data:image/jpeg;base64,${image}`;
  if (image.startsWith('R0lGOD')) return `data:image/gif;base64,${image}`;
  if (image.startsWith('UklGR')) return `data:image/webp;base64,${image}`;
  return `data:image/png;base64,${image}`;
};

function CompanyImagePanel({ label, attachmentName, refRecId, value, pendingFile, mode, editing, onChange }: CompanyImagePanelProps): React.ReactElement {
  const [error, setError] = useState<string | null>(null);
  const documents = useQuery({
    queryKey: ['documents', LEGAL_ENTITY_TABLE_ID, refRecId],
    queryFn: ({ signal }) => documentApi.list(LEGAL_ENTITY_TABLE_ID, refRecId, signal),
    enabled: refRecId > 0,
  });
  const attachment = documents.data?.items.find((document) => document.name === attachmentName);
  const preview = useQuery({
    queryKey: ['document-preview', attachment?.id],
    queryFn: ({ signal }) => documentApi.previewBlob(attachment!.id, signal),
    enabled: Boolean(attachment?.id),
  });
  const [attachmentSource, setAttachmentSource] = useState<string | null>(null);
  React.useEffect(() => {
    if (!preview.data) { setAttachmentSource(null); return undefined; }
    const url = URL.createObjectURL(preview.data);
    setAttachmentSource(url);
    return () => URL.revokeObjectURL(url);
  }, [preview.data]);
  const source = pendingFile !== undefined ? imageSource(value) : (attachmentSource ?? imageSource(value));
  const loadImage = (file: File | undefined) => {
    if (!file) return;
    if (!file.type.startsWith('image/')) { setError('Select a valid image file.'); return; }
    if (file.size > 2 * 1024 * 1024) { setError('The image must be 2 MB or smaller.'); return; }
    const reader = new FileReader();
    reader.onerror = () => setError('Unable to read the selected image.');
    reader.onload = () => {
      const result = typeof reader.result === 'string' ? reader.result : '';
      const base64 = result.match(/^data:image\/[^;]+;base64,(.+)$/)?.[1];
      if (!base64) { setError('The selected image could not be encoded.'); return; }
      setError(null);
      onChange(result, file);
    };
    reader.readAsDataURL(file);
  };

  return <Box sx={{ px: 1.25, py: 1.1, minHeight: mode === 'banner' ? 150 : 145 }}>
    <Stack direction="row" spacing={1.5} sx={{ alignItems: 'flex-start' }}>
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Stack direction="row" spacing={1.5} sx={{ mb: 1 }}>
          <Button component="label" size="small" disabled={!editing} sx={{ minWidth: 0, p: 0, fontSize: 12, textTransform: 'none' }}>Change<input hidden type="file" accept="image/png,image/jpeg,image/webp,image/gif" onChange={(event) => { loadImage(event.target.files?.[0]); event.currentTarget.value = ''; }} /></Button>
          <Button size="small" disabled={!editing || !source} onClick={() => { setError(null); onChange(null, null); }} sx={{ minWidth: 0, p: 0, color: 'text.secondary', fontSize: 12, textTransform: 'none' }}>Remove</Button>
        </Stack>
        {error ? <Alert severity="error" sx={{ mb: 1, py: 0 }}>{error}</Alert> : null}
        <Box sx={{ width: mode === 'banner' ? { xs: '100%', sm: 470 } : 112, height: mode === 'banner' ? 105 : 112, display: 'grid', placeItems: 'center', overflow: 'hidden', border: source && mode === 'logo' ? '1px solid #315efb' : '1px solid transparent', borderRadius: 0.5, bgcolor: '#fff' }}>
          {source ? <Box component="img" src={source} alt={label} sx={{ display: 'block', maxWidth: '100%', maxHeight: '100%', objectFit: 'contain' }} /> : <Typography color="text.secondary" sx={{ fontSize: 11 }}>No image</Typography>}
        </Box>
      </Box>
      {mode === 'banner' ? <Box sx={{ width: 260, pt: 2.5, display: { xs: 'none', md: 'block' } }}><Typography sx={{ mb: 1, fontSize: 12, color: 'text.secondary' }}>Dashboard company image type</Typography><Typography sx={{ width: 152, pb: 0.5, borderBottom: '1px solid #777', fontSize: 12 }}>Banner</Typography></Box> : null}
    </Stack>
  </Box>;
}

function AddressPanel({ record, editing, onChange }: CollectionPanelProps): React.ReactElement {
  const [selected, setSelected] = useState<(string | number)[]>(record.addresses[0]?.id ? [record.addresses[0].id] : []);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const selectedAddress = record.addresses.find((address) => selected.includes(address.id)) ?? null;
  const columns = useMemo<ColumnDef<LegalEntityAddress>[]>(() => [
    { field: 'description', headerName: 'Name or description', width: 223 },
    { field: 'address', headerName: 'Address', width: 221 },
    { field: 'roles', headerName: 'Purpose', width: 222, renderCell: ({ row }) => row.roles.join(', ') },
    { field: 'primary', headerName: 'Primary', width: 140, renderCell: ({ value }) => value ? 'Yes' : 'No' },
  ], []);

  const save = (draft: LogisticsPostalAddress) => {
    const id = selectedAddress?.id ?? `new-address-${crypto.randomUUID()}`;
    const next: LegalEntityAddress = {
      id, location: selectedAddress?.location ?? 0, locationId: draft.locationId ?? '',
      description: draft.description, address: formatAddress(draft), primary: Boolean(draft.primary),
      street: draft.street ?? '', city: draft.city ?? '', state: draft.state ?? '', zipCode: draft.zipCode ?? '',
      county: draft.county ?? '', countryRegionId: draft.countryRegionId, districtName: draft.district ?? '',
      validFrom: draft.validFrom || null, validTo: draft.validTo || null, roles: draft.roles ?? ['Business'],
    };
    onChange({ ...record, addresses: selectedAddress ? record.addresses.map((item) => item.id === id ? next : item) : [...record.addresses, next] });
    setSelected([id]);
  };

  return <>
    <TabularDetailPanel rows={record.addresses} columns={columns} addLabel="Add" removeLabel="Remove"
      selectedIds={selected} onSelectionChange={setSelected} disabled={!editing}
      onAdd={() => { setSelected([]); setDrawerOpen(true); }}
      actions={[
        { id: 'edit', label: 'Edit', ariaLabel: 'Edit address', icon: <EditOutlinedIcon sx={{ fontSize: 16 }} />, disabled: !selectedAddress, onClick: () => setDrawerOpen(true) },
        { id: 'map', label: 'Map', icon: <MapOutlinedIcon />, disabled: !selectedAddress?.address, onClick: () => selectedAddress && window.open(`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(selectedAddress.address)}`, '_blank', 'noopener,noreferrer') },
        { id: 'more', label: 'More options' },
      ]}
      storageKey="organization.legal-entity.addresses" height={360} rowHeight={117} />
    <LogisticsPostalAddressDrawer open={drawerOpen} onClose={() => setDrawerOpen(false)} onSave={save} initialData={selectedAddress ? toPostalAddress(selectedAddress) : null} />
  </>;
}

function ContactPanel({ record, editing, onChange }: CollectionPanelProps): React.ReactElement {
  const [selected, setSelected] = useState<(string | number)[]>(record.contacts[0]?.id ? [record.contacts[0].id] : []);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const selectedContact = record.contacts.find((contact) => selected.includes(contact.id)) ?? null;
  const columns = useMemo<ColumnDef<LegalEntityContact>[]>(() => [
    { field: 'description', headerName: 'Description', minWidth: 160, flex: 1 },
    { field: 'type', headerName: 'Type', width: 120 },
    { field: 'number', headerName: 'Contact number/address', minWidth: 220, flex: 1 },
    { field: 'extension', headerName: 'Extension', width: 100 },
    { field: 'primary', headerName: 'Primary', width: 85, renderCell: ({ value }) => value ? <Chip size="small" label="Yes" color="primary" variant="outlined" /> : 'No' },
  ], []);

  const save = (draft: LogisticsElectronicAddress) => {
    const id = selectedContact?.id ?? `new-contact-${crypto.randomUUID()}`;
    const next: LegalEntityContact = {
      id, location: selectedContact?.location ?? 0, locationId: draft.locationId ?? '', description: draft.description,
      type: draft.type, number: draft.number, extension: draft.extension ?? '', primary: Boolean(draft.primary), roles: draft.roles ?? ['Business'],
    };
    onChange({ ...record, contacts: selectedContact ? record.contacts.map((item) => item.id === id ? next : item) : [...record.contacts, next] });
    setSelected([id]);
  };

  return <>
    <TabularDetailPanel rows={record.contacts} columns={columns} addLabel="Add" removeLabel="Remove"
      selectedIds={selected} onSelectionChange={setSelected} disabled={!editing}
      onAdd={() => { setSelected([]); setDrawerOpen(true); }}
      onRemove={() => { onChange({ ...record, contacts: record.contacts.filter((item) => !selected.includes(item.id)) }); setSelected([]); }}
      actions={[{ id: 'edit', label: 'Advanced', icon: <EditOutlinedIcon />, disabled: !selectedContact, onClick: () => setDrawerOpen(true) }]}
      storageKey="organization.legal-entity.contacts" height={148} />
    <LogisticsElectronicAddressDrawer open={drawerOpen} onClose={() => setDrawerOpen(false)} onSave={save} initialData={selectedContact ? toElectronicAddress(selectedContact) : null} />
  </>;
}

function nullable(input: unknown): string | null { const result = String(input ?? '').trim(); return result || null; }

function formatAddress(address: LogisticsPostalAddress): string {
  return [address.street, address.district, address.city, address.state, address.zipCode, address.countryRegionId].filter(Boolean).join(', ');
}

function toPostalAddress(address: LegalEntityAddress): LogisticsPostalAddress {
  return {
    id: address.id, locationId: address.locationId, description: address.description, roles: address.roles,
    validFrom: address.validFrom?.split('T')[0] ?? '', validTo: address.validTo?.split('T')[0] ?? '',
    countryRegionId: address.countryRegionId, state: address.state, city: address.city, district: address.districtName,
    street: address.street, zipCode: address.zipCode, county: address.county, primary: address.primary,
  };
}

function toElectronicAddress(contact: LegalEntityContact): LogisticsElectronicAddress {
  const supported: ElectronicAddressType[] = ['Phone', 'Email', 'URL', 'Telex', 'Fax', 'InstantMessage'];
  return {
    id: contact.id, locationId: contact.locationId, description: contact.description,
    type: supported.includes(contact.type as ElectronicAddressType) ? contact.type as ElectronicAddressType : 'Phone',
    number: contact.number, extension: contact.extension, roles: contact.roles, primary: contact.primary,
  };
}
