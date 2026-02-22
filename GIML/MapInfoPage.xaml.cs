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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GIML
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapInfoPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private DetailedMapInfo _info;
        public DetailedMapInfo info
        {
            get => _info;
            set
            {
                _info = value;
                System.Diagnostics.Debug.WriteLine("info 属性已更新");
                OnPropertyChanged();
            }
        }

        private DetailedMapInfoTags _tags;
        public DetailedMapInfoTags tags
        {
            get => _tags;
            set
            {
                if (_tags != value)
                {
                    _tags = value;
                    OnPropertyChanged();
                    // 如果希望 Tags 内部的属性变化也通知，可以在此处监听 Tags 的变化
                }
            }
        }

        private MapRules _rules;
        public MapRules rules
        {
            get => _rules;
            set
            {
                if (_rules != value)
                {
                    _rules = value;
                    OnPropertyChanged();
                    // 如果希望 Tags 内部的属性变化也通知，可以在此处监听 Tags 的变化
                }
            }
        }

        private WayzerUser _user;
        public WayzerUser user
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged();
                    // 如果希望 Tags 内部的属性变化也通知，可以在此处监听 Tags 的变化
                }
            }
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

        public MapInfoPage()
        {
            //_info = new DetailedMapInfo();
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is MapInfo map)
            {
                LoadDetailedInfoAsync(map.latest);
            }
        }

        private async Task LoadDetailedInfoAsync(string hash)
        {
            var service = new NetworkService();

            IsLoading = true;

            try
            {
                var LoadedInfo = await service.GetDetailedInfo(hash);
                System.Diagnostics.Debug.WriteLine(LoadedInfo.PreviewImg);
                if (LoadedInfo != null)
                {
                    info = LoadedInfo;
                    tags = LoadedInfo.Tags;
                    rules = LoadedInfo.Tags.Rules;
                    user = LoadedInfo.Uploader;

                    // 检查 Rules 是否为 null
                    if (rules == null)
                        System.Diagnostics.Debug.WriteLine("警告：rules 为 null");
                    else
                        System.Diagnostics.Debug.WriteLine($"isWavesOn = {rules.isWavesOn}");

                    Bindings.Update();
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载失败: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
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
    }
}
