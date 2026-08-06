using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    [ScopedService]
    public class ValidationEngine : IValidationEngine
    {
        private readonly ApplicationDbContext _context;

        public ValidationEngine(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ValidationResult>> ValidateRequestAsync(long processId, long? requestId, List<WfRequestDetail> requestDetails)
        {
            var results = new List<ValidationResult>();

            // Fetch active validations for the request controls of this process
            var validations = await _context.WfRequestControlsValidations
                .Include(v => v.RequestControl)
                .Where(v => v.RequestControl.ProcessId == processId && v.IsActive)
                .OrderBy(v => v.SortOrder)
                .ToListAsync();

            // Create a lookup for submitted controls by their Code
            var controlsList = await _context.WfRequestControls
                .Where(c => c.ProcessId == processId)
                .ToListAsync();

            var detailLookup = new Dictionary<string, string>(); // code -> value
            var idLookup = new Dictionary<long, string>(); // requestControlId -> value
            
            foreach (var detail in requestDetails)
            {
                if (detail.ControlDataId.HasValue)
                {
                    idLookup[detail.ControlDataId.Value] = detail.ControlValue ?? "";
                    var ctrl = controlsList.FirstOrDefault(c => c.RecId == detail.ControlDataId.Value);
                    if (ctrl != null && !string.IsNullOrEmpty(ctrl.Code))
                    {
                        detailLookup[ctrl.Code.ToLower()] = detail.ControlValue ?? "";
                    }
                }
            }

            foreach (var validation in validations)
            {
                var requestControlId = validation.RequestControlId;
                var control = validation.RequestControl;
                if (control == null) continue;

                idLookup.TryGetValue(requestControlId, out var value);
                value ??= "";

                bool isValid = true;
                string type = validation.ValidationType.ToLower();

                switch (type)
                {
                    case "required":
                        isValid = !string.IsNullOrWhiteSpace(value);
                        break;

                    case "minlength":
                        if (int.TryParse(validation.ValidationExpression, out int minLen))
                        {
                            isValid = value.Length >= minLen;
                        }
                        break;

                    case "maxlength":
                        if (int.TryParse(validation.ValidationExpression, out int maxLen))
                        {
                            isValid = value.Length <= maxLen;
                        }
                        break;

                    case "regex":
                        if (!string.IsNullOrEmpty(validation.ValidationExpression))
                        {
                            try
                            {
                                isValid = Regex.IsMatch(value, validation.ValidationExpression);
                            }
                            catch { isValid = false; }
                        }
                        break;

                    case "minvalue":
                        if (decimal.TryParse(value, out decimal valMin) && decimal.TryParse(validation.ValidationExpression, out decimal limitMin))
                        {
                            isValid = valMin >= limitMin;
                        }
                        else
                        {
                            isValid = false;
                        }
                        break;

                    case "maxvalue":
                        if (decimal.TryParse(value, out decimal valMax) && decimal.TryParse(validation.ValidationExpression, out decimal limitMax))
                        {
                            isValid = valMax <= limitMax;
                        }
                        else
                        {
                            isValid = false;
                        }
                        break;

                    case "range":
                        var rangeParts = (validation.ValidationExpression ?? "").Split('-');
                        if (rangeParts.Length == 2 && decimal.TryParse(value, out decimal valRange) &&
                            decimal.TryParse(rangeParts[0], out decimal rMin) && decimal.TryParse(rangeParts[1], out decimal rMax))
                        {
                            isValid = valRange >= rMin && valRange <= rMax;
                        }
                        else
                        {
                            isValid = false;
                        }
                        break;

                    case "email":
                        isValid = string.IsNullOrEmpty(value) || Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                        break;

                    case "phone":
                        isValid = string.IsNullOrEmpty(value) || Regex.IsMatch(value, @"^[+0-9\s-]{7,15}$");
                        break;

                    case "unique":
                        if (!string.IsNullOrEmpty(value))
                        {
                            // If it's a new request, requestId is null or 0.
                            var query = _context.WfRequestDetails
                                .Where(d => d.ControlDataId == requestControlId && d.ControlValue == value);

                            if (requestId.HasValue && requestId.Value > 0)
                            {
                                query = query.Where(d => d.RequestId != requestId.Value);
                            }

                            var exists = await query.AnyAsync();
                            isValid = !exists;
                        }
                        break;

                    case "exists":
                        var existsParts = (validation.ValidationExpression ?? "").Split('.');
                        if (existsParts.Length == 2 && !string.IsNullOrEmpty(value))
                        {
                            var tableName = existsParts[0];
                            var columnName = existsParts[1];
                            isValid = await CheckDatabaseExistenceAsync(tableName, columnName, value);
                        }
                        break;

                    case "expression":
                    case "customexpression":
                    case "crossfield":
                    case "conditional":
                        if (!string.IsNullOrEmpty(validation.ValidationExpression))
                        {
                            isValid = EvaluateExpression(validation.ValidationExpression, value, detailLookup);
                        }
                        break;

                    case "filesize":
                    case "fileextension":
                        isValid = true;
                        break;

                    case "daterange":
                        var dateParts = (validation.ValidationExpression ?? "").Split('|');
                        if (dateParts.Length == 2 && DateTime.TryParse(value, out DateTime valDate) &&
                            DateTime.TryParse(dateParts[0], out DateTime dFrom) && DateTime.TryParse(dateParts[1], out DateTime dTo))
                        {
                            isValid = valDate >= dFrom && valDate <= dTo;
                        }
                        else
                        {
                            isValid = false;
                        }
                        break;
                }

                if (!isValid)
                {
                    results.Add(new ValidationResult
                    {
                        RequestControlId = requestControlId,
                        ControlName = control.Name,
                        ErrorMessageAr = validation.ErrorMessageAr,
                        ErrorMessageEn = validation.ErrorMessageEn,
                        Severity = validation.Severity
                    });
                }
            }

            return results;
        }

        private async Task<bool> CheckDatabaseExistenceAsync(string tableName, string columnName, string value)
        {
            var sql = $"SELECT COUNT(1) FROM [{tableName.Replace("]", "]]")}] WHERE [{columnName.Replace("]", "]]")}] = @p0";
            try
            {
                var count = await _context.Database.SqlQueryRaw<int>(sql, value).FirstOrDefaultAsync();
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        private bool EvaluateExpression(string expression, string currentValue, Dictionary<string, string> allValues)
        {
            var expr = expression.Replace("{value}", currentValue);

            foreach (var kv in allValues)
            {
                expr = expr.Replace($"{{{kv.Key}}}", kv.Value);
            }

            expr = expr.Replace(" ", "");

            try
            {
                string[] ops = new[] { "==", "!=", ">=", "<=", ">", "<" };
                string matchedOp = ops.FirstOrDefault(op => expr.Contains(op));
                if (matchedOp == null) return false;

                var sides = expr.Split(new[] { matchedOp }, StringSplitOptions.None);
                if (sides.Length != 2) return false;

                var left = sides[0];
                var right = sides[1];

                if (decimal.TryParse(left, out decimal leftNum) && decimal.TryParse(right, out decimal rightNum))
                {
                    return matchedOp switch
                    {
                        "==" => leftNum == rightNum,
                        "!=" => leftNum != rightNum,
                        ">=" => leftNum >= rightNum,
                        "<=" => leftNum <= rightNum,
                        ">" => leftNum > rightNum,
                        "<" => leftNum < rightNum,
                        _ => false
                    };
                }

                left = left.Trim('\'', '"');
                right = right.Trim('\'', '"');

                return matchedOp switch
                {
                    "==" => left.Equals(right, StringComparison.OrdinalIgnoreCase),
                    "!=" => !left.Equals(right, StringComparison.OrdinalIgnoreCase),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }
    }
}

