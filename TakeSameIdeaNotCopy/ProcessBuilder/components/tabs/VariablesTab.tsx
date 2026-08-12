import React, { useState } from 'react';
import {
    Box, Typography, Button, Paper, TextField, FormControl,
    InputLabel, Select, MenuItem, FormControlLabel, Switch, IconButton, Chip, Stack, FormHelperText
} from '@mui/material';
import { Add, PlaylistAddCheck, Delete } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { GeneratableTextField } from '../../../../../../components/common/GeneratableTextField';
import { notify } from '../../../../../../lib/notify';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import type { DataType } from '../../types';
import type { WfDataType } from '../../../../types';

interface VariablesTabProps {
    dataTypes: WfDataType[];
    variablesSaving: boolean;
    saveVariablesToBackend: () => void;
}

export const VariablesTab: React.FC<VariablesTabProps> = React.memo(({
    dataTypes,
    variablesSaving,
    saveVariablesToBackend,
}) => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';

    const {
        processInfo,
        variables,
        addVariable,
        updateVariable,
        deleteVariable,
        setSelectedNode: setSelected,
    } = useProcessBuilderContext();

    const [showErrors, setShowErrors] = useState(false);

    const handleSave = () => {
        setShowErrors(true);
        const anyInvalid = variables.some(v => !(isRtl ? v.nameAR : v.name)?.trim() || !v.dataTypeId);
        if (anyInvalid) return;
        const seenCodes = new Set<string>();
        for (const v of variables) {
            const trimmedCode = (v.code || '').trim();
            if (seenCodes.has(trimmedCode)) {
                notify.error(t('workflow.validation.duplicate_variable_code', `Duplicate variable code found: "${trimmedCode}".`));
                return;
            }
            seenCodes.add(trimmedCode);
        }
        saveVariablesToBackend();
    };

    return (
        <Box sx={{ p: 2, width: '100%', maxWidth: '100%', boxSizing: 'border-box', minWidth: 0 }}>
            <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2, flexWrap: 'wrap' }}>
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                    {t('workflow.process_variables', 'Process Variables')}
                </Typography>
                {variables.some((v) => v.dirty) && (
                    <Chip size="small" color="warning" label={t('common.unsaved', 'unsaved')} />
                )}
                <Button startIcon={<Add />} variant="outlined" size="small" onClick={addVariable}>
                    {t('workflow.add_variable', 'Add Variable')}
                </Button>
                <Button
                    variant="contained"
                    size="small"
                    startIcon={<PlaylistAddCheck />}
                    disabled={!processInfo.id || variablesSaving}
                    onClick={handleSave}
                >
                    {variablesSaving ? t('common.saving', 'Saving…') : t('workflow.save_variables', 'Save Variables')}
                </Button>
            </Stack>
            {!processInfo.id && (
                <Typography variant="caption" color="warning.main" sx={{ display: 'block', mb: 1 }}>
                    {t('workflow.save_process_first_variables', 'Save the Process first to enable variables (ProcessId required).')}
                </Typography>
            )}

            {variables.length === 0 ? (
                <Paper variant="outlined" sx={{ p: 4, textAlign: 'center' }}>
                    <Typography color="text.secondary">
                        {t('workflow.no_variables_yet', 'No variables yet. Click "Add Variable" to create one.')}
                    </Typography>
                </Paper>
            ) : (
                <Stack spacing={1.5}>
                    {variables.map((v) => (
                        <Paper
                            key={v.id}
                            variant="outlined"
                            sx={{ p: 1.5, cursor: 'pointer' }}
                            onClick={() => setSelected({ kind: 'variable', id: v.id })}
                            onFocusCapture={() => setSelected({ kind: 'variable', id: v.id })}
                        >
                            <Stack direction="row" spacing={1} alignItems="center" sx={{ mb: 1, flexWrap: 'wrap', gap: 1 }}>
                                <Box sx={{ width: 180 }} onClick={(e) => e.stopPropagation()}>
                                    <GeneratableTextField
                                        label={t('common.code', 'Code')}
                                        value={v.code}
                                        onChange={(val) => updateVariable(v.id, { code: val })}
                                        sequenceType="WfVariable"
                                    />
                                </Box>
                                {isRtl ? (
                                    <TextField
                                        size="small"
                                        label={`${t('common.name_ar', 'Name (AR)')} *`}
                                        value={v.nameAR}
                                        onClick={(e) => e.stopPropagation()}
                                        onChange={(e) => updateVariable(v.id, { nameAR: e.target.value, name: e.target.value })}
                                        error={showErrors && !v.nameAR?.trim()}
                                        helperText={showErrors && !v.nameAR?.trim() ? t('common.required', 'Required') : undefined}
                                        sx={{ flexGrow: 1 }}
                                        dir="rtl"
                                    />
                                ) : (
                                    <TextField
                                        size="small"
                                        label={`${t('common.name', 'Name')} *`}
                                        value={v.name}
                                        onClick={(e) => e.stopPropagation()}
                                        onChange={(e) => updateVariable(v.id, { name: e.target.value, nameAR: e.target.value })}
                                        error={showErrors && !v.name?.trim()}
                                        helperText={showErrors && !v.name?.trim() ? t('common.required', 'Required') : undefined}
                                        sx={{ flexGrow: 1 }}
                                    />
                                )}
                                <FormControl size="small" error={showErrors && !v.dataTypeId} sx={{ minWidth: 160 }} onClick={(e) => e.stopPropagation()}>
                                    <InputLabel>{`${t('workflow.data_type', 'Data Type')} *`}</InputLabel>
                                    <Select
                                        label={`${t('workflow.data_type', 'Data Type')} *`}
                                        value={v.dataTypeId === '' ? '' : v.dataTypeId}
                                        onChange={(e) => updateVariable(v.id, {
                                            dataTypeId: e.target.value as any,
                                            dataType: dataTypes.find(d => d.id === e.target.value)?.code as DataType ?? 'string',
                                        })}
                                    >
                                        <MenuItem value="">—</MenuItem>
                                        {dataTypes.map((dt) => (
                                            <MenuItem key={dt.id} value={dt.id}>
                                                {dt.name ?? dt.code}
                                            </MenuItem>
                                        ))}
                                    </Select>
                                    {showErrors && !v.dataTypeId && <FormHelperText>{t('common.required', 'Required')}</FormHelperText>}
                                </FormControl>
                                <FormControlLabel
                                    onClick={(e) => e.stopPropagation()}
                                    control={
                                        <Switch
                                            size="small"
                                            checked={v.isActive}
                                            onChange={(e) => updateVariable(v.id, { isActive: e.target.checked })}
                                        />
                                    }
                                    label={t('common.active', 'Active')}
                                />
                                <IconButton
                                    size="small"
                                    color="error"
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        deleteVariable(v.id);
                                    }}
                                >
                                    <Delete fontSize="small" />
                                </IconButton>
                            </Stack>
                            <Stack direction="row" spacing={1} onClick={(e) => e.stopPropagation()}>
                                {isRtl ? (
                                    <TextField
                                        size="small"
                                        label={t('common.description_ar', 'Description (AR)')}
                                        fullWidth
                                        multiline
                                        rows={3}
                                        value={v.descriptionAR}
                                        onChange={(e) => updateVariable(v.id, { descriptionAR: e.target.value, description: e.target.value })}
                                        dir="rtl"
                                    />
                                ) : (
                                    <TextField
                                        size="small"
                                        label={t('common.description', 'Description')}
                                        fullWidth
                                        multiline
                                        rows={3}
                                        value={v.description}
                                        onChange={(e) => updateVariable(v.id, { description: e.target.value, descriptionAR: e.target.value })}
                                    />
                                )}
                            </Stack>
                            {v.dirty && (
                                <Chip
                                    label={t('common.unsaved', 'unsaved')}
                                    size="small"
                                    color="warning"
                                    sx={{ mt: 1, height: 18, fontSize: 10 }}
                                />
                            )}
                        </Paper>
                    ))}
                </Stack>
            )}
        </Box>
    );
});

VariablesTab.displayName = 'VariablesTab';
export default VariablesTab;
