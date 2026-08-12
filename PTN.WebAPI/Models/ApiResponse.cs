namespace PTN.WebAPI.Models
{
    public class ApiResponse<T>
    {
        public string Message { get; set; } = "İşlem başarıyla tamamlandı";
        public bool Success { get; set; } = true;
        public int Status { get; set; } = 200;
        public T? Data { get; set; }

        public static ApiResponse<T> SuccessResult(T data, string message = "İşlem başarıyla tamamlandı", int status = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Status = status,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailResult(string message, int status = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Status = status,
                Message = message,
                Data = default
            };
        }
    }
}