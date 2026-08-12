import React from 'react';
import {
    Box, Button, Checkbox, FormControl, FormControlLabel, InputLabel,
    MenuItem, Select, Stack, TextField, Typography,
} from '@mui/material';
import { CloudUpload, Draw, Search as SearchIcon } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import type { RequestControl } from '../../types';

export const ControlPreview: React.FC<{ control: RequestControl }> = ({ control: c }) => {
    const { t } = useTranslation();
    if (!c.visible) return null;
    const common = { disabled: c.readOnly, required: c.required, fullWidth: true, size: 'small' as const };
    const items = (c.optionsX ?? []).map((o) => o.en || o.value || o.ar);
    switch (c.type) {
        case 'textbox':
        case 'text':
            return <TextField {...common} label={c.label} defaultValue={c.defaultValue} />;
        case 'digits':
            return <TextField {...common} type="number" label={c.label} defaultValue={c.defaultValue} />;
        case 'longtext':
            return <TextField {...common} multiline rows={3} label={c.label} defaultValue={c.defaultValue} />;
        case 'url':
            return <TextField {...common} type="url" label={c.label} defaultValue={c.defaultValue} />;
        case 'time':
            return <TextField {...common} type="time" label={c.label} InputLabelProps={{ shrink: true }} />;
        case 'date':
        case 'calendar':
            return <TextField {...common} type="date" label={c.label} InputLabelProps={{ shrink: true }} />;
        case 'dropdown':
        case 'dropdown-db':
        case 'dropdown-manual':
        case 'dropdownlist':
        case 'combobox':
            return (
                <FormControl {...common}>
                    <InputLabel>{c.label}</InputLabel>
                    <Select label={c.label} defaultValue="">
                        {(items.length ? items : (c.options ?? [])).map((o) =>
                            <MenuItem key={o} value={o}>{o}</MenuItem>)}
                    </Select>
                </FormControl>
            );
        case 'checkbox':
            return <FormControlLabel control={<Checkbox disabled={c.readOnly} />} label={c.label} />;
        case 'checkboxlist':
        case 'radiobuttonlist':
            return (
                <Box>
                    <Typography variant="caption" sx={{ fontWeight: 700 }}>{c.label}</Typography>
                    <Stack>{items.map((o) =>
                        <FormControlLabel key={o} control={<Checkbox size="small" />} label={o} />
                    )}</Stack>
                </Box>
            );
        case 'label':
            return <Typography variant="body2" sx={{ fontWeight: 700 }}>{c.label}</Typography>;
        case 'file':
            return (
                <Button variant="outlined" component="label" startIcon={<CloudUpload />} disabled={c.readOnly}>
                    {c.label || t('common.upload_file', 'Upload file')}<input hidden type="file" />
                </Button>
            );
        case 'signature':
            return (
                <Box sx={{ border: '1px dashed', borderColor: 'divider', p: 2, textAlign: 'center', borderRadius: 1 }}>
                    <Draw /><Typography variant="caption" display="block">{c.label || t('common.sign_here', 'Sign here')}</Typography>
                </Box>
            );
        case 'lookup':
        case 'showroom':
        case 'employeesearch':
        case 'employeeid':
        case 'location':
        case 'advertiser':
            return <TextField {...common} label={c.label} placeholder={`${t('common.search', 'Search')} ${c.type}…`}
                InputProps={{ endAdornment: <SearchIcon fontSize="small" /> }} />;
        case 'table':
        case 'grid': {
            const rows = c.gridRows ?? [];
            const colCount = rows.reduce((m, r) => Math.max(m, r.length), 1);
            return (
                <Box sx={{ border: '1px solid', borderColor: 'divider', borderRadius: 1, p: 1 }}>
                    <Typography variant="caption" sx={{ fontWeight: 700 }}>{c.label || t('workflow.table', 'Table')}</Typography>
                    <Box component="table" sx={{ width: '100%', mt: 0.5, fontSize: 12, borderCollapse: 'collapse' }}>
                        <tbody>{rows.map((r, ri) => (
                            <tr key={ri}>{Array.from({ length: colCount }, (_, ci) => (
                                <td key={ci} style={{ border: '1px solid #ddd', padding: 4 }}>{r[ci] ?? ''}</td>
                            ))}</tr>
                        ))}</tbody>
                    </Box>
                </Box>
            );
        }
        case 'color':
            return (
                <Stack direction="row" spacing={1} alignItems="center">
                    <Box sx={{
                        width: 24, height: 24, borderRadius: 0.5, border: '1px solid #ccc',
                        bgcolor: c.color ?? '#000',
                    }} />
                    <Typography variant="caption">{c.label} · {c.color}</Typography>
                </Stack>
            );
    }
    return null;
};
