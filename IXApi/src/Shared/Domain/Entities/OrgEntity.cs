namespace IAX.IXApi.Shared.Domain.Entities;

public abstract class OrgEntity : MasterEntity<long>
{
    public short DepartmentId { get; set; }
    public long PartyId { get; set; }
}
