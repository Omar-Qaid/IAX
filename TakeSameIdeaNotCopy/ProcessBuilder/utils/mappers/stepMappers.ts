import type { WfStep } from '../../../../types';
import type { Step } from '../../types';
import { uid } from '../uid';

// ============================================================
// Step DTO ↔ Local Model Mappers
// ============================================================

export const mapWfStepToLocal = (dto: WfStep): Step => {
    let ext: any = {};
    try { ext = dto.extendedProperties ? JSON.parse(dto.extendedProperties) : {}; } catch { /* ignore */ }
    return {
        id: uid(),
        serverId: dto.id,
        code: dto.code ?? '',
        name: dto.name ?? '',
        nameAR: dto.nameAR ?? '',
        order: dto.sortOrder ?? 1,
        score: Number(dto.score) || 0,
        autoPassingHrs: dto.autoPassingHrs ?? 0,
        allMandatory: !!dto.allMandatory,
        sysField: !!dto.sysField,
        isActive: dto.isActive !== false,
        status: dto.isActive ? 'active' : 'pending',
        assignedUsers: ext.assignedUsers ?? '',
        assignedRoles: ext.assignedRoles ?? '',
        condition: ext.condition,
        activities: [],
        dirty: false,
    };
};

export const mapLocalToWfStep = (s: Step, processId: number): Omit<WfStep, 'id'> & { id?: number } => ({
    id: s.serverId,
    code: s.code || `STEP${String(s.order).padStart(3, '0')}`,
    name: s.name,
    nameAR: s.nameAR || s.name,
    processId: processId,
    sortOrder: s.order,
    score: s.score ?? 0,
    autoPassingHrs: s.autoPassingHrs ?? 0,
    allMandatory: !!s.allMandatory,
    sysField: !!s.sysField,
    isActive: s.isActive !== false,
    isDeleted: false,
    extendedProperties: JSON.stringify({
        assignedUsers: s.assignedUsers,
        assignedRoles: s.assignedRoles,
        condition: s.condition ?? null,
    }),
} as any);
