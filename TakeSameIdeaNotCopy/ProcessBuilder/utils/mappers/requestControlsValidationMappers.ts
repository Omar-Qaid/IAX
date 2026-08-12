import type { WfRequestControlsValidation } from '../../../../types';
import type { RequestControlsValidation, RequestControl } from '../../types';
import { uid } from '../uid';

export const mapWfRequestControlsValidationToLocal = (
    dto: WfRequestControlsValidation,
    requestControls: RequestControl[]
): RequestControlsValidation => {
    const localRequestControl = requestControls.find(c => c.serverId === dto.requestControlId);

    return {
        id: uid(),
        serverId: dto.id,
        requestControlId: localRequestControl?.id ?? '',
        validationType: dto.validationType,
        validationExpression: dto.validationExpression,
        operator: dto.operator || '',
        value: dto.value || '',
        maskInput: dto.maskInput || '',
        errorMessageAr: dto.errorMessageAr,
        errorMessageEn: dto.errorMessageEn,
        severity: dto.severity,
        sortOrder: dto.sortOrder || 0,
        isActive: dto.isActive !== false,
        dirty: false,
    };
};

export const mapLocalToWfRequestControlsValidation = (
    v: RequestControlsValidation,
    requestControls: RequestControl[]
): Omit<WfRequestControlsValidation, 'id'> & { id?: number } => {
    const requestControl = requestControls.find(c => c.id === v.requestControlId);

    return {
        id: v.serverId,
        requestControlId: requestControl?.serverId || 0,
        validationType: v.validationType,
        validationExpression: v.validationExpression,
        operator: v.operator,
        value: v.value,
        maskInput: v.maskInput,
        errorMessageAr: v.errorMessageAr,
        errorMessageEn: v.errorMessageEn,
        severity: v.severity,
        sortOrder: v.sortOrder || 0,
        isActive: v.isActive,
    };
};
