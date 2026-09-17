using System.Text.Json;
using IAX.IXMcp.Writes;

namespace IAX.IXMcp.Tests;

public sealed class WriteSafetyGateTests
{
    [Fact]
    public async Task Approval_is_bound_to_subject_company_tool_catalog_and_payload()
    {
        var store = new DevelopmentWriteSafetyStore();
        var intent = Intent("idem-1", "DAT", "{\"amount\":10}");
        store.AddApproval(Approval(intent));
        var gate = new WriteSafetyGate(store, TimeProvider.System);

        var accepted = await gate.AuthorizeAsync(intent, "approval-1");
        var changed = await gate.AuthorizeAsync(Intent("idem-2", "USMF", "{\"amount\":10}"), "approval-1");

        Assert.True(accepted.Allowed);
        Assert.Equal("reserved", accepted.Code);
        Assert.False(changed.Allowed);
        Assert.Equal("approval_mismatch", changed.Code);
    }

    [Fact]
    public async Task Same_idempotency_key_replays_only_the_same_payload()
    {
        var store = new DevelopmentWriteSafetyStore();
        var first = Intent("same-key", "DAT", "{\"amount\":10}");
        store.AddApproval(Approval(first));
        var gate = new WriteSafetyGate(store, TimeProvider.System);
        await gate.AuthorizeAsync(first, "approval-1");

        var replay = await gate.AuthorizeAsync(first, "approval-1");
        var changed = Intent("same-key", "DAT", "{\"amount\":11}");
        store.AddApproval(Approval(changed));
        var conflict = await gate.AuthorizeAsync(changed, "approval-1");

        Assert.Equal("idempotent_replay", replay.Code);
        Assert.Equal("idempotency_conflict", conflict.Code);
    }

    [Fact]
    public void Payload_hash_is_stable_across_object_property_order()
    {
        var first = Intent("key-a", "DAT", "{\"amount\":10,\"code\":\"A\"}");
        var second = Intent("key-b", "DAT", "{\"code\":\"A\",\"amount\":10}");

        Assert.Equal(WriteSafetyGate.ComputePayloadHash(first), WriteSafetyGate.ComputePayloadHash(second));
    }

    private static WriteIntent Intent(string key, string company, string json) => new(
        "user-a", company, "finance_test_write", 1, "catalog-hash",
        JsonDocument.Parse(json).RootElement.Clone(), key);

    private static WriteApproval Approval(WriteIntent intent) => new(
        "approval-1", intent.Subject, intent.Company, intent.ToolName, intent.ContractVersion,
        intent.CatalogHash, WriteSafetyGate.ComputePayloadHash(intent), DateTimeOffset.UtcNow.AddMinutes(5));
}
