using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace GIML
{
    public static class MindustryTextConverter
    {
        // 预定义颜色映射表（可根据需要扩充）
        private static readonly Dictionary<string, string> PredefinedColors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["black"] = "#000000",
            ["white"] = "#FFFFFF",
            ["red"] = "#FF0000",
            ["orange"] = "#FF7F00",
            ["yellow"] = "#FFFF00",
            ["green"] = "#00FF00",
            ["cyan"] = "#00FFFF",
            ["blue"] = "#0000FF",
            ["purple"] = "#7F00FF",
            ["pink"] = "#FF69B4",
            ["gray"] = "#808080",
            ["lightgray"] = "#D3D3D3",
            ["darkgray"] = "#A9A9A9",
        };

        public static Paragraph ToRichTextParagraph(string input)
        {
            var paragraph = new Paragraph();
            if (string.IsNullOrEmpty(input))
                return paragraph;

            int i = 0;
            int length = input.Length;
            Brush currentBrush = null; // null 表示默认颜色

            while (i < length)
            {
                if (input[i] == '[' && i + 1 < length)
                {
                    int end = input.IndexOf(']', i + 1);
                    if (end != -1)
                    {
                        string tag = input.Substring(i + 1, end - i - 1);
                        i = end + 1;

                        if (string.IsNullOrEmpty(tag))
                        {
                            // [] 重置为默认颜色
                            currentBrush = null;
                        }
                        else
                        {
                            // 尝试解析颜色
                            currentBrush = ParseColorToBrush(tag);
                        }
                        continue;
                    }
                }

                // 收集连续普通文本
                int start = i;
                while (i < length && input[i] != '[')
                {
                    i++;
                }
                string text = input.Substring(start, i - start);
                if (!string.IsNullOrEmpty(text))
                {
                    var run = new Run { Text = text };
                    if (currentBrush != null)
                        run.Foreground = currentBrush;
                    paragraph.Inlines.Add(run);
                }
            }

            return paragraph;
        }

        // 辅助方法：将颜色标记转换为 Brush
        private static Brush ParseColorToBrush(string colorTag)
        {
            // 先检查是否为已知颜色名称
            if (PredefinedColors.TryGetValue(colorTag, out string hex))
            {
                return new SolidColorBrush(HexToColor(hex));
            }
            // 检查是否为 #RGB 或 #RRGGBB 格式
            if (colorTag.StartsWith("#") && (colorTag.Length == 4 || colorTag.Length == 7))
            {
                return new SolidColorBrush(HexToColor(colorTag));
            }
            // 未知颜色返回 null
            return null;
        }

        // 将十六进制字符串转换为 Color
        private static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 3)
            {
                // 将 #RGB 转换为 #RRGGBB
                hex = new string(new char[] { hex[0], hex[0], hex[1], hex[1], hex[2], hex[2] });
            }
            if (hex.Length == 6)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return Color.FromArgb(255, r, g, b);
            }
            return Colors.Black; // 默认黑色
        }
    }
}
