namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

public class ContactInfoDto
{
    public string? Id { get; set; }
    public long Location { get; set; }
    public string? LocationId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Extension { get; set; }
    public bool Primary { get; set; }
    public List<string>? Roles { get; set; }
}
