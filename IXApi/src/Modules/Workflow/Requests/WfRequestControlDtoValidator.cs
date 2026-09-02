using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestControlDtoValidator : BaseValidator<WfRequestControlDto>
    {
        public WfRequestControlDtoValidator()
        {
             RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
             RuleFor(x => x.ReferenceType)
                 .Must(value => value == null || ReportingMetadata.ReferenceTypes.Contains(value))
                 .WithMessage("ReferenceType is not supported");
             RuleFor(x => x.FieldRole)
                 .Must(ReportingMetadata.FieldRoles.Contains)
                 .WithMessage("FieldRole must be Dimension, Measure, or Both");
             RuleFor(x => x.DataType)
                 .Must(ReportingMetadata.DataTypes.Contains)
                 .WithMessage("DataType is not supported");
             RuleFor(x => x.DefaultAggregation)
                 .Must(ReportingMetadata.Aggregations.Contains)
                 .WithMessage("DefaultAggregation is not supported");
        }
    }

    internal static class ReportingMetadata
    {
        public static readonly HashSet<string> ReferenceTypes = new(StringComparer.Ordinal)
        {
            "Lookup", "Employee", "Showroom", "Branch", "Company", "Department", "BusinessUnit",
            "Area", "City", "Country", "Location", "Customer", "Vendor", "Item", "ItemGroup",
            "Category", "Warehouse", "PaymentMethod", "ViolationType", "Invoice", "PurchaseOrder",
            "SalesOrder", "Process", "User"
        };
        public static readonly HashSet<string> FieldRoles = new(StringComparer.Ordinal)
            { "Dimension", "Measure", "Both" };
        public static readonly HashSet<string> DataTypes = new(StringComparer.Ordinal)
            { "String", "Integer", "Decimal", "Date", "DateTime", "Time", "Boolean" };
        public static readonly HashSet<string> Aggregations = new(StringComparer.Ordinal)
            { "NONE", "SUM", "COUNT", "COUNT_DISTINCT", "AVG", "MIN", "MAX" };
    }
}


