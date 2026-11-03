using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HcmGender = IAX.IXApi.Modules.Finance.Foundation.Genders.Gender;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.HumanResources;

internal static class HcmWorkerShowroomAssignmentDemoData
{
    internal static HcmWorkerShowroomAssignment Create(long worker, long showroom) =>
        new HcmWorkerShowroomAssignment { DataAreaId = "dat", HcmWorkerId = worker, HcmShowroomId = showroom, ValidFrom = new DateOnly(2026, 1, 1), IsPrimary = true };
}
