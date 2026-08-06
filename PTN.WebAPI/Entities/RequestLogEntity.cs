namespace PTN.WebAPI.Entities
{
    public class RequestLogEntity
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string RequestParams { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public string Timing { get; set; }

        // Sonuna ? koyarak int? (boş bırakılabilir tamsayı) yapıyoruz:
        public int? StatusCode { get; set; }

        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}