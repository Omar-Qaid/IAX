using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using Mapster;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

public sealed class MarkupTransCommandService : IMarkupTransCommandService
{
    private readonly IUnitOfWork _unitOfWork;
    private DbContext _db => _unitOfWork.Context;
    private readonly ICompanyExecutionContext _company;
    private readonly IMarkupChargeCalculator _calculator;

    public MarkupTransCommandService(IUnitOfWork unitOfWork, ICompanyExecutionContext company, IMarkupChargeCalculator calculator)
    {
        _unitOfWork = unitOfWork;
        _company = company;
        _calculator = calculator;
    }

    public async Task<MarkupTrans> CreateAsync(MarkupModuleType moduleType, int transTableId, long documentRecId, MarkupTransDto input, CancellationToken ct)
    {
        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        var nextLine = await _db.Set<MarkupTrans>()
            .Where(x => x.TransRecId == documentRecId
                && (moduleType == MarkupModuleType.Customer
                    ? x.ModuleType == MarkupModuleType.Customer || x.ModuleType == MarkupModuleType.Sales
                    : x.ModuleType == moduleType)
                && (x.TransTableId == transTableId || (transTableId == 2002 && x.TransTableId == 0))
                && x.DataAreaId == dataAreaId)
            .Select(x => (decimal?)x.LineNum).MaxAsync(ct) ?? 0;
        var entity = new MarkupTrans
        {
            DataAreaId = dataAreaId,
            TransRecId = documentRecId,
            TransTableId = transTableId,
            LineNum = nextLine + 1,
            TransDate = input.TransDate == default ? DateTime.UtcNow.Date : input.TransDate,
            ModuleType = moduleType
        };
        input.Adapt<MarkupTransDto, MarkupTrans>(entity);
        entity.CalculatedAmount = await _calculator.CalculateAmount(entity, ct);
        await _db.Set<MarkupTrans>().AddAsync(entity, ct);
        await _unitOfWork.CompleteAsync(ct);
        return entity;
    }

    public async Task<MarkupTrans> UpdateAsync(MarkupTrans entity, MarkupTransDto input, CancellationToken ct)
    {
        input.Adapt<MarkupTransDto, MarkupTrans>(entity);
        entity.CalculatedAmount = await _calculator.CalculateAmount(entity, ct);
        await _unitOfWork.CompleteAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(MarkupTrans entity, CancellationToken ct)
    {
        _db.Set<MarkupTrans>().Remove(entity);
        await _unitOfWork.CompleteAsync(ct);
    }

}
