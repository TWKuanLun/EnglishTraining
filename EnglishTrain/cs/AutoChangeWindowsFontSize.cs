using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace EnglishTrain.cs
{
    /// <summary>自動改變WPF視窗字體大小的Class。</summary>
    class AutoChangeWindowsFontSize
    {
        private double UserResolutionWidth, defaultResolutionWidth, scale, preWindowWidth;
        Window window;
        /// <summary>取得所有相依物件。</summary>
        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        var child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }
        public AutoChangeWindowsFontSize(Window window, double defaultResolutionWidth)
        {
            UserResolutionWidth = SystemParameters.PrimaryScreenWidth;
            this.defaultResolutionWidth = defaultResolutionWidth;
            preWindowWidth = window.Width;
            this.window = window;
            this.window.SizeChanged += Window_SizeChanged;
            scale = UserResolutionWidth / defaultResolutionWidth;
            var str = new StringBuilder();
            foreach (var c in FindLogicalChildren<Control>(window))
            {
                c.FontSize = c.FontSize * scale;
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double scaleWidth = window.ActualWidth / preWindowWidth;
            preWindowWidth = window.ActualWidth;
            foreach (var c in FindLogicalChildren<Control>(window))
            {
                c.FontSize = c.FontSize * scaleWidth;
            }
        }
    }
}
