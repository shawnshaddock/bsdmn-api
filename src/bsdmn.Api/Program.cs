using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HttpJsonRpc;
using Newtonsoft.Json;

namespace bsdmn.Api
{
    class Program
    {
        static void Main(string[] args)
        {
            string address = null;

#if !DEBUG
            address = "http://*:80/";
#endif

            Masternode.Poll();
            JsonRpc.Start(address);
            Console.ReadLine();
        }
    }
}
