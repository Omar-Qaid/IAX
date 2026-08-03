export interface DialogState<T = unknown> { open: boolean; data?: T; loading?: boolean; error?: string | null }
