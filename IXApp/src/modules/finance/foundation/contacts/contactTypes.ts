export interface Contact {
  recId: number;
  contactPersonId: string;
  party: number;
  name: string;
  nameAlias: string;
  languageId: string;
  addressBookNames: string;
  initials: string | null;
  birthYear: number | null;
  birthMonth: number | null;
  birthday: number | null;
  anniversaryYear: number | null;
  anniversaryMonth: number | null;
  anniversaryDay: number | null;
  telephone: string | null;
  extension: string | null;
  nameParts?: { firstName: string; middleName: string; lastNamePrefix: string; lastName: string };
  gender: number | string | null;
  maritalStatus: number | string | null;
  inactive: boolean;
  vendorPortalAccessAllowed: boolean;
}
