using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class AccountsPayableScenarioDemoData
{
    internal const string LocalVendor = "ERP-AP-LOCAL", ServiceVendor = "ERP-AP-SERVICE", ForeignVendor = "ERP-AP-USD", DiscountVendor = "ERP-AP-DISC";
    internal const string Item = "ERP-AP-STOCK1", SecondItem = "ERP-AP-STOCK2", ModelGroup = "ERP-AP-ACCRUAL";
    internal const string ForeignBank = "ERP-AP-USD", Discount = "ERP2P10";
    internal static DateTime ScenarioDate(IConfiguration configuration)
    {
        var value = configuration["Seeding:AccountsPayable:ScenarioDate"] ?? "2026-10-15";
        if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw new InvalidOperationException("Seeding:AccountsPayable:ScenarioDate must be yyyy-MM-dd in an open fiscal period.");
        return date.Date;
    }

    internal static readonly (string Account, string Name, string Group, string Currency, string TaxGroup)[] Vendors =
    [
        (LocalVendor, "Example Cedar Office Supplies", "DOM", "SAR", "KSA"),
        (ServiceVendor, "Example Cedar Maintenance Services", "SERVICES", "SAR", "KSA"),
        (ForeignVendor, "Example Cedar International Consulting", "INT", "USD", "EXP"),
        (DiscountVendor, "Example Cedar Early Payment Supplies", "DOM", "SAR", "KSA")
    ];
}
