namespace WolfisBlazorPage.Structure
{
    public class APIService
    {
        private readonly HttpClient _http;


        public APIService(HttpClient http)
        {
            _http = http;
        }

    }
}
