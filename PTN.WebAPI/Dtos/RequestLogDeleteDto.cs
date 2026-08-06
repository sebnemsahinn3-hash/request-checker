namespace PTN.WebAPI.Dtos
{
    public class RequestLogDeleteDto
    {
        // True gelirse tüm kayıtları siler
        public bool IsAllDelete { get; set; }

        // Sayı girilirse (örn: 5) ilk oluşan o kadar kaydı siler
        public int? Count { get; set; }
    }
}