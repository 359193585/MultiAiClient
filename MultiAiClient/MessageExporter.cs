using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace MultiAIClient
{
    internal class MessageExporter
    {
        /// <summary>
        /// 将消息列表和URL导出到用户指定的文本文件
        /// </summary>
        /// <param name="messages">消息字符串列表</param>
        /// <param name="currentUrl">当前的网页URL</param>
        /// <param name="defaultFileBaseName">默认的文件名基础部分</param>
        public string  ExportMessagesToFile(List<string> messages, string currentUrl, string defaultFileBaseName = "webview_messages")
        {
            // 输入验证
            if (messages == null || messages.Count == 0)
            {
                MessageBox.Show("没有可导出的消息。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return "";
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                Title = "保存消息文件",
                FileName = GenerateDefaultFileName(defaultFileBaseName), // 生成带时间戳的默认文件名
                InitialDirectory = GetLastLocalFixedDrive()
            };

            bool? dialogResult = saveFileDialog.ShowDialog();

            string fullPath = saveFileDialog.FileName;
            if (dialogResult == true)
            {
                try
                {
                    WriteMessagesToFile(messages, currentUrl, fullPath);
                    MessageBox.Show($"消息已成功导出到：\n{fullPath}", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    string? directoryPath = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
                    {
                        Process.Start("explorer.exe", directoryPath);
                    }
                    return fullPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出文件时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return "";
        }

        /// <summary>
        /// 生成带日期时间戳的默认文件名
        /// </summary>
        private string GenerateDefaultFileName(string baseName)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"{baseName}_{timestamp}.txt";
        }

        /// <summary>
        /// 将消息列表和URL信息写入指定路径的文件
        /// </summary>
        private void WriteMessagesToFile(List<string> messages, string currentUrl, string filePath)
        {
            // 使用UTF-8编码确保中文等字符正确保存
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // 写入文件头部信息
                writer.WriteLine("// 网页消息导出");
                writer.WriteLine($"// 导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"// 来源URL: {currentUrl}");
                writer.WriteLine($"// 消息数量: {messages.Count} 条");
                writer.WriteLine(new string('-', 50)); // 分隔线
                writer.WriteLine();

                // 逐行写入消息内容，并编号
                for (int i = 0; i < messages.Count; i++)
                {
                    writer.WriteLine($"{i + 1:D2}. {messages[i]}");
                    writer.WriteLine();
                }

                writer.WriteLine();
                writer.WriteLine("// 导出结束");
            }
        }

        /// <summary>
        /// 获取最后一个本地硬盘（固定磁盘）的根路径
        /// </summary>
        private string GetLastLocalFixedDrive()
        {
            try
            {
                var lastDrive = DriveInfo.GetDrives()
                    .Where(drive => drive.IsReady && drive.DriveType == DriveType.Fixed) // 确保驱动器就绪且为本地硬盘
                    .OrderByDescending(drive => drive.Name) // 按盘符名称降序排列（例如 Z:、Y:、...、C:）
                    .Select(drive => drive.Name)
                    .FirstOrDefault(); // 取第一个，即排序后的“最后一个”

                return String.IsNullOrEmpty(lastDrive)? "c:\\":lastDrive;
            }
            catch (Exception ex)
            {
                // 记录日志或处理异常
                System.Diagnostics.Debug.WriteLine($"获取驱动器信息时出错: {ex.Message}");
                return "c:\\";
            }
        }
    }
}
