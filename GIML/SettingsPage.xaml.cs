using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using WinRT.Interop;

using Windows.Storage.Pickers;  // 用于 FolderPicker

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GIML
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            LoadSavedFolder();
        }

        private void LoadSavedFolder()
        {
            // 读取保存的文件夹路径
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("GameFolderPath", out object path))
            {
                FolderPathBox.Text = path.ToString();
            }
            if (localSettings.Values.TryGetValue("JavaPath", out object javaPath))
            {
                JavaPathBox.Text = javaPath.ToString();
            }

        }

        private async void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPicker = new FolderPicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                folderPicker.FileTypeFilter.Add("*"); // 需要至少一个文件类型筛选

                // 获取当前窗口句柄
                var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(folderPicker, hWnd);

                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    string folderPath = folder.Path;
                    FolderPathBox.Text = folderPath;

                    // 保存到设置
                    ApplicationData.Current.LocalSettings.Values["GameFolderPath"] = folderPath;
                }

                if (App.MainWindow is MainWindow mainWindow)
                {
                    await mainWindow.ScanAndUpdateInstancesAsync();
                }
            }
            catch (Exception ex)
            {
                // 记录或显示错误
                System.Diagnostics.Debug.WriteLine($"选择文件时出错: {ex}");
                // 可选：向用户显示错误对话框
                var errorDialog = new ContentDialog
                {
                    Title = "错误",
                    Content = $"无法打开文件选择器: {ex.Message}",
                    CloseButtonText = "确定",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void BrowseJava_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileOpenPicker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                fileOpenPicker.FileTypeFilter.Add(".exe");

                // 获取当前窗口句柄
                var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(fileOpenPicker, hWnd);

                StorageFile file = await fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    string filePath = file.Path;
                    JavaPathBox.Text = filePath;

                    // 保存到设置
                    ApplicationData.Current.LocalSettings.Values["JavaPath"] = filePath;
                }

                if (App.MainWindow is MainWindow mainWindow)
                {
                    await mainWindow.ScanAndUpdateInstancesAsync();
                }
            }
            catch (Exception ex)
            {
                // 记录或显示错误
                System.Diagnostics.Debug.WriteLine($"选择文件时出错: {ex}");
                // 可选：向用户显示错误对话框
                var errorDialog = new ContentDialog
                {
                    Title = "错误",
                    Content = $"无法打开文件选择器: {ex.Message}",
                    CloseButtonText = "确定",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}
