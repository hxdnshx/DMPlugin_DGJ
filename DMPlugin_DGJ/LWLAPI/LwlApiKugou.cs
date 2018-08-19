namespace DMPlugin_DGJ.LWLAPI
{
    internal sealed class LwlApiKugou : LwlApiBaseModule
    {
        private const string NAME = "酷狗音乐";
        private const string DESCRIPTION = "搜索酷狗音乐的歌曲";

        internal LwlApiKugou()
        {
            SetServiceName("kugou");
            SetInfo(INFO_PREFIX + NAME, INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, DESCRIPTION);
        }
    }
}
