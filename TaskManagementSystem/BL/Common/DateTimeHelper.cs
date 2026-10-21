namespace TaskManagementSystem.BL.Common
{
    public static class DateTimeHelper
    {
        private static readonly Lazy<TimeZoneInfo> _egyptTimeZone = new(() =>
        {
            try
            {
                // Standard Windows identifier
                return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            }
            catch
            {
                try
                {
                    // Standard IANA / Linux identifier
                    return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
                }
                catch
                {
                    return TimeZoneInfo.Local;
                }
            }
        });

        public static TimeZoneInfo EgyptTimeZone => _egyptTimeZone.Value;

        /// <summary>
        /// Gets current date and time in Egypt Time (or machine local time).
        /// </summary>
        public static DateTime NowInEgypt()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptTimeZone);
        }

        /// <summary>
        /// Converts a UTC or unspecified DateTime to Egypt Time.
        /// </summary>
        public static DateTime ToEgyptTime(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dt, EgyptTimeZone);
            }
            if (dt.Kind == DateTimeKind.Unspecified)
            {
                var utc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                return TimeZoneInfo.ConvertTimeFromUtc(utc, EgyptTimeZone);
            }
            return TimeZoneInfo.ConvertTime(dt, EgyptTimeZone);
        }

        /// <summary>
        /// Formats date for display in Egypt time.
        /// </summary>
        public static string FormatEgypt(DateTime dt, string format = "yyyy-MM-dd hh:mm tt")
        {
            return ToEgyptTime(dt).ToString(format);
        }
    }
}
