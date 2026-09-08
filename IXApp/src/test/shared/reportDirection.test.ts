import { describe, expect, it } from 'vitest';
import { resolveReportDirection } from '@shared/components/report-viewer/reportDirection';

describe('resolveReportDirection', () => {
  it('keeps an RTL application RTL for a legacy LTR template', () => {
    expect(resolveReportDirection('ltr', 'en', 'rtl')).toBe('rtl');
  });

  it('uses LTR localization even when an Arabic template was designed RTL', () => {
    expect(resolveReportDirection('rtl', 'ar', 'ltr')).toBe('ltr');
  });

  it('preserves LTR in an LTR application', () => {
    expect(resolveReportDirection('ltr', 'en', 'ltr')).toBe('ltr');
  });
});
