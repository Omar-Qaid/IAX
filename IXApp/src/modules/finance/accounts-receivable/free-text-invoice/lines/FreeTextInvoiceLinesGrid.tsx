import React from 'react';
import { Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import { MasterDetailLines, MasterDetailLinesGrid } from '@patterns/master-detail';
import type { FreeTextInvoiceDocument } from '../details/useFreeTextInvoiceDocument';
import { isValidInvoiceLine, newLine, type Line } from './freeTextInvoiceLine';
import { useFreeTextInvoiceLineColumns } from './useFreeTextInvoiceLineColumns';

/** Invoice line actions and validation, rendered by the shared master-details grid. */
export function FreeTextInvoiceLinesGrid({
  scope,
  company,
}: {
  scope: FreeTextInvoiceDocument;
  company: string;
}) {
  const {
    t,
    editing,
    amount,
    ledgerText,
    setLedgerLabels,
    taxes,
    selectedLineId,
    setSelectedLineId,
    beginEdit,
    draftRef,
    busy,
    update,
    allowedEdit,
    display,
    selectedLine,
    grid,
    filterVisible,
    changeDraft,
  } = scope;
  const lineColumns = useFreeTextInvoiceLineColumns({
    company,
    editing,
    amount,
    ledgerText,
    setLedgerLabels,
    taxes,
  });
  const selectedLineIds = React.useMemo(
    () => (selectedLineId ? [selectedLineId] : []),
    [selectedLineId]
  );
  const addLine = async () => {
    // View mode: Add line switches the page to edit mode first (D365).
    if (!editing) beginEdit();
    if (!draftRef.current || busy || !(await grid.current?.saveEdit())) return;
    const line = {
      ...newLine(),
      lineNum: draftRef.current.lines.length + 1,
      taxGroup: draftRef.current.taxGroup,
    };
    update({ lines: [...draftRef.current.lines, line] });
    setSelectedLineId(line.id);
  };
  const removeLine = () => {
    grid.current?.cancelEdit();
    update({ lines: display.lines.filter((line) => line.id !== selectedLineId) });
    setSelectedLineId(undefined);
  };
  return (
    <MasterDetailLines
      toolbarLabel={t('freeTextInvoices.lines')}
      actions={[
        {
          id: 'add',
          label: t('freeTextInvoices.addLine'),
          icon: <AddIcon />,
          disabled: (!editing && !allowedEdit) || busy,
          onClick: () => void addLine(),
        },
        {
          id: 'remove',
          label: t('actions.remove'),
          icon: <DeleteOutlineIcon />,
          disabled: !editing || busy || !selectedLine,
          onClick: removeLine,
        },
      ]}
      saving={busy}
      grid={
        <>
          <MasterDetailLinesGrid<Line>
            editing={editing}
            busy={busy}
            canEdit={allowedEdit}
            onEdit={beginEdit}
            ref={grid}
            rows={display.lines}
            columns={lineColumns}
            storageKey="accounts-receivable.free-invoice-lines"
            hideFilterRow={!filterVisible}
            selectionMode="single"
            selectedIds={selectedLineIds}
            onRowClick={(line) => setSelectedLineId(line.id)}
            onSelectionChange={(ids) => setSelectedLineId(ids[0] ? String(ids[0]) : undefined)}
            onRowSave={(row) => {
              const current = draftRef.current;
              if (!current || !row.id) return;
              const original = current.lines.find((line) => line.id === row.id);
              if (!original) return;
              const updated = {
                ...original,
                ...row,
                quantity: Number(row.quantity ?? original.quantity),
                unitPrice: Number(row.unitPrice ?? original.unitPrice),
                ledgerDimension: Number(row.ledgerDimension ?? original.ledgerDimension),
              };
              if (!isValidInvoiceLine(updated)) {
                throw new Error(
                  t(
                    'freeTextInvoices.requiredLines',
                    'Select a customer and invoice date. Add at least one line with a description, ledger account, positive quantity and non-negative unit price.'
                  )
                );
              }
              changeDraft({
                ...current,
                lines: current.lines.map((line) => (line.id === row.id ? updated : line)),
              });
            }}
          />
          {editing && (
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
              {t('freeTextInvoices.serverAmounts')}
            </Typography>
          )}
        </>
      }
    />
  );
}
