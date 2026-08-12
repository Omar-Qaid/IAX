import type { WfActivityControl } from '../../../../types';
import type { ControlType, DropdownOptionX, RequestControl } from '../../types';
import { uid } from '../uid';
import { CONTROL_PALETTE } from '../palette';
import {
    buildDropdownXml, buildGridXml, parseDropdownXml, parseGridXml,
} from '../xmlHelpers';

// ============================================================
// Shared Constants
// ============================================================

const COLOR_HEX_REGEX = /^#[0-9a-f]{3,8}$/i;

/**
 * Maps raw catalog type strings to normalized ControlType values.
 * Order matters — more specific patterns must come before general ones.
 */
const CATALOG_TYPE_MAP: [RegExp, ControlType][] = [
    [/digits/, 'digits'],
    [/longtext|long_text/, 'longtext'],
    [/employeesearch/, 'employeesearch'],
    [/employeeid/, 'employeeid'],
    [/showroom/, 'showroom'],
    [/signature/, 'signature'],
    [/location/, 'location'],
    [/advertiser/, 'advertiser'],
    [/checkboxlist/, 'checkboxlist'],
    [/checkbox/, 'checkbox'],
    [/radiobuttonlist|radiobutton/, 'radiobuttonlist'],
    [/dropdownlist.*db|fromdatabase|filldatabase/, 'dropdown-db'],
    [/dropdownlist.*manual|fillmanually/, 'dropdown-manual'],
    [/dropdownlist|dropdown/, 'dropdown-manual'],
    [/combobox/, 'dropdown-manual'],
    [/calendar/, 'date'],
    [/date/, 'date'],
    [/time/, 'time'],
    [/url/, 'url'],
    [/file/, 'file'],
    [/table/, 'table'],
    [/grid/, 'table'],
    [/color/, 'color'],
];

// ============================================================
// Shared Utilities
// ============================================================

/**
 * Infers a DataType string from a WfDataType lookup list.
 */
export const inferDataType = (id: number | '', dataTypes: import('../../../../types').WfDataType[]): import('../../types').DataType => {
    if (id === '' || id === 0) return 'string';
    const dt = dataTypes.find((d) => d.id === id);
    const n = (dt?.code ?? dt?.name ?? '').toLowerCase();
    if (n.includes('num') || n.includes('int') || n.includes('dec') || n.includes('float')) return 'number';
    if (n.includes('bool')) return 'boolean';
    if (n.includes('date') || n.includes('time')) return 'date';
    if (n.includes('obj') || n.includes('json')) return 'object';
    return 'string';
};

/**
 * Resolves a catalog ControlType string from raw catalog metadata.
 * Used by request control mappers to infer the display type from the control catalog.
 */
export const inferControlTypeFromCatalog = (
    catalog: any,
    ext: any,
    optionsX?: DropdownOptionX[],
    gridRows?: string[][],
    color?: string,
): ControlType => {
    const raw = ((catalog as any)?.controlType ?? catalog?.code ?? catalog?.name ?? '').toLowerCase().replace(/\s+/g, '');
    for (const [re, t] of CATALOG_TYPE_MAP) {
        if (re.test(raw)) return t;
    }
    if (optionsX) {
        const t = ext.type as ControlType;
        return (t === 'checkboxlist' || t === 'radiobuttonlist') ? t : 'dropdown-manual';
    }
    if (gridRows) return 'table';
    if (color) return 'color';
    return (ext.type as ControlType) ?? 'text';
};

/**
 * Parses the `extendedProperties` field from a control DTO into its component parts.
 */
const parseControlExtProps = (rawExt: string) => {
    let ext: any = {};
    let optionsX: DropdownOptionX[] | undefined;
    let gridRows: string[][] | undefined;
    let color: string | undefined;

    const trimmed = rawExt.trim();
    if (trimmed.startsWith('{')) {
        try { ext = JSON.parse(trimmed); } catch { /* ignore */ }

        if (ext.rawXml) {
            if (ext.rawXml.startsWith('<Data')) optionsX = parseDropdownXml(ext.rawXml);
            if (ext.rawXml.startsWith('<Root')) gridRows = parseGridXml(ext.rawXml);
        }
        if (ext.color) color = ext.color;
    } else if (trimmed.startsWith('<Data')) {
        optionsX = parseDropdownXml(trimmed);
    } else if (trimmed.startsWith('<Root')) {
        gridRows = parseGridXml(trimmed);
    } else if (COLOR_HEX_REGEX.test(trimmed)) {
        color = trimmed;
    }

    return { ext, optionsX, gridRows, color };
};

/**
 * Builds the `extendedProperties` string for a control DTO from local state.
 */
const buildControlExtProps = (c: RequestControl): string => {
    const isManual = c.type === 'dropdown' || c.type === 'dropdown-manual'
        || c.type === 'checkboxlist' || c.type === 'radiobuttonlist';
    const isTable = c.type === 'table' || c.type === 'grid';

    let rawXml = '';
    if (isManual) rawXml = buildDropdownXml(c.optionsX ?? []);
    else if (isTable) rawXml = buildGridXml(c.gridRows ?? []);

    return JSON.stringify({
        type: c.type,
        name: c.name,
        code: c.code,
        defaultValue: c.defaultValue,
        readOnly: c.readOnly,
        visible: c.visible,
        validations: c.validations,
        visibilityRule: c.visibilityRule,
        bindVariableId: c.bindVariableId,
        color: c.color,
        rawXml
    });
};

// ============================================================
// Activity Control DTO ↔ Local Model Mappers
// ============================================================

export const mapWfActivityControlToLocal = (dto: WfActivityControl): RequestControl => {
    const { ext, optionsX, gridRows, color } = parseControlExtProps(dto.extendedProperties ?? '');
    const inferType = (): ControlType => {
        if (optionsX) {
            const t = ext.type as ControlType;
            return (t === 'checkboxlist' || t === 'radiobuttonlist') ? t : 'dropdown-manual';
        }
        if (gridRows) return 'table';
        if (color) return 'color';
        return (ext.type as ControlType) ?? 'text';
    };
    return {
        id: uid(),
        serverId: dto.id,
        controlId: dto.controlId,
        label: dto.name ?? ext.label ?? '',
        labelAR: dto.nameAR ?? ext.labelAR ?? '',
        name: ext.name || dto.name || '',
        nameAR: ext.nameAR || dto.nameAR || '',
        code: dto.code || ext.code || '',
        type: inferType(),
        required: !!dto.mandatory,
        mandatory: !!dto.mandatory,
        uniqueKey: !!dto.uniqueKey,
        score: dto.score ?? 0,
        usedAsCriteria: !!dto.usedAsCriteria,
        sortOrder: dto.sortOrder ?? 0,
        extendedProperties: dto.extendedProperties,
        defaultValue: ext.defaultValue ?? '',
        readOnly: ext.readOnly ?? false,
        visible: dto.isActive !== false,
        options: undefined,
        optionsX,
        gridRows,
        color,
        validations: ext.validations ?? [],
        visibilityRule: ext.visibilityRule,
        bindVariableId: ext.bindVariableId,
        dirty: false,
    };
};

export const mapLocalToWfActivityControl = (
    c: RequestControl,
    activityId: number,
    processId: number,
    index: number,
): Omit<WfActivityControl, 'id'> & { id?: number } => {
    const extendedProperties = buildControlExtProps(c);
    const fallbackLabel = CONTROL_PALETTE.find((p) => p.type === c.type)?.label ?? c.type;
    const label = c.label?.trim() || fallbackLabel;
    const labelAR = c.labelAR?.trim() || label;

    const name = label;
    const nameAR = labelAR;

    return {
        id: c.serverId,
        activityId,
        relatedObjectId: processId,
        controlId: c.controlId ?? 0,
        code: c.code || `CTRL_${c.id.substring(0, 6)}`,
        name,
        nameAR,
        controlLabel: label,
        controlLabelAR: labelAR,
        mandatory: !!(c.mandatory ?? c.required),
        uniqueKey: !!c.uniqueKey,
        score: c.score ?? 0,
        usedAsCriteria: !!c.usedAsCriteria,
        usedInSearch: false,
        sortOrder: c.sortOrder ?? (index + 1) * 10,
        extendedProperties,
        isActive: c.visible !== false,
        isDeleted: false,
    } as any;
};

// ============================================================
// Request Control DTO ↔ Local Model Mappers
// ============================================================

export const mapWfRequestControlToLocal = (dto: any, wfControls: any[]): RequestControl => {
    const { ext, optionsX, gridRows, color } = parseControlExtProps(dto.extendedProperties ?? '');
    const catalog = wfControls.find((c) => c.id === dto.controlId);

    const inferType = (): ControlType => inferControlTypeFromCatalog(catalog, ext, optionsX, gridRows, color);
    return {
        id: uid(),
        serverId: dto.id,
        controlId: dto.controlId,
        label: dto.name ?? ext.label ?? '',
        labelAR: dto.nameAR ?? ext.labelAR ?? '',
        name: ext.name || dto.name || '',
        nameAR: ext.nameAR || dto.nameAR || '',
        code: dto.code || ext.code || '',
        type: inferType(),
        required: !!dto.mandatory,
        mandatory: !!dto.mandatory,
        uniqueKey: !!dto.uniqueKey,
        score: dto.score ?? 0,
        usedAsCriteria: !!dto.usedAsCriteria,
        sortOrder: dto.sortOrder ?? 0,
        extendedProperties: dto.extendedProperties,
        defaultValue: ext.defaultValue ?? '',
        readOnly: ext.readOnly ?? false,
        visible: dto.isActive !== false,
        options: undefined,
        optionsX,
        gridRows,
        color,
        validations: ext.validations ?? [],
        visibilityRule: ext.visibilityRule,
        bindVariableId: ext.bindVariableId,
        dirty: false,
    };
};

export const mapLocalToWfRequestControl = (
    c: RequestControl,
    processId: number,
    index: number,
): any => {
    const extendedProperties = buildControlExtProps(c);
    const fallbackLabel = CONTROL_PALETTE.find((p) => p.type === c.type)?.label ?? c.type;
    const label = c.label?.trim() || fallbackLabel;
    const labelAR = c.labelAR?.trim() || label;

    const name = label;
    const nameAR = labelAR;

    return {
        id: c.serverId,
        processId,
        controlId: c.controlId ?? 0,
        code: c.code || `CTRL_${c.id.substring(0, 6)}`,
        name,
        nameAR,
        controlLabel: label,
        controlLabelAR: labelAR,
        mandatory: !!(c.mandatory ?? c.required),
        uniqueKey: !!c.uniqueKey,
        score: c.score ?? 0,
        usedAsCriteria: !!c.usedAsCriteria,
        usedInSearch: false,
        sortOrder: c.sortOrder ?? (index + 1) * 10,
        extendedProperties,
        isActive: c.visible !== false,
        isDeleted: false,
    };
};

// ============================================================
// Control Catalog Utilities
// ============================================================

export const findControlIdByType = (type: string, wfControls: any[]): number | undefined => {
    const norm = (s?: string) => (s ?? '').toLowerCase().replace(/[\s_-]/g, '');
    const target = norm(type);
    return wfControls?.find((c: any) => {
        if (norm(c.code) === target || norm(c.name) === target) return true;
        // Also match via CATALOG_TYPE_MAP so 'dropdown-manual' finds 'DropDownList Fill Manually'
        const raw = norm(c.controlType ?? c.code ?? c.name ?? '');
        for (const [re, t] of CATALOG_TYPE_MAP) {
            if (re.test(raw) && norm(t) === target) return true;
        }
        return false;
    })?.id;
};

// ============================================================
// Variable DTO ↔ Local Model Mappers
// ============================================================

export const mapWfVariableToLocal = (dto: any, dataTypes: any[]): import('../../types').ProcessVariable => ({
    id: uid(),
    serverId: dto.id,
    code: dto.code ?? '',
    name: dto.name ?? '',
    nameAR: dto.nameAR ?? '',
    description: dto.description ?? '',
    descriptionAR: dto.descriptionAR ?? '',
    dataTypeId: dto.dataTypeId ?? '',
    sortOrder: dto.sortOrder ?? 0,
    isActive: dto.isActive ?? true,
    dataType: inferDataType(dto.dataTypeId ?? '', dataTypes),
    defaultValue: '',
    required: false,
    scope: 'process',
    dirty: false,
});

export const mapLocalToWfVariable = (v: import('../../types').ProcessVariable, processId: number): any => ({
    id: v.serverId,
    code: v.code,
    name: v.name,
    nameAR: v.nameAR,
    description: v.description,
    descriptionAR: v.descriptionAR,
    dataTypeId: Number(v.dataTypeId) || 0,
    processId: processId,
    sortOrder: v.sortOrder,
    isActive: v.isActive,
    isDeleted: false,
});
