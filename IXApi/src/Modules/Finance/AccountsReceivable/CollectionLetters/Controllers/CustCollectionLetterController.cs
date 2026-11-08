using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

[ApiController]
[Route("api/v1/CustCollectionLetter")]
public sealed class CustCollectionLetterController(ICustCollectionLetterService service) : ControllerBase
{
    [HttpPost("generate")]
    [DomainPermission("AccountsReceivable", "CollectionLetters", "Create")]
    public async Task<IActionResult> Generate(CustCollectionLetterGenerateRequest request, CancellationToken ct)
    {
        try
        {
            var numbers = await service.GenerateFirstLettersAsync(request.AccountNum, request.CurrencyCode, request.LetterDate, ct);
            return Ok(APIResponse<IReadOnlyList<string>>.Ok(numbers));
        }
        catch (SubledgerPostingException ex)
        {
            return UnprocessableEntity(APIResponse<IReadOnlyList<string>>.Fail(ex.Message));
        }
    }

    [HttpPost("{number}/post")]
    [DomainPermission("AccountsReceivable", "CollectionLetters", "Post")]
    public async Task<IActionResult> Post(string number, CustCollectionLetterPostRequest request, CancellationToken ct)
    {
        try
        {
            return await service.PostWarningAsync(number, request.PostingDate, ct) ? Ok(APIResponse<string>.Ok(number)) : NoContent();
        }
        catch (SubledgerPostingException ex)
        {
            return UnprocessableEntity(APIResponse<string>.Fail(ex.Message));
        }
    }
}
