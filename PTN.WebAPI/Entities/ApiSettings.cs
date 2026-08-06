namespace PTN.WebAPI.Entities
{
    //todo: Apiye istek atılırken gerekli url ve apiKey bilgileri bu tablodan alınmalı.
    public class ApiSettings
    {
        public int Id { get; set; }
        public string Name { get;set; }
        public string Url { get; set; }
        public string ApiKey { get; set; }
        
    }
}
