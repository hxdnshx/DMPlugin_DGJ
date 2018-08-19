﻿using System;
using System.IO;
using System.Threading;

namespace DMPlugin_DGJ
{
    public abstract class SongsSearchModule
    {
        private bool canChangeName = true;
        private PluginMain plugin = null;

        #region 模块信息

        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; private set; } = "歌曲平台名称";

        /// <summary>
        /// 模块作者
        /// </summary>
        public string ModuleAuthor { get; private set; } = "作者名字";

        /// <summary>
        /// 模块作者联系方式
        /// </summary>
        public string ModuleContact { get; private set; } = "联系方式";

        /// <summary>
        /// 模块版本号
        /// </summary>
        public string ModuleVersion { get; private set; } = "未填写";

        /// <summary>
        /// 搜索模块说明
        /// </summary>
        public string ModuleDescription { get; private set; } = "没有填写说明";


        /// <summary>
        /// 是否负责下载
        /// </summary>
        public bool IsHandleDownlaod
        { get; protected set; }

        /// <summary>
        /// 需要显示设置选项
        /// </summary>
        public bool IsNeedSettings
        { get; protected set; }


        /// <summary>
        /// 设置搜索模块信息
        /// 注意：此处只能设置一次
        /// 
        /// 模块名称最好不与其他模块重复
        /// 并且表达出是在哪个平台搜索的
        /// 作者联系方式用于生成报错文件
        /// </summary>
        /// <param name="name">模块名称</param>
        /// <param name="author">模块作者</param>
        /// <param name="contact">作者联系方式</param>
        /// <param name="version">版本号</param>
        /// <param name="description">模块说明</param>
        /// <param name="lyc">是否支持歌词</param>
        protected void SetInfo(string name, string author, string contact, string version, string description)
        {
            if (canChangeName)
            {
                canChangeName = false;
                ModuleName = name;
                ModuleAuthor = author;
                ModuleContact = contact;
                ModuleVersion = version;
                ModuleDescription = description;
            }
        }

        #endregion

        /// <summary>
        /// 搜索歌曲
        /// </summary>
        /// <param name="who">搜索人昵称</param>
        /// <param name="what">要搜索的字符串</param>
        /// <param name="needLyric">是否需要歌词</param>
        /// <returns>打包好的搜索结果</returns>
        protected abstract SongInfo Search(string keyword);

        /// <summary>
        /// 主插件调用用
        /// </summary>
        /// <param name="username">搜索人昵称</param>
        /// <param name="keyword">要搜索的字符串</param>
        /// <param name="needLyric">是否需要歌词</param>
        /// <returns>搜索结果</returns>
        internal SongInfo SafeSearch(string keyword)
        {
            try
            {
                return Search(keyword);
            }
            catch (Exception ex)
            {
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬点歌姬歌曲搜索引擎" + ModuleName + "错误报告.txt"))
                    {
                        outfile.WriteLine("请将错误报告发给 " + ModuleAuthor + " 谢谢，联系方式：" + ModuleContact);
                        outfile.WriteLine("参数：" + keyword);
                        outfile.WriteLine(ModuleName + " 本地时间：" + DateTime.Now.ToString());
                        outfile.Write(ex.ToString());
                        new Thread(() =>
                        {
                            System.Windows.MessageBox.Show("点歌姬歌曲搜索引擎“" + ModuleName + @"”遇到了未处理的错误
日志已经保存在桌面,请发给引擎作者 " + ModuleAuthor + ", 联系方式：" + ModuleContact);
                        }).Start();
                    }
                }
                catch (Exception)
                { }
                return null;
            }
        }

        protected abstract string GetDownloadUrl(SongInfo songInfo);

        internal string SafeGetDownloadUrl(SongInfo songInfo)
        {
            try
            {
                return GetDownloadUrl(songInfo);
            }
            catch (Exception ex)
            {
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬点歌姬歌曲搜索引擎" + ModuleName + "错误报告.txt"))
                    {
                        outfile.WriteLine("请将错误报告发给 " + ModuleAuthor + " 谢谢，联系方式：" + ModuleContact);
                        outfile.WriteLine("参数：" + songInfo.Id);
                        outfile.WriteLine(ModuleName + " 本地时间：" + DateTime.Now.ToString());
                        outfile.Write(ex.ToString());
                        new Thread(() =>
                        {
                            System.Windows.MessageBox.Show("点歌姬歌曲搜索引擎“" + ModuleName + @"”遇到了未处理的错误
日志已经保存在桌面,请发给引擎作者 " + ModuleAuthor + ", 联系方式：" + ModuleContact);
                        }).Start();
                    }
                }
                catch (Exception)
                { }
                return null;
            }
        }

        /// <summary>
        /// 请在不能使用普通方式下载歌曲文件的情况下重写替代下载
        /// </summary>
        /// <param name="item">要下载的歌曲信息</param>
        /// <returns>下载是否成功</returns>
        protected abstract DownloadStatus Download(SongItem item);

        /// <summary>
        /// 主插件调用用
        /// </summary>
        /// <param name="item">要下载的歌曲信息</param>
        /// <returns>下载是否成功</returns>
        internal DownloadStatus SafeDownload(SongItem item)
        {
            try
            {
                return Download(item);
            }
            catch (Exception ex)
            {
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬点歌姬歌曲搜索引擎" + ModuleName + "错误报告.txt"))
                    {
                        outfile.WriteLine("请将错误报告发给 " + ModuleAuthor + " 谢谢，联系方式：" + ModuleContact);
                        outfile.WriteLine("参数：filepath=" + item.FilePath + " url=" + item.DownloadURL + " id=" + item.SongID);
                        outfile.WriteLine(ModuleName + " 本地时间：" + DateTime.Now.ToString());
                        outfile.Write(ex.ToString());
                        new Thread(() =>
                        {
                            System.Windows.MessageBox.Show("点歌姬歌曲搜索引擎“" + ModuleName + @"”遇到了未处理的错误
日志已经保存在桌面,请发给引擎作者 " + ModuleAuthor + ", 联系方式：" + ModuleContact);
                        }).Start();
                    }
                }
                catch (Exception)
                { }
                return DownloadStatus.Failed;
            }
        }


        /// <summary>
        /// 设置搜索模块时会调用
        /// 会在新线程调用
        /// </summary>
        protected virtual void Setting()
        {

        }

        /// <summary>
        /// 主插件调用用
        /// </summary>
        internal void SafeSetting()
        {
            new Thread(() =>
            {
                try
                { Setting(); }
                catch (Exception ex)
                {
                    try
                    {
                        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬点歌姬歌曲搜索引擎" + ModuleName + "错误报告.txt"))
                        {
                            outfile.WriteLine("请将错误报告发给 " + ModuleAuthor + " 谢谢，联系方式：" + ModuleContact);
                            outfile.WriteLine(ModuleName + " 本地时间：" + DateTime.Now.ToString());
                            outfile.Write(ex.ToString());
                            new Thread(() =>
                            {
                                System.Windows.MessageBox.Show("点歌姬歌曲搜索引擎“" + ModuleName + @"”遇到了未处理的错误
日志已经保存在桌面,请发给引擎作者 " + ModuleAuthor + ", 联系方式：" + ModuleContact);
                            }).Start();
                        }
                    }
                    catch (Exception)
                    { }
                }
            })
            { Name = "ModuleSafeSetting", IsBackground = true }.Start();
        }


        /// <summary>
        /// 设置插件本体
        /// </summary>
        /// <param name="pl">插件本体</param>
        /// <returns>模块本体</returns>
        internal SongsSearchModule SetMainPlugin(PluginMain pl)
        {
            plugin = pl;
            return this;
        }

        /// <summary>
        /// 点歌姬主插件版本号
        /// </summary>
        public string MainPluginVersion
        { get { return plugin.PluginVer; } }

        /// <summary>
        /// 输出文本到弹幕姬日志
        /// </summary>
        /// <param name="text">要显示的文本</param>
        /// <param name="danmu">是否显示到侧边栏</param>
        protected void Log(string text, bool danmu = false)
        {
            if (plugin != null)
            {
                text = "搜索模块 " + ModuleName + "：" + text;
                plugin.Log(text);
                if (danmu)
                {
                    plugin.AddDM(text);
                }
            }
        }

        public enum DownloadStatus
        {
            /// <summary>
            /// 下载成功
            /// </summary>
            Success = 0,
            /// <summary>
            /// 下载失败
            /// </summary>
            Failed = -1
        }
    }
}
