using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.Transfer;

public sealed class InventTransOriginTransferShipConfiguration : IEntityTypeConfiguration<InventTransOriginTransferShip>
{
    public void Configure(EntityTypeBuilder<InventTransOriginTransferShip> builder)
    {
        builder.Property(x => x.TransferShipDataAreaId).HasMaxLength(FieldLengths.CompanyId);
        builder.Property(x => x.TransferShipId).HasMaxLength(FieldLengths.TransferId);
        builder.Property(x => x.TransferShipLineNum).HasPrecision(28, 16);
        builder.HasIndex(x => new { x.DataAreaId, x.InventTransOrigin }).IsUnique();
        builder.HasIndex(x => new { x.TransferShipDataAreaId, x.TransferShipId, x.TransferShipLineNum });
    }
}

public sealed class InventTransOriginTransferTransitToConfiguration : IEntityTypeConfiguration<InventTransOriginTransferTransitTo>
{
    public void Configure(EntityTypeBuilder<InventTransOriginTransferTransitTo> builder)
    {
        builder.Property(x => x.TransferTransitToDataAreaId).HasMaxLength(FieldLengths.CompanyId);
        builder.Property(x => x.TransferTransitToId).HasMaxLength(FieldLengths.TransferId);
        builder.Property(x => x.TransferTransitToLineNum).HasPrecision(28, 16);
        builder.HasIndex(x => new { x.DataAreaId, x.InventTransOrigin }).IsUnique();
        builder.HasIndex(x => new { x.TransferTransitToDataAreaId, x.TransferTransitToId, x.TransferTransitToLineNum });
    }
}

public sealed class InventTransOriginTransferTransitFromConfiguration : IEntityTypeConfiguration<InventTransOriginTransferTransitFrom>
{
    public void Configure(EntityTypeBuilder<InventTransOriginTransferTransitFrom> builder)
    {
        builder.Property(x => x.TransferTransitFromDataAreaId).HasMaxLength(FieldLengths.CompanyId);
        builder.Property(x => x.TransferTransitFromId).HasMaxLength(FieldLengths.TransferId);
        builder.Property(x => x.TransferTransitFromLineNum).HasPrecision(28, 16);
        builder.HasIndex(x => new { x.DataAreaId, x.InventTransOrigin }).IsUnique();
        builder.HasIndex(x => new { x.TransferTransitFromDataAreaId, x.TransferTransitFromId, x.TransferTransitFromLineNum });
    }
}

public sealed class InventTransOriginTransferReceiveConfiguration : IEntityTypeConfiguration<InventTransOriginTransferReceive>
{
    public void Configure(EntityTypeBuilder<InventTransOriginTransferReceive> builder)
    {
        builder.Property(x => x.TransferReceiveDataAreaId).HasMaxLength(FieldLengths.CompanyId);
        builder.Property(x => x.TransferReceiveId).HasMaxLength(FieldLengths.TransferId);
        builder.Property(x => x.TransferReceiveLineNum).HasPrecision(28, 16);
        builder.HasIndex(x => new { x.DataAreaId, x.InventTransOrigin }).IsUnique();
        builder.HasIndex(x => new { x.TransferReceiveDataAreaId, x.TransferReceiveId, x.TransferReceiveLineNum });
    }
}
