namespace PTN.WebAPI.Entities
{
    //todo: Apiye istek atılırken gerekli url ve apiKey bilgileri bu tablodan alınmalı.
    public class ApiSettings
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        
    }
}
