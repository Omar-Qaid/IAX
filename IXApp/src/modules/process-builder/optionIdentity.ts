import type { BuilderControl } from './types/processBuilderTypes';

export function updateControlOptions(control: BuilderControl, patch: Partial<BuilderControl>): BuilderControl {
  if (!patch.options || patch.optionIds || !control.optionIds) return { ...control, ...patch };
  const indices = patch.options.length === control.options.length &&
    !patch.options.every((label) => control.options.includes(label))
    ? control.options.map((_, index) => index)
    : patch.options.map((label) => control.options.indexOf(label));
  const optionIds = indices.map((index) => index < 0 ? null : control.optionIds![index]);
  return {
    ...control, ...patch, optionIds,
    optionAliases: patch.optionAliases ?? indices.map((index) => control.optionAliases?.[index] ?? ''),
    optionScores: patch.optionScores ?? indices.map((index) => control.optionScores?.[index] ?? 0),
    optionFeatureConfigurations: patch.optionFeatureConfigurations ?? indices.map((index) =>
      control.optionFeatureConfigurations?.[index] ?? {
        requireFileUpload: false, sendAlertMessage: false, alertMessage: '', performerIds: [],
        showOtherControls: false, visibleControlIds: [],
      }),
  };
}

export function moveOptionMetadata(control: BuilderControl, from: number, to: number) {
  const move = <T,>(items: T[] | undefined) => {
    if (!items) return undefined;
    const result = [...items];
    const [item] = result.splice(from, 1);
    result.splice(to, 0, item);
    return result;
  };
  return {
    optionIds: move(control.optionIds),
    optionAliases: move(control.optionAliases),
    optionScores: move(control.optionScores),
    optionFeatureConfigurations: move(control.optionFeatureConfigurations),
  };
}
