import {
  createContext,
  createElement,
  useContext,
  useEffect,
  useRef,
  useState,
  type ReactNode,
} from 'react';
import type { SalesOrderLineRecord } from '../api/salesOrderLinesApi';
import {
  EnterpriseCrudActions,
  type EnterpriseCrudActionsProps,
} from '@shared/components/action-pane/EnterpriseCrudActions';

function useLineState() {
  const [draftLine, setDraftLine] = useState<(SalesOrderLineRecord & { orderId: string }) | null>(
    null
  );
  const [lineBaseline, setLineBaseline] = useState<SalesOrderLineRecord | null>(null);
  const [activeField, setActiveField] = useState('itemNumber');
  const [savingLine, setSavingLine] = useState(false);
  const [lineError, setLineError] = useState('');
  const savedNewRowIdRef = useRef<string | undefined>(undefined);
  const pendingSaveRef = useRef<Promise<boolean> | null>(null);
  const commitLineRef = useRef<(() => Promise<boolean>) | null>(null);
  const savingLineRef = useRef(false);
  const navigatingCellRef = useRef(false);
  const totalsRefreshTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  useEffect(
    () => () => {
      if (totalsRefreshTimerRef.current) clearTimeout(totalsRefreshTimerRef.current);
    },
    []
  );
  return {
    draftLine,
    setDraftLine,
    lineBaseline,
    setLineBaseline,
    activeField,
    setActiveField,
    savingLine,
    setSavingLine,
    lineError,
    setLineError,
    savedNewRowIdRef,
    pendingSaveRef,
    commitLineRef,
    savingLineRef,
    navigatingCellRef,
    totalsRefreshTimerRef,
  };
}

const LineStateContext = createContext<ReturnType<typeof useLineState> | null>(null);

/** Keep drafts across Lines/Header tabs without rerendering the order on every keystroke. */
export function SalesOrderLinesProvider({ children }: { children: ReactNode }) {
  const value = useLineState();
  return createElement(LineStateContext.Provider, { value }, children);
}

export function useSalesOrderLineState() {
  const state = useContext(LineStateContext);
  if (!state) throw new Error('Sales order lines require their order state provider.');
  return state;
}

/** The page may leave edit mode only after its active line has been accepted. */
export function SalesOrderDocumentActions(props: EnterpriseCrudActionsProps) {
  const state = useSalesOrderLineState();
  const [committing, setCommitting] = useState(false);
  const saveLock = useRef(false);
  return createElement(EnterpriseCrudActions, {
    ...props,
    saving: props.saving || state.savingLine || committing,
    onSave: async () => {
      if (saveLock.current) return;
      saveLock.current = true;
      setCommitting(true);
      try {
        if (
          state.draftLine &&
          state.commitLineRef.current &&
          !(await state.commitLineRef.current())
        )
          return;
        props.onSave?.();
      } finally {
        saveLock.current = false;
        setCommitting(false);
      }
    },
    onCancel: () => {
      state.setDraftLine(null);
      state.setLineBaseline(null);
      state.setActiveField('');
      state.setLineError('');
      props.onCancel?.();
    },
  });
}
