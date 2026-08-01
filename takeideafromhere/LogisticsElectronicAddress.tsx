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
    styled
} from '@mui/material';
import { ExpandMore as ExpandMoreIcon } from '@mui/icons-material';

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

interface LogisticsElectronicAddressProps {
    open: boolean;
    onClose: () => void;
    onSave: (data: any) => void;
    initialData?: any;
    addressTypes?: string[];
}

export function LogisticsElectronicAddress({ open, onClose, onSave, initialData, addressTypes = ['Phone', 'Email', 'URL', 'Telex', 'Fax', 'InstantMessage'] }: LogisticsElectronicAddressProps) {
    const [formData, setFormData] = useState(() => {
        if (initialData) {
            return {
                id: initialData.id,
                locationId: initialData.locationId || '',
                description: initialData.description || initialData.name || '',
                type: initialData.type || 'Phone',
                number: initialData.number || '',
                extension: initialData.extension || '',
                roles: initialData.roles || ['Business'],
                primary: initialData.primary || false
            };
        }
        return {
            id: null,
            locationId: '',
            description: '',
            type: 'Phone',
            number: '',
            extension: '',
            roles: ['Business'],
            primary: false
        };
    });

    React.useEffect(() => {
        if (open) {
            if (initialData) {
                setFormData({
                    id: initialData.id,
                    locationId: initialData.locationId || '',
                    description: initialData.description || initialData.name || '',
                    type: initialData.type || 'Phone',
                    number: initialData.number || '',
                    extension: initialData.extension || '',
                    roles: initialData.roles || ['Business'],
                    primary: initialData.primary || false
                });
            } else {
                setFormData({
                    id: null,
                    locationId: '',
                    description: '',
                    type: 'Phone',
                    number: '',
                    extension: '',
                    roles: ['Business'],
                    primary: false
                });
            }
        }
    }, [open, initialData]);

    const handleSave = () => {
        if (!formData.description || !formData.type || !formData.number) {
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
                                Contact information
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
                        <RequiredFieldLabel>Description</RequiredFieldLabel>
                        {!formData.description ? (
                            <ErrorTextField fullWidth value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} />
                        ) : (
                            <StyledTextField fullWidth value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} />
                        )}
                    </Box>

                    <Box>
                        <RequiredFieldLabel>Type</RequiredFieldLabel>
                        <StyledSelect 
                            fullWidth 
                            value={formData.type} 
                            onChange={(e) => setFormData({ ...formData, type: e.target.value as string })}
                        >
                            {addressTypes.map(t => (
                                <MenuItem key={t} value={t} sx={{ fontSize: '13px' }}>{t}</MenuItem>
                            ))}
                        </StyledSelect>
                    </Box>

                    <Box>
                        <RequiredFieldLabel>Contact number/address</RequiredFieldLabel>
                        {!formData.number ? (
                            <ErrorTextField fullWidth value={formData.number} onChange={(e) => setFormData({ ...formData, number: e.target.value })} />
                        ) : (
                            <StyledTextField fullWidth value={formData.number} onChange={(e) => setFormData({ ...formData, number: e.target.value })} />
                        )}
                    </Box>

                    <Box>
                        <FieldLabel>Extension</FieldLabel>
                        <StyledTextField fullWidth value={formData.extension} onChange={(e) => setFormData({ ...formData, extension: e.target.value })} />
                    </Box>

                    <Box sx={{ mt: 1 }}>
                        <FieldLabel>Primary</FieldLabel>
                        <FormControlLabel
                            control={<Switch size="small" checked={formData.primary} onChange={(e) => setFormData({ ...formData, primary: e.target.checked })} sx={{ ml: 1 }} color="primary" />}
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
