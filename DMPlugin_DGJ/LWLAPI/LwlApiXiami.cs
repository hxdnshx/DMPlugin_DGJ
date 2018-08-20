namespace DMPlugin_DGJ.LWLAPI
{
    internal sealed class LwlApiXiami : LwlApiBaseModule
    {
        private const string NAME = "虾米音乐";
        private const string DESCRIPTION = "搜索虾米音乐的歌曲";

        internal LwlApiXiami()
        {
            SetServiceName("xiami");
            SetInfo(INFO_PREFIX + NAME, INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, DESCRIPTION);
        }
    }
}
