import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  TextField,
  MenuItem,
  Switch,
  FormControlLabel,
  Drawer,
  Divider,
  Stack,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import IconButton from '@mui/material/IconButton';
import { useTranslation } from 'react-i18next';
import { useCountryRegions, useStates, useCities, useCounties } from '@shared/hooks/useLogisticsAddress';
import type { LogisticsPostalAddress } from '@shared/types/logistics';

export interface LogisticsPostalAddressDrawerProps {
  open: boolean;
  onClose: () => void;
  onSave: (address: LogisticsPostalAddress) => void;
  initialData?: LogisticsPostalAddress | null;
}

export function LogisticsPostalAddressDrawer({
  open,
  onClose,
  onSave,
  initialData,
}: LogisticsPostalAddressDrawerProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === 'ar';

  const defaultValidFrom = new Date().toISOString().split('T')[0];
  const defaultValidTo = '2154-12-31';

  const [formData, setFormData] = useState<LogisticsPostalAddress>(() => ({
    id: initialData?.id ?? null,
    locationId: initialData?.locationId || '',
    description: initialData?.description || '',
    roles: initialData?.roles || ['Business'],
    validFrom: initialData?.validFrom ? initialData.validFrom.split('T')[0] : defaultValidFrom,
    validTo: initialData?.validTo ? initialData.validTo.split('T')[0] : defaultValidTo,
    countryRegionId: initialData?.countryRegionId || '',
    state: initialData?.state || '',
    city: initialData?.city || '',
    district: initialData?.district || '',
    street: initialData?.street || '',
    building: initialData?.building || '',
    zipCode: initialData?.zipCode || '',
    buildingComplement: initialData?.buildingComplement || '',
    postBox: initialData?.postBox || '',
    county: initialData?.county || '',
    primary: initialData?.primary ?? true,
    primaryForCountry: initialData?.primaryForCountry ?? true,
  }));

  const [errors, setErrors] = useState<{ description?: boolean; countryRegionId?: boolean }>({});

  useEffect(() => {
    if (open) {
      setFormData({
        id: initialData?.id ?? null,
        locationId: initialData?.locationId || '',
        description: initialData?.description || '',
        roles: initialData?.roles || ['Business'],
        validFrom: initialData?.validFrom ? initialData.validFrom.split('T')[0] : defaultValidFrom,
        validTo: initialData?.validTo ? initialData.validTo.split('T')[0] : defaultValidTo,
        countryRegionId: initialData?.countryRegionId || '',
        state: initialData?.state || '',
        city: initialData?.city || '',
        district: initialData?.district || '',
        street: initialData?.street || '',
        building: initialData?.building || '',
        zipCode: initialData?.zipCode || '',
        buildingComplement: initialData?.buildingComplement || '',
        postBox: initialData?.postBox || '',
        county: initialData?.county || '',
        primary: initialData?.primary ?? true,
        primaryForCountry: initialData?.primaryForCountry ?? true,
      });
      setErrors({});
    }
  }, [open, initialData]);

  const { data: countries = [] } = useCountryRegions();
  const { data: states = [] } = useStates(formData.countryRegionId);
  const { data: cities = [] } = useCities(formData.state);
  const { data: counties = [] } = useCounties(formData.state);

  const handleSave = () => {
    const newErrors = {
      description: !formData.description.trim(),
      countryRegionId: !formData.countryRegionId,
    };
    setErrors(newErrors);

    if (newErrors.description || newErrors.countryRegionId) {
      return;
    }

    onSave(formData);
    onClose();
  };

  const tText = (key: string, fallback: string) => {
    const val = t(key);
    return !val || val === key ? fallback : val;
  };

  return (
    <Drawer
      anchor={isRtl ? 'left' : 'right'}
      open={open}
      onClose={onClose}
      slotProps={{
        paper: {
          sx: {
            width: { xs: '100%', sm: 420 },
            display: 'flex',
            flexDirection: 'column',
          },
        },
      }}
    >
      <Box sx={{ p: 2.5, display: 'flex', alignItems: 'center', justifyContent: 'space-between', borderBottom: '1px solid', borderColor: 'divider' }}>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          {initialData ? tText('logistics.editAddress', 'Edit address') : tText('logistics.newAddress', 'New address')}
        </Typography>
        <IconButton size="small" onClick={onClose} aria-label={t('common.close') || 'Close'}>
          <CloseIcon fontSize="small" />
        </IconButton>
      </Box>

      <Box sx={{ flex: 1, overflowY: 'auto', p: 2.5, display: 'flex', flexDirection: 'column', gap: 2 }}>
        <TextField
          label={tText('logistics.locationId', 'Location ID')}
          value={formData.locationId || '(Auto-generated on save)'}
          size="small"
          fullWidth
          slotProps={{ input: { readOnly: true } }}
          sx={{ bgcolor: 'action.hover' }}
        />

        <TextField
          label={tText('logistics.description', 'Name or description')}
          required
          size="small"
          fullWidth
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          error={errors.description}
          helperText={errors.description ? tText('common.requiredField', 'Description is required') : undefined}
        />

        <Stack direction="row" spacing={2}>
          <TextField
            label={tText('logistics.validFrom', 'Valid from')}
            type="date"
            size="small"
            fullWidth
            value={formData.validFrom}
            onChange={(e) => setFormData({ ...formData, validFrom: e.target.value })}
            slotProps={{ inputLabel: { shrink: true } }}
          />
          <TextField
            label={tText('logistics.validTo', 'Valid to')}
            type="date"
            size="small"
            fullWidth
            value={formData.validTo}
            onChange={(e) => setFormData({ ...formData, validTo: e.target.value })}
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </Stack>

        <TextField
          select
          label={tText('logistics.countryRegion', 'Country/region')}
          required
          size="small"
          fullWidth
          value={formData.countryRegionId}
          onChange={(e) =>
            setFormData({
              ...formData,
              countryRegionId: e.target.value,
              state: '',
              city: '',
              county: '',
            })
          }
          error={errors.countryRegionId}
          helperText={errors.countryRegionId ? tText('common.requiredField', 'Country is required') : undefined}
        >
          {countries.map((c) => (
            <MenuItem key={c.countryRegionId} value={c.countryRegionId}>
              {c.name ? `${c.countryRegionId} - ${c.name}` : c.countryRegionId}
            </MenuItem>
          ))}
        </TextField>

        <TextField
          select
          label={t('logistics.state') || 'State'}
          size="small"
          fullWidth
          disabled={!formData.countryRegionId}
          value={formData.state}
          onChange={(e) =>
            setFormData({
              ...formData,
              state: e.target.value,
              city: '',
              county: '',
            })
          }
        >
          <MenuItem value="">
            <em>{t('common.none') || 'None'}</em>
          </MenuItem>
          {states.map((s) => (
            <MenuItem key={s.stateId} value={s.stateId}>
              {s.stateId} - {s.name}
            </MenuItem>
          ))}
        </TextField>

        <TextField
          select
          label={t('logistics.city') || 'City'}
          size="small"
          fullWidth
          disabled={!formData.state}
          value={formData.city}
          onChange={(e) => setFormData({ ...formData, city: e.target.value })}
        >
          <MenuItem value="">
            <em>{t('common.none') || 'None'}</em>
          </MenuItem>
          {cities.map((c) => (
            <MenuItem key={c.cityKey} value={c.cityKey}>
              {c.name}
            </MenuItem>
          ))}
        </TextField>

        <TextField
          select
          label={t('logistics.county') || 'County'}
          size="small"
          fullWidth
          disabled={!formData.state}
          value={formData.county}
          onChange={(e) => setFormData({ ...formData, county: e.target.value })}
        >
          <MenuItem value="">
            <em>{t('common.none') || 'None'}</em>
          </MenuItem>
          {counties.map((c) => (
            <MenuItem key={c.countyId} value={c.countyId}>
              {c.name}
            </MenuItem>
          ))}
        </TextField>

        <TextField
          label={t('logistics.street') || 'Street'}
          multiline
          rows={3}
          size="small"
          fullWidth
          value={formData.street}
          onChange={(e) => setFormData({ ...formData, street: e.target.value })}
        />

        <Stack direction="row" spacing={2}>
          <TextField
            label={t('logistics.building') || 'Building'}
            size="small"
            fullWidth
            value={formData.building}
            onChange={(e) => setFormData({ ...formData, building: e.target.value })}
          />
          <TextField
            label={t('logistics.zipCode') || 'ZIP/postal code'}
            size="small"
            fullWidth
            value={formData.zipCode}
            onChange={(e) => setFormData({ ...formData, zipCode: e.target.value })}
          />
        </Stack>

        <Stack direction="row" spacing={2}>
          <TextField
            label={t('logistics.buildingComplement') || 'Building complement'}
            size="small"
            fullWidth
            value={formData.buildingComplement}
            onChange={(e) => setFormData({ ...formData, buildingComplement: e.target.value })}
          />
          <TextField
            label={t('logistics.postBox') || 'Post box'}
            size="small"
            fullWidth
            value={formData.postBox}
            onChange={(e) => setFormData({ ...formData, postBox: e.target.value })}
          />
        </Stack>

        <Divider sx={{ my: 1 }} />

        <FormControlLabel
          control={
            <Switch
              size="small"
              checked={formData.primary}
              onChange={(e) => setFormData({ ...formData, primary: e.target.checked })}
              color="primary"
            />
          }
          label={<Typography variant="body2">{t('logistics.primary') || 'Primary'}</Typography>}
        />

        <FormControlLabel
          control={
            <Switch
              size="small"
              checked={formData.primaryForCountry}
              onChange={(e) => setFormData({ ...formData, primaryForCountry: e.target.checked })}
              color="primary"
            />
          }
          label={<Typography variant="body2">{t('logistics.primaryForCountry') || 'Primary for country/region'}</Typography>}
        />
      </Box>

      <Box sx={{ p: 2, borderTop: '1px solid', borderColor: 'divider', display: 'flex', gap: 1, justifyContent: 'flex-end', bgcolor: 'grey.50' }}>
        <Button variant="outlined" size="small" onClick={onClose}>
          {tText('common.cancel', 'Cancel')}
        </Button>
        <Button variant="contained" size="small" onClick={handleSave}>
          {tText('common.ok', 'OK')}
        </Button>
      </Box>
    </Drawer>
  );
}
