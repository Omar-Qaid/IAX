namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
/// <summary>Stable business references described by the Foundation manifest and checked against persisted seeders.</summary>
public static class FoundationSeedExample
{
 public static IReadOnlyList<(string Entity, string Field, string Value)> References { get; } =
 [
  ("OrganizationUnit","Code","FNV2-FIN"), ("OrganizationRole","Code","FNV2-BUYER"),
  ("HcmPosition","Code","FNV2-BUYER"), ("HcmWorker","PersonnelNumber","FNV2-BUYER"),
  ("HcmShowroom","PersonnelNumber","FNV2-SHOWROOM"), ("PaymSched","Name","FNV2"),
  ("PaymTerm","PaymTermId","FNV2-60"), ("DlvMode","Code","FNV2"), ("DlvTerm","Code","FNV2"),
  ("DimensionHierarchy","Name","FNV2"), ("DimensionAttribute","Name","FNV2Department"),
  ("TaxPeriodHead","TaxPeriod","Monthly"), ("Currency","CurrencyCode","SAR")
 ];
}
