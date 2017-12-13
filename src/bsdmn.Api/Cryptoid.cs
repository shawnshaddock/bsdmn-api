using System.Net.Http;
using System.Threading.Tasks;

namespace bsdmn.Api
{
    public static class Cryptoid
    {
        public static string ApiKey => "daf0be5adabe";
        public static string Url => "https://chainz.cryptoid.info/bsd/api.dws";

        public static async Task<string> GetAsync(string request)
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync($"{Url}?key={ApiKey}&{request}");

            return result;
        }

        public static async Task<decimal> GetBalance(string address)
        {
            var balanceString = await GetAsync($"q=getbalance&a={address}");
            return decimal.Parse(balanceString);
        }
    }
}