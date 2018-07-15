using System;
using System.IO;

namespace DMPlugin_DGJ
{
    /// <summary>
    /// 插件本体
    /// </summary>
    public class PluginMain : BilibiliDM_PluginFramework.DMPlugin
    {
        /// <summary>
        /// 主插件本体，方便插件其他部分代码调用
        /// </summary>
        internal static PluginMain self;

        /// <summary>
        /// 插件本体
        /// </summary>
        public PluginMain()
        {
            self = this;

            Center.Init();
            Center.syncInit();
        }

        /// <summary>
        /// 管理插件
        /// </summary>
        public override void Admin()
        {
            MainWindow w = Center.Mainw;
            w.Show();
            w.Topmost = true;
            w.Topmost = false;
        }

        /// <summary>
        /// 反向初始化插件
        /// </summary>
        public override void DeInit()
        {
            PlayControl.Next();
            Config.Save();
            OutputControl.DeInit();
            try
            { Directory.Delete(Config.SongsCachePath, true); }
            catch (Exception)
            { }
        }

        public void MusicPlay()
        {
            PlayControl.Play();
        }

        public void MusicPause()
        {
            PlayControl.Pause();
        }

        public void MusicNext()
        {
            PlayControl.Next();
        }

        public void MusicVolume(int volume)
        {
            PlayControl.SetVol(volume);
        }
    }
}