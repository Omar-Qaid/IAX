import { describe, expect, it } from 'vitest';
import { referenceFilterFieldsFallback } from '@modules/process-builder/api/referenceFilterApi';

describe('reference filter metadata fallback', () => {
  it('provides all HcmWorker scalar fields and marks foreign keys', () => {
    const fields = referenceFilterFieldsFallback('Employee');
    expect(fields.map((item) => item.name)).toEqual(expect.arrayContaining([
      'RecId', 'PersonnelNumber', 'Person', 'GenderId', 'NationalityId',
      'HireDate', 'BirthDate', 'UserId', 'DataAreaId', 'IsActive', 'IsDeleted',
      'OrganizationAssignment.DepartmentId', 'OrganizationAssignment.HcmManagerWorkerId',
      'OrganizationAssignment.OccupationId', 'OrganizationAssignment.ValidFrom',
    ]));
    expect(fields.find((item) => item.name === 'Person')).toMatchObject({ isForeignKey: true });
    expect(fields.find((item) => item.name === 'PersonnelNumber')?.operators).toContain('contains');
  });

  it('provides HcmShowroom scalar fields and its party lookup', () => {
    const fields = referenceFilterFieldsFallback('Showroom');
    expect(fields.map((item) => item.name)).toEqual(expect.arrayContaining(['RecId', 'PersonnelNumber', 'Party']));
    expect(fields.find((item) => item.name === 'Party')).toMatchObject({ isForeignKey: true });
    expect(fields.map((item) => item.name)).toEqual(expect.arrayContaining([
      'ShowroomAssignment.HcmWorkerId', 'ShowroomAssignment.HcmShowroomId',
      'ShowroomAssignment.ValidFrom', 'ShowroomAssignment.ValidTo',
      'ShowroomAssignment.IsPrimary', 'ShowroomAssignment.RecId',
    ]));
    for (const name of ['ShowroomAssignment.HcmWorkerId', 'ShowroomAssignment.HcmShowroomId']) {
      expect(fields.find((item) => item.name === name)).toMatchObject({
        isForeignKey: true, operators: ['equals', 'notEquals', 'isEmpty', 'isNotEmpty'],
      });
    }
    expect(referenceFilterFieldsFallback('Employee').some((item) => item.name.startsWith('ShowroomAssignment.'))).toBe(false);
  });
});
