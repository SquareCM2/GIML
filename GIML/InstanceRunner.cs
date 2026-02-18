using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GIML
{
    internal class InstanceRunner
    {
        public void RunInstance(string jarPath, string InstanceName)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            string JavaPath = null;
            string FolderPath = null;
            if (localSettings.Values.TryGetValue("JavaPath", out object javaPath))
            {
                JavaPath = javaPath.ToString();
            }
            else
            {
                return;
            }

            if (localSettings.Values.TryGetValue("GameFolderPath", out object path))
            {
                FolderPath = path.ToString();
                //FolderPath = FolderPath.Substring(1, FolderPath.Length - 2);
            }
            else
            {
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = $"\"{JavaPath}\"",
                Arguments = $"-jar \"{jarPath}\"",
                UseShellExecute = false,
                // 添加环境变量示例
                EnvironmentVariables = { ["MINDUSTRY_DATA_DIR"] = $"{FolderPath}\\{InstanceName}" }
            };
            Process.Start(startInfo);
        }
    }
}
