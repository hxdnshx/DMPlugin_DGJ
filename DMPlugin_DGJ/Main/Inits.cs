using NAudio.Wave;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DMPlugin_DGJ
{
    internal static partial class Center
    {
        /**
         * 插件初始化顺序：
         * 
         * 初始化插件信息 - InitPluginInfo
         * 加载类库 - InitLib
         * 初始化播放器 - InitPlayer
         * 初始化窗口 - InitWindow
         * 
         * 下载模块 - InitDowndloder
         * 初始化模块列表 - InitModule - 被配置文件依赖
         * 窗体数据绑定 - InitWindowDataBinding - GUI绑定搜索模块
         * 加载配置文件 - Config.Load() - 依赖模块列表
         * 输出文本数据 - InitOutput - 依赖配置文件
         * 弹幕姬事件 - InitEventBind
         * 版本检查 - InitAbort
         * 
         * */

        internal static void Init()
        {
            InitPluginInfo();

            try
            { Directory.CreateDirectory(Center.ConfigPath); }
            catch (Exception) { }

            try
            { InitLib(); }
            catch (Exception ex)
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                using (StreamWriter outfile = new StreamWriter(path + @"\点歌姬类库加载错误报告.txt"))
                {
                    outfile.WriteLine($"加载库 插件版本{Plg_ver} 本地时间：" + DateTime.Now.ToString());
                    outfile.Write(ex.ToString());
                }
                new Thread(() =>
                {
                    System.Windows.MessageBox.Show("点歌姬类库加载出错！\r\n安装一下.NET 4.6.2 再试试？");
                })
                { Name = "点歌姬类库加载错误弹窗", IsBackground = true }
                .Start();
                throw; // 让弹幕姬不加载插件
            }

            try
            {
                InitPlayer();
                InitWindow();
            }
            catch (Exception ex)
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                using (StreamWriter outfile = new StreamWriter(path + @"\点歌姬同步初始化错误报告.txt"))
                {
                    outfile.WriteLine($"同步初始化 插件版本{Plg_ver} 本地时间：" + DateTime.Now.ToString());
                    outfile.Write(ex.ToString());
                }
                new Thread(() =>
                {
                    System.Windows.MessageBox.Show("点歌姬同步初始化错误！");
                })
                { Name = "点歌姬同步初始化错误弹窗", IsBackground = true }
                .Start();
                throw; // 让弹幕姬不加载插件
            }
        }

        private static void InitPluginInfo()
        {
            PluginMain p = PluginMain.self;
            p.PluginAuth = Plg_auth;
            p.PluginCont = Plg_cont;
            p.PluginDesc = Plg_desc;
            p.PluginName = Plg_name;
            p.PluginVer = Plg_ver;
        }

        internal static void syncInit()
        {
            new Thread(() =>
            {
                try
                {
                    InitDowndloder();
                    InitModule();
                    InitWindowDataBinding();
                    Config.Load();
                    InitOutput();
                    InitEventBind();

                    InitAbort();
                }
                catch (Exception ex)
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    using (StreamWriter outfile = new StreamWriter(path + @"\点歌姬异步初始化错误报告.txt"))
                    {
                        outfile.WriteLine("异步初始化 本地时间：" + DateTime.Now.ToString());
                        outfile.Write(ex.ToString());
                    }
                    new Thread(() =>
                    {
                        System.Windows.MessageBox.Show("点歌姬异步初始化错误！");
                    })
                    { Name = "点歌姬异步初始化错误弹窗", IsBackground = true }
                    .Start();
                }
            })
            {
                IsBackground = true,
                Name = "点歌姬异步初始化线程"
            }.Start();
        }

        private static void InitWindowDataBinding()
        {
            PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                Mainw.SongList.ItemsSource = Songs;

                Mainw.ModuleList.ItemsSource = SearchModules;

                Mainw.Combo_SearchModuleA.ItemsSource = SearchModules;
                Mainw.Combo_SearchModuleB.ItemsSource = SearchModules;
                // Mainw.Combo_SearchModuleA.SelectedItem = SearchModules[2];

                Mainw.BlackList.ItemsSource = BlackList;

                Mainw.Combo_OutputType.Items.Add("WaveOutEvent");
                Mainw.Combo_OutputType.Items.Add("DirectSound");

                Mainw.setPlayPauseButtonIcon(false);
                // Mainw.Combo_OutputType.SelectedIndex = 1; // 优先使用 DirectSound
            }));
        }

        private static void InitAbort()
        {
            PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            { Mainw.AboutWeb.Navigate(new Uri("https://www.danmuji.org/resource/DGJ/about")); }));

            versionChecker = new VersionChecker("DGJ");
            if (versionChecker.FetchInfo())
            {
                if (versionChecker.hasNewVersion(Plg_ver))
                {
                    string info = "插件有新版本了！最新版本:" + versionChecker.Version + "  当前版本:" + Plg_ver;
                    info += "  更新时间:" + versionChecker.UpdateDateTime.ToString("yyyy.MM.dd HH:mm");
                    info += "\r\n" + "下载地址：" + versionChecker.WebPageUrl;
                    info += "\r\n" + versionChecker.UpdateDescription;
                    Logg(info);
                }
            }
            else
            {
                Logg("版本检查失败：" + versionChecker.lastException.Message);
            }
        }

        internal static void LoadPlayDevices(int? type = null)
        {
            int t = type != null ? (int)type : Mainw.Combo_OutputType.SelectedIndex;
            if (Mainw.Combo_DeviceChoose.ItemsSource != null)
            { Mainw.Combo_DeviceChoose.ItemsSource = null; }
            else
            { Mainw.Combo_DeviceChoose.Items.Clear(); }
            switch (t)
            {
                case 0:
                default: // WaveOutEvent
                    Mainw.Combo_DeviceChoose.DisplayMemberPath = "";
                    Mainw.Combo_DeviceChoose.SelectedValuePath = "";
                    for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
                    {
                        var capabilities = WaveOut.GetCapabilities(deviceId);
                        Mainw.Combo_DeviceChoose.Items.Add(String.Format("设备 {0}  {1}", deviceId, capabilities.ProductName));
                    }
                    break;
                case 1: // DirectSound
                    Mainw.Combo_DeviceChoose.DisplayMemberPath = "Description";
                    Mainw.Combo_DeviceChoose.SelectedValuePath = "Guid";
                    Mainw.Combo_DeviceChoose.ItemsSource = DirectSoundOut.Devices;
                    break;
            }
            if (Mainw.Combo_DeviceChoose.Items.Count > 0)
            { Mainw.Combo_DeviceChoose.SelectedIndex = 0; }
        }

        private static void InitEventBind()
        {
            PluginMain p = PluginMain.self;
            p.ReceivedDanmaku += DanmukuReceiver.ReceivedDanmaku;
            p.ReceivedRoomCount += DanmukuReceiver.ReceivedRoomCount;
            p.Connected += DanmukuReceiver.Connected;
            p.Disconnected += DanmukuReceiver.Disconnected;
        }

        private static void InitLib()
        {
            try
            {
                File.WriteAllBytes(ConfigPath + "\\点歌姬播放模块", Properties.Resources.NAudio);
            }
            catch (Exception)
            {
                if (!File.Exists(ConfigPath + "\\点歌姬播放模块"))
                { throw; }
            }
            Assembly.LoadFrom(ConfigPath + "\\点歌姬播放模块");


            try
            {
                File.WriteAllBytes(ConfigPath + "\\点歌姬界面渲染A", Properties.Resources.MDT_W);
            }
            catch (Exception)
            {
                if (!File.Exists(ConfigPath + "\\点歌姬界面渲染A"))
                { throw; }
            }
            Assembly.LoadFrom(ConfigPath + "\\点歌姬界面渲染A");


            try
            {
                File.WriteAllBytes(ConfigPath + "\\点歌姬界面渲染B", Properties.Resources.MDC);
            }
            catch (Exception)
            {
                if (!File.Exists(ConfigPath + "\\点歌姬界面渲染B"))
                { throw; }
            }
            Assembly.LoadFrom(ConfigPath + "\\点歌姬界面渲染B");
        }

        private static void InitWindow()
        {
            Mainw = new MainWindow();
        }

        private static void InitOutput()
        {
            OutputControl.Init();
        }

        private static void InitPlayer()
        {
            PlayControl.Init();
        }

        private static void InitDowndloder()
        {
            DownloadControl.Init();
            DownloadWatchDog.Init();
        }

        private static void InitModule()
        {
            // SearchModules.Add(new netease_resrsa().setMainPlugin(PluginMain.that));
            // SearchModules.Add(new netease_old().setMainPlugin(PluginMain.that));
            // SearchModules.Add(new netease_suggest().setMainPlugin(PluginMain.that));
            SearchModules.Add(new LWLAPI.LwlApiNetease().setMainPlugin(PluginMain.self));
            SearchModules.Add(new LWLAPI.LwlApiTencent().setMainPlugin(PluginMain.self));
            SearchModules.Add(new LWLAPI.LwlApiKugou().setMainPlugin(PluginMain.self));
            SearchModules.Add(new LWLAPI.LwlApiXiami().setMainPlugin(PluginMain.self));
            SearchModules.Add(new LWLAPI.LwlApiBaidu().setMainPlugin(PluginMain.self));

            var path = "";
            try
            {
                path = Path.Combine(ConfigPath, "搜索拓展");
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                PluginMain.self.Log("加载搜索拓展时出错：" + ex.Message);
                return;
            }
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (file.EndsWith(".点歌姬搜索拓展") /*|| file.EndsWith(".gem")*/ )
                {
                    try
                    {
                        var dll = Assembly.LoadFrom(file);
                        foreach (var exportedType in dll.GetExportedTypes())
                        {
                            try
                            {
                                if (exportedType.BaseType == typeof(SongsSearchModule))
                                {
                                    var Module = (SongsSearchModule)Activator.CreateInstance(exportedType);
                                    SearchModules.Add(Module.setMainPlugin(PluginMain.self));
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}
