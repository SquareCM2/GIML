using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
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

    public class MapInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("preview")]
        public string PreviewImg { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("latest")]
        public string latest { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        public string Version { get; set; }
    }

    public class DetailedMapInfo
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tags")]
        public DetailedMapInfoTags Tags { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("user")]
        public WayzerUser Uploader { get; set; }

        [JsonPropertyName("preview")]
        public string PreviewImg { get; set; }
    }

    public class WayzerUser
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("gid")]
        public string Gid { get; set; }
    }

    public class LoadoutItem
    {
        [JsonPropertyName("item")]
        public string Item { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }

    public class MapRules
    {
        [JsonPropertyName("waves")]
        public bool isWavesOn { get; set; } = false;

        [JsonPropertyName("attackMode")]
        public bool isAttackOn { get; set; } = false;

        [JsonPropertyName("damageExplosions")]
        public bool DamageExplosions { get; set; } = false;

        [JsonPropertyName("fire")]
        public bool Fire { get; set; } = false;

        [JsonPropertyName("unitAmmo")]
        public bool UnitAmmo { get; set; } = false;

        [JsonPropertyName("unitDamageMultiplier")]
        public double UnitDamageMultiplier { get; set; } = 1;

        [JsonPropertyName("buildCostMultiplier")]
        public double BuildCostMultiplier { get; set; } = 1;

        [JsonPropertyName("buildSpeedMultiplier")]
        public double BuildSpeedMultiplier { get; set; } = 1;

        [JsonPropertyName("unitBuildSpeedMultiplier")]
        public double UnitBuildSpeedMultiplier { get; set; } = 1;

        [JsonPropertyName("deconstructRefundMultiplier")]
        public double DeconstructRefundMultiplier { get; set; } = 0;

        [JsonPropertyName("blockHealthMultiplier")]
        public double BlockHealthMultiplier { get; set; } = 1;

        [JsonPropertyName("blockDamageMultiplier")]
        public double BlockDamageMultiplier { get; set; } = 1;

        [JsonPropertyName("enemyCoreBuildRadius")]
        public int EnemyCoreBuildRadius { get; set; } = 400;

        [JsonPropertyName("waveSpacing")]
        public int WaveSpacing { get; set; } = 7200;

        [JsonPropertyName("unitCap")]
        public int UnitCap { get; set; } = 0;

        [JsonPropertyName("unitCapVariable")]
        public bool UnitCapVariable { get; set; } = true;

        [JsonPropertyName("loadout")]
        public List<LoadoutItem> Loadout { get; set; }
    }

    public class DetailedMapInfoTags
    {
        [JsonPropertyName("mods")]
        public List<string> Mods { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("build")]
        public int buildVer { get; set; }

        [JsonPropertyName("description")]
        public string Desc { get; set; }

        [JsonPropertyName("rules")]
        [JsonConverter(typeof(MapRulesConverter))]
        public MapRules Rules { get; set; }
    }

    internal class NetworkService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        public async Task<List<ServerInfo>> GetServersAsync()
        {
            var url = "https://api.mindustry.top/servers/list";

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

        public async Task<List<MapInfo>> GetMapsAsync(int begin, string mode, string version, string sorting, string search)
        {
            var uriBuilder = new UriBuilder("https://api.mindustry.top/maps/list");

            var query = new List<string>();
            query.Add($"begin={begin}");

            // 构建 search 参数的各个部分
            var searchParts = new List<string>();
            if (!string.IsNullOrEmpty(search))
                searchParts.Add(search); // 用户输入的关键词
            if (!string.IsNullOrEmpty(mode))
                searchParts.Add($"@mode:{mode}");
            if (!string.IsNullOrEmpty(version))
                searchParts.Add($"@version:{version}");
            if (!string.IsNullOrEmpty(sorting))
                searchParts.Add($"@sort:{sorting}");

            if (searchParts.Count > 0)
            {
                // 用加号连接所有部分
                string searchQuery = string.Join("+", searchParts);
                query.Add($"search={searchQuery}");
            }

            uriBuilder.Query = string.Join("&", query);

            System.Diagnostics.Debug.WriteLine($"请求url：{uriBuilder.Uri}");

            using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var maps = JsonSerializer.Deserialize<List<MapInfo>>(json);
                if(maps == null)
                    return new List<MapInfo>();

                foreach (var map in maps)
                {
                    if (map.Tags != null)
                    {
                        foreach (var tag in map.Tags)
                        {
                            // 假设版本号以 v 开头（不区分大小写）
                            if (tag.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                            {
                                map.Version = tag;
                                break;
                            }
                        }
                    }
                    // 如果没找到，Version 保持默认（可后续赋默认值）
                }
                return maps;
            }
            return new List<MapInfo>();
        }

        public async Task<DetailedMapInfo> GetDetailedInfo(string hash)
        {
            var url = $"https://api.mindustry.top/maps/{hash}.json";

            System.Diagnostics.Debug.WriteLine($"请求url：{url}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"JSON 响应开头: {json.Substring(0, Math.Min(100, json.Length))}");
                try
                {
                    var info = JsonSerializer.Deserialize<DetailedMapInfo>(json);
                    if (info.Tags?.Rules == null)
                    {
                        info.Tags.Rules = new MapRules();
                        info.Tags.Rules.EnemyCoreBuildRadius = 50;
                        info.Tags.Rules.WaveSpacing = 120;
                        info.Tags.Rules.DeconstructRefundMultiplier = 0.5;
                    }
                    else
                    {
                        info.Tags.Rules.EnemyCoreBuildRadius /= 8;
                        info.Tags.Rules.WaveSpacing /= 60;
                        info.Tags.Rules.DeconstructRefundMultiplier = (info.Tags.Rules.DeconstructRefundMultiplier == 0) ? 0.5 : info.Tags.Rules.DeconstructRefundMultiplier;
                    }
                    return info ?? new DetailedMapInfo();
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"反序列化失败: {ex}");
                    // 可选：将完整 JSON 写入文件以供检查
                    return new DetailedMapInfo();
                }
            }
            else
            {
                // 打印错误状态码和内容
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"HTTP {(int)response.StatusCode}: {errorContent}");
                return new DetailedMapInfo();
            }
        }
    }
}
