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
            Masternode.StartMonitoring();

            JsonRpc.OnReceivedRequest(c =>
            {
                Console.WriteLine($"method: {c.Request?.Method} params: {c.Request?.Params}");

                return Task.CompletedTask;
            });

            var address = "http://*:5000";
#if DEBUG
            address = "http://localhost:5000/";
#endif

            JsonRpc.Start(address);

            Console.CancelKeyPress += (sender, eventArgs) => ResetEvent.Set();
            ResetEvent.WaitOne();
        }
    }
}
