export type RouteParameter = string | number | boolean;

export function buildPath(template: string, parameters: Record<string, RouteParameter | null | undefined> = {}) {
  return Object.entries(parameters).reduce((path, [key, value]) => value == null ? path : path.replace(`:${key}`, encodeURIComponent(String(value))), template);
}

export function withQuery(path: string, query: Record<string, RouteParameter | null | undefined | RouteParameter[]>) {
  const search = new URLSearchParams();
  Object.entries(query).forEach(([key, raw]) => {
    const values = Array.isArray(raw) ? raw : [raw];
    values.forEach(value => { if (value != null) search.append(key, String(value)); });
  });
  const serialized = search.toString();
  return serialized ? `${path}?${serialized}` : path;
}

export function normalizePath(path: string) { const value = `/${path}`.replace(/\/{2,}/g, '/'); return value.length > 1 ? value.replace(/\/$/, '') : value; }

export const routeHelpers = { buildPath, withQuery, normalizePath };
