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
