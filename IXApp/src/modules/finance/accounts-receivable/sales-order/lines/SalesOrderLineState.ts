import { CommitAwareCrudActions } from '@shared/components/action-pane/CommitAwareCrudActions';
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
import { type EnterpriseCrudActionsProps } from '@shared/components/action-pane/EnterpriseCrudActions';

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

/** Maps the sales line draft lifecycle to the generic page action coordinator. */
export function SalesOrderDocumentActions(props: EnterpriseCrudActionsProps) {
  const state = useSalesOrderLineState();
  return createElement(CommitAwareCrudActions, {
    ...props,
    saving: props.saving || state.savingLine,
    beforeSave: () => (state.draftLine ? (state.commitLineRef.current?.() ?? true) : true),
    onCancelDraft: () => {
      state.setDraftLine(null);
      state.setLineBaseline(null);
      state.setActiveField('');
      state.setLineError('');
    },
  });
}
