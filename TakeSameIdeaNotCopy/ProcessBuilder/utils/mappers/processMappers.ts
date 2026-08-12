import type { WfProcess } from '../../../../types';
import type { ProcessInfo } from '../../types';

// ============================================================
// Process DTO ↔ Local Model Mappers
// ============================================================

export const mapDtoToInfo = (dto: WfProcess): ProcessInfo => ({
    id: dto.id,
    code: dto.code ?? '',
    name: dto.name ?? '',
    nameAR: dto.nameAR ?? '',
    description: dto.description ?? '',
    descriptionAR: dto.descriptionAR ?? '',
    categoryId: dto.categoryId ?? '',
    priorityId: dto.priorityId ?? '',
    processType: (dto as any).processType ?? 'Process',
    score: dto.sortOrder ?? (dto as any).score ?? 100,
    canRepeat: !!dto.canRepeat,
    mandatoryDocs: !!dto.mandatoryAttachments,
    isActive: dto.isActive ?? true,
    isDeleted: dto.isDeleted ?? false,
});

export const mapInfoToDto = (info: ProcessInfo): Omit<WfProcess, 'id'> & { id?: number } => ({
    id: info.id,
    code: info.code,
    name: info.name,
    nameAR: info.nameAR,
    description: info.description,
    descriptionAR: info.descriptionAR,
    categoryId: Number(info.categoryId) || 0,
    priorityId: Number(info.priorityId) || 0,
    processType: info.processType,
    score: info.score,
    sortOrder: info.score,
    canRepeat: info.canRepeat,
    mandatoryAttachments: info.mandatoryDocs,
    isActive: info.isActive,
    isDeleted: info.isDeleted,
} as any);
