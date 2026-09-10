import { Box, Button, MenuItem, Stack, TextField, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { BuilderCondition, BuilderTransition, BuilderVariable } from '../types/processBuilderTypes';
import { normalizeTransitionValue, TransitionValueField } from './TransitionValueField';
import { processBuilderTokens as tokens } from './processBuilderTokens';

export function TransitionRuleGroup({ transition, variables, onChange }: {
  transition: BuilderTransition;
  variables: BuilderVariable[];
  onChange: (patch: Partial<BuilderTransition>) => void;
}) {
  const { t } = useAppTranslation();
  const conditions = transition.additionalConditions ?? [];
  const operatorKeys: Record<string, string> = { '=': 'equals', '!=': 'notEquals', '>': 'greater', '<': 'less', '>=': 'atLeast', '<=': 'atMost', contains: 'contains', isEmpty: 'empty' };
  const operatorLabel = (operator: string) => t('wfProcessBuilder.settings.ruleGroup.operators.' + (operatorKeys[operator] ?? operator));
  const update = (index: number, patch: Partial<BuilderCondition>) => onChange({
    additionalConditions: conditions.map((condition, position) => position === index ? { ...condition, ...patch } : condition),
  });
  return (
    <Box sx={{ border: `1px solid ${tokens.border}`, p: 1, gridColumn: '1 / -1' }}>
      <Stack spacing="8px">
        <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 700 }}>{t('wfProcessBuilder.settings.ruleGroup.title')}</Typography>
        <Typography sx={{ fontSize: tokens.fontSize.caption, color: tokens.textMuted }}>{t('wfProcessBuilder.settings.ruleGroup.help')}</Typography>
        <Box sx={{ bgcolor: 'action.hover', p: 1 }}>
          <Typography sx={{ fontSize: tokens.fontSize.caption, fontWeight: 600 }}>{t('wfProcessBuilder.settings.ruleGroup.preview')}</Typography>
          {[transition, ...conditions].map((condition, index) => <Typography key={index} sx={{ fontSize: tokens.fontSize.body, overflowWrap: 'anywhere' }}>
            {index > 0 && (transition.conditionCombinator === 'OR' ? t('wfProcessBuilder.settings.ruleGroup.orWord') : t('wfProcessBuilder.settings.ruleGroup.andWord'))}{' '}
            {variables.find((variable) => variable.id === condition.variableId)?.name || t('wfProcessBuilder.settings.ruleGroup.selectField')}{' '}
            {operatorLabel(condition.operator)}{' '}
            {condition.operator !== 'isEmpty' && (condition.value === 'true' ? t('wfProcessBuilder.settings.yes') : condition.value === 'false' ? t('wfProcessBuilder.settings.no') : condition.value || '...')}
          </Typography>)}
        </Box>
        {conditions.length > 0 && <TextField select fullWidth size="small" label={t('wfProcessBuilder.settings.ruleGroup.combine')}
          value={transition.conditionCombinator ?? 'AND'} onChange={(event) => onChange({ conditionCombinator: event.target.value as 'AND' | 'OR' })}>
          <MenuItem value="AND">{t('wfProcessBuilder.settings.ruleGroup.and')}</MenuItem>
          <MenuItem value="OR">{t('wfProcessBuilder.settings.ruleGroup.or')}</MenuItem>
        </TextField>}
        {conditions.map((condition, index) => (
          <Stack key={index} spacing="6px" sx={{ border: `1px solid ${tokens.border}`, p: 0.75 }}>
            <Typography sx={{ fontSize: tokens.fontSize.caption, fontWeight: 600 }}>{t('wfProcessBuilder.settings.ruleGroup.condition', { number: index + 2 })}</Typography>
            <TextField select fullWidth size="small" label={t('wfProcessBuilder.settings.ruleGroup.field')} value={condition.variableId}
              onChange={(event) => update(index, { variableId: event.target.value, value: normalizeTransitionValue(condition.value, variables.find((variable) => variable.id === event.target.value)?.dataType) })}>
              <MenuItem value="">{t('wfProcessBuilder.settings.ruleGroup.selectField')}</MenuItem>
              {condition.variableId && !variables.some((variable) => variable.id === condition.variableId) && <MenuItem value={condition.variableId}>{t('wfProcessBuilder.settings.ruleGroup.missingVariable')}</MenuItem>}
              {variables.map((variable) => <MenuItem key={variable.id} value={variable.id}>{variable.name}</MenuItem>)}
            </TextField>
            <TextField select fullWidth size="small" label={t('wfProcessBuilder.settings.ruleGroup.check')} value={condition.operator}
              onChange={(event) => update(index, { operator: event.target.value as BuilderCondition['operator'] })}>
              {['=', '!=', '>', '<', '>=', '<=', 'contains', 'isEmpty'].map((operator) => <MenuItem key={operator} value={operator}>{operatorLabel(operator)}</MenuItem>)}
            </TextField>
            {condition.operator !== 'isEmpty' && <TransitionValueField dataType={variables.find((variable) => variable.id === condition.variableId)?.dataType}
              value={condition.value} onChange={(value) => update(index, { value })} />}
            <Button size="small" color="error" onClick={() => onChange({ additionalConditions: conditions.filter((_, position) => position !== index) })}>{t('common.remove')}</Button>
          </Stack>
        ))}
        <Button size="small" variant="outlined" onClick={() => onChange({ additionalConditions: [...conditions, { variableId: '', operator: '=', value: '' }] })}>{t('wfProcessBuilder.settings.ruleGroup.add')}</Button>
        {conditions.length > 0 && <Typography sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.caption }}>{t('wfProcessBuilder.settings.ruleGroup.draftHelp')}</Typography>}
      </Stack>
    </Box>
  );
}

