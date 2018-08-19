namespace DMPlugin_DGJ.LWLAPI
{
    internal sealed class LwlApiBaidu : LwlApiBaseModule
    {
        private const string NAME = "百度音乐";
        private const string DESCRIPTION = "搜索百度音乐的歌曲";

        internal LwlApiBaidu()
        {
            SetServiceName("baidu");
            SetInfo(INFO_PREFIX + NAME, INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, DESCRIPTION);
        }
    }
}
