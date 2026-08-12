namespace PTN.WebAPI.Constants
{
    public static class UserConstants
    {
        private const string Prefix = "Users";
        public const string UserNotFound = $"{Prefix}:UserNotFound";
        public const string EmailAlreadyExists = $"{Prefix}:EmailAlreadyExists";
        public const string UserCreatedSuccess = $"{Prefix}:UserCreatedSuccess";
    }
}