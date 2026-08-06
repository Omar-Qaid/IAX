using System.Globalization;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    /// <summary>
    /// A lightweight, dependency-free parser/evaluator for standard 5-field CRON expressions.
    /// Fields (in order): minute, hour, day-of-month, month, day-of-week.
    ///
    /// Supported per field:
    ///   *          any value
    ///   5          a single value
    ///   1,3,5      a comma-separated list
    ///   1-5        an inclusive range
    ///   */15       a step over the whole range
    ///   1-10/2     a step over a range
    ///
    /// Day-of-week: 0 or 7 = Sunday. Months: 1-12. This keeps the system free of
    /// external NuGet dependencies (consistent with the notification background service).
    /// </summary>
    public sealed class SysCronExpression
    {
        private readonly bool[] _minutes = new bool[60];
        private readonly bool[] _hours = new bool[24];
        private readonly bool[] _daysOfMonth = new bool[32]; // 1..31
        private readonly bool[] _months = new bool[13];      // 1..12
        private readonly bool[] _daysOfWeek = new bool[7];   // 0..6 (Sun..Sat)

        public string Expression { get; }

        private SysCronExpression(string expression) => Expression = expression;

        /// <summary>
        /// Parses a 5-field CRON expression. Throws <see cref="FormatException"/> on invalid input.
        /// </summary>
        public static SysCronExpression Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new FormatException("CRON expression is empty.");

            var parts = expression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5)
                throw new FormatException(
                    $"CRON expression must have exactly 5 fields (got {parts.Length}): '{expression}'.");

            var cron = new SysCronExpression(expression);
            ParseField(parts[0], 0, 59, cron._minutes);
            ParseField(parts[1], 0, 23, cron._hours);
            ParseField(parts[2], 1, 31, cron._daysOfMonth);
            ParseField(parts[3], 1, 12, cron._months);
            ParseDayOfWeek(parts[4], cron._daysOfWeek);
            return cron;
        }

        /// <summary>Returns true if the expression is syntactically valid.</summary>
        public static bool TryParse(string expression, out SysCronExpression? cron)
        {
            try { cron = Parse(expression); return true; }
            catch { cron = null; return false; }
        }

        /// <summary>
        /// Computes the next occurrence strictly after <paramref name="after"/>.
        /// Returns null if no occurrence is found within the next ~4 years (safety bound).
        /// </summary>
        public DateTime? GetNextOccurrence(DateTime after)
        {
            // Start from the next whole minute after the reference time.
            var candidate = new DateTime(after.Year, after.Month, after.Day, after.Hour, after.Minute, 0, after.Kind)
                .AddMinutes(1);

            var limit = candidate.AddYears(4);
            while (candidate < limit)
            {
                if (_months[candidate.Month]
                    && _hours[candidate.Hour]
                    && _minutes[candidate.Minute]
                    && DayMatches(candidate))
                {
                    return candidate;
                }
                candidate = candidate.AddMinutes(1);
            }
            return null;
        }

        /// <summary>
        /// Day-of-month and day-of-week are OR'd together when both are restricted,
        /// which matches standard cron (Vixie) semantics.
        /// </summary>
        private bool DayMatches(DateTime dt)
        {
            bool domRestricted = !_daysOfMonth.Skip(1).All(b => b); // any false in 1..31 => restricted
            bool dowRestricted = !_daysOfWeek.All(b => b);

            bool domMatch = _daysOfMonth[dt.Day];
            bool dowMatch = _daysOfWeek[(int)dt.DayOfWeek];

            if (domRestricted && dowRestricted) return domMatch || dowMatch;
            if (domRestricted) return domMatch;
            if (dowRestricted) return dowMatch;
            return true;
        }

        private static void ParseField(string field, int min, int max, bool[] target)
        {
            foreach (var token in field.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                int step = 1;
                var range = token;

                var slash = token.Split('/');
                if (slash.Length == 2)
                {
                    range = slash[0];
                    step = int.Parse(slash[1], CultureInfo.InvariantCulture);
                    if (step <= 0) throw new FormatException($"Invalid step '{token}'.");
                }

                int rangeStart, rangeEnd;
                if (range == "*")
                {
                    rangeStart = min;
                    rangeEnd = max;
                }
                else if (range.Contains('-'))
                {
                    var bounds = range.Split('-');
                    rangeStart = int.Parse(bounds[0], CultureInfo.InvariantCulture);
                    rangeEnd = int.Parse(bounds[1], CultureInfo.InvariantCulture);
                }
                else
                {
                    rangeStart = rangeEnd = int.Parse(range, CultureInfo.InvariantCulture);
                }

                if (rangeStart < min || rangeEnd > max || rangeStart > rangeEnd)
                    throw new FormatException($"Field value '{token}' out of range [{min}-{max}].");

                for (int v = rangeStart; v <= rangeEnd; v += step)
                    target[v] = true;
            }
        }

        private static void ParseDayOfWeek(string field, bool[] target)
        {
            // Normalise 7 -> 0 (Sunday) before parsing into a 0..6 array.
            foreach (var token in field.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                int step = 1;
                var range = token;

                var slash = token.Split('/');
                if (slash.Length == 2)
                {
                    range = slash[0];
                    step = int.Parse(slash[1], CultureInfo.InvariantCulture);
                    if (step <= 0) throw new FormatException($"Invalid step '{token}'.");
                }

                int start, end;
                if (range == "*")
                {
                    start = 0; end = 6;
                }
                else if (range.Contains('-'))
                {
                    var bounds = range.Split('-');
                    start = Normalize(int.Parse(bounds[0], CultureInfo.InvariantCulture));
                    end = Normalize(int.Parse(bounds[1], CultureInfo.InvariantCulture));
                }
                else
                {
                    start = end = Normalize(int.Parse(range, CultureInfo.InvariantCulture));
                }

                if (start <= end)
                {
                    for (int v = start; v <= end; v += step) target[v] = true;
                }
                else
                {
                    // wrapped range e.g. 6-7 -> 6,0
                    for (int v = start; v <= 6; v += step) target[v] = true;
                    for (int v = 0; v <= end; v += step) target[v] = true;
                }
            }

            static int Normalize(int dow)
            {
                if (dow < 0 || dow > 7) throw new FormatException($"Day-of-week '{dow}' out of range [0-7].");
                return dow == 7 ? 0 : dow;
            }
        }
    }
}
