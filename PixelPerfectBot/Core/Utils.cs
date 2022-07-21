using System.Net;

namespace PixelPerfectBot.Core
{
    public class Utils
    {
        public async Task<string> GetRequest(string url)
        {
            HttpResponseMessage request = await new HttpClient().GetAsync(url);
            using Stream webStream = request.Content.ReadAsStream();

            using StreamReader reader = new StreamReader(webStream);
            string data = reader.ReadToEnd();
            return data;
        }
    }
}
