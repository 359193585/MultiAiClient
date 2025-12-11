/*****************************************
 *
 * 项目名称： MultiAIClient  
 * 文 件 名:  InjectScripts.cs
 * 命名空间： MultiAIClient
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/12/10 11:52
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MultiAIClient
{
    internal partial class Scripts
    {
        //统一注入的聊天消息导航脚本
        internal static async Task<(string moduleScript, string initScript)> GetChatMessageNaviJS(string cssSelector)
        {
            if (string.IsNullOrEmpty(cssSelector))
            {
                return ($"(function(){{console.log(cssSelector is null or empty.);}})();", "");
            }
            string jsCode = await LoadInjectionScriptAsync("ChatMesgNavi.js");
            string initScript = @"
if (window.__my_injected__ && window.__my_injected__.chatNavigator) {
    window.__my_injected__.chatNavigator.init('" + cssSelector + @"');
}";

            return (jsCode, initScript);
        }

        internal static string GetNextMesgageJS()
        {
            string script = " __my_injected__.cn.n();";
            script = "__my_injected__.chatNavigator.goToNextUserMessage();";
            return script;
        }

        internal static string GetPrevMesgageJS()
        {
            string script = " __my_injected__.cn.p();";
            script =  "__my_injected__.chatNavigator.goToPrevUserMessage();";
            return script;
        }
        internal static string GetMesgCountJS()
        {
            string script = " __my_injected__.cn.c();";
            script = "__my_injected__.chatNavigator.messageCount();";
            return script;
        }

        internal static string GetAllUserMessagesJS()
        {
            string script = " __my_injected__.cn.g()";
            script =  "__my_injected__.chatNavigator.getMesgStr()";
            return script;
        }

        #region 辅助方法

        // 注入并运行脚本的通用方法
        internal static async Task<bool> RunInjectScript(WebView2 targetWebView, string script, string content)
        {
            if(targetWebView == null)
            {
                Debug.WriteLine("WebView2 CoreWebView2 未初始化，无法注入脚本。");
                return false;
            }
            string currentUrl = targetWebView.Source?.ToString().ToLower() ?? "";
            try
            {
                var result = await targetWebView.CoreWebView2.ExecuteScriptAsync(script);
                await Task.Delay(1000); // 额外等待1秒确保动态内容加载

                if (result == "true")
                {
                    Debug.WriteLine($"{currentUrl},{content},注入或运行成功: {result}");
                    return result == "true";
                }
                else
                {
                    Debug.WriteLine($"{currentUrl},{content},注入或运行失败: {result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{currentUrl},{content},注入或运行异常: {ex.Message}");
                return false;
            }
        }
        
        // 转义JavaScript文本
        internal static string EscapeJavaScriptText(string text)
        {
            return text.Replace("\\", "\\\\")
                      .Replace("'", "\\'")
                      .Replace("\"", "\\\"")
                      .Replace("`", "\\`")
                      .Replace("\r", "\\r")
                      .Replace("\n", "\\n");
        }

        internal async static Task<string> LoadInjectionScriptAsync(string scriptFileName)
        {
            string jsCode;

#if DEBUG

            // Debug模式：从原始代码的文件夹中读取js文件
            string sourceScriptPath = Path.Combine(
               AppDomain.CurrentDomain.BaseDirectory,
               $@"..\..\..\ComInjectScripts\{scriptFileName}"); // 根据项目结构调整".."的个数,确保debug模式下能读取原始的js文件
            string debugJsPath = Path.GetFullPath(sourceScriptPath);
            jsCode = await File.ReadAllTextAsync(debugJsPath);
#else
            // Release模式：从嵌入式资源中读取混淆后的文件
            // 注意：混淆后的文件路径和名称已在.csproj中定义（obfuscated目录和.obfuscated.js后缀）
            var assembly = Assembly.GetExecutingAssembly();
            // 注意：资源名称的组成规则是 [项目默认命名空间].[文件在项目中的路径]（注意路径中的斜杠变为点，目录分隔符变为点）
            // 例如，如果项目默认命名空间是MyApp，文件是Scripts/obfuscated/main.obfuscated.js
            // 则资源名可能是 MyApp.Scripts.obfuscated.main.obfuscated.js
            // 具体名称可以通过 assembly.GetManifestResourceNames() 方法调试查看
            string  resourceName = $"MultiAIClient.ComInjectScripts.obfuscated.{Path.GetFileNameWithoutExtension(scriptFileName)}.obfuscated.js";
            if (resourceName == null)
                return ConsolePrintJS();

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return ConsolePrintJS();
                }

                using StreamReader reader = new StreamReader(stream);
                jsCode = await reader.ReadToEndAsync();
            }
#endif
            return jsCode;
        }

        private static string  ConsolePrintJS()
        {
            return @"
(function(){
console.log('can not read obfuscated scripts: resource not found.')
}
)();//# sourceURL=cannotreadscripts.js";
        }

        #endregion

       
    }
}
