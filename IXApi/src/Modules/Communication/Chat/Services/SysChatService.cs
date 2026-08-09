using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Communication.Persistence;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Infrastructure.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Communication.Chat.Services
{
    /// <summary>
    /// Persists chat messages and broadcasts them to the room over <see cref="SysChatHub"/>.
    /// Used both by the hub (real-time send) and the REST controller (e.g. system-posted
    /// messages), so delivery semantics are identical regardless of entry point.
    /// </summary>
    public class SysChatService : ISysChatService
    {
        private readonly ICommunicationDataContext _db;
        private readonly IHubContext<SysChatHub> _hub;

        public SysChatService(ICommunicationDataContext db, IHubContext<SysChatHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        public async Task<SysChatMessageDto> SendAsync(string roomId, string senderId, string content, CancellationToken ct = default)
        {
            var entity = new SysChatMessage
            {
                RoomId = roomId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsActive = true,
            };

            _db.Set<SysChatMessage>().Add(entity);
            await _db.SaveChangesAsync(ct);

            var dto = Map(entity);

            var payload = SysRealtimeMessage.Create(SysRealtimeEventType.ChatMessage, dto, senderId);

            // 1) Deliver to everyone currently in the room (open thread â†’ live append).
            await _hub.Clients
                .Group(SysChatHub.ChatGroup(roomId))
                .SendAsync("ReceiveMessage", payload, ct);

            // 2) Also notify each conversation participant on their personal user group, so a
            //    recipient who is NOT currently viewing this room still gets a real-time signal
            //    to refresh their conversation list / unread badge (every authenticated client is
            //    auto-joined to "user_{id}" on connect â€” see SysHubBase).
            foreach (var participant in DmParticipants(roomId))
            {
                await _hub.Clients
                    .Group($"user_{participant}")
                    .SendAsync("ReceiveMessage", payload, ct);
            }

            return dto;
        }

        public async Task<(IReadOnlyList<SysChatMessageDto> Items, int TotalCount)> GetHistoryAsync(
            string roomId, int pageNumber = 1, int pageSize = 50, CancellationToken ct = default)
        {
            var query = _db.Set<SysChatMessage>()
                .AsNoTracking()
                .Where(m => m.RoomId == roomId);

            var total = await query.CountAsync(ct);

            var rows = await query
                .OrderByDescending(m => m.SentAt)
                .Skip((Math.Max(1, pageNumber) - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            // Map in memory â€” Map() is a C# method EF can't translate to SQL.
            var items = rows.Select(Map).ToList();

            return (items, total);
        }

        public async Task<IReadOnlyList<SysChatConversationDto>> GetConversationsAsync(string userId, CancellationToken ct = default)
        {
            // Candidate rooms: DMs containing the user, rooms the user has posted in, plus 'general'.
            var dmRooms = _db.Set<SysChatMessage>()
                .Where(m => m.RoomId.StartsWith("dm:") && m.RoomId.Contains(userId))
                .Select(m => m.RoomId);
            var mineRooms = _db.Set<SysChatMessage>()
                .Where(m => m.SenderId == userId)
                .Select(m => m.RoomId);

            var roomIds = await dmRooms.Concat(mineRooms).Distinct().ToListAsync(ct);
            if (!roomIds.Contains("general")) roomIds.Add("general");

            var reads = await _db.Set<SysChatReadState>()
                .AsNoTracking()
                .Where(r => r.UserId == userId && roomIds.Contains(r.RoomId))
                .ToDictionaryAsync(r => r.RoomId, r => r.LastReadAt, ct);

            var result = new List<SysChatConversationDto>();
            foreach (var room in roomIds)
            {
                var last = await _db.Set<SysChatMessage>()
                    .Where(m => m.RoomId == room)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => new { m.Content, m.SenderId, m.SentAt })
                    .FirstOrDefaultAsync(ct);

                var lastReadAt = reads.TryGetValue(room, out var lr) ? lr : DateTime.MinValue;
                var unread = await _db.Set<SysChatMessage>()
                    .CountAsync(m => m.RoomId == room
                                  && m.SenderId != userId && m.SentAt > lastReadAt, ct);

                result.Add(new SysChatConversationDto
                {
                    RoomId = room,
                    LastMessage = last?.Content,
                    LastSenderId = last?.SenderId,
                    LastSentAt = last?.SentAt,
                    UnreadCount = unread,
                });
            }

            return result.OrderByDescending(c => c.LastSentAt ?? DateTime.MinValue).ToList();
        }

        public async Task MarkReadAsync(string userId, string roomId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            // Atomic SQL MERGE â€” a single round-trip that inserts or updates.
            // No race condition possible: the unique index (UserId, RoomId) is held
            // exclusively by the MERGE, so concurrent calls just serialise cleanly.
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                MERGE INTO SysChatReadStates AS target
                USING (SELECT {userId} AS UserId, {roomId} AS RoomId) AS source
                ON target.UserId = source.UserId AND target.RoomId = source.RoomId
                WHEN MATCHED THEN
                    UPDATE SET LastReadAt = {now}, LastModifiedAt = {now}
                WHEN NOT MATCHED THEN
                    INSERT (UserId, RoomId, LastReadAt, IsActive, CreatedAt, LastModifiedAt)
                    VALUES (source.UserId, source.RoomId, {now}, 1, {now}, {now});
            ", ct);
        }

        /// <summary>
        /// Participant user ids encoded in a DM room key ("dm:a:b" â†’ ["a","b"]).
        /// Non-DM rooms return none (membership isn't tracked; the room broadcast + client
        /// polling cover those).
        /// </summary>
        private static IEnumerable<string> DmParticipants(string roomId) =>
            roomId.StartsWith("dm:", StringComparison.Ordinal)
                ? roomId[3..].Split(':', StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>();

        private static SysChatMessageDto Map(SysChatMessage m) => new()
        {
            RecId = m.RecId,
            RoomId = m.RoomId,
            SenderId = m.SenderId,
            Content = m.Content,
            SentAt = m.SentAt,
        };
    }
}

