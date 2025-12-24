/*****************************************
 *
 * 项目名称： MultiAIClient  
 * 文 件 名:  TabControlDragDropBehavior.cs
 * 命名空间： MultiAIClient
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/12/24 13:39
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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

namespace MultiAIClient
{
    public static class TabControlDragDropBehavior
    {
        public static string ConfigFileName = "";

        public static readonly DependencyProperty IsDragDropEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsDragDropEnabled",
                typeof(bool),
                typeof(TabControlDragDropBehavior),
                new PropertyMetadata(false, OnIsDragDropEnabledChanged));

        public static bool GetIsDragDropEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragDropEnabledProperty);
        }

        public static void SetIsDragDropEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragDropEnabledProperty, value);
        }

        private static void OnIsDragDropEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabControl tabControl)
            {
                if ((bool)e.NewValue)
                {
                    tabControl.PreviewMouseMove += TabControl_PreviewMouseMove;
                    tabControl.PreviewMouseLeftButtonDown += TabControl_PreviewMouseLeftButtonDown;
                    tabControl.Drop += OnDrop;
                    tabControl.AllowDrop = true;
                }
                else
                {
                    tabControl.PreviewMouseMove -= TabControl_PreviewMouseMove;
                    tabControl.PreviewMouseLeftButtonDown -= TabControl_PreviewMouseLeftButtonDown;
                    tabControl.Drop -= OnDrop;
                    tabControl.AllowDrop = false;
                }
            }
        }

        private static Point _dragStartPoint;
        private static TabItem? _draggedTab;

        private static void TabControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement source && source.FindAncestor<TabItem>() is TabItem tabItem)
            {
                _dragStartPoint = e.GetPosition(null);
                _draggedTab = tabItem;
            }

        }

        private static void TabControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedTab != null)
            {
                Point currentPosition = e.GetPosition(null);
                if (Math.Abs(currentPosition.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPosition.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DragDrop.DoDragDrop(_draggedTab, _draggedTab, DragDropEffects.Move);
                }
            }
        }
        private static void OnDrop(object sender, DragEventArgs e)
        {
            var tabControl = (TabControl)sender;
            var targetTabItem = e.OriginalSource as FrameworkElement;

            while (targetTabItem != null && !(targetTabItem is TabItem))
            {
                targetTabItem = VisualTreeHelper.GetParent(targetTabItem) as FrameworkElement;
            }

            if (targetTabItem is TabItem targetItem && _draggedTab is TabItem sourceItem)
            {
                int sourceIndex = tabControl.Items.IndexOf(sourceItem);
                int targetIndex = tabControl.Items.IndexOf(targetItem);

                if (sourceIndex != targetIndex && sourceIndex >= 0 && targetIndex >= 0)
                {
                    // 移动项
                    tabControl.Items.RemoveAt(sourceIndex);
                    tabControl.Items.Insert(targetIndex, sourceItem);
                    sourceItem.IsSelected = true;

                    System.Diagnostics.Debug.WriteLine($"[OnDrop] 移动 TabItem: {sourceIndex} -> {targetIndex}");
                    System.Diagnostics.Debug.WriteLine($"  移动后 Content 类型: {sourceItem.Content?.GetType().Name}");

                    // 保存新顺序到JSON
                    SaveTabOrder(tabControl);
                }
            }

            _draggedTab = null;
        }
        public static void SaveTabOrder(TabControl tabControl)
        {
            if(String.IsNullOrEmpty(ConfigFileName))
            {
                return;
            }

            try
            {
                var newConfig = new List<AiServiceConfig>();

                for (int i = 0; i < tabControl.Items.Count; i++)
                {
                    if (tabControl.Items[i] is TabItem item)
                    {
                        AiServiceConfig? aiService = item.Tag as AiServiceConfig;
                        if(aiService == null)
                        {
                            continue;
                        }
                        newConfig.Add(new AiServiceConfig
                        {
                            Name = aiService.Name,
                            Url = aiService.Url,
                            DataFolderName = aiService.DataFolderName,
                            IconPath = aiService.IconPath,
                            Order = i
                        });
                    }
                }

                // 保存到JSON文件
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string jsonString = JsonSerializer.Serialize(newConfig, options);
                File.WriteAllText(ConfigFileName, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"配置文件保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
    }
    public static class VisualTreeHelperExtensions
    {
        public static T? FindAncestor<T>(this DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor) return ancestor;
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
