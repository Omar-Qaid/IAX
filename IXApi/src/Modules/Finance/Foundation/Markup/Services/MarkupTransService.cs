using FluentValidation;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

public sealed class MarkupTransService(
    IUnitOfWork unitOfWork,
    ICompanyExecutionContext company,
    IMarkupTransCommandService commands,
    IValidator<MarkupTransDto> validator) : IMarkupTransService
{
    private const int SalesTableId = 2002;
    private const int SalesLineId = 2003;
    private DbContext _db => unitOfWork.Context;
    private readonly ICompanyExecutionContext _company = company;
    private readonly IMarkupTransCommandService _commands = commands;
    private readonly IValidator<MarkupTransDto> _validator = validator;

    public async Task<MarkupResult<IEnumerable<MarkupTransDto>>> GetDocumentChargesAsync(
        string documentType,
        long documentRecId,
        string level = "header",
        CancellationToken cancellationToken = default)
    {
        var moduleType = ParseDocumentType(documentType);
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        if (moduleType == null)
            return new(default, new("Document type must be sales or purchase."));
        var transTableId = ResolveTransTableId(moduleType.Value, level);
        if (transTableId == null)
            return new(default, new("Charge level must be header or line."));

        var targetError = await ValidateTarget(moduleType.Value, transTableId.Value, documentRecId, dataAreaId, cancellationToken);
        if (targetError != null) return new(default, new(targetError, true));

        var entities = await _db.Set<MarkupTrans>()
            .AsNoTracking()
            .Where(x => x.TransRecId == documentRecId
                && (moduleType == MarkupModuleType.Customer
                    ? x.ModuleType == MarkupModuleType.Customer || x.ModuleType == MarkupModuleType.Sales
                    : x.ModuleType == moduleType)
                && (x.TransTableId == transTableId
                    || (transTableId == SalesTableId && x.TransTableId == 0))
                && x.DataAreaId == dataAreaId)
            .OrderBy(x => x.LineNum)
            .ToListAsync(cancellationToken);
        var records = entities.Adapt<List<MarkupTransDto>>();

        return new(records);
    }

    public async Task<MarkupResult<IEnumerable<MarkupCodeDto>>> GetChargeCodesAsync(string documentType, CancellationToken cancellationToken = default)
    {
        var moduleType = ParseDocumentType(documentType);
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        if (moduleType == null)
            return new(default, new("Document type must be sales or purchase."));

        var setupModule = moduleType == MarkupModuleType.Customer
            ? ModuleInventPurchSales.Sales
            : ModuleInventPurchSales.Purchase;
        var codes = await _db.Set<MarkupTable>().AsNoTracking()
            .Where(x => x.ModuleType == setupModule && x.DataAreaId == dataAreaId)
            .OrderBy(x => x.MarkupCode)
            .ToListAsync(cancellationToken);
        return new(codes.Adapt<List<MarkupCodeDto>>());
    }

    public async Task<MarkupResult<MarkupTransDto>> CreateAsync(
        string documentType,
        long documentRecId,
        MarkupTransDto input,
        string level = "header",
        CancellationToken cancellationToken = default)
    {
        var moduleType = ParseDocumentType(documentType);
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        if (moduleType == null)
            return new(default, new("Document type must be sales or purchase."));
        var transTableId = ResolveTransTableId(moduleType.Value, level);
        if (transTableId == null)
            return new(default, new("Charge level must be header or line."));
        var validation = await ValidateInput(moduleType.Value, transTableId.Value, documentRecId, input, cancellationToken);
        if (validation != null) return new(default, validation);

        var entity = await _commands.CreateAsync(moduleType.Value, transTableId.Value, documentRecId, input, cancellationToken);
        return new(entity.Adapt<MarkupTransDto>());
    }

    public async Task<MarkupResult<MarkupTransDto>> UpdateAsync(long id, MarkupTransDto input, CancellationToken cancellationToken = default)
    {
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        var entity = await _db.Set<MarkupTrans>().FirstOrDefaultAsync(
            x => x.RecId == id && x.DataAreaId == dataAreaId,
            cancellationToken);
        if (entity == null) return new(default, new("Charge transaction not found.", true));
        var validation = await ValidateInput(entity.ModuleType, entity.TransTableId, entity.TransRecId, input, cancellationToken);
        if (validation != null) return new(default, validation);
        entity = await _commands.UpdateAsync(entity, input, cancellationToken);
        return new(entity.Adapt<MarkupTransDto>());
    }

    public async Task<MarkupResult<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        var entity = await _db.Set<MarkupTrans>().FirstOrDefaultAsync(
            x => x.RecId == id && x.DataAreaId == dataAreaId,
            cancellationToken);
        if (entity == null) return new(default, new("Charge transaction not found.", true));
        await _commands.DeleteAsync(entity, cancellationToken);
        return new(true);
    }

    private async Task<MarkupServiceError?> ValidateInput(MarkupModuleType moduleType, int transTableId, long documentRecId, MarkupTransDto input, CancellationToken ct)
    {
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        var validation = await _validator.ValidateAsync(input, ct);
        if (!validation.IsValid) return new(validation.Errors[0].ErrorMessage);
        var isSalesCharge = moduleType is MarkupModuleType.Customer or MarkupModuleType.Sales;
        var setupModule = isSalesCharge ? ModuleInventPurchSales.Sales : ModuleInventPurchSales.Purchase;
        if (!await _db.Set<MarkupTable>().AsNoTracking().AnyAsync(
                x => x.MarkupCode == input.MarkupCode
                    && x.ModuleType == setupModule
                    && x.DataAreaId == dataAreaId,
                ct))
            return new("The selected charges code is not valid for this document type.");
        var targetError = await ValidateTarget(moduleType, transTableId, documentRecId, dataAreaId, ct);
        if (targetError != null)
            return new(targetError, true);
        return null;
    }

    private async Task<string?> ValidateTarget(
        MarkupModuleType moduleType,
        int transTableId,
        long documentRecId,
        string dataAreaId,
        CancellationToken ct)
    {
        if (moduleType is not (MarkupModuleType.Customer or MarkupModuleType.Sales)) return null;

        if (transTableId == SalesLineId)
            return await _db.Set<SalesLine>().AsNoTracking().AnyAsync(
                x => x.RecId == documentRecId && x.DataAreaId == dataAreaId,
                ct)
                ? null
                : "Sales order line not found.";

        return await _db.Set<SalesTable>().AsNoTracking().AnyAsync(
            x => x.RecId == documentRecId && x.DataAreaId == dataAreaId,
            ct)
            ? null
            : "Sales order not found.";
    }

    private static MarkupModuleType? ParseDocumentType(string value) => value.ToLowerInvariant() switch
    {
        "sales" => MarkupModuleType.Customer,
        "purchase" => MarkupModuleType.Vendor,
        _ => null
    };

    private static int? ResolveTransTableId(MarkupModuleType moduleType, string level) => (moduleType, level.ToLowerInvariant()) switch
    {
        (MarkupModuleType.Customer or MarkupModuleType.Sales, "header") => SalesTableId,
        (MarkupModuleType.Customer or MarkupModuleType.Sales, "line") => SalesLineId,
        (MarkupModuleType.Vendor, "header") => 0,
        _ => null
    };

}
