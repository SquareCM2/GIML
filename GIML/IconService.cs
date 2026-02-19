using System;
using System.Runtime.InteropServices;
using Microsoft.UI;

namespace GIML;

public static class IconService
{
    public static IconId GetApplicationIconId()
    {
        // Visual Studio 为 .NET 应用程序分配的资源 ID [citation:3]
        IntPtr iconResourceId = new(32512); // 固定值，代表主图标

        // 获取当前进程的模块句柄
        IntPtr hModule = GetModuleHandle(null);
        if (hModule == IntPtr.Zero)
        {
            return default;
        }

        // 加载图标
        IntPtr hIcon = LoadIcon(hModule, iconResourceId);
        if (hIcon == IntPtr.Zero)
        {
            return default;
        }

        // 将图标句柄转换为 WinUI 可用的 IconId
        return Win32Interop.GetIconIdFromIcon(hIcon);
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr LoadIcon(IntPtr hModule, IntPtr lpIconName);
}