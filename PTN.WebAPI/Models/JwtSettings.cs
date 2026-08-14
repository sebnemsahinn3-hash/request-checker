namespace PTN.WebAPI.Models
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = "PTNHealthAPI";
        public string Audience { get; set; } = "PTNHealthUI";
        public int ExpirationInMinutes { get; set; } = 120;
    }
}