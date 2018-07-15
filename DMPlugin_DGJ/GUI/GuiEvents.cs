using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DMPlugin_DGJ
{
    internal partial class MainWindow : Window
    {

        #region -- 搜索模块相关 --

        /// <summary>
        /// 主搜索模块修改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModuleAChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox b = (ComboBox)sender;
            Center.CurrentModuleA = b.SelectedItem as SongsSearchModule;
        }

        /// <summary>
        /// 备用搜索模块修改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModuleBChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox b = (ComboBox)sender;
            Center.CurrentModuleB = b.SelectedItem as SongsSearchModule;
        }

        /// <summary>
        /// 删除备用搜索模块事件
        /// </summary>                        
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModuleB_RemClick(object sender, RoutedEventArgs e)
        {
            Combo_SearchModuleB.SelectedItem = null;
            Center.CurrentModuleB = null;
        }

        /// <summary>
        /// 变更选择模块判定是否显示设置选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModuleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SongsSearchModule m = ((DataGrid)sender).SelectedItem as SongsSearchModule;
            if (m == null)
            { Module_Setting.Visibility = Visibility.Collapsed; }
            else
            { Module_Setting.Visibility = m.NeedSettings ? Visibility.Visible : Visibility.Collapsed; }

        }

        /// <summary>
        /// 设置搜索模块事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModuleSettingClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var items = (DataGrid)contextMenu.PlacementTarget;
            if (items.SelectedCells.Count == 0)
            { return; }
            var m = items.SelectedCells[0].Item as SongsSearchModule;
            if (m != null)
            { m.SafeSetting(); }
        }

        /// <summary>
        /// 添加歌曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSong(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var items = (DataGrid)contextMenu.PlacementTarget;
            if (items.SelectedCells.Count == 0)
                return;
            var item = items.SelectedCells[0].Item as SongsSearchModule;
            if (item == null)
            { return; }

            InputDialog i = new InputDialog("歌曲名字：", "", "添加歌曲");
            if (i.ShowDialog() == true && i.Answer != string.Empty)
            {
                SongItem song = item.SafeSearch(Config.MASTER_NAME, System.Web.HttpUtility.UrlEncode(i.Answer), Config.needLyric);
                if (song != null)
                {
                    Center.AddSong(song);
                    Center.Logg("手动添加歌曲成功：" + song.SongName);
                }
            }
            i = null;
        }
        #endregion

        #region -- 综合界面相关 --

        /// <summary>
        /// 切歌，按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextSong_Click(object sender, RoutedEventArgs e)
        {
            PlayControl.Next();
        }

        /// <summary>
        /// 播放或暂停，按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayControl_PausePlay(object sender, RoutedEventArgs e)
        {
            if (PlayControl.isPause)
            { PlayControl.Play(); }
            else
            { PlayControl.Pause(); }
        }

        /// <summary>
        /// 更新播放设备列表，按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshPlayDevice(object sender, RoutedEventArgs e)
        {
            Center.LoadPlayDevices();
        }

        /// <summary>
        /// 切换播放设备事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayDeviceChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox c = (ComboBox)sender;
            switch (Config.OutputType)
            {
                default:
                case 0:
                    if (c.SelectedIndex != -1)
                    { Config.DeviceId = c.SelectedIndex; }
                    return;
                case 1:
                    if (c.SelectedItem != null)
                    { Config.DirectSoundDevice = (c.SelectedItem as NAudio.Wave.DirectSoundDeviceInfo).Guid; }
                    return;
            }
        }

        /// <summary>
        /// 修改播放类型事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutputTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox c = (ComboBox)sender;
            Config.OutputType = c.SelectedIndex;
            Center.LoadPlayDevices();
        }

        /// <summary>
        /// 修改音量事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vol_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PlayControl.SetVol((int)((Slider)sender).Value);
        }

        /// <summary>
        /// 是否需要搜索歌词
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void needLyricChange(object sender, RoutedEventArgs e)
        {
            Config.needLyric = (bool)((CheckBox)sender).IsChecked;
        }

        /// <summary>
        /// 是否显示歌词到侧边栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void needLyricChangeDisplay(object sender, RoutedEventArgs e)
        {
            Config.DisplayLyric = (bool)((CheckBox)sender).IsChecked;
        }

        /// <summary>
        /// 修改播放位置事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayTimeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = (Slider)sender;
            if (s.IsFocused && s.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed)
            { PlayControl.SetTime(s.Value); }
        }

        #endregion

        #region -- 歌曲列表相关 --
        /// <summary>
        /// 删除歌曲事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var items = (DataGrid)contextMenu.PlacementTarget;
            if (items.SelectedCells.Count == 0)
                return;
            var item = items.SelectedCells[0].Item as SongItem;
            if (item == null)
                return;
            switch (item.Status)
            {
                case SongItem.SongStatus.WaitingDownload:
                    Center.RemoveSong(item);
                    return;
                case SongItem.SongStatus.Downloading:
                    DownloadControl.CancelDownload();
                    return;
                default:
                case SongItem.SongStatus.WaitingPlay:
                    if (item.FilePath != null && item.FilePath != "")
                        new System.Threading.Thread((object o) => { try { System.IO.File.Delete(o.ToString()); } catch (Exception) { } })
                        { IsBackground = true, Name = "切歌后删除文件" }.Start(item.FilePath);
                    Center.RemoveSong(item);
                    return;
                case SongItem.SongStatus.Playing:
                    PlayControl.Next();
                    return;
            }
        }

        /// <summary>
        /// 优先歌曲事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveSoundOrder_Click(object sender, RoutedEventArgs e)
        {
            var items = (((MenuItem)sender).Parent as ContextMenu).PlacementTarget as DataGrid;
            if (items.SelectedIndex != -1)
            {
                Center.MoveSong(items.SelectedIndex + 1);
            }
        }
        #endregion

        #region -- 黑 名 单相关 --
        /// <summary>
        /// 添加一条黑名单信息事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BLK_AddBlackInfo(object sender, RoutedEventArgs e)
        {
            Center.BlackList.Add(new BlackInfoItem());
        }

        /// <summary>
        /// 删除选中的黑名单信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BLK_Delete(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var items = (DataGrid)contextMenu.PlacementTarget;
            if (items.SelectedCells.Count == 0)
            { return; }
            var item = items.SelectedCells[0].Item as BlackInfoItem;
            Center.BlackList.Remove(item);
        }

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BLK_Load(object sender, RoutedEventArgs e)
        {
            Config.BLK_Load();
        }

        /// <summary>
        /// 从文件读取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BLK_Save(object sender, RoutedEventArgs e)
        {
            Config.BLK_Save();
        }

        #endregion

        #region -- 文本输出相关 --

        /// <summary>
        /// 列表前文字修改
        /// </summary>                            
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Output_Before_Change(object sender, TextChangedEventArgs e)
        { Config.OPTBefore = ((TextBox)sender).Text; }

        /// <summary>
        /// 单行歌曲模版修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Output_Line_Change(object sender, TextChangedEventArgs e)
        { Config.OPTLine = ((TextBox)sender).Text; }

        /// <summary>
        /// 显示歌曲数量修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Output_LineNum_Change(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            int v;
            if (int.TryParse(t.Text, out v))
            { Config.OPTLineNum = v; }
            else
            { t.Text = Config.OPTLineNum.ToString(); t.SelectionStart = t.Text.Length; }
        }

        /// <summary>
        /// 列表后文字修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Output_After_Change(object sender, TextChangedEventArgs e)
        { Config.OPTAfter = ((TextBox)sender).Text; }

        /// <summary>
        /// 空列表显示文字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Set_OEL_Change(object sender, TextChangedEventArgs e)
        { Config.OutputEmptyList = ((TextBox)sender).Text; }

        /// <summary>
        /// 其他播放器歌名显示前缀
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Set_ONP_Change(object sender, TextChangedEventArgs e)
        { Config.OutputOtherNamePrefix = ((TextBox)sender).Text; }

        #endregion

        #region -- 设置界面相关 --

        /// <summary>
        /// Max Songs Count
        /// 允许最大歌曲数量修改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Set_MSC_Change(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            int v;
            if (int.TryParse(t.Text, out v))
            { Config.maxSongCount = v; }
            else
            { t.Text = Config.maxSongCount.ToString(); t.SelectionStart = t.Text.Length; }
        }

        /// <summary>
        /// 普通用户是否能多点歌曲设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Set_CMS_Change(object sender, RoutedEventArgs e)
        { Config.CanMultiSong = (bool)((ToggleButton)sender).IsChecked; }

        /// <summary>
        /// 只允许房管点歌
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Set_AO_Change(object sender, RoutedEventArgs e)
        { Config.AdminOnly = (bool)((ToggleButton)sender).IsChecked; }

        /// <summary>
        /// 控制其他播放器的播放暂停设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Set_COP_Change(object sender, RoutedEventArgs e)
        { Config.ControlOtherPlayer = (bool)((ToggleButton)sender).IsChecked; }

        /// <summary>
        /// 一首歌最多播放秒数设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Set_MPS_Change(object sender, RoutedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            if (int.TryParse(t.Text, out int v) && v >= 0)
            { Config.MaxPlaySecond = v; }
            else
            { t.Text = Config.MaxPlaySecond.ToString(); t.SelectionStart = t.Text.Length; }
        }

        private void Set_VN_Change(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            if (int.TryParse(t.Text, out int v) && v >= 0)
            { Config.VoteNext = v; }
            else
            { t.Text = Config.VoteNext.ToString(); t.SelectionStart = t.Text.Length; }
        }

        private void Set_LST_Change(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            if (int.TryParse(t.Text, out int v) && v > 0)
            { Config.LogStayTime = v; }
            else
            { t.Text = Config.LogStayTime.ToString(); t.SelectionStart = t.Text.Length; }
        }

        /// <summary>
        /// 能否弹幕控制设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Set_DC_Change(object sender, RoutedEventArgs e)
        { Config.DanmakuControl = (bool)((ToggleButton)sender).IsChecked; }

        /// <summary>
        /// 只显示一行歌词
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Set_OL_Change(object sender, RoutedEventArgs e)
        { Config.OneLyric = (bool)((ToggleButton)sender).IsChecked; }


        #endregion

    }
}
