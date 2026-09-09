import React, { useMemo, useState } from 'react';
import { Alert, Chip } from '@mui/material';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import MapOutlinedIcon from '@mui/icons-material/MapOutlined';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import { ApiError } from '@core/api/apiError';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { ColumnDef } from '@shared/components/data-grid/types';
import type { LogisticsElectronicAddress, LogisticsPostalAddress } from '@shared/types/logistics';
import { LogisticsPostalAddressDrawer } from './LogisticsPostalAddressDrawer';
import { LogisticsElectronicAddressDrawer } from './LogisticsElectronicAddressDrawer';

interface StoredPostalAddress extends LogisticsPostalAddress {
  id: string;
  location: number;
  address?: string;
}

interface StoredElectronicAddress extends LogisticsElectronicAddress {
  id: string;
  location: number;
}

interface PostalAddressPayload {
  id: string;
  location: number;
  locationId: string;
  description: string;
  address: string;
  primary: boolean;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  county: string;
  countryRegionId: string;
  districtName: string;
  validFrom: string | null;
  validTo: string | null;
  roles: string[];
}

interface ElectronicAddressPayload {
  id: string;
  location: number;
  locationId: string;
  description: string;
  type: string;
  number: string;
  extension: string;
  primary: boolean;
  roles: string[];
}

interface PartyPanelProps {
  partyId: number;
  editing: boolean;
  storageKey: string;
}

const requireData = <T,>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The party logistics response did not contain data.', 500);
  return response.data;
};

const toPostalAddressPayload = (
  address: StoredPostalAddress | LogisticsPostalAddress,
): PostalAddressPayload => ({
  id: address.id == null ? '' : String(address.id),
  location: 'location' in address ? address.location : 0,
  locationId: address.locationId ?? '',
  description: address.description.trim(),
  address: 'address' in address ? address.address ?? '' : '',
  primary: address.primary ?? false,
  street: address.street ?? '',
  city: address.city ?? '',
  state: address.state ?? '',
  zipCode: address.zipCode ?? '',
  county: address.county ?? '',
  countryRegionId: address.countryRegionId,
  districtName: address.district ?? '',
  validFrom: address.validFrom || null,
  validTo: address.validTo || null,
  roles: address.roles ?? [],
});

const toElectronicAddressPayload = (
  contact: StoredElectronicAddress | LogisticsElectronicAddress,
): ElectronicAddressPayload => ({
  id: contact.id == null ? '' : String(contact.id),
  location: 'location' in contact ? contact.location : 0,
  locationId: contact.locationId ?? '',
  description: contact.description.trim(),
  type: contact.type,
  number: contact.number.trim(),
  extension: contact.extension ?? '',
  primary: contact.primary ?? false,
  roles: contact.roles ?? [],
});

const partyLogisticsApi = {
  async addresses(partyId: number, signal?: AbortSignal): Promise<StoredPostalAddress[]> {
    const response = await apiClient.get<ApiResponse<StoredPostalAddress[]>>(`/v1/LogisticsPostalAddress/Party/${partyId}`, { signal });
    return requireData(response.data);
  },
  async saveAddress(partyId: number, address: StoredPostalAddress | LogisticsPostalAddress, update: boolean): Promise<void> {
    const method = update ? apiClient.put : apiClient.post;
    const response = await method<ApiResponse<StoredPostalAddress>>(
      `/v1/LogisticsPostalAddress/Party/${partyId}`,
      toPostalAddressPayload(address),
    );
    requireData(response.data);
  },
  async deleteAddress(partyId: number, location: number): Promise<void> {
    await apiClient.delete(`/v1/LogisticsPostalAddress/Party/${partyId}/${location}`);
  },
  async contacts(partyId: number, signal?: AbortSignal): Promise<StoredElectronicAddress[]> {
    const response = await apiClient.get<ApiResponse<StoredElectronicAddress[]>>(`/v1/LogisticsElectronicAddress/Party/${partyId}`, { signal });
    return requireData(response.data);
  },
  async saveContact(partyId: number, contact: StoredElectronicAddress | LogisticsElectronicAddress, update: boolean): Promise<void> {
    const method = update ? apiClient.put : apiClient.post;
    const response = await method<ApiResponse<StoredElectronicAddress>>(
      `/v1/LogisticsElectronicAddress/Party/${partyId}`,
      toElectronicAddressPayload(contact),
    );
    requireData(response.data);
  },
  async deleteContact(partyId: number, location: number): Promise<void> {
    await apiClient.delete(`/v1/LogisticsElectronicAddress/Party/${partyId}/${location}`);
  },
};

export function PartyPostalAddressPanel({ partyId, editing, storageKey }: PartyPanelProps): React.ReactElement {
  const { t } = useAppTranslation();
  const client = useQueryClient();
  const validPartyId = Number.isFinite(partyId) && partyId > 0 ? partyId : null;
  const key = ['party-postal-addresses', validPartyId] as const;
  const query = useQuery({ queryKey: key, queryFn: ({ signal }) => partyLogisticsApi.addresses(validPartyId!, signal), enabled: validPartyId !== null });
  const rows = query.data ?? [];
  const [selectedIds, setSelectedIds] = useState<(string | number)[]>([]);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const selected = rows.find((row) => selectedIds.includes(row.id)) ?? null;
  const columns = useMemo<ColumnDef<StoredPostalAddress>[]>(() => [
    { field: 'description', headerName: t('legalEntities.addresses.description'), width: 220 },
    { field: 'address', headerName: t('legalEntities.addresses.address'), minWidth: 260, flex: 1 },
    { field: 'roles', headerName: t('legalEntities.addresses.purpose'), width: 180, renderCell: ({ row }) => row.roles?.join(', ') ?? '' },
    { field: 'primary', headerName: t('legalEntities.addresses.primary'), width: 90, renderCell: ({ value }) => value ? t('common.yes') : t('common.no') },
  ], [t]);
  const run = async (action: () => Promise<void>) => {
    setError(null);
    try { await action(); await client.invalidateQueries({ queryKey: key }); setDrawerOpen(false); }
    catch (reason) { setError(reason instanceof Error ? reason.message : String(reason)); }
  };
  return <>
    {(error || query.error) && <Alert severity="error" sx={{ m: 1 }}>{error ?? (query.error instanceof Error ? query.error.message : String(query.error))}</Alert>}
    <TabularDetailPanel rows={rows} columns={columns} addLabel={t('actions.add')} removeLabel={t('actions.remove')} selectedIds={selectedIds} onSelectionChange={setSelectedIds} disabled={!editing || validPartyId === null || query.isLoading} onAdd={() => { if (validPartyId === null) return; setSelectedIds([]); setDrawerOpen(true); }} onRemove={() => selected && validPartyId !== null && void run(() => partyLogisticsApi.deleteAddress(validPartyId, selected.location))} actions={[
      { id: 'edit', label: t('actions.edit'), icon: <EditOutlinedIcon />, disabled: !selected || validPartyId === null, onClick: () => setDrawerOpen(true) },
      { id: 'map', label: t('legalEntities.actions.map'), icon: <MapOutlinedIcon />, disabled: !selected?.address, onClick: () => selected?.address && window.open(`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(selected.address)}`, '_blank', 'noopener,noreferrer') },
    ]} storageKey={storageKey} height={220} />
    <LogisticsPostalAddressDrawer open={drawerOpen && validPartyId !== null} onClose={() => setDrawerOpen(false)} onSave={(draft) => validPartyId !== null && void run(() => partyLogisticsApi.saveAddress(validPartyId, selected ? { ...draft, id: selected.id, location: selected.location } : draft, Boolean(selected)))} initialData={selected} />
  </>;
}

export function PartyElectronicAddressPanel({ partyId, editing, storageKey }: PartyPanelProps): React.ReactElement {
  const { t } = useAppTranslation();
  const client = useQueryClient();
  const validPartyId = Number.isFinite(partyId) && partyId > 0 ? partyId : null;
  const key = ['party-electronic-addresses', validPartyId] as const;
  const query = useQuery({ queryKey: key, queryFn: ({ signal }) => partyLogisticsApi.contacts(validPartyId!, signal), enabled: validPartyId !== null });
  const rows = query.data ?? [];
  const [selectedIds, setSelectedIds] = useState<(string | number)[]>([]);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const selected = rows.find((row) => selectedIds.includes(row.id)) ?? null;
  const columns = useMemo<ColumnDef<StoredElectronicAddress>[]>(() => [
    { field: 'description', headerName: t('fields.description'), minWidth: 170, flex: 1 },
    { field: 'type', headerName: t('legalEntities.contacts.type'), width: 120 },
    { field: 'number', headerName: t('legalEntities.contacts.contact'), minWidth: 220, flex: 1 },
    { field: 'extension', headerName: t('fields.extension'), width: 100 },
    { field: 'primary', headerName: t('legalEntities.addresses.primary'), width: 90, renderCell: ({ value }) => value ? <Chip size="small" label={t('common.yes')} color="primary" variant="outlined" /> : t('common.no') },
  ], [t]);
  const run = async (action: () => Promise<void>) => {
    setError(null);
    try { await action(); await client.invalidateQueries({ queryKey: key }); setDrawerOpen(false); }
    catch (reason) { setError(reason instanceof Error ? reason.message : String(reason)); }
  };
  return <>
    {(error || query.error) && <Alert severity="error" sx={{ m: 1 }}>{error ?? (query.error instanceof Error ? query.error.message : String(query.error))}</Alert>}
    <TabularDetailPanel rows={rows} columns={columns} addLabel={t('actions.add')} removeLabel={t('actions.remove')} selectedIds={selectedIds} onSelectionChange={setSelectedIds} disabled={!editing || validPartyId === null || query.isLoading} onAdd={() => { if (validPartyId === null) return; setSelectedIds([]); setDrawerOpen(true); }} onRemove={() => selected && validPartyId !== null && void run(() => partyLogisticsApi.deleteContact(validPartyId, selected.location))} actions={[{ id: 'edit', label: t('legalEntities.actions.advanced'), icon: <EditOutlinedIcon />, disabled: !selected || validPartyId === null, onClick: () => setDrawerOpen(true) }]} storageKey={storageKey} height={180} />
    <LogisticsElectronicAddressDrawer open={drawerOpen && validPartyId !== null} onClose={() => setDrawerOpen(false)} onSave={(draft) => validPartyId !== null && void run(() => partyLogisticsApi.saveContact(validPartyId, selected ? { ...draft, id: selected.id, location: selected.location } : draft, Boolean(selected)))} initialData={selected} />
  </>;
}
