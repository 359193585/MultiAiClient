/*****************************************
 *
 * 项目名称： MultiAIClient  
 * 文 件 名:  InjectAndSubmitToYuanbao.cs
 * 命名空间： MultiAIClient.MultiUrlInject
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/12/9 8:56
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

namespace MultiAIClient.MultiUrlInject
{
    internal partial class InjectYuanbao
    {
        internal static async Task<bool> Hide_Yuanbao_Element(WebView2 targetWebView)
        {
            string script = await Scripts.LoadInjectionScriptAsync("YuanbaoHideElement.js");
            return await Scripts.RunInjectScript(targetWebView, script, "腾讯元宝隐藏元素脚本");
        }
      
        internal static async Task<bool> InjectJsToYuanbao(WebView2 targetWebView)
        {
            string script = await Scripts.LoadInjectionScriptAsync("YuanbaoSend.js");
            return await Scripts.RunInjectScript(targetWebView, script, "腾讯元宝脚本");
        }
        internal static async Task<bool> SubmitToYuanbao(WebView2 targetWebView, string text)
        {
            string currentUrl = targetWebView.Source?.ToString().ToLower() ?? "";
            string[] cssSelectors = GetSelector.GetSelectorsByUrl(currentUrl);
            string cssSelector = cssSelectors[0];
            string sendBtnSelector = cssSelectors[2];
            string injectText = Scripts.EscapeJavaScriptText(text);

            string script = $"window.__my_injected__.yuanbaoSend.tryCompleteOperation(`{cssSelector}`,`{injectText}`,`{sendBtnSelector}`)";
            return await Scripts.RunInjectScript(targetWebView, script, "腾讯元宝发送脚本");
        }
      
    }
}
