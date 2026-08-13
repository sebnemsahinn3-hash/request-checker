namespace PTN.WebAPI.Dtos
{
    public class DatabaseHealthDto
    {
        public bool IsConnected { get; set; }
        public int TotalLogCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}