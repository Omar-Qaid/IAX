/**
 * Centralized Print Engine — Runtime Data Resolution
 *
 * Generic data context and binding resolver used by ReportViewer
 * to map field bindings to actual values. Module-specific data factories
 * (e.g. createruntimeReportData in Workflow) remain in their respective modules.
 */

import type { PrintFieldBinding } from '../report-designer/types';

// ---------------------------------------------------------------------------
// Runtime data context
// ---------------------------------------------------------------------------

export interface runtimeReportData {
  system: Record<string, unknown>;
  company: Record<string, unknown>;
  report: Record<string, unknown>;
  requestControls: Record<string, unknown>;
  repeating: Record<string, unknown>;
}

// ---------------------------------------------------------------------------
// Utilities
// ---------------------------------------------------------------------------

interface LocationControlValue {
  address?: unknown;
  latitude?: unknown;
  longitude?: unknown;
}

export const formatRequestControlValue = (value: string, controlType: string): string => {
  const normalizedType = controlType.replace(/[^a-z0-9]/gi, '').toLocaleLowerCase();
  if (normalizedType !== 'location' || !value.trim()) return value;

  try {
    const parsed = JSON.parse(value) as unknown;
    if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) return value;

    const location = parsed as LocationControlValue;
    const address = typeof location.address === 'string' ? location.address.trim() : '';
    if (address) return address;

    const hasCoordinates =
      (typeof location.latitude === 'number' || typeof location.latitude === 'string') &&
      (typeof location.longitude === 'number' || typeof location.longitude === 'string');
    return hasCoordinates ? `${location.latitude}, ${location.longitude}` : value;
  } catch {
    // Preserve plain-text and malformed legacy location values instead of hiding them.
    return value;
  }
};

// ---------------------------------------------------------------------------
// Binding resolver
// ---------------------------------------------------------------------------

export const resolveRuntimeBinding = (
  data: runtimeReportData,
  binding: PrintFieldBinding
): unknown => {
  if (binding.sourceType === 'requestControl') {
    const id = binding.requestControlId ?? binding.controlId;
    return id == null ? undefined : data.requestControls[String(id)];
  }
  if (binding.sourceType === 'system')
    return binding.source ? data.system[binding.source] : undefined;
  if (binding.sourceType === 'company')
    return binding.source ? data.company[binding.source] : undefined;
  if (binding.sourceType === 'report')
    return binding.source ? data.report[binding.source] : undefined;
  if (binding.sourceType === 'repeating')
    return binding.source ? data.repeating[binding.source] : undefined;
  return undefined;
};
