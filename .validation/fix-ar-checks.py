from pathlib import Path
p=Path('IXApp/src/modules/finance/accounts-receivable/sales-order/lines/SalesOrderLineFields.tsx');s=p.read_text().replace("if (!activeHeader || name === 'itemNumber')\n      return field(label, String(displayedLine[name] ?? ''), name !== 'itemNumber');","if (!activeHeader) return field(label, String(displayedLine[name] ?? ''), true);")
with p.open('r+',newline='') as f:f.write(s);f.truncate()
p=Path('IXApp/src/test/shared/RecordPreview.test.tsx');s=p.read_text();marker="  it('previews a form lookup by its masterRoute, with no per-field setup', async () => {";test='''  it('disables master navigation around custom editors until view mode returns', () => {
    const show = (editing: boolean) => (
      <MemoryRouter><AppProviders>
        <DataGrid rows={[{ id: '1', group: 'CONTR' }]} navigationDisabled={editing}
          columns={[{ field: 'group', headerName: 'Customer group', preview: { entity: 'testGroup' },
            renderCell: ({ value }) => editing ? <input aria-label="Group editor" defaultValue={String(value)} /> : String(value) }]}
          height={200} hideToolbar />
      </AppProviders></MemoryRouter>
    );
    const { rerender } = render(show(false));
    expect(screen.getByRole('link', { name: 'CONTR' })).toBeInTheDocument();
    rerender(show(true));
    expect(screen.queryByRole('link')).toBeNull();
    const editor = screen.getByRole('textbox', { name: 'Group editor' });
    expect(editor.closest('a')).toBeNull();
    fireEvent.change(editor, { target: { value: 'LOCAL' } });
    expect(editor).toHaveValue('LOCAL');
    rerender(show(false));
    expect(screen.getByRole('link', { name: 'CONTR' })).toBeInTheDocument();
  });

''';s=s.replace(marker,test+marker)
with p.open('r+',newline='') as f:f.write(s);f.truncate()
