import React, { useCallback, useMemo, useState } from 'react';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { DetailSectionConfig, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { LogisticsElectronicAddressDrawer, LogisticsPostalAddressDrawer } from '@shared/components/logistics';
import type { ElectronicAddressType, LogisticsElectronicAddress, LogisticsPostalAddress } from '@shared/types/logistics';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface LegalEntity { id: string; name: string; company: string; values: DetailValues }
interface AddressRow { id: string; description: string; address: string; purpose: string; primary: string; logistics: LogisticsPostalAddress }
interface ContactRow { id: string; description: string; type: ElectronicAddressType; contact: string; extension: string; primary: string; logistics?: LogisticsElectronicAddress }

const defaults: DetailValues = { memo: '', arabicName: 'شركة الحياة لمواد البناء المحدودة', hierarchy: true, consolidation: false, elimination: false, fullName: '', localizedRegion: '', language: 'en-US', timeZone: '(GMT+03:00) Kuwait, Riyadh', branch: '', dunsNumber: '', naics: '', taxRepresentative: '' };
const INITIAL_ENTITIES: LegalEntity[] = [
  { id: 'entity-hbmc', name: 'AlHayat Building Materials Company', company: 'HBMC', values: { ...defaults } },
  { id: 'entity-dat', name: 'Company accounts data', company: 'dat', values: { ...defaults, arabicName: '' } },
];
const INITIAL_ADDRESS: LogisticsPostalAddress = { id: 'address-1', locationId: 'LOC-0001', description: 'AlHayat Building Materials Company', roles: ['Business'], validFrom: '2024-01-01', validTo: '2154-12-31', countryRegionId: 'SAU', state: 'MKH', city: 'JED', district: 'Al-Safa district', street: '', building: '7926', zipCode: '23241', primary: true, primaryForCountry: true };
const INITIAL_ADDRESSES: AddressRow[] = [{ id: 'address-1', description: INITIAL_ADDRESS.description, address: 'Saudi Arabia, Makkah Region\nJeddah, Al-Safa district, 7926, 23241', purpose: 'Business', primary: 'Yes', logistics: INITIAL_ADDRESS }];
const INITIAL_CONTACTS: ContactRow[] = [
  { id: 'contact-phone', description: 'phone', type: 'Phone', contact: '00966126407777', extension: '', primary: '✓' },
  { id: 'contact-fax', description: 'FAX', type: 'Fax', contact: '00966122272727', extension: '', primary: '✓' },
  { id: 'contact-telex', description: 'Telex', type: 'Telex', contact: '15541141214411', extension: '', primary: '✓' },
];

export function LegalEntityPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const [records, setRecords] = useState(INITIAL_ENTITIES);
  const [addresses, setAddresses] = useState(INITIAL_ADDRESSES);
  const [contacts, setContacts] = useState(INITIAL_CONTACTS);
  const [addressSelection, setAddressSelection] = useState<(string | number)[]>([INITIAL_ADDRESSES[0].id]);
  const [contactSelection, setContactSelection] = useState<(string | number)[]>([INITIAL_CONTACTS[0].id]);
  const [addressDrawerOpen, setAddressDrawerOpen] = useState(false);
  const [contactDrawerOpen, setContactDrawerOpen] = useState(false);
  const [editingAddress, setEditingAddress] = useState<LogisticsPostalAddress | null>(null);
  const [editingContact, setEditingContact] = useState<LogisticsElectronicAddress | null>(null);

  const openNewAddress = useCallback(() => { setEditingAddress(null); setAddressDrawerOpen(true); }, []);
  const openSelectedAddress = useCallback(() => {
    const row = addresses.find((item) => item.id === addressSelection[0]);
    if (row) { setEditingAddress(row.logistics); setAddressDrawerOpen(true); }
  }, [addressSelection, addresses]);
  const saveAddress = useCallback((value: LogisticsPostalAddress) => {
    const id = String(value.id ?? editingAddress?.id ?? `address-${Date.now()}`);
    const country = value.countryRegionId === 'SAU' ? 'Saudi Arabia' : value.countryRegionId;
    const address = [country, value.state, value.city, value.district, value.street, value.building, value.zipCode].filter(Boolean).join(', ');
    const row: AddressRow = { id, description: value.description, address, purpose: value.roles?.join(', ') || 'Business', primary: value.primary ? 'Yes' : 'No', logistics: { ...value, id } };
    setAddresses((current) => current.some((item) => item.id === id) ? current.map((item) => item.id === id ? row : item) : [...current, row]);
    setAddressSelection([id]);
  }, [editingAddress]);

  const contactFromRow = useCallback((row: ContactRow): LogisticsElectronicAddress => row.logistics ?? ({ id: row.id, description: row.description, type: row.type, number: row.contact, extension: row.extension, roles: ['Business'], primary: row.primary === 'Yes' || row.primary.includes('✓') }), []);
  const openNewContact = useCallback(() => { setEditingContact(null); setContactDrawerOpen(true); }, []);
  const openSelectedContact = useCallback(() => {
    const row = contacts.find((item) => item.id === contactSelection[0]);
    if (row) { setEditingContact(contactFromRow(row)); setContactDrawerOpen(true); }
  }, [contactFromRow, contactSelection, contacts]);
  const saveContact = useCallback((value: LogisticsElectronicAddress) => {
    const id = String(value.id ?? editingContact?.id ?? `contact-${Date.now()}`);
    const row: ContactRow = { id, description: value.description, type: value.type, contact: value.number, extension: value.extension || '', primary: value.primary ? 'Yes' : 'No', logistics: { ...value, id } };
    setContacts((current) => current.some((item) => item.id === id) ? current.map((item) => item.id === id ? row : item) : [...current, row]);
    setContactSelection([id]);
  }, [editingContact]);
  const addressColumns = useMemo<ColumnDef<AddressRow>[]>(() => [
    { field: 'description', headerName: 'legalEntities.addresses.description', width: 210 }, { field: 'address', headerName: 'legalEntities.addresses.address', width: 280 }, { field: 'purpose', headerName: 'legalEntities.addresses.purpose', width: 170 }, { field: 'primary', headerName: 'legalEntities.addresses.primary', minWidth: 100, flex: 1 },
  ], []);
  const contactColumns = useMemo<ColumnDef<ContactRow>[]>(() => [
    { field: 'description', headerName: 'fields.description', width: 165 }, { field: 'type', headerName: 'legalEntities.contacts.type', width: 125 }, { field: 'contact', headerName: 'legalEntities.contacts.contact', width: 220 }, { field: 'extension', headerName: 'fields.extension', width: 100 }, { field: 'primary', headerName: 'legalEntities.addresses.primary', minWidth: 100, flex: 1 },
  ], []);
  const sections = useMemo<DetailSectionConfig[]>(() => [
    { id: 'general', title: t('legalEntities.sections.general'), columns: 8, groups: [
      { id: 'memo', fields: [{ name: 'memo', label: t('legalEntities.fields.memo') }] },
      { id: 'arabic', fields: [{ name: 'arabicName', label: t('fields.arabicName') }] },
      { id: 'hierarchy', fields: [{ name: 'hierarchy', label: t('legalEntities.fields.hierarchy'), type: 'boolean' }] },
      { id: 'processes', fields: [{ name: 'consolidation', label: t('legalEntities.fields.consolidation'), type: 'boolean' }, { name: 'elimination', label: t('legalEntities.fields.elimination'), type: 'boolean' }] },
      { id: 'fullName', fields: [{ name: 'fullName', label: t('legalEntities.fields.fullName') }] },
      { id: 'region', fields: [{ name: 'localizedRegion', label: t('legalEntities.fields.localizedRegion') }] },
      { id: 'language', title: t('legalEntities.groups.language'), fields: [{ name: 'language', label: t('legalEntities.fields.language') }] },
      { id: 'timeZone', title: t('legalEntities.groups.timeZone'), fields: [{ name: 'timeZone', label: t('legalEntities.fields.timeZone') }] },
    ] },
    { id: 'addresses', title: t('legalEntities.sections.addresses'), content: <TabularDetailPanel rows={addresses} columns={addressColumns} addLabel={t('actions.add')} removeLabel={t('actions.delete')} selectedIds={addressSelection} onSelectionChange={setAddressSelection} onAdd={openNewAddress} actions={[{ id: 'edit', label: t('actions.edit'), onClick: openSelectedAddress, disabled: addressSelection.length === 0 }, { id: 'map', label: t('legalEntities.actions.map') }, { id: 'more', label: t('legalEntities.actions.moreOptions') }]} storageKey="organization.legal-entities.addresses" height={218} /> },
    { id: 'contacts', title: t('legalEntities.sections.contacts'), content: <TabularDetailPanel rows={contacts} columns={contactColumns} addLabel={t('actions.add')} removeLabel={t('actions.remove')} selectedIds={contactSelection} onSelectionChange={setContactSelection} onAdd={openNewContact} onRemove={() => { setContacts((current) => current.filter((row) => !contactSelection.includes(row.id))); setContactSelection([]); }} actions={[{ id: 'edit', label: t('actions.edit'), onClick: openSelectedContact, disabled: contactSelection.length === 0 }, { id: 'advanced', label: t('legalEntities.actions.advanced') }]} storageKey="organization.legal-entities.contacts" height={124} /> },
    { id: 'statutory', title: t('legalEntities.sections.statutory'), groups: [
      { id: 'general', title: t('legalEntities.sections.general'), fields: [{ name: 'branch', label: t('legalEntities.fields.branch') }] },
      { id: 'duns', fields: [{ name: 'dunsNumber', label: t('legalEntities.fields.dunsNumber') }] },
      { id: 'naics', fields: [{ name: 'naics', label: t('legalEntities.fields.naics') }, { name: 'taxRepresentative', label: t('legalEntities.fields.taxRepresentative') }] },
    ] },
  ], [addressColumns, addressSelection, addresses, contactColumns, contactSelection, contacts, openNewAddress, openNewContact, openSelectedAddress, openSelectedContact, t]);
  const header = (id: 'name' | 'company') => ({ id, label: t(`legalEntities.fields.${id}`), getValue: (record: LegalEntity) => record[id], setValue: (record: LegalEntity, value: string | number | boolean) => ({ ...record, [id]: String(value) }) });
  const config: EnterpriseListDetailsConfig<LegalEntity> = {
    dataSource: { type: 'controlled', records, onRecordsChange: setRecords },
    createRecord: () => ({ id: `entity-${Date.now()}`, name: '', company: '', values: { ...defaults } }),
    getPrimaryText: (record) => record.name, getSecondaryText: (record) => record.company,
    matchesSearch: (record, query) => `${record.name} ${record.company}`.toLocaleLowerCase(currentLanguage.code).includes(query.toLocaleLowerCase(currentLanguage.code)),
    getValues: (record) => record.values, setValues: (record, values) => ({ ...record, values }), headerFields: [header('name'), header('company')], sections,
    presentation: { mode: 'list', listWidth: 176, headerMaxWidth: 360 },
    permissions: { view: 'legalEntity.view', create: 'legalEntity.manage', edit: 'legalEntity.manage', delete: 'legalEntity.manage' },
    validate: (record) => ({ ...(!record.name.trim() ? { name: t('validation.required', { field: t('legalEntities.fields.name') }) } : {}), ...(!record.company.trim() ? { company: t('validation.required', { field: t('legalEntities.fields.company') }) } : {}) }),
    advancedFilter: { fieldLabel: t('legalEntities.fields.name'), fields: [{ id: 'name', label: t('legalEntities.fields.name'), getValue: (record) => record.name }, { id: 'company', label: t('legalEntities.fields.company'), getValue: (record) => record.company }], matches: (record, value) => record.name.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()) },
    commands: ['hierarchy', 'registrationIds', 'registrationSearch', 'electronicProperties', 'options'].map((id) => ({ id, label: t(`legalEntities.commands.${id}`) })),
  };
  return <ListDetailsPage
    variant="enterprise"
    title={t('pages.legalEntities.title')}
    config={config}
    dialogs={<>
      <LogisticsPostalAddressDrawer open={addressDrawerOpen} initialData={editingAddress} onClose={() => setAddressDrawerOpen(false)} onSave={saveAddress} />
      <LogisticsElectronicAddressDrawer open={contactDrawerOpen} initialData={editingContact} onClose={() => setContactDrawerOpen(false)} onSave={saveContact} />
    </>}
  />;
}
