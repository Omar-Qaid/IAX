import React from 'react';
import { Alert, Stack, Typography } from '@mui/material';
import { useParams } from 'react-router-dom';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { MasterDetailPage } from '@patterns/master-detail';
import { CommitAwareCrudActions } from '@shared/components/action-pane/CommitAwareCrudActions';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import { AccessDeniedState } from '@shared/components/feedback/AccessDeniedState';
import { PageEditSessionContext } from '@shared/components/data-grid/PageEditSession';
import { localizedName } from '@shared/utilities/localizedName';
import type { DetailSectionConfig } from '@patterns/list-details/types';
import { accountContacts } from '../routes/contactRoutes';
import { emptyContact, type ContactPersonDetailsProps } from './contactPersonModel';
import { buildContactPersonHeaderSections } from './ContactPersonHeaderSections';
import { useContactPersonDocument } from './useContactPersonDocument';

/** Contact create/view/edit forms use the shared master-details shell without invented document lines. */
export function ContactPersonDetails(props: ContactPersonDetailsProps): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const { kind = '', accountNum = '' } = useParams();
  const { state, config, busy, more, setMore, ribbons, hasPermission, actionError, beginEdit } =
    useContactPersonDocument(props);
  const [deleteOpen, setDeleteOpen] = React.useState(false);
  const row = state.draft ?? emptyContact();
  const canView = hasPermission(PERMISSIONS.CONTACT_VIEW);
  const canEdit = hasPermission(PERMISSIONS.CONTACT_EDIT);
  const canCreate = hasPermission(PERMISSIONS.CONTACT_CREATE);
  const canDelete = hasPermission(PERMISSIONS.CONTACT_DELETE);
  const canChange = state.isNew ? canCreate : canEdit;
  if (!canView || (!props.record && !canCreate)) return <AccessDeniedState />;
  const sections = buildContactPersonHeaderSections({
    row,
    editing: state.editing && canChange,
    more,
    setMore,
  }).map((section) => ({
    ...section,
    visualVariant: 'legalEntity' as const,
    editing: state.editing && canChange,
    groups: section.groups?.map((group) => ({
      ...group,
      fields: group.fields.map((field) => ({ ...field, disabled: field.disabled || busy })),
    })),
  }));
  const messages = [state.error, actionError, ...Object.values(state.validationErrors)].filter(
    Boolean
  );
  const notices: DetailSectionConfig[] = messages.length
    ? [
        {
          id: 'contact-errors',
          title: '',
          hideHeader: true,
          content: (
            <Alert severity="error">
              <Stack>
                {messages.map((message, index) => (
                  <Typography key={index} variant="body2">
                    {message}
                  </Typography>
                ))}
              </Stack>
            </Alert>
          ),
        },
      ]
    : [];
  return (
    <PageEditSessionContext.Provider
      value={{
        editing: state.editing && canChange,
        saving: busy,
        requestEdit: beginEdit,
        registerEditor: state.registerEditor,
      }}
    >
      <MasterDetailPage
        records={state.records.map((record) => ({
          ...record,
          title: localizedName(record, isRtl) || record.contactPersonId,
          subtitle: record.contactPersonId,
        }))}
        selectedId={state.selectedId ?? undefined}
        onSelect={(id) => {
          if (!state.editing && !busy && id !== row.id) props.open(id);
        }}
        title={
          state.isNew
            ? t('contactPerson.new', 'New contact')
            : localizedName(row, isRtl) ||
              row.contactPersonId ||
              t('contactPerson.contact', 'Contact')
        }
        subtitle={props.account}
        viewLabel={t('common.standardView', 'Standard view')}
        status={row.inactive ? t('common.inactive', 'Inactive') : t('common.active', 'Active')}
        breadcrumb={{
          listPath: accountContacts(kind, accountNum),
          label: state.isNew
            ? t('contactPerson.new', 'New contact')
            : t('contactPerson.contact', 'Contact'),
        }}
        filterLabel={t('actions.filter', 'Filter')}
        emptyLabel={t('messages.noRecords', 'No records')}
        editing={state.editing || busy}
        defaultNavigationOpen={Boolean(props.record)}
        navigationWidthStorageKey="contactPerson.navigation-width"
        searchNavigation
        matchesSearch={(record, query) =>
          `${record.contactPersonId} ${record.name} ${record.nameAlias}`
            .toLocaleLowerCase()
            .includes(query.toLocaleLowerCase())
        }
        backAction={
          <ActionPaneBackButton
            label={t('actions.back', 'Back')}
            disabled={state.editing || busy}
            onClick={() => props.open()}
          />
        }
        actionPane={
          <CommitAwareCrudActions
            editLabel={t('actions.edit', 'Edit')}
            newLabel={t('actions.new', 'New')}
            deleteLabel={t('actions.delete', 'Delete')}
            saveLabel={t('actions.save', 'Save')}
            cancelLabel={t('actions.cancel', 'Cancel')}
            editPermission={PERMISSIONS.CONTACT_EDIT}
            newPermission={PERMISSIONS.CONTACT_CREATE}
            deletePermission={PERMISSIONS.CONTACT_DELETE}
            savePermission={state.isNew ? PERMISSIONS.CONTACT_CREATE : PERMISSIONS.CONTACT_EDIT}
            canEdit={canEdit && Boolean(state.selected) && !busy}
            canNew={canCreate && !busy}
            canDelete={canDelete && Boolean(state.selected) && !busy}
            editing={state.editing}
            saving={busy}
            onEdit={beginEdit}
            onNew={() => props.open('new')}
            onDelete={() => setDeleteOpen(true)}
            onSave={() => state.save()}
            onCancel={state.cancel}
          />
        }
        ribbons={ribbons}
        sections={[...notices, ...sections]}
        values={config.getValues(row)}
        onFieldChange={(name, value) => {
          if (!busy && canChange) state.changeValue(name, value);
        }}
        onFieldEdit={canEdit && !busy ? beginEdit : undefined}
        canEditField={(name) => canEdit && !busy && !['account', 'contactPersonId'].includes(name)}
        recordTableName="ContactPerson"
        getAuditRecordId={(record) => record.recId}
        showOptionsMenu={!state.isNew}
        onRefresh={() => {
          if (!state.editing && !busy) void state.refresh();
        }}
      />
      <ConfirmationDialog
        open={deleteOpen}
        title={t('actions.delete', 'Delete')}
        message={t('contactPerson.confirmDelete', 'Delete this contact?')}
        confirmLabel={t('actions.delete', 'Delete')}
        loading={busy}
        onClose={() => {
          if (!busy) setDeleteOpen(false);
        }}
        onConfirm={() => {
          void state.remove().finally(() => setDeleteOpen(false));
        }}
      />
    </PageEditSessionContext.Provider>
  );
}
