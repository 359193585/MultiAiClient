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
        /// <summary>
        ///  根据URL获取选择器
        /// </summary>
        /// <param name="currentUrl"></param>
        /// <returns>stringp[],依次是：输入框选择器，提问问题选择器，发送按钮选择器</returns>
        internal static string[] GetSelectorsByUrl(string currentUrl)
        {
            if (currentUrl.Contains("yuanbao.tencent.com"))
                return [
                    ".ql-editor", 
                    ".hyc-component-text .hyc-content-text", 
                    "#yuanbao-send-btn"];

            if (currentUrl.Contains("gemini.google.com"))
                return [ 
                    "div.ql-editor.textarea.new-input-ui",
                    "span[class=\"user-query-bubble-with-background ng-star-inserted\"]" ,
                    ".mdc-icon-button.submit"];

            if (currentUrl.Contains("chatgpt.com"))
                return [
                    "#prompt-textarea", 
                    "div[data-message-author-role=\"user\"]" , 
                    "#composer-submit-button"];

            if (currentUrl.Contains("deepseek.com"))
                return [
                    "textarea[class=\"_27c9245 ds-scroll-area d96f2d2a\"][placeholder]", 
                    ".ds-message .fbb737a4", 
                    ".ds-icon-button.ds-icon-button--l.ds-icon-button--sizing-container" ];

            if (currentUrl.Contains("doubao.com"))
                return [
                    "textarea[data-testid=\"chat_input_input\"][placeholder]",
                    ".max-w-full .container-QQkdo4",
                    "button[data-testid=\"chat_input_send_button\" ][id=\"flow-end-msg-send\"]"];


            //其他，尝试一个初始值
            return [
                "textarea",
                ".query-text-line", 
                "button"];
        }
    }
}
