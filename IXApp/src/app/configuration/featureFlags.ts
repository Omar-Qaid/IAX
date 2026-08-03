export const featureFlags = {
  advancedFiltering: true,
  gridPersonalization: true,
  inlineEditing: true,
  rightUtilityRail: true,
} as const;
export type FeatureFlag = keyof typeof featureFlags;
export const isFeatureEnabled = (flag: FeatureFlag) => featureFlags[flag];
