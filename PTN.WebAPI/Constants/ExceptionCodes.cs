namespace PTN.WebAPI.Constants
{
    public static class ExceptionCodes
    {
        // Validasyon Hata Anahtarları
        public const string CountInvalid = "Validation:CountInvalid";
        public const string CountTooLarge = "Validation:CountTooLarge";
        public const string CountRequired = "Validation:CountRequired";
        public const string CountNullWhenIsAllDelete = "Validation:CountNullWhenIsAllDelete";
        public const string IsAllDeleteRequired = "Validation:IsAllDeleteRequired";
        public const string ErrorMessage = "RequestMessages:ErrorMessage";
        public const string SuccessMessage = "RequestMessages:SuccessMessage";
    }
}