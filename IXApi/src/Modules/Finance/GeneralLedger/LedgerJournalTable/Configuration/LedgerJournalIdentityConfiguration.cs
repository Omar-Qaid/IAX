using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

public sealed class LedgerConfiguration : IEntityTypeConfiguration<IAX.IXApi.Modules.Finance.Entities.Ledger>
{
    public void Configure(EntityTypeBuilder<IAX.IXApi.Modules.Finance.Entities.Ledger> builder) =>
        builder.HasIndex(x => x.DataAreaId).IsUnique().HasFilter("[IsDeleted] = 0");
}

public sealed class LedgerJournalNameConfiguration : IEntityTypeConfiguration<LedgerJournalName>
{
    public void Configure(EntityTypeBuilder<LedgerJournalName> builder) =>
        builder.HasIndex(x => new { x.DataAreaId, x.JournalName }).IsUnique().HasFilter("[IsDeleted] = 0");
}

public sealed class LedgerJournalTableConfiguration : IEntityTypeConfiguration<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>
{
    public void Configure(EntityTypeBuilder<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable> builder) =>
        builder.HasIndex(x => new { x.DataAreaId, x.JournalNum }).IsUnique().HasFilter("[IsDeleted] = 0");
}

public sealed class LedgerJournalTransConfiguration : IEntityTypeConfiguration<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>
{
    public void Configure(EntityTypeBuilder<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans> builder) =>
        builder.HasIndex(x => new { x.DataAreaId, x.JournalNum, x.LineNum }).IsUnique().HasFilter("[IsDeleted] = 0");
}
