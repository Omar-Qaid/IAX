import { useEffect, useState } from 'react';
import type { ProcessBuilderDocument } from '../types/processBuilderTypes';

const key = (id: string) => `ixapp.process-builder.${id}`;
export const loadProcessBuilderDraft = (id: string, fallback: ProcessBuilderDocument) => {
  try {
    const raw = localStorage.getItem(key(id));
    if (!raw) return fallback;
    const parsed = JSON.parse(raw) as ProcessBuilderDocument;
    const normalizeControl = (control: ProcessBuilderDocument['requestControls'][number], index = 0) => ({ ...control, code: control.code ?? `CTRL-${String(index + 1).padStart(4, '0')}`, visible: control.visible ?? true, uniqueKey: control.uniqueKey ?? false, usedAsCriteria: control.usedAsCriteria ?? false, defaultValue: control.defaultValue ?? '', options: control.options ?? [], validations: (control.validations ?? []).map((validation, validationIndex) => ({ ...validation, secondaryValue: validation.secondaryValue ?? '', operator: validation.operator ?? '', mask: validation.mask ?? '', messageAR: validation.messageAR ?? '', severity: validation.severity ?? 'Error', sortOrder: validation.sortOrder ?? (validationIndex + 1) * 10, active: validation.active ?? true })), visibilityCondition: control.visibilityCondition ?? null });
    return {
      ...fallback,
      ...parsed,
      variables: (parsed.variables ?? []).map((variable, index) => ({ ...variable, description: variable.description ?? '', descriptionAR: variable.descriptionAR ?? '', sortOrder: variable.sortOrder ?? (index + 1) * 10, active: variable.active ?? true, scope: variable.scope ?? 'process' })),
      requestControls: (parsed.requestControls ?? []).map(normalizeControl),
      steps: (parsed.steps ?? []).map((step, stepIndex) => ({ ...step, code: step.code ?? `STEP-${String(stepIndex + 1).padStart(5, '0')}`, score: step.score ?? 0, active: step.active ?? true, systemField: step.systemField ?? false, condition: step.condition ?? null, activities: (step.activities ?? []).map((activity, activityIndex) => ({ ...activity, code: activity.code ?? `ACT-${String(activityIndex + 1).padStart(5, '0')}`, assignmentMode: activity.assignmentMode ?? 'any', active: activity.active ?? true, mandatoryDocs: activity.mandatoryDocs ?? false, autoPassEnabled: activity.autoPassEnabled ?? false, autoPassingHours: activity.autoPassingHours ?? 0, actions: activity.actions ?? [], validations: activity.validations ?? [], config: activity.config ?? { apiMethod: 'GET', apiUrl: '', notifyEmails: '' }, condition: activity.condition ?? null, controls: (activity.controls ?? []).map(normalizeControl) })) })),
      transitions: (parsed.transitions ?? []).map((transition, index) => ({ ...transition, sortOrder: transition.sortOrder ?? (index + 1) * 10, active: transition.active ?? true, triggerSource: transition.triggerSource ?? 'none', triggerId: transition.triggerId ?? '' })),
    };
  } catch { return fallback; }
};
export const useProcessBuilderDraft = (document: ProcessBuilderDocument, dirty: boolean, onSaved?: () => void) => {
  const [savedAt, setSavedAt] = useState<Date | null>(null);
  useEffect(() => {
    if (!dirty) return;
    const timer = window.setTimeout(() => { try { localStorage.setItem(key(document.id), JSON.stringify(document)); setSavedAt(new Date()); onSaved?.(); } catch { /* Draft remains dirty when browser storage is unavailable. */ } }, 500);
    return () => window.clearTimeout(timer);
  }, [dirty, document, onSaved]);
  return { savedAt, clear: () => localStorage.removeItem(key(document.id)) };
};
