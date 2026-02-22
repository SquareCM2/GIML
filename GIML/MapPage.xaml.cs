using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

using Windows.Storage;               // 用于文件操作
using System.Net.Http;                // 下载文件

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GIML
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page, INotifyPropertyChanged
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

        private ObservableCollection<MapInfo> _maps;
        public ObservableCollection<MapInfo> maps
        {
            get => _maps;
            set
            {
                _maps = value;
                OnPropertyChanged();
            }
        }

        private int _currentBegin;
        private bool _isLoadingMore;
        private bool _hasMore;
        private string _currentMode, _currentVersion, _currentSorting, _currentSearch = string.Empty;

        public MapPage()
        {
            _maps = new ObservableCollection<MapInfo>();
            InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var newSearch = (sender as TextBox)?.Text;
            if (newSearch == _currentSearch) // 未改变，直接返回
                return;

            _currentSearch = newSearch;
            _ = LoadMoreMapsAsync(reset: true);
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                this.Focus(FocusState.Programmatic);

                e.Handled = true; // 阻止事件继续冒泡
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combobox = sender as ComboBox;
            if (combobox == null) return;
            switch (combobox.Name)
            {
                case "ModeComboBox":
                    _currentMode = (combobox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                    break;
                case "SortComboBox":
                    _currentSorting = (combobox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                    break;
                case "VersionComboBox":
                    _currentVersion = (combobox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                    break;
                default:
                    return; // 忽略未知的 ComboBox
            }
            _ = LoadMoreMapsAsync(reset: true);
        }

        private async Task LoadMoreMapsAsync(bool reset = false)
        {
            if (_isLoadingMore) return;

            if (reset)
            {
                _currentBegin = 0;
                _hasMore = true;
                maps.Clear();
            }

            if (!_hasMore) return;

            _isLoadingMore = true;
            IsLoading = true;

            var service = new NetworkService();

            try
            {
                // 调用已有的方法，传入当前筛选条件和 begin
                var newMaps = await service.GetMapsAsync(_currentBegin, _currentMode, _currentVersion, _currentSorting, _currentSearch);

                if (newMaps != null && newMaps.Count > 0)
                {
                    foreach (var map in newMaps)
                        maps.Add(map);

                    if (newMaps.Count < 15)
                        _hasMore = false;
                    else
                        _currentBegin += newMaps.Count;
                }
                else
                {
                    _hasMore = false;
                }
            }
            catch (Exception ex)
            {
                // 处理异常（例如弹出提示）
                System.Diagnostics.Debug.WriteLine($"加载失败: {ex}");
            }
            finally
            {
                _isLoadingMore = false;
                IsLoading = false;
            }
        }

        private void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            // 当滚动到底部（距离底部 100 像素内）且不在加载时，触发加载更多
            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 10)
            {
                _ = LoadMoreMapsAsync();
            }
        }

        private void MapsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(MapsGridView);
            if (scrollViewer != null)
            {
                scrollViewer.ViewChanged += OnScrollViewerViewChanged;
                System.Diagnostics.Debug.WriteLine("ScrollViewer 绑定成功");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("未找到 ScrollViewer");
            }
        }

        // 辅助方法：查找视觉树中的子元素
        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t)
                    return t;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void CopyCommandButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取按钮及其绑定的地图数据
            var button = sender as Button;
            var map = button?.Tag as MapInfo;
            if (map == null) return;

            // 构造要复制的命令（使用地图的 Id 作为标识）
            string command = $"/vote map {map.Id}";

            // 将命令放入剪贴板
            var dataPackage = new DataPackage();
            dataPackage.SetText(command);
            Clipboard.SetContent(dataPackage);

            // 可选：给用户一个短暂的反馈（如弹出提示）
            // 这里简单输出调试信息
            System.Diagnostics.Debug.WriteLine($"已复制命令: {command}");
        }

        private async void MapDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var map = button?.Tag as MapInfo;
            if (map == null) return;

            // 获取所有实例（从 App.GameInstances 静态属性）
            var instances = App.GameInstances;
            if (instances == null || instances.Count == 0)
            {
                await ShowMessage("没有可用的游戏实例，请先下载一个实例。");
                return;
            }

            // 创建选择实例的对话框
            var dialog = new ContentDialog
            {
                Title = "选择目标实例",
                PrimaryButtonText = "下载",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            // 创建一个 ComboBox 显示实例名称
            var comboBox = new ComboBox
            {
                PlaceholderText = "请选择一个实例",
                Width = 300,
                Margin = new Thickness(20)
            };
            foreach (var inst in instances)
            {
                comboBox.Items.Add(new ComboBoxItem
                {
                    Content = inst.Name,
                    Tag = inst   // 将实例对象存储在 Tag 中
                });
            }
            comboBox.SelectedIndex = 0; // 默认选中第一个

            dialog.Content = comboBox;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var selectedItem = comboBox.SelectedItem as ComboBoxItem;
                var selectedInstance = selectedItem?.Tag as GameInstance;
                if (selectedInstance == null) return;

                // 开始下载地图
                await DownloadMapToInstanceAsync(map, selectedInstance);
            }
        }

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var map = button?.Tag as MapInfo;
            if (map == null) return;

            // 假设 MapPage 所在的 Frame 可以直接使用 this.Frame

            if (App.MainWindow is MainWindow mainWindow)
            {
                // 禁用侧边栏菜单项
                mainWindow.SetNavMenuItemsEnabled(false);

                // 导航到 NewInstancePage
                Frame.Navigate(typeof(MapInfoPage), map);
            }
            
        }

        private async Task DownloadMapToInstanceAsync(MapInfo map, GameInstance instance)
        {
            try
            {
                // 构建目标文件夹：jar文件所在目录 + 实例名(不含扩展名) + \maps
                string jarDirectory = Path.GetDirectoryName(instance.JarPath);
                string instanceName = Path.GetFileNameWithoutExtension(instance.JarPath);
                string mapsFolder = Path.Combine(jarDirectory, instanceName, "maps");

                // 确保文件夹存在
                Directory.CreateDirectory(mapsFolder);
                string downloadUrl = $"https://api.mindustry.top/maps/{map.latest}.msav";

                string fileName = $"{map.latest}.msav";
                string filePath = Path.Combine(mapsFolder, fileName);

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();

                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }
                ShowMessage($"成功下载到地址：{filePath}");
            }
            catch (Exception ex)
            {
                ShowMessage($"下载失败：{ex.Message}");
            }
        }

        private async Task ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "提示",
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
