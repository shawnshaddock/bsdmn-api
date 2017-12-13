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

        public string NodeId => $"{Vin}-{Index}";
        public string Address { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string Status { get; set; }
        public int Protocol { get; set; }
        public string PubKey { get; set; }
        public string Vin { get; set; }
        public int Index { get; set; }
        public DateTime LastSeen { get; set; }
        public int ActiveSeconds { get; set; }
        public string ActiveDuration { get; set; }
        public int Rank { get; set; }
        public decimal? Balance { get; set; }
        public string ConnectionTest { get; set; }

        public static async void Poll()
        {
            while (true)
            {
                try
                {
                    //full
                    var fullResult = await BitSendCli.RunAsync("masternodelist full");
                    var fullReader = new JsonTextReader(new StringReader(fullResult));

                    //rank
                    var rankResult = await BitSendCli.RunAsync("masternodelist rank");
                    var rankReader = new JsonTextReader(new StringReader(rankResult));
                    var ranks = new List<(string address, int rank)>();
                    var vinIndexes = new Dictionary<string, int>();
                    var addressIndexes = new Dictionary<string, int>();

                    while (rankReader.Read())
                    {
                        if (rankReader.TokenType == JsonToken.PropertyName)
                        {
                            var address = (string) rankReader.Value;
                            var rank = rankReader.ReadAsInt32() ?? 0;
                            ranks.Add((address, rank));
                        }
                    }

                    while (fullReader.Read())
                    {
                        if (fullReader.TokenType == JsonToken.PropertyName)
                        {
                            var masternode = new Masternode();
                            masternode.Vin = ((string) fullReader.Value).Trim();

                            var info = fullReader.ReadAsString();
                            var values = info.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            masternode.Status = values[0];
                            masternode.Protocol = int.Parse(values[1]);
                            masternode.PubKey = values[2];

                            var address = values[3];
                            masternode.Address = address;
                            var portIndex = address.LastIndexOf(':');

                            if (portIndex > -1)
                            {
                                masternode.IP = address.Substring(0, portIndex);
                                masternode.Port = int.Parse(address.Substring(portIndex + 1));
                            }

                            var lastSeenSeconds = int.Parse(values[4]);
                            masternode.LastSeen = Unix.GetTimestampFromSeconds(lastSeenSeconds);

                            masternode.ActiveSeconds = int.Parse(values[5]);
                            var activeDuration = TimeSpan.FromSeconds(masternode.ActiveSeconds);
                            masternode.ActiveDuration = $"{activeDuration:%d} days {activeDuration:hh} hours {activeDuration:mm} minutes {activeDuration:ss} seconds";

                            if (vinIndexes.TryGetValue(masternode.Vin, out var vinIndex))
                            {
                                vinIndex++;
                            }

                            vinIndexes[masternode.Vin] = vinIndex;
                            masternode.Index = vinIndex;

                            if (addressIndexes.TryGetValue(masternode.Address, out var addressIndex))
                            {
                                addressIndex++;
                            }

                            addressIndexes[masternode.Address] = addressIndex;
                            var addressRanks = ranks.Where(r => r.address == masternode.Address).ToList();
                            if (addressRanks.Count > addressIndex) masternode.Rank = addressRanks[addressIndex].rank;

                            All[masternode.NodeId] = masternode;
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

        //public static async void PollConnections()
        //{
        //    while (true)
        //    {
        //        foreach (var masternode in All.Values)
        //        {
        //            try
        //            {
        //                masternode.ConnectionTest = await Netcat.TestConnectionAsync(masternode.IP, masternode.Port);
        //            }
        //            catch (Exception e)
        //            {
        //                Console.WriteLine(e);
        //            }
        //        }
        //    }
        //}

        [JsonRpcMethod(Description = "Lists all masternodes. Supports filtering by status and protocol.")]
        public static Task<List<Masternode>> ListAsync(string status = null, int? protocol = null, string address = null, string vin = null, string pubkey = null, string nodeId = null,
            [JsonRpcParameter(Description = "Searches address, vin, pubkey, and nodeId")] string searchText = null)
        {
            var masternodes = All.Values
                .Where(mn => status == null || mn.Status == status)
                .Where(mn => protocol == null || mn.Protocol == protocol.Value)
                .Where(mn => address == null || mn.Address.StartsWith(address))
                .Where(mn => vin == null || mn.Vin.StartsWith(vin))
                .Where(mn => pubkey == null || mn.PubKey.StartsWith(pubkey))
                .Where(mn => nodeId == null || mn.NodeId.StartsWith(nodeId))
                .Where(mn => searchText == null || mn.Address.StartsWith(searchText) || mn.Vin.StartsWith(searchText) || mn.PubKey.StartsWith(searchText) || mn.NodeId.StartsWith(searchText))
                .ToList();

            return Task.FromResult(masternodes);
        }

        [JsonRpcMethod(Description = "Gets a single masternode by address, vin, pubkey, or id. If multiple masternodes match returns the first one.")]
        public static async Task<Masternode> GetAsync(string address = null, string vin = null, string pubkey = null, string nodeId = null)
        {
            // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
            var masternode = All.Values
                .Where(mn => address == null || mn.Address.StartsWith(address))
                .Where(mn => vin == null || mn.Vin.StartsWith(vin))
                .Where(mn => pubkey == null || mn.PubKey.StartsWith(pubkey))
                .Where(mn => nodeId == null || mn.NodeId.StartsWith(nodeId))
                .FirstOrDefault();

            if (masternode != null)
            {
                masternode.Balance = await Cryptoid.GetBalance(masternode.PubKey);
            }

            return masternode;
        }

        [JsonRpcMethod(Description = "Returns the total count of masternodes. Supports filtering by status and protocol.")]
        public static async Task<int> GetCountAsync(string status = null, int? protocol = null)
        {
            var masternodes = await ListAsync(status, protocol);
            return masternodes.Count;
        }
    }
}