using System;
using System.Threading;
using System.Threading.Tasks;
using HttpJsonRpc;

namespace bsdmn.Api
{
    class Program
    {
        private static ManualResetEvent ResetEvent { get; } = new ManualResetEvent(false);


        static void Main(string[] args)
        {
            Masternode.Poll();

            JsonRpc.OnReceivedRequest(c =>
            {
                Console.WriteLine($"method: {c.Request?.Method} params: {c.Request?.Params}");

                return Task.CompletedTask;
            });

            JsonRpc.Start("http://*:5000");

            Console.CancelKeyPress += (sender, eventArgs) => ResetEvent.Set();
            ResetEvent.WaitOne();
        }
    }
}
