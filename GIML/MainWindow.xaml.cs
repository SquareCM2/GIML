using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI.ApplicationSettings;
using Windows.Storage;  // 包含 ApplicationData
using GIML;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
//感谢深度求索公司对本项目的大力支持。

namespace GIML
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            resizeWindow();
            ContentFrame.Navigate(typeof(HomePage));
            NavView.BackRequested += OnNavViewBackRequested;
        }

        private void resizeWindow()
        {
            // 1. 获取当前窗口的句柄 (HWND)
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // 2. 通过句柄获取窗口的唯一 ID，进而得到 AppWindow 对象
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            // 3. 使用 Resize 方法设置你想要的初始大小 (宽度, 高度)，单位是像素
            // 例如，这里设置为 800x600
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 800, Height = 600 });

            // 获取窗口所在的显示区域
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);

            // 计算居中位置的坐标
            var centerPosition = new PointInt32(
                (displayArea.WorkArea.Width - 800) / 2,
                (displayArea.WorkArea.Height - 600) / 2
            );

            // 移动窗口到计算出的坐标
            appWindow.Move(centerPosition);

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                // 禁止窗口被调整大小（核心代码）
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
            }

            // 设置默认选中项
            NavView.SelectedItem = HomeMenuItem;

            // 同时可以默认导航到主页（可选）
            ContentFrame.Navigate(typeof(HomePage));
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                // 如果点击的是设置按钮，导航到设置页面
                // 注意：你需要先创建一个名为 SettingsPage 的页面
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                // 根据点击的菜单项进行导航
                var invokedItem = args.InvokedItemContainer as NavigationViewItem;
                if (invokedItem?.Tag != null)
                {
                    string pageTag = invokedItem.Tag.ToString();
                    switch (pageTag)
                    {
                        case "HomePage":
                            ContentFrame.Navigate(typeof(HomePage));
                            break;
                        case "DownloadPage":
                            ContentFrame.Navigate(typeof(DownloadPage));
                            break;
                            // 你可以在这里继续添加其他页面的判断
                    }
                }
            }
        }

        private bool _isInitialized = false; // 标记是否已执行过初始化

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                // 在这里执行你的初始化逻辑（例如扫描实例）
                await ScanAndUpdateInstancesAsync();
            }
        }

        public async Task ScanAndUpdateInstancesAsync()
        {
            // 读取设置
            var localSettings = ApplicationData.Current.LocalSettings;
            if (!localSettings.Values.TryGetValue("GameFolderPath", out object folderPathObj))
                return; // 未设置文件夹

            string folderPath = folderPathObj.ToString();

            // 执行扫描
            var instances = await InstanceScanner.ScanForJarFilesAsync(folderPath, true);

            // 获取当前显示的 HomePage
            if (ContentFrame.Content is HomePage homePage)
            {
                // 更新主页的实例集合
                homePage.UpdateInstances(instances);
            }
        }

        public void SetNavMenuItemsEnabled(bool isEnabled)
        {
            // 遍历普通菜单项
            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem navItem)
                {
                    navItem.IsEnabled = isEnabled;
                }
            }

            // 遍历底部菜单项（如果有）
            foreach (var item in NavView.FooterMenuItems)
            {
                if (item is NavigationViewItem navItem)
                {
                    navItem.IsEnabled = isEnabled;
                }
            }

            if (NavView.IsSettingsVisible && NavView.SettingsItem is NavigationViewItem settingsItem)
            {
                settingsItem.IsEnabled = isEnabled;
            }
        }

        private void OnNavViewBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            // 如果当前 Frame 可以后退
            if (ContentFrame.CanGoBack)
            {
                // 恢复菜单项启用状态
                SetNavMenuItemsEnabled(true);

                // 执行后退
                ContentFrame.GoBack();
            }
        }

        public void NavigateToPage(Type pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
