namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

/// <summary>Phase one always revalidates existing reference data on rerun.</summary>
public enum SeedExecutionPolicy
{
    InsertOrValidate = 0
}
