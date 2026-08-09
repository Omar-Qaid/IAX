namespace IAX.IXApi.Modules.Organization.Showrooms
{
    /// <summary>Lightweight view of a seller (employee) for the showroom sellers page.</summary>
    public class ShowroomSellerDto
    {
        public long RecId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? OccupationName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }
}

