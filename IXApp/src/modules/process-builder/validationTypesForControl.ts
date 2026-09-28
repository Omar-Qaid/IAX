import type { BuilderControlType, BuilderValidationType } from './types/processBuilderTypes';

// Keep legacy aliases supported by the API, but expose one canonical option for
// each behavior in the Process Builder menu.
const conditional: BuilderValidationType[] = ['compare', 'crossField', 'expression'];
const text: BuilderValidationType[] = [
  'required', 'minLength', 'maxLength', 'exactLength', 'pattern',
  'startsWith', 'endsWith', 'contains', 'email', 'url', 'phone', 'saudiMobile',
  'saudiNationalId', 'saudiIban', 'taxNumber', 'passport', 'inputMask', ...conditional,
];
const scalar: BuilderValidationType[] = ['required', ...conditional];
const types: Record<BuilderControlType, readonly BuilderValidationType[]> = {
  text, longtext: text,
  digits: ['required', 'minValue', 'maxValue', 'range', 'uniquePerApplicant', 'uniqueGlobal', ...conditional],
  employeeid: ['required', ...conditional],
  date: ['required', 'minDate', 'maxDate', ...conditional],
  time: scalar,
  url: ['required', 'url', 'minLength', 'maxLength', 'pattern', ...conditional],
  'dropdown-db': scalar,
  'dropdown-manual': scalar,
  checkbox: scalar,
  checkboxlist: ['required', 'minSelected', 'maxSelected', ...conditional],
  radiobuttonlist: scalar,
  file: ['required', 'fileExtensions', 'fileSize', 'maxFiles'],
  table: ['required'],
  label: [],
  employeesearch: scalar,
  showroom: scalar,
  signature: ['required'],
  location: ['required'],
  advertiser: scalar,
};

export function validationTypesForControl(type?: BuilderControlType): readonly BuilderValidationType[] {
  return type ? types[type] : [];
}
