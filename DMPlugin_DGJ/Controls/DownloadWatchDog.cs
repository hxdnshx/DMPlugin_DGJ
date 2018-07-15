using System;
using System.Threading;

namespace DMPlugin_DGJ
{
    internal class DownloadWatchDog
    {
        private static int second = 20;
        private static int min_speed = 25;

        private static DateTime time = new DateTime(0);
        private static readonly DateTime zero = new DateTime(0);
        private static bool running = false;

        internal static void Init()
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        if (running && time != zero)
                        {
                            if ((DateTime.Now - time) >= new TimeSpan(0, 0, second))
                            {
                                Center.Logg("下载速度长时间过慢，自动切断下载", true, true);
                                DownloadControl.CancelDownload();
                            }
                        }
                    }
                    catch (Exception) { }
                }
            })
            { Name = "DownloadWatchDog", IsBackground = true }.Start();
        }

        internal static void Start()
        { time = zero; running = true; }

        internal static void Stop()
        { running = false; }

        internal static void UpdateStatus(int speed_now, long bytesReceived, long totalBytesToReceive)
        {
            if (speed_now / 1024d < min_speed)
            { // 速度过慢
                if (time == zero)
                { time = DateTime.Now; }
            }
            else // 正常速度
            { if (time != zero) { time = zero; } }
        }
    }
}
