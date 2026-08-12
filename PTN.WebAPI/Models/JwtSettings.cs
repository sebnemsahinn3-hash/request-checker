namespace PTN.WebAPI.Models
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = "PTN_Super_Secret_Key_For_JWT_Authentication_2026!";
        public string Issuer { get; set; } = "PTNHealthAPI";
        public string Audience { get; set; } = "PTNHealthUI";
        public int ExpirationInMinutes { get; set; } = 120;
    }
}