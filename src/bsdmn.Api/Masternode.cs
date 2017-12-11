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
                try
                {
                    var addressIndexs = new Dictionary<string, int>();

                    //full - Print info in format 'status protocol pubkey address lastseen activeseconds' (can be additionally filtered, partial match)
                    var result = await BitSendCli.RunAsync("masternodelist full");
                    var reader = new JsonTextReader(new StringReader(result));

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            var masternode = new Masternode();
                            masternode.Vin = ((string)reader.Value).Trim();

                            var info = reader.ReadAsString();
                            var values = info.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            masternode.Status = values[0];
                            masternode.Protocol = int.Parse(values[1]);
                            masternode.PubKey = values[2];
                            masternode.Address = values[3];

                            var lastSeenSeconds = int.Parse(values[4]);
                            masternode.LastSeen = Unix.GetTimestampFromSeconds(lastSeenSeconds);

                            var activeseconds = int.Parse(values[5]);
                            masternode.ActiveDuration = TimeSpan.FromSeconds(activeseconds);

                            if (addressIndexs.TryGetValue(masternode.Address, out var index))
                            {
                                index++;
                            }

                            addressIndexs[masternode.Address] = index;

                            masternode.Id = GetMasternodeId(masternode.Address, index);

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
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                await Task.Delay(TimeSpan.FromMinutes(2));
            }
        }

        private static string GetMasternodeId(string address, int index)
        {
            return $"{address}-{index}";
        }

        [JsonRpcMethod]
        public static Task<List<Masternode>> ListAsync(string status = null, int? protocol = null)
        {
            var masternodes = All.Values.AsEnumerable();

            if (status != null) masternodes = masternodes.Where(mn => mn.Status == status).ToList();
            if (protocol != null) masternodes = masternodes.Where(mn => mn.Protocol == protocol.Value);

            return Task.FromResult(masternodes.ToList());
        }

        [JsonRpcMethod]
        public static Task<Masternode> GetAsync(string address = null, string vin = null, string pubkey = null, string id = null)
        {
            // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
            var masternode = All.Values
                .Where(mn => address == null || mn.Address.StartsWith(address))
                .Where(mn => vin == null || mn.Vin.StartsWith(vin))
                .Where(mn => pubkey == null || mn.PubKey.StartsWith(pubkey))
                .Where(mn => id == null || mn.Id.StartsWith(id))
                .FirstOrDefault();

            return Task.FromResult(masternode);
        }

        [JsonRpcMethod]
        public static async Task<int> GetCountAsync(string status = null, int? protocol = null)
        {
            var masternodes = await ListAsync(status, protocol);
            return masternodes.Count;
        }
    }
}