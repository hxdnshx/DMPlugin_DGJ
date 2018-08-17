using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Timers;

namespace DMPlugin_DGJ
{
    internal class PlayControl
    {
        private static Thread thr = null;
        private static System.Timers.Timer timer;

        private static IWavePlayer waveout;
        private static Mp3FileReader FileReader;

        private static bool playNotDone = true;
        private static Lrc lrc = null;

        private static Action<float> setVolumeDelegate;

        internal static void Init()
        {
            if (thr != null && thr.IsAlive)
            {
                thr.Abort();
            }

            timer = new System.Timers.Timer(100);
            timer.Elapsed += new ElapsedEventHandler(timer_tick);

            thr = new Thread(loop) { Name = "PlayLoop", IsBackground = true };
            thr.Start();
        }

        internal static void loop()
        {
            while (true)
            {
                Thread.Sleep(300);

                foreach (SongItem iitem in Center.Songs)
                {
                    if (iitem._Status == SongItem.SongStatus.WaitingPlay)
                    {
                        iitem.setStatus(SongItem.SongStatus.Playing);
                        playNotDone = true;

                        try
                        {
                            Load(iitem);
                            Play();

                            while (playNotDone)
                                Thread.Sleep(500);
                        }
                        catch (Exception ex)
                        {
                            ThrowPlayException(ex, iitem);
                        }
                        finally
                        {
                            playNotDone = false;
                            UnLoad();
                            RemoveSong(iitem);
                            if (Config.broadcasterLoop)
                            {
                                iitem.setStatus(SongItem.SongStatus.WaitingDownload);
                                Center.AddSong(iitem);
                            }
                        }
                        goto done;
                    }
                }
                done:
                ;
            }
        }

        private static void ThrowPlayException(Exception ex, SongItem song)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            using (StreamWriter outfile = new StreamWriter(path + @"\点歌姬播放错误报告.txt"))
            {
                outfile.WriteLine("播放错误日志 **请将本文件发给宅急送队长** 本地时间：" + DateTime.Now.ToString());
                outfile.WriteLine($"歌曲信息 歌名：{song.SongName}；歌手：{song.SingersText}；歌曲ID：{song.SongID}；");
                outfile.WriteLine("歌曲文件路径：" + song.FilePath);
                outfile.WriteLine($"搜索模块信息 名称：{song.Module.ModuleName}；作者：{song.Module.ModuleAuther}；联系方式：{song.Module.ModuleCont}；歌词支持：{song.Module.SuppLyric.ToString()}；");
                outfile.WriteLine("--------------");
                outfile.Write(ex.ToString());
            }
            new Thread(() =>
            {
                System.Windows.MessageBox.Show("点歌姬播放歌曲错误！\n请将桌面上的错误日志发给宅急送队长");
            })
            { Name = "点歌姬异步初始化错误弹窗", IsBackground = true }
            .Start();
        }

        #region - 控制播放模块 -
        /// <summary>
        /// 加载歌曲
        /// </summary>
        /// <param name="itemm">要加载的歌曲</param>
        private static void Load(SongItem itemm)
        {
            if (Config.ControlOtherPlayer)
            { API.SendKey(System.Windows.Input.Key.MediaStop); }
            DanmukuReceiver.VoteUids.Clear();

            waveout = Create();

            FileReader = new Mp3FileReader(itemm._FilePath);

            var sampleChannel = new SampleChannel(FileReader, true);
            setVolumeDelegate = vol => sampleChannel.Volume = vol;

            var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
            waveout.Init(postVolumeMeter);

            waveout.PlaybackStopped += Waveout_PlaybackStopped;
            SetVol();

            lrc = itemm._FLyric ?? itemm.getFLyric();
        }

        private static IWavePlayer Create()
        {
            switch (Config.OutputType)
            {
                case 0:
                default:
                    return new WaveOutEvent()
                    { DeviceNumber = Config.DeviceId };
                case 1:
                    return new DirectSoundOut(Config.DirectSoundDevice);
            }
        }

        /// <summary>
        /// 播放歌曲
        /// </summary>
        internal static bool Play()
        {
            if (!playNotDone)
                return false;
            if (waveout != null)
                waveout.Play();
            if (timer != null)
                timer.Start();

            Center.Mainw.setPlayPauseButtonIcon(!PlayControl.isPause);

            return true;
        }

        /// <summary>
        /// 暂停歌曲
        /// </summary>
        internal static bool Pause()
        {
            if (!playNotDone)
                return false;
            if (waveout != null)
                waveout.Pause();
            if (timer != null)
                timer.Stop();

            Center.Mainw.setPlayPauseButtonIcon(!PlayControl.isPause);

            return true;
        }

        /// <summary>
        /// 是否暂停状态
        /// </summary>
        /// <returns></returns>
        internal static bool isPause
        { get { return (waveout != null) ? waveout.PlaybackState != PlaybackState.Playing : true; } }

        /// <summary>
        /// 切歌
        /// </summary>
        internal static bool Next()
        {
            if (!playNotDone)
                return false;
            playNotDone = false;
            return true;
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="v">音量 0到100</param>
        internal static bool SetVol(int v = -1)
        {
            if (v < -1)
                return false;
            if (v == -1)
            {
                setVolumeDelegate?.Invoke(Config.songVol / 100f);
                return false;
            }
            if (v > 100)
                v = 100;
            if (Config.songVol == v)
                return true;
            if (waveout != null)
                setVolumeDelegate?.Invoke(v / 100f);
            Config.songVol = v;
            return true;
        }

        /// <summary>
        /// 关闭歌曲流并释放资源
        /// </summary>
        private static void UnLoad()
        {
            timer.Stop();

            if (waveout != null)
            {
                waveout.Stop();
                waveout.Dispose();
            }
            if (FileReader != null)
            { FileReader.Dispose(); }

            setVolumeDelegate = null;
            waveout = null;
            FileReader = null;
            lrc = null;

            TimeSpan t = TimeSpan.FromTicks(0);
            Center.Mainw.setDisplayTime(t, t);
            Config.setPlayTime(t, t);
            Center.Mainw.setPlayPauseButtonIcon(!PlayControl.isPause);
        }

        /// <summary>
        /// 设置歌曲播放事件
        /// </summary>
        /// <param name="d">整个歌曲播放时间千分比，不是秒数！！</param>
        internal static void SetTime(double d)
        {
            if (waveout != null && FileReader != null)
            { FileReader.CurrentTime = TimeSpan.FromSeconds(FileReader.TotalTime.TotalSeconds * d / 1000.0); }
        }

        /// <summary>
        /// 从文件和播放列表里移除歌曲
        /// </summary>
        /// <param name="song"></param>
        private static void RemoveSong(SongItem song)
        {
            try
            {
                File.Delete(song._FilePath);
            }
            catch (System.Exception ex)
            {
                Center.Logg("播放模块 删除歌曲缓存失败：" + ex.Message);
            }

            Center.RemoveSong(song);
        }

        #endregion

        private static void timer_tick(object sender, ElapsedEventArgs e)
        {
            TimeSpan playtime = FileReader.CurrentTime;
            Center.Mainw.setDisplayTime(playtime, FileReader.TotalTime);
            Config.setPlayTime(playtime, FileReader.TotalTime);

            double nt = playtime.TotalSeconds;

            if (Config.MaxPlaySecond > 0 && Config.MaxPlaySecond < nt)
            { Next(); }

            if (lrc == null)
                return;

            string nowlrc = "";
            string nextlrc = "";

            foreach (KeyValuePair<double, string> k in lrc.LrcWord)
            {
                if (nt <= k.Key)
                {
                    nextlrc = k.Value;
                    OutputControl.WriteLyric(nowlrc, nextlrc);
                    return;
                }
                else
                {
                    nowlrc = k.Value;
                }

                // if(i < lastLrcNum) // 2016-8-9 增加修改播放时间功能，此处去掉
                //     continue; // 如果查找的歌词在显示的前面就跳过
                // if(lrcdone) // 已经查找完毕
                // {
                //     nextlrc = k.Value;
                //     OutputControl.WriteLyric(nowlrc, nextlrc);
                //     return;
                // }
                // if(nt >= k.Key) // 现在播放时间比歌词要显示的时间晚
                // {
                //     nowlrc = k.Value;
                //     //lastLrcNum = i + 1;
                //     lrcdone = true;
                // }
            }
        }

        private static void Waveout_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            playNotDone = false;
            OutputControl.WriteLyric();
            Center.Mainw.setDisplayTime(new TimeSpan(0), new TimeSpan(0));

            if (Config.ControlOtherPlayer && (Center.Songs.Count <= 1
                || Center.Songs[1].Status != SongItem.SongStatus.WaitingPlay))
            { API.SendKey(System.Windows.Input.Key.MediaNextTrack); }
        }
    }
}
