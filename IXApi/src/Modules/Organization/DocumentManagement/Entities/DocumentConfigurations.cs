using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Entities;

public sealed class DocuTypeConfiguration : IEntityTypeConfiguration<DocuType>
{
    public void Configure(EntityTypeBuilder<DocuType> builder)
    {
        builder.ToTable("DocuType"); builder.HasKey(x => x.RecId);
        builder.Property(x => x.ActionClassId).HasColumnName("ACTIONCLASSID");
        builder.Property(x => x.ArchivePath).HasColumnName("ARCHIVEPATH");
        builder.Property(x => x.DocuStructureType).HasColumnName("DOCUSTRUCTURETYPE");
        builder.Property(x => x.FilePlace).HasColumnName("FILEPLACE");
        builder.Property(x => x.FileRemovalConfirmation).HasColumnName("FILEREMOVALCONFIRMATION");
        builder.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Parameters).HasColumnName("PARAMETERS");
        builder.Property(x => x.RemoveOption).HasColumnName("REMOVEOPTION");
        builder.Property(x => x.TypeGroup).HasColumnName("TYPEGROUP");
        builder.Property(x => x.TypeId).HasColumnName("TYPEID").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Host).HasColumnName("HOST").HasMaxLength(510);
        builder.Property(x => x.Site).HasColumnName("SITE").HasMaxLength(510);
        builder.Property(x => x.FolderPath).HasColumnName("FOLDERPATH").HasMaxLength(510);
        builder.Property(x => x.DataAreaId).HasColumnName("DATAAREAID").HasMaxLength(8).IsRequired();
        builder.Property(x => x.Partition).HasColumnName("PARTITION");
        builder.HasAlternateKey(x => x.TypeId);
        DocumentConfiguration.MapSystemFields(builder);
    }
}

public sealed class DocuValueConfiguration : IEntityTypeConfiguration<DocuValue>
{
    public void Configure(EntityTypeBuilder<DocuValue> builder)
    {
        builder.ToTable("DocuValue"); builder.HasKey(x => x.RecId);
        builder.Property(x => x.File).HasColumnName("FILE_");
        builder.Property(x => x.FileName).HasColumnName("FILENAME").HasMaxLength(518).IsRequired();
        builder.Property(x => x.FileType).HasColumnName("FILETYPE").HasMaxLength(518).IsRequired();
        builder.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(120).IsRequired();
        builder.Property(x => x.OriginalFileName).HasColumnName("ORIGINALFILENAME").HasMaxLength(518).IsRequired();
        builder.Property(x => x.Path).HasColumnName("PATH");
        builder.Property(x => x.Type).HasColumnName("TYPE");
        builder.Property(x => x.FileId).HasColumnName("FILEID");
        builder.Property(x => x.McrDocuSubject).HasColumnName("MCRDOCUSUBJECT");
        builder.Property(x => x.AccessInformation).HasColumnName("ACCESSINFORMATION").HasMaxLength(2520).IsRequired();
        builder.Property(x => x.StorageProviderId).HasColumnName("STORAGEPROVIDERID");
        builder.Property(x => x.DocumentHashNumber).HasColumnName("DOCUMENTHASHNUMBER").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Partition).HasColumnName("PARTITION");
        DocumentConfiguration.MapSystemFields(builder);
    }
}

public sealed class DocuRefConfiguration : IEntityTypeConfiguration<DocuRef>
{
    public void Configure(EntityTypeBuilder<DocuRef> builder)
    {
        builder.ToTable("DocuRef"); builder.HasKey(x => x.RecId);
        builder.Property(x => x.RefCompanyId).HasColumnName("REFCOMPANYID").HasMaxLength(8).IsRequired();
        builder.Property(x => x.ActualCompanyId).HasColumnName("ACTUALCOMPANYID").HasMaxLength(8).IsRequired();
        builder.Property(x => x.Author).HasColumnName("AUTHOR");
        builder.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Notes).HasColumnName("NOTES");
        builder.Property(x => x.Party).HasColumnName("PARTY");
        builder.Property(x => x.RefRecId).HasColumnName("REFRECID");
        builder.Property(x => x.RefTableId).HasColumnName("REFTABLEID");
        builder.Property(x => x.Restriction).HasColumnName("RESTRICTION");
        builder.Property(x => x.TypeId).HasColumnName("TYPEID").HasMaxLength(40).IsRequired();
        builder.Property(x => x.ValueRecId).HasColumnName("VALUERECID");
        builder.Property(x => x.SmmEmailEntryId).HasColumnName("SMMEMAILENTRYID").HasMaxLength(510).IsRequired();
        builder.Property(x => x.SmmEmailStoreId).HasColumnName("SMMEMAILSTOREID").HasMaxLength(600).IsRequired();
        builder.Property(x => x.SmmTable).HasColumnName("SMMTABLE");
        builder.Property(x => x.EncyclopediaItemId).HasColumnName("ENCYCLOPEDIAITEMID").HasMaxLength(20).IsRequired();
        builder.Property(x => x.ContactPersonId).HasColumnName("CONTACTPERSONID").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Partition).HasColumnName("PARTITION");
        builder.Property(x => x.IsJustification).HasColumnName("ISJUSTIFICATION");
        builder.Property(x => x.DocumentId).HasColumnName("DOCUMENTID");
        builder.Property(x => x.DefaultAttachment).HasColumnName("DEFAULTATTACHMENT");
        builder.Property(x => x.EngChgEngineeringReference).HasColumnName("ENGCHGENGINEERINGREFERENCE").HasMaxLength(72).IsRequired();
        builder.Property(x => x.EngChgEngineeringDocument).HasColumnName("ENGCHGENGINEERINGDOCUMENT");
        builder.Property(x => x.IsEnabledForVirtualEntitySync).HasColumnName("ISENABLEDFORVIRTUALENTITYSYNC");
        builder.Property(x => x.DataAreaId).HasColumnName("DATAAREAID").HasMaxLength(8).IsRequired();
        builder.HasIndex(x => new { x.RefTableId, x.RefRecId, x.RefCompanyId }).HasDatabaseName("IX_DocuRef_Record");
        builder.HasIndex(x => x.ValueRecId).IsUnique();
        builder.HasOne(x => x.DocuType).WithMany().HasForeignKey(x => x.TypeId).HasPrincipalKey(x => x.TypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DocuValue).WithOne().HasForeignKey<DocuRef>(x => x.ValueRecId).OnDelete(DeleteBehavior.Restrict);
        DocumentConfiguration.MapSystemFields(builder);
    }
}

internal static class DocumentConfiguration
{
    internal static void MapSystemFields<T>(EntityTypeBuilder<T> builder) where T : BaseEntity<long>
    {
        builder.Property(x => x.RecId).HasColumnName("RECID").ValueGeneratedOnAdd();
        builder.Property(x => x.RecVersion).HasColumnName("RECVERSION");
        builder.Property(x => x.RowVersion).HasColumnName("SYSROWVERSION").IsRowVersion();
        builder.Property(x => x.CreatedBy).HasColumnName("CREATEDBY").HasMaxLength(40).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("CREATEDDATETIME").HasColumnType("datetime").IsRequired();
        builder.Property(x => x.LastModifiedBy).HasColumnName("MODIFIEDBY").HasMaxLength(40).IsRequired();
        builder.Property(x => x.LastModifiedAt).HasColumnName("MODIFIEDDATETIME").HasColumnType("datetime").IsRequired();
    }
}
