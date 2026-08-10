import type { GlobalSettings, SettingsRepository, UserSettings } from '../types/settingsTypes';

let globalSettings: GlobalSettings = {
  recId: 1,
  appName: 'IXApp',
  defaultLanguage: 'en',
  timeZone: 'Asia/Riyadh',
  currency: 'SAR',
  dateFormat: 'yyyy-MM-dd',
  enableAuditLog: true,
  maxUploadSize: 10_485_760,
  paginationSize: 25,
  decimalPlaces: 2,
};

let userSettings: UserSettings = {
  recId: 1,
  userId: 'usr-001',
  theme: 'light',
  language: 'en',
  pageSize: 25,
  notificationEnabled: true,
  dashboardLayout: 'default',
};

const copy = <T extends object>(value: T): T => ({ ...value });

export const settingsMockRepository: SettingsRepository = {
  async getGlobal(): Promise<GlobalSettings> {
    return copy(globalSettings);
  },
  async updateGlobal(settings): Promise<GlobalSettings> {
    globalSettings = copy(settings);
    return copy(globalSettings);
  },
  async getUser(): Promise<UserSettings> {
    return copy(userSettings);
  },
  async updateUser(settings): Promise<UserSettings> {
    userSettings = copy(settings);
    return copy(userSettings);
  },
};
