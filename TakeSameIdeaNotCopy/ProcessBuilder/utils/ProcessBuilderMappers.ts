/**
 * @deprecated
 * This file is kept for backward-compatibility only.
 * All mapper logic has been split into focused subdomain files:
 *
 *   utils/mappers/processMappers.ts   → WfProcess ↔ ProcessInfo
 *   utils/mappers/stepMappers.ts      → WfStep ↔ Step
 *   utils/mappers/activityMappers.ts  → WfActivity ↔ Activity
 *   utils/mappers/controlMappers.ts   → Controls, Variables, Catalog utilities
 *
 * Please update your imports to use `../utils/mappers` (barrel) or the
 * specific subdomain file directly.
 */
export * from './mappers';
