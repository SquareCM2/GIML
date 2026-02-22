using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;

namespace GIML
{
    public class ItemNameConverter : IValueConverter
    {
        // 英文到中文的映射表（不区分大小写）
        private static readonly Dictionary<string, string> ItemNameMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["copper"] = "铜",
                ["lead"] = "铅",
                ["coal"] = "煤炭",
                ["graphite"] = "石墨",
                ["titanium"] = "钛",
                ["thorium"] = "钍",
                ["silicon"] = "硅",
                ["plastanium"] = "塑钢",
                ["phase-fabric"] = "相织布",
                ["surge-alloy"] = "巨浪合金",
                ["spore-pod"] = "孢子荚",
                ["sand"] = "沙",
                ["blast-compound"] = "爆炸混合物",
                ["pyratite"] = "硫化物",
                ["metaglass"] = "钢化玻璃",
                ["scrap"] = "废料",
                ["fissile-matter"] = "裂变产物",
                ["beryllium"] = "铍",
                ["tungsten"] = "钨",
                ["oxide"] = "氧化物",
                ["carbide"] = "碳化物",
                ["dormant-cyst"] = "休眠囊肿"
            };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string itemKey && ItemNameMap.TryGetValue(itemKey, out string chineseName))
                return chineseName;
            // 如果找不到映射，返回原始值（或空字符串）
            return value ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}