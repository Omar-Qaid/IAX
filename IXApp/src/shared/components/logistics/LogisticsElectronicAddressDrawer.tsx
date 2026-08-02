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
  IconButton,
  Divider,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { useTranslation } from 'react-i18next';
import type { LogisticsElectronicAddress, ElectronicAddressType } from '@shared/types/logistics';

export interface LogisticsElectronicAddressDrawerProps {
  open: boolean;
  onClose: () => void;
  onSave: (address: LogisticsElectronicAddress) => void;
  initialData?: LogisticsElectronicAddress | null;
  addressTypes?: ElectronicAddressType[];
}

const DEFAULT_ADDRESS_TYPES: ElectronicAddressType[] = [
  'Phone',
  'Email',
  'URL',
  'Telex',
  'Fax',
  'InstantMessage',
];

export function LogisticsElectronicAddressDrawer({
  open,
  onClose,
  onSave,
  initialData,
  addressTypes = DEFAULT_ADDRESS_TYPES,
}: LogisticsElectronicAddressDrawerProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === 'ar';

  const [formData, setFormData] = useState<LogisticsElectronicAddress>(() => ({
    id: initialData?.id ?? null,
    locationId: initialData?.locationId || '',
    description: initialData?.description || '',
    type: initialData?.type || 'Phone',
    number: initialData?.number || '',
    extension: initialData?.extension || '',
    roles: initialData?.roles || ['Business'],
    primary: initialData?.primary ?? false,
  }));

  const [errors, setErrors] = useState<{ description?: boolean; number?: boolean }>({});

  useEffect(() => {
    if (open) {
      setFormData({
        id: initialData?.id ?? null,
        locationId: initialData?.locationId || '',
        description: initialData?.description || '',
        type: initialData?.type || 'Phone',
        number: initialData?.number || '',
        extension: initialData?.extension || '',
        roles: initialData?.roles || ['Business'],
        primary: initialData?.primary ?? false,
      });
      setErrors({});
    }
  }, [open, initialData]);

  const tText = (key: string, fallback: string) => {
    const val = t(key);
    return !val || val === key ? fallback : val;
  };

  const handleSave = () => {
    const newErrors = {
      description: !formData.description.trim(),
      number: !formData.number.trim(),
    };
    setErrors(newErrors);

    if (newErrors.description || newErrors.number) {
      return;
    }

    onSave(formData);
    onClose();
  };

  return (
    <Drawer
      anchor={isRtl ? 'left' : 'right'}
      open={open}
      onClose={onClose}
      slotProps={{
        paper: {
          sx: {
            width: { xs: '100%', sm: 400 },
            display: 'flex',
            flexDirection: 'column',
          },
        },
      }}
    >
      <Box
        sx={{
          p: 2.5,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          borderBottom: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          {tText('logistics.contactInformation', 'Contact information')}
        </Typography>
        <IconButton size="small" onClick={onClose} aria-label={tText('common.close', 'Close')}>
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
          label={tText('logistics.description', 'Description')}
          required
          size="small"
          fullWidth
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          error={errors.description}
          helperText={errors.description ? tText('common.requiredField', 'Description is required') : undefined}
        />

        <TextField
          select
          label={tText('logistics.type', 'Type')}
          required
          size="small"
          fullWidth
          value={formData.type}
          onChange={(e) => setFormData({ ...formData, type: e.target.value as ElectronicAddressType })}
        >
          {addressTypes.map((type) => (
            <MenuItem key={type} value={type}>
              {tText(`logistics.types.${type}`, type)}
            </MenuItem>
          ))}
        </TextField>

        <TextField
          label={tText('logistics.contactNumber', 'Contact number/address')}
          required
          size="small"
          fullWidth
          value={formData.number}
          onChange={(e) => setFormData({ ...formData, number: e.target.value })}
          error={errors.number}
          helperText={errors.number ? tText('common.requiredField', 'Contact number/address is required') : undefined}
        />

        <TextField
          label={tText('logistics.extension', 'Extension')}
          size="small"
          fullWidth
          value={formData.extension}
          onChange={(e) => setFormData({ ...formData, extension: e.target.value })}
        />

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
          label={<Typography variant="body2">{tText('logistics.primary', 'Primary')}</Typography>}
        />
      </Box>

      <Box
        sx={{
          p: 2,
          borderTop: '1px solid',
          borderColor: 'divider',
          display: 'flex',
          gap: 1,
          justifyContent: 'flex-end',
          bgcolor: 'grey.50',
        }}
      >
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
