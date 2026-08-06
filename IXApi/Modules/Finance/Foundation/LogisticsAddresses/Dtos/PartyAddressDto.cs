using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public class AddressInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public long Location { get; set; }
        public string LocationId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool Primary { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string CountryRegionId { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class ContactInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public long Location { get; set; }
        public string LocationId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public bool Primary { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}

