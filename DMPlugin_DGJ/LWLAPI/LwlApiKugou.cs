namespace DMPlugin_DGJ.LWLAPI
{
    internal sealed class LwlApiKugou : LwlApiBaseModule
    {
        private const string name = "酷狗音乐";
        private const string desc = "搜索酷狗音乐的歌曲";

        internal LwlApiKugou()
        {
            SetServiceName("kugou");
            setInfo(INFO_PREFIX + name, INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, desc, INFO_LYRIC);
        }
    }
}
