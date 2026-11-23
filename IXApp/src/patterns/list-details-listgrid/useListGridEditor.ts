import { useContext, useEffect, useLayoutEffect, useRef } from 'react';
import {
  PageEditSessionContext,
  type PageEditor,
} from '@shared/components/data-grid/PageEditSession';

/** Connect custom list-grid editors to the page action pane's Save/Cancel lifecycle. */
export function useListGridEditor(editor: PageEditor) {
  const session = useContext(PageEditSessionContext);
  const latest = useRef(editor);
  useLayoutEffect(() => {
    latest.current = editor;
  }, [editor]);
  const register = session?.registerEditor;
  useEffect(
    () =>
      register?.({
        saveEdit: () => latest.current.saveEdit(),
        cancelEdit: () => latest.current.cancelEdit(),
      }),
    [register]
  );
  return session;
}
