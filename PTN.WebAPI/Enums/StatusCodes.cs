using System.ComponentModel;

namespace PTN.WebAPI.Enums
{
    public enum StatusCodes
    {
        [Description("StatusCodes:01")]
        Success = 1,

        [Description("StatusCodes:02")]
        BadRequest = 2,

        [Description("StatusCodes:03")]
        Unauthorized = 3,

        [Description("StatusCodes:04")]
        NotFound = 4,

        [Description("StatusCodes:05")]
        InternalServerError = 5
    }
}