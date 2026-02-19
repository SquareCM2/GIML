using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.WindowsAppSDK;
using Windows.ApplicationModel.Background;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinRT.Interop;

namespace GIML
{
    public class AvailableInstance
    {
        public string Name { get; set; }                // 版本名称（例如 "v7.0 Build 145"）
        public string TagName { get; set; }             // Git 标签（例如 "v7.0.145"）
        public DateTime PublishedAt { get; set; }       // 发布时间
        public string Body { get; set; }                 // 更新说明
        public string DownloadUrl { get; set; }          // 下载链接（通常是 assets 中的第一个）
        public string Type { get; set; }                //客户端类型（原版/BE/学术/X）
    }

    public class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }          // release 的显示名称

        [JsonPropertyName("body")]
        public string Body { get; set; }           // 更新内容

        [JsonPropertyName("assets")]
        public List<Asset> Assets { get; set; }

        [JsonPropertyName("published_at")]
        public string PublishedAt { get; set; }
    }

    public class Asset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }

    public class GitHubService
    {
        private readonly HttpClient _httpClient;

        public GitHubService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GIML/1.0");
        }

        public async Task<List<AvailableInstance>> GetReleasesAsync(string type)
        {
            string owner = null, repo = null;

            switch (type)
            {
                case "vanilla":
                    owner = "Anuken";
                    repo = "Mindustry";
                    break;
                case "be":
                    owner = "Anuken";
                    repo = "MindustryBuilds";
                    break;
                case "arc":
                    owner = "Jackson11500";
                    repo = "Mindustry-CN-ARC-Builds";
                    break;
                case "x":
                    owner = "TinyLake";
                    repo = "MindustryX";
                    break;
            }

            var url = $"https://api.github.com/repos/{owner}/{repo}/releases";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            // GitHub API强制要求设置User-Agent头，这是必须的
            request.Headers.Add("User-Agent", "GIML");
            // 可选：指定API版本
            request.Headers.Add("Accept", "application/vnd.github.v3+json");

            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.TryGetValue("GitHubToken", out object githubToken))
            {
                request.Headers.Add("Authorization", "token " + githubToken.ToString());
            }

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return ParseReleases(JsonSerializer.Deserialize<List<GitHubRelease>>(json), type);
            }
            return new List<AvailableInstance>();
        }

        public DateTime StringToDateTime(string s)
        {
            
            int year = int.Parse(s.Substring(0, 4));
            int month = int.Parse(s.Substring(5, 2));
            int day = int.Parse(s.Substring(8, 2));
            int hour = int.Parse(s.Substring(11, 2));
            int minute = int.Parse(s.Substring(14, 2));
            int second = int.Parse(s.Substring(17, 2));

            DateTime date = new DateTime(year, month, day, hour, minute, second);
            return date;
        }

        private List<AvailableInstance> ParseReleases(List<GitHubRelease> releases, String type)
        {
            List<AvailableInstance> list = new List<AvailableInstance>();
            foreach (var release in releases)
            {
                AvailableInstance instance = new AvailableInstance();
                instance.Type = type;
                instance.TagName = release.TagName;
                instance.Name = release.Name == "" ? "Build " + release.TagName : release.Name;
                instance.Body = release.Body;
                instance.PublishedAt = release.PublishedAt != null ? StringToDateTime(release.PublishedAt) : DateTime.MinValue;
                Asset mindustryJar = new Asset();
                switch (type)
                {
                    case "vanilla":
                        mindustryJar = release.Assets?.Find(a => a.Name == "Mindustry.jar");
                        break;
                    case "be":
                        mindustryJar = release.Assets?.Find(a => a.Name.Contains("Mindustry-BE-Desktop-"));
                        break;
                    case "arc":
                        mindustryJar = release.Assets?.Find(a => a.Name.Contains("Mindustry-CN-ARC-Desktop-"));
                        break;
                    case "x":
                        mindustryJar = release.Assets?.Find(a => a.Name.Contains("-Desktop.jar"));
                        break;
                }

                if (mindustryJar != null)
                {
                    instance.DownloadUrl = mindustryJar.BrowserDownloadUrl;
                }
                list.Add(instance);
            }
            return list;
        }
    }
}