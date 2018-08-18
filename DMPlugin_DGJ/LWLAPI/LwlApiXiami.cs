namespace DMPlugin_DGJ.LWLAPI
{
    internal sealed class LwlApiXiami : LwlApiBaseModule
    {
        private const string name = "虾米音乐";
        private const string desc = "搜索虾米音乐的歌曲";

        internal LwlApiXiami()
        {
            SetServiceName("xiami");
            setInfo(INFO_PREFIX + name, INFO_AUTHOR, INFO_EMAIL, INFO_VERSION, desc, INFO_LYRIC);
        }
    }
}
