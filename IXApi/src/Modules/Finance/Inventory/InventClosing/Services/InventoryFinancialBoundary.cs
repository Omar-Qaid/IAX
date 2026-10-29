using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Text;

namespace IAX.IXApi.Modules.Finance.Inventory;

public static class InventoryFinancialBoundary
{
    public static async Task<string?> CheckAsync(DbContext db, string area, string item, DateTime date, CancellationToken ct)
    {
        if (db.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (db.Database.CurrentTransaction == null) throw new InvalidOperationException("Financial inventory posting requires a transaction.");
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.Transaction = db.Database.CurrentTransaction.GetDbTransaction();
            command.CommandText = "DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=@key,@LockMode='Shared',@LockOwner='Transaction',@LockTimeout=10000; SELECT @r;";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@key";
            parameter.Value = "InventClose:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"InventClose:{area}")));
            command.Parameters.Add(parameter);
            if (await command.ExecuteScalarAsync(ct) is not int result || result < 0) return "Inventory close is currently locking financial postings.";
        }
        var closed = await db.Set<InventClosing>().AsNoTracking().AnyAsync(x => x.DataAreaId == area
            && (x.ScopeItemId == "" || x.ScopeItemId == item) && x.AdjustmentType == InventAdjustmentType.Closing
            && x.InventCostStatus == InventCostStatus.Finished && x.TransDate >= date.Date, ct);
        return closed ? "Inventory is financially closed through this posting date for the selected item." : null;
    }
}
