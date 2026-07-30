export type ID = string;

export interface BaseEntity {
  id: ID;
  createdAt: string;
  createdBy?: string;
  modifiedAt?: string;
  modifiedBy?: string;
}

export type StatusType = 'active' | 'onHold' | 'blocked' | 'draft' | 'confirmed' | 'invoiced' | 'cancelled' | 'open';

export interface SelectOption<T = string> {
  value: T;
  label: string;
  disabled?: boolean;
}
