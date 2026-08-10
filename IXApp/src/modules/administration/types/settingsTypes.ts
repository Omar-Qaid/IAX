export interface GlobalSettings {
  recId: number;
  appName: string;
  defaultLanguage: string;
  timeZone: string;
  currency: string;
  dateFormat: string;
  enableAuditLog: boolean;
  maxUploadSize: number;
  paginationSize: number;
  decimalPlaces: number;
}

export interface UserSettings {
  recId: number;
  userId: string;
  theme: string;
  language: string;
  pageSize: number;
  notificationEnabled: boolean;
  dashboardLayout: string;
}

export interface SettingsRepository {
  getGlobal: (signal?: AbortSignal) => Promise<GlobalSettings>;
  updateGlobal: (settings: GlobalSettings) => Promise<GlobalSettings>;
  getUser: (signal?: AbortSignal) => Promise<UserSettings>;
  updateUser: (settings: UserSettings) => Promise<UserSettings>;
}
