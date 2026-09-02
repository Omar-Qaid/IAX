import { useEffect, useState } from 'react';
import type { ProcessBuilderDocument } from '../types/processBuilderTypes';

const key = (id: string) => `ixapp.process-builder.${id}`;
export const loadProcessBuilderDraft = (id: string, fallback: ProcessBuilderDocument) => {
  try {
    const raw = localStorage.getItem(key(id));
    if (!raw) return fallback;
    const parsed = JSON.parse(raw) as ProcessBuilderDocument;
    const normalizeControl = (
      control: ProcessBuilderDocument['requestControls'][number],
      index = 0
    ) => ({
      ...control,
      code: control.code ?? '',
      controlId: control.controlId ?? '',
      sortOrder: index + 1,
      score: control.score ?? 0,
      visible: control.visible ?? true,
      uniqueKey: control.uniqueKey ?? false,
      usedAsCriteria: control.usedAsCriteria ?? false,
      canFilter: control.canFilter ?? true,
      canGroup: control.canGroup ?? true,
      canSort: control.canSort ?? true,
      referenceType: control.referenceType ?? null,
    fieldRole: control.fieldRole ?? 'Dimension',
    dataType: control.dataType ?? 'String',
    defaultAggregation: control.defaultAggregation ?? 'NONE',
      defaultValue: control.defaultValue ?? '',
      options: control.options ?? [],
      optionAliases: (control.options ?? []).map(
        (_, optionIndex) => control.optionAliases?.[optionIndex] ?? ''
      ),
      optionScores: control.optionScores ?? (control.options ?? []).map(() => 0),
      optionFeatureConfigurations: (control.options ?? []).map((_, optionIndex) => ({
        requireFileUpload: false,
        sendAlertMessage: false,
        alertMessage: '',
        performerIds: [],
        showOtherControls: false,
        visibleControlIds: [],
        ...(control.optionFeatureConfigurations?.[optionIndex] ?? {}),
      })),
      validations: (control.validations ?? []).map((validation, validationIndex) => ({
        ...validation,
        secondaryValue: validation.secondaryValue ?? '',
        operator: validation.operator ?? '',
        mask: validation.mask ?? '',
        messageAlias: validation.messageAlias ?? '',
        severity: validation.severity ?? 'Error',
        sortOrder: validation.sortOrder ?? (validationIndex + 1) * 10,
        active: validation.active ?? true,
      })),
      visibilityCondition: control.visibilityCondition ?? null,
    });
    return {
      ...fallback,
      ...parsed,
      variables: (parsed.variables ?? []).map((variable, index) => ({
        ...variable,
        description: variable.description ?? '',
        sortOrder: variable.sortOrder ?? (index + 1) * 10,
        active: variable.active ?? true,
        scope: variable.scope ?? 'process',
      })),
      requestControls: (parsed.requestControls ?? []).map(normalizeControl),
      steps: (parsed.steps ?? []).map((step) => ({
        ...step,
        code: step.code ?? '',
        score: step.score ?? 0,
        active: step.active ?? true,
        systemField: step.systemField ?? false,
        condition: step.condition ?? null,
        activities: (step.activities ?? []).map((activity, activityIndex) => ({
          ...activity,
          code: activity.code ?? '',
          activityTypeId: activity.activityTypeId ?? '',
          score: activity.score ?? 0,
          sortOrder: activity.sortOrder ?? (activityIndex + 1) * 10,
          assignmentMode: activity.assignmentMode ?? 'any',
          active: activity.active ?? true,
          mandatoryDocs: activity.mandatoryDocs ?? false,
          autoPassEnabled: activity.autoPassEnabled ?? false,
          autoPassingHours: activity.autoPassingHours ?? 0,
          actions: activity.actions ?? [],
          validations: activity.validations ?? [],
          config: activity.config ?? { apiMethod: 'GET', apiUrl: '', notifyEmails: '' },
          condition: activity.condition ?? null,
          controls: (activity.controls ?? []).map(normalizeControl),
        })),
      })),
      transitions: (parsed.transitions ?? []).map((transition, index) => ({
        ...transition,
        operatorId: transition.operatorId ?? '',
        sortOrder: transition.sortOrder ?? (index + 1) * 10,
        active: transition.active ?? true,
        triggerSource:
          (transition.triggerSource as string) === 'activityControl'
            ? 'activity'
            : (transition.triggerSource ?? 'none'),
        triggerId: transition.triggerId ?? '',
      })),
    };
  } catch {
    return fallback;
  }
};
export const useProcessBuilderDraft = (
  document: ProcessBuilderDocument,
  dirty: boolean,
  onSaved?: () => void
) => {
  const [savedAt, setSavedAt] = useState<Date | null>(null);
  useEffect(() => {
    if (!dirty) return;
    const timer = window.setTimeout(() => {
      try {
        localStorage.setItem(key(document.id), JSON.stringify(document));
        setSavedAt(new Date());
        onSaved?.();
      } catch {
        /* Draft remains dirty when browser storage is unavailable. */
      }
    }, 500);
    return () => window.clearTimeout(timer);
  }, [dirty, document, onSaved]);
  return { savedAt, clear: () => localStorage.removeItem(key(document.id)) };
};
