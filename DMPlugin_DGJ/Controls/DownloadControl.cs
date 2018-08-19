using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace DMPlugin_DGJ
{
    internal class DownloadControl
    {
        private static Thread thr = null;
        private static WebClient wc;

        private static Stopwatch sw = new Stopwatch(); // 用于计算下载速度
        private static long lastUpdateDownloadedSize = 0; // 上次更新的下载大小，用于计算实时下载速度
        private static DateTime lastUpdateTime; // 上次更新的时间，用于计算实时下载速度
        private static object downloadSpeedLock = new object();

        private static bool downloadFlag = false; // false为没有在下载
        private static SongItem dlItem = null; // 正在下载的歌曲，用于出错时从列表删除

        internal static void Init()
        {
            if (thr != null)
            {
                if (thr.IsAlive)
                { thr.Abort(); }
            }
            thr = new Thread(loop) { Name = "DownloadLoop", IsBackground = true };
            thr.Start();
        }

        private static void loop()
        {
            while (true)
            {
                Thread.Sleep(300);

                foreach (SongItem item in Center.Songs)
                {
                    if (item.Status == SongItem.SongStatus.WaitingDownload)
                    {
                        item.FilePath = Config.SongsCachePath + "\\" + GenFileName(item);
                        Download(item);

                        goto done;
                    }
                }
                done:
                ;
            }
        }

        private static string GenFileName(SongItem item)
        {
            string s = item.ModuleName;
            s += item.SongID;
            s += item.SongName;
            DateTime d = DateTime.Now;
            s += d.Hour.ToString() + d.Minute.ToString() + d.Second.ToString();

            return IllegalPathFilter(s) + ".点歌姬缓存";
        }

        private static string IllegalPathFilter(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + " ";
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, string.Empty);
        }

        private static void Download(SongItem item)
        {
            try
            { Directory.CreateDirectory(Config.SongsCachePath); }
            catch (Exception) { }

            dlItem = item;
            dlItem.SetStatus(SongItem.SongStatus.Downloading);

            if (item.Module.IsHandleDownlaod) // 如果搜索模块负责下载文件
            {
                Center.Mainw.setDownloadStatus("由搜索模块负责下载中");
                switch (item.Module.SafeDownload(item))
                {
                    case 1: // 下载成功
                        dlItem.SetStatus(SongItem.SongStatus.WaitingPlay);
                        Center.Logg("歌曲下载 下载成功：" + item.SongName);
                        Center.Mainw.setDownloadStatus("搜索模块返回下载成功");
                        return;
                    case 0: // 下载失败 错误信息由module输出
                    default:
                        Center.RemoveSong(item);
                        Center.Mainw.setDownloadStatus("搜索模块返回下载失败");
                        return;
                }
            }
            else // 如果搜索模块不负责下载文件
            {
                wc = new WebClient();

                wc.DownloadProgressChanged += onDownloadProgressChanged;
                wc.DownloadFileCompleted += onDownloadFileCompleted;
                wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.450 Safari/537.35");
                try
                {
                    sw.Reset();
                    lastUpdateDownloadedSize = 0;
                    lastUpdateTime = DateTime.Now;
                    wc.DownloadFileAsync(new Uri(item.DownloadURL), item.FilePath);
                    Center.Mainw.setDownloadStatus("正在连接服务器");
                    sw.Start();
                    DownloadWatchDog.Start();
                    downloadFlag = true; // 正在下载歌曲
                }
                catch (Exception ex)
                {
                    sw.Reset();
                    Center.Logg("下载歌曲" + item.SongName + "出错：" + ex.Message, true, true);
                    Center.Mainw.setDownloadStatus("下载失败：" + ex.Message);
                }

                while (downloadFlag)
                {
                    Thread.Sleep(500);
                }
                dlItem = null;
            }
        }

        /// <summary>
        /// 取消正在进行的下载
        /// </summary>
        internal static void CancelDownload()
        { wc?.CancelAsync(); }

        private static void onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //lock(downloadSpeedLock)
            //{
            //计算实时下载速度
            DateTime now = DateTime.Now;
            TimeSpan interval = now - lastUpdateTime;
            if (interval.TotalSeconds < 0.5)
            { return; }
            double timeDiff = interval.TotalSeconds;
            long sizeDiff = e.BytesReceived - lastUpdateDownloadedSize;
            lastUpdateDownloadedSize = e.BytesReceived;
            lastUpdateTime = now;
            int speed_now = (int)Math.Floor(sizeDiff / timeDiff);

            // 计算平均下载速度
            string speed_avg = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));

            // 下载内容百分比
            string baifenbi = e.ProgressPercentage.ToString() + "%";

            // 下载大小
            string downloadSize = string.Format("{0} MB / {1} MB", (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));

            DownloadWatchDog.UpdateStatus(speed_now, e.BytesReceived, e.TotalBytesToReceive);
            Center.Mainw.setDownloadStatus($"实时速度：{ string.Format("{0} kb/s", (speed_now / 1024d).ToString("0.00")) } 百分比：{ baifenbi } 大小：{ downloadSize}");
            //}
        }

        private static void onDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            bool sucFlag = true;
            if (e.Cancelled)
            {
                Center.Logg("下载被取消");
                Center.Mainw.setDownloadStatus("下载被取消");
                sucFlag = false;
                try
                { File.Delete(dlItem.FilePath); }
                catch (Exception) { }
            }
            else if (e.Error != null)
            {
                Center.Logg("下载出错 " + e.Error.Message, true, true);
                Center.Mainw.setDownloadStatus("下载出错 " + e.Error.Message);
                sucFlag = false;
            }

            if (sucFlag)
            {
                Center.Logg("歌曲下载 下载成功：" + dlItem.SongName, false, true);
                Center.Mainw.setDownloadStatus("下载完毕");
                dlItem.SetStatus(SongItem.SongStatus.WaitingPlay);
            }
            else
            { Center.RemoveSong(dlItem); }
            DownloadWatchDog.Stop();
            wc = null;
            downloadFlag = false; // 允许进行下一首歌的下载
        }
    }
}
