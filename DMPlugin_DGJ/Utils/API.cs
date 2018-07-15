using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace DMPlugin_DGJ
{
    internal static class API
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        internal const int KEYEVENTF_EXTENDEDKEY = 0x0001; // Key down flag
        internal const int KEYEVENTF_KEYUP = 0x0002; // Key up flag
        internal const int VK_MEDIA_NEXT_TRACK = 0xB0;
        internal const int VK_MEDIA_STOP = 0xB2;

        internal const uint SMTO_ABORTIFHUNG = 0x0002;
        internal const uint WM_GETTEXT = 0xD;
        internal const int MAX_STRING_SIZE = 32768;

        internal static String String(this SongItem.SongStatus status)
        {
            switch (status)
            {
                case SongItem.SongStatus.WaitingDownload:
                    return "等待下载";
                case SongItem.SongStatus.Downloading:
                    return "正在下载";
                case SongItem.SongStatus.WaitingPlay:
                    return "等待播放";
                case SongItem.SongStatus.Playing:
                    return "正在播放";
                default:
                    return "？？？？";
            }
        }

        /// <summary>
        /// 发送键盘按键
        /// </summary>
        /// <param name="k">要发送的按键</param>
        internal static void SendKey(Key k)
        {
            switch (k)
            {
                case Key.MediaNextTrack:
                    keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, 0);
                    return;
                case Key.MediaStop:
                    keybd_event(VK_MEDIA_STOP, 0, KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event(VK_MEDIA_STOP, 0, KEYEVENTF_KEYUP, 0);
                    return;
                default:
                    return;
            }
        }

        /// <summary>
        /// 遍历进程中的窗体
        /// </summary>
        /// <param name="p">进程</param>
        /// <returns></returns>
        static IEnumerable<IntPtr> EnumerateProcessWindowHandles(Process p)
        {
            var handles = new List<IntPtr>();
            foreach (ProcessThread thread in p.Threads)
            { EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero); }
            return handles;
        }

        /// <summary>
        /// 获取其他播放器的歌曲名字
        /// </summary>
        /// <returns>歌曲名字</returns>
        internal static string getOtherSongName()
        {
            // 网易云音乐：OrpheusBrowserHost
            // QQ音乐：QQmusic

            string[] classs = { "OrpheusBrowserHost" };
            string[] processes = { "QQmusic" };

            foreach (string s in classs)
            {
                IntPtr w = FindWindow(s, null);
                if (w != IntPtr.Zero)
                { return GetWindowTitle(w); }
            }

            foreach (string sp in processes)
            {
                Process[] ps = Process.GetProcessesByName(sp);
                if (ps.Length > 0)
                {
                    foreach (var handle in EnumerateProcessWindowHandles(ps[0]))
                    {
                        string w = GetWindowTitle(handle);
                        if (w.Contains("-"))
                        { return w; }
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// 从窗体获取标题
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        private static string GetWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// http 访问
        /// </summary>
        /// <param name="url">要访问的地址</param>
        /// <param name="cookies">要带的Cookie，null为不带</param>
        /// <param name="data">要post的参数，null为用get访问</param>
        /// <param name="referer">要带的referer，null为默认</param>
        /// <returns>http访问结果</returns>
        internal static string http(string url, CookieContainer cookies = null, string data = null, string referer = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            if (cookies != null)
            {
                request.CookieContainer = cookies;
            }

            if (referer != null)
            {
                request.Referer = referer;
            }
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
            request.Timeout = 30000;

            if (data != null)
            {
                var postData = Encoding.UTF8.GetBytes(data);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            return responseString;
        }

        /// <summary>
        /// 不带 cookie http 访问
        /// </summary>
        /// <param name="url">要访问的地址</param>
        /// <param name="data">要post的参数，null为用get访问</param>
        /// <param name="referer">要带的referer，null为默认</param>
        /// <returns>http访问结果</returns>
        static string http(string url, string data = null, string referer = null)
        {
            return http(url, null, data, referer);
        }
        /// <summary>
        /// get 方法 http 访问   
        /// </summary> 
        /// <param name="url">要访问的地址</param>
        /// <param name="cookies">要带的Cookie，null为不带</param>
        /// <param name="referer">要带的referer，null为默认</param>
        /// <returns>http访问结果</returns>
        static string http(string url, CookieContainer cookies = null, string referer = null)
        {
            return http(url, cookies, null, referer);
        }

    }
}
