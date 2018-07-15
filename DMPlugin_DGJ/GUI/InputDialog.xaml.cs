using System;
using System.Windows;

namespace DMPlugin_DGJ
{
    /// <summary>
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    internal partial class InputDialog : Window
    {
        /// <summary>
        /// 创建一个弹出窗
        /// </summary>
        /// <param name="Que">问题</param>
        /// <param name="defaultAns">默认填写的回答</param>
        /// <param name="title">窗口标题</param>
        public InputDialog(string Que, string defaultAns = "", string title = "")
        {
            InitializeComponent();
            this.Topmost = true;
            this.lblQuestion.Content = Que;
            this.txtAnswer.Text = defaultAns;
            this.Title = title;
            this.Focus();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtAnswer.SelectAll();
            txtAnswer.Focus();
        }

        public string Answer
        {
            get { return txtAnswer.Text; }
        }
    }
}
