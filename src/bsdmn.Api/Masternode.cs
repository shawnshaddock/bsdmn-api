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

        public string Id => $"{Vin}-{Index}";
        public string Address { get; set; }
        public string Status { get; set; }
        public int Protocol { get; set; }
        public string PubKey { get; set; }
        public string Vin { get; set; }
        public int Index { get; set; }
        public DateTime LastSeen { get; set; }
        public TimeSpan ActiveDuration { get; set; }
        public int Rank { get; set; }

        public static async void Poll()
        {
            while (true)
            {
                try
                {
                    //full - Print info in format 'status protocol pubkey address lastseen activeseconds' (can be additionally filtered, partial match)
                    var fullResult = await BitSendCli.RunAsync("masternodelist full");
                    var fullReader = new JsonTextReader(new StringReader(fullResult));

                    //ranks
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
                            masternode.Address = values[3];

                            var lastSeenSeconds = int.Parse(values[4]);
                            masternode.LastSeen = Unix.GetTimestampFromSeconds(lastSeenSeconds);

                            var activeseconds = int.Parse(values[5]);
                            masternode.ActiveDuration = TimeSpan.FromSeconds(activeseconds);

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

                            All[masternode.Id] = masternode;
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

        [JsonRpcMethod(Description = "Lists all masternodes. Supports filtering by status and protocol.")]
        public static Task<List<Masternode>> ListAsync(string status = null, int? protocol = null)
        {
            var masternodes = All.Values.AsEnumerable();

            if (status != null) masternodes = masternodes.Where(mn => mn.Status == status).ToList();
            if (protocol != null) masternodes = masternodes.Where(mn => mn.Protocol == protocol.Value);

            return Task.FromResult(masternodes.ToList());
        }

        [JsonRpcMethod(Description = "Gets a single masternode by address, vin, pubkey, or id. If multiple masternodes match returns the first one.")]
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

        [JsonRpcMethod(Description = "Returns the total count of masternodes. Supports filtering by status and protocol.")]
        public static async Task<int> GetCountAsync(string status = null, int? protocol = null)
        {
            var masternodes = await ListAsync(status, protocol);
            return masternodes.Count;
        }
    }
}