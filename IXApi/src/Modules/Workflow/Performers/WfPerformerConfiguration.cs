using IAX.IXApi.Modules.Workflow.Performers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerConfiguration : IEntityTypeConfiguration<WfPerformer>
    {
        public void Configure(EntityTypeBuilder<WfPerformer> builder)
        {
            builder.ToTable("WfPerformers");

            builder.HasKey(x => x.RecId);
            builder.Property(x => x.RecId)
                .HasColumnName("PerformerId")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Name)
                .HasColumnName("PerformerName")
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnName("Activated")
                .IsRequired();

            builder.Property(x => x.PerformerTypeId)
                .HasColumnName("PerformerType")
                .HasColumnType("smallint")
                .IsRequired();


            builder.Property(x => x.RelatedField)
                .HasColumnName("RelatedField");

            builder.Property(x => x.IsEmployee)
                .HasColumnName("IsEmployee")
                .IsRequired();

            builder.Property(x => x.IsApplicant)
                .HasColumnName("IsApplicant")
                .IsRequired();

            builder.Property(x => x.IsManager1)
                .HasColumnName("IsManager1")
                .IsRequired();

            builder.Property(x => x.IsManager2)
                .HasColumnName("IsManager2")
                .IsRequired();

            builder.Property(x => x.IsManager3)
                .HasColumnName("IsManager3")
                .IsRequired();

            builder.Property(x => x.IsManager4)
                .HasColumnName("IsManager4")
                .IsRequired();

            builder.Property(x => x.SqlTable)
                .HasColumnName("SqlTable")
                .HasMaxLength(200);

            builder.Property(x => x.SqlField)
                .HasColumnName("SqlField")
                .HasMaxLength(200);

            builder.Property(x => x.SqlWhere)
                .HasColumnName("SqlWhere")
                .HasMaxLength(2000);

            // Ignore columns not present in WfPerformers table but present in base Entity<long>
            //builder.Ignore(x => x.Code);
            //builder.Ignore(x => x.IsDeleted);
            //builder.Ignore(x => x.RowVersion);
        }
    }
}

