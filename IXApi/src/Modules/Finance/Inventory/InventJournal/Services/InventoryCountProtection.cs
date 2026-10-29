using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Text;

namespace IAX.IXApi.Modules.Finance.Inventory;

public static class InventoryCountProtection
{
    public static async Task<string?> CheckAsync(DbContext db, string area, string item, string dimension,
        CancellationToken ct, string? ownJournal = null)
    {
        if (db.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (db.Database.CurrentTransaction == null)
                throw new InvalidOperationException("Inventory protection requires a transaction.");
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.Transaction = db.Database.CurrentTransaction.GetDbTransaction();
            command.CommandText = "DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=@key,@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000; SELECT @r;";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@key";
            parameter.Value = "InventJournal:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"InventStock:{area}:{item}:{dimension}")));
            command.Parameters.Add(parameter);
            if (await command.ExecuteScalarAsync(ct) is not int result || result < 0)
                return "Inventory movement could not acquire its stock lock.";
        }
        var frozen = await (from header in db.Set<InventJournalTable>().AsNoTracking()
                            join line in db.Set<InventJournalTrans>().AsNoTracking()
                                on new { header.DataAreaId, header.JournalId } equals new { line.DataAreaId, line.JournalId }
                            where header.DataAreaId == area && header.CountingFrozen
                                && header.JournalId != ownJournal && line.ItemId == item && line.InventDimId == dimension
                            select header.JournalId).FirstOrDefaultAsync(ct);
        return frozen == null ? null : $"Inventory is frozen for counting journal '{frozen}'.";
    }
}
