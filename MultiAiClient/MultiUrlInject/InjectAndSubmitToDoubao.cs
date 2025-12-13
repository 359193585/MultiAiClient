/*****************************************
 *
 * 项目名称： MultiAIClient  
 * 文 件 名:  InjectDoubao.cs
 * 命名空间： MultiAIClient.MultiUrlInject
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/12/12 12:47
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAIClient.MultiUrlInject
{
    internal class InjectCommno
    {
        internal static async Task<bool> InjectModule(WebView2 targetWebView)
        {
            string script = await Scripts.LoadInjectionScriptAsync("CommnoSend.js");
            return await Scripts.RunInjectScript(targetWebView, script, "通用脚本注入");
        }

        internal static async Task<bool> SubmitQuestion(WebView2 targetWebView, string text)
        {
            string currentUrl = targetWebView.Source?.ToString().ToLower() ?? "";
            string[] cssSelectors = GetSelector.GetSelectorsByUrl(currentUrl);
            string cssSelector = cssSelectors[0];
            string sendBtnSelector = cssSelectors[2];
            string injectText = Scripts.EscapeJavaScriptText(text);
            string script = $"window.__my_injected__.commonSend.tryCompleteOperation(`{cssSelector}`,`{injectText}`,`{sendBtnSelector}`)";
            return await Scripts.RunInjectScript(targetWebView, script, "通用发送脚本");
        }
    }
}
