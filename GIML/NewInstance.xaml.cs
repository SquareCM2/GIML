using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Controls.Markdown;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Net.Http;
using Windows.Storage;
using WinRT.Interop;




// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GIML
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewInstance : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        private AvailableInstance _selectedRelease;
        public AvailableInstance SelectedRelease
        {
            get => _selectedRelease;
            set
            {
                if (_selectedRelease != value)
                {
                    _selectedRelease = value;
                    OnPropertyChanged();
                }
            }
        }

        private string ProcessInstanceName(string type, string version)
        {
            string baseName = $"{type}-{version}".Replace(".", "-");
            string candidate = baseName;
            int counter = 2;

            if (!string.IsNullOrEmpty(SettingsManager.Load().GameFolderPath))
            {
                string folderPath = SettingsManager.Load().GameFolderPath;
                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    // 循环直到找到一个不存在的文件名（带 .jar 扩展名）
                    while (File.Exists(Path.Combine(folderPath, candidate + ".jar")))
                    {
                        candidate = $"{baseName}({counter})";
                        counter++;
                    }
                }
            }

            return candidate; // 返回唯一的文件名（不含扩展名）
        }

        private void ReleaseSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.FirstOrDefault() as AvailableInstance;
            if (selected != null)
            {
                Desc_MarkdownBlock.Text = selected.Body ?? "";
                InstanceName.Text = selected.Name ?? "";
                InstanceTime.Text =  selected.PublishedAt.ToString() ?? "";
                switch(selected.Type)
                {
                    case "vanilla":
                        InstanceType.Text = "原版";
                        break;
                    case "be":
                        InstanceType.Text = "前沿构建";
                        break;
                    case "arc":
                        InstanceType.Text = "学术端";
                        break;
                    case "x":
                        InstanceType.Text = "X端";
                        break;
                }
                InstanceVersion.Text = selected.TagName ?? "";
                InstancePathName.PlaceholderText = ProcessInstanceName(selected.Type, selected.TagName);
                UpdateDownloadButtonBasedOnName();
            }
            else
            {
                Desc_MarkdownBlock.Text = "";
                InstanceName.Text = "";
                InstanceTime.Text = "";
                InstanceType.Text = "";
                InstanceVersion.Text = "";
                InstanceDownloadButton.IsEnabled = false;
                InstancePathName.PlaceholderText = "";
            }
        }

        private List<AvailableInstance> _allReleases; // 所有从 API 获取的原始数据
        private ObservableCollection<AvailableInstance> _filteredReleases; // 绑定到 GridView

        public NewInstance()
        {
            InitializeComponent();
            _filteredReleases = new ObservableCollection<AvailableInstance>();
            ReleasesGridView.ItemsSource = _filteredReleases;
            LoadReleasesAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // 返回前恢复菜单项
            if (App.MainWindow is MainWindow mainWindow)
            {
                mainWindow.SetNavMenuItemsEnabled(true);
            }

            // 执行后退
            Frame.GoBack();
        }

        private async Task LoadReleasesAsync()
        {
            var service = new GitHubService();
            // 这里根据默认选中的来源加载
            var tag = (ReleasesFliter.SelectedItem as ComboBoxItem)?.Tag.ToString();
            if(_allReleases == null)
            {
                try
                {
                    IsLoading = true;
                    _allReleases = await service.GetReleasesAsync(tag);
                    ApplyFilter(tag);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"加载失败: {ex}");
                }
                finally
                {
                    IsLoading = false;
                }

                return;
            }
            var flitered = (from release in _allReleases where release.Type == tag select release);
            if (flitered.Count() == 0)
            {
                try
                {
                    IsLoading = true; // 开始加载

                    _allReleases.AddRange(await service.GetReleasesAsync(tag));

                    // 更新 GridView 的数据源
                    ApplyFilter(tag);
                }
                catch (Exception ex)
                {
                    // 可选：显示错误信息
                    System.Diagnostics.Debug.WriteLine($"加载失败: {ex}");
                    // 可以显示一个错误提示给用户
                }
                finally
                {
                    IsLoading = false; // 无论成功失败，结束加载
                }
            }

            ApplyFilter(tag);
        }

        private void ApplyFilter(string tag)
        {
            _filteredReleases.Clear();
            var filtered = from release in _allReleases where release.Type == tag select release;

            foreach (var release in filtered)
            {
                _filteredReleases.Add(release);
            }
        }

        private void FliterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 重新加载数据
            LoadReleasesAsync();
        }

        private async Task DownloadFileAsync(string url, IProgress<double> progress, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            var fileName = (InstancePathName.Text == "" ? InstancePathName.PlaceholderText : InstancePathName.Text) + ".jar";
            string filePath = "";
            if (!string.IsNullOrEmpty(SettingsManager.Load().GameFolderPath))
            {
                filePath = Path.Combine(SettingsManager.Load().GameFolderPath, fileName);
            }

            await using var fileStream = File.Create(filePath);
            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalRead += bytesRead;
                if (totalBytes > 0)
                {
                    double percentage = (double)totalRead / totalBytes * 100;
                    progress.Report(percentage);
                }
            }
        }

        private string GetTargetFilePath()
        {
            string fileName = (InstancePathName.Text == "" ? InstancePathName.PlaceholderText : InstancePathName.Text) + ".jar";
            if (!string.IsNullOrEmpty(SettingsManager.Load().GameFolderPath))
            {
                return Path.Combine(SettingsManager.Load().GameFolderPath, fileName);
            }
            return null;
        }

        private void DeleteIncompleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    System.Diagnostics.Debug.WriteLine($"已删除不完整文件: {filePath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"删除文件失败: {ex}");
                }
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRelease == null)
            {
                return;
            }
            string targetFilePath = GetTargetFilePath();
            //var dialog = Resources["DownloadingDialog"] as ContentDialog;
            //dialog.XamlRoot = this.XamlRoot;
            //var progressBar = dialog.FindName("DownloadProgressBar") as ProgressBar;
            //var statusText = dialog.FindName("DownloadStatusText") as TextBlock;

            var progressBar = new ProgressBar { Width = 300, Height = 20, Minimum = 0, Maximum = 100 };
            var statusText = new TextBlock { Text = "正在准备...", HorizontalAlignment = HorizontalAlignment.Center };
            var stack = new StackPanel();
            stack.Children.Add(progressBar);
            stack.Children.Add(statusText);

            var dialog = new ContentDialog
            {
                Title = "正在下载",
                Content = stack,
                CloseButtonText = "取消",
                XamlRoot = this.XamlRoot,
                MinWidth = 400
            };

            using var cts = new CancellationTokenSource();

            dialog.CloseButtonClick += (s, args) =>
            {
                cts.Cancel();
            };

            var showTask = dialog.ShowAsync();

            try
            {
                var progress = new Progress<double>(p =>
                {
                    progressBar.Value = p;
                    statusText.Text = $"下载进度：{p:F1}%";
                });

                await DownloadFileAsync(SelectedRelease.DownloadUrl, progress, cts.Token);

                if (App.MainWindow is MainWindow mainWindow)
                {
                    await mainWindow.ScanAndUpdateInstancesAsync();
                }

                if (dialog.IsLoaded)
                {
                    dialog.Hide();
                }

                CancelButton_Click(null, null);
            }
            catch (OperationCanceledException) {
                DeleteIncompleteFile(targetFilePath);
            }
            catch(Exception ex)
            {
                DeleteIncompleteFile(targetFilePath);
                if (dialog.IsLoaded)
                {
                    dialog.Hide(); 
                }
                await new ContentDialog
                {
                    Title = "下载失败",
                    Content = ex.Message,
                    CloseButtonText = "确定",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
        }

        public static bool IsValidInstanceName(string input)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            invalidChars = invalidChars.Concat(Path.GetInvalidPathChars()).ToArray();
            return !(input.IndexOfAny(invalidChars) != -1);
        }

        private void UpdateDownloadButtonBasedOnName()
        {
            // 判断依据：如果文本框有输入，则用输入内容；否则用 PlaceholderText（默认名称）
            string nameToCheck = string.IsNullOrEmpty(InstancePathName.Text)
                                ? InstancePathName.PlaceholderText
                                : InstancePathName.Text;
            bool isValid = IsValidInstanceName(nameToCheck);

            ErrorTextBlock.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;

            // 控制边框颜色
            if (isValid)
            {
                // 恢复默认边框颜色（从系统资源获取）
                InstancePathName.BorderBrush = (Brush)Application.Current.Resources["TextControlBorderBrush"];
            }
            else
            {
                // 设置为红色
                InstancePathName.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }

            // 下载按钮必须同时满足：有选中的 release 且名称合法
            InstanceDownloadButton.IsEnabled = (SelectedRelease != null) && isValid;
        }

        // 文本框内容变化时触发验证
        private void InstancePathName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDownloadButtonBasedOnName();
        }
    }
}
