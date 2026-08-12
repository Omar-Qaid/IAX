/**
 * Generates a simple, fast client-side random alphanumeric UID.
 * Used for identifying local steps, activities, controls, and validations.
 */
export const uid = (): string => Math.random().toString(36).slice(2, 10);
