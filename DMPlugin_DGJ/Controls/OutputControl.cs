using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;

namespace DMPlugin_DGJ
{

    internal class OutputControl
    {
        private static readonly string outputPath = Path.Combine(Center.ConfigPath, "输出文本");

        private static readonly string Songs_path = Path.Combine(outputPath, Path.GetFileName("歌曲信息.txt"));
        private static readonly string Configs_path = Path.Combine(outputPath, Path.GetFileName("设置信息.txt"));
        private static readonly string Lyric_path = Path.Combine(outputPath, Path.GetFileName("歌词输出.txt"));
        private static readonly string Log_path = Path.Combine(outputPath, Path.GetFileName("日志输出.txt"));

        private static Thread Songs_thr = null;
        private static string Songs_str = "";

        private static Thread Log_thr = null;
        private static bool Log_NeedClear = true;
        private static Queue<string> Log_queue = new Queue<string>();

        private static Thread Configs_thr = null;
        private static string Configs_str = "";

        private static string lastLrc = "";
        private static bool LrcMainA = true;

        internal static void Init()
        {
            try
            {
                Directory.CreateDirectory(outputPath);
                File.WriteAllText(Songs_path, "", Encoding.UTF8);
                //File.WriteAllText(Configs_path, "", Encoding.UTF8);
                File.WriteAllText(Lyric_path, "", Encoding.UTF8);
            }
            catch (Exception)
            { }
            if (Songs_thr == null)
            {
                Songs_thr = new Thread(Songs_output) { Name = "Songs_output", IsBackground = true };
                Songs_thr.Start();
            }
            if (Log_thr == null)
            {
                Log_thr = new Thread(Log_output) { Name = "Log_output", IsBackground = true };
                Log_thr.Start();
            }
            //if(Configs_thr == null)
            //    new Thread(Configs_output) { Name = "Configs_output", IsBackground = true }.Start();
        }

        private static void Songs_output()
        {
            while (true)
            {
                Thread.Sleep(Config.outputUpdateTime);

                string before = Config.OPTBefore;
                if (before != string.Empty)
                {
                    before = before
                        .Replace("[歌曲数量]", Center.Songs.Count.ToString())
                        .Replace("[歌曲上限]", Config.maxSongCount.ToString())
                        .Replace("[播放进度]", $@"{Config.time_now:%m}:{Config.time_now:ss}")
                        .Replace("[歌曲长度]", $@"{Config.time_total:%m}:{Config.time_total:ss}");
                    before += Config.CRLF;
                }

                string after = Config.OPTAfter;
                if (after != string.Empty)
                {
                    after = after
                        .Replace("[歌曲数量]", Center.Songs.Count.ToString())
                        .Replace("[歌曲上限]", Config.maxSongCount.ToString())
                        .Replace("[播放进度]", $@"{Config.time_now:%m}:{Config.time_now:ss}")
                        .Replace("[歌曲长度]", $@"{Config.time_total:%m}:{Config.time_total:ss}");
                }

                string middle = string.Empty;

                if (Center.Songs.Count == 0)
                {
                    string other = API.getOtherSongName();
                    middle = (other != string.Empty) ? Config.OutputOtherNamePrefix + other : Config.OutputEmptyList;
                    middle += Config.CRLF;
                }
                else
                {
                    int i = 0;
                    foreach (SongItem item in Center.Songs)
                    {
                        if (i >= Config.OPTLineNum)
                            break;

                        if (i > 0)
                            middle += Config.CRLF;

                        string t = Config.OPTLine;
                        t = t.Replace("[序号]", (i + 1).ToString())
                            .Replace("[状态]", item.Status.String())
                            .Replace("[歌名]", item.SongName)
                            .Replace("[歌手]", item.SingersText)
                            .Replace("[点歌人]", item.UserName);

                        middle += t;
                        i++;
                    }
                    middle += (after != string.Empty) ? Config.CRLF : string.Empty;

                }
                WriteSongs(before + middle + after);

            }
        }

        private static void Configs_output()
        {
            while (true)
            {
                Thread.Sleep(Config.outputUpdateTime * 5);

                string output = "";

                output += "歌曲列表上限" + Config.maxSongCount.ToString() + "\r\n";
                output += "播放音量：" + Config.songVol.ToString() + "\r\n";

                if (Configs_str != output)
                {
                    Configs_str = output;
                    File.WriteAllText(Configs_path, output, Encoding.UTF8);
                }

            }
        }

        private static void Log_output()
        {
            while (true)
            {
                if (Log_queue.Count > 0)
                {
                    File.WriteAllText(Log_path, Log_queue.Dequeue(), Encoding.UTF8);
                    Log_NeedClear = true;
                    Thread.Sleep(Config.LogStayTime * 1000);
                }
                else
                {
                    if (Log_NeedClear)
                        File.WriteAllText(Log_path, string.Empty, Encoding.UTF8);
                    Log_NeedClear = false;
                    Thread.Sleep(200);
                }
            }
        }

        internal static void AddLog(string text) => Log_queue.Enqueue(text);

        private static void WriteSongs(string str)
        {
            if (Songs_str != str)
            {
                PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    Center.Mainw.Output_View.Text = str;
                }));
                File.WriteAllText(Songs_path, str, Encoding.UTF8);
                Songs_str = str;
            }
        }

        internal static void WriteLyric(string nowlrc = "", string nextlrc = "")
        {
            string output = nowlrc + "\r\n" + nextlrc;

            if (output == lastLrc)
                return;
            lastLrc = output;

            try
            {
                if (Config.OneLyric)
                { File.WriteAllText(Lyric_path, nowlrc, Encoding.UTF8); }
                else
                { File.WriteAllText(Lyric_path, output, Encoding.UTF8); }
            }
            catch (Exception) { }

            if (Config.DisplayLyric && nowlrc != string.Empty)
            { Center.FakeDM("歌词", nowlrc); }

            if (Center.Mainw != null)
            {
                PluginMain.self.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    if (LrcMainA)
                    {
                        Center.Mainw.LyricDisplayA.Inlines.Clear();
                        Center.Mainw.LyricDisplayA.Inlines.Add(new System.Windows.Documents.Run(nowlrc) { TextDecorations = TextDecorations.Underline });
                        Center.Mainw.LyricDisplayB.Text = nextlrc;
                    }
                    else
                    {
                        Center.Mainw.LyricDisplayB.Inlines.Clear();
                        Center.Mainw.LyricDisplayB.Inlines.Add(new System.Windows.Documents.Run(nowlrc) { TextDecorations = TextDecorations.Underline });
                        Center.Mainw.LyricDisplayA.Text = nextlrc;
                    }
                    LrcMainA ^= true;
                }));
            }
        }

        internal static void DeInit()
        {
            Songs_thr?.Abort();
            Songs_thr.Join(Config.outputUpdateTime);
            WriteSongs(string.Empty);
            WriteLyric();
        }

    }
}
