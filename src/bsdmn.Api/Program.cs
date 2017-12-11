using System;
using System.Threading.Tasks;
using HttpJsonRpc;

namespace bsdmn.Api
{
    class Program
    {
        static void Main(string[] args)
        {
            Masternode.Poll();

            JsonRpc.OnReceivedRequest(c =>
            {
                Console.WriteLine($"method: {c.Request?.Method} params: {c.Request?.Params}");

                return Task.CompletedTask;
            });

            JsonRpc.Start("http://*:5000");
            Console.ReadLine();
        }
    }
}
