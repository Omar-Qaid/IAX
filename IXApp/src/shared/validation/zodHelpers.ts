import type { ZodIssue } from 'zod';

export type ValidationErrorMap = Record<string, string>;

export const mapZodIssues = (issues: ZodIssue[]): ValidationErrorMap =>
  issues.reduce<ValidationErrorMap>((errors, issue) => {
    const path = issue.path.join('.') || 'form';
    if (!errors[path]) errors[path] = issue.message;
    return errors;
  }, {});
