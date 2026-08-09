using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Organization.Showrooms
{
    public class OrgShowroomDto : MasterEntityDto<long>
    {
        public short DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Location { get; set; }
        public int SellerCount { get; set; }
    }
}

