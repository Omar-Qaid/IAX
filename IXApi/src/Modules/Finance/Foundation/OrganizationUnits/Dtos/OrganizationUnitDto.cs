using System;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;

public class OrganizationUnitDto : MasterEntityDto<long>
{
    public DateOnly ValidFrom { get; set; } = DateOnly.MinValue;
    public DateOnly? ValidTo { get; set; }
    public byte OrganizationUnitType { get; set; }
    public long? ParentOrganizationUnitId { get; set; }
}
