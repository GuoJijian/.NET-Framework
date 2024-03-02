namespace Webapi.Core
{
    public class HttpsConfig : IConfig
    {
        public bool UseHttps { get; set; } = false;
        public string Certificate2FileName { get; set; }
        public string Password { get; set; }
    }
}
