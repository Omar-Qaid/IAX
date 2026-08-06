using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Variables
{
    public class WfDataTypeConfiguration : IEntityTypeConfiguration<WfDataType>
    {
        public void Configure(EntityTypeBuilder<WfDataType> builder)
        {
            builder.ToTable("WfDataTypes");

            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            builder.HasData(
                new WfDataType { RecId = 1, Code = "STR",  Name = "String",  NameAR = "نص",    SortOrder = 1, IsActive = true, IsDeleted = false, CreatedAt = seedDate, LastModifiedAt = seedDate },
                new WfDataType { RecId = 2, Code = "NUM",  Name = "Number",  NameAR = "رقم",   SortOrder = 2, IsActive = true, IsDeleted = false, CreatedAt = seedDate, LastModifiedAt = seedDate },
                new WfDataType { RecId = 3, Code = "BOOL", Name = "Boolean", NameAR = "منطقي", SortOrder = 3, IsActive = true, IsDeleted = false, CreatedAt = seedDate, LastModifiedAt = seedDate },
                new WfDataType { RecId = 4, Code = "DATE", Name = "Date",    NameAR = "تاريخ", SortOrder = 4, IsActive = true, IsDeleted = false, CreatedAt = seedDate, LastModifiedAt = seedDate }
            );
        }
    }
}
