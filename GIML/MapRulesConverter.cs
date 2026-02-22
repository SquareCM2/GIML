using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GIML
{
    public class MapRulesConverter : System.Text.Json.Serialization.JsonConverter<MapRules>
    {
        public override MapRules Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string hjsonString = reader.GetString();
                try
                {
                    return ParseMapRulesFromHjson(hjsonString);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"正则提取失败: {ex.Message}");
                    return null;
                }
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                return System.Text.Json.JsonSerializer.Deserialize<MapRules>(ref reader, options);
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            else
            {
                reader.Skip();
                return null;
            }
        }

        private MapRules ParseMapRulesFromHjson(string hjson)
        {
            var rules = new MapRules();
            var regexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline;

            // 辅助方法：提取布尔值
            bool GetBool(string key)
            {
                var match = Regex.Match(hjson, $@"\b{key}\s*:\s*(true|false)\b", regexOptions);
                return match.Success && match.Groups[1].Value == "true";
            }

            // 辅助方法：提取整数，若缺失返回默认值
            int GetInt(string key, int defaultValue)
            {
                var match = Regex.Match(hjson, $@"\b{key}\s*:\s*(-?\d+)\b", regexOptions);
                return match.Success ? int.Parse(match.Groups[1].Value) : defaultValue;
            }

            // 辅助方法：提取浮点数，若缺失返回默认值
            double GetDouble(string key, double defaultValue)
            {
                var match = Regex.Match(hjson, $@"\b{key}\s*:\s*(-?\d+(?:\.\d+)?)\b", regexOptions);
                return match.Success ? double.Parse(match.Groups[1].Value) : defaultValue;
            }

            // 填充 MapRules 属性
            rules.isWavesOn = GetBool("waves");
            rules.isAttackOn = GetBool("attackMode");
            rules.DamageExplosions = GetBool("damageExplosions");
            rules.Fire = GetBool("fire");
            rules.UnitAmmo = GetBool("unitAmmo");
            rules.UnitCapVariable = GetBool("unitCapVariable");

            rules.EnemyCoreBuildRadius = GetInt("enemyCoreBuildRadius", 400);
            rules.WaveSpacing = GetInt("waveSpacing", 7200);
            rules.UnitCap = GetInt("unitCap", 0);

            rules.UnitDamageMultiplier = GetDouble("unitDamageMultiplier", 1);
            rules.BuildCostMultiplier = GetDouble("buildCostMultiplier", 1);
            rules.BuildSpeedMultiplier = GetDouble("buildSpeedMultiplier", 1);
            rules.UnitBuildSpeedMultiplier = GetDouble("unitBuildSpeedMultiplier", 1);
            rules.DeconstructRefundMultiplier = GetDouble("deconstructRefundMultiplier", 0);
            rules.BlockHealthMultiplier = GetDouble("blockHealthMultiplier", 1);
            rules.BlockDamageMultiplier = GetDouble("blockDamageMultiplier", 1);

            rules.Loadout = ParseLoadout(hjson);

            return rules;

            List<LoadoutItem> ParseLoadout(string hjson)
            {
                var list = new List<LoadoutItem>();
                // 匹配 loadout 数组的内容（非贪婪，取第一个 [...] 内的内容）
                var arrayMatch = Regex.Match(hjson, @"loadout\s*:\s*\[(.*?)\]", regexOptions);
                if (!arrayMatch.Success)
                    return list;

                string loadoutContent = arrayMatch.Groups[1].Value;

                // 逐个匹配对象
                var objMatches = Regex.Matches(loadoutContent, @"\{\s*item\s*:\s*([^,}\s]+)\s*,\s*amount\s*:\s*(\d+)\s*\}", regexOptions);
                foreach (Match objMatch in objMatches)
                {
                    list.Add(new LoadoutItem
                    {
                        Item = objMatch.Groups[1].Value,
                        Amount = int.Parse(objMatch.Groups[2].Value)
                    });
                }
                return list;
            }
        }

        //解析 loadout 数组
        public override void Write(Utf8JsonWriter writer, MapRules value, JsonSerializerOptions options)
        {
            System.Text.Json.JsonSerializer.Serialize(writer, value, options);
        }
    }
}