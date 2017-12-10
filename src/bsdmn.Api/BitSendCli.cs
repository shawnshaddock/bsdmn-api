using System.Diagnostics;
using System.Threading.Tasks;

namespace bsdmn.Api
{
    public static class BitSendCli
    {
        public static async Task<string> RunAsync(string command)
        {
            var p = new Process();
            p.StartInfo.FileName = "bitsend-cli";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Arguments = command;
            p.Start();
            var result = await p.StandardOutput.ReadToEndAsync();

            return result;
        }
    }
}