using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace DMPlugin_DGJ
{
    internal class Config
    {
        private static readonly string configpath = Path.Combine(Center.ConfigPath, Path.GetFileName("点歌姬配置文件"));
        private static readonly string blackpath = Path.Combine(Center.ConfigPath, Path.GetFileName("点歌姬歌曲黑名单"));
        private static readonly string outputpath = Path.Combine(Center.ConfigPath, Path.GetFileName("点歌姬输出文本格式"));

        internal static void Load()
        {
            IniFile ini = new IniFile(configpath);

            int aint;
            songVol = (int.TryParse(ini.Read("songVol"), out aint) ? aint : DefaultConfig.songVol);
            maxSongCount = (int.TryParse(ini.Read("maxSongCount"), out aint) ? aint : DefaultConfig.maxSongCount);
            // outputUpdateTime = (int.TryParse(ini.Read("outputUpdateTime"), out aint) ? aint : DefaultConfig.outputUpdateTime);
            outputUpdateTime = DefaultConfig.outputUpdateTime;
            MaxPlaySecond = (int.TryParse(ini.Read("MaxPlaySecond"), out aint) ? aint : DefaultConfig.MaxPlaySecond);
            VoteNext = (int.TryParse(ini.Read("VoteNext"), out aint) ? aint : DefaultConfig.VoteNext);
            LogStayTime = (int.TryParse(ini.Read("LogStayTime"), out aint) ? aint : DefaultConfig.LogStayTime);
            OutputType = (int.TryParse(ini.Read("OutputType"), out aint) ? aint : DefaultConfig.OutputType);

            bool abool;
            CanMultiSong = (bool.TryParse(ini.Read("CanMultiSong"), out abool) ? abool : DefaultConfig.CanMultiSong);
            AdminOnly = (bool.TryParse(ini.Read("AdminOnly"), out abool) ? abool : DefaultConfig.AdminOnly);
            ControlOtherPlayer = (bool.TryParse(ini.Read("ControlOtherPlayer"), out abool) ? abool : DefaultConfig.ControlOtherPlayer);
            DanmakuControl = (bool.TryParse(ini.Read("DanmakuControl"), out abool) ? abool : DefaultConfig.DanmakuControl);
            OneLyric = (bool.TryParse(ini.Read("OneLyric"), out abool) ? abool : DefaultConfig.OneLyric);
            DanmakuSongFirst = (bool.TryParse(ini.Read("DanmakuSongFirst"), out abool) ? abool : DefaultConfig.DanmakuSongFirst);
            BroadcasterLoop = (bool.TryParse(ini.Read("BroadcasterLoop"), out abool) ? abool : DefaultConfig.BroadcasterLoop);

            OutputEmptyList = ini.KeyExists("OutputEmptyList") ? ini.Read("OutputEmptyList") : DefaultConfig.OutputEmptyList;
            OutputOtherNamePrefix = ini.KeyExists("OutputOtherNamePrefix") ? ini.Read("OutputOtherNamePrefix") : DefaultConfig.OutputOtherNamePrefix;

            OPT_Load();

            PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                Center.Mainw.Vol.Value = songVol;
                Center.Mainw.Setting_MaxSongCount.Text = maxSongCount.ToString();
                Center.Mainw.Setting_CanMultiSong.IsChecked = CanMultiSong;
                Center.Mainw.Setting_AdminOnly.IsChecked = AdminOnly;
                Center.Mainw.Setting_MaxPlaySecond.Text = MaxPlaySecond.ToString();
                Center.Mainw.Setting_VoteNext.Text = VoteNext.ToString();
                Center.Mainw.Setting_LogStayTime.Text = LogStayTime.ToString();
                Center.Mainw.Setting_ControlOtherPlayer.IsChecked = ControlOtherPlayer;
                Center.Mainw.Setting_DanmakuControl.IsChecked = DanmakuControl;
                Center.Mainw.Setting_OneLyric.IsChecked = OneLyric;
                Center.Mainw.Setting_DanmakuSongFirst.IsChecked = DanmakuSongFirst;
                Center.Mainw.Setting_BroadcasterLoop.IsChecked = BroadcasterLoop;
                Center.Mainw.Setting_OutputEmptyList.Text = OutputEmptyList;
                Center.Mainw.Setting_OutputOtherNamePrefix.Text = OutputOtherNamePrefix;
                Center.Mainw.Combo_OutputType.SelectedIndex = OutputType;

                BLK_Load();

                if (Center.SearchModules.Count > 0)
                {
                    string moduleA = ini.Read("moduleA");
                    string moduleB = ini.Read("moduleB");

                    if (!string.IsNullOrEmpty(moduleA))
                    {
                        foreach (SongsSearchModule s in Center.SearchModules)
                        {
                            if (s.ModuleName == moduleA)
                            { Center.Mainw.Combo_SearchModuleA.SelectedItem = s; break; }
                        }
                    }
                    if (Center.Mainw.Combo_SearchModuleA.SelectedItem == null)
                    { Center.Mainw.Combo_SearchModuleA.SelectedItem = Center.SearchModules[0]; }

                    if (!string.IsNullOrEmpty(moduleB))
                    {
                        foreach (SongsSearchModule s in Center.SearchModules)
                        {
                            if (s.ModuleName == moduleB)
                            { Center.Mainw.Combo_SearchModuleB.SelectedItem = s; break; }
                        }
                    }
                }
            }));
        }

        internal static void Save()
        {
            IniFile ini = new IniFile(configpath);
            ini.Write("请勿编辑此文件", "Don't Edit This File", "请勿编辑此文件");

            ini.Write("songVol", songVol.ToString());
            ini.Write("maxSongCount", maxSongCount.ToString());
            ini.Write("outputUpdateTime", outputUpdateTime.ToString());
            ini.Write("OutputType", OutputType.ToString());
            ini.Write("CanMultiSong", CanMultiSong.ToString());
            ini.Write("AdminOnly", AdminOnly.ToString());
            ini.Write("MaxPlaySecond", MaxPlaySecond.ToString());
            ini.Write("VoteNext", VoteNext.ToString());
            ini.Write("LogStayTime", LogStayTime.ToString());
            ini.Write("ControlOtherPlayer", ControlOtherPlayer.ToString());
            ini.Write("OneLyric", OneLyric.ToString());
            ini.Write("DanmakuControl", DanmakuControl.ToString());
            ini.Write("OutputEmptyList", OutputEmptyList);
            ini.Write("OutputOtherNamePrefix", OutputOtherNamePrefix);
            ini.Write("DanmakuSongFirst",DanmakuSongFirst.ToString());
            ini.Write("BroadcasterLoop", BroadcasterLoop.ToString());

            ini.Write("moduleA", R_ModuleNameA); // 保存当前选择的搜索模块的名字
            ini.Write("moduleB", R_ModuleNameB);

            OPT_Save();
            BLK_Save();
        }

        /// <summary>
        /// 从文件加载歌曲黑名单
        /// </summary>
        internal static void BLK_Load()
        {
            try
            {
                JArray j = JArray.Parse(File.ReadAllText(blackpath, Encoding.UTF8));
                Center.BlackList.Clear();
                foreach (JObject o in j)
                {
                    Center.BlackList.Add(new BlackInfoItem()
                    {
                        BLK_Enable = o["BLK_Enable"].ToObject<bool>(),
                        BLK_Type = o["BLK_Type"].ToObject<BlackInfoType>(),
                        BLK_Text = o["BLK_Text"].ToString()
                    });
                }
            }
            catch (Exception) { return; }
        }

        /// <summary>
        /// 保存歌曲黑名单到文件
        /// </summary>
        internal static void BLK_Save()
        { File.WriteAllText(blackpath, JsonConvert.SerializeObject(Center.BlackList), Encoding.UTF8); }

        internal static void OPT_Load()
        {
            try
            {
                JObject j = JObject.Parse(File.ReadAllText(outputpath));
                OPTBefore = j["Before"].ToString();
                OPTLine = j["Line"].ToString();
                OPTLineNum = j["Num"].ToObject<int>();
                OPTAfter = j["After"].ToString();
            }
            catch (Exception ex) when (ex is JsonException || ex is IOException)
            {
                OPTBefore = DefaultConfig.OPTBefore;
                OPTLine = DefaultConfig.OPTLine;
                OPTLineNum = DefaultConfig.OPTLineNum;
                OPTAfter = DefaultConfig.OPTAfter;
            }
            finally
            {
                PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    Center.Mainw.Output_Before.Text = OPTBefore;
                    Center.Mainw.Output_Line.Text = OPTLine;
                    Center.Mainw.Output_LineNum.Text = OPTLineNum.ToString();
                    Center.Mainw.Output_After.Text = OPTAfter;
                }));
            }
        }

        internal static void OPT_Save()
        { File.WriteAllText(outputpath, JsonConvert.SerializeObject(new { Before = OPTBefore, After = OPTAfter, Line = OPTLine, Num = OPTLineNum }), Encoding.UTF8); }


        #region - 仅本次有效的配置 -


        internal static string MASTER_NAME
        { get; set; }
        = "主播";

        /// <summary>
        /// DerectSound输出设备
        /// </summary>
        internal static Guid DirectSoundDevice
        { get; set; }

        /// <summary>
        /// 播放输出类型 0:WaveOutEvent 1:DirectSound
        /// </summary>
        /// 现已加入需要保存到配置文件的设置大礼包
        internal static int OutputType
        { get; set; }

        /// <summary>
        /// 歌曲播放的设备Id
        /// </summary>
        internal static int DeviceId
        { get; set; }

        /// <summary>
        /// 是否显示歌词到弹幕侧边栏
        /// </summary>
        internal static bool DisplayLyric
        { get; set; }

        /// <summary>
        /// 当前选择的搜索模块A名字
        /// </summary>
        private static string R_ModuleNameA
        {
            get
            {
                return Center.CurrentModuleA == null ? "" : Center.CurrentModuleA.ModuleName;
            }
        }
        /// <summary>
        /// 当前选择的搜索模块B名字
        /// </summary>
        private static string R_ModuleNameB
        {
            get
            {
                return Center.CurrentModuleB == null ? "" : Center.CurrentModuleB.ModuleName;
            }
        }

        /// <summary>
        /// 换行符
        /// </summary>
        internal static string CRLF { get { return Environment.NewLine; } }
        #endregion


        #region - 需要文件读写的配置 -

        /// <summary>
        /// 歌曲缓存位置
        /// cachepath
        /// </summary>
        internal static string SongsCachePath
        {
            get
            { return Path.Combine(Center.ConfigPath, "歌曲缓存"); }
        }

        /// <summary>
        /// 歌曲播放音量
        /// songvol
        /// </summary>
        internal static int songVol
        { get; set; }

        // /// <summary>
        // /// 是否需要歌词
        // /// needLryic
        // /// </summary>
        // internal static bool needLyric
        // { get; set; }

        /// <summary>
        /// 播放列表里最多几首歌
        /// maxSongCount
        /// </summary>
        internal static int maxSongCount
        { get; set; }

        /// <summary>
        /// 允许普通用户多点歌曲
        /// </summary>
        internal static bool CanMultiSong
        { get; set; }

        /// <summary>
        /// 只允许房管点歌
        /// </summary>
        internal static bool AdminOnly
        { get; set; }

        /// <summary>
        /// 一首歌最多播放时间（秒
        /// </summary>
        internal static int MaxPlaySecond
        { get; set; }

        /// <summary>
        /// 切歌需要票数
        /// </summary>
        internal static int VoteNext
        { get; set; }

        /// <summary>
        /// 日志输出停留时间（秒）
        /// </summary>
        internal static int LogStayTime
        { get; set; }

        /// <summary>
        /// 没歌时控制其他播放器
        /// </summary>
        internal static bool ControlOtherPlayer
        { get; set; }

        /// <summary>
        /// 能否弹幕控制
        /// </summary>
        internal static bool DanmakuControl
        { get; set; }

        /// <summary>
        /// 只输出一行歌词
        /// </summary>
        internal static bool OneLyric
        { get; set; }

        /// <summary>
        /// 列表无歌曲并且没开其他播放器时输出字符
        /// </summary>
        internal static string OutputEmptyList
        { get; set; }

        /// <summary>
        /// 其他播放器歌曲显示输出前缀
        /// </summary>
        internal static string OutputOtherNamePrefix
        { get; set; }

        /// <summary>
        /// 输出文件更新时间间隔（毫秒）
        /// outputUpdateTime
        /// </summary>
        internal static int outputUpdateTime
        { get; set; }

        /// <summary>
        /// 优先播放弹幕上点的歌曲
        /// DanmakuSongFirst
        /// </summary>
        internal static bool DanmakuSongFirst
        { get; set; }

        /// <summary>
        /// 播主点的歌曲在播放结束后插入到播放列表末尾
        /// BroadcasterLoop
        /// </summary>
        internal static bool BroadcasterLoop
        { get; set; }

        #endregion

        #region -- 输出文本相关设置 --

        /// <summary>
        /// 输出列表前文字
        /// </summary>
        internal static string OPTBefore { get; set; }

        /// <summary>
        /// 输出列表单行格式
        /// </summary>
        internal static string OPTLine { get; set; }

        /// <summary>
        /// 输出列表输出行数
        /// </summary>
        internal static int OPTLineNum { get; set; }

        /// <summary>
        /// 输出列表后文字
        /// </summary>
        internal static string OPTAfter { get; set; }

        /// <summary>
        /// 设置输出文本用播放进度
        /// </summary>
        /// <param name="now">当前播放时间</param>
        /// <param name="total">歌曲总长度</param>
        internal static void setPlayTime(TimeSpan now, TimeSpan total)
        { time_now = now; time_total = total; }

        internal static TimeSpan time_now;
        internal static TimeSpan time_total;

        #endregion
    }
}
