import React from 'react';
import {
    Box, Button, FormControl, IconButton, MenuItem, Select, Stack,
    TextField, ToggleButton, ToggleButtonGroup,
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { Add, Delete } from '@mui/icons-material';
import type { ConditionGroup, Operator } from '../../types';
import { uid } from '../../utils/uid';

interface Props {
    group: ConditionGroup | undefined;
    onChange: (g: ConditionGroup | undefined) => void;
    fields: string[];
}

const OPERATORS: Operator[] = ['=', '!=', '>', '<', '>=', '<=', 'contains', 'isEmpty'];

export const ConditionBuilder: React.FC<Props> = ({ group, onChange, fields }) => {
    const { t } = useTranslation();
    const g = group ?? { logic: 'AND' as const, expressions: [] };
    const update = (patch: Partial<ConditionGroup>) => onChange({ ...g, ...patch });
    const addExpr = () => update({
        expressions: [...g.expressions, { id: uid(), field: fields[0] ?? '', operator: '=', value: '' }],
    });
    const updExpr = (id: string, patch: Partial<typeof g.expressions[number]>) =>
        update({ expressions: g.expressions.map((e) => (e.id === id ? { ...e, ...patch } : e)) });
    const delExpr = (id: string) =>
        update({ expressions: g.expressions.filter((e) => e.id !== id) });

    return (
        <Box>
            <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
                <ToggleButtonGroup
                    value={g.logic} exclusive size="small"
                    onChange={(_, v) => v && update({ logic: v })}
                >
                    <ToggleButton value="AND">AND</ToggleButton>
                    <ToggleButton value="OR">OR</ToggleButton>
                </ToggleButtonGroup>
                <Box sx={{ flexGrow: 1 }} />
                <Button size="small" startIcon={<Add />} onClick={addExpr}>{t('workflow.expression', 'Expression')}</Button>
                {group && <Button size="small" color="error" onClick={() => onChange(undefined)}>{t('common.clear', 'Clear')}</Button>}
            </Stack>
            <Stack spacing={1}>
                {g.expressions.map((e) => (
                    <Stack key={e.id} direction="row" spacing={0.5}>
                        <FormControl size="small" sx={{ minWidth: 100, flex: 2 }}>
                            <Select value={e.field} onChange={(ev) => updExpr(e.id, { field: ev.target.value })}>
                                {fields.map((n) => <MenuItem key={n} value={n}>{n}</MenuItem>)}
                            </Select>
                        </FormControl>
                        <FormControl size="small" sx={{ minWidth: 70 }}>
                            <Select value={e.operator} onChange={(ev) => updExpr(e.id, { operator: ev.target.value as Operator })}>
                                {OPERATORS.map((o) => <MenuItem key={o} value={o}>{o}</MenuItem>)}
                            </Select>
                        </FormControl>
                        <TextField size="small" sx={{ flex: 2 }}
                            value={e.value} onChange={(ev) => updExpr(e.id, { value: ev.target.value })} />
                        <IconButton size="small" color="error" onClick={() => delExpr(e.id)}>
                            <Delete fontSize="small" />
                        </IconButton>
                    </Stack>
                ))}
            </Stack>
        </Box>
    );
};
