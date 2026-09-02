using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestControlConfiguration : IEntityTypeConfiguration<WfRequestControl>
    {
        public void Configure(EntityTypeBuilder<WfRequestControl> builder)
        {
            builder.ToTable("WfRequestControls", table =>
            {
                table.HasCheckConstraint("CK_WfRequestControls_ReferenceType", "[ReferenceType] IS NULL OR [ReferenceType] IN (N'Lookup',N'Employee',N'Showroom',N'Branch',N'Company',N'Department',N'BusinessUnit',N'Area',N'City',N'Country',N'Location',N'Customer',N'Vendor',N'Item',N'ItemGroup',N'Category',N'Warehouse',N'PaymentMethod',N'ViolationType',N'Invoice',N'PurchaseOrder',N'SalesOrder',N'Process',N'User')");
                table.HasCheckConstraint("CK_WfRequestControls_FieldRole", "[FieldRole] IN (N'Dimension',N'Measure',N'Both')");
                table.HasCheckConstraint("CK_WfRequestControls_DataType", "[DataType] IN (N'String',N'Integer',N'Decimal',N'Date',N'DateTime',N'Time',N'Boolean')");
                table.HasCheckConstraint("CK_WfRequestControls_DefaultAggregation", "[DefaultAggregation] IN (N'NONE',N'SUM',N'COUNT',N'COUNT_DISTINCT',N'AVG',N'MIN',N'MAX')");
            });

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("RequestControlId")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ReferenceType).HasMaxLength(50);
            builder.Property(x => x.FieldRole).HasMaxLength(20).HasDefaultValue("Dimension");
            builder.Property(x => x.DataType).HasMaxLength(20).HasDefaultValue("String");
            builder.Property(x => x.DefaultAggregation).HasMaxLength(20).HasDefaultValue("NONE");
            builder.Property(x => x.CanFilter).HasDefaultValue(true);
            builder.Property(x => x.CanGroup).HasDefaultValue(true);
            builder.Property(x => x.CanSort).HasDefaultValue(true);

            // Configure relationships
            builder.HasOne(x => x.Control)
                .WithMany()
                .HasForeignKey(x => x.ControlId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Process)
                .WithMany()
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

