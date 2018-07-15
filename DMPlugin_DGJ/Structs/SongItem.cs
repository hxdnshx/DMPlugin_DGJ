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
        /// 歌名
        /// </summary>
        public string SongName
        { get { return _SongName; } }

        internal string _SongName
        { get; set; }


        /// <summary>
        /// 歌曲ID
        /// </summary>
        public string SongID
        { get { return _SongID; } }

        internal string _SongID
        { get; set; }

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
        { get { return _Singers; } }

        internal string[] _Singers
        { get; set; }

        /// <summary>
        /// 点歌人
        /// </summary>
        public string User
        { get { return _User; } }

        internal string _User
        { get; set; }

        /// <summary>
        /// 下载地址
        /// </summary>
        public string DownloadURL
        { get { return _DownloadURL; } }

        internal string _DownloadURL
        { get; set; }

        /// <summary>
        /// 歌曲文件储存路径
        /// </summary>
        public string FilePath
        { get { return _FilePath; } }

        internal string _FilePath
        { get; set; }

        /// <summary>
        /// 文本歌词
        /// </summary>
        public string Lyric
        { get { return _Lyric; } }

        internal string _Lyric
        { get; set; }

        /// <summary>
        /// 歌曲备注
        /// </summary>
        public string Note
        { get { return _Note; } }

        internal string _Note
        { get; set; }

        /// <summary>
        /// 歌曲状态
        /// </summary>
        public SongStatus Status
        { get { return _Status; } }

        internal SongStatus _Status
        { get; private set; }

        internal void setStatus(SongStatus status)
        {
            _Status = status;
            RaisePropertyChanged("Status");
        }
        /// <summary>
        /// 格式化后的歌词
        /// </summary>
        internal Lrc _FLyric = null;

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
        private SongItem(SongsSearchModule _module, string _Name, string _ID, string _WhoWantThis, string[] _Singers, string _DownloadURL, string _lyric = "", string _note = "")
        {
            _Status = SongStatus.WaitingDownload;
            Module = _module;
            _SongName = _Name;
            _SongID = _ID;
            this._Singers = _Singers;
            _User = _WhoWantThis;
            this._DownloadURL = _DownloadURL;
            _Lyric = _lyric;
            _Note = _note;

            RaisePropertyChanged("");
        }

        /// <summary>
        /// 创建一个歌曲信息
        /// </summary>
        /// <param name="module">创建歌曲信息的搜素模块</param>
        /// <param name="Name">歌曲名称</param>
        /// <param name="ID">歌曲ID</param>
        /// <param name="Who">点歌人昵称</param>
        /// <param name="Singers">歌手</param>
        /// <param name="DownloadURL">MP3下载地址</param>
        /// <param name="lyric">歌词文本</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        public static SongItem init(SongsSearchModule module, string Name, string ID, string Who, string[] Singers, string DownloadURL, string lyric = "", string note = "")
        {
            return new SongItem(module, Name, ID, Who, Singers, DownloadURL, lyric, note);
        }

        /// <summary>
        /// 从文本歌词解析歌词
        /// </summary>
        /// <param name="lyric"></param>
        /// <returns>结果</returns>
        internal Lrc getFLyric(string lyric = "")
        {
            if (lyric == "")
                lyric = _Lyric;
            if (lyric == "")
            {
                _FLyric = null;
            }
            else
            {
                Lrc l = Lrc.InitLrc(lyric);
                _FLyric = l;
            }
            return _FLyric;
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
