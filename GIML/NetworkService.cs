using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json.Serialization;
using Microsoft.UI.Xaml.Documents;
using Windows.Media.Devices;

namespace GIML
{
    public class ServerInfo
    { 
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("mapName")]
        public string MapName { get; set; }

        [JsonPropertyName("players")]
        public int Players { get; set; }

        [JsonPropertyName("wave")]
        public int Wave { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        public string Limit_Str { get; set; }

        [JsonPropertyName("description")]
        public string Desc { get; set; }

        [JsonPropertyName("timeMs")]
        public int NetworkDelay { get; set; }

        [JsonPropertyName("lastOnline")]
        public long LastOnline { get; set; }

        [JsonPropertyName("online")]
        public bool isOnline { get; set; }
    }
    internal class NetworkService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        public async Task<List<ServerInfo>> GetServersAsync()
        {
            var url = "https://api.mindustry.top/servers/list";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("User-Agent", "GIML");

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var servers = JsonSerializer.Deserialize<List<ServerInfo>>(json);
                foreach(var server in servers)
                {
                    server.Limit_Str = server.Limit == 0 ? "无限制" : server.Limit.ToString();
                }
                return servers;
            }
            return new List<ServerInfo>();
        }
    }
}
