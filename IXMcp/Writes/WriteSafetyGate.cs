using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IAX.IXMcp.Writes;

public sealed record WriteIntent(
    string Subject,
    string Company,
    string ToolName,
    int ContractVersion,
    string CatalogHash,
    JsonElement Arguments,
    string IdempotencyKey);

public sealed record WriteApproval(
    string ApprovalId,
    string Subject,
    string Company,
    string ToolName,
    int ContractVersion,
    string CatalogHash,
    string PayloadHash,
    DateTimeOffset ExpiresAt);

public sealed record WriteGateResult(bool Allowed, string Code, string PayloadHash);

public interface IWriteSafetyStore
{
    ValueTask<WriteApproval?> FindApprovalAsync(string approvalId, CancellationToken cancellationToken);
    ValueTask<string?> FindPayloadHashAsync(string idempotencyKey, CancellationToken cancellationToken);
    ValueTask<bool> TryReserveAsync(string idempotencyKey, string payloadHash, CancellationToken cancellationToken);
}

public sealed class WriteSafetyGate(IWriteSafetyStore store, TimeProvider timeProvider)
{
    public async ValueTask<WriteGateResult> AuthorizeAsync(
        WriteIntent intent,
        string approvalId,
        CancellationToken cancellationToken = default)
    {
        var payloadHash = ComputePayloadHash(intent);
        var approval = await store.FindApprovalAsync(approvalId, cancellationToken);
        if (approval is null)
        {
            return new(false, "approval_required", payloadHash);
        }

        if (approval.ExpiresAt <= timeProvider.GetUtcNow())
        {
            return new(false, "approval_expired", payloadHash);
        }

        if (approval.Subject != intent.Subject
            || approval.Company != intent.Company
            || approval.ToolName != intent.ToolName
            || approval.ContractVersion != intent.ContractVersion
            || approval.CatalogHash != intent.CatalogHash
            || approval.PayloadHash != payloadHash)
        {
            return new(false, "approval_mismatch", payloadHash);
        }

        var existingHash = await store.FindPayloadHashAsync(intent.IdempotencyKey, cancellationToken);
        if (existingHash is not null)
        {
            return new(existingHash == payloadHash, existingHash == payloadHash ? "idempotent_replay" : "idempotency_conflict", payloadHash);
        }

        return await store.TryReserveAsync(intent.IdempotencyKey, payloadHash, cancellationToken)
            ? new(true, "reserved", payloadHash)
            : new(false, "idempotency_race", payloadHash);
    }

    public static string ComputePayloadHash(WriteIntent intent)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            intent.Subject,
            intent.Company,
            intent.ToolName,
            intent.ContractVersion,
            intent.CatalogHash,
            arguments = Canonicalize(intent.Arguments)
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    private static object? Canonicalize(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToDictionary(
                property => property.Name,
                property => Canonicalize(property.Value),
                StringComparer.Ordinal),
        JsonValueKind.Array => element.EnumerateArray().Select(Canonicalize).ToArray(),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number when element.TryGetInt64(out var integer) => integer,
        JsonValueKind.Number when element.TryGetDecimal(out var number) => number,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => element.GetRawText()
    };
}

public sealed class DevelopmentWriteSafetyStore : IWriteSafetyStore
{
    private readonly ConcurrentDictionary<string, WriteApproval> approvals = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> reservations = new(StringComparer.Ordinal);

    public void AddApproval(WriteApproval approval) => approvals[approval.ApprovalId] = approval;

    public ValueTask<WriteApproval?> FindApprovalAsync(string approvalId, CancellationToken cancellationToken) =>
        ValueTask.FromResult(approvals.GetValueOrDefault(approvalId));

    public ValueTask<string?> FindPayloadHashAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        ValueTask.FromResult(reservations.GetValueOrDefault(idempotencyKey));

    public ValueTask<bool> TryReserveAsync(string idempotencyKey, string payloadHash, CancellationToken cancellationToken) =>
        ValueTask.FromResult(reservations.TryAdd(idempotencyKey, payloadHash));
}
