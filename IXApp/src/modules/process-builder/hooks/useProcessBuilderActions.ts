import { useNavigate } from 'react-router-dom';
import { useAsyncAction } from '@shared/hooks/useAsyncAction';
import { useNotifications } from '@shared/hooks/useNotifications';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { WORKFLOW_ROUTE_PATHS } from '@modules/workflow/routes/workflowRoutePaths';
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import {
  saveProcessBuilder,
  saveProcessVariables,
  saveProcessSteps,
  saveProcessActivities,
  saveProcessRequestControls,
  saveProcessTransitions,
} from '../api/processBuilderApi';
import type { ProcessBuilderDocument } from '../types/processBuilderTypes';
import { navigationStorageKey } from './processBuilderNavigation';
import { processScheduleDraftKey } from '../processScheduleDraft';

type SectionSaveOptions<TResult> = {
  persist: (document: ProcessBuilderDocument) => Promise<TResult>;
  apply: (result: TResult) => void;
  message: 'variables' | 'activities' | 'controls' | 'transitions' | 'steps';
};

/** Section adapters supply only persistence, reconciliation, and message identity. */
function useProcessBuilderSectionSave<TResult>({
  persist,
  apply,
  message,
}: SectionSaveOptions<TResult>) {
  const { t } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  return useAsyncAction(
    async () => {
      const result = await persist(useProcessBuilderStore.getState().document);
      apply(result);
      notifySuccess(t(`wfProcessBuilder.messages.${message}Saved`));
    },
    (error) =>
      notifyError(
        error instanceof Error ? error.message : t(`wfProcessBuilder.messages.${message}SaveFailed`)
      )
  );
}

export function useProcessBuilderActions() {
  const navigate = useNavigate();
  const { t } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const { run: save, running: saving } = useAsyncAction(
    async () => {
      const document = useProcessBuilderStore.getState().document;
      const previousId = document.id;
      const persisted = await saveProcessBuilder(document);
      if (previousId !== persisted.id) {
        const scheduleDraft = localStorage.getItem(processScheduleDraftKey(previousId));
        if (scheduleDraft) {
          localStorage.setItem(processScheduleDraftKey(persisted.id), scheduleDraft);
          localStorage.removeItem(processScheduleDraftKey(previousId));
        }
      }
      localStorage.removeItem(`ixapp.process-builder.${previousId}`);
      useProcessBuilderStore.getState().applyPersistedDocument(persisted);
      notifySuccess(t('wfProcessBuilder.messages.processSaved'));
      if (previousId === 'new') {
        const navigation = sessionStorage.getItem(navigationStorageKey(previousId));
        if (navigation) sessionStorage.setItem(navigationStorageKey(persisted.id), navigation);
        navigate(WORKFLOW_ROUTE_PATHS.processBuilder(persisted.id), { replace: true });
      }
    },
    (error) =>
      notifyError(
        error instanceof Error ? error.message : t('wfProcessBuilder.messages.processSaveFailed')
      )
  );
  const { run: saveVariables, running: savingVariables } = useProcessBuilderSectionSave({
    persist: saveProcessVariables,
    apply: (result) =>
      useProcessBuilderStore.getState().setPersistedVariables(result.variables, result.variableIds),
    message: 'variables',
  });
  const { run: saveActivities, running: savingActivities } = useProcessBuilderSectionSave({
    persist: saveProcessActivities,
    apply: (result) =>
      useProcessBuilderStore.getState().setPersistedActivities(result.document, result.activityIds),
    message: 'activities',
  });
  const { run: saveRequestControls, running: savingRequestControls } = useProcessBuilderSectionSave(
    {
      persist: saveProcessRequestControls,
      apply: (result) =>
        useProcessBuilderStore
          .getState()
          .setPersistedRequestControls(result.controls, result.controlIds),
      message: 'controls',
    }
  );
  const { run: saveTransitions, running: savingTransitions } = useProcessBuilderSectionSave({
    persist: saveProcessTransitions,
    apply: (result) =>
      useProcessBuilderStore.getState().setPersistedTransitions(result.transitions),
    message: 'transitions',
  });
  const { run: saveSteps, running: savingSteps } = useProcessBuilderSectionSave({
    persist: saveProcessSteps,
    apply: (result) =>
      useProcessBuilderStore.getState().setPersistedSteps(result.steps, result.stepIds),
    message: 'steps',
  });
  return {
    save,
    saving,
    saveVariables,
    savingVariables,
    saveActivities,
    savingActivities,
    saveRequestControls,
    savingRequestControls,
    saveTransitions,
    savingTransitions,
    saveSteps,
    savingSteps,
  };
}
