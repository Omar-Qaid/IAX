using System.Globalization;
using FluentValidation;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class EcoResProductService : BaseService<EcoResProduct>, IEcoResProductService
{
    private readonly IValidator<EcoResProductInputDto> _validator;
    private DbContext Db => _unitOfWork.Context;

    public EcoResProductService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        IValidator<EcoResProductInputDto> validator) : base(unitOfWork, currentUser) => _validator = validator;

    public async Task<List<EcoResProductDto>> ListAsync(CancellationToken ct = default)
    {
        var products = await Db.Set<EcoResProduct>().AsNoTracking()
            .OrderBy(x => x.DisplayProductNumber).ToListAsync(ct);
        var ids = products.Select(x => x.RecId).ToList();
        var translations = await Db.Set<EcoResProductTranslation>().AsNoTracking()
            .Where(x => ids.Contains(x.Product)).ToListAsync(ct);
        return products.Select(x => Map(x, translations)).ToList();
    }

    public async Task<(EcoResProductDto? Product, string? Error)> CreateAsync(EcoResProductInputDto input, CancellationToken ct = default)
    {
        var number = NormalizeNumber(input.ProductNumber);
        var error = await ValidateAsync(input, ct);
        if (error is not null) return (null, error);
        if (await Db.Set<EcoResProduct>().AnyAsync(x => x.DisplayProductNumber == number, ct))
            return (null, $"Product number '{number}' already exists.");

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var product = new EcoResProduct { DisplayProductNumber = number, SearchName = Clean(input.SearchName), ProductType = ParseProductType(input.ProductType) };
            Db.Add(product);
            await _unitOfWork.CompleteAsync(ct);
            Db.Add(new EcoResDistinctProduct { RecId = product.RecId });
            var translations = CreateTranslations(product.RecId, input);
            Db.AddRange(translations);
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return (Map(product, translations), null);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<(EcoResProductDto? Product, string? Error)> UpdateAsync(long recId, EcoResProductInputDto input, CancellationToken ct = default)
    {
        var product = await Db.Set<EcoResProduct>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (product is null) return (null, "The product was not found.");
        var number = NormalizeNumber(input.ProductNumber);
        var error = await ValidateAsync(input, ct);
        if (error is not null) return (null, error);
        if (await Db.Set<EcoResProduct>().AnyAsync(x => x.RecId != recId && x.DisplayProductNumber == number, ct))
            return (null, $"Product number '{number}' already exists.");
        var requestedType = ParseProductType(input.ProductType);
        if (requestedType != product.ProductType && await Db.Set<ReleasedItem>().AnyAsync(x => x.Product == recId, ct))
            return (null, "Product type cannot be changed after the product has been released to a company.");
        product.DisplayProductNumber = number;
        product.SearchName = Clean(input.SearchName);
        product.ProductType = requestedType;
        var translations = await Db.Set<EcoResProductTranslation>().Where(x => x.Product == recId).ToListAsync(ct);
        UpsertTranslation(translations, product.RecId, "en-us", input.Name, input.Description);
        UpsertTranslation(translations, product.RecId, "ar", input.NameAlias, input.Description);
        await _unitOfWork.CompleteAsync(ct);
        return (Map(product, translations), null);
    }

    public async Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct = default)
    {
        var product = await Db.Set<EcoResProduct>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (product is null) return (false, "The product was not found.");
        if (await Db.Set<ReleasedItem>().AnyAsync(x => x.Product == recId, ct))
            return (false, "A product released to a company cannot be deleted.");
        var translations = await Db.Set<EcoResProductTranslation>().Where(x => x.Product == recId).ToListAsync(ct);
        var distinctProduct = await Db.Set<EcoResDistinctProduct>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        Db.RemoveRange(translations);
        if (distinctProduct is not null) Db.Remove(distinctProduct);
        Db.Remove(product);
        await _unitOfWork.CompleteAsync(ct);
        return (true, null);
    }

    private void UpsertTranslation(List<EcoResProductTranslation> rows, long productId, string languageId, string name, string description)
    {
        var row = rows.FirstOrDefault(x => x.LanguageId == languageId);
        if (row is null) { row = new EcoResProductTranslation { Product = productId, LanguageId = languageId }; rows.Add(row); Db.Add(row); }
        row.Name = Clean(name); row.Description = Clean(description);
    }

    private static List<EcoResProductTranslation> CreateTranslations(long productId, EcoResProductInputDto input) =>
    [
        new() { Product = productId, LanguageId = "en-us", Name = Clean(input.Name), Description = Clean(input.Description) },
        new() { Product = productId, LanguageId = "ar", Name = Clean(input.NameAlias), Description = Clean(input.Description) },
    ];

    private static EcoResProductDto Map(EcoResProduct product, IEnumerable<EcoResProductTranslation> translations)
    {
        var rows = translations.Where(x => x.Product == product.RecId).ToList();
        var english = rows.FirstOrDefault(x => x.LanguageId == "en-us") ?? rows.FirstOrDefault();
        var arabic = rows.FirstOrDefault(x => x.LanguageId.StartsWith("ar"));
        return new EcoResProductMappingSource(product, english, arabic).Adapt<EcoResProductDto>();
    }

    private static string NormalizeNumber(string? value) => Clean(value).ToUpperInvariant();
    private static ItemType ParseProductType(string? value) => Enum.TryParse<ItemType>(value, true, out var type) ? type : ItemType.Item;
    private static string Clean(string? value) => value?.Trim() ?? string.Empty;
    private async Task<string?> ValidateAsync(EcoResProductInputDto input, CancellationToken ct) =>
        (await _validator.ValidateAsync(input, ct)).Errors.FirstOrDefault()?.ErrorMessage;
}
