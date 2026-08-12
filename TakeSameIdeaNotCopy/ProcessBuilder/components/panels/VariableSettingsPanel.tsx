import React from 'react';
import {
    Stack, Typography, Chip, FormControlLabel, Switch,
    FormControl, InputLabel, Select, MenuItem, Button
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useParams } from 'react-router-dom';
import { DynamicField } from '../../../../../../components/common/DynamicField';
import { BilingualField } from '../../../../../../components/common/BilingualField';
import { GeneratableTextField } from '../../../../../../components/common/GeneratableTextField';
import { AccountTree } from '@mui/icons-material';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { useProcessBuilderData } from '../../hooks/useProcessBuilderData';
import { inferDataType } from '../../utils/ProcessBuilderMappers';

export const VariableSettingsPanel: React.FC = React.memo(() => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';
    const { id: routeId } = useParams<{ id: string }>();
    const isEditMode = !!routeId && routeId !== 'new';

    const {
        variables, updateVariable, selectedNode, processInfo
    } = useProcessBuilderContext();

    const {
        dataTypes, variablesSaving, saveVariablesToBackend
    } = useProcessBuilderData(processInfo.id, isEditMode, true);

    if (selectedNode.kind !== 'variable') return null;
    const variable = variables.find(v => v.id === selectedNode.id);
    if (!variable) return null;

    return (
        <Stack spacing={2} sx={{ p: 2 }}>
            <Stack direction="row" alignItems="center" spacing={1}>
                <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>{t('workflow.variable', 'Variable')}</Typography>
                {variable.serverId
                    ? <Chip size="small" color="success" label={`ID: ${variable.serverId}`} sx={{ borderRadius: 1.5, fontWeight: 600 }} />
                    : <Chip size="small" label={t('common.new', 'New')} sx={{ borderRadius: 1.5, fontWeight: 600 }} />}
                {variable.dirty && <Chip size="small" color="warning" label={t('common.unsaved', 'unsaved')} sx={{ borderRadius: 1.5, fontWeight: 600 }} />}
            </Stack>

            {/* Code field with Sync button */}
            <GeneratableTextField
                label={t('common.code')}
                value={variable.code ?? ''}
                onChange={(val) => updateVariable(variable.id, { code: val })}
                sequenceType="WfVariable"
                required
                error={!variable.code}
                helperText={!variable.code ? t('common.required', 'Required') : undefined}
                disabled={!!variable.serverId}
            />

            <BilingualField
                labelEN={t('common.name')}
                labelAR={t('common.name_ar')}
                nameValue={variable.name}
                nameARValue={variable.nameAR}
                onChange={(name, nameAR) => updateVariable(variable.id, { name, nameAR })}
                required
                error={!variable.name || !variable.nameAR}
                errorMessage={t('common.required', 'Required')}
            />

            <BilingualField
                labelEN={t('common.description', 'Description')}
                labelAR={t('common.description_ar', 'Description (AR)')}
                nameValue={variable.description}
                nameARValue={variable.descriptionAR}
                onChange={(description, descriptionAR) => updateVariable(variable.id, { description, descriptionAR })}
                multiline
                rows={3}
            />

            <FormControl size="small" sx={{ '& .MuiOutlinedInput-root': { borderRadius: 1.5 } }}>
                <InputLabel>{t('workflow.data_type', 'Data Type')} *</InputLabel>
                <Select label={t('workflow.data_type', 'Data Type') + ' *'} value={variable.dataTypeId}
                    onChange={(e) => {
                        const id = e.target.value as number | '';
                        updateVariable(variable.id, { dataTypeId: id, dataType: inferDataType(id, dataTypes) as any });
                    }}>
                    <MenuItem value=""><em>({t('common.none', 'none')})</em></MenuItem>
                    {dataTypes.map((d) =>
                        <MenuItem key={d.id} value={d.id}>{isRtl ? d.nameAR || d.name : d.name} ({d.code})</MenuItem>)}
                </Select>
            </FormControl>

            <DynamicField config={{ name: 'sortOrder', label: t('common.sort_order', 'Sort Order'), type: 'number' }} value={variable.sortOrder}
                onChange={(val) => updateVariable(variable.id, { sortOrder: Number(val) || 0 })} />

            <Stack direction="row" spacing={2}>
                <FormControlLabel control={
                    <Switch checked={variable.isActive}
                        onChange={(e) => updateVariable(variable.id, { isActive: e.target.checked })} />
                } label={t('common.active', 'Active')} />
                <FormControlLabel control={
                    <Switch checked={!!variable.required}
                        onChange={(e) => updateVariable(variable.id, { required: e.target.checked })} />
                } label={t('common.required', 'Required')} />
            </Stack>

            <Button variant="contained" size="medium" startIcon={<AccountTree />}
                disabled={!processInfo.id || variablesSaving}
                onClick={saveVariablesToBackend}
                sx={{ borderRadius: 1.5, py: 1, textTransform: 'none', fontWeight: 600 }}>
                {variablesSaving ? t('common.saving', 'Saving…') : t('workflow.save_variables', 'Save Variables')}
            </Button>
        </Stack>
    );
});

VariableSettingsPanel.displayName = 'VariableSettingsPanel';
