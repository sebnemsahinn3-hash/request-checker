namespace PTN.WebAPI.Constants
{
    public static class AuthConstants
    {
        private const string Prefix = "Auth";
        public const string Unauthorized = $"{Prefix}:Unauthorized";
        public const string Forbidden = $"{Prefix}:Forbidden";
        public const string InvalidCredentials = $"{Prefix}:InvalidCredentials";
    }
}
