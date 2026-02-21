using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GIML
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerPage : Page, INotifyPropertyChanged
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

        private List<ServerInfo> _servers;
        public List<ServerInfo> Servers
        {
            get => _servers;
            set
            {
                _servers = value;
                OnPropertyChanged();
            }
        }

        public ServerPage()
        {
            InitializeComponent();
            LoadServersAsync();
        }

        private async Task LoadServersAsync()
        {
            var service = new NetworkService();
            try
            {
                IsLoading = true;
                var servers = await service.GetServersAsync();
                System.Diagnostics.Debug.WriteLine($"原始数据: {servers?.Count ?? 0} 个服务器");
                Servers = servers;
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

        private void CopyAddressButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取按钮的 Tag 并转换为 ServerInfo
            var button = sender as Button;
            var server = button?.Tag as ServerInfo;
            if (server == null) return;

            // 创建数据包并设置文本内容
            DataPackage package = new DataPackage();
            package.SetText(server.Address);
            Clipboard.SetContent(package);

            // 可选：给用户一个短暂的反馈（例如按钮文字闪烁或 ToolTip）
            // 这里简单显示一个提示（可能需要更复杂实现，但基础功能已完成）
            System.Diagnostics.Debug.WriteLine($"已复制地址: {server.Address}");
        }
    }
}
