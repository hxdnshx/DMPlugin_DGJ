using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace DMPlugin_DGJ
{
    internal static partial class Center
    {

        internal static readonly string Plg_name = "点歌姬";
        internal static readonly string Plg_auth = "宅急送队长";
        internal static readonly string Plg_ver = "2.0.1";
        internal static readonly string Plg_desc = "用弹幕来播放歌曲吧";
        internal static readonly string Plg_cont = "15253直播间";
        /// <summary>
        /// 管理窗体
        /// </summary>
        internal static MainWindow Mainw;

        /// <summary>
        /// 歌曲搜索引擎模块列表
        /// </summary>
        internal static ObservableCollection<SongsSearchModule> SearchModules = new ObservableCollection<SongsSearchModule>();

        /// <summary>
        /// 当前使用的搜索模块
        /// </summary>
        internal static SongsSearchModule CurrentModuleA = null;

        /// <summary>
        /// 当前使用的备用模块
        /// </summary>
        internal static SongsSearchModule CurrentModuleB = null;

        /// <summary>
        /// 储存的歌曲列表
        /// </summary>
        internal static ObservableCollection<SongItem> Songs = new ObservableCollection<SongItem>();

        /// <summary>
        /// 黑名单列表
        /// </summary>
        internal static ObservableCollection<BlackInfoItem> BlackList = new ObservableCollection<BlackInfoItem>();

        /// <summary>
        /// 版本检查
        /// </summary>
        internal static VersionChecker versionChecker;

        /// <summary>
        /// 配置文件夹路径
        /// </summary>
        internal static readonly string ConfigPath = Path.Combine(AssemblyDirectory, "点歌姬");


        internal static void AddSong(SongItem song)
        {
            PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                Songs.Add(song);
            }));
        }

        internal static void RemoveSong(SongItem song)
        {
            PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                Songs.Remove(song);
            }));
        }

        internal static void MoveSong(int v)
        {
            PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                if (Songs.Count < v || v < 3) // 如果歌曲数量不够 或 已经是前两位的歌曲
                    return;
                Songs.Move(v - 1, 1);
            }));
        }

        internal static SongInfo SearchSong(string keyword)
        {
            if (CurrentModuleA == null)
            { Logg("没有歌曲搜索模块！", true); return null; } // 默认搜索模块没设置

            string str_encoded = System.Web.HttpUtility.UrlEncode(keyword);

            SongInfo i = CurrentModuleA.SafeSearch(str_encoded);
            if (i == null && CurrentModuleB != null)
            { i = CurrentModuleB.SafeSearch(str_encoded); }

            // if (i != null && i.User != username)
            // {
            //     Logg($"搜索模块“{i.ModuleName}”因传递数据错误已被禁用", true);
            //     PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            //     {
            //         SearchModules.Remove(i.Module);
            //         if (Mainw.Combo_SearchModuleA.SelectedIndex == -1) // 防null
            //         { Mainw.Combo_SearchModuleA.SelectedIndex = 0; }
            //     }));
            //     return null;
            // }

            return i;
        }

        /// <summary>
        /// 检查是否在黑名单中
        /// </summary>
        /// <param name="i">歌曲信息</param>
        /// <returns>是否在黑名单中</returns>
        internal static bool IsInBlackList(SongInfo i)
        {
            string singerstr = string.Join("", i.Singers);

            foreach (BlackInfoItem b in BlackList)
            {
                if (b.BLK_Enable)
                {
                    switch (b.BLK_Type)
                    {
                        case BlackInfoType.歌名:
                            if (i.Name.IndexOf(b.BLK_Text, StringComparison.CurrentCultureIgnoreCase) > -1)
                            { return true; }
                            break;
                        case BlackInfoType.歌手:
                            if (singerstr.IndexOf(b.BLK_Text, StringComparison.CurrentCultureIgnoreCase) > -1)
                            { return true; }
                            break;
                        case BlackInfoType.歌曲ID:
                            if (i.Id == b.BLK_Text)
                            { return true; }
                            break;
                        default:
                            break;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 显示文本到日志/弹幕侧边栏
        /// 方便插件其他部分调用
        /// </summary>
        /// <param name="text">显示的文本</param>
        /// <param name="danmu">是否需要显示到侧边栏</param>
        internal static void Logg(string text, bool danmu = false, bool logfile = false)
        {
            PluginMain.self.Log(text);
            if (danmu)
            {
                PluginMain.self.AddDM(text);
            }
            if (logfile)
            {
                OutputControl.AddLog(text);
            }
        }

        /// <summary>
        /// 伪造一个弹幕侧边栏信息
        /// </summary>
        /// <param name="name">显示的名字</param>
        /// <param name="text">显示的文字</param>
        /// <param name="red">名字是否显示为红色</param>
        /// <param name="fullscreen">是否显示到滚动弹幕栏</param>
        internal static void fakeDM(string name, string text, bool red = true, bool fullscreen = false)
        {
            PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                dynamic mw = System.Windows.Application.Current.MainWindow;
                mw.AddDMText(name, text, red, fullscreen);
            }));
        }

        internal static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
