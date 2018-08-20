namespace DMPlugin_DGJ.LWLAPI
{
    internal sealed class LwlApiNetease : LwlApiBaseModule
    {
        private const string NAME = "网易云音乐";
        private const string DESCRIPTION = "搜索网易云音乐的歌曲";

        internal LwlApiNetease()
        {
            SetServiceName("netease");
            SetInfo(INFO_PREFIX + NAME, INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, DESCRIPTION);
        }
    }
}
