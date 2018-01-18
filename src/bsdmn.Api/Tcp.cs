using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace bsdmn.Api
{
    public static class Tcp
    {
        public static async Task<ConnectionTest> TestConnectionAsync(string ip, int port, TimeSpan timeout)
        {
            var test = new ConnectionTest();
            test.TestedAt = DateTime.UtcNow;

            using (var client = new TcpClient())
            {
                try
                {
                    var connectTask = client.ConnectAsync(ip, port);
                    await Task.WhenAny(connectTask, Task.Delay(timeout));
                    test.Connected = connectTask.IsCompleted;
                }
                catch (Exception e)
                {
                    test.ErrorMessage = e.Message;
                }
            }

            return test;
        }
    }
}