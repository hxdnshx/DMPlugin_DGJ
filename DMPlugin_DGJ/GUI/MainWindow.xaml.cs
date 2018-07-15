using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DMPlugin_DGJ
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    internal partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        #region
        private void NumFilter(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(String)) || !IsTextAllowed((String)e.DataObject.GetData(typeof(String))))
            {
                e.CancelCommand();
            }
        }
        private void NumFilter(object sender, TextCompositionEventArgs e) => e.Handled = !IsTextAllowed(e.Text);
        private static readonly Regex IsTextAllowedRegex = new Regex("[^0-9.-]+", RegexOptions.Compiled);
        private static bool IsTextAllowed(string text) => !IsTextAllowedRegex.IsMatch(text);
        #endregion

        /// <summary>
        /// 设置播放进度显示
        /// </summary>
        /// <param name="now">当前时间</param>
        /// <param name="total">总长度</param>
        internal void setDisplayTime(TimeSpan now, TimeSpan total)
        {
            PluginMain.self.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                PlayTime.Value = total.Ticks != 0 ? now.Ticks * 1000 / total.Ticks : 0;
                PlayTimeText.Text = string.Format(@"{0:%m}:{0:ss}/{1:%m}:{1:ss}", now, total);
            }));
        }

        /// <summary>
        /// 设置下载状态
        /// </summary>
        /// <param name="text">要设置的文本</param>
        internal void setDownloadStatus(string text)
        { PluginMain.self.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { DownloadStatus.Text = "下载状态：" + text; })); }

        /// <summary>
        /// 设置播放暂停按钮图标
        /// </summary>
        /// <param name="isPlaying">当前是否正在播放歌曲(显示暂停图标)</param>
        internal void setPlayPauseButtonIcon(bool isPlaying)
        {
            if (CheckAccess())
            {
                string resource = isPlaying ? "Canvas_Pause" : "Canvas_Play";
                var obj = (UIElement)this.FindResource(resource);
                PlayPauseButtonView.Child = obj;
            }
            else
            {
                PluginMain.self.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { setPlayPauseButtonIcon(isPlaying); }));
            }
        }

    }
}
