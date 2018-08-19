namespace DMPlugin_DGJ.LWLAPI
{
    internal sealed class LwlApiTencent : LwlApiBaseModule
    {
        private const string NAME = "QQ音乐";
        private const string DESCRIPTION = "搜索QQ音乐的歌曲";

        internal LwlApiTencent()
        {
            SetServiceName("tencent");
            SetInfo(INFO_PREFIX + NAME, INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, DESCRIPTION);
        }
    }
}
