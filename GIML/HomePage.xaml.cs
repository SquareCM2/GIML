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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GIML
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    // 游戏实例模型
    public class GameInstance
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string JarPath { get; set; }
        public string IconPath { get; set; } // 图标路径，可以是本地路径或 ms-appx:///
    }

    // 占位符类（用于表示“添加新实例”项）
    public class AddInstancePlaceholder
    {
        // 不需要任何属性，仅用作类型标识
    }

    public class GameItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate InstanceTemplate { get; set; }
        public DataTemplate AddTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is GameInstance)
                return InstanceTemplate;
            else if (item is AddInstancePlaceholder)
                return AddTemplate;
            return base.SelectTemplateCore(item);
        }
    }

    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            value != null;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotImplementedException();
    }

    public sealed partial class HomePage : Page, INotifyPropertyChanged
    {
        private GameInstance _selectedInstance;
        public GameInstance SelectedInstance
        {
            get => _selectedInstance;
            set
            {
                // 如果传入的值是 GameInstance 类型，则正常更新
                if (value is GameInstance newInstance)
                {
                    if (_selectedInstance != newInstance)
                    {
                        _selectedInstance = newInstance;
                        OnPropertyChanged();
                    }
                }
                else
                {
                    // 如果传入的不是 GameInstance（例如 AddInstancePlaceholder），则设为 null
                    if (_selectedInstance != null)
                    {
                        _selectedInstance = null;
                        OnPropertyChanged();
                    }
                }
            }
        }
        public ObservableCollection<object> GameItems { get; } = new();
        public HomePage()
        {
            InitializeComponent();
            GameItems = new ObservableCollection<object>();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        public async void ShowMessage(string message)
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private object _selectedObject;
        public object SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject != value)
                {
                    _selectedObject = value;
                    OnPropertyChanged();

                    // 根据实际类型更新 SelectedInstance
                    if (value is GameInstance instance)
                        SelectedInstance = instance;
                    else
                        SelectedInstance = null; // 占位符或其他情况均视为未选中实例
                }
            }
        }

        private bool _isProcessingSelection;

        private void NavigateToNewInstancePage()
        {
            // 获取 MainWindow 实例（可以通过 App.MainWindow 或 Application.Current 获取）
            if (App.MainWindow is MainWindow mainWindow)
            {
                // 禁用侧边栏菜单项
                mainWindow.SetNavMenuItemsEnabled(false);

                // 导航到 NewInstancePage
                mainWindow.NavigateToPage(typeof(NewInstance));
            }
        }


        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isProcessingSelection) return;
            _isProcessingSelection = true;

            try
            {
                // 如果新选中项是占位符
                if (e.AddedItems.FirstOrDefault() is AddInstancePlaceholder)
                {
                    // 立即取消选中占位符（将 SelectedObject 设为 null）
                    SelectedObject = null;

                    // 导航到下载页面（确保 DownloadPage 存在）
                    NavigateToNewInstancePage();
                }
            }
            finally
            {
                _isProcessingSelection = false;
            }
        }



        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedInstance != null)
            {
                try
                {
                    // 创建 InstanceRunner 实例并运行
                    var runner = new InstanceRunner();
                    // 由于 RunInstance 可能阻塞，建议在后台线程运行
                    Task.Run(() => runner.RunInstance(SelectedInstance.JarPath, SelectedInstance.Name));
                }
                catch (Exception ex)
                {
                    // 捕获并显示错误
                    ShowMessage($"启动失败: {ex.Message}");
                }
            }
        }

        public void UpdateInstances(List<GameInstance> newInstances)
        {
            // 清空现有实例
            for (int i = GameItems.Count - 1; i >= 0; i--)
            {
                GameItems.RemoveAt(i);
            }

            // 添加新实例
            foreach (var inst in newInstances)
            {
                GameItems.Add(inst);
            }

            // 确保占位符在最后（如果不存在则添加）
            if (!GameItems.Any(item => item is AddInstancePlaceholder))
            {
                GameItems.Add(new AddInstancePlaceholder());
            }
        }
    }
}
