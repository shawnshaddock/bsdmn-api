using System.Diagnostics;
using System.Threading.Tasks;

namespace bsdmn.Api
{
    public static class Netcat
    {
        public static async Task<bool> CanConnectAsync(string ip, int port)
        {
            var p = new Process();
            p.StartInfo.FileName = "nc";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Arguments = $"-zv -w 1 {ip} {port}";
            p.Start();

            var result = await p.StandardOutput.ReadToEndAsync();

            return result.StartsWith("Connection");
        }
    }
}