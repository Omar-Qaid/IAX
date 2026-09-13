using System.Xml.Linq;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private const string RequiredRule = "<Validation><Required>true</Required></Validation>";

    private static async Task SeedOptionsAndValidationsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        // ---------------------------------------------------------
        // 1. WfRequestControls Options and Validations
        // ---------------------------------------------------------
        var requestControls = await db.WfRequestControls.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 603L)
            .ToListAsync(ct);

        foreach (var ctrl in requestControls)
        {
            // Extract Options from XML
            if (!string.IsNullOrWhiteSpace(ctrl.ExtendedProperties) && ctrl.ExtendedProperties.Contains("<Data>"))
            {
                try
                {
                    var doc = XDocument.Parse(ctrl.ExtendedProperties);
                    var items = doc.Descendants("Item").ToList();
                    var existingOptions = await db.WfRequestControlsOptions.IgnoreQueryFilters()
                        .Where(x => x.RequestControlId == ctrl.RecId).ToListAsync(ct);
                    
                    int sortOrder = 1;
                    foreach (var item in items)
                    {
                        var enName = item.Element("en")?.Value ?? item.Element("value")?.Value ?? "Option";
                        var arName = item.Element("ar")?.Value;
                        var val = item.Element("value")?.Value ?? enName;

                        if (!existingOptions.Any(x => x.Value == val))
                        {
                            db.WfRequestControlsOptions.Add(new WfRequestControlsOption
                            {
                                RequestControlId = ctrl.RecId,
                                Name = enName,
                                NameAlias = arName,
                                Value = val,
                                SortOrder = sortOrder,
                                Score = 0,
                                CreatedBy = owner,
                                OwnerAccountId = owner,
                                IsActive = true
                            });
                        }
                        sortOrder++;
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }

            // Extract Validations from ValidationRules flag
            if (ctrl.ValidationRules == RequiredRule)
            {
                var existingVals = await db.WfRequestControlsValidations.IgnoreQueryFilters()
                    .Where(x => x.RequestControlId == ctrl.RecId && x.ValidationType == "Required").ToListAsync(ct);
                
                if (!existingVals.Any())
                {
                    db.WfRequestControlsValidations.Add(new WfRequestControlsValidation
                    {
                        RequestControlId = ctrl.RecId,
                        ValidationType = "Required",
                        ErrorMessage = "This field is required.",
                        ErrorMessageAlias = "هذا الحقل مطلوب.",
                        Severity = "Error",
                        SortOrder = 1,
                        CreatedBy = owner,
                        OwnerAccountId = owner,
                        IsActive = true
                    });
                }
            }
        }

        // ---------------------------------------------------------
        // 2. WfActivityControls Options and Validations
        // ---------------------------------------------------------
        var activityControls = await db.WfActivityControls.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 603L)
            .ToListAsync(ct);

        foreach (var ctrl in activityControls)
        {
            // Extract Options from XML
            if (!string.IsNullOrWhiteSpace(ctrl.ExtendedProperties) && ctrl.ExtendedProperties.Contains("<Data>"))
            {
                try
                {
                    var doc = XDocument.Parse(ctrl.ExtendedProperties);
                    var items = doc.Descendants("Item").ToList();
                    var existingOptions = await db.WfActivityControlsOptions.IgnoreQueryFilters()
                        .Where(x => x.ActivityControlId == ctrl.RecId).ToListAsync(ct);
                    
                    int sortOrder = 1;
                    foreach (var item in items)
                    {
                        var enName = item.Element("en")?.Value ?? item.Element("value")?.Value ?? "Option";
                        var arName = item.Element("ar")?.Value;
                        var val = item.Element("value")?.Value ?? enName;

                        if (!existingOptions.Any(x => x.Value == val))
                        {
                            db.WfActivityControlsOptions.Add(new WfActivityControlsOption
                            {
                                ActivityControlId = ctrl.RecId,
                                Name = enName,
                                NameAlias = arName,
                                Value = val,
                                SortOrder = sortOrder,
                                CreatedBy = owner,
                                OwnerAccountId = owner,
                                IsActive = true
                            });
                        }
                        sortOrder++;
                    }
                }
                catch { }
            }

            // Extract Validations from ValidationRules flag
            if (ctrl.ValidationRules == RequiredRule)
            {
                var existingVals = await db.WfActivityControlsValidations.IgnoreQueryFilters()
                    .Where(x => x.ActivityControlId == ctrl.RecId && x.ValidationType == "Required").ToListAsync(ct);
                
                if (!existingVals.Any())
                {
                    db.WfActivityControlsValidations.Add(new WfActivityControlsValidation
                    {
                        ActivityControlId = ctrl.RecId,
                        Code = "VAL_" + ctrl.RecId,
                        Name = "Required Validation",
                        ValidationType = "Required",
                        ErrorMessage = "This field is required.",
                        NameAlias = "هذا الحقل مطلوب.",
                        Severity = "Error",
                        SortOrder = 1,
                        CreatedBy = owner,
                        OwnerAccountId = owner,
                        IsActive = true
                    });
                }
            }
        }

        // Save everything
        await db.SaveChangesAsync(ct);
    }
}
