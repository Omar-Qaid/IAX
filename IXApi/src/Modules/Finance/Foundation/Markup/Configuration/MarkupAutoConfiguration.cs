using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup.Configuration;

public sealed class MarkupAutoTableConfiguration : IEntityTypeConfiguration<MarkupAutoTable>
{
    public void Configure(EntityTypeBuilder<MarkupAutoTable> builder)
    {
        builder.Property(x => x.AccountRelation).HasMaxLength(FieldLengths.AccountNum);
        builder.Property(x => x.DlvModeRelation).HasMaxLength(FieldLengths.DlvModeId);
        builder.Property(x => x.ItemRelation).HasMaxLength(FieldLengths.ItemId);
        builder.Property(x => x.ReturnRelation).HasMaxLength(FieldLengths.ReferenceId);
        builder.Property(x => x.RetailChannelRelation).HasMaxLength(FieldLengths.ReferenceId);
        builder.Property(x => x.SHA256Hash).HasMaxLength(64);
        builder.Property(x => x.Description).HasMaxLength(FieldLengths.Txt);
        builder.HasIndex(x => new { x.DataAreaId, x.ModuleType, x.AccountCode, x.AccountRelation, x.ItemCode, x.ItemRelation });
    }
}

public sealed class MarkupAutoLineConfiguration : IEntityTypeConfiguration<MarkupAutoLine>
{
    public void Configure(EntityTypeBuilder<MarkupAutoLine> builder)
    {
        builder.Property(x => x.CurrencyCode).HasMaxLength(FieldLengths.CurrencyCode);
        builder.Property(x => x.MarkupCurrencyCode).HasMaxLength(FieldLengths.CurrencyCode);
        builder.Property(x => x.MarkupCode).HasMaxLength(FieldLengths.MarkupCode);
        builder.Property(x => x.TaxGroup).HasMaxLength(FieldLengths.TaxGroup);
        builder.Property(x => x.TaxItemGroup).HasMaxLength(FieldLengths.TaxItemGroup);
        builder.Property(x => x.Txt).HasMaxLength(FieldLengths.Txt);
        builder.Property(x => x.InventSiteId).HasMaxLength(FieldLengths.InventSiteId);
        builder.Property(x => x.InventLocationId).HasMaxLength(FieldLengths.InventLocationId);
        builder.HasIndex(x => new { x.DataAreaId, x.TableRecId, x.LineNum });
        // TableTableId is retained for the D365 polymorphic reference. No physical FK is
        // created because it can point to another table type.
    }
}
