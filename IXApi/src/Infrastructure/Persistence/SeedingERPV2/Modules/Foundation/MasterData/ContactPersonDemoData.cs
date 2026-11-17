namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

/// <summary>Fictional, bilingual contacts for the existing Foundation customer/vendor party.</summary>
public static class ContactPersonDemoData
{
    public sealed record Example(string Code, string FirstName, string LastName, string ArabicName,
        string Initials, bool CustomerContact, bool Inactive = false)
    {
        public string Name => $"{FirstName} {LastName}";
    }

    public static IReadOnlyList<Example> Examples { get; } = [
        new("FNV2-SALES", "Sara", "Ali", "سارة علي", "SA", true),
        new("FNV2-ACCOUNTS", "Ahmed", "Saleh", "أحمد صالح", "AS", false),
        new("FNV2-INACTIVE", "Mona", "Hassan", "منى حسن", "MH", true, true),
    ];

    public static DateTime NameValidFrom { get; } = new(2020, 1, 1);
}
