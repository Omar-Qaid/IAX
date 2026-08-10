import { z } from 'zod';

export const globalSettingsSchema = z.object({
  recId: z.number().int().nonnegative(),
  appName: z.string().trim().min(1).max(256),
  defaultLanguage: z.string().trim().min(1).max(10),
  timeZone: z.string().trim().min(1).max(50),
  currency: z.string().trim().min(1).max(10),
  dateFormat: z.string().trim().min(1).max(50),
  enableAuditLog: z.boolean(),
  maxUploadSize: z.coerce.number().int().positive(),
  paginationSize: z.coerce.number().int().min(1).max(100),
  decimalPlaces: z.coerce.number().int().min(0),
});

export const userSettingsSchema = z.object({
  recId: z.number().int().nonnegative(),
  userId: z.string(),
  theme: z.string().trim().min(1).max(20),
  language: z.string().trim().min(1).max(10),
  pageSize: z.coerce.number().int().min(1).max(100),
  notificationEnabled: z.boolean(),
  dashboardLayout: z.string().max(2000),
});
