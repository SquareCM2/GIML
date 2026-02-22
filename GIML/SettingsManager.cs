using System;
using System.IO;
using System.Text.Json;

public class AppSettings
{
    public string GameFolderPath { get; set; }
    public string JavaPath { get; set; }
    public string GitHubToken { get; set; }
}

public static class SettingsManager
{
    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GIML", "settings.json");

    public static AppSettings Load()
    {
        System.Diagnostics.Debug.WriteLine($"Load called from {Environment.StackTrace}");
        if (!File.Exists(SettingsFilePath))
            return new AppSettings();
        try
        {
            string json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings(); // 出错时返回默认设置
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            string dir = Path.GetDirectoryName(SettingsFilePath);
            Directory.CreateDirectory(dir);
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            // 可选：记录错误
            System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex}");
        }
    }
}