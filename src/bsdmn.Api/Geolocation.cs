using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bsdmn.Api
{
    public class Geolocation
    {
        public string IP { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }

        public static async Task<Geolocation> GetAsync(string ip)
        {
            var client = new HttpClient();
            var jsonResult = await client.GetStringAsync($"https://freegeoip.net/json/{ip}");
            var jResult = JsonConvert.DeserializeObject<JObject>(jsonResult);

            var result = new Geolocation();
            result.IP = jResult.Value<string>("ip");
            result.CountryCode = jResult.Value<string>("country_code");
            result.CountryName = jResult.Value<string>("country_name");

            return result;
        }
    }
}