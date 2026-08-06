using IAX.IXApi.Shared.Application.Contracts;
using System.Globalization;
using System.Linq.Expressions;

namespace IAX.IXApi.Shared.Application.Querying
{
    public static class DataGrid
    {
        #region Operator Constants
        private static class Operators
        {
            public const string Equals = "equals";
            public const string DoesNotEqual = "doesnotequal";
            public const string Contains = "contains";
            public const string DoesNotContain = "doesnotcontain";
            public const string StartsWith = "startswith";
            public const string EndsWith = "endswith";
            public const string IsEmpty = "isempty";
            public const string IsNotEmpty = "isnotempty";
            public const string IsAnyOf = "isanyof";
            public const string Is = "is";
            public const string Not = "not";
            public const string After = "after";
            public const string OnOrAfter = "onorafter";
            public const string Before = "before";
            public const string OnOrBefore = "onorbefore";
        }

        private static bool IsEmptyValue(string? value) => string.IsNullOrEmpty(value) || value == "undefined";
        #endregion

        #region Data Grid Expression
        private static string Capitalize(string input) => string.IsNullOrEmpty(input) ? input : char.ToUpperInvariant(input[0]) + input.Substring(1);
        private static Expression? DataGridIsAnyOfExpression(Expression property, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(v => v.Trim())
                              .ToList();

            if (values.Count == 0)
                return null;

            var targetType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;

            Expression? combined = null;
            foreach (var val in values)
            {
                try
                {
                    // String comparisons are always lowercased (property is already .ToLower())
                    var normalized = targetType == typeof(string) ? val.ToLowerInvariant() : val;
                    var typedValue = Convert.ChangeType(normalized, targetType);
                    var constant = Expression.Constant(typedValue, property.Type);
                    var equalsExpr = Expression.Equal(property, constant);
                    combined = combined == null ? equalsExpr : Expression.OrElse(combined, equalsExpr);
                }
                catch (Exception)
                {
                    // Skip invalid values - they won't match the filter
                }
            }

            return combined;
        }
        public static Expression<Func<T, object>> DataGridSortExpression<T>(string sortField)
        {
            if (string.IsNullOrEmpty(sortField)) return null!;

            var parameter = Expression.Parameter(typeof(T), "x");

            var property = GetNestedProperty(parameter, sortField);

            Expression body = property.Type.IsValueType ? Expression.Convert(property, typeof(object)) : (Expression)property;

            return Expression.Lambda<Func<T, object>>(body, parameter);
        }
        public static MemberExpression GetNestedProperty(Expression parameter, string propertyPath)
        {
            var props = propertyPath.Split('.');
            Expression current = parameter;

            foreach (var prop in props)
            {
                var type = current.Type;
                var pi = type.GetProperty(
                    prop,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase
                );
                if (pi == null)
                    throw new ArgumentException($"Property '{prop}' not found on type '{type.Name}'");
                current = Expression.Property(current, pi);
            }

            return (MemberExpression)current;
        }
        public static Expression<Func<T, bool>>? DataGridFilterExpression<T>(List<QueryFilterFieldDto> filters)
        {
            if (filters == null || filters.Count == 0)
                return null;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combined = null;

            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Field)) continue;

                var property = GetNestedProperty(parameter, filter.Field);
                var propertyType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;
                var operatorLower = filter.Operator?.ToLowerInvariant(); // Cache toLowerCase result

                // Null-check for nullable members (e.g. long? foreign-key ids). The value-type branch
                // below skips empty values, so without this an isempty/isnotempty on a nullable id
                // (e.g. "employees with no showroom") would silently be a no-op.
                if ((operatorLower == Operators.IsEmpty || operatorLower == Operators.IsNotEmpty)
                    && Nullable.GetUnderlyingType(property.Type) != null
                    && IsEmptyValue(filter.Value))
                {
                    var nullConstant = Expression.Constant(null, property.Type);
                    Expression nullCheck = operatorLower == Operators.IsEmpty
                        ? Expression.Equal(property, nullConstant)
                        : Expression.NotEqual(property, nullConstant);
                    combined = combined == null ? nullCheck : Expression.AndAlso(combined, nullCheck);
                    continue;
                }

                Expression? body = null;
                if (propertyType != typeof(DateOnly) && propertyType != typeof(DateTime) && propertyType.IsValueType)
                {
                    if (IsEmptyValue(filter.Value))
                        continue;

                    ConstantExpression? constant = null;
                    if (operatorLower != Operators.IsAnyOf)
                    {
                        var value = filter.Value ?? (propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null);
                        object converted;
                        if (propertyType == typeof(bool) && value is string sv)
                        {
                            if (bool.TryParse(sv, out var b)) converted = b;
                            else converted = sv == "1" || sv.Equals("yes", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            converted = Convert.ChangeType(value, propertyType)!;
                        }
                        constant = Expression.Constant(converted, property.Type);
                    }
                    body = operatorLower switch
                    {
                        "=" or Operators.Equals => Expression.Equal(property, constant!),
                        "!=" or Operators.DoesNotEqual => Expression.NotEqual(property, constant!),
                        ">" => Expression.GreaterThan(property, constant!),
                        ">=" => Expression.GreaterThanOrEqual(property, constant!),
                        "<" => Expression.LessThan(property, constant!),
                        "<=" => Expression.LessThanOrEqual(property, constant!),
                        Operators.IsEmpty => Expression.Equal(property, constant!),
                        Operators.IsNotEmpty => Expression.NotEqual(property, constant!),
                        Operators.IsAnyOf => DataGridIsAnyOfExpression(property, filter.Value ?? string.Empty),
                        _ => null
                    };
                }
                else if (propertyType == typeof(DateOnly) || propertyType == typeof(DateOnly?))
                {
                    if (IsEmptyValue(filter.Value) && (operatorLower == Operators.IsEmpty || operatorLower == Operators.IsNotEmpty))
                    {
                        var constant = Expression.Constant(null, property.Type);
                        body = operatorLower switch
                        {
                            Operators.IsEmpty => Expression.Equal(property, constant),
                            Operators.IsNotEmpty => Expression.NotEqual(property, constant),
                            _ => null
                        };
                    }
                    else
                    {
                        string dateString = filter.Value ?? string.Empty;
                        string format = "ddd MMM dd yyyy HH:mm:ss 'GMT'zzz";
                        int tzIndex = dateString.IndexOf(" (");
                        if (tzIndex > 0)
                            dateString = dateString.Substring(0, tzIndex);

                        if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture,
                                                   DateTimeStyles.AdjustToUniversal, out DateTime parsedDate))
                        {
                            var dateOnly = DateOnly.FromDateTime(parsedDate);
                            Type targetType = property.Type == typeof(DateOnly?) ? typeof(DateOnly?) : typeof(DateOnly);
                            var constant = Expression.Constant(dateOnly, targetType);

                            body = operatorLower switch
                            {
                                Operators.Is => Expression.Equal(property, constant),
                                Operators.Not => Expression.NotEqual(property, constant),
                                Operators.After => Expression.GreaterThan(property, constant),
                                Operators.OnOrAfter => Expression.GreaterThanOrEqual(property, constant),
                                Operators.Before => Expression.LessThan(property, constant),
                                Operators.OnOrBefore => Expression.LessThanOrEqual(property, constant),
                                _ => null
                            };
                        }
                    }
                }
                else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                {
                    if (IsEmptyValue(filter.Value) && (operatorLower == Operators.IsEmpty || operatorLower == Operators.IsNotEmpty))
                    {
                        var constant = Expression.Constant(null, property.Type);
                        body = operatorLower switch
                        {
                            Operators.IsEmpty => Expression.Equal(property, constant),
                            Operators.IsNotEmpty => Expression.NotEqual(property, constant),
                            _ => null
                        };
                    }
                    else
                    {
                        string dateString = filter.Value ?? string.Empty;
                        string format = "ddd MMM dd yyyy HH:mm:ss 'GMT'zzz";
                        int tzIndex = dateString.IndexOf(" (");
                        if (tzIndex > 0)
                            dateString = dateString.Substring(0, tzIndex);

                        if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime parsedDate))
                        {
                            var dateOnly = parsedDate.Date;
                            var constant = Expression.Constant(dateOnly, typeof(DateTime));

                            // Extract .Value.Date if nullable
                            Expression left = property.Type == typeof(DateTime?)
                                ? Expression.Property(Expression.Property(property, "Value"), "Date")
                                : Expression.Property(property, "Date");

                            body = operatorLower switch
                            {
                                Operators.Is => Expression.Equal(left, constant),
                                Operators.Not => Expression.NotEqual(left, constant),
                                Operators.After => Expression.GreaterThan(left, constant),
                                Operators.OnOrAfter => Expression.GreaterThanOrEqual(left, constant),
                                Operators.Before => Expression.LessThan(left, constant),
                                Operators.OnOrBefore => Expression.LessThanOrEqual(left, constant),
                                _ => null
                            };
                        }
                    }
                }
                else // String type
                {
                    // Null-safe string operations
                    var notNull = Expression.NotEqual(property, Expression.Constant(null));
                    var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
                    var propToLower = Expression.Call(property, toLower);
                    var constStr = Expression.Constant((filter.Value ?? "").ToLowerInvariant());

                    Expression? stringExpr = operatorLower switch
                    {
                        Operators.Contains => Expression.Call(propToLower, nameof(string.Contains), null, constStr),
                        Operators.DoesNotContain => Expression.Not(Expression.Call(propToLower, nameof(string.Contains), null, constStr)),
                        Operators.Equals => Expression.Equal(propToLower, constStr),
                        Operators.DoesNotEqual => Expression.NotEqual(propToLower, constStr),
                        Operators.StartsWith => Expression.Call(propToLower, nameof(string.StartsWith), null, constStr),
                        Operators.EndsWith => Expression.Call(propToLower, nameof(string.EndsWith), null, constStr),
                        Operators.IsEmpty => Expression.Equal(property, Expression.Constant(string.Empty)),
                        Operators.IsNotEmpty => Expression.NotEqual(property, Expression.Constant(string.Empty)),
                        Operators.IsAnyOf => DataGridIsAnyOfExpression(propToLower, filter.Value ?? string.Empty),
                        _ => null
                    };

                    // Combine with null check (except for IsEmpty/IsNotEmpty which need to handle null)
                    if (stringExpr != null && operatorLower != Operators.IsEmpty && operatorLower != Operators.IsNotEmpty)
                    {
                        body = Expression.AndAlso(notNull, stringExpr);
                    }
                    else
                    {
                        body = stringExpr;
                    }
                }

                if (body == null) continue;

                combined = combined == null ? body : Expression.AndAlso(combined, body);
            }

            return combined != null ? Expression.Lambda<Func<T, bool>>(combined, parameter) : null;
        }


        public static string GetPropertyPath<TDto>(Expression<Func<TDto, object>> expression)
        {
            var members = new List<string>();

            Expression? body = expression.Body;

            if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
                body = unary.Operand;

            while (body is MemberExpression member)
            {
                members.Add(member.Member.Name);
                body = member.Expression;
            }

            members.Reverse();
            return string.Join('.', members);
        }
        #endregion
    }
}
