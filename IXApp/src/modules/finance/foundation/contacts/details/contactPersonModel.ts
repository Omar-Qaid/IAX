import type { Contact } from '../contactTypes';
export interface ContactPersonDetailsProps {
  account: string;
  endpoint: string;
  queryKey: unknown[];
  record?: Contact;
  contacts: Contact[];
  open: (id?: string) => void;
}
export interface ContactRecord extends Contact {
  id: string;
  firstName: string;
  middleName: string;
  lastNamePrefix: string;
  lastName: string;
  birthdayDate: string;
  anniversaryDate: string;
}
export const genders = ['None', 'Male', 'Female', 'NonSpecific'];
export const statuses = ['None', 'Single', 'Married', 'Divorced', 'Widowed'];
export const enumIndex = (value: number | string | null | undefined, options: string[]) =>
  typeof value === 'string' ? Math.max(0, options.indexOf(value)) : (value ?? 0);
const dateValue = (year?: number | null, month?: number | null, day?: number | null) =>
  year && month && day
    ? `${String(year).padStart(4, '0')}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`
    : '';
export const toRecord = (contact: Contact): ContactRecord => ({
  ...contact,
  id: String(contact.recId),
  firstName: contact.nameParts?.firstName ?? '',
  middleName: contact.nameParts?.middleName ?? '',
  lastNamePrefix: contact.nameParts?.lastNamePrefix ?? '',
  lastName: contact.nameParts?.lastName ?? '',
  birthdayDate: dateValue(contact.birthYear, contact.birthMonth, contact.birthday),
  anniversaryDate: dateValue(
    contact.anniversaryYear,
    contact.anniversaryMonth,
    contact.anniversaryDay
  ),
});
export const emptyContact = (): ContactRecord =>
  toRecord({
    recId: 0,
    contactPersonId: '',
    party: 0,
    name: '',
    nameAlias: '',
    languageId: 'en-US',
    addressBookNames: '',
    initials: '',
    birthYear: null,
    birthMonth: null,
    birthday: null,
    anniversaryYear: null,
    anniversaryMonth: null,
    anniversaryDay: null,
    telephone: null,
    extension: null,
    nameParts: { firstName: '', middleName: '', lastNamePrefix: '', lastName: '' },
    gender: 0,
    maritalStatus: 0,
    inactive: false,
    vendorPortalAccessAllowed: false,
  });
const namePartsOf = (row: ContactRecord) => ({
  firstName: row.firstName,
  middleName: row.middleName,
  lastNamePrefix: row.lastNamePrefix,
  lastName: row.lastName,
});
export const fullName = (row: ContactRecord) =>
  row.nameParts || Object.values(namePartsOf(row)).some(Boolean)
    ? Object.values(namePartsOf(row))
        .map((value) => value.trim())
        .filter(Boolean)
        .join(' ')
    : row.name;

export const toContactInput = (row: ContactRecord) => ({
  name: fullName(row),
  nameAlias: row.nameAlias,
  languageId: row.languageId,
  addressBookNames: row.addressBookNames,
  initials: row.initials,
  nameParts:
    row.nameParts || Object.values(namePartsOf(row)).some(Boolean) ? namePartsOf(row) : null,
  gender: enumIndex(row.gender, genders),
  maritalStatus: enumIndex(row.maritalStatus, statuses),
  birthday: row.birthdayDate || null,
  anniversary: row.anniversaryDate || null,
  inactive: row.inactive,
});
