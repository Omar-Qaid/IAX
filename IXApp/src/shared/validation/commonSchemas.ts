import { z } from 'zod';
import { validationMessage } from './validationMessages';

export const requiredTextSchema = (field: string) =>
  z.string().trim().min(1, validationMessage.required(field));

export const emailSchema = () =>
  z.string().trim().email(validationMessage.invalidEmail());

export const optionalEmailSchema = () =>
  z.union([z.literal(''), emailSchema()]).optional();

export const urlSchema = () =>
  z.string().trim().url(validationMessage.invalidUrl());

export const numberSchema = () =>
  z.coerce.number({ invalid_type_error: validationMessage.invalidNumber() });

export const dateSchema = () =>
  z.coerce.date({ invalid_type_error: validationMessage.invalidDate() });
