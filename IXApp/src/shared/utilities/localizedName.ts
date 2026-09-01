export interface LocalizedNameValue {
  name?: string | null;
  nameAlias?: string | null;
}

export const localizedName = (
  value: LocalizedNameValue | null | undefined,
  isRtl: boolean
): string => {
  const name = value?.name?.trim() ?? '';
  if (!isRtl) return name;
  return value?.nameAlias?.trim() || name;
};
