import type { BaseEntity } from './common';

export interface AuditInfo {
  createdAt: string;
  createdBy?: string;
  modifiedAt?: string;
  modifiedBy?: string;
}

export type EntityWithAudit<T> = T & BaseEntity;
