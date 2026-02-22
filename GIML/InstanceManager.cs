using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Capture.Core;
using Windows.Storage;
using WinRT.Interop;

namespace GIML
{
    public static class InstanceManager
    {
        private static readonly string FileName = "instancesData.json";
        private static string FilePath => Path.Combine((!string.IsNullOrEmpty(SettingsManager.Load().GameFolderPath) ? SettingsManager.Load().GameFolderPath : ""), FileName);

        // 加载所有实例（同步，适合启动时调用）
        public static List<GameInstance> Load()
        {
            if (!File.Exists(FilePath))
                return new List<GameInstance>();

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<GameInstance>>(json) ?? new List<GameInstance>();
        }

        // 保存所有实例（异步，适合修改后调用）
        public static async Task SaveAsync(List<GameInstance> instances)
        {
            string json = JsonSerializer.Serialize(instances, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);
        }

        // 添加或更新单个实例（简化版，可根据需要扩展）
        public static async Task AddOrUpdateAsync(GameInstance instance)
        {
            var instances = Load();
            var existing = instances.Find(i => i.Id == instance.Id);
            if (existing != null)
            {
                // 更新现有实例的属性（例如 Name、Version 等）
                existing.Name = instance.Name;
                existing.Version = instance.Version;
                existing.JarPath = instance.JarPath;
                existing.IconPath = instance.IconPath;
                existing.Type = instance.Type;
            }
            else
            {
                instances.Add(instance);
            }
            await SaveAsync(instances);
        }

        // 根据 Id 获取实例
        public static GameInstance GetById(string id)
        {
            var instances = Load();
            return instances.Find(i => i.Id == id);
        }
    }
}