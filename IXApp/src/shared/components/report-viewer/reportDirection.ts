export type ResolvedReportDirection = 'ltr' | 'rtl';

/** Runtime print direction follows the active application localization. */
export function resolveReportDirection(
  _templateDirection: ResolvedReportDirection | undefined,
  _templateLanguage: string | undefined,
  applicationDirection: ResolvedReportDirection
): ResolvedReportDirection {
  return applicationDirection;
}
