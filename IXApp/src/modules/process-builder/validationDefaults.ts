import type { BuilderValidation, BuilderValidationType } from './types/processBuilderTypes';

export const DEFAULT_VALIDATION_MESSAGES: Record<BuilderValidationType, string> = {
  required: 'This field is required.',
  minLength: 'The value is shorter than the minimum length.',
  maxLength: 'The value exceeds the maximum length.',
  exactLength: 'The value does not have the required length.',
  length: 'The value does not have the required length.',
  minValue: 'The value is below the minimum allowed value.',
  maxValue: 'The value exceeds the maximum allowed value.',
  range: 'The value is outside the allowed range.',
  regex: 'The value does not match the required pattern.',
  pattern: 'The value does not match the required pattern.',
  startsWith: 'The value does not start with the required text.',
  endsWith: 'The value does not end with the required text.',
  contains: 'The value does not contain the required text.',
  email: 'Enter a valid email address.',
  url: 'Enter a valid URL.',
  phone: 'Enter a valid phone number.',
  saudiMobile: 'Enter a valid Saudi mobile number.',
  saudiNationalId: 'Enter a valid Saudi National ID.',
  saudiIban: 'Enter a valid Saudi IBAN.',
  taxNumber: 'Enter a valid tax number.',
  passport: 'Enter a valid passport number.',
  fileExtensions: 'Select a file with an allowed extension.',
  fileSize: 'The selected file exceeds the allowed size.',
  maxFiles: 'Too many files were selected.',
  minSelected: 'Select at least the minimum number of items.',
  maxSelected: 'Too many items were selected.',
  compare: 'The value does not satisfy the comparison rule.',
  comparison: 'The value does not satisfy the comparison rule.',
  expression: 'The value does not satisfy the validation expression.',
  custom: 'The value is invalid.',
  crossField: 'The value does not match the related field.',
  mask: 'The value does not match the required format.',
  inputMask: 'The value does not match the required format.',
};

const customMessageTypes = new Set<BuilderValidationType>(['regex', 'pattern', 'expression', 'custom']);

export const validationUsesCustomMessage = (type: BuilderValidationType): boolean =>
  customMessageTypes.has(type);

export const resolvedValidationMessage = (rule: Pick<BuilderValidation, 'type' | 'message'>): string =>
  rule.message.trim() || DEFAULT_VALIDATION_MESSAGES[rule.type];
