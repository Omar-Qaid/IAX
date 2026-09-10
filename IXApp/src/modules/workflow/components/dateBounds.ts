/** Inclusive calendar-day bounds. Relative dates use the UTC calendar on both client and server. */
export function resolveDateBound(expression: string, now = new Date()): string | null {
  const text = expression.trim().toLowerCase();
  const fixed = /^(\d{4})-(\d{2})-(\d{2})$/.exec(text);
  if (fixed) {
    const date = new Date(`${text}T00:00:00.000Z`);
    return Number(fixed[1]) >= 1 && Number.isFinite(date.getTime()) && date.toISOString().slice(0, 10) === text ? text : null;
  }
  const relative = /^today(?:\s*([+-])\s*(\d{1,4})\s*([dmy]))?$/.exec(text);
  if (!relative) return null;
  const date = new Date(now.toISOString().slice(0, 10) + 'T00:00:00.000Z');
  const amount = Number(relative[2] ?? 0) * (relative[1] === '-' ? -1 : 1);
  if (relative[3] === 'd') date.setUTCDate(date.getUTCDate() + amount);
  else if (relative[3]) {
    const day = date.getUTCDate();
    date.setUTCDate(1);
    date.setUTCMonth(date.getUTCMonth() + amount * (relative[3] === 'y' ? 12 : 1));
    const end = new Date(date.getTime());
    end.setUTCMonth(end.getUTCMonth() + 1);
    end.setUTCDate(0);
    date.setUTCDate(Math.min(day, end.getUTCDate()));
  }
  return date.getUTCFullYear() >= 1 && date.getUTCFullYear() <= 9999 ? date.toISOString().slice(0, 10) : null;
}

export function dateBoundValid(type: string, value: string, expression: string, now = new Date()): boolean {
  const match = /^(\d{4}-\d{2}-\d{2})(?:T(?:[01]\d|2[0-3]):[0-5]\d(?::[0-5]\d(?:\.\d{1,7})?)?)?$/.exec(value);
  const actual = match ? resolveDateBound(match[1], now) : null;
  const bound = resolveDateBound(expression, now);
  return !!actual && !!bound && (type.toLowerCase() === 'mindate' ? actual >= bound : actual <= bound);
}
