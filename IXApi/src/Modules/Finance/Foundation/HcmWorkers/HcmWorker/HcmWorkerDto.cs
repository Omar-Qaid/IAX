using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

public class HcmWorkerDto : MasterEntityDto<long>
{
    public string PersonnelNumber { get; set; } = string.Empty;
    public long Person { get; set; }
    public string? UserId { get; set; }
    public short OccupationId { get; set; }
    public string? OccupationName { get; set; }
    public byte GenderId { get; set; }
    public string? GenderName { get; set; }
    public short NationalityId { get; set; }
    public string? NationalityName { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? BirthDate { get; set; }
}