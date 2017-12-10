using System.Threading.Tasks;
using HttpJsonRpc;

namespace bsdmn.Api
{
    public static class Wallet
    {
        [JsonRpcMethod]
        public static async Task<string> GetDonateAddressAsync()
        {
            var address = await BitSendCli.RunAsync("getaccountaddress donate");

            return address.Trim();
        }
    }
}