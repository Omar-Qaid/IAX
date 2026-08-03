export type FeedbackSeverity = 'success' | 'info' | 'warning' | 'error';
export interface FeedbackMessage { id?: string; message: string; severity?: FeedbackSeverity; title?: string }
