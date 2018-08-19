using DMPlugin_DGJ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongSearchModuleExample
{
    public class ExampleModuleMain : SongsSearchModule
    {
        public ExampleModuleMain()
        {
            SetInfo("搜索模块名字", "搜索模块作者", "联系方式（报错时显示）", "搜索模块版本号", "搜索模块一句话介绍");
        }

        protected override SongItem Search(string who, string what, bool needLyric = false)
        {
            return null;
        }

        protected override int Download(SongItem item)
        {
            return base.Download(item);
        }
    }
}
