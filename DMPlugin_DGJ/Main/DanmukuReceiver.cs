using BilibiliDM_PluginFramework;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Threading;

namespace DMPlugin_DGJ
{
    internal class DanmukuReceiver
    {

        internal static void DGJ_AdminComment(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg) || !Config.DanmakuControl)
            { return; } // 如果弹幕msg没有内容或者不允许弹幕控制

            string[] msgs = msg.Split(new string[] { " " }, System.StringSplitOptions.RemoveEmptyEntries);
            switch (msgs[0])
            {
                case "暫停":
                case "暂停":
                    if (PlayControl.Pause())
                        Log("歌曲已经暂停");
                    return;
                case "播放":
                    if (PlayControl.Play())
                        Log("歌曲继续播放");
                    return;
                case "切歌":
                    if (PlayControl.Next())
                        Log("已经切掉正在播放的歌曲");
                    return;
                case "刪歌":
                case "删歌":
                    int num;
                    if (msgs.Length >= 2 && int.TryParse(msgs[1], out num))
                    {
                        num--;
                        if (num >= 0 && num < Center.Songs.Count)
                        {
                            SongItem item = Center.Songs[num];
                            Log($"切掉了 {item.User} 点的 {item.SongName}", true);
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
                                        new Thread((object o) =>
                                        {
                                            try
                                            {
                                                File.Delete(o.ToString());
                                            }
                                            catch (Exception) { }
                                        })
                                        {
                                            IsBackground = true,
                                            Name = "切歌后删除文件"
                                        }.Start(item.FilePath);
                                    Center.RemoveSong(item);
                                    return;
                                case SongItem.SongStatus.Playing:
                                    PlayControl.Next();
                                    return;
                            }
                        }
                    }
                    return;
                case "音量":
                    int v;
                    if (msgs.Length >= 2 && int.TryParse(msgs[1], out v))
                    {
                        if (PlayControl.SetVol(v))
                            Log("音量设置成功", true);
                        else
                            Log("音量设置失败", true);
                    }
                    else
                        Log("音量应该是数字", true);
                    return;
                case "上限":
                    int sv;
                    if (msgs.Length >= 2 && int.TryParse(msgs[1], out sv))
                    {
                        Config.maxSongCount = sv;
                        Log("歌曲列表上限已修改为" + sv + "首歌", true);
                    }
                    else
                        Log("上限应该是数字", true);
                    return;
                case "优先":
                case "優先":
                    if (msgs.Length >= 2 && int.TryParse(msgs[1], out int songIndex))
                        Center.MoveSong(songIndex);
                    else
                        Log("歌曲序号应该是数字", true);
                    return;
                case "歌詞":
                case "歌词":
                    Config.needLyric ^= true;
                    Log("歌词搜索已" + (Config.needLyric ? "启用" : "禁用"), true);
                    return;
                case "保存配置":
                    Config.Save();
                    return;
                case "加載配置":
                case "加载配置":
                    Config.Load();
                    return;
                default:
                    break;
            }

        }

        internal static void DGJ_NormalComment(ReceivedDanmakuArgs e)
        {
            string[] msgs = e.Danmaku.CommentText.Split(new string[] { " " }, System.StringSplitOptions.RemoveEmptyEntries);
            switch (msgs[0])
            {
                case "點歌":
                case "点歌":
                    if (msgs.Length < 2)
                        return;
                    if (Center.Songs.Count < Config.maxSongCount)
                    {
                        if (Config.AdminOnly && !e.Danmaku.isAdmin)
                            return;

                        if (!Config.CanMultiSong && !e.Danmaku.isAdmin)
                        {
                            foreach (SongItem s in Center.Songs)
                            {
                                if (s.User == e.Danmaku.UserName)
                                {
                                    Log($"点歌失败：{e.Danmaku.UserName}已有歌在列表中", true);
                                    return;
                                }
                            }
                        }

                        string what = "";
                        int it = 0;
                        foreach (string str in msgs)
                        {
                            if (++it == 1)
                                continue;
                            if (it != 2)
                                what += " ";
                            what += str;
                        }

                        SongItem i = Center.SearchSong(e.Danmaku.UserName, what);

                        if (i != null)
                        {
                            if (Center.isBlack(i))
                            {
                                Log($"歌曲“{i.SongName}”在黑名单中", true);
                                return;
                            }

                            bool flag = true;
                            foreach (SongItem s in Center.Songs)
                                if (s.SongID == i.SongID && s.ModuleName == i.ModuleName)
                                { flag = false; break; }
                            if (flag)
                            {
                                Center.AddSong(i);
                                Log("点歌成功 " + i.SongName, true);
                            }
                            else
                            { Log("歌曲重复了", true); }
                        }
                    }
                    else
                    { Log("点歌失败 列表曲目过多", true); }
                    return;
                case "取消點歌":
                case "取消点歌":
                    SongItem item = null;
                    foreach (SongItem s in Center.Songs)
                        if (s.User == e.Danmaku.UserName)
                            item = s;
                    if (item != null)
                    {
                        switch (item.Status)
                        {
                            case SongItem.SongStatus.WaitingDownload:
                                Center.RemoveSong(item);
                                break;
                            default:
                            case SongItem.SongStatus.WaitingPlay:
                            case SongItem.SongStatus.Downloading:
                                Center.RemoveSong(item);
                                if (item.FilePath != null && item.FilePath != "")
                                    new Thread((object o) =>
                                    {
                                        try
                                        { File.Delete(o.ToString()); }
                                        catch (Exception) { }
                                    })
                                    {
                                        IsBackground = true,
                                        Name = "取消点歌后删除文件"
                                    }.Start(item.FilePath);
                                break;
                            case SongItem.SongStatus.Playing:
                                break;
                        }
                    }
                    return;
                case "切歌":
                    if (Config.VoteNext > 0)
                    {
                        if (!VoteUids.Contains(e.Danmaku.UserID))
                        {
                            VoteUids.Add(e.Danmaku.UserID);
                            if (VoteUids.Count >= Config.VoteNext)
                            {
                                Log("投票切歌生效", true);
                                VoteUids.Clear();
                                PlayControl.Next();
                            }
                            else
                            {
                                Log($"投票切歌 当前{VoteUids.Count}票 需要{Config.VoteNext}票", true);
                            }
                        }
                    }
                    return;
                default:
                    break;
            }
        }

        internal static List<int> VoteUids = new List<int>();

        internal static void ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            switch (e.Danmaku.MsgType)
            {
                case MsgTypeEnum.Comment:
                    string msg = e.Danmaku.CommentText;
                    if (msg.StartsWith("%") && e.Danmaku.isAdmin)
                    {
                        DGJ_AdminComment(msg.Substring(1));
                    }
                    else
                    {
                        DGJ_NormalComment(e);
                    }
                    return;
                case MsgTypeEnum.GiftSend:
                    break;
                case MsgTypeEnum.GiftTop:
                    break;
                case MsgTypeEnum.Welcome:
                    break;
                case MsgTypeEnum.LiveStart:
                    break;
                case MsgTypeEnum.LiveEnd:
                    break;
                case MsgTypeEnum.Unknown:
                    break;
                default:
                    break;
            }
        }

        internal static void Disconnected(object sender, DisconnectEvtArgs e)
        {

        }

        internal static void ReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {

        }

        internal static void Connected(object sender, ConnectedEvtArgs e)
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
                return;

        }

        private static void Log(string text, bool logfile = false)
        {
            Center.Logg(text, true, logfile);
        }
    }
}
