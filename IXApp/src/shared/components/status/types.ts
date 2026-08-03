export type StatusTone = 'default' | 'info' | 'success' | 'warning' | 'error';
export interface StatusOption<T extends string = string> { value: T; label: string; tone?: StatusTone }
