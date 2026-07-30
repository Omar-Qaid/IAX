export function getEnumOptions<T extends Record<string, string>>(
  enumObj: T
): Array<{ value: T[keyof T]; label: string }> {
  return Object.entries(enumObj).map(([key, value]) => ({
    value: value as T[keyof T],
    label: key.replace(/([A-Z])/g, ' $1').trim(),
  }));
}
