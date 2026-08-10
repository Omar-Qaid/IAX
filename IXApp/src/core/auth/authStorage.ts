const TOKEN_KEY = 'ixapp_auth_token';

let accessToken: string | null = null;

const getSessionStorage = (): Storage | null => {
  try {
    return globalThis.sessionStorage ?? null;
  } catch {
    return null;
  }
};

const migrateLegacyToken = (): string | null => {
  try {
    const legacyToken = globalThis.localStorage?.getItem(TOKEN_KEY) ?? null;
    globalThis.localStorage?.removeItem(TOKEN_KEY);
    globalThis.localStorage?.removeItem('ixapp_auth_user');
    return legacyToken;
  } catch {
    return null;
  }
};

export const authStorage = {
  getToken(): string | null {
    if (accessToken) return accessToken;
    accessToken = getSessionStorage()?.getItem(TOKEN_KEY) ?? migrateLegacyToken();
    if (accessToken) getSessionStorage()?.setItem(TOKEN_KEY, accessToken);
    return accessToken;
  },

  setToken(token: string): void {
    accessToken = token;
    getSessionStorage()?.setItem(TOKEN_KEY, token);
    try {
      globalThis.localStorage?.removeItem(TOKEN_KEY);
      globalThis.localStorage?.removeItem('ixapp_auth_user');
    } catch {
      // Storage may be unavailable in restricted browser contexts.
    }
  },

  removeToken(): void {
    try {
      getSessionStorage()?.removeItem(TOKEN_KEY);
      globalThis.localStorage?.removeItem(TOKEN_KEY);
      globalThis.localStorage?.removeItem('ixapp_auth_user');
    } catch {
      // Storage may be unavailable in restricted browser contexts.
    }
    accessToken = null;
  },

  clearAll(): void {
    this.removeToken();
  },
};
