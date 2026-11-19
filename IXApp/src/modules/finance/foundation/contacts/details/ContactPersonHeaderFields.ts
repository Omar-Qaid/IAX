import type { DetailValues } from '@patterns/list-details/types';
import { enumIndex, genders, statuses, type ContactRecord } from './contactPersonModel';

export const getContactValues = (row: ContactRecord, account: string): DetailValues => ({
  contactPersonId: row.contactPersonId || 'Automatic',
  account,
  name: row.name,
  firstName: row.firstName,
  middleName: row.middleName,
  lastNamePrefix: row.lastNamePrefix,
  lastName: row.lastName,
  nameAlias: row.nameAlias,
  languageId: row.languageId,
  addressBookNames: row.addressBookNames,
  initials: row.initials ?? '',
  gender: enumIndex(row.gender, genders),
  maritalStatus: enumIndex(row.maritalStatus, statuses),
  birthdayDate: row.birthdayDate,
  anniversaryDate: row.anniversaryDate,
  inactive: row.inactive,
});

export const setContactValues = (row: ContactRecord, values: DetailValues): ContactRecord => ({
  ...row,
  name: String(values.name ?? ''),
  firstName: String(values.firstName ?? ''),
  middleName: String(values.middleName ?? ''),
  lastNamePrefix: String(values.lastNamePrefix ?? ''),
  lastName: String(values.lastName ?? ''),
  nameAlias: String(values.nameAlias ?? ''),
  languageId: String(values.languageId ?? ''),
  addressBookNames: String(values.addressBookNames ?? ''),
  initials: String(values.initials ?? ''),
  gender: Number(values.gender),
  maritalStatus: Number(values.maritalStatus),
  birthdayDate: String(values.birthdayDate ?? ''),
  anniversaryDate: String(values.anniversaryDate ?? ''),
  inactive: Boolean(values.inactive),
});
