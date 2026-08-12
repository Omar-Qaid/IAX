import React from 'react';
import type { ReactNode } from 'react';
import { useProcessBuilderStore, defaultProcessInfo } from '../store/processBuilderStore';
import type { ProcessBuilderState } from '../store/processBuilderStore';

// Re-export the store and types to maintain backwards compatibility
export { defaultProcessInfo };
export type { ProcessBuilderState as ProcessBuilderContextType };

// The provider is no longer strictly necessary with Zustand, but we keep it
// so that `<ProcessBuilderProvider>` wrappers in `ProcessBuilderPage` don't break.
export const ProcessBuilderProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    return <>{children}</>;
};

/**
 * @deprecated This uses the global Zustand store now. Use selectors for better performance.
 * E.g., const addStep = useProcessBuilderContext(state => state.addStep);
 */
export const useProcessBuilderContext = useProcessBuilderStore;
