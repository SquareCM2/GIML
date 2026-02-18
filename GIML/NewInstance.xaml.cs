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
using Microsoft.WindowsAppSDK.Runtime;
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
            string s = null;
            s = type + "-" + version;
            s = s.Replace(".", "-");
            return s;
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
                InstanceDownloadButton.IsEnabled = true;
                InstancePathName.PlaceholderText = ProcessInstanceName(selected.Type, selected.TagName);
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

        //private async Task DownloadFileAsync(string url, IProgress<double> progress, CancellationToken cancellationToken)
        //{
        //    using var httpClient = new HttpClient();
        //    using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        //    response.EnsureSuccessStatusCode();

        //    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        //    await using var contentStream = await response.Content.ReadAsStreamAsync();
        //    // 确定保存路径，例如：%LocalAppData%\GIML\Downloads\文件名
        //    var fileName = InstancePathName.Text == "" ? InstancePathName.PlaceholderText : InstancePathName.Text;
        //    var localSettings = ApplicationData.Current.LocalSettings;
        //    string filePath = "";
        //    if (localSettings.Values.TryGetValue("GameFolderPath", out object instancePath))
        //    {
        //        filePath = Path.Combine(instancePath.ToString(), fileName);
        //    }

        //    await using var fileStream = File.Create(filePath);
        //    var buffer = new byte[8192];
        //    long totalRead = 0;
        //    int bytesRead;
        //    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        //    {
        //        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
        //        totalRead += bytesRead;
        //        if (totalBytes > 0)
        //        {
        //            double percentage = (double)totalRead / totalBytes * 100;
        //            progress.Report(percentage);
        //        }
        //    }
        //}

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRelease != null)
            {
                DownloadingDialog.XamlRoot = this.XamlRoot;
                ContentDialogResult result = await DownloadingDialog.ShowAsync();
            }
        }
    }
}
