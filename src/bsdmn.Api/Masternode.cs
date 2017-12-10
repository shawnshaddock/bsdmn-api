using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HttpJsonRpc;
using Newtonsoft.Json;

namespace bsdmn.Api
{
    public class Masternode
    {
        private static ConcurrentDictionary<string, Masternode>  All { get; } = new ConcurrentDictionary<string, Masternode>();

        public string Id { get; set; }
        public string Address { get; set; }
        public int Index { get; set; }
        public string Status { get; set; }
        public int Protocol { get; set; }
        public string PubKey { get; set; }
        public string Vin { get; set; }
        public DateTime LastSeen { get; set; }
        public TimeSpan ActiveDuration { get; set; }
        public int Rank { get; set; }

        public static async void Poll()
        {
            while (true)
            {
                var addressIndexs = new Dictionary<string, int>();

                //full - Print info in format 'status protocol pubkey vin lastseen activeseconds' (can be additionally filtered, partial match)
                var result = await BitSendCli.RunAsync("masternodelist full");
                var reader = new JsonTextReader(new StringReader(result));

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var masternode = new Masternode();
                        masternode.Address = ((string) reader.Value).Trim();
                        if (addressIndexs.TryGetValue(masternode.Address, out var index))
                        {
                            index++;
                        }

                        masternode.Index = index;
                        addressIndexs[masternode.Address] = index;

                        masternode.Id = GetMasternodeId(masternode.Address, masternode.Index);

                        var info = reader.ReadAsString();
                        var values = info.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        masternode.Status = values[0];
                        masternode.Protocol = int.Parse(values[1]);
                        masternode.PubKey = values[2];
                        masternode.Vin = values[3];

                        var lastSeenSeconds = int.Parse(values[4]);
                        masternode.LastSeen = Unix.GetTimestampFromSeconds(lastSeenSeconds);

                        var activeseconds = int.Parse(values[5]);
                        masternode.ActiveDuration = TimeSpan.FromSeconds(activeseconds);

                        All[masternode.Id] = masternode;
                    }
                }

                //ranks
                result = await BitSendCli.RunAsync("masternodelist rank");
                reader = new JsonTextReader(new StringReader(result));
                addressIndexs = new Dictionary<string, int>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var address = (string)reader.Value;
                        if (addressIndexs.TryGetValue(address, out var index))
                        {
                            index++;
                        }

                        addressIndexs[address] = index;

                        var id = GetMasternodeId(address, index);
                        var rank = reader.ReadAsInt32() ?? 0;

                        if (All.TryGetValue(id, out var masternode)) masternode.Rank = rank;
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(2));
            }
        }

        private static string GetMasternodeId(string address, int index)
        {
            return $"{address}-{index}";
        }

        [JsonRpcMethod]
        public static Task<List<Masternode>> ListAsync(string status = null)
        {
            var masternodes = All.Values.OrderBy(mn => mn.Rank).ToList();

            if (status != null) masternodes = masternodes.Where(mn => mn.Status == status).ToList();

            return Task.FromResult(masternodes);
        }

        [JsonRpcMethod]
        public static Task<Masternode> GetAsync(string pubKey)
        {
            var masternode = All.Values.FirstOrDefault(mn => mn.PubKey == pubKey);

            return Task.FromResult(masternode);
        }

        [JsonRpcMethod]
        public static async Task<int> GetCountAsync(string status = null)
        {
            var masternodes = await ListAsync(status);
            return masternodes.Count;
        }
    }
}