export type FormErrors<T> = Partial<Record<keyof T, string>>;
export type FormMode = 'view' | 'edit' | 'create';
