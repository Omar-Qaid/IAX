import type { PrintTemplateSummary } from '../types/printTemplate.types';

const isPublished = (status: PrintTemplateSummary['status']): boolean =>
  status === 1 || String(status).toLowerCase() === 'published';

export const selectPublishedTemplates = (
  templates: readonly PrintTemplateSummary[] | undefined
): PrintTemplateSummary[] =>
  [...(templates ?? [])]
    .filter(
      (template) =>
        template.isActive && template.currentVersionId != null && isPublished(template.status)
    )
    .sort(
      (left, right) =>
        Number(right.isDefault) - Number(left.isDefault) || left.name.localeCompare(right.name)
    );

export const selectDefaultPublishedTemplate = (
  templates: readonly PrintTemplateSummary[] | undefined
): PrintTemplateSummary | undefined =>
  selectPublishedTemplates(templates).find((template) => template.isDefault);
