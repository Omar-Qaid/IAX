import type { BuilderControlType, BuilderValidationType } from './types/processBuilderTypes';

const conditional: BuilderValidationType[] = ['compare', 'comparison', 'crossField', 'expression', 'custom'];
const text: BuilderValidationType[] = [
  'required', 'minLength', 'maxLength', 'exactLength', 'length', 'regex', 'pattern',
  'startsWith', 'endsWith', 'contains', 'email', 'url', 'phone', 'saudiMobile',
  'saudiNationalId', 'saudiIban', 'taxNumber', 'passport', 'mask', 'inputMask', ...conditional,
];
const scalar: BuilderValidationType[] = ['required', ...conditional];
const types: Record<BuilderControlType, readonly BuilderValidationType[]> = {
  text, longtext: text,
  digits: ['required', 'minValue', 'maxValue', 'range', ...conditional],
  employeeid: ['required', ...conditional],
  date: ['required', 'minDate', 'maxDate', ...conditional],
  time: scalar,
  url: ['required', 'url', 'minLength', 'maxLength', 'regex', 'pattern', ...conditional],
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
