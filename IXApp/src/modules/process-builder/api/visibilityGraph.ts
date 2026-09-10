import type { BuilderControl } from '../types/processBuilderTypes';

export function validateVisibilityCycles(controls: BuilderControl[]): void {
  const byId = new Map(controls.map((control) => [control.id, control]));
  const optionTargets = new Set(controls.flatMap((control) =>
    (control.optionFeatureConfigurations ?? []).flatMap((feature) =>
      feature.showOtherControls ? feature.visibleControlIds : [])));
  const finished = new Set<string>();
  for (const control of controls) {
    const path: string[] = [];
    const positions = new Map<string, number>();
    let id: string | undefined = control.id;
    while (id && byId.has(id) && !finished.has(id)) {
      const cycleStart = positions.get(id);
      if (cycleStart !== undefined) {
        const cycle = path.slice(cycleStart);
        if (!cycle.some((member) => optionTargets.has(member)))
          throw new Error(`Visibility conditions form a closed cycle: ${cycle.map((member) => byId.get(member)!.label).join(' → ')}.`);
        break;
      }
      positions.set(id, path.length);
      path.push(id);
      id = byId.get(id)?.visibilityCondition?.variableId;
    }
    path.forEach((member) => finished.add(member));
  }
}
