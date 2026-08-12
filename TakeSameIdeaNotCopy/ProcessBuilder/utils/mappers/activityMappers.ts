import type { WfActivity, WfActivityType } from '../../../../types';
import type { Activity, ActivityType } from '../../types';
import { uid } from '../uid';

// ============================================================
// Activity DTO ↔ Local Model Mappers
// ============================================================

export const mapWfActivityToLocal = (dto: WfActivity, activityTypes: WfActivityType[]): Activity => {
    const rawExt = dto.extendedProperties ?? '';
    let ext: any = {};
    try {
        ext = rawExt ? JSON.parse(rawExt) : {};
    } catch { /* ignore */ }

    const inferLocalType = (): ActivityType => {
        const at = activityTypes.find((t) => t.id === dto.activityTypeId);
        const code = (at?.code ?? '').toLowerCase();
        if (code.includes('approval')) return 'approval';
        if (code.includes('review')) return 'review';
        if (code.includes('data')) return 'data-entry';
        if (code.includes('api')) return 'api';
        if (code.includes('notification') || code.includes('email') || code.includes('sms')) return 'notification';
        return 'approval';
    };

    return {
        id: uid(),
        serverId: dto.id,
        code: dto.code ?? '',
        type: inferLocalType(),
        name: dto.name ?? '',
        nameAR: dto.nameAR ?? '',
        isActive: dto.isActive !== false,
        activityTypeId: dto.activityTypeId || '',
        performerId: dto.performerId || '',
        score: dto.score ?? 0,
        sysNotificationTemplateId: dto.sysNotificationTemplateId ?? '',
        alertingBySystem: !!dto.alertingBySystem,
        alertingByEmail: !!dto.alertingByEmail,
        alertingBySms: !!dto.alertingBySms,
        alertingByWhatsApp: !!dto.alertingByWhatsApp,
        showPreviousSteps: !!dto.showPreviousSteps,
        showPreviousDocs: !!dto.showPreviousDocs,
        mandatoryDocs: !!dto.mandatoryDocs,
        autoPassEnabled: !!dto.autoPassEnabled,
        autoPassingHrs: dto.autoPassingHrs ?? 0,
        dirty: false,
        sortOrder: dto.sortOrder ?? 0,

        // Local JSON fields (controls will be overridden from WfActivityControls if available):
        controls: ext.controls ?? [],
        actions: ext.actions ?? [],
        condition: ext.condition,
        validations: ext.validations ?? [],
        assignedUsers: ext.assignedUsers ?? '',
        assignedRoles: ext.assignedRoles ?? '',
        assignmentMode: ext.assignmentMode ?? 'any',
        config: ext.config ?? {},
    };
};

export const mapLocalToWfActivity = (a: Activity, stepServerId: number): Omit<WfActivity, 'id'> & { id?: number } => {
    // Controls are now stored in WfActivityControls table — not in extendedProperties.
    const extendedProperties = JSON.stringify({
        actions: a.actions,
        condition: a.condition,
        validations: a.validations,
        assignedUsers: a.assignedUsers,
        assignedRoles: a.assignedRoles,
        assignmentMode: a.assignmentMode,
        config: a.config,
    });

    const code = a.code || '';

    let name = a.name?.trim();
    let nameAR = a.nameAR?.trim();
    if (!name && nameAR) name = nameAR;
    if (!nameAR && name) nameAR = name;
    if (!name) name = 'Workflow Activity';
    if (!nameAR) nameAR = 'Workflow Activity';

    return {
        id: a.serverId,
        code,
        name,
        nameAR,
        stepId: stepServerId,
        activityTypeId: Number(a.activityTypeId) || 0,
        performerId: Number(a.performerId) || 0,
        score: a.score ?? 0,
        sysNotificationTemplateId: a.sysNotificationTemplateId === '' || a.sysNotificationTemplateId == null
            ? null
            : Number(a.sysNotificationTemplateId),
        alertingBySystem: !!a.alertingBySystem,
        alertingByEmail: !!a.alertingByEmail,
        alertingBySms: !!a.alertingBySms,
        alertingByWhatsApp: !!a.alertingByWhatsApp,
        showPreviousSteps: !!a.showPreviousSteps,
        showPreviousDocs: !!a.showPreviousDocs,
        mandatoryDocs: !!a.mandatoryDocs,
        autoPassEnabled: !!a.autoPassEnabled,
        autoPassingHrs: a.autoPassingHrs ?? 0,

        extendedProperties,
        isActive: a.isActive !== false,
        isDeleted: false,
        sortOrder: a.sortOrder ?? 0,
    } as any;
};
