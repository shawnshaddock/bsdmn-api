﻿using System;
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

            JsonRpc.OnReceivedHttpRequest(c =>
            {
                c.Response.AddHeader("Access-Control-Allow-Origin", "*");

                if (c.Request.HttpMethod == "OPTIONS")
                {
                    c.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                    c.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                    c.Response.Close();
                }

                return Task.CompletedTask;
            });

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

            JsonRpc.Stop();
        }
    }
}
