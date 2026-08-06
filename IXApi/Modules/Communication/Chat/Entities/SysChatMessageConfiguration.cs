using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Communication.Chat.Entities
{
    public class SysChatMessageConfiguration : IEntityTypeConfiguration<SysChatMessage>
    {
        public void Configure(EntityTypeBuilder<SysChatMessage> builder)
        {
            builder.ToTable("SysChatMessages");

            // History queries are room-scoped and time-ordered.
            builder.HasIndex(x => new { x.RoomId, x.SentAt });
        }
    }
}
