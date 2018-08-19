using System.ComponentModel;

namespace DMPlugin_DGJ
{
    /// <summary>
    /// 歌曲信息
    /// </summary>
    public class SongItem : INotifyPropertyChanged
    {

        /// <summary>
        /// 搜索模块名称
        /// </summary>
        public string ModuleName
        { get { return Module.ModuleName; } }

        /// <summary>
        /// 搜索模块
        /// </summary>
        internal SongsSearchModule Module
        { get; set; }

        /// <summary>
        /// 歌曲ID
        /// </summary>
        public string SongID
        { get; internal set; }

        /// <summary>
        /// 歌名
        /// </summary>
        public string SongName
        { get; internal set; }

        /// <summary>
        /// string的歌手列表
        /// </summary>
        public string SingersText
        {
            get
            {
                string output = "";
                foreach (string str in Singers)
                    output += str + ";";
                return output;
            }
        }

        /// <summary>
        /// 歌手列表
        /// </summary>
        public string[] Singers
        { get; internal set; }

        /// <summary>
        /// 点歌人
        /// </summary>
        public string UserName
        { get; internal set; }

        // /// <summary>
        /// 下载地址
        /// </summary>
        // public string DownloadURL
        // { get; internal set; }

        /// <summary>
        /// 歌曲文件储存路径
        /// </summary>
        public string FilePath
        { get; internal set; }

        /// <summary>
        /// 文本歌词
        /// </summary>
        public Lrc Lyric
        { get; internal set; }

        /// <summary>
        /// 歌曲备注
        /// </summary>
        public string Note
        { get; internal set; }

        /// <summary>
        /// 歌曲状态
        /// </summary>
        public SongStatus Status
        { get; private set; }

        internal void SetStatus(SongStatus status)
        {
            Status = status;
            RaisePropertyChanged("Status");
        }

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        /// <summary>
        /// 新建一个预定义下载地址的歌曲物品
        /// </summary>
        /// <param name="_module">搜索模块</param>
        /// <param name="_Name">歌名</param>
        /// <param name="_ID">歌曲ID</param>
        /// <param name="_WhoWantThis">点歌人</param>
        /// <param name="_Singers">歌手列表</param>
        /// <param name="_DownloadURL">下载地址</param>
        /// <param name="_lyric">文本格式歌词</param>
        /// <param name="_note">歌曲信息备注</param>
        internal SongItem(SongInfo songInfo, string userName)
        {
            Status = SongStatus.WaitingDownload;

            UserName = userName;

            Module = songInfo.Module;
            SongID = songInfo.Id;
            SongName = songInfo.Name;
            Singers = songInfo.Singers;
            Lyric = Lrc.InitLrc(songInfo.Lyric);
            Note = songInfo.Note;
            // DownloadURL = _DownloadURL;

            RaisePropertyChanged("");
        }

        /// <summary>
        /// 歌曲播放状态
        /// </summary>
        public enum SongStatus
        {
            /// <summary>
            /// 等待下载
            /// </summary>
            WaitingDownload,
            /// <summary>
            /// 正在下载
            /// </summary>
            Downloading,
            /// <summary>
            /// 等待播放
            /// </summary>
            WaitingPlay,
            /// <summary>
            /// 正在播放
            /// </summary>
            Playing
        }
    }
}
