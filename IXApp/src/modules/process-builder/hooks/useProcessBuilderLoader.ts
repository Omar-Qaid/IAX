import { useEffectEvent, useLayoutEffect, useState } from 'react';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useNotifications } from '@shared/hooks/useNotifications';
import {
  createProcessBuilderDocument,
  useProcessBuilderStore,
} from '../store/useProcessBuilderStore';
import { loadProcessBuilderDraft } from './useProcessBuilderDraft';
import { readNavigationState } from './processBuilderNavigation';
import {
  getProcessCodeMetadata,
  getVariableCodeMetadata,
  getStepCodeMetadata,
  getActivityCodeMetadata,
  getRequestControlCodeMetadata,
  loadProcessBuilder,
} from '../api/processBuilderApi';

export function useProcessBuilderLoader(builderId: string) {
  const { t } = useAppTranslation();
  const { notifyError } = useNotifications();
  const initialize = useProcessBuilderStore((state) => state.initialize);
  const restoreNavigation = useProcessBuilderStore((state) => state.restoreNavigation);
  const reportLoadError = useEffectEvent((error: unknown) => {
    notifyError(error instanceof Error ? error.message : t('wfProcessBuilder.messages.loadFailed'));
  });
  const [loading, setLoading] = useState(true);
  const [manualVariableCode, setManualVariableCode] = useState(false);
  const [manualStepCode, setManualStepCode] = useState(false);
  const [manualActivityCode, setManualActivityCode] = useState(false);
  const [manualRequestControlCode, setManualRequestControlCode] = useState(false);
  useLayoutEffect(() => {
    let active = true;
    const controller = new AbortController();
    const load = async () => {
      setLoading(true);
      const metadataPromise = Promise.allSettled([
        getVariableCodeMetadata(),
        getStepCodeMetadata(),
        getActivityCodeMetadata(),
        getRequestControlCodeMetadata(),
      ]);
      try {
        if (builderId === 'new') {
          const fallback = createProcessBuilderDocument('new');
          const recovered = loadProcessBuilderDraft(builderId, fallback);
          if (active) {
            initialize(recovered);
            const navigation = readNavigationState(builderId);
            if (navigation) restoreNavigation(navigation);
          }
          const metadata = await getProcessCodeMetadata();
          if (active && !metadata.manual)
            useProcessBuilderStore.getState().setGeneratedCode(metadata.previewCode ?? '');
        } else {
          const fallback = await loadProcessBuilder(Number(builderId), controller.signal);
          if (active) {
            initialize(loadProcessBuilderDraft(builderId, fallback));
            const navigation = readNavigationState(builderId);
            if (navigation) restoreNavigation(navigation);
          }
        }
      } catch (error) {
        if (active) {
          // A preview failure must not discard edits already made in a new draft.
          if (builderId !== 'new') initialize(createProcessBuilderDocument(builderId));
          reportLoadError(error);
        }
      } finally {
        const [variableMetadata, stepMetadata, activityMetadata, requestControlMetadata] =
          await metadataPromise;
        if (active) {
          if (variableMetadata.status === 'fulfilled')
            setManualVariableCode(variableMetadata.value.manual);
          if (stepMetadata.status === 'fulfilled') setManualStepCode(stepMetadata.value.manual);
          if (activityMetadata.status === 'fulfilled')
            setManualActivityCode(activityMetadata.value.manual);
          if (requestControlMetadata.status === 'fulfilled')
            setManualRequestControlCode(requestControlMetadata.value.manual);
        }
        if (active) setLoading(false);
      }
    };
    void load();
    return () => {
      active = false;
      controller.abort();
    };
  }, [builderId, initialize, restoreNavigation]);
  return {
    loading,
    manualVariableCode,
    manualStepCode,
    manualActivityCode,
    manualRequestControlCode,
  };
}
