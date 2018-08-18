namespace DMPlugin_DGJ.LWLAPI
{
    internal sealed class LwlApiTencent : LwlApiBaseModule
    {
        private const string name = "QQ音乐";
        private const string desc = "搜索QQ音乐的歌曲";

        internal LwlApiTencent()
        {
            SetServiceName("tencent");
            setInfo(INFO_PREFIX + name, INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, desc, INFO_LYRIC);
        }
    }
}
