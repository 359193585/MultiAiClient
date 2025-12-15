using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace MultiAIClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 捕获所有未处理的异常
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                File.WriteAllText("startup_crash.log", $"发生未处理异常: {args.ExceptionObject}");
            };

            try
            {
                // 您的原有启动代码
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                File.WriteAllText("startup_crash.log", $"启动过程中发生异常: {ex}");
                throw; // 重新抛出，让事件查看器也能捕获
            }
        }
    }

}
