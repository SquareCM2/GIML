using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
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
        /// 

        public static string GetJarVersion(string jarPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(jarPath))
            {
                ZipArchiveEntry version = archive.GetEntry("version.properties");
                if (version == null)
                {
                    return null;
                }
                using (StreamReader reader = new StreamReader(version.Open(), Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("build="))
                        {
                            return line.Substring("build=".Length).Trim();
                        }
                    }
                }
            }
            return null;
        }

        public static string JarToType(string jarPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(jarPath))
            {
                ZipArchiveEntry version = archive.GetEntry("version.properties");
                if (version == null)
                    return null;
                using (StreamReader reader = new StreamReader(version.Open(), Encoding.UTF8))
                {
                    string line;
                    while((line = reader.ReadLine()) != null)
                    {
                        if(line.StartsWith("type="))
                        {
                            if (line.Substring("type=".Length).Trim() == "official")
                                break;
                            else if (line.Substring("type=".Length).Trim() == "bleeding-edge")
                            {
                                return "be";
                            }
                        }
                    }
                }

                ZipArchiveEntry mainfest = archive.GetEntry("META-INF/MANIFEST.MF");
                if (mainfest == null)
                    return null;
                using (StreamReader reader = new StreamReader(mainfest.Open(), Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("Main-Class:"))
                        {
                            if (line.Substring("Main-Class:".Length).Trim() == "mindustry.desktop.DesktopLauncher")
                                break;
                            else if (line.Substring("Main-Class:".Length).Trim() == "mindustry.desktop.DesktopLauncher0")
                            {
                                return "arc";
                            }
                        }
                    }
                }

                bool hasMindustryXFolder = archive.Entries.Any(entry =>
                    entry.FullName.StartsWith("mindustryX/", StringComparison.OrdinalIgnoreCase));
                return hasMindustryXFolder ? "x" : "vanilla";
            }
        }

        public static string GetIconFromType(string type)
        {
            switch(type)
            {
                case "vanilla":
                    return "ms-appx:///Assets/GameIcons/DefaultIcon.png";
                case "be":
                    return "ms-appx:///Assets/GameIcons/DefaultIcon.png";
                case "arc":
                    return "ms-appx:///Assets/GameIcons/ArcIcon.png";
                case "x":
                    return "ms-appx:///Assets/GameIcons/XIcon.png";
            }
            return "ms-appx:///Assets/GameIcons/DefaultIcon.png";
        }

        public static string GetTypeName(string type)
        {
            switch (type)
            {
                case "vanilla":
                    return "原版";
                case "be":
                    return "前沿构建";
                case "arc":
                    return "学术端";
                case "x":
                    return "X端";
            }
            return "未知";
        }

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
                    string instanceType = JarToType(jarPath);
                    // 可以读取 jar 的版本信息等（可选）
                    instances.Add(new GameInstance
                    {
                        Name = name,
                        Version = GetJarVersion(jarPath), // 后续可扩展
                        JarPath = jarPath, // 需要给 GameInstance 添加 JarPath 属性
                        Type = GetTypeName(instanceType),
                        IconPath = GetIconFromType(instanceType)
                    });
                }
            });

            return instances;
        }
    }
}