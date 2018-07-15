using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DMPlugin_DGJ
{
    internal class IniFile
    {
        string Path;
        string defaultSec = "config";

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        internal IniFile(string IniPath)
        {
            Path = new FileInfo(IniPath).FullName.ToString();
            return;
        }

        internal string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? defaultSec, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        internal void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? defaultSec, Key, Value, Path);
        }

        internal void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? defaultSec);
        }

        internal void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? defaultSec);
        }

        internal bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
