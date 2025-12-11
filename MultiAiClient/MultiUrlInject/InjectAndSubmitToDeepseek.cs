/*****************************************
 *
 * 项目名称： MultiAIClient  
 * 文 件 名:  InjectAndSubmitToDeepseek.cs
 * 命名空间： MultiAIClient
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/12/10 22:48
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

using Microsoft.Web.WebView2.Wpf;

namespace MultiAIClient.MultiUrlInject
{
    internal partial class InjectDeepseek
    {
        internal static async Task<bool> InjectModule(WebView2 targetWebView)
        {
            string script = await Scripts.LoadInjectionScriptAsync("DeepseekSend.js");
            return await Scripts.RunInjectScript(targetWebView, script, "DeepSeek 脚本");
        }

        internal static async Task<bool> SubmitQuestion(WebView2 targetWebView, string text)
        {
            string currentUrl = targetWebView.Source?.ToString().ToLower() ?? "";
            string[] cssSelectors = GetSelector.GetSelectorsByUrl(currentUrl);
            string cssSelector = cssSelectors[0];
            string sendBtnSelector = cssSelectors[2];
            string injectText = Scripts.EscapeJavaScriptText(text);
            string script = $"window.__my_injected__.deepseekSend.tryCompleteOperation(`{cssSelector}`,`{injectText}`,`{sendBtnSelector}`)";
            return await Scripts.RunInjectScript(targetWebView, script, "DeepSeek 发送脚本");
        }

    }
}
