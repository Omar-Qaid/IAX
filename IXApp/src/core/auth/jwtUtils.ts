interface JwtPayload {
  exp?: number;
}

const decodeBase64Url = (value: string): string => {
  const normalized = value.replace(/-/g, '+').replace(/_/g, '/');
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=');
  return globalThis.atob(padded);
};

export const getTokenExpiration = (token: string): number | null => {
  try {
    const payload = token.split('.')[1];
    if (!payload) return null;
    const decoded = JSON.parse(decodeBase64Url(payload)) as JwtPayload;
    return typeof decoded.exp === 'number' ? decoded.exp * 1000 : null;
  } catch {
    return null;
  }
};

export const getTokenLifetimeSeconds = (token: string): number => {
  const expiresAt = getTokenExpiration(token);
  return expiresAt ? Math.max(0, Math.floor((expiresAt - Date.now()) / 1000)) : 0;
};
