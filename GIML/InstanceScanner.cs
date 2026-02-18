using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GIML
{
    public static class InstanceScanner
    {
        /// <summary>
        /// 扫描指定文件夹中的所有 jar 文件，返回 GameInstance 列表
        /// </summary>
        /// <param name="folderPath">要扫描的文件夹路径</param>
        /// <param name="includeSubfolders">是否包含子文件夹</param>
        /// <returns>GameInstance 列表</returns>
        public static async Task<List<GameInstance>> ScanForJarFilesAsync(string folderPath, bool includeSubfolders)
        {
            var instances = new List<GameInstance>();

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return instances;

            // 使用 Task.Run 在后台线程执行文件搜索，避免阻塞 UI
            await Task.Run(() =>
            {
                var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var jarFiles = Directory.EnumerateFiles(folderPath, "*.jar", searchOption);

                foreach (var jarPath in jarFiles)
                {
                    // 以文件名作为实例名称（不含扩展名）
                    string name = Path.GetFileNameWithoutExtension(jarPath);
                    // 可以读取 jar 的版本信息等（可选）
                    instances.Add(new GameInstance
                    {
                        Name = name,
                        Version = "未知", // 后续可扩展
                        JarPath = jarPath, // 需要给 GameInstance 添加 JarPath 属性
                        IconPath = "ms-appx:///Assets/GameIcons/DefaultIcon.png" // 默认图标
                    });
                }
            });

            return instances;
        }
    }
}