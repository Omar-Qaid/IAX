/**
 * ProcessBuilder Mapper Barrel
 *
 * Re-exports all DTO ↔ local model mapper functions from their
 * respective subdomain files. Import from this barrel to maintain
 * a single, stable import path for consumers.
 *
 * Subdomain breakdown:
 *  - processMappers  → WfProcess ↔ ProcessInfo
 *  - stepMappers     → WfStep ↔ Step
 *  - activityMappers → WfActivity ↔ Activity
 *  - controlMappers  → WfActivityControl / WfRequestControl / ProcessVariable ↔ local models
 */
export * from './processMappers';
export * from './stepMappers';
export * from './activityMappers';
export * from './controlMappers';
export * from './transitionMappers';
export * from './requestControlsValidationMappers';

