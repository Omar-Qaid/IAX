import type { ActionDefinition } from './types';
export type ActionDefinitionFactory<TContext = void> = (context: TContext) => ActionDefinition;
export const defineAction = <T extends ActionDefinition>(action: T): T => action;
export const defineActions = <T extends readonly ActionDefinition[]>(actions: T): T => actions;
export const actionDefinitions = { defineAction, defineActions };
