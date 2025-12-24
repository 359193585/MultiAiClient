  /*****************************************
 *
 * 项目名称： MultiAIClient  
 * 文 件 名:  MainWindow.xaml.cs
 * 命名空间： MultiAIClient
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/12/4 18:44
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

using Microsoft.Web.WebView2.Core;
using MultiAIClient.MultiUrlInject;
using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace MultiAIClient
{
  
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string AppDataPath =
        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyMultiAIClientData");

        //private readonly string AppDataPath =  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyMultiAIClientData");

        private Dictionary<TabItem, WebView2> _tabWebViewMapping = [];
        private Dictionary<TabItem, Ellipse> _statusIndicators = new Dictionary<TabItem, Ellipse>();

        List<AiServiceConfig>? services;  // All ai url settings


        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
            InitializeAllTabs();
            double screenHeight = SystemParameters.WorkArea.Height;
            this.Height = screenHeight;
            this.MaxHeight = screenHeight;
            this.Top = 0;
            this.Left = 0;
            try
            {
                this.Icon = new BitmapImage(new Uri("pack://application:,,,/Icons/MyAppIcon.ico"));
            }
            catch { }
        }

        private void InitializeAllTabs()
        {
            ConfigGetSet configGetSet = new ConfigGetSet();
            services = configGetSet.GetConfig();
            TabControlDragDropBehavior.ConfigFileName = configGetSet.ConfigFileName;

            if (services == null || services.Count == 0)
            {
                MessageBox.Show("配置文件中未找到 AI 服务配置。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                foreach (var service in services)
                {
                   
                    // 1 创建 TabItem
                    // TabItem 标题是StackPanel容器，有水平排列的图片和文字）
                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    if (File.Exists(service.IconPath))
                    {
                        Image image = LoadIcon(service);
                        stackPanel.Children.Add(image);
                    }

                    var textBlock = new TextBlock
                    {
                        Text = service.Name,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5, 0, 0, 0)
                    };
                    stackPanel.Children.Add(textBlock);

                    // 添加加载状态指示器（初始隐藏）
                    var statusIndicator = new Ellipse
                    {
                        Width = 8,
                        Height = 8,
                        Fill = Brushes.Gray, // 初始灰色表示未加载
                        Margin = new Thickness(8, 0, 0, 0),
                        Visibility = Visibility.Collapsed,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    stackPanel.Children.Add(statusIndicator);

                    // 2 创建 WebView2 控件实例
                    var webView = new WebView2()
                    {
                        Name = $"{service.Name.Replace(" ", "")}WebView" // 动态命名
                    };
                    webView.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;

                    var tabItem = new TabItem()
                    {
                        Header = stackPanel,
                        Content = webView,
                        Tag = service
                    };

                    tabItem.ContextMenu = (ContextMenu)this.FindResource("TabItemContextMenu");//右键菜单
                    _tabWebViewMapping.Add(tabItem, webView);
                    _statusIndicators.Add(tabItem, statusIndicator);

                    UpdateTabStatus(tabItem, TabStatus.NotLoaded);
                    AIAggregatorTabs.Items.Add(tabItem);

                    _ = InitializeWebViewWithEnvironment(
                        webView: webView,
                        url: service.Url,
                        serviceName: service.Name,
                        dataFolderName: service.DataFolderName,
                        tabItem: tabItem
                    );
                    AIAggregatorTabs.SelectionChanged += OnTabSelectionChanged;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载或初始化服务失败: {ex.Message}", "严重错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

      
     

        private async void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mesgCount.Content = "";
            if (e.AddedItems.Count == 0 || e.Source != AIAggregatorTabs) return;

            var selectedTabItem = e.AddedItems[0] as TabItem;
            if (selectedTabItem?.Content is WebView2 targetWebView)
            {
                if (targetWebView.CoreWebView2 == null)
                {
                    var serviceConfig = selectedTabItem.Tag as AiServiceConfig;
                    if (serviceConfig != null)
                    {
                        await InitializeWebViewWithEnvironment(
                           webView: targetWebView,
                           url: serviceConfig.Url,
                           serviceName: serviceConfig.Name,
                           dataFolderName: serviceConfig.DataFolderName,
                           tabItem: selectedTabItem
                            );
                    }
                }
                else
                {
                    // 已经初始化
                    //targetWebView.Reload();  // 刷新页面（可选）
                    UpdateTabStatus(selectedTabItem, TabStatus.Loaded);

                }
            }
        }
        
        private void OnLoadInBrowserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WebView2? webView2;
            GetWebViewObject(sender, out webView2);
            if (webView2 == null)
            {
                return;
            }
            // url 浏览器打开
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = webView2.Source.ToString(),
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"在浏览器中打开失败: {ex.Message}", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void OnRefreshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WebView2? webView2;
            GetWebViewObject(sender, out webView2);
            if (webView2 == null)
            {
                return;
            }
            // 刷新页面
            try
            {
                webView2.Reload();
                mesgCount.Content = "";
            }
            catch { }
        }
        private void WebView2_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {

            WebView2? webView = sender as WebView2;
            if (webView == null) return;
            if (e.IsSuccess)
            {
                DenyDevToolInRelease(webView);
                // 订阅上下文菜单请求事件
                webView.CoreWebView2.ContextMenuRequested += WebView2_CoreWebView2_ContextMenuRequested;
              
            }
        }

        //  上下文菜单请求事件
        private void WebView2_CoreWebView2_ContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {
#if RELEASE
            try
            {
                var menuItems = e.MenuItems;

                // 查找名为 "inspectElement" 的“检查”元素菜单项
                var inspectItem = menuItems.FirstOrDefault(item => item.Name == "inspectElement");

                if (inspectItem != null)
                {
                    menuItems.Remove(inspectItem);
                }
            }
            catch { }
           
#endif
        }

        //页面加载完成，进行注入脚本，修改状态指示等
        private async void WebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            WebView2? webView = sender as WebView2;
            if (webView == null) return;

            // 检查导航是否成功完成
            if (e.IsSuccess)
            {
                var activeTabItem = AIAggregatorTabs.SelectedItem as TabItem;
                if (activeTabItem != null)
                {
                    UpdateTabStatus(activeTabItem, TabStatus.Loaded); //完成，显示绿点
                }

                string currentUrl = webView.Source?.ToString().ToLower() ?? "";

                // 不同的URL，注入不同的脚本
                string[] cssSelector = GetSelector.GetSelectorsByUrl(currentUrl);
                (string moduleScript, string initScript) = await Scripts.GetChatMessageNaviJS(cssSelector[1]);

                try
                {
                    // 所有页面通用注入 定位问题脚本
                    await Scripts.RunInjectScript(webView, moduleScript, "注入提问问题定位查询脚本");
                    await Scripts.RunInjectScript(webView, initScript, "初始化问题选择器");

                    // 特定网站的额外注入 自动发送问题
                    if (currentUrl.Contains("gemini.google.com") || currentUrl.Contains("gemini"))
                    {
                        //Gemini
                        await InjectGemini.InjectModule(webView);

                    }
                    else if (currentUrl.Contains("yuanbao.tencent.com"))
                    {
                        // 腾讯元宝
                        await InjectYuanbao.InjectJsToYuanbao(webView);
                        await InjectYuanbao.Hide_Yuanbao_Element(webView);


                    }
                    else if (currentUrl.Contains("chatgpt.com"))
                    {
                        // ChatGPT
                        await InjectChatgpt.InjectModule(webView);
                    }
                    else if (currentUrl.Contains("deepseek.com"))
                    {
                        //deepseek
                        await InjectDeepseek.InjectModule(webView);
                    }
                    else if (currentUrl.Contains("doubao.com"))
                    {
                        //豆包
                        await InjectCommno.InjectModule(webView);
                    }
                    else
                    {
                        //其他类似的AI对话
                        await InjectCommno.InjectModule(webView);
                    }

                    Debug.WriteLine("导航完成，脚本注入操作完成，请关注状态是否成功。");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"脚本注入失败: {ex.Message}");
                }
            }
        }
        private static Image LoadIcon(AiServiceConfig service)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            using (var stream = new FileStream(service.IconPath, FileMode.Open, FileAccess.Read))
            {
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }
            var image = new Image
            {
                Source = bitmapImage,
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            return image;
        }

        /// <summary>
        ///  WebView2 初始化webview，配置独立的用户数据文件夹
        /// </summary>
        private async Task InitializeWebViewWithEnvironment(
            WebView2 webView,
            string url,
            string serviceName,
            string dataFolderName,
            TabItem tabItem)
        {
            try
            {
                //  构造唯一的、隔离的用户数据文件夹路径
                string userDataPath = System.IO.Path.Combine(AppDataPath, dataFolderName);
                if (!Directory.Exists(userDataPath))
                {
                    Directory.CreateDirectory(userDataPath);
                }
                UpdateTabStatus(tabItem, TabStatus.NotLoaded);

                // 创建 EnvironmentOptions
                var creationOptions = new CoreWebView2EnvironmentOptions();
#if DEBUG
                // 强制禁用缓存 (仅用于调试，生产环境应移除)
                //creationOptions.AdditionalBrowserArguments = "--disable-application-cache";

                // 选项1: 只禁用磁盘缓存，保留内存缓存
                //creationOptions.AdditionalBrowserArguments = "--disk-cache-size=1";

                // 选项2: 禁用HTTP缓存，但保留其他缓存
                //creationOptions.AdditionalBrowserArguments = "--disable-http-cache";

                // 选项3: 设置极短的缓存时间
                //creationOptions.AdditionalBrowserArguments = "--aggressive-cache-discard";

                // 选项4: 禁用特定类型的缓存
                //creationOptions.AdditionalBrowserArguments = "--disable-features=VizDisplayCompositor";
#endif

                //  创建 CoreWebView2Environment,传递给 WebView2 控件
                CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, userDataPath, creationOptions);
                await webView.EnsureCoreWebView2Async(environment);
                // 订阅导航开始事件
                webView.CoreWebView2.NavigationStarting += WebView2_CoreWebView2_NavigationStarting;
                webView.CoreWebView2.FrameCreated += WebView2_CoreWebView2_FrameCreated;
                webView.CoreWebView2.NewWindowRequested += WebView2_CoreWebView2_NewWindowRequested;


#if DEBUG
                // 自动打开开发者模式
                //webView.CoreWebView2.OpenDevToolsWindow(); 
#endif



                if (webView.CoreWebView2 != null)
                {
                    webView.Source = new Uri(url);
                    Console.WriteLine($"{serviceName} 初始化成功，数据路径: {userDataPath}");
                    // 订阅导航开始事件（显示加载中）
                    webView.CoreWebView2.NavigationStarting += (sender, e) =>
                    {
                        UpdateTabStatus(tabItem, TabStatus.Loading);
                    };
                }
                Console.WriteLine($"{serviceName} 环境预初始化成功，数据路径: {userDataPath}");
                webView.NavigationCompleted += WebView2_NavigationCompleted;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化 {serviceName} 失败: {ex.Message}");
                UpdateTabStatus(tabItem, TabStatus.Error);
            }
        }

      
        #region  url open methods
        private void WebView2_CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            HandleNavigation(e.Uri, e);
        }

        private void WebView2_CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            if (HandleNavigation(e.Uri, e))
            {
                e.Handled = true;
            }
        }
        private void WebView2_CoreWebView2_FrameCreated(object? sender, CoreWebView2FrameCreatedEventArgs e)
        {
            CoreWebView2Frame frame = e.Frame;
            frame.NavigationStarting += Frame_NavigationStarting;
            frame.Destroyed += (s, args) =>
            {
                frame.NavigationStarting -= Frame_NavigationStarting;
            };
        }

        private void Frame_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            HandleNavigation(e.Uri, e);
        }

        private void WebView2_CoreWebView2_FrameNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            HandleNavigation(e.Uri, e);
        }

        private bool HandleNavigation(string navigationUrl, object eventArgs)
        {
            if (string.IsNullOrEmpty(navigationUrl) ||
                navigationUrl.StartsWith("about:") ||
                navigationUrl.StartsWith("edge:") ||
                !(navigationUrl.StartsWith("http://") || navigationUrl.StartsWith("https://")))
            {
                return false;
            }
            if(services==null || services.Count ==0)
            {
                return false;
            }
            foreach (var service in services)
            {
                if (Uri.TryCreate(navigationUrl, UriKind.Absolute, out Uri? navigatingUri) &&
                    Uri.TryCreate(service.Url, UriKind.Absolute, out Uri? allowedUri))
                {
                    if (navigatingUri.Host.Equals(allowedUri.Host, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            switch (eventArgs)
            {
                case CoreWebView2NavigationStartingEventArgs navStartingArgs:
                    navStartingArgs.Cancel = true; // 取消主框架导航
                    break;
                case CoreWebView2NewWindowRequestedEventArgs newWindowArgs:
                    break;
            }
            try
                {
                    Process.Start(new ProcessStartInfo(navigationUrl) { UseShellExecute = true });
                }
                catch { }
            return true;
        }
        #endregion

        public enum TabStatus
        {
            NotLoaded,   // 未加载 - 无指示点
            Loading,     // 加载中 - 黄色
            Loaded,      // 加载完成 - 绿色
            Error        // 加载错误 - 红色
        }

        // 更新页签状态的方法
        private void UpdateTabStatus(TabItem tabItem, TabStatus status)
        {
            // 确保在UI线程上执行
            Dispatcher.Invoke(() =>
            {
                if (_statusIndicators.TryGetValue(tabItem, out var statusIndicator) && statusIndicator != null)
                {
                    switch (status)
                    {
                        case TabStatus.NotLoaded:
                            statusIndicator.Visibility = Visibility.Collapsed;
                            break;
                        case TabStatus.Loading:
                            statusIndicator.Fill = Brushes.Yellow;
                            statusIndicator.Visibility = Visibility.Visible;
                            statusIndicator.ToolTip = "加载中...";
                            break;
                        case TabStatus.Loaded:
                            statusIndicator.Fill = Brushes.LimeGreen;
                            statusIndicator.Visibility = Visibility.Visible;
                            statusIndicator.ToolTip = "已加载";
                            break;
                        case TabStatus.Error:
                            statusIndicator.Fill = Brushes.Red;
                            statusIndicator.Visibility = Visibility.Visible;
                            statusIndicator.ToolTip = "加载失败";
                            break;
                    }
                }
            });
        }

        private void DenyDevToolInRelease(WebView2? webView)
        {
#if RELEASE
            try
            {


                // 禁止开发者工具，禁用默认右键菜单和浏览器快捷键
                var settings = webView.CoreWebView2.Settings;
                settings.AreDevToolsEnabled = false;
                //settings.AreDefaultContextMenusEnabled = false;
                settings.AreBrowserAcceleratorKeysEnabled = false;

            } catch{ }
#endif
        }
        private void TabControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                // 查找点击位置上的 TabItem
                var hitResult = VisualTreeHelper.HitTest(AIAggregatorTabs, e.GetPosition(AIAggregatorTabs));
                if (hitResult != null)
                {
                    TabItem? hitTabItem = FindParent<TabItem>(hitResult.VisualHit);
                    if (hitTabItem != null)
                    {
                        // 设置当前选中的页签为右键点击的页签
                        hitTabItem.IsSelected = true;
                        // 获取关联的 WebView2 实例
                        if (_tabWebViewMapping.TryGetValue(hitTabItem, out var webView2))
                        {
                            // 为菜单项动态绑定数据上下文
                            var contextMenu = (ContextMenu)this.FindResource("TabItemContextMenu");
                            contextMenu.DataContext = webView2;
                            contextMenu.PlacementTarget = hitTabItem;
                            contextMenu.IsOpen = true;
                        }
                        e.Handled = true;
                    }
                }
            }
        }

        // 在视觉树中查找父级 TabItem
        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }
      
        private TextBlock? FindTextBlockInStackPanel(Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is TextBlock textBlock)
                {
                    return textBlock;
                }
            }
            return null;
        }
        private async void UniversalSendButton_Click(object sender, RoutedEventArgs e)
        {
            _ = await GetAllQuestions();//显示当前会话的提问次数

            string inputText = UniversalInputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(inputText))
            {
                MessageBox.Show("请输入内容。", "提醒", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 检查如何发送
            if (checkBoxSendOnly.IsChecked == true)
            {
                //仅发送到当前激活的页签
                var (tabItem, targetWebView, tabName) = GetActiveTabInfo();
                if (targetWebView == null)
                {
                    MessageBox.Show("没有激活的页签", "提醒", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                await InjectTextToWebview(inputText, targetWebView);
            }
            else
            {
                // 遍历所有WebView2实例后全部发送
                foreach (var webViewPair in _tabWebViewMapping)
                {
                    WebView2 targetWebView = webViewPair.Value;

                    // 确保WebView2核心组件已加载
                    if (targetWebView?.CoreWebView2 == null)
                    {
                        continue;
                    }
                    await InjectTextToWebview(inputText, targetWebView);
                }
            }
            // 清空主输入框
            if (checkBoxClean.IsChecked == true) UniversalInputTextBox.Clear();

        }

        private static async Task InjectTextToWebview(string inputText, WebView2 targetWebView)
        {
            try
            {
                // 获取当前WebView2正在访问的URL，用于判断是哪个AI网站
                string currentUrl = targetWebView.Source?.ToString().ToLower() ?? "";
                if (currentUrl.Contains("gemini.google.com") || currentUrl.Contains("gemini"))
                {
                    //Gemini
                    await InjectGemini.SubmitQuestion(targetWebView, inputText);
                }
                else if (currentUrl.Contains("yuanbao.tencent.com"))
                {
                    // 腾讯元宝
                    await InjectYuanbao.SubmitToYuanbao(targetWebView, inputText);
                }
                else if (currentUrl.Contains("chatgpt.com"))
                {
                    // ChatGPT
                    await InjectChatgpt.SubmitQuestion(targetWebView, inputText);
                }
                else if (currentUrl.Contains("deepseek.com"))
                {
                    //deepseek
                    await InjectDeepseek.SubmitQuestion(targetWebView, inputText);
                }
                else if (currentUrl.Contains("doubao.com"))
                {
                    //豆包AI
                    await InjectCommno.SubmitQuestion(targetWebView, inputText);
                }
                else
                {
                    //其他任意网站
                    await InjectCommno.SubmitQuestion(targetWebView, inputText);
                }

            }
            catch (Exception ex)
            {
                // 处理单个页签注入失败的情况，避免影响其他页签
                Debug.WriteLine($"注入文本到页签失败: {ex.Message}");
            }
        }

        private async void ButtonNextMesg_Click(object sender, RoutedEventArgs e)
        {
            mesgCount.Content = await IndexButtonClick(Scripts.GetNextMesgageJS());
        }

        private async void ButtonPrevMesg_Click(object sender, RoutedEventArgs e)
        {
            mesgCount.Content = await IndexButtonClick(Scripts.GetPrevMesgageJS());
        }

        private async Task<string> IndexButtonClick(string command)
        {
            var (tabItem, targetWebView, tabName) = GetActiveTabInfo();
            if (targetWebView == null)
            {
                MessageBox.Show("没有激活的页签", "提醒", MessageBoxButton.OK, MessageBoxImage.Information);
                return "";
            }
            await Scripts.RunInjectScript(targetWebView, command, "定位到提问");
            string MesgCount = await targetWebView.CoreWebView2.ExecuteScriptAsync(Scripts.GetMesgCountJS());//返回问题定位和数量,如：1/21
            return MesgCount.Trim('\"');
        }

        // 获取当前激活的页签和WebView2实例
        private (TabItem? tabItem, WebView2? webView, string tabName) GetActiveTabInfo()
        {
            var activeTabItem = AIAggregatorTabs.SelectedItem as TabItem;
            if (activeTabItem == null)
            {
                return (null, null, "未找到激活页签");
            }
            if (!_tabWebViewMapping.TryGetValue(activeTabItem, out var activeWebView))
            {
                return (activeTabItem, null, "未找到对应的WebView2实例");
            }
            string tabName = GetTabIdentifier(activeTabItem);
            return (activeTabItem, activeWebView, tabName);
        }
        private string GetTabIdentifier(TabItem tabItem)
        {
            if (tabItem.Tag is AiServiceConfig serviceConfig)
            {
                return serviceConfig.Name;
            }

            if (tabItem.Header is StackPanel headerPanel)
            {
                TextBlock? textBlock = FindTextBlockInStackPanel(headerPanel);
                return textBlock?.Text ?? "未知页签";
            }

            if (tabItem.Header is string headerText)
            {
                return headerText;
            }

            return "未命名页签";
        }

       

        private bool GetWebViewObject(object sender, out WebView2? webView2)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var targetTabItem = contextMenu?.PlacementTarget as TabItem;
            webView2 = null;
            if (targetTabItem == null) return false;
            if (_tabWebViewMapping.TryGetValue(targetTabItem, out webView2))
            {
                string tabName = "未知页签"; // 默认值

                if (targetTabItem.Header is StackPanel headerPanel)
                {
                    // 在StackPanel中查找TextBlock控件
                    TextBlock? textBlock = FindTextBlockInStackPanel(headerPanel);
                    if (textBlock != null)
                    {
                        tabName = textBlock.Text;
                    }
                }
                else if (targetTabItem.Header is string headerString)
                {
                    tabName = headerString;
                }
                if (webView2 == null) return false;
            }

            return true;
        }

        private async void ButtonExportMesg_Click(object sender, RoutedEventArgs e)
        {
            (List<string> messages, string url) = await GetAllQuestions();
            string exportedFilePath = new MessageExporter().ExportMessagesToFile(messages, url, "webview_messages");
        }

        private async Task<(List<string> message, string url)> GetAllQuestions()
        {
            var (tabItem, targetWebView, tabName) = GetActiveTabInfo();
            if (targetWebView == null)
            {
                MessageBox.Show("没有激活的页签", "提醒", MessageBoxButton.OK, MessageBoxImage.Information);
                return (new List<string>(), string.Empty);
            }
            string jsonResult = await targetWebView.CoreWebView2.ExecuteScriptAsync(Scripts.GetAllUserMessagesJS());
            if (!string.IsNullOrEmpty(jsonResult) && jsonResult != "null")
            {
                string cleanJson = jsonResult.Trim('"');
                cleanJson = System.Text.RegularExpressions.Regex.Unescape(cleanJson); // 处理转义字符

                // Json 反序列化
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                List<string>? messages = JsonSerializer.Deserialize<List<string>>(cleanJson, options);
                if (messages == null)
                {
                    MessageBox.Show("未能解析消息内容。", "提醒", MessageBoxButton.OK, MessageBoxImage.Information);
                    return (new List<string>(), string.Empty);
                }
                mesgCount.Content = $"共{messages.Count}次提问";
                return (messages, targetWebView.Source.ToString());

            }
            return (new List<string>(), string.Empty);
        }

        #region 引入SetWindowPos函数，空值窗口置顶
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        // 窗体置顶常用的参数值
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // 置顶
        static readonly IntPtr HWND_NOTTOPMOST = new IntPtr(-2); // 置顶
        private const uint SWP_NOSIZE = 0x0001; // 不改变窗口大小
        private const uint SWP_NOMOVE = 0x0002; // 不改变窗口位置
        private const uint SWP_SHOWWINDOW = 0x0040; // 显示窗口
        private IntPtr GetWindowHandle()
        {
            // 获取窗口句柄
            return new WindowInteropHelper(this).Handle;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            TopMostToggle.IsChecked = false;
        }

        // 窗体置顶
        private void TopMostToggle_Click(object sender, RoutedEventArgs e)
        {
            if (TopMostToggle.IsChecked == true)
            {
                IntPtr handle = GetWindowHandle();
                SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
            else
            {
                IntPtr handle = GetWindowHandle();
                SetWindowPos(handle, HWND_NOTTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }

        }
        #endregion

        private async void ButtonBottomMesg_Click(object sender, RoutedEventArgs e)
        {
            mesgCount.Content = await IndexButtonClick(Scripts.GetBottomMesgageJS());
        }

        private async void ButtonTopMesg_Click(object sender, RoutedEventArgs e)
        {
            mesgCount.Content = await IndexButtonClick(Scripts.GetTopMesgageJS());
        }
    }
}