using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses;
using System;

namespace IAX.IXApi.Modules.Organization.Employees
{
    public class HcmWorkerDto : MasterEntityDto<long>
    {
        public short DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public short OccupationId { get; set; }
        public string? OccupationName { get; set; }
        public byte GenderId { get; set; }
        public short NationalityId { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? BirthDate { get; set; }
        public long? ShowroomId { get; set; }
        public string? ShowroomName { get; set; }

        public List<AddressInfoDto>? Addresses { get; set; }
        public List<ContactInfoDto>? Contacts { get; set; }
    }
}

