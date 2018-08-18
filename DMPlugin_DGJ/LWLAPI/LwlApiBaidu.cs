namespace DMPlugin_DGJ.LWLAPI
{
    internal sealed class LwlApiBaidu : LwlApiBaseModule
    {
        private const string name = "百度音乐";
        private const string desc = "搜索百度音乐的歌曲";

        internal LwlApiBaidu()
        {
            SetServiceName("baidu");
            setInfo(INFO_PREFIX + name, INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, desc, INFO_LYRIC);
        }
    }
}
