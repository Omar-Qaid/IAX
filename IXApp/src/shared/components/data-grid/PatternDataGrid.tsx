import React from 'react';
import { Box } from '@mui/material';
import { DataGrid } from './DataGrid';
import type { DataGridProps, DataGridHandle } from './types';
import { PATTERN_GRID_DEFAULTS } from './patternGridDefaults';
import { PageEditSessionContext } from './PageEditSession';

/** Common invoice-style grid and custom editor layout for page patterns. */
export function PatternDataGrid<T>(
  props: DataGridProps<T> & { ref?: React.Ref<DataGridHandle> }
): React.ReactElement {
  const session = React.useContext(PageEditSessionContext);
  const gridRef = React.useRef<DataGridHandle>(null);
  React.useImperativeHandle(props.ref, () => gridRef.current!);
  const editable = Boolean(props.onRowSave || props.masterForm || props.onCellEditRequest);
  const registerEditor = session?.registerEditor;
  React.useEffect(() => {
    if (!registerEditor || !editable) return;
    return registerEditor({
      saveEdit: () => gridRef.current?.saveEdit() ?? Promise.resolve(true),
      cancelEdit: () => gridRef.current?.cancelEdit(),
    });
  }, [registerEditor, editable]);
  const requestEdit = React.useCallback(
    (row: T, field: string) => {
      session?.requestEdit?.();
      props.onCellEditRequest?.(row, field);
    },
    [session?.requestEdit, props.onCellEditRequest]
  );
  const rowHeight = props.rowHeight ?? PATTERN_GRID_DEFAULTS.rowHeight;
  const columns = React.useMemo(
    () =>
      props.columns.map((column) => {
        const renderEditCell = column.renderEditCell;
        return renderEditCell
          ? {
              ...column,
              renderEditCell: (params: Parameters<NonNullable<typeof renderEditCell>>[0]) => (
                <Box
                  sx={{
                    width: '100%',
                    minWidth: 0,
                    height: rowHeight,
                    display: 'flex',
                    alignItems: 'center',
                  }}
                >
                  {renderEditCell(params)}
                </Box>
              ),
            }
          : column;
      }),
    [props.columns, rowHeight]
  );
  return (
    <DataGrid
      {...PATTERN_GRID_DEFAULTS}
      {...props}
      ref={gridRef}
      columns={columns}
      masterForm={session ? session.editing && editable : props.masterForm}
      editDisabled={props.editDisabled || session?.saving}
      onCellEditRequest={
        session
          ? editable && !session.editing && !session.saving && session.requestEdit
            ? requestEdit
            : undefined
          : props.onCellEditRequest
      }
    />
  );
}
