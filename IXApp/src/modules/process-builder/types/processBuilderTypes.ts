export type BuilderNode =
  | { kind: 'process' }
  | { kind: 'variable'; id: string }
  | { kind: 'requestControl'; id: string }
  | { kind: 'step'; id: string }
  | { kind: 'activity'; stepId: string; id: string }
  | { kind: 'control'; stepId: string; activityId: string; id: string }
  | { kind: 'transition'; id: string };

export type BuilderDataType = 'text' | 'number' | 'boolean' | 'date' | 'object';
export type BuilderControlType = 'digits' | 'text' | 'longtext' | 'date' | 'time' | 'url' | 'dropdown-db' | 'dropdown-manual' | 'checkbox' | 'checkboxlist' | 'radiobuttonlist' | 'table' | 'label' | 'employeesearch' | 'employeeid' | 'file' | 'showroom' | 'signature' | 'location' | 'advertiser';
export type BuilderActivityType = 'approval' | 'review' | 'data-entry' | 'api' | 'notification';
export type BuilderOperator = '=' | '!=' | '>' | '<' | '>=' | '<=' | 'contains' | 'isEmpty';
export type BuilderActionType = 'approve' | 'reject' | 'return' | 'escalate';

export interface BuilderVariable { id: string; code: string; name: string; description: string; dataType: BuilderDataType; sortOrder: number; required: boolean; active: boolean; scope: 'process' | 'step' | 'activity' | 'global'; defaultValue: string }
export type BuilderValidationType = 'required' | 'minLength' | 'maxLength' | 'exactLength' | 'minValue' | 'maxValue' | 'range' | 'regex' | 'startsWith' | 'endsWith' | 'contains' | 'email' | 'url' | 'phone' | 'saudiMobile' | 'saudiNationalId' | 'saudiIban' | 'taxNumber' | 'passport' | 'fileExtensions' | 'fileSize' | 'minSelected' | 'maxSelected' | 'custom' | 'crossField';
export interface BuilderValidation { id: string; type: BuilderValidationType; value: string; secondaryValue: string; operator: string; mask: string; message: string; messageAR: string; severity: 'Error' | 'Warning' | 'Information'; sortOrder: number; active: boolean }
export interface BuilderCondition { variableId: string; operator: BuilderOperator; value: string }
export interface BuilderControl { id: string; code: string; label: string; labelAR: string; type: BuilderControlType; required: boolean; readOnly: boolean; visible: boolean; uniqueKey: boolean; usedAsCriteria: boolean; defaultValue: string; options: string[]; validations: BuilderValidation[]; visibilityCondition: BuilderCondition | null }
export interface BuilderActivityAction { id: string; type: BuilderActionType; label: string; nextStepId: string; condition: BuilderCondition | null }
export interface BuilderActivity { id: string; code: string; name: string; type: BuilderActivityType; performer: string; assignmentMode: 'any' | 'all' | 'round-robin'; active: boolean; required: boolean; mandatoryDocs: boolean; autoPassEnabled: boolean; autoPassingHours: number; controls: BuilderControl[]; actions: BuilderActivityAction[]; validations: BuilderValidation[]; condition: BuilderCondition | null; config: { apiMethod: 'GET' | 'POST' | 'PUT' | 'DELETE'; apiUrl: string; notifyEmails: string } }
export interface BuilderStep { id: string; code: string; name: string; order: number; score: number; autoPassingHours: number; allMandatory: boolean; active: boolean; systemField: boolean; condition: BuilderCondition | null; activities: BuilderActivity[] }
export interface BuilderTransition { id: string; name: string; sourceStepId: string; targetStepId: string; variableId: string; operator: BuilderOperator; value: string; sortOrder: number; active: boolean; triggerSource: 'none' | 'requestControl' | 'activityControl'; triggerId: string }
export interface ProcessBuilderDocument {
  id: string;
  code: string;
  name: string;
  description: string;
  categoryId: string;
  priorityId: string;
  processType: string;
  score: number;
  canRepeat: boolean;
  mandatoryDocs: boolean;
  active: boolean;
  variables: BuilderVariable[];
  requestControls: BuilderControl[];
  steps: BuilderStep[];
  transitions: BuilderTransition[];
}
