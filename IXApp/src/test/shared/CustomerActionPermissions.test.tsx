import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, fireEvent } from '@test/testUtils';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { EnterpriseCommandUtilities } from '@shared/components/action-pane/EnterpriseCommandUtilities';
const access = vi.hoisted(() => ({ allowed: new Set<string>() }));
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({ hasPermission: (permission: string) => access.allowed.has(permission) }),
}));
beforeEach(() => access.allowed.clear());
const labels = {
  editLabel: 'Edit',
  newLabel: 'New',
  deleteLabel: 'Delete',
  canEdit: true,
  canNew: true,
  canDelete: true,
  editPermission: 'edit',
  newPermission: 'create',
  deletePermission: 'delete',
  onEdit: vi.fn(),
  onNew: vi.fn(),
  onDelete: vi.fn(),
};
describe('customer action permission enforcement', () => {
  it('does not expose mutation actions when the caller has only view access', () => {
    render(<EnterpriseCrudActions {...labels} />);
    expect(screen.queryByRole('button', { name: 'Edit' })).toBeNull();
    expect(screen.queryByRole('button', { name: 'New' })).toBeNull();
    expect(screen.queryByRole('button', { name: 'Delete' })).toBeNull();
  });
  it('retains only the permitted mutation and invokes its handler', () => {
    access.allowed.add('create');
    const create = vi.fn();
    render(<EnterpriseCrudActions {...labels} onNew={create} />);
    fireEvent.click(screen.getByRole('button', { name: 'New' }));
    expect(create).toHaveBeenCalledOnce();
    expect(screen.queryByRole('button', { name: 'Delete' })).toBeNull();
  });
  it('also protects Save while preserving Cancel', () => {
    render(
      <EnterpriseCrudActions
        {...labels}
        editing
        savePermission="edit"
        onSave={vi.fn()}
        onCancel={vi.fn()}
      />
    );
    expect(screen.queryByRole('button', { name: 'Save' })).toBeNull();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
  });
  it('allows Save for a create-only user when the parent is creating a record', () => {
    access.allowed.add('create');
    render(
      <EnterpriseCrudActions
        {...labels}
        editing
        savePermission="create"
        onSave={vi.fn()}
        onCancel={vi.fn()}
      />
    );
    expect(screen.getByRole('button', { name: 'Save' })).toBeEnabled();
  });
  it('disables unsupported utilities and opens the current page in a new window', () => {
    const open = vi.spyOn(window, 'open').mockReturnValue(null);
    render(
      <EnterpriseCommandUtilities
        personalizeLabel="Personalize"
        guideLabel="Guide"
        notificationsLabel="Notifications"
        refreshLabel="Refresh"
        openWindowLabel="Open window"
      />
    );
    for (const name of ['Personalize', 'Guide', 'Notifications', 'Refresh'])
      expect(screen.getByRole('button', { name })).toBeDisabled();
    fireEvent.click(screen.getByRole('button', { name: 'Open window' }));
    expect(open).toHaveBeenCalledWith(window.location.href, '_blank', 'noopener,noreferrer');
    open.mockRestore();
  });
});
