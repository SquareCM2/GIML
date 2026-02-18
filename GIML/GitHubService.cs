using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

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

    public async Task<List<AvailableInstance>> GetReleasesAsync(string owner, string repo, string type)
    {
        var url = $"https://api.github.com/repos/{owner}/{repo}/releases";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // GitHub API强制要求设置User-Agent头，这是必须的
        request.Headers.Add("User-Agent", "GIML");
        // 可选：指定API版本
        request.Headers.Add("Accept", "application/vnd.github.v3+json");

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            // 需要反序列化 GitHub API 的响应
            // 可以用 System.Text.Json 或 Newtonsoft.Json
            return ParseReleases(JsonSerializer.Deserialize<List<GitHubRelease>>(json), type);
        }
        return new List<AvailableInstance>();
    }

    private List<AvailableInstance> ParseReleases(List<GitHubRelease> releases, String type)
    {
        List<AvailableInstance> list = new List<AvailableInstance>();
        foreach (var release in releases)
        {
            AvailableInstance instance = new AvailableInstance();
            instance.Type = type;
            instance.TagName = release.TagName;
            instance.Name = release.Name;
            instance.Body = release.Body;
            var mindustryJar = release.Assets?.Find(a => a.Name == "Mindustry.jar");
            if (mindustryJar != null)
            {
                instance.DownloadUrl = mindustryJar.BrowserDownloadUrl;
            }
            list.Add(instance);
        }
        return list;
    }
}