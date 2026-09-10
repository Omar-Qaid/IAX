namespace IAX.IXApi.Shared.Application.Identity;

/// <summary>Identity established by a trusted worker in its own dependency-injection scope.</summary>
public sealed class BackgroundExecutionIdentity
{
    public string? UserId { get; private set; }
    public string? DataAreaId { get; private set; }
    public string? OwnerAccountId { get; private set; }

    public void Initialize(string userId, string dataAreaId, string ownerAccountId)
    {
        if (UserId is not null) throw new InvalidOperationException("Background identity is already established.");
        if (string.IsNullOrWhiteSpace(userId) || userId == "sys" || string.IsNullOrWhiteSpace(dataAreaId))
            throw new ArgumentException("An execution account and company are required.");
        UserId = userId;
        DataAreaId = dataAreaId;
        OwnerAccountId = ownerAccountId;
    }
}
