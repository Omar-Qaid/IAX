using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Communication.Chat.Entities
{
    public class SysChatReadStateConfiguration : IEntityTypeConfiguration<SysChatReadState>
    {
        public void Configure(EntityTypeBuilder<SysChatReadState> builder)
        {
            builder.ToTable("SysChatReadStates");

            // One read-state row per (user, room).
            builder.HasIndex(x => new { x.UserId, x.RoomId }).IsUnique();
        }
    }
}
