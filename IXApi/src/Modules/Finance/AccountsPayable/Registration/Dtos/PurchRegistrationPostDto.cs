namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Quantities arriving against a purchase order. <c>UnitPrice</c> on a line is ignored: registration
/// records arrival, not value.
/// </summary>
public sealed record PurchRegistrationPostDto(DateTime? RegistrationDate, List<PurchPostLineDto> Lines);
