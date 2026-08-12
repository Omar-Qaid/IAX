// =====================================================================
// Shared types for the Process Builder feature
// =====================================================================

export type DataType = 'string' | 'number' | 'boolean' | 'date' | 'object' | (string & {});

export type ControlType =
    | 'digits' | 'text' | 'longtext' | 'date' | 'time' | 'url'
    | 'dropdown-db' | 'dropdown-manual'
    | 'checkbox' | 'checkboxlist' | 'radiobuttonlist'
    | 'table' | 'label'
    | 'employeesearch' | 'employeeid'
    | 'file' | 'showroom' | 'signature' | 'location' | 'advertiser'
    // legacy / aux types kept for back-compat with existing local data
    | 'textbox' | 'dropdown' | 'lookup' | 'grid' | 'color' | 'calendar' | 'dropdownlist' | 'combobox'
    | (string & {});

export interface DropdownOptionX { ar: string; en: string; value: string }

export type ActivityType = 'approval' | 'review' | 'data-entry' | 'api' | 'notification' | (string & {});
export type ActionType = 'approve' | 'reject' | 'return' | 'escalate' | (string & {});
export type ConditionLogic = 'AND' | 'OR';
export type Operator = '=' | '!=' | '>' | '<' | '>=' | '<=' | 'contains' | 'isEmpty';

export type VariableScope = 'process' | 'step' | 'activity' | 'global' | (string & {});

/** Mirrors WfVariables columns. */
export interface ProcessVariable {
    id: string;
    serverId?: number;
    code: string;
    name: string;
    nameAR: string;
    description: string;
    descriptionAR: string;
    dataTypeId: number | '';
    sortOrder: number;
    isActive: boolean;
    dataType: DataType;
    defaultValue: string;
    required: boolean;
    scope: VariableScope;
    dirty?: boolean;
}

export interface ValidationRule {
    id: string;
    type: 'required' | 'min' | 'max' | 'regex' | 'email';
    value?: string;
    message: string;
}

export interface ConditionExpr {
    id: string;
    field: string;
    operator: Operator;
    value: string;
}

export interface ConditionGroup {
    logic: ConditionLogic;
    expressions: ConditionExpr[];
}

export interface RequestControl {
    id: string;
    serverId?: number;
    code?: string;
    controlId?: number;
    labelAR?: string;
    mandatory?: boolean;
    uniqueKey?: boolean;
    score?: number;
    usedAsCriteria?: boolean;
    sortOrder?: number;
    extendedProperties?: string;
    dirty?: boolean;
    label: string;
    name: string;
    nameAR?: string;
    type: ControlType;
    required: boolean;
    readOnly: boolean;
    visible: boolean;
    defaultValue: string;
    options?: string[];
    optionsX?: DropdownOptionX[];
    gridRows?: string[][];
    color?: string;
    mask?: string;
    validations: ValidationRule[];
    visibilityRule?: ConditionGroup;
    bindVariableId?: string;
}

export interface ActivityAction {
    id: string;
    type: ActionType;
    label: string;
    nextStepId?: string;
    condition?: ConditionGroup;
}

export interface Activity {
    id: string;
    serverId?: number;
    code?: string;
    name: string;
    nameAR?: string;
    type: ActivityType;
    isActive?: boolean;
    dirty?: boolean;
    activityTypeId?: number | '';
    performerId?: number | '';
    score?: number;
    sysNotificationTemplateId?: number | '';
    alertingBySystem?: boolean;
    alertingByEmail?: boolean;
    alertingBySms?: boolean;
    alertingByWhatsApp?: boolean;
    showPreviousSteps?: boolean;
    showPreviousDocs?: boolean;
    mandatoryDocs?: boolean;
    autoPassEnabled?: boolean;
    autoPassingHrs?: number;
    controls: RequestControl[];
    actions: ActivityAction[];
    condition?: ConditionGroup;
    validations: ValidationRule[];
    assignedUsers: string;
    assignedRoles: string;
    assignmentMode: 'any' | 'all' | 'round-robin';
    config: {
        notifyEmails?: string;
        apiUrl?: string;
        apiMethod?: 'GET' | 'POST' | 'PUT' | 'DELETE';
    };
    deletedControlServerIds?: number[];
    importance?: 'Low' | 'Medium' | 'High' | '';
    isRequired?: boolean;
    sortOrder?: number;
}

export type StepStatus = 'pending' | 'active' | 'completed' | 'skipped';

export interface Step {
    id: string;
    serverId?: number;
    code?: string;
    name: string;
    nameAR?: string;
    order: number;
    score?: number;
    autoPassingHrs?: number;
    allMandatory?: boolean;
    sysField?: boolean;
    isActive?: boolean;
    status: StepStatus;
    assignedUsers: string;
    assignedRoles: string;
    condition?: ConditionGroup;
    activities: Activity[];
    dirty?: boolean;
}

/** Mirrors WfProcesses columns. */
export interface ProcessInfo {
    id?: number;
    code: string;
    name: string;
    nameAR: string;
    description: string;
    descriptionAR: string;
    categoryId: number | '';
    priorityId: number | '';
    processType: string;
    score: number;
    canRepeat: boolean;
    mandatoryDocs: boolean;
    isActive: boolean;
    isDeleted: boolean;
}

export type SelectedNode =
    | { kind: 'process' }
    | { kind: 'variable'; id: string }
    | { kind: 'requestControl'; id: string }
    | { kind: 'step'; id: string }
    | { kind: 'activity'; stepId: string; id: string }
    | { kind: 'control'; stepId: string; activityId: string; id: string }
    | { kind: 'transition'; id: string };

export interface Transition {
    id: string;
    serverId?: number;
    processId?: number;
    activityId?: string;
    requestControlId?: string;
    activityControlId?: string;
    variableId: string;
    operatorId: number | '';
    value: string;
    stepId: string;
    sortOrder: number;
    isActive: boolean;
    dirty?: boolean;
}

export interface RequestControlsValidation {
    id: string;
    serverId?: number;
    requestControlId: string;
    activityControlId?: string;
    validationType: string;
    validationExpression?: string;
    operator?: string;
    value?: string;
    maskInput?: string;
    errorMessageAr: string;
    errorMessageEn: string;
    severity: string;
    sortOrder: number;
    isActive: boolean;
    dirty?: boolean;
}


