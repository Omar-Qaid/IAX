namespace IAX.IXApi.Modules.Identity.Users
{
    public class AspNetUserDto
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string? PhotoUrl { get; set; }

        // Link to an HR employee record.
        public long? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        // Account enabled = not currently locked out. Derived in mapping.
        public bool Enabled { get; set; }

        // RBAC — populated by AuthController and UserController from Identity + permission tables.
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<string> AllowedCompanies { get; set; } = new();
        public string DefaultCompany { get; set; } = "dat";
    }
}
