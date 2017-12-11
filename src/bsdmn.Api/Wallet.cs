using System.Threading.Tasks;
using HttpJsonRpc;

namespace bsdmn.Api
{
    public static class Wallet
    {
        [JsonRpcMethod(Description = "Donate to this address if you want to support the hosting and development of this API.")]
        public static async Task<string> GetDonateAddressAsync()
        {
            var address = await BitSendCli.RunAsync("getaccountaddress donate");

            return address.Trim();
        }
    }
}