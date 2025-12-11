/*****************************************
 *
 * 项目名称： MultiAIClient  
 * 文 件 名:  GetSelectorByUrl.cs
 * 命名空间： MultiAIClient
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/12/5 12:52
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

namespace MultiAIClient
{
    internal class GetSelector
    {
        // 根据URL获取选择器
        internal static string GetSelectorByUrl(string url)
        {
            if (url.Contains("yuanbao.tencent.com")) return ".ql-editor";
            if (url.Contains("gemini.google.com")) return "div.ql-editor.textarea.new-input-ui";
            if (url.Contains("chatgpt.com")) return "#prompt-textarea";
            if (url.Contains("deepseek.com")) return "textarea";



            if (url.Contains("chat.openai.com")) return "#prompt-textarea";
            if (url.Contains("claude.ai")) return ".prose textarea";
            return "";
        }
        internal static string[] GetSelectorsByUrl(string url)
        {
            if (url.Contains("yuanbao.tencent.com"))
                return new string[] { ".ql-editor", ".hyc-component-text .hyc-content-text", "#yuanbao-send-btn" };

            if (url.Contains("gemini.google.com"))
                return new string[] { 
                    "div.ql-editor.textarea.new-input-ui",
                    ".query-text-line.ng-star-inserted" ,
                    ".mdc-icon-button.submit"};

            if (url.Contains("chatgpt.com"))
                return new string[] { "#prompt-textarea", "div[data-message-author-role=\"user\"]" , "#composer-submit-button" };

            if (url.Contains("deepseek.com"))
                return new string[] { "textarea", ".ds-message .fbb737a4", ".ds-icon-button.ds-icon-button--l.ds-icon-button--sizing-container" };


            return new string[] { "", "","","" };
        }
    }
}
