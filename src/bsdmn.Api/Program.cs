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
            Masternode.Poll();
            JsonRpc.Start();
            Console.ReadLine();
        }
    }
}
