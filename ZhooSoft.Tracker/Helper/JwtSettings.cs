namespace ZhooSoft.Tracker.Helper
{
    public class JwtSettings
    {
        #region Properties

        public string Audience { get; set; } = string.Empty;

        public int ExpirationInMinutes { get; set; }

        public string Issuer { get; set; } = string.Empty;

        public string Secret { get; set; } = string.Empty;

        #endregion
    }
}
