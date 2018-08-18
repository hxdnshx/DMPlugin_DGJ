namespace DMPlugin_DGJ.LWLAPI
{
    internal sealed class LwlApiNetease : LwlApiBaseModule
    {
        private const string name = "网易云音乐";
        private const string desc = "搜索网易云音乐的歌曲";

        internal LwlApiNetease()
        {
            SetServiceName("netease");
            setInfo(INFO_PREFIX + name, INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, desc, INFO_LYRIC);
        }
    }
}
