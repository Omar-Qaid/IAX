/**
 * Centralized Print Engine — Published Template Selection
 *
 * Filters and sorts template summaries to identify published, active templates.
 * Used by any module to present a list of available templates to end users.
 */

import { reportDesignerApi } from '../report-designer/api/reportDesignerApi';
import type { PrintTemplateLanguage, ReportDesignerSummary } from '../report-designer/types';

const isPublished = (status: ReportDesignerSummary['status']): boolean =>
  status === 1 || String(status).toLowerCase() === 'published';

export const selectPublishedTemplates = (
  templates: readonly ReportDesignerSummary[] | undefined
): ReportDesignerSummary[] =>
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
  templates: readonly ReportDesignerSummary[] | undefined
): ReportDesignerSummary | undefined =>
  selectPublishedTemplates(templates).find((template) => template.isDefault);
