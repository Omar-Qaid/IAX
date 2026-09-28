using System.Text.Json;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using IAX.IXApi.Modules.Organization.DocumentManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Requests;

public partial class WfRequestService
{
    internal const int SubmissionValueLimit = 1_000_000;

    internal static readonly HashSet<string> SubmissionRuleTypes = new(StringComparer.Ordinal)
    {
        "required", "minlength", "maxlength", "exactlength", "length", "minvalue", "maxvalue",
        "mindate", "maxdate", "range", "regex", "pattern", "email", "url", "phone", "saudimobile",
        "saudinationalid", "saudiiban", "taxnumber", "passport", "startswith", "endswith", "contains",
        "fileextensions", "fileextension", "allowedextensions", "allowedtypes", "filesize", "maxfilesize",
        "minselected", "maxselected", "maxfiles", "compare", "comparison", "crossfield", "expression",
        "custom", "customexpression", "conditional", "mask", "inputmask", "unique", "uniqueglobal", "uniqueperapplicant"
    };

    private async Task LockSubmissionAsync(long processId, CancellationToken ct)
    {
        // The lock is database-wide, company-scoped and held until commit/rollback.
        // It serializes check-and-insert across API instances, including absent values.
        if (!_unitOfWork.Context.Database.IsSqlServer()) return;
        var resource = $"Workflow.Submit:{_currentUser.GetDataAreaId()?.ToUpperInvariant()}:{processId}";
        await _unitOfWork.Context.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive',
                @LockOwner='Transaction', @LockTimeout=15000;
            IF @result < 0 THROW 51000, 'Could not acquire workflow submission lock. Please retry.', 1;
            """, ct);
    }

    internal static bool SelectionIsValid(DynamicRequestControlDto control, string value)
    {
        // Table options describe columns, not selectable answers.
        var type = Normalize(control.ControlType);
        var reference = string.Equals(control.ReferenceType, "Employee", StringComparison.OrdinalIgnoreCase)
            || string.Equals(control.ReferenceType, "Showroom", StringComparison.OrdinalIgnoreCase);
        if (type == "table" || control.Options.Count == 0 && !reference && type is not ("select" or "radio" or "checkboxlist" or "employeeid" or "employeesearch" or "showroom"))
            return true;
        if (IsEmpty(value)) return true;
        var selected = SelectedValues(value);
        return selected.Count > 0 && (type == "checkboxlist" || selected.Count == 1)
            && selected.All(answer => control.Options.Any(option => option.Value == answer));
    }

    internal static void NormalizeUploadedFiles(DynamicRequestFormDto form, SubmitDynamicRequestDto submission)
    {
        if (submission.UploadTargets.Count != submission.Files.Count)
            throw new ArgumentException("Every uploaded file must have exactly one attachment target.");
        if (submission.Files.Any(file => file.Length <= 0))
            throw new ArgumentException("Attachments must contain a non-empty file.");
        if (form.MandatoryDocuments && submission.Files.Count == 0)
            throw new ArgumentException("This process requires at least one document attachment.");

        string Metadata(IEnumerable<int> indexes) => JsonSerializer.Serialize(indexes.Select(index => new
        {
            n = Path.GetFileName(submission.Files[index].FileName),
            s = submission.Files[index].Length,
            t = submission.Files[index].ContentType
        }));

        foreach (var control in form.Controls.Where(control => Normalize(control.ControlType) == "file"))
        {
            var indexes = Enumerable.Range(0, submission.Files.Count).Where(index =>
                submission.UploadTargets[index].RequestControlId == control.RequestControlId
                && submission.UploadTargets[index].OptionId == null).ToList();
            var answer = submission.Values.FirstOrDefault(value => value.RequestControlId == control.RequestControlId);
            if (indexes.Count == 0 && !IsEmpty(answer?.Value ?? control.DefaultValue))
                throw new ArgumentException($"{control.Label}: upload the actual files, not only file metadata.");
            if (indexes.Count > 0)
            {
                if (control.ReadOnly) throw new ArgumentException($"{control.Label} is read-only.");
                if (answer == null) submission.Values.Add(new() { RequestControlId = control.RequestControlId, Value = Metadata(indexes) });
                else answer.Value = Metadata(indexes);
            }
        }
        foreach (var group in submission.UploadTargets.Select((target, index) => (target, index))
                     .Where(item => item.target.OptionId.HasValue).GroupBy(item => item.target.OptionId!.Value))
        {
            var answer = submission.OptionFeatureValues.FirstOrDefault(value => value.OptionId == group.Key);
            if (answer == null) submission.OptionFeatureValues.Add(new() { OptionId = group.Key, FileValue = Metadata(group.Select(item => item.index)) });
            else answer.FileValue = Metadata(group.Select(item => item.index));
        }
        foreach (var answer in submission.OptionFeatureValues)
            if (!IsEmpty(answer.FileValue) && !submission.UploadTargets.Any(target => target.OptionId == answer.OptionId))
                throw new ArgumentException("Upload the actual option files, not only file metadata.");
    }

    private static void ValidateUploadTargets(SubmitDynamicRequestDto submission,
        List<DynamicRequestControlDto> visible,
        List<(DynamicRequestControlDto Control, DynamicRequestOptionDto Option)> selectedOptions)
    {
        foreach (var target in submission.UploadTargets)
        {
            if (target.RequestControlId == null && target.OptionId == null) continue;
            var control = visible.FirstOrDefault(item => item.RequestControlId == target.RequestControlId);
            if (control == null || (target.OptionId == null
                    ? Normalize(control.ControlType) != "file"
                    : !selectedOptions.Any(item => item.Control.RequestControlId == control.RequestControlId
                        && item.Option.OptionId == target.OptionId && item.Option.FeatureConfiguration.RequireFileUpload)))
                throw new ArgumentException("An attachment targets an unavailable field or option.");
        }
    }

    private async Task SaveSubmissionFilesAsync(SubmitDynamicRequestDto submission, long requestId,
        List<DynamicRequestAttachmentOwnerDto> owners, List<string> storedPaths, CancellationToken ct)
    {
        for (var index = 0; index < submission.Files.Count; index++)
        {
            var file = submission.Files[index];
            var target = submission.UploadTargets[index];
            var detailId = target.RequestControlId.HasValue
                ? owners.Single(owner => owner.RequestControlId == target.RequestControlId && owner.OptionId == target.OptionId).DetailRecId
                : requestId;
            await using var stream = file.OpenReadStream();
            var document = await _documents.CreateAsync(new CreateDocumentCommand
            {
                RefTableId = target.RequestControlId.HasValue ? 1002 : 1001,
                RefRecId = detailId, TypeId = "File", Name = file.FileName,
                FileName = file.FileName, FileSize = file.Length, MimeType = file.ContentType, Content = stream
            }, ct);
            var path = _context.Set<DocuValue>().Local.Single(value => value.RecId == document.ValueRecId).Path;
            if (!string.IsNullOrEmpty(path)) storedPaths.Add(path);
        }
    }
}
