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
  });
});
