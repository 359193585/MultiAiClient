using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string AppDataPath =
        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyMultiAIClientData");

        //private readonly string AppDataPath =  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyMultiAIClientData");

        private Dictionary<TabItem, WebView2> _tabWebViewMapping = [];
        // 状态指示器字典
        private Dictionary<TabItem, Ellipse> _statusIndicators = new Dictionary<TabItem, Ellipse>();
        public MainWindow()
        {
            InitializeComponent();
            InitializeAllTabs();
            double screenHeight = SystemParameters.WorkArea.Height;
            this.Height = screenHeight;
            this.MaxHeight = screenHeight;
            this.Top = 0;
            this.Left = 0;
        }
        private async void InitializeAllTabs()
        {
            string configPath = "config.json";
            if (!File.Exists(configPath))
            {
                MessageBox.Show("配置文件 config.json 未找到。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string jsonString = await File.ReadAllTextAsync(configPath);
                List<AiServiceConfig>? services = JsonSerializer.Deserialize<List<AiServiceConfig>>(jsonString);

                if (services == null || services.Count == 0)
                {
                    MessageBox.Show("配置文件中未找到 AI 服务配置。", "警告");
                    return;
                }

                // 创建 TabItem 和 WebView2 控件
                foreach (var service in services)
                {
                    // 创建 WebView2 控件实例
                    var webView = new WebView2()
                    {
                        Name = $"{service.Name.Replace(" ", "")}WebView" // 动态命名
                    };

                    // 创建 TabItem
                    // 创建 StackPanel 容器（用于水平排列图片和文字）
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

                    var tabItem = new TabItem()
                    {
                        Header = stackPanel,
                        Content = webView,
                        Tag = service
                    };
                    tabItem.ContextMenu = (ContextMenu)this.FindResource("TabItemContextMenu");
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
                    // 刷新页面（可选）
                    //targetWebView.Reload();
                    UpdateTabStatus(selectedTabItem, TabStatus.Loaded);

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
        ///  WebView2 初始化和导航方法，带有独立的用户数据文件夹配置
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

                //  创建 CoreWebView2Environment,传递给 WebView2 控件
                CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, userDataPath);
                await webView.EnsureCoreWebView2Async(environment);
#if DEBUG
                webView.CoreWebView2.OpenDevToolsWindow();  //开发者模式
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
                if (_statusIndicators.TryGetValue(tabItem, out Ellipse statusIndicator))
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

        //页面加载完成，进行注入脚本，修改状态指示
        private async void WebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            WebView2? webView2 = sender as WebView2;
            if (webView2 == null) return;

            // 检查导航是否成功完成
            if (e.IsSuccess)
            {
                var activeTabItem = AIAggregatorTabs.SelectedItem as TabItem;
                UpdateTabStatus(activeTabItem, TabStatus.Loaded); //完成，显示绿点

                // 根据当前URL决定要注入的脚本
                string[] cssSelector = GetSelector.GetSelectorsByUrl(webView2.Source.ToString().ToLower() ?? "");
#if DEBUG
                string js = InjectTextIntoInput.ChatMessageNavi_Debug(cssSelector[1]);
#endif
#if !DEBUG
                string js = InjectTextIntoInput.ChatMessageNavi_Debug_Minified(cssSelector[1]);
#endif
                try
                {
                    await webView2.CoreWebView2.ExecuteScriptAsync(js);//注入问题查询脚本  所有页面都注入，但js不一样，会随着选择器进行调整

                    string currentUrl = webView2.Source?.ToString().ToLower() ?? "";
                    if (currentUrl.Contains("gemini.google.com") || currentUrl.Contains("gemini"))
                    {
                        //Gemini
                    }
                    else if (currentUrl.Contains("yuanbao.tencent.com"))
                    {
                        // 腾讯元宝
                        await InjectTextIntoInput.InjectToYuanbao(webView2);
                    }
                    else if (currentUrl.Contains("chatgpt.com"))
                    {
                        // ChatGPT
                    }
                    else if (currentUrl.Contains("deepseek.com"))
                    {
                        //deepseek
                    }



                    Debug.WriteLine("导航完成，所有脚本注入成功。");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"脚本注入失败: {ex.Message}");
                }
            }
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
        private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WebView2? webView2;
            GetWebViewObject(sender, out webView2);
            if (webView2==null)
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
                MessageBox.Show("请输入内容。");
                return;
            }

            // 检查是否仅发送到当前激活的页签
            if ((bool)checkBoxSendOnly.IsChecked)
            {
                var (tabItem, targetWebView, tabName) = GetActiveTabInfo();
                if (targetWebView == null)
                {
                    MessageBox.Show("没有激活的页签");
                    return;
                }
                await InjectTextToWebview(inputText, targetWebView);
            }
            else
            {
                // 遍历所有WebView2实例
                foreach (var webViewPair in _tabWebViewMapping)
                {
                    WebView2 targetWebView = webViewPair.Value;

                    // 确保WebView2核心组件已加载
                    if (targetWebView?.CoreWebView2 == null)
                    {
                        continue; // 跳过未初始化的WebView2
                    }
                    await InjectTextToWebview(inputText, targetWebView);
                }
            }
            // 清空主输入框
            if((bool)checkBoxClean.IsChecked) UniversalInputTextBox.Clear();
            
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
                    await InjectTextIntoInput.InjectIntoGeminiWithRetry(targetWebView, inputText);
                }
                else if (currentUrl.Contains("yuanbao.tencent.com"))
                {
                    // 腾讯元宝
                    //await InjectTextIntoInput.InjectAndSubmitToYuanbao(targetWebView, inputText);
                    await InjectTextIntoInput.SubmitToYuanbao(targetWebView, inputText);
                }
                else if (currentUrl.Contains("chatgpt.com"))
                {
                    // ChatGPT
                    await InjectTextIntoInput.InjectIntoChatgptWithRetry(targetWebView, inputText);
                }
                else if (currentUrl.Contains("deepseek.com"))
                {
                    //deepseek
                    await InjectTextIntoInput.InjectIntoDeepseekWithRetry(targetWebView, inputText);
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
#if DEBUG
            await IndexButtonClick("window.__my_injected__.chatNavigator.goToNextUserMessage()");
            mesgCount.Content = await GetIndexAndCount("window.__my_injected__.chatNavigator.messageCount()");
#endif
#if !DEBUG
            await IndexButtonClick("window.__my_injected__.cn.n()");
            mesgCount.Content = await GetIndexAndCount("window.__my_injected__.cn.c()");
#endif
        }

        private async void ButtonPrevMesg_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            await IndexButtonClick("window.__my_injected__.chatNavigator.goToPrevUserMessage()");
            mesgCount.Content = await GetIndexAndCount("window.__my_injected__.chatNavigator.messageCount()");
            
#endif
#if !DEBUG
            await IndexButtonClick("window.__my_injected__.cn.p()");
            mesgCount.Content = await GetIndexAndCount("window.__my_injected__.cn.c()");

#endif
        }

        private async Task<bool> IndexButtonClick(string command)
        {
            var (tabItem, targetWebView, tabName) = GetActiveTabInfo();
            if (targetWebView == null)
            {
                MessageBox.Show("没有激活的页签");
                return false;
            }
            await InjectTextIntoInput.InjectRunIndexQuestion(targetWebView, command);
            return true;
        }
        private async Task<string> GetIndexAndCount(string command)
        {
            var (tabItem, targetWebView, tabName) = GetActiveTabInfo();
            if (targetWebView == null)
            {
                MessageBox.Show("没有激活的页签");
                return "0/0";
            }
            var result  = await targetWebView.CoreWebView2.ExecuteScriptAsync(command);
            if (result != null) return result.ToString().Trim('\"'); ;
            return " ";
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

        private void OpenInBrowserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WebView2? webView2;
            GetWebViewObject(sender, out webView2);
            if (webView2==null)
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
                MessageBox.Show($"在浏览器中打开失败: {ex.Message}");
            }

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
            (List<string> messages,string url) = await GetAllQuestions();
            string exportedFilePath = new MessageExporter().ExportMessagesToFile(messages,url, "webview_messages");
        }

        private async Task<(List<string> message ,string url)>GetAllQuestions()
        {
            var (tabItem, targetWebView, tabName) = GetActiveTabInfo();
            if (targetWebView == null)
            {
                MessageBox.Show("没有激活的页签");
                return (new List<string>(), string.Empty);
            }
#if DEBUG
            string jsonResult = await targetWebView.CoreWebView2.ExecuteScriptAsync("window.__my_injected__.chatNavigator.getMesgStr()");
#endif
#if !DEBUG
            string jsonResult = await targetWebView.CoreWebView2.ExecuteScriptAsync("window.__my_injected__.cn.g()");
#endif
            if (!string.IsNullOrEmpty(jsonResult) && jsonResult != "null")
            {
                string cleanJson = jsonResult.Trim('"');
                cleanJson = System.Text.RegularExpressions.Regex.Unescape(cleanJson); // 处理转义字符

                // 使用 System.Text.Json 反序列化
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                List<string>? messages = JsonSerializer.Deserialize<List<string>>(cleanJson, options);
                if (messages == null)
                {
                    MessageBox.Show("未能解析消息内容。");
                    return (new List<string>(), string.Empty);
                }
                mesgCount.Content = $"共{messages.Count}次提问";
                return (messages, targetWebView.Source.ToString());

            }
            return (new List<string>(), string.Empty);
        }
    }
}