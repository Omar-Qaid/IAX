import { environment } from '@core/configuration/environment';
import { settingsMockRepository } from '../adapters/settingsMockRepository';
import { settingsApiRepository } from '../api/settingsApiRepository';
import type { SettingsRepository } from '../types/settingsTypes';

export const settingsService: SettingsRepository = environment.enableMockApi
  ? settingsMockRepository
  : settingsApiRepository;
