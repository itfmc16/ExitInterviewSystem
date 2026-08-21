namespace ExitInterviewSystem.Helpers
{
    /// <summary>
    /// Accurate local time for South Africa (SAST, UTC+2).
    /// Use AppTime.Now everywhere instead of DateTime.Now / UtcNow.
    /// </summary>
    public static class AppTime
    {
        private static readonly TimeZoneInfo SaZone = GetSaZone();

        private static TimeZoneInfo GetSaZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    OperatingSystem.IsWindows()
                        ? "South Africa Standard Time"
                        : "Africa/Johannesburg");
            }
            catch
            {
                // Fixed UTC+2 if zone not found
                return TimeZoneInfo.CreateCustomTimeZone(
                    "SAST", TimeSpan.FromHours(2), "South Africa Standard Time", "SAST");
            }
        }

        public static DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SaZone);

        public static DateTime Today => Now.Date;
    }
}
