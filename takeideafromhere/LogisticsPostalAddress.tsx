import React, { useState } from 'react';
import {
    Box,
    Typography,
    Button,
    TextField,
    Select,
    MenuItem,
    Switch,
    FormControlLabel,
    Drawer,
    IconButton,
    styled
} from '@mui/material';
import { Close as CloseIcon, ExpandMore as ExpandMoreIcon } from '@mui/icons-material';
import { useCountryRegions, useStates, useCities, useCounties } from './useLogisticsAddress';

const DrawerContainer = styled(Box)(({ theme }) => ({
    width: 400,
    height: '100%',
    display: 'flex',
    flexDirection: 'column',
    backgroundColor: '#ffffff',
    fontFamily: '"Segoe UI", "Inter", sans-serif',
}));

const Header = styled(Box)(({ theme }) => ({
    padding: '16px 24px',
    borderBottom: '1px solid #e1dfdd',
}));

const Content = styled(Box)(({ theme }) => ({
    flex: 1,
    overflowY: 'auto',
    padding: '16px 24px',
    display: 'flex',
    flexDirection: 'column',
    gap: '12px',
}));

const Footer = styled(Box)(({ theme }) => ({
    padding: '16px 24px',
    borderTop: '1px solid #e1dfdd',
    display: 'flex',
    gap: '8px',
}));

const FieldLabel = styled(Typography)(({ theme }) => ({
    fontSize: '12px',
    color: '#605E5C',
    marginBottom: '4px'
}));

const RequiredFieldLabel = styled(FieldLabel)(({ theme }) => ({
    '&::after': {
        content: '" *"',
        color: '#a4262c'
    }
}));

const StyledTextField = styled(TextField)({
    '& .MuiOutlinedInput-root': {
        height: '32px',
        fontSize: '13px',
        borderRadius: '2px',
        backgroundColor: '#ffffff',
        '&.Mui-focused fieldset': {
            borderColor: '#0F6CBD',
            borderWidth: '1px',
        }
    }
});

const StyledTextArea = styled(TextField)({
    '& .MuiOutlinedInput-root': {
        fontSize: '13px',
        borderRadius: '2px',
        backgroundColor: '#ffffff',
        padding: '8px',
        '&.Mui-focused fieldset': {
            borderColor: '#0F6CBD',
            borderWidth: '1px',
        }
    }
});

const StyledSelect = styled(Select)({
    height: '32px',
    fontSize: '13px',
    borderRadius: '2px',
    backgroundColor: '#ffffff',
    '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
        borderColor: '#0F6CBD',
        borderWidth: '1px',
    }
});

const ErrorTextField = styled(StyledTextField)({
    '& .MuiOutlinedInput-root': {
        '& fieldset': {
            borderColor: '#a4262c',
        },
        '&:hover fieldset': {
            borderColor: '#a4262c',
        }
    }
});

const ErrorSelect = styled(StyledSelect)({
    '& .MuiOutlinedInput-notchedOutline': {
        borderColor: '#a4262c',
    },
    '&:hover .MuiOutlinedInput-notchedOutline': {
        borderColor: '#a4262c',
    }
});

interface LogisticsPostalAddressProps {
    open: boolean;
    onClose: () => void;
    onSave: (data: any) => void;
    initialData?: any;
}

export function LogisticsPostalAddress({ open, onClose, onSave, initialData }: LogisticsPostalAddressProps) {
    const [formData, setFormData] = useState(() => {
        if (initialData) {
            return {
                id: initialData.id,
                locationId: initialData.locationId || '',
                description: initialData.description || initialData.name || '',
                roles: initialData.roles || ['Business'],
                validFrom: initialData.validFrom ? initialData.validFrom.split('T')[0] : new Date().toISOString().split('T')[0],
                validTo: initialData.validTo ? initialData.validTo.split('T')[0] : '2154-12-31',
                country: initialData.countryRegionId || '',
                state: initialData.state || '',
                city: initialData.city || '',
                district: initialData.district || '',
                street: initialData.street || '',
                building: initialData.building || '',
                zipCode: initialData.zipCode || '',
                buildingComplement: initialData.buildingComplement || '',
                postBox: initialData.postBox || '',
                county: initialData.county || '',
                primary: initialData.primary || false,
                primaryForCountry: initialData.primaryForCountry || false
            };
        }
        return {
            id: null,
            locationId: '',
            description: '',
            roles: ['Business'],
            validFrom: new Date().toISOString().split('T')[0],
            validTo: '2154-12-31',
            country: '',
            state: '',
            city: '',
            district: '',
            street: '',
            building: '',
            zipCode: '',
            buildingComplement: '',
            postBox: '',
            county: '',
            primary: true,
            primaryForCountry: true
        };
    });

    React.useEffect(() => {
        if (open) {
            if (initialData) {
                setFormData({
                    id: initialData.id,
                    locationId: initialData.locationId || '',
                    description: initialData.description || initialData.name || '',
                    roles: initialData.roles || ['Business'],
                    validFrom: initialData.validFrom ? initialData.validFrom.split('T')[0] : new Date().toISOString().split('T')[0],
                    validTo: initialData.validTo ? initialData.validTo.split('T')[0] : '2154-12-31',
                    country: initialData.countryRegionId || '',
                    state: initialData.state || '',
                    city: initialData.city || '',
                    district: initialData.district || '',
                    street: initialData.street || '',
                    building: initialData.building || '',
                    zipCode: initialData.zipCode || '',
                    buildingComplement: initialData.buildingComplement || '',
                    postBox: initialData.postBox || '',
                    county: initialData.county || '',
                    primary: initialData.primary || false,
                    primaryForCountry: initialData.primaryForCountry || false
                });
            } else {
                setFormData({
                    id: null,
                    locationId: '',
                    description: '',
                    roles: ['Business'],
                    validFrom: new Date().toISOString().split('T')[0],
                    validTo: '2154-12-31',
                    country: '',
                    state: '',
                    city: '',
                    district: '',
                    street: '',
                    building: '',
                    zipCode: '',
                    buildingComplement: '',
                    postBox: '',
                    county: '',
                    primary: true,
                    primaryForCountry: true
                });
            }
        }
    }, [open, initialData]);


    const { data: countries } = useCountryRegions();
    const { data: states } = useStates(formData.country);
    const { data: cities } = useCities(formData.state);
    const { data: counties } = useCounties(formData.state);

    const handleSave = () => {
        if (!formData.description || !formData.country) {
            // basic validation handled visually via Error components for demo
            return;
        }
        onSave(formData);
        onClose();
    };

    return (
        <Drawer anchor="right" open={open} onClose={onClose} PaperProps={{ sx: { width: 400 } }}>
            <DrawerContainer>
                <Header>
                    <Box display="flex" justifyContent="space-between" alignItems="flex-start">
                        <Box>
                            <Box display="flex" alignItems="center" sx={{ mb: 0.5, cursor: 'pointer' }}>
                                <Typography sx={{ fontSize: '12px', color: '#605E5C' }}>Standard view</Typography>
                                <ExpandMoreIcon sx={{ fontSize: 14, color: '#605E5C', ml: 0.5 }} />
                            </Box>
                            <Typography sx={{ fontSize: '20px', fontWeight: 600, color: '#323130' }}>
                                New address
                            </Typography>
                        </Box>
                    </Box>
                </Header>

                <Content>
                    <Box>
                        <FieldLabel>Location ID</FieldLabel>
                        <StyledTextField fullWidth value={formData.locationId || '(Auto-generated on save)'} InputProps={{ readOnly: true }} sx={{ backgroundColor: '#f3f2f1' }} />
                    </Box>

                    <Box>
                        <RequiredFieldLabel>Name or description</RequiredFieldLabel>
                        {!formData.description ? (
                            <ErrorTextField fullWidth value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} />
                        ) : (
                            <StyledTextField fullWidth value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} />
                        )}
                    </Box>


                    <Box display="flex" gap={2}>
                        <Box flex={1}>
                            <FieldLabel>Valid from</FieldLabel>
                            <StyledTextField 
                                type="date" 
                                fullWidth 
                                value={formData.validFrom} 
                                onChange={(e) => setFormData({ ...formData, validFrom: e.target.value })} 
                                InputLabelProps={{ shrink: true }}
                            />
                        </Box>
                        <Box flex={1}>
                            <FieldLabel>Valid to</FieldLabel>
                            <StyledTextField 
                                type="date" 
                                fullWidth 
                                value={formData.validTo} 
                                onChange={(e) => setFormData({ ...formData, validTo: e.target.value })} 
                                InputLabelProps={{ shrink: true }}
                            />
                        </Box>
                    </Box>

                    <Box>
                        <RequiredFieldLabel>Country/region</RequiredFieldLabel>
                        {!formData.country ? (
                            <ErrorSelect fullWidth value={formData.country} onChange={(e) => setFormData({ ...formData, country: e.target.value as string, state: '', city: '', county: '' })}>
                                {countries?.map(c => (
                                    <MenuItem key={c.countryRegionId} value={c.countryRegionId} sx={{ fontSize: '13px' }}>{c.countryRegionId}</MenuItem>
                                ))}
                            </ErrorSelect>
                        ) : (
                            <StyledSelect fullWidth value={formData.country} onChange={(e) => setFormData({ ...formData, country: e.target.value as string, state: '', city: '', county: '' })}>
                                {countries?.map(c => (
                                    <MenuItem key={c.countryRegionId} value={c.countryRegionId} sx={{ fontSize: '13px' }}>{c.countryRegionId}</MenuItem>
                                ))}
                            </StyledSelect>
                        )}
                    </Box>

                    <Box>
                        <FieldLabel>State</FieldLabel>
                        <StyledSelect fullWidth value={formData.state} onChange={(e) => setFormData({ ...formData, state: e.target.value as string, city: '', county: '' })} displayEmpty>
                            <MenuItem value="" sx={{ fontSize: '13px' }}></MenuItem>
                            {states?.map(s => (
                                <MenuItem key={s.stateId} value={s.stateId} sx={{ fontSize: '13px' }}>{s.stateId} - {s.name}</MenuItem>
                            ))}
                        </StyledSelect>
                    </Box>

                    <Box>
                        <FieldLabel>City</FieldLabel>
                        <StyledSelect fullWidth value={formData.city} onChange={(e) => setFormData({ ...formData, city: e.target.value as string })} displayEmpty>
                            <MenuItem value="" sx={{ fontSize: '13px' }}></MenuItem>
                            {cities?.map(c => (
                                <MenuItem key={c.cityKey} value={c.cityKey} sx={{ fontSize: '13px' }}>{c.name}</MenuItem>
                            ))}
                        </StyledSelect>
                    </Box>

                    <Box>
                        <FieldLabel>District</FieldLabel>
                        <StyledSelect fullWidth value={formData.district} onChange={(e) => setFormData({ ...formData, district: e.target.value as string })} displayEmpty>
                            <MenuItem value="" sx={{ fontSize: '13px' }}></MenuItem>
                        </StyledSelect>
                    </Box>

                    <Box>
                        <FieldLabel>Street</FieldLabel>
                        <StyledTextArea fullWidth multiline rows={4} value={formData.street} onChange={(e) => setFormData({ ...formData, street: e.target.value })} />
                    </Box>

                    <Box>
                        <FieldLabel>Building</FieldLabel>
                        <StyledTextField fullWidth value={formData.building} onChange={(e) => setFormData({ ...formData, building: e.target.value })} sx={{ width: '150px' }} />
                    </Box>

                    <Box>
                        <FieldLabel>ZIP/postal code</FieldLabel>
                        <StyledTextField fullWidth value={formData.zipCode} onChange={(e) => setFormData({ ...formData, zipCode: e.target.value })} sx={{ width: '150px' }} />
                    </Box>

                    <Box>
                        <FieldLabel>Building complement</FieldLabel>
                        <StyledTextField fullWidth value={formData.buildingComplement} onChange={(e) => setFormData({ ...formData, buildingComplement: e.target.value })} />
                    </Box>

                    <Box>
                        <FieldLabel>Post box</FieldLabel>
                        <StyledTextField fullWidth value={formData.postBox} onChange={(e) => setFormData({ ...formData, postBox: e.target.value })} sx={{ width: '200px' }} />
                    </Box>

                    <Box>
                        <FieldLabel>County</FieldLabel>
                        <StyledSelect fullWidth value={formData.county} onChange={(e) => setFormData({ ...formData, county: e.target.value as string })} displayEmpty>
                            <MenuItem value="" sx={{ fontSize: '13px' }}></MenuItem>
                            {counties?.map(c => (
                                <MenuItem key={c.countyId} value={c.countyId} sx={{ fontSize: '13px' }}>{c.name}</MenuItem>
                            ))}
                        </StyledSelect>
                    </Box>

                    <Box sx={{ mt: 1 }}>
                        <FieldLabel>Primary</FieldLabel>
                        <FormControlLabel
                            control={<Switch size="small" checked={formData.primary} onChange={(e) => setFormData({ ...formData, primary: e.target.checked })} sx={{ ml: 1 }} color="primary" />}
                            label={<Typography sx={{ fontSize: '13px', color: '#323130' }}>Yes</Typography>}
                        />
                    </Box>

                    <Box>
                        <FieldLabel>Primary for country/region</FieldLabel>
                        <FormControlLabel
                            control={<Switch size="small" checked={formData.primaryForCountry} onChange={(e) => setFormData({ ...formData, primaryForCountry: e.target.checked })} sx={{ ml: 1 }} color="primary" />}
                            label={<Typography sx={{ fontSize: '13px', color: '#323130' }}>Yes</Typography>}
                        />
                    </Box>
                </Content>

                <Footer>
                    <Button
                        variant="contained"
                        disableElevation
                        onClick={handleSave}
                        sx={{
                            backgroundColor: '#0F6CBD',
                            color: '#ffffff',
                            textTransform: 'none',
                            fontSize: '13px',
                            minWidth: '80px',
                            height: '32px',
                            borderRadius: '2px',
                            '&:hover': { backgroundColor: '#0c5da3' }
                        }}
                    >
                        OK
                    </Button>
                    <Button
                        variant="outlined"
                        onClick={onClose}
                        sx={{
                            borderColor: '#8a8886',
                            color: '#323130',
                            textTransform: 'none',
                            fontSize: '13px',
                            minWidth: '80px',
                            height: '32px',
                            borderRadius: '2px',
                            '&:hover': { borderColor: '#323130', backgroundColor: '#f3f2f1' }
                        }}
                    >
                        Cancel
                    </Button>
                </Footer>
            </DrawerContainer>
        </Drawer>
    );
}
