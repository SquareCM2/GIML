using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GIML
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RunningInstance : Page
    {

        private Process _gameProcess;
        private bool _isProcessExited = false;

        public RunningInstance()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string jarPath)
            {
                StartGameProcess(jarPath);
            }
        }

        private async void StartGameProcess(string jarPath)
        {
            string JavaPath = null;
            string FolderPath = null;
            if (!string.IsNullOrEmpty(SettingsManager.Load().JavaPath))
            {
                JavaPath = SettingsManager.Load().JavaPath;
            }
            else
            {
                return;
            }

            if (!string.IsNullOrEmpty(SettingsManager.Load().GameFolderPath))
            {
                FolderPath = SettingsManager.Load().GameFolderPath;
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
                EnvironmentVariables = { ["MINDUSTRY_DATA_DIR"] = $"{FolderPath}\\{Path.GetFileNameWithoutExtension(jarPath)}" }
            };
            _gameProcess = new Process();
            _gameProcess.StartInfo = startInfo;
            _gameProcess.StartInfo.UseShellExecute = false;
            _gameProcess.StartInfo.RedirectStandardOutput = true;
            _gameProcess.StartInfo.RedirectStandardError = true;
            _gameProcess.StartInfo.CreateNoWindow = true;  // 不显示控制台窗口

            _gameProcess.EnableRaisingEvents = true;

            App.IsGameRunning = true;

            _gameProcess.Exited += (s, e) =>
            {
                _isProcessExited = true;
                // 进程退出时在 UI 线程上追加提示
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    AppendOutput("游戏进程已退出。");
                    App.IsGameRunning = false;
                    CloseButton_Click(null, null);
                });
            };

            try
            {
                _gameProcess.Start();

                // 异步读取标准输出
                Task.Run(async () =>
                {
                    using (var reader = _gameProcess.StandardOutput)
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = await reader.ReadLineAsync();
                            _ = DispatcherQueue.TryEnqueue(() => AppendOutput(line));
                        }
                    }
                });

                // 异步读取错误输出（用不同颜色标记）
                Task.Run(async () =>
                {
                    using (var reader = _gameProcess.StandardError)
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = await reader.ReadLineAsync();
                            _ = DispatcherQueue.TryEnqueue(() => AppendOutput("[错误] " + line));
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                AppendOutput($"启动失败: {ex.Message}");
            }
        }

        // 向输出文本框添加一行，并自动滚动到底部
        private void AppendOutput(string text)
        {
            if (string.IsNullOrEmpty(OutputTextBlock.Text))
                OutputTextBlock.Text = text;
            else
                OutputTextBlock.Text += Environment.NewLine + text;

            // 自动滚动到底部
            OutputScrollViewer.ChangeView(null, OutputScrollViewer.ScrollableHeight + 16, null, true);
        }

        // 点击“关闭并返回”按钮：终止进程（如果仍在运行）并返回
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // 如果进程还在运行，尝试终止
            if (!_isProcessExited && _gameProcess != null && !_gameProcess.HasExited)
            {
                try
                {
                    _gameProcess.Kill();
                }
                catch { }
            }

            // 返回前恢复菜单项
            if (App.MainWindow is MainWindow mainWindow)
            {
                mainWindow.SetNavMenuItemsEnabled(true);
                mainWindow.MainFrame.Navigate(typeof(HomePage));
                mainWindow.MainFrame.BackStack.Clear();
            }
        }

        // 在导航离开前，阻止用户意外离开（如果进程还在运行）
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // 如果进程仍在运行且不是通过我们的按钮主动返回，可以取消导航
            //if (!_isProcessExited && _gameProcess != null && !_gameProcess.HasExited)
            //{
            //    // 可选：弹出确认对话框
            //    // 这里简单取消导航，让用户先关闭游戏
            //    e.Cancel = true;
            //    // 可提示用户先关闭游戏
            //    AppendOutput("游戏仍在运行，请先关闭游戏再返回。");
            //}
            base.OnNavigatingFrom(e);
        }
    }
}

