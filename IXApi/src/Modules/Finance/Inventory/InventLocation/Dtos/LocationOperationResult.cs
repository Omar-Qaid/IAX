namespace IAX.IXApi.Modules.Finance.Inventory;

public enum LocationOperationStatus { Success, NotFound, Duplicate, SiteNotFound, CodeChanged, InvalidReference, Referenced }
public sealed record LocationOperationResult(InventLocationResponseDto? Location, LocationOperationStatus Status, string? Error = null);
