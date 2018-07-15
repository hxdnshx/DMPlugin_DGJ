namespace DMPlugin_DGJ
{
    internal static class DefaultConfig
    {
        internal const int songVol              = 100 ;
        internal const int maxSongCount         = 5 ;
        internal const int outputUpdateTime     = 250 ;
        internal const int MaxPlaySecond        = 0 ;
        internal const int VoteNext             = 0 ;
        internal const int LogStayTime          = 5;
        internal const int OutputType           = 0 ;

        internal const bool needLyric           = true ;
        internal const bool CanMultiSong        = false ;
        internal const bool AdminOnly           = false ;
        internal const bool ControlOtherPlayer  = false ;
        internal const bool DanmakuControl      = true ;
        internal const bool OneLyric            = false ;

        internal const int OPTLineNum               = 5  ;
        internal const string OPTBefore             = "点歌命令：“点歌 歌名”\r\n[播放进度]/[歌曲长度]";
        internal const string OPTLine               = "[状态] - [歌名] - [歌手] - [点歌人]";
        internal const string OPTAfter              = "点歌列表中当前有[歌曲数量]首歌曲";
        internal const string OutputEmptyList       = "点歌列表中没有歌曲";
        internal const string OutputOtherNamePrefix = "当前播放歌曲：";
    }
}
