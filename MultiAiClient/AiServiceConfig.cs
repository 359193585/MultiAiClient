/*****************************************
 *
 * 项目名称： MultiAIClient  
 * 文 件 名:  AiServiceConfig.cs
 * 命名空间： MultiAIClient
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/12/24 8:19
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace MultiAIClient
{
    /// <summary>
    /// AI 服务配置项的结构
    /// </summary>
    public class AiServiceConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("dataFolderName")]
        public string DataFolderName { get; set; } = string.Empty;

        [JsonPropertyName("iconPath")]
        public string IconPath { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int Order { get; set; } = 0;
    }

    public class ConfigGetSet
    {
        public string ConfigFileName = "config.json";
        private List<AiServiceConfig>? _aiServices;
        public ConfigGetSet()
        {
            string exeDir = Path.GetDirectoryName(Environment.ProcessPath!)!;
            string configPath = Path.Combine(exeDir, "config.json");
            if (!File.Exists(configPath))
            {
                MessageBox.Show($"配置文件 {configPath} 未找到。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        internal List<AiServiceConfig>? GetConfig()
        {
#if DEBUG
            ConfigFileName = Path.GetFullPath("..\\..\\..\\config.json");

#endif
            try
            {
                string jsonString = File.ReadAllText(ConfigFileName);
                if (!String.IsNullOrEmpty(jsonString))
                {
                    _aiServices = JsonSerializer.Deserialize<List<AiServiceConfig>>(jsonString);
                    return _aiServices;
                }
            }
            catch 
            {
            }
            return [];
        }
        internal void SetConfig(List<AiServiceConfig>? aiServices)
        {
            File.WriteAllText(ConfigFileName, JsonSerializer.Serialize(aiServices));
        }
    }
}
